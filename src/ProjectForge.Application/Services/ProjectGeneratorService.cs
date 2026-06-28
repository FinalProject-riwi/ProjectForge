using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;
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
public partial class ProjectGeneratorService : IProjectGeneratorService
{
    private readonly IProjectRepository _projects;
    private readonly ITemplateRepository _templates;
    private readonly IDesignPatternRepository _designPatterns;
    private readonly IShellExecutor _shell;
    private readonly IGitHubService _github;
    private readonly IAiSuggestionService _ai;
    private readonly IConfiguration _config;
    private readonly IGenerationHubNotifier _hub;
    private readonly IEncryptionService _encryptionService;

    public ProjectGeneratorService(
        IProjectRepository projects,
        ITemplateRepository templates,
        IDesignPatternRepository designPatterns,
        IShellExecutor shell,
        IGitHubService github,
        IAiSuggestionService ai,
        IConfiguration config,
        IGenerationHubNotifier hub,
        IEncryptionService encryptionService)
    {
        _projects  = projects;
        _templates = templates;
        _designPatterns = designPatterns;
        _shell     = shell;
        _github    = github;
        _ai        = ai;
        _config    = config;
        _hub       = hub;
        _encryptionService = encryptionService;
    }

    public async Task<GenerationResult> GenerateAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetFullAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        var cfg = project.WizardConfig;
        var workBase = _config["Generation:WorkspacePath"] ?? Path.Combine(Path.GetTempPath(), "projectforge");

        // Sanitizar nombre para filesystem/Git: solo letras, dígitos y guiones
        var slug = System.Text.RegularExpressions.Regex.Replace(
            project.Name.ToLowerInvariant().Replace(" ", "-"),
            @"[^a-z0-9\-]", "-");
        // Garantizar unicidad con el ID del proyecto → evita race conditions
        // cuando dos proyectos tienen el mismo nombre
        var projectPath = Path.Combine(workBase, $"{slug}-{project.Id}");

