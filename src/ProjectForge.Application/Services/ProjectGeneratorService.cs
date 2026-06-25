using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

// ─── Contrato mínimo de hub que necesita la capa Application ─────────────────
// Evita dependencia directa de ProjectForge.Web desde Application.
public interface IGenerationHubNotifier
{
    Task SendLogAsync(int projectId, string step, string message, bool isError = false);
    Task SendStatusAsync(int projectId, string status);
}

/// <summary>
/// Orquesta la generación del proyecto: ejecuta comandos CLI locales,
/// aplica plantillas y sube el resultado a GitHub.
/// Emite logs en tiempo real al navegador via SignalR.
/// </summary>
public class ProjectGeneratorService : IProjectGeneratorService
{
    private readonly IProjectRepository _projects;
    private readonly ITemplateRepository _templates;
    private readonly IShellExecutor _shell;
    private readonly IGitHubService _github;
    private readonly IAiSuggestionService _ai;
    private readonly IConfiguration _config;
    private readonly IGenerationHubNotifier _hub;

    public ProjectGeneratorService(
        IProjectRepository projects,
        ITemplateRepository templates,
        IShellExecutor shell,
        IGitHubService github,
        IAiSuggestionService ai,
        IConfiguration config,
        IGenerationHubNotifier hub)
    {
        _projects  = projects;
        _templates = templates;
        _shell     = shell;
        _github    = github;
        _ai        = ai;
        _config    = config;
        _hub       = hub;
    }

    public async Task<GenerationResult> GenerateAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetFullAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        var cfg = project.WizardConfig;
        var workBase    = _config["Generation:WorkspacePath"] ?? Path.Combine(Path.GetTempPath(), "projectforge");
        var projectPath = Path.Combine(workBase, project.Name.ToLowerInvariant().Replace(" ", "-"));