        try
        {
            await UpdateStatusAsync(project, ProjectStatus.Generating, ct);
            await _hub.SendStatusAsync(projectId, "Generating");

            // ── Pre-flight: verificar que las herramientas CLI necesarias estén instaladas ──
            var missingTools = await CheckRequiredToolsAsync(cfg, ct);
            if (missingTools.Count > 0)
            {
                var toolList = string.Join(", ", missingTools);
                await _hub.SendStatusAsync(projectId, $"MissingTools:{string.Join("|", missingTools)}");
                throw new InvalidOperationException(
                    $"Herramientas requeridas no encontradas en el servidor: {toolList}. " +
                    "Instálalas en el contenedor o usa Docker Compose para generarlas.");
            }

            // 1. Crear carpeta de trabajo
            Directory.CreateDirectory(projectPath);
            await EmitLogAsync(project, "Scaffold", $"📁 Directorio de trabajo: {projectPath}", ct: ct);

            // 2. Scaffolding según arquitectura
            await ScaffoldProjectAsync(project, cfg, projectPath, ct);
            await ScaffoldDotNetBaseFilesAsync(project, cfg, projectPath, ct);   // .env, appsettings DB config
            await ScaffoldPhpBaseFilesAsync(project, cfg, projectPath, ct);
            await ScaffoldJavaScriptBaseFilesAsync(project, cfg, projectPath, ct);
            await ScaffoldPythonFilesInternalAsync(project, cfg, projectPath, ct);
            await ScaffoldPythonBaseFilesAsync(project, cfg, projectPath, ct);   // requirements.txt, .env, database.py, models.py, Dockerfile
            await ScaffoldJavaFilesInternalAsync(project, cfg, projectPath, ct);
            await ScaffoldJavaBaseFilesAsync(project, cfg, projectPath, ct);     // DB config, pom.xml, Dockerfile

            // 3. Scaffold adicional según patrón de diseño
            await ScaffoldDesignPatternsAsync(project, cfg, projectPath, ct);

            // 4. Aplicar plantillas de BD e infraestructura
            await ApplyTemplatesAsync(project, cfg, projectPath, ct);

            // 5. Instalar dependencias / librerías seleccionadas
            await InstallLibrariesAsync(project, cfg, projectPath, ct);

            // 6. Generar README con IA
            await GenerateReadmeAsync(project, cfg, projectPath, ct);

            // 7. Crear repo GitHub y hacer push
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

    // ─── Pre-flight: verificar herramientas CLI necesarias ────────────────────

    /// <summary>
    /// Verifica que las herramientas CLI necesarias para el stack seleccionado
    /// estén disponibles en el servidor (contenedor Docker).
    /// Devuelve lista de herramientas faltantes con hint de instalación.
    /// Formato de cada ítem: "NombreTool||HintInstalacion"
    /// </summary>
    private async Task<List<string>> CheckRequiredToolsAsync(WizardConfig cfg, CancellationToken ct)
    {
        var missing = new List<string>();
        var checks  = GetRequiredToolChecks(cfg);

        foreach (var (toolName, checkCmd, installHint) in checks)
        {
            try
            {
                var result = await _shell.RunAsync(checkCmd, Path.GetTempPath(), ct);
                // Consideramos disponible si salió con código 0 o produjo stdout
                if (!result.Success && string.IsNullOrWhiteSpace(result.Stdout))
                    missing.Add($"{toolName}||{installHint}");
            }
            catch
            {
                missing.Add($"{toolName}||{installHint}");
            }
        }

        return missing;
    }

    private static List<(string ToolName, string CheckCmd, string InstallHint)> GetRequiredToolChecks(WizardConfig cfg)
    {
        var checks = new List<(string, string, string)>();

        switch (cfg.Architecture)
        {
            case ArchitectureType.DotNet:
                checks.Add(("dotnet CLI", "dotnet --version", "https://dotnet.microsoft.com/download"));
                break;

            case ArchitectureType.Java:
                checks.Add(("Java JDK 17+", "java -version", "https://adoptium.net/"));
                checks.Add(("Maven", "mvn -version", "https://maven.apache.org/install.html"));
                break;

            case ArchitectureType.Python:
                checks.Add(("Python 3", "python3 --version", "https://www.python.org/downloads/"));
                checks.Add(("pip3", "pip3 --version", "Se instala con Python 3.4+"));
                if (cfg.Framework == FrameworkType.Django)
                    checks.Add(("Django", "python3 -c \"import django; print(django.__version__)\"",
                        "pip3 install django"));
                if (cfg.Framework == FrameworkType.FastAPI)
                    checks.Add(("FastAPI", "python3 -c \"import fastapi; print(fastapi.__version__)\"",
                        "pip3 install fastapi uvicorn"));
                if (cfg.Framework == FrameworkType.Flask)
                    checks.Add(("Flask", "python3 -c \"import flask; print(flask.__version__)\"",
                        "pip3 install flask"));
                break;

            case ArchitectureType.Php:
                checks.Add(("PHP CLI", "php --version", "https://www.php.net/manual/en/install.php"));
                checks.Add(("Composer", "composer --version", "https://getcomposer.org/download/"));
                break;

            case ArchitectureType.JavaScript:
            case ArchitectureType.TypeScript:
                checks.Add(("Node.js", "node --version", "https://nodejs.org/en/download/"));
                checks.Add(("npm", "npm --version", "Se instala automáticamente con Node.js"));
                if (cfg.Framework is FrameworkType.NestJs or FrameworkType.NestTs)
                    checks.Add(("NestJS CLI", "nest --version",
                        "npm install -g @nestjs/cli"));
                if (cfg.Framework is FrameworkType.NextJs or FrameworkType.NextTs)
                    checks.Add(("npx", "npx --version", "Se instala con npm 5.2+"));
                break;
        }

        // Git es requerido para todos
        checks.Add(("Git", "git --version", "https://git-scm.com/downloads"));

        return checks;
    }

}