        try
        {
            await UpdateStatusAsync(project, ProjectStatus.Generating, ct);
            await _hub.SendStatusAsync(projectId, "Generating");

            // 1. Crear carpeta de trabajo
            Directory.CreateDirectory(projectPath);
            await EmitLogAsync(project, "Scaffold", $"📁 Directorio de trabajo: {projectPath}", ct: ct);

            // 2. Scaffolding según arquitectura
            await ScaffoldProjectAsync(project, cfg, projectPath, ct);

            // 3. Aplicar plantillas de BD e infraestructura
            await ApplyTemplatesAsync(project, cfg, projectPath, ct);

            // 4. Instalar dependencias / librerías seleccionadas
            await InstallLibrariesAsync(project, cfg, projectPath, ct);

            // 5. Generar README con IA
            await GenerateReadmeAsync(project, cfg, projectPath, ct);

            // 6. Crear repo GitHub y hacer push
            await EmitLogAsync(project, "GitHub", "🔗 Creando repositorio en GitHub...", ct: ct);
            var repoUrl = await PushToGitHubAsync(project, projectPath, ct);

            project.LocalPath      = projectPath;
            project.RepositoryUrl  = repoUrl;
            await UpdateStatusAsync(project, ProjectStatus.Published, ct);
            await _hub.SendStatusAsync(projectId, "Published");

            return new GenerationResult(true, null, projectPath, repoUrl);
        }
        catch (Exception ex)
        {
            await EmitLogAsync(project, "Error", $"❌ {ex.Message}", isError: true, ct: ct);
            project.ErrorMessage = ex.Message;
            await UpdateStatusAsync(project, ProjectStatus.Failed, ct);
            await _hub.SendStatusAsync(projectId, "Failed");
            return new GenerationResult(false, ex.Message, projectPath, null);
        }
    }

    // ─── Paso 2: Scaffold por arquitectura ────────────────────────────────────

    private async Task ScaffoldProjectAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Scaffold", $"🏗️  Iniciando scaffold ({cfg.Architecture} / {cfg.Framework})...", ct: ct);
        var commands = GetScaffoldCommands(cfg, project.Name, path);

        foreach (var cmd in commands)
        {
            await EmitLogAsync(project, "Scaffold", $"$ {cmd.Command}", ct: ct);
            var result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);

            if (!string.IsNullOrWhiteSpace(result.Stdout))
                await EmitLogAsync(project, "Scaffold", result.Stdout.Trim(), ct: ct);

            if (!result.Success)
            {
                await EmitLogAsync(project, "Scaffold", result.Stderr, isError: true, ct: ct);
                throw new InvalidOperationException($"Scaffold falló: {result.Stderr}");
            }

            await EmitLogAsync(project, "Scaffold", "✅ Comando completado", ct: ct);
        }
    }

    private static IEnumerable<(string Command, string? WorkingDir)> GetScaffoldCommands(
        WizardConfig cfg, string projectName, string path)
    {
        var safeName = projectName.Replace(" ", "");

        return cfg.Architecture switch
        {
            ArchitectureType.DotNet => cfg.Framework switch
            {
                FrameworkType.AspNetCoreWebApi => new[]
                {
                    ($"dotnet new webapi -n {safeName} -o {path} --no-https false", (string?)null),
                    ($"dotnet new sln -n {safeName}", path),
                    ($"dotnet sln add {safeName}.csproj", path),
                },
                FrameworkType.AspNetCoreMVC => new[]
                {
                    ($"dotnet new mvc -n {safeName} -o {path}", (string?)null),
                },
                FrameworkType.BlazorServer => new[]
                {
                    ($"dotnet new blazorserver -n {safeName} -o {path}", (string?)null),
                },
                _ => new[] { ($"dotnet new webapi -n {safeName} -o {path}", (string?)null) }
            },

            ArchitectureType.Python => cfg.Framework switch
            {
                FrameworkType.FastAPI => new[]
                {
                    ($"mkdir -p {path}/app/api/v1 {path}/app/models {path}/app/services {path}/tests", (string?)null),
                    ($"python3 -m venv {path}/venv", null),
                },
                FrameworkType.Django => new[]
                {
                    ($"django-admin startproject {safeName} {path}", (string?)null),
                },
                _ => new[] { ($"mkdir -p {path}/src {path}/tests", (string?)null) }
            },

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => new[]
            {
                ($"npm init -y", path),
                cfg.Framework == FrameworkType.NestJs
                    ? ($"npm i -g @nestjs/cli && nest new {safeName} --directory . --skip-git", path)
                    : ($"npm install express", path),
            },

            ArchitectureType.Java => new[]
            {
                ($"curl -s https://start.spring.io/starter.zip " +
                 $"-d type=maven-project -d language=java -d bootVersion={cfg.FrameworkVersion} " +
                 $"-d artifactId={safeName.ToLower()} -d packaging=jar " +
                 $"-d dependencies=web,actuator -o {path}/project.zip && " +
                 $"cd {path} && unzip -q project.zip && rm project.zip", (string?)null),
            },

            ArchitectureType.Php => cfg.Framework switch
            {
                FrameworkType.Symfony => new[]
                {
                    ($"composer create-project symfony/skeleton {path}", (string?)null),
                },
                _ => new[]
                {
                    ($"composer create-project laravel/laravel {path}", (string?)null),
                }
            },

            _ => Array.Empty<(string, string?)>()
        };
    }

    // ─── Paso 3: Aplicar plantillas ───────────────────────────────────────────

    private async Task ApplyTemplatesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Templates", "📄 Aplicando plantillas de infraestructura...", ct: ct);

        var vars = new Dictionary<string, string>
        {
            ["APP_NAME"] = project.Name.ToLower().Replace(" ", "-"),
            ["DB_NAME"]  = $"{project.Name.ToLower().Replace(" ", "_")}_db",
            ["DB_PORT"]  = GetDefaultDbPort(cfg.Database).ToString(),
            ["APP_PORT"] = "8080"
        };

        if (cfg.Infrastructure == InfrastructureType.DockerCompose)
        {
            var tpl = await _templates.GetTemplateAsync(cfg.Architecture, "compose", cfg.Database, InfrastructureType.DockerCompose);
            if (tpl != null)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "docker-compose.yml"), InterpolateTemplate(tpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ docker-compose.yml generado", ct: ct);
            }

            var dockerfile = await _templates.GetTemplateAsync(cfg.Architecture, "dockerfile");
            if (dockerfile != null)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "Dockerfile"), InterpolateTemplate(dockerfile.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ Dockerfile generado", ct: ct);
            }
        }

        if (cfg.Infrastructure == InfrastructureType.Kubernetes)
        {
            var k8sDir = Path.Combine(path, "k8s");
            Directory.CreateDirectory(k8sDir);
            var templates = await _templates.GetInfraTemplatesAsync(InfrastructureType.Kubernetes, cfg.Database);
            foreach (var tpl in templates)
            {
                var filename = $"{tpl.Name.ToLower().Replace(" ", "-")}.yaml";
                await File.WriteAllTextAsync(Path.Combine(k8sDir, filename), InterpolateTemplate(tpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", $"✅ k8s/{filename} generado", ct: ct);
            }
        }

        var gitignore = await _templates.GetTemplateAsync(cfg.Architecture, "gitignore");
        if (gitignore != null)
        {
            await File.WriteAllTextAsync(Path.Combine(path, ".gitignore"), gitignore.Content, ct);
            await EmitLogAsync(project, "Templates", "✅ .gitignore generado", ct: ct);
        }

        var ciDir = Path.Combine(path, ".github", "workflows");
        Directory.CreateDirectory(ciDir);
        var ciTemplate = await _templates.GetTemplateAsync(cfg.Architecture, "ci");
        if (ciTemplate != null)
        {
            await File.WriteAllTextAsync(Path.Combine(ciDir, "ci.yml"), InterpolateTemplate(ciTemplate.Content, vars), ct);
            await EmitLogAsync(project, "Templates", "✅ .github/workflows/ci.yml generado", ct: ct);
        }
    }

    // ─── Paso 4: Instalar dependencias ────────────────────────────────────────

    private async Task InstallLibrariesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var libs = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();
        if (!libs.Any())
        {
            await EmitLogAsync(project, "Dependencies", "ℹ️  Sin librerías adicionales seleccionadas", ct: ct);
            return;
        }

        await EmitLogAsync(project, "Dependencies", $"📦 Instalando {libs.Count} librerías...", ct: ct);

        foreach (var cmd in GetInstallCommands(cfg.Architecture, cfg.Framework, libs))
        {
            await EmitLogAsync(project, "Dependencies", $"$ {cmd}", ct: ct);
            var result = await _shell.RunAsync(cmd, path, ct);
            if (result.Success)
                await EmitLogAsync(project, "Dependencies", "✅ Instalado", ct: ct);
            else
                await EmitLogAsync(project, "Dependencies", $"⚠️ Advertencia: {result.Stderr}", isError: true, ct: ct);
        }
    }

    private static IEnumerable<string> GetInstallCommands(ArchitectureType arch, FrameworkType fw, List<string> libs) =>
        arch switch
        {
            ArchitectureType.DotNet                                     => libs.Select(l => $"dotnet add package {l}"),
            ArchitectureType.Python                                     => new[] { $"pip install {string.Join(" ", libs)}" },
            ArchitectureType.JavaScript or ArchitectureType.TypeScript  => new[] { $"npm install {string.Join(" ", libs)}" },
            ArchitectureType.Php                                        => new[] { $"composer require {string.Join(" ", libs)}" },
            _                                                           => Enumerable.Empty<string>()
        };

    // ─── Paso 5: README con IA ────────────────────────────────────────────────

    private async Task GenerateReadmeAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "README", "🤖 Generando README.md con IA (cache-first)...", ct: ct);

        var patterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? new();
        var libs     = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();

        var req = new ReadmeGenerationRequest(
            project.Name, project.Description,
            cfg.Framework, cfg.Database, cfg.Infrastructure,
            patterns, libs, project.RepositoryUrl);

        var readme = await _ai.GenerateReadmeAsync(req);
        project.GeneratedReadme = readme;
        await File.WriteAllTextAsync(Path.Combine(path, "README.md"), readme, ct);
        await EmitLogAsync(project, "README", "✅ README.md generado", ct: ct);
    }

    // ─── Paso 6: GitHub ───────────────────────────────────────────────────────

    private async Task<string> PushToGitHubAsync(Project project, string path, CancellationToken ct)
    {
        var token   = project.User.AccessToken;
        var repoUrl = await _github.CreateRepositoryAsync(token, project.Name, project.Description, false);
        await EmitLogAsync(project, "GitHub", $"✅ Repositorio creado: {repoUrl}", ct: ct);

        await EmitLogAsync(project, "GitHub", "$ git init && git add . && git commit", ct: ct);
        await _shell.RunAsync("git init", path, ct);
        await _shell.RunAsync("git add .", path, ct);
        await _shell.RunAsync($"git commit -m \"chore: initial scaffold by ProjectForge\"", path, ct);
        await _shell.RunAsync($"git remote add origin {repoUrl}", path, ct);
        await _shell.RunAsync("git branch -M main", path, ct);

        await EmitLogAsync(project, "GitHub", "$ git push -u origin main", ct: ct);
        await _github.PushToRepositoryAsync(path, repoUrl, token);
        await EmitLogAsync(project, "GitHub", "🚀 Push completado. ¡Proyecto en GitHub!", ct: ct);

        return repoUrl;
    }

    // ─── Utilidades ───────────────────────────────────────────────────────────

    private static string InterpolateTemplate(string content, Dictionary<string, string> vars)
    {
        foreach (var (k, v) in vars)
            content = content.Replace($"{{{{{k}}}}}", v);
        return content;
    }

    private static int GetDefaultDbPort(DatabaseType db) => db switch
    {
        DatabaseType.MySQL      => 3306,
        DatabaseType.PostgreSQL => 5432,
        DatabaseType.SqlServer  => 1433,
        DatabaseType.MongoDB    => 27017,
        DatabaseType.Redis      => 6379,
        _                       => 5432
    };

    /// <summary>
    /// Actualiza el estado del proyecto en DB y emite el cambio por SignalR.
    /// </summary>
    private async Task UpdateStatusAsync(Project project, ProjectStatus status, CancellationToken ct = default)
    {
        project.Status    = status;
        project.UpdatedAt = DateTime.UtcNow;
        await _projects.UpdateAsync(project);
    }

    /// <summary>
    /// Emite el log al SignalR hub (visible en el browser) Y lo persiste en DB.
    /// Todos los parámetros opcionales van con nombre para evitar ambigüedad con CancellationToken.
    /// </summary>
    private async Task EmitLogAsync(
        Project project,
        string step,
        string message,
        bool isError       = false,
        string? command    = null,
        int? exitCode      = null,
        CancellationToken ct = default)
    {
        // 1. Enviar al navegador en tiempo real
        await _hub.SendLogAsync(project.Id, step, message, isError);

        // 2. Persistir en DB
        project.Logs.Add(new ProjectLog
        {
            Step            = step,
            Message         = message,
            IsError         = isError,
            CommandExecuted = command,
            ExitCode        = exitCode
        });
        await _projects.UpdateAsync(project);
    }
}
