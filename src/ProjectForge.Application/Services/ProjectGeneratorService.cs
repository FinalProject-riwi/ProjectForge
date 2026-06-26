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
    private readonly IDesignPatternRepository _designPatterns;
    private readonly IShellExecutor _shell;
    private readonly IGitHubService _github;
    private readonly IAiSuggestionService _ai;
    private readonly IConfiguration _config;
    private readonly IGenerationHubNotifier _hub;

    public ProjectGeneratorService(
        IProjectRepository projects,
        ITemplateRepository templates,
        IDesignPatternRepository designPatterns,
        IShellExecutor shell,
        IGitHubService github,
        IAiSuggestionService ai,
        IConfiguration config,
        IGenerationHubNotifier hub)
    {
        _projects  = projects;
        _templates = templates;
        _designPatterns = designPatterns;
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
            await ScaffoldJavaScriptBaseFilesAsync(project, cfg, projectPath, ct);

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

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => cfg.Framework switch
            {
                FrameworkType.NodeJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm install", (string?)path),
                    ($"npm pkg set scripts.start=node src/index.js", (string?)path),
                },
                FrameworkType.ExpressJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm install express", (string?)path),
                    ($"npm pkg set scripts.start=node src/server.js", (string?)path),
                },
                FrameworkType.NestJs => new[]
                {
                    ($"npx @nestjs/cli new {safeName} --language JS --package-manager npm --skip-git --directory .", (string?)path),
                },
                FrameworkType.NextJs => new[]
                {
                    ($"npm create next-app@latest . -- --js --app --eslint --src-dir --import-alias \"@/*\"", (string?)path),
                },
                FrameworkType.NestTs => new[]
                {
                    ($"npx @nestjs/cli new {safeName} --language TS --package-manager npm --skip-git --directory .", (string?)path),
                },
                FrameworkType.NextTs => new[]
                {
                    ($"npm create next-app@latest . -- --ts --app --eslint --src-dir --import-alias \"@/*\"", (string?)path),
                },
                _ => new[]
                {
                    ($"npm init -y", (string?)path),
                }
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

    private async Task ScaffoldDesignPatternsAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Count == 0)
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  Sin patrones seleccionados", ct: ct);
            return;
        }

        if (cfg.Architecture != ArchitectureType.Php &&
            cfg.Architecture != ArchitectureType.JavaScript &&
            cfg.Architecture != ArchitectureType.TypeScript)
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  El scaffold de patrones está habilitado solo para PHP y JavaScript por ahora", ct: ct);
            return;
        }

        var availablePatterns = (await _designPatterns.GetByArchitectureAsync(cfg.Architecture)).ToList();
        var appliedAny = false;

        foreach (var selected in selectedPatterns.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalized = NormalizePatternToken(selected);
            var patternEnum = ResolvePhpDesignPattern(normalized);
            if (!patternEnum.HasValue)
            {
                await EmitLogAsync(project, "Patterns", $"⚠️ Patrón no reconocido: {selected}", isError: true, ct: ct);
                continue;
            }

            var entry = ResolvePhpPatternEntry(availablePatterns, patternEnum, cfg.Framework);

            if (entry != null)
            {
                await EmitLogAsync(project, "Patterns", $"🧩 Aplicando patrón: {entry.Name}", ct: ct);
                foreach (var cmd in ReadScaffoldCommands(entry.ScaffoldCommandsJson))
                {
                    await EmitLogAsync(project, "Patterns", $"$ {cmd}", ct: ct);
                    var result = await _shell.RunAsync(cmd, path, ct);

                    if (!result.Success)
                    {
                        await EmitLogAsync(project, "Patterns", result.Stderr, isError: true, ct: ct);
                        throw new InvalidOperationException($"Scaffold de patrón falló: {result.Stderr}");
                    }
                }
            }
            var files = cfg.Architecture == ArchitectureType.Php
                ? BuildPhpPatternFiles(cfg.Framework, patternEnum.Value, project.Name)
                : BuildJavaScriptPatternFiles(cfg.Framework, patternEnum.Value, project.Name);
            foreach (var file in files)
            {
                var fullPath = Path.Combine(path, file.RelativePath);
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(fullPath))
                {
                    await File.WriteAllTextAsync(fullPath, file.Content, ct);
                    await EmitLogAsync(project, "Patterns", $"✅ {file.RelativePath} generado", ct: ct);
                }
            }

            if (cfg.Framework == FrameworkType.Laravel &&
                patternEnum == DesignPattern.Repository)
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\ProjectRepositoryServiceProvider::class", ct);
            }

            if (cfg.Framework == FrameworkType.Laravel &&
                patternEnum == DesignPattern.CleanArchitecture)
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\CleanArchitectureServiceProvider::class", ct);
            }

            if (cfg.Framework == FrameworkType.Laravel &&
                patternEnum == DesignPattern.HexagonalArchitecture)
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\HexagonalServiceProvider::class", ct);
            }

            if (cfg.Framework == FrameworkType.Symfony &&
                patternEnum == DesignPattern.Repository)
            {
                await EnsureSymfonyServiceBindingAsync(path, "App\\Contract\\ProjectRepositoryInterface", "App\\Repository\\ProjectRepository", ct);
            }

            if (cfg.Framework == FrameworkType.Symfony &&
                patternEnum == DesignPattern.CleanArchitecture)
            {
                await EnsureSymfonyServiceBindingAsync(path, "App\\Contract\\ProjectRepositoryInterface", "App\\Infrastructure\\Persistence\\DoctrineProjectRepository", ct);
            }

            if (cfg.Framework == FrameworkType.Symfony &&
                patternEnum == DesignPattern.HexagonalArchitecture)
            {
                await EnsureSymfonyServiceBindingAsync(path, "App\\Port\\ProjectRepositoryPort", "App\\Adapters\\Persistence\\DoctrineProjectRepository", ct);
            }

            if (cfg.Framework == FrameworkType.NestJs &&
                patternEnum == DesignPattern.CQRS)
            {
                await EnsureNestJsAppModuleRegistrationAsync(path, ct);
            }

            if (cfg.Framework == FrameworkType.NodeJs &&
                patternEnum == DesignPattern.CQRS)
            {
                await EnsureNodeJsCqrsAppAsync(path, ct);
            }

            if (cfg.Framework == FrameworkType.ExpressJs &&
                patternEnum == DesignPattern.CQRS)
            {
                await EnsureExpressJsCqrsAppAsync(path, ct);
            }

            appliedAny = true;
        }

        if (!appliedAny)
            await EmitLogAsync(project, "Patterns", "ℹ️  No se pudo resolver ningún scaffold de patrón", ct: ct);
    }

    private async Task ScaffoldJavaScriptBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.JavaScript && cfg.Architecture != ArchitectureType.TypeScript)
            return;

        if (cfg.Framework is not (FrameworkType.NodeJs or FrameworkType.ExpressJs))
            return;

        var files = BuildJavaScriptBaseFiles(cfg.Framework, project.Name);
        foreach (var file in files)
        {
            var fullPath = Path.Combine(path, file.RelativePath);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(fullPath))
            {
                await File.WriteAllTextAsync(fullPath, file.Content, ct);
                await EmitLogAsync(project, "Scaffold", $"✅ {file.RelativePath} generado", ct: ct);
            }
        }
    }

    private static IEnumerable<string> ReadScaffoldCommands(string? scaffoldCommandsJson)
    {
        if (string.IsNullOrWhiteSpace(scaffoldCommandsJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(scaffoldCommandsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static DesignPatternEntry? ResolvePhpPatternEntry(
        IReadOnlyCollection<DesignPatternEntry> availablePatterns,
        DesignPattern? pattern,
        FrameworkType framework)
    {
        if (!pattern.HasValue)
            return null;

        var candidates = availablePatterns.Where(p => p.Pattern == pattern.Value).ToList();
        if (candidates.Count == 0)
            return null;

        if (framework == FrameworkType.Symfony)
        {
            return candidates.FirstOrDefault(p => p.Name.Contains("Symfony", StringComparison.OrdinalIgnoreCase))
                ?? candidates.First();
        }

        if (framework == FrameworkType.NestJs)
        {
            return candidates.FirstOrDefault(p =>
                p.Name.Contains("NestJS", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("NestJs", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
                ?? candidates.First();
        }

        return candidates.FirstOrDefault(p =>
                !p.Name.Contains("Symfony", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Contains("NestJS", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Contains("NestJs", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
            ?? candidates.First();
    }

    private static DesignPattern? ResolvePhpDesignPattern(string normalizedLabel) => normalizedLabel switch
    {
        "repository" or "repositorypattern" => DesignPattern.Repository,
        "cleanarchitecture" or "cleanarch" => DesignPattern.CleanArchitecture,
        "hexagonalarchitecture" or "hexagonal" => DesignPattern.HexagonalArchitecture,
        "ddd" or "domaindrivendesign" or "domain-driven design" => DesignPattern.DomainDrivenDesign,
        "eventsourcing" => DesignPattern.EventSourcing,
        "microservices" => DesignPattern.Microservices,
        "cqrs" => DesignPattern.CQRS,
        "mediator" => DesignPattern.Mediator,
        "saga" => DesignPattern.Saga,
        _ => null
    };

    private static string NormalizePatternToken(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c))
            .ToArray();
        return new string(chars);
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpPatternFiles(
        FrameworkType framework,
        DesignPattern pattern,
        string projectName)
    {
        var appName = ToClassName(projectName);

        return framework switch
        {
            FrameworkType.Laravel => BuildLaravelPatternFiles(pattern, appName),
            FrameworkType.Symfony => BuildSymfonyPatternFiles(pattern, appName),
            _ => []
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptPatternFiles(
        FrameworkType framework,
        DesignPattern pattern,
        string projectName)
    {
        return pattern switch
        {
            DesignPattern.Repository => new[]
            {
                ("src/domain/project.js", """
class Project {
  constructor({ id = null, name = '', description = null } = {}) {
    this.id = id;
    this.name = name;
    this.description = description;
  }
}

module.exports = { Project };
"""),
                ("src/ports/project-repository.js", """
class ProjectRepositoryPort {
  async all() {
    throw new Error('Not implemented');
  }

  async find(id) {
    throw new Error('Not implemented');
  }

  async create(project) {
    throw new Error('Not implemented');
  }

  async update(id, project) {
    throw new Error('Not implemented');
  }
}

module.exports = { ProjectRepositoryPort };
"""),
                ("src/application/create-project-use-case.js", """
const { Project } = require('../domain/project');

class CreateProjectUseCase {
  constructor(projects) {
    this.projects = projects;
  }

  async execute(name, description = null) {
    return this.projects.create(new Project({ name, description }));
  }
}

module.exports = { CreateProjectUseCase };
"""),
                ("src/infrastructure/in-memory-project-repository.js", """
const { Project } = require('../domain/project');

class InMemoryProjectRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  async all() {
    return Array.from(this.items.values());
  }

  async find(id) {
    return this.items.get(id) ?? null;
  }

  async create(project) {
    const created = new Project({
      id: this.nextId++,
      name: project.name,
      description: project.description,
    });

    this.items.set(created.id, created);
    return created;
  }

  async update(id, project) {
    const current = this.items.get(id);
    if (!current) {
      return null;
    }

    current.name = project.name;
    current.description = project.description;
    return current;
  }
}

module.exports = { InMemoryProjectRepository };
"""),
            },
            DesignPattern.CleanArchitecture => new[]
            {
                ("src/domain/project.js", """
class Project {
  constructor({ id = null, name = '', description = null } = {}) {
    this.id = id;
    this.name = name;
    this.description = description;
  }
}

module.exports = { Project };
"""),
                ("src/application/use-cases/create-project-use-case.js", """
const { Project } = require('../../domain/project');

class CreateProjectUseCase {
  constructor(projects) {
    this.projects = projects;
  }

  async execute(name, description = null) {
    return this.projects.save(new Project({ name, description }));
  }
}

module.exports = { CreateProjectUseCase };
"""),
                ("src/infrastructure/persistence/in-memory-project-repository.js", """
const { Project } = require('../../../domain/project');

class InMemoryProjectRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  async save(project) {
    const item = new Project({
      id: this.nextId++,
      name: project.name,
      description: project.description,
    });

    this.items.set(item.id, item);
    return item;
  }

  async find(id) {
    return this.items.get(id) ?? null;
  }

  async all() {
    return Array.from(this.items.values());
  }
}

module.exports = { InMemoryProjectRepository };
"""),
                ("src/interfaces/http/project-controller.js", """
class ProjectController {
  constructor(createProjectUseCase) {
    this.createProjectUseCase = createProjectUseCase;
  }

  async create(req, res) {
    const project = await this.createProjectUseCase.execute(req.body.name, req.body.description ?? null);
    res.status(201).json(project);
  }
}

module.exports = { ProjectController };
"""),
            },
            DesignPattern.HexagonalArchitecture => new[]
            {
                ("src/domain/project.js", """
class Project {
  constructor({ id = null, name = '', description = null } = {}) {
    this.id = id;
    this.name = name;
    this.description = description;
  }
}

module.exports = { Project };
"""),
                ("src/ports/project-repository-port.js", """
class ProjectRepositoryPort {
  async save(project) {
    throw new Error('Not implemented');
  }

  async find(id) {
    throw new Error('Not implemented');
  }

  async all() {
    throw new Error('Not implemented');
  }
}

module.exports = { ProjectRepositoryPort };
"""),
                ("src/application/use-cases/create-project-use-case.js", """
const { Project } = require('../../domain/project');

class CreateProjectUseCase {
  constructor(projects) {
    this.projects = projects;
  }

  async execute(name, description = null) {
    return this.projects.save(new Project({ name, description }));
  }
}

module.exports = { CreateProjectUseCase };
"""),
                ("src/adapters/persistence/in-memory-project-repository.js", """
const { Project } = require('../../../domain/project');

class InMemoryProjectRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  async save(project) {
    const item = new Project({
      id: this.nextId++,
      name: project.name,
      description: project.description,
    });

    this.items.set(item.id, item);
    return item;
  }

  async find(id) {
    return this.items.get(id) ?? null;
  }

  async all() {
    return Array.from(this.items.values());
  }
}

module.exports = { InMemoryProjectRepository };
"""),
            },
            DesignPattern.CQRS => BuildJavaScriptCqrsPatternFiles(framework),
            DesignPattern.Mediator => new[]
            {
                ("src/application/mediator.js", """
class ProjectMediator {
  async dispatch(message) {
    return message;
  }
}

module.exports = { ProjectMediator };
"""),
                ("src/application/messages/create-project-message.js", """
class CreateProjectMessage {
  constructor(payload = {}) {
    this.payload = payload;
  }
}

module.exports = { CreateProjectMessage };
"""),
                ("src/application/handlers/create-project-handler.js", """
class CreateProjectHandler {
  async handle(message) {
    return message.payload;
  }
}

module.exports = { CreateProjectHandler };
"""),
            },
            DesignPattern.Microservices => new[]
            {
                ("src/services/project-service.js", """
class ProjectService {
  async request(payload) {
    return payload;
  }
}

module.exports = { ProjectService };
"""),
                ("src/events/project-created.js", """
class ProjectCreated {
  constructor(payload = {}) {
    this.payload = payload;
  }
}

module.exports = { ProjectCreated };
"""),
                ("src/workers/project-worker.js", """
class ProjectWorker {
  async handle() {
    // TODO: sincronizar microservicios
  }
}

module.exports = { ProjectWorker };
"""),
                ("src/integrations/github-client.js", """
class GitHubClient {
  async createRepository(payload) {
    return payload;
  }
}

module.exports = { GitHubClient };
"""),
            },
            _ => []
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptCqrsPatternFiles(FrameworkType framework) =>
        framework switch
        {
            FrameworkType.NestJs => BuildNestJsCqrsPatternFiles(),
            FrameworkType.NodeJs => BuildNodeJsCqrsPatternFiles(),
            FrameworkType.ExpressJs => BuildExpressJsCqrsPatternFiles(),
            FrameworkType.NextJs => BuildNextJsCqrsPatternFiles(),
            _ => BuildGenericJavaScriptCqrsPatternFiles()
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildGenericJavaScriptCqrsPatternFiles() =>
        new[]
        {
            ("src/application/commands/create-project-command.js", """
class CreateProjectCommand {
  constructor(payload = {}) {
    this.payload = payload;
  }
}

module.exports = { CreateProjectCommand };
"""),
            ("src/application/queries/get-project-query.js", """
class GetProjectQuery {
  constructor(id) {
    this.id = id;
  }
}

module.exports = { GetProjectQuery };
"""),
            ("src/application/handlers/create-project-command-handler.js", """
class CreateProjectCommandHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle(command) {
    return this.projects.create(command.payload);
  }
}

module.exports = { CreateProjectCommandHandler };
"""),
            ("src/application/handlers/get-project-query-handler.js", """
class GetProjectQueryHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle(query) {
    return this.projects.find(query.id);
  }
}

module.exports = { GetProjectQueryHandler };
"""),
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildNestJsCqrsPatternFiles() =>
        new[]
        {
            ("src/cqrs/commands/create-project.command.js", """
class CreateProjectCommand {
  constructor(payload = {}) {
    this.payload = payload;
  }
}

module.exports = { CreateProjectCommand };
"""),
            ("src/cqrs/queries/get-project.query.js", """
class GetProjectQuery {
  constructor(id) {
    this.id = id;
  }
}

module.exports = { GetProjectQuery };
"""),
            ("src/cqrs/queries/get-projects.query.js", """
class GetProjectsQuery {}

module.exports = { GetProjectsQuery };
"""),
            ("src/cqrs/events/project-created.event.js", """
class ProjectCreatedEvent {
  constructor(projectId, name, description = null) {
    this.projectId = projectId;
    this.name = name;
    this.description = description;
  }
}

module.exports = { ProjectCreatedEvent };
"""),
            ("src/cqrs/handlers/create-project.handler.js", """
const { CommandHandler, EventBus } = require('@nestjs/cqrs');
const { CreateProjectCommand } = require('../commands/create-project.command');
const { ProjectCreatedEvent } = require('../events/project-created.event');

@CommandHandler(CreateProjectCommand)
class CreateProjectHandler {
  constructor(projects, eventBus) {
    this.projects = projects;
    this.eventBus = eventBus;
  }

  async execute(command) {
    const project = this.projects.create(command.payload);
    this.eventBus.publish(new ProjectCreatedEvent(project.id, project.name, project.description ?? null));
    return project;
  }
}

module.exports = { CreateProjectHandler };
"""),
            ("src/cqrs/handlers/get-project.handler.js", """
const { QueryHandler } = require('@nestjs/cqrs');
const { GetProjectQuery } = require('../queries/get-project.query');

@QueryHandler(GetProjectQuery)
class GetProjectHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async execute(query) {
    return this.projects.find(query.id);
  }
}

module.exports = { GetProjectHandler };
"""),
            ("src/cqrs/handlers/get-projects.handler.js", """
const { QueryHandler } = require('@nestjs/cqrs');
const { GetProjectsQuery } = require('../queries/get-projects.query');

@QueryHandler(GetProjectsQuery)
class GetProjectsHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async execute() {
    return this.projects.all();
  }
}

module.exports = { GetProjectsHandler };
"""),
            ("src/cqrs/handlers/project-created.handler.js", """
const { EventsHandler } = require('@nestjs/cqrs');
const { ProjectCreatedEvent } = require('../events/project-created.event');

@EventsHandler(ProjectCreatedEvent)
class ProjectCreatedHandler {
  handle(event) {
    console.log(`Project created: ${event.projectId} - ${event.name}`);
  }
}

module.exports = { ProjectCreatedHandler };
"""),
            ("src/projects/projects.repository.js", """
class ProjectsRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  create(payload) {
    const project = {
      id: this.nextId++,
      name: payload.name,
      description: payload.description ?? null,
      createdAt: new Date().toISOString(),
    };

    this.items.set(project.id, project);
    return project;
  }

  find(id) {
    return this.items.get(Number(id)) ?? null;
  }

  all() {
    return Array.from(this.items.values());
  }
}

module.exports = { ProjectsRepository };
"""),
            ("src/projects/projects.service.js", """
const { CommandBus, QueryBus } = require('@nestjs/cqrs');
const { CreateProjectCommand } = require('../cqrs/commands/create-project.command');
const { GetProjectQuery } = require('../cqrs/queries/get-project.query');
const { GetProjectsQuery } = require('../cqrs/queries/get-projects.query');

class ProjectsService {
  constructor(commandBus, queryBus) {
    this.commandBus = commandBus;
    this.queryBus = queryBus;
  }

  create(payload) {
    return this.commandBus.execute(new CreateProjectCommand(payload));
  }

  findOne(id) {
    return this.queryBus.execute(new GetProjectQuery(id));
  }

  findAll() {
    return this.queryBus.execute(new GetProjectsQuery());
  }
}

module.exports = { ProjectsService };
"""),
            ("src/projects/projects.controller.js", """
const { Body, Controller, Get, Param, ParseIntPipe, Post } = require('@nestjs/common');

@Controller('projects')
class ProjectsController {
  constructor(projects) {
    this.projects = projects;
  }

  @Post()
  async create(@Body() body) {
    return this.projects.create(body);
  }

  @Get()
  async findAll() {
    return this.projects.findAll();
  }

  @Get(':id')
  async findOne(@Param('id', ParseIntPipe) id) {
    return this.projects.findOne(id);
  }
}

module.exports = { ProjectsController };
"""),
            ("src/projects/projects.module.js", """
const { Module } = require('@nestjs/common');
const { CqrsModule } = require('@nestjs/cqrs');
const { ProjectsController } = require('./projects.controller');
const { ProjectsService } = require('./projects.service');
const { ProjectsRepository } = require('./projects.repository');
const { CreateProjectHandler } = require('../cqrs/handlers/create-project.handler');
const { GetProjectHandler } = require('../cqrs/handlers/get-project.handler');
const { GetProjectsHandler } = require('../cqrs/handlers/get-projects.handler');
const { ProjectCreatedHandler } = require('../cqrs/handlers/project-created.handler');

@Module({
  imports: [CqrsModule],
  controllers: [ProjectsController],
  providers: [
    ProjectsService,
    ProjectsRepository,
    CreateProjectHandler,
    GetProjectHandler,
    GetProjectsHandler,
    ProjectCreatedHandler,
  ],
})
class ProjectsModule {}

module.exports = { ProjectsModule };
"""),
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildNodeJsCqrsPatternFiles() =>
        BuildCommonJsCqrsCoreFiles()
            .Concat(new[]
            {
                ("src/app.js", """
const { createCqrsRuntime } = require('./cqrs/runtime');
const { CreateProjectCommand } = require('./cqrs/commands/create-project.command');
const { GetProjectQuery } = require('./cqrs/queries/get-project.query');
const { GetProjectsQuery } = require('./cqrs/queries/get-projects.query');

const runtime = createCqrsRuntime();

function readBody(req) {
  return new Promise(resolve => {
    let raw = '';
    req.on('data', chunk => {
      raw += chunk;
    });
    req.on('end', () => {
      if (!raw) {
        resolve({});
        return;
      }

      try {
        resolve(JSON.parse(raw));
      } catch {
        resolve({});
      }
    });
  });
}

function sendJson(res, statusCode, payload) {
  res.writeHead(statusCode, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(payload));
}

function createApp() {
  return async (req, res) => {
    const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);

    if (req.method === 'GET' && url.pathname === '/health') {
      sendJson(res, 200, { status: 'ok', app: 'node-cqrs' });
      return;
    }

    if (req.method === 'GET' && url.pathname === '/projects') {
      const projects = await runtime.queryBus.execute(new GetProjectsQuery());
      sendJson(res, 200, projects);
      return;
    }

    if (req.method === 'GET' && url.pathname.startsWith('/projects/')) {
      const id = Number(url.pathname.split('/')[2]);
      const project = await runtime.queryBus.execute(new GetProjectQuery(id));
      if (!project) {
        sendJson(res, 404, { message: 'Project not found' });
        return;
      }

      sendJson(res, 200, project);
      return;
    }

    if (req.method === 'POST' && url.pathname === '/projects') {
      const body = await readBody(req);
      const project = await runtime.commandBus.execute(new CreateProjectCommand(body));
      sendJson(res, 201, project);
      return;
    }

    sendJson(res, 404, { message: 'Not Found' });
  };
}

module.exports = createApp;
"""),
            })
            .ToList();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildExpressJsCqrsPatternFiles() =>
        BuildCommonJsCqrsCoreFiles()
            .Concat(new[]
            {
                ("src/app.js", """
const express = require('express');
const { createCqrsRuntime } = require('./cqrs/runtime');
const { CreateProjectCommand } = require('./cqrs/commands/create-project.command');
const { GetProjectQuery } = require('./cqrs/queries/get-project.query');
const { GetProjectsQuery } = require('./cqrs/queries/get-projects.query');

function createApp() {
  const app = express();
  const runtime = createCqrsRuntime();

  app.use(express.json());
  app.get('/health', (_req, res) => res.json({ status: 'ok', app: 'express-cqrs' }));
  app.get('/', (_req, res) => res.json({ app: 'express-cqrs', status: 'running' }));

  app.get('/projects', async (_req, res) => {
    const projects = await runtime.queryBus.execute(new GetProjectsQuery());
    res.json(projects);
  });

  app.get('/projects/:id', async (req, res) => {
    const project = await runtime.queryBus.execute(new GetProjectQuery(Number(req.params.id)));
    if (!project) {
      res.status(404).json({ message: 'Project not found' });
      return;
    }

    res.json(project);
  });

  app.post('/projects', async (req, res) => {
    const project = await runtime.commandBus.execute(new CreateProjectCommand(req.body ?? {}));
    res.status(201).json(project);
  });

  return app;
}

module.exports = createApp();
"""),
            })
            .ToList();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildNextJsCqrsPatternFiles() =>
        BuildNextJsCqrsCoreFiles()
            .Concat(new[]
            {
                ("src/app/api/projects/route.js", """
import { createCqrsRuntime } from '../../../lib/cqrs/runtime.js';
import { CreateProjectCommand } from '../../../lib/cqrs/commands/create-project.command.js';
import { GetProjectsQuery } from '../../../lib/cqrs/queries/get-projects.query.js';

const runtime = createCqrsRuntime();

export async function GET() {
  const projects = await runtime.queryBus.execute(new GetProjectsQuery());
  return Response.json(projects);
}

export async function POST(request) {
  const payload = await request.json();
  const project = await runtime.commandBus.execute(new CreateProjectCommand(payload));
  return Response.json(project, { status: 201 });
}
"""),
                ("src/app/api/projects/[id]/route.js", """
import { createCqrsRuntime } from '../../../../lib/cqrs/runtime.js';
import { GetProjectQuery } from '../../../../lib/cqrs/queries/get-project.query.js';

const runtime = createCqrsRuntime();

export async function GET(_request, { params }) {
  const project = await runtime.queryBus.execute(new GetProjectQuery(Number(params.id)));
  if (!project) {
    return Response.json({ message: 'Project not found' }, { status: 404 });
  }

  return Response.json(project);
}
"""),
            })
            .ToList();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildCommonJsCqrsCoreFiles() =>
        new[]
        {
            ("src/domain/project.js", """
class Project {
  constructor({ id = null, name = '', description = null, createdAt = null } = {}) {
    this.id = id;
    this.name = name;
    this.description = description;
    this.createdAt = createdAt;
  }
}

module.exports = { Project };
"""),
            ("src/cqrs/command-bus.js", """
class CommandBus {
  constructor() {
    this.handlers = new Map();
  }

  register(commandType, handler) {
    this.handlers.set(commandType.name, handler);
    return this;
  }

  async execute(command) {
    const handler = this.handlers.get(command.constructor.name);
    if (!handler) {
      throw new Error(`No command handler registered for ${command.constructor.name}`);
    }

    return handler.handle(command);
  }
}

module.exports = { CommandBus };
"""),
            ("src/cqrs/query-bus.js", """
class QueryBus {
  constructor() {
    this.handlers = new Map();
  }

  register(queryType, handler) {
    this.handlers.set(queryType.name, handler);
    return this;
  }

  async execute(query) {
    const handler = this.handlers.get(query.constructor.name);
    if (!handler) {
      throw new Error(`No query handler registered for ${query.constructor.name}`);
    }

    return handler.handle(query);
  }
}

module.exports = { QueryBus };
"""),
            ("src/cqrs/event-bus.js", """
class EventBus {
  constructor() {
    this.listeners = new Map();
  }

  subscribe(eventType, listener) {
    const key = eventType.name;
    const current = this.listeners.get(key) ?? [];
    current.push(listener);
    this.listeners.set(key, current);
    return this;
  }

  async publish(event) {
    const listeners = this.listeners.get(event.constructor.name) ?? [];
    await Promise.all(listeners.map(listener => listener.handle(event)));
  }
}

module.exports = { EventBus };
"""),
            ("src/cqrs/commands/create-project.command.js", """
class CreateProjectCommand {
  constructor(payload = {}) {
    this.payload = payload;
  }
}

module.exports = { CreateProjectCommand };
"""),
            ("src/cqrs/queries/get-project.query.js", """
class GetProjectQuery {
  constructor(id) {
    this.id = id;
  }
}

module.exports = { GetProjectQuery };
"""),
            ("src/cqrs/queries/get-projects.query.js", """
class GetProjectsQuery {}

module.exports = { GetProjectsQuery };
"""),
            ("src/cqrs/events/project-created.event.js", """
class ProjectCreatedEvent {
  constructor(projectId, name, description = null) {
    this.projectId = projectId;
    this.name = name;
    this.description = description;
  }
}

module.exports = { ProjectCreatedEvent };
"""),
            ("src/cqrs/handlers/create-project.handler.js", """
const { ProjectCreatedEvent } = require('../events/project-created.event');

class CreateProjectHandler {
  constructor(projects, eventBus) {
    this.projects = projects;
    this.eventBus = eventBus;
  }

  async handle(command) {
    const project = this.projects.create(command.payload);
    await this.eventBus.publish(new ProjectCreatedEvent(project.id, project.name, project.description));
    return project;
  }
}

module.exports = { CreateProjectHandler };
"""),
            ("src/cqrs/handlers/get-project.handler.js", """
class GetProjectHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle(query) {
    return this.projects.find(query.id);
  }
}

module.exports = { GetProjectHandler };
"""),
            ("src/cqrs/handlers/get-projects.handler.js", """
class GetProjectsHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle() {
    return this.projects.all();
  }
}

module.exports = { GetProjectsHandler };
"""),
            ("src/cqrs/handlers/project-created.handler.js", """
class ProjectCreatedHandler {
  async handle(event) {
    console.log(`Project created: ${event.projectId} - ${event.name}`);
  }
}

module.exports = { ProjectCreatedHandler };
"""),
            ("src/cqrs/runtime.js", """
const { CommandBus } = require('./command-bus');
const { QueryBus } = require('./query-bus');
const { EventBus } = require('./event-bus');
const { ProjectsRepository } = require('../infrastructure/projects.repository');
const { CreateProjectCommand } = require('./commands/create-project.command');
const { GetProjectQuery } = require('./queries/get-project.query');
const { GetProjectsQuery } = require('./queries/get-projects.query');
const { ProjectCreatedEvent } = require('./events/project-created.event');
const { CreateProjectHandler } = require('./handlers/create-project.handler');
const { GetProjectHandler } = require('./handlers/get-project.handler');
const { GetProjectsHandler } = require('./handlers/get-projects.handler');
const { ProjectCreatedHandler } = require('./handlers/project-created.handler');

function createCqrsRuntime() {
  const projects = new ProjectsRepository();
  const commandBus = new CommandBus();
  const queryBus = new QueryBus();
  const eventBus = new EventBus();

  commandBus.register(CreateProjectCommand, new CreateProjectHandler(projects, eventBus));
  queryBus.register(GetProjectQuery, new GetProjectHandler(projects));
  queryBus.register(GetProjectsQuery, new GetProjectsHandler(projects));
  eventBus.subscribe(ProjectCreatedEvent, new ProjectCreatedHandler());

  return { projects, commandBus, queryBus, eventBus };
}

module.exports = { createCqrsRuntime };
"""),
            ("src/infrastructure/projects.repository.js", """
const { Project } = require('../domain/project');

class ProjectsRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  create(payload) {
    const project = new Project({
      id: this.nextId++,
      name: payload.name,
      description: payload.description ?? null,
      createdAt: new Date().toISOString(),
    });

    this.items.set(project.id, project);
    return project;
  }

  find(id) {
    return this.items.get(Number(id)) ?? null;
  }

  all() {
    return Array.from(this.items.values());
  }
}

module.exports = { ProjectsRepository };
"""),
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildNextJsCqrsCoreFiles() =>
        new[]
        {
            ("src/lib/projects/project.js", """
export class Project {
  constructor({ id = null, name = '', description = null, createdAt = null } = {}) {
    this.id = id;
    this.name = name;
    this.description = description;
    this.createdAt = createdAt;
  }
}
"""),
            ("src/lib/cqrs/command-bus.js", """
export class CommandBus {
  constructor() {
    this.handlers = new Map();
  }

  register(commandType, handler) {
    this.handlers.set(commandType.name, handler);
    return this;
  }

  async execute(command) {
    const handler = this.handlers.get(command.constructor.name);
    if (!handler) {
      throw new Error(`No command handler registered for ${command.constructor.name}`);
    }

    return handler.handle(command);
  }
}
"""),
            ("src/lib/cqrs/query-bus.js", """
export class QueryBus {
  constructor() {
    this.handlers = new Map();
  }

  register(queryType, handler) {
    this.handlers.set(queryType.name, handler);
    return this;
  }

  async execute(query) {
    const handler = this.handlers.get(query.constructor.name);
    if (!handler) {
      throw new Error(`No query handler registered for ${query.constructor.name}`);
    }

    return handler.handle(query);
  }
}
"""),
            ("src/lib/cqrs/event-bus.js", """
export class EventBus {
  constructor() {
    this.listeners = new Map();
  }

  subscribe(eventType, listener) {
    const key = eventType.name;
    const current = this.listeners.get(key) ?? [];
    current.push(listener);
    this.listeners.set(key, current);
    return this;
  }

  async publish(event) {
    const listeners = this.listeners.get(event.constructor.name) ?? [];
    await Promise.all(listeners.map(listener => listener.handle(event)));
  }
}
"""),
            ("src/lib/cqrs/commands/create-project.command.js", """
export class CreateProjectCommand {
  constructor(payload = {}) {
    this.payload = payload;
  }
}
"""),
            ("src/lib/cqrs/queries/get-project.query.js", """
export class GetProjectQuery {
  constructor(id) {
    this.id = id;
  }
}
"""),
            ("src/lib/cqrs/queries/get-projects.query.js", """
export class GetProjectsQuery {}
"""),
            ("src/lib/cqrs/events/project-created.event.js", """
export class ProjectCreatedEvent {
  constructor(projectId, name, description = null) {
    this.projectId = projectId;
    this.name = name;
    this.description = description;
  }
}
"""),
            ("src/lib/cqrs/handlers/create-project.handler.js", """
import { ProjectCreatedEvent } from '../events/project-created.event.js';

export class CreateProjectHandler {
  constructor(projects, eventBus) {
    this.projects = projects;
    this.eventBus = eventBus;
  }

  async handle(command) {
    const project = this.projects.create(command.payload);
    await this.eventBus.publish(new ProjectCreatedEvent(project.id, project.name, project.description));
    return project;
  }
}
"""),
            ("src/lib/cqrs/handlers/get-project.handler.js", """
export class GetProjectHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle(query) {
    return this.projects.find(query.id);
  }
}
"""),
            ("src/lib/cqrs/handlers/get-projects.handler.js", """
export class GetProjectsHandler {
  constructor(projects) {
    this.projects = projects;
  }

  async handle() {
    return this.projects.all();
  }
}
"""),
            ("src/lib/cqrs/handlers/project-created.handler.js", """
export class ProjectCreatedHandler {
  async handle(event) {
    console.log(`Project created: ${event.projectId} - ${event.name}`);
  }
}
"""),
            ("src/lib/cqrs/runtime.js", """
import { CommandBus } from './command-bus.js';
import { QueryBus } from './query-bus.js';
import { EventBus } from './event-bus.js';
import { ProjectsRepository } from '../projects/projects.repository.js';
import { CreateProjectCommand } from './commands/create-project.command.js';
import { GetProjectQuery } from './queries/get-project.query.js';
import { GetProjectsQuery } from './queries/get-projects.query.js';
import { ProjectCreatedEvent } from './events/project-created.event.js';
import { CreateProjectHandler } from './handlers/create-project.handler.js';
import { GetProjectHandler } from './handlers/get-project.handler.js';
import { GetProjectsHandler } from './handlers/get-projects.handler.js';
import { ProjectCreatedHandler } from './handlers/project-created.handler.js';

export function createCqrsRuntime() {
  const projects = new ProjectsRepository();
  const commandBus = new CommandBus();
  const queryBus = new QueryBus();
  const eventBus = new EventBus();

  commandBus.register(CreateProjectCommand, new CreateProjectHandler(projects, eventBus));
  queryBus.register(GetProjectQuery, new GetProjectHandler(projects));
  queryBus.register(GetProjectsQuery, new GetProjectsHandler(projects));
  eventBus.subscribe(ProjectCreatedEvent, new ProjectCreatedHandler());

  return { projects, commandBus, queryBus, eventBus };
}
"""),
            ("src/lib/projects/projects.repository.js", """
import { Project } from './project.js';

export class ProjectsRepository {
  constructor() {
    this.items = new Map();
    this.nextId = 1;
  }

  create(payload) {
    const project = new Project({
      id: this.nextId++,
      name: payload.name,
      description: payload.description ?? null,
      createdAt: new Date().toISOString(),
    });

    this.items.set(project.id, project);
    return project;
  }

  find(id) {
    return this.items.get(Number(id)) ?? null;
  }

  all() {
    return Array.from(this.items.values());
  }
}
"""),
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptBaseFiles(
        FrameworkType framework,
        string projectName)
    {
        var appName = projectName.ToLowerInvariant().Replace(" ", "-");
        var files = framework switch
        {
            FrameworkType.NodeJs => new[]
        {
            ("src/index.js", """
const http = require('http');
const createApp = require('./app');

const port = process.env.PORT || 3000;
const app = createApp();

http.createServer(app).listen(port, () => {
  console.log(`{{APP_NAME}} listening on port ${port}`);
});
"""),
            ("src/app.js", """
function createApp() {
  return (req, res) => {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ app: '{{APP_NAME}}', status: 'ok' }));
  };
}

module.exports = createApp;
"""),
        },
            FrameworkType.ExpressJs => new[]
        {
            ("src/server.js", """
const app = require('./app');

const port = process.env.PORT || 3000;

app.listen(port, () => {
  console.log(`{{APP_NAME}} listening on port ${port}`);
});
"""),
            ("src/app.js", """
const express = require('express');

function createApp() {
  const app = express();

  app.use(express.json());
  app.get('/health', (_req, res) => res.json({ status: 'ok', app: '{{APP_NAME}}' }));
  app.get('/', (_req, res) => res.json({ app: '{{APP_NAME}}', status: 'running' }));

  return app;
}

module.exports = createApp();
"""),
            ("src/routes/index.js", """
module.exports = function registerRoutes(app) {
  app.get('/ping', (_req, res) => res.json({ ok: true }));
};
"""),
        },
            _ => []
        };

        return files
            .Select(file => (RelativePath: file.Item1, Content: file.Item2.Replace("{{APP_NAME}}", appName)))
            .ToList();
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelPatternFiles(
        DesignPattern pattern,
        string appName) => pattern switch
    {
        DesignPattern.Repository => new[]
        {
            ("app/Contracts/ProjectRepositoryInterface.php", """
<?php

namespace App\Contracts;

interface ProjectRepositoryInterface
{
    public function all(): array;

    public function find(int $id): ?Project;

    public function create(array $data): array;

    public function update(int $id, array $data): ?array;
}
"""),
            ("app/Models/Project.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Project extends Model
{
    protected $fillable = ['name', 'description'];
}
"""),
            ("app/Repositories/ProjectRepository.php", """
<?php

namespace App\Repositories;

use App\Contracts\ProjectRepositoryInterface;
use App\Models\Project as ProjectModel;

class ProjectRepository implements ProjectRepositoryInterface
{
    public function all(): array
    {
        return ProjectModel::query()->latest()->get()->toArray();
    }

    public function find(int $id): ?Project
    {
        return ProjectModel::query()->find($id)?->toArray();
    }

    public function create(array $data): array
    {
        return ProjectModel::query()->create($data)->toArray();
    }

    public function update(int $id, array $data): ?array
    {
        $project = ProjectModel::query()->find($id);
        if (!$project) {
            return null;
        }

        $project->fill($data);
        $project->save();

        return $project->toArray();
    }
}
"""),
            ("app/Providers/ProjectRepositoryServiceProvider.php", """
<?php

namespace App\Providers;

use App\Contracts\ProjectRepositoryInterface;
use App\Repositories\ProjectRepository;
use Illuminate\Support\ServiceProvider;

class ProjectRepositoryServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryInterface::class, ProjectRepository::class);
    }
}
"""),
        },
        DesignPattern.CleanArchitecture => new[]
        {
            ("app/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = '',
        public readonly ?string $description = null
    ) {
    }
}
"""),
            ("app/Contracts/ProjectRepositoryInterface.php", """
<?php

namespace App\Contracts;

use App\Domain\Entities\Project;

interface ProjectRepositoryInterface
{
    public function save(Project $project): Project;
}
"""),
            ("app/Application/UseCases/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Contracts\ProjectRepositoryInterface;
use App\Domain\Entities\Project;

final class CreateProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects)
    {
    }

    public function execute(string $name, ?string $description = null): Project
    {
        return $this->projects->save(new Project(name: $name, description: $description));
    }
}
"""),
            ("app/Models/Project.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Project extends Model
{
    protected $fillable = ['name', 'description'];
}
"""),
            ("app/Infrastructure/Persistence/EloquentProjectRepository.php", """
<?php

namespace App\Infrastructure\Persistence;

use App\Contracts\ProjectRepositoryInterface;
use App\Domain\Entities\Project;
use App\Models\Project as ProjectModel;

final class EloquentProjectRepository implements ProjectRepositoryInterface
{
    public function save(Project $project): Project
    {
        $model = ProjectModel::query()->create([
            'name' => $project->name,
            'description' => $project->description,
        ]);

        return new Project(
            id: $model->id,
            name: $model->name,
            description: $model->description,
        );
    }
}
"""),
            ("app/Providers/CleanArchitectureServiceProvider.php", """
<?php

namespace App\Providers;

use App\Contracts\ProjectRepositoryInterface;
use App\Infrastructure\Persistence\EloquentProjectRepository;
use Illuminate\Support\ServiceProvider;

final class CleanArchitectureServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryInterface::class, EloquentProjectRepository::class);
    }
}
"""),
            ("database/migrations/2026_01_01_000000_create_projects_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('projects', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('projects');
    }
};
"""),
        },
        DesignPattern.HexagonalArchitecture => new[]
        {
            ("app/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = '',
        public readonly ?string $description = null
    ) {
    }
}
"""),
            ("app/Ports/ProjectRepositoryPort.php", """
<?php

namespace App\Ports;

use App\Domain\Entities\Project;

interface ProjectRepositoryPort
{
    public function save(Project $project): Project;
}
"""),
            ("app/Application/UseCases/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Domain\Entities\Project;
use App\Ports\ProjectRepositoryPort;

final class CreateProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryPort $projects)
    {
    }

    public function execute(string $name, ?string $description = null): Project
    {
        return $this->projects->save(new Project(name: $name, description: $description));
    }
}
"""),
            ("app/Models/Project.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Project extends Model
{
    protected $fillable = ['name', 'description'];
}
"""),
            ("app/Adapters/Persistence/EloquentProjectRepository.php", """
<?php

namespace App\Adapters\Persistence;

use App\Domain\Entities\Project;
use App\Models\Project as ProjectModel;
use App\Ports\ProjectRepositoryPort;

final class EloquentProjectRepository implements ProjectRepositoryPort
{
    public function save(Project $project): Project
    {
        $model = ProjectModel::query()->create([
            'name' => $project->name,
            'description' => $project->description,
        ]);

        return new Project(
            id: $model->id,
            name: $model->name,
            description: $model->description,
        );
    }
}
"""),
            ("app/Providers/HexagonalServiceProvider.php", """
<?php

namespace App\Providers;

use App\Adapters\Persistence\EloquentProjectRepository;
use App\Ports\ProjectRepositoryPort;
use Illuminate\Support\ServiceProvider;

final class HexagonalServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryPort::class, EloquentProjectRepository::class);
    }
}
"""),
            ("database/migrations/2026_01_01_000000_create_projects_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('projects', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('projects');
    }
};
"""),
        },
        DesignPattern.DomainDrivenDesign => new[]
        {
            ("app/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = ''
    ) {
    }
}
"""),
            ("app/Domain/ValueObjects/ProjectName.php", """
<?php

namespace App\Domain\ValueObjects;

final class ProjectName
{
    public function __construct(public readonly string $value)
    {
    }
}
"""),
            ("app/Application/Services/ProjectCreator.php", """
<?php

namespace App\Application\Services;

final class ProjectCreator
{
    public function create(array $data): array
    {
        return $data;
    }
}
"""),
        },
        DesignPattern.EventSourcing => new[]
        {
            ("app/Events/ProjectCreated.php", """
<?php

namespace App\Events;

final class ProjectCreated
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("app/Listeners/RecordProjectCreated.php", """
<?php

namespace App\Listeners;

use App\Events\ProjectCreated;

final class RecordProjectCreated
{
    public function handle(ProjectCreated $event): void
    {
        // TODO: persistir evento
    }
}
"""),
            ("app/Jobs/ReplayProjectEvents.php", """
<?php

namespace App\Jobs;

final class ReplayProjectEvents
{
    public function handle(): void
    {
        // TODO: reprocesar eventos
    }
}
"""),
        },
        DesignPattern.Microservices => new[]
        {
            ("app/Services/ProjectClient.php", """
<?php

namespace App\Services;

final class ProjectClient
{
    public function request(array $payload): array
    {
        return $payload;
    }
}
"""),
            ("app/Jobs/SyncProject.php", """
<?php

namespace App\Jobs;

final class SyncProject
{
    public function handle(): void
    {
        // TODO: sincronizar microservicios
    }
}
"""),
            ("app/Integrations/GitHub/GitHubRepositoryClient.php", """
<?php

namespace App\Integrations\GitHub;

final class GitHubRepositoryClient
{
    public function createRepository(array $payload): array
    {
        return $payload;
    }
}
"""),
        },
        DesignPattern.CQRS => new[]
        {
            ("app/Commands/CreateProjectCommand.php", """
<?php

namespace App\Commands;

final class CreateProjectCommand
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("app/Queries/GetProjectQuery.php", """
<?php

namespace App\Queries;

final class GetProjectQuery
{
    public function __construct(public readonly int $id)
    {
    }
}
"""),
            ("app/Handlers/CreateProjectHandler.php", """
<?php

namespace App\Handlers;

final class CreateProjectHandler
{
    public function handle(array $payload): array
    {
        return $payload;
    }
}
"""),
        },
        DesignPattern.Mediator => new[]
        {
            ("app/Actions/CreateProjectAction.php", """
<?php

namespace App\Actions;

final class CreateProjectAction
{
    public function execute(array $payload): array
    {
        return $payload;
    }
}
"""),
            ("app/Actions/NotifyProjectCreatedAction.php", """
<?php

namespace App\Actions;

final class NotifyProjectCreatedAction
{
    public function execute(array $payload): void
    {
        // TODO: notificar creación
    }
}
"""),
            ("app/Services/ProjectMediator.php", """
<?php

namespace App\Services;

final class ProjectMediator
{
    public function dispatch(object $message): mixed
    {
        return $message;
    }
}
"""),
        },
        DesignPattern.Saga => new[]
        {
            ("app/Sagas/ProjectProvisioningSaga.php", """
<?php

namespace App\Sagas;

final class ProjectProvisioningSaga
{
    public function run(array $payload): array
    {
        return $payload;
    }
}
"""),
            ("app/Events/ProjectProvisioned.php", """
<?php

namespace App\Events;

final class ProjectProvisioned
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("app/Listeners/CompleteProjectProvisioning.php", """
<?php

namespace App\Listeners;

use App\Events\ProjectProvisioned;

final class CompleteProjectProvisioning
{
    public function handle(ProjectProvisioned $event): void
    {
        // TODO: completar saga
    }
}
"""),
        },
        _ => []
    };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyPatternFiles(
        DesignPattern pattern,
        string appName) => pattern switch
    {
        DesignPattern.Repository => new[]
        {
            ("src/Entity/Project.php", """
<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'projects')]
class Project
{
    public function __construct(
        #[ORM\Id]
        #[ORM\GeneratedValue]
        #[ORM\Column(type: 'integer')]
        public ?int $id = null,
        #[ORM\Column(length: 255)]
        public string $name = '',
        #[ORM\Column(type: 'text', nullable: true)]
        public ?string $description = null
    ) {
    }
}
"""),
            ("src/Contract/ProjectRepositoryInterface.php", """
<?php

namespace App\Contract;

use App\Entity\Project;

interface ProjectRepositoryInterface
{
    public function all(): array;

    public function find(int $id): ?array;

    public function create(Project $project): Project;

    public function update(int $id, Project $project): ?Project;
}
"""),
            ("src/Repository/ProjectRepository.php", """
<?php

namespace App\Repository;

use App\Contract\ProjectRepositoryInterface;
use App\Entity\Project;
use Doctrine\ORM\EntityManagerInterface;

final class ProjectRepository implements ProjectRepositoryInterface
{
    public function __construct(private readonly EntityManagerInterface $entityManager)
    {
    }

    public function all(): array
    {
        return $this->entityManager->getRepository(Project::class)->findBy([], ['id' => 'DESC']);
    }

    public function find(int $id): ?Project
    {
        return $this->entityManager->find(Project::class, $id);
    }

    public function create(Project $project): Project
    {
        $this->entityManager->persist($project);
        $this->entityManager->flush();

        return $project;
    }

    public function update(int $id, Project $project): ?Project
    {
        $existing = $this->entityManager->find(Project::class, $id);

        if (!$existing) {
            return null;
        }

        $existing->name = $project->name;
        $existing->description = $project->description;
        $this->entityManager->flush();

        return $existing;
    }
}
"""),
            ("src/Application/UseCase/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCase;

use App\Contract\ProjectRepositoryInterface;
use App\Entity\Project;

final class CreateProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects)
    {
    }

    public function execute(string $name, ?string $description = null): Project
    {
        $project = new Project();
        $project->name = $name;
        $project->description = $description;

        return $this->projects->create($project);
    }
}
"""),
            ("migrations/Version20260101000000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260101000000 extends AbstractMigration
{
    public function getDescription(): string
    {
        return 'Create projects table';
    }

    public function up(Schema $schema): void
    {
        $table = $schema->createTable('projects');
        $table->addColumn('id', 'integer', ['autoincrement' => true]);
        $table->addColumn('name', 'string', ['length' => 255]);
        $table->addColumn('description', 'text', ['notnull' => false]);
        $table->setPrimaryKey(['id']);
    }

    public function down(Schema $schema): void
    {
        $schema->dropTable('projects');
    }
}
"""),
        },
        DesignPattern.CleanArchitecture => new[]
        {
            ("src/Domain/Entity/Project.php", """
<?php

namespace App\Domain\Entity;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = '',
        public readonly ?string $description = null
    ) {
    }
}
"""),
            ("src/Contract/ProjectRepositoryInterface.php", """
<?php

namespace App\Contract;

use App\Domain\Entity\Project;

interface ProjectRepositoryInterface
{
    public function save(Project $project): Project;

    public function find(int $id): ?Project;

    public function all(): array;
}
"""),
            ("src/Application/UseCase/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCase;

use App\Contract\ProjectRepositoryInterface;
use App\Domain\Entity\Project;

final class CreateProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects)
    {
    }

    public function execute(string $name, ?string $description = null): Project
    {
        return $this->projects->save(new Project(name: $name, description: $description));
    }
}
"""),
            ("src/Entity/Project.php", """
<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'projects')]
class Project
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column(type: 'integer')]
    public ?int $id = null;

    #[ORM\Column(length: 255)]
    public string $name = '';

    #[ORM\Column(type: 'text', nullable: true)]
    public ?string $description = null;
}
"""),
            ("src/Infrastructure/Persistence/DoctrineProjectRepository.php", """
<?php

namespace App\Infrastructure\Persistence;

use App\Contract\ProjectRepositoryInterface;
use App\Domain\Entity\Project as ProjectDomain;
use App\Entity\Project as ProjectRecord;
use Doctrine\ORM\EntityManagerInterface;

final class DoctrineProjectRepository implements ProjectRepositoryInterface
{
    public function __construct(private readonly EntityManagerInterface $entityManager)
    {
    }

    public function save(ProjectDomain $project): ProjectDomain
    {
        $record = $project->id !== null
            ? $this->entityManager->find(ProjectRecord::class, $project->id)
            : new ProjectRecord();

        if (!$record instanceof ProjectRecord) {
            $record = new ProjectRecord();
        }

        $record->name = $project->name;
        $record->description = $project->description;

        $this->entityManager->persist($record);
        $this->entityManager->flush();

        return new ProjectDomain(
            id: $record->id,
            name: $record->name,
            description: $record->description,
        );
    }

    public function find(int $id): ?ProjectDomain
    {
        $record = $this->entityManager->find(ProjectRecord::class, $id);

        if (!$record instanceof ProjectRecord) {
            return null;
        }

        return new ProjectDomain(
            id: $record->id,
            name: $record->name,
            description: $record->description,
        );
    }

    public function all(): array
    {
        $records = $this->entityManager->getRepository(ProjectRecord::class)->findBy([], ['id' => 'DESC']);

        return array_map(
            static fn (ProjectRecord $record): ProjectDomain => new ProjectDomain(
                id: $record->id,
                name: $record->name,
                description: $record->description,
            ),
            $records
        );
    }
}
"""),
            ("migrations/Version20260101000000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260101000000 extends AbstractMigration
{
    public function getDescription(): string
    {
        return 'Create projects table';
    }

    public function up(Schema $schema): void
    {
        $table = $schema->createTable('projects');
        $table->addColumn('id', 'integer', ['autoincrement' => true]);
        $table->addColumn('name', 'string', ['length' => 255]);
        $table->addColumn('description', 'text', ['notnull' => false]);
        $table->setPrimaryKey(['id']);
    }

    public function down(Schema $schema): void
    {
        $schema->dropTable('projects');
    }
}
"""),
        },
        DesignPattern.HexagonalArchitecture => new[]
        {
            ("src/Port/ProjectRepositoryPort.php", """
<?php

namespace App\Port;

use App\Domain\Entity\Project;

interface ProjectRepositoryPort
{
    public function save(Project $project): Project;

    public function find(int $id): ?Project;

    public function all(): array;
}
"""),
            ("src/Application/UseCase/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCase;

use App\Domain\Entity\Project;
use App\Port\ProjectRepositoryPort;

final class CreateProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryPort $projects)
    {
    }

    public function execute(string $name, ?string $description = null): Project
    {
        return $this->projects->save(new Project(name: $name, description: $description));
    }
}
"""),
            ("src/Domain/Entity/Project.php", """
<?php

namespace App\Domain\Entity;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = '',
        public readonly ?string $description = null
    ) {
    }
}
"""),
            ("src/Entity/Project.php", """
<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'projects')]
class Project
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column(type: 'integer')]
    public ?int $id = null;

    #[ORM\Column(length: 255)]
    public string $name = '';

    #[ORM\Column(type: 'text', nullable: true)]
    public ?string $description = null;
}
"""),
            ("src/Adapters/Persistence/DoctrineProjectRepository.php", """
<?php

namespace App\Adapters\Persistence;

use App\Domain\Entity\Project as ProjectDomain;
use App\Entity\Project as ProjectRecord;
use App\Port\ProjectRepositoryPort;
use Doctrine\ORM\EntityManagerInterface;

final class DoctrineProjectRepository implements ProjectRepositoryPort
{
    public function __construct(private readonly EntityManagerInterface $entityManager)
    {
    }

    public function save(ProjectDomain $project): ProjectDomain
    {
        $record = $project->id !== null
            ? $this->entityManager->find(ProjectRecord::class, $project->id)
            : new ProjectRecord();

        if (!$record instanceof ProjectRecord) {
            $record = new ProjectRecord();
        }

        $record->name = $project->name;
        $record->description = $project->description;

        $this->entityManager->persist($record);
        $this->entityManager->flush();

        return new ProjectDomain(
            id: $record->id,
            name: $record->name,
            description: $record->description,
        );
    }

    public function find(int $id): ?ProjectDomain
    {
        $record = $this->entityManager->find(ProjectRecord::class, $id);

        if (!$record instanceof ProjectRecord) {
            return null;
        }

        return new ProjectDomain(
            id: $record->id,
            name: $record->name,
            description: $record->description,
        );
    }

    public function all(): array
    {
        $records = $this->entityManager->getRepository(ProjectRecord::class)->findBy([], ['id' => 'DESC']);

        return array_map(
            static fn (ProjectRecord $record): ProjectDomain => new ProjectDomain(
                id: $record->id,
                name: $record->name,
                description: $record->description,
            ),
            $records
        );
    }
}
"""),
            ("migrations/Version20260101000000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260101000000 extends AbstractMigration
{
    public function getDescription(): string
    {
        return 'Create projects table';
    }

    public function up(Schema $schema): void
    {
        $table = $schema->createTable('projects');
        $table->addColumn('id', 'integer', ['autoincrement' => true]);
        $table->addColumn('name', 'string', ['length' => 255]);
        $table->addColumn('description', 'text', ['notnull' => false]);
        $table->setPrimaryKey(['id']);
    }

    public function down(Schema $schema): void
    {
        $schema->dropTable('projects');
    }
}
"""),
        },
        DesignPattern.DomainDrivenDesign => new[]
        {
            ("src/Domain/Entity/Project.php", """
<?php

namespace App\Domain\Entity;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public readonly string $name = ''
    ) {
    }
}
"""),
            ("src/Domain/ValueObject/ProjectName.php", """
<?php

namespace App\Domain\ValueObject;

final class ProjectName
{
    public function __construct(public readonly string $value)
    {
    }
}
"""),
            ("src/Application/Service/ProjectCreator.php", """
<?php

namespace App\Application\Service;

final class ProjectCreator
{
    public function create(array $data): array
    {
        return $data;
    }
}
"""),
        },
        DesignPattern.EventSourcing => new[]
        {
            ("src/Event/ProjectCreated.php", """
<?php

namespace App\Event;

final class ProjectCreated
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("src/EventListener/ProjectCreatedListener.php", """
<?php

namespace App\EventListener;

use App\Event\ProjectCreated;

final class ProjectCreatedListener
{
    public function __invoke(ProjectCreated $event): void
    {
        // TODO: persistir evento
    }
}
"""),
            ("src/MessageHandler/ReplayProjectEventsHandler.php", """
<?php

namespace App\MessageHandler;

final class ReplayProjectEventsHandler
{
    public function __invoke(object $message): void
    {
        // TODO: reprocesar eventos
    }
}
"""),
        },
        DesignPattern.Microservices => new[]
        {
            ("src/Service/ProjectClient.php", """
<?php

namespace App\Service;

final class ProjectClient
{
    public function request(array $payload): array
    {
        return $payload;
    }
}
"""),
            ("src/MessageHandler/SyncProjectHandler.php", """
<?php

namespace App\MessageHandler;

final class SyncProjectHandler
{
    public function __invoke(object $message): void
    {
        // TODO: sincronizar microservicios
    }
}
"""),
            ("src/Integration/GitHub/GitHubRepositoryClient.php", """
<?php

namespace App\Integration\GitHub;

final class GitHubRepositoryClient
{
    public function createRepository(array $payload): array
    {
        return $payload;
    }
}
"""),
        },
        DesignPattern.CQRS => new[]
        {
            ("src/Command/CreateProjectCommand.php", """
<?php

namespace App\Command;

final class CreateProjectCommand
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("src/Query/GetProjectQuery.php", """
<?php

namespace App\Query;

final class GetProjectQuery
{
    public function __construct(public readonly int $id)
    {
    }
}
"""),
            ("src/Handler/CreateProjectHandler.php", """
<?php

namespace App\Handler;

final class CreateProjectHandler
{
    public function __invoke(array $payload): array
    {
        return $payload;
    }
}
"""),
        },
        DesignPattern.Mediator => new[]
        {
            ("src/Service/ProjectMediator.php", """
<?php

namespace App\Service;

final class ProjectMediator
{
    public function dispatch(object $message): mixed
    {
        return $message;
    }
}
"""),
            ("src/Message/CreateProjectMessage.php", """
<?php

namespace App\Message;

final class CreateProjectMessage
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("src/MessageHandler/CreateProjectHandler.php", """
<?php

namespace App\MessageHandler;

final class CreateProjectHandler
{
    public function __invoke(object $message): void
    {
        // TODO: procesar mensaje
    }
}
"""),
        },
        DesignPattern.Saga => new[]
        {
            ("src/Saga/ProjectProvisioningSaga.php", """
<?php

namespace App\Saga;

final class ProjectProvisioningSaga
{
    public function run(array $payload): array
    {
        return $payload;
    }
}
"""),
            ("src/Event/ProjectProvisioned.php", """
<?php

namespace App\Event;

final class ProjectProvisioned
{
    public function __construct(public readonly array $payload = [])
    {
    }
}
"""),
            ("src/EventListener/CompleteProjectProvisioningListener.php", """
<?php

namespace App\EventListener;

use App\Event\ProjectProvisioned;

final class CompleteProjectProvisioningListener
{
    public function __invoke(ProjectProvisioned $event): void
    {
        // TODO: completar saga
    }
}
"""),
        },
        _ => []
    };

    private static async Task EnsureLaravelProviderRegistrationAsync(string path, string providerEntry, CancellationToken ct)
    {
        var providersFile = Path.Combine(path, "bootstrap", "providers.php");
        if (!File.Exists(providersFile))
            return;

        var content = await File.ReadAllTextAsync(providersFile, ct);
        if (content.Contains(providerEntry, StringComparison.Ordinal))
            return;

        var insertMarker = "return [";
        var idx = content.IndexOf(insertMarker, StringComparison.Ordinal);
        if (idx < 0)
            return;

        var insertAt = content.IndexOf('\n', idx);
        if (insertAt < 0)
            return;

        var updated = content.Insert(insertAt + 1, $"    {providerEntry},\n");
        await File.WriteAllTextAsync(providersFile, updated, ct);
    }

    private static async Task EnsureSymfonyServiceBindingAsync(string path, string serviceId, string implementationService, CancellationToken ct)
    {
        var servicesFile = Path.Combine(path, "config", "services.yaml");
        if (!File.Exists(servicesFile))
            return;

        var content = await File.ReadAllTextAsync(servicesFile, ct);
        var bindingLine = $"{serviceId}: '@{implementationService}'";
        if (content.Contains(bindingLine, StringComparison.Ordinal))
            return;

        var insertMarker = "services:";
        var idx = content.IndexOf(insertMarker, StringComparison.Ordinal);
        if (idx < 0)
            return;

        var insertAt = content.IndexOf('\n', idx);
        if (insertAt < 0)
            return;

        var updated = content.Insert(insertAt + 1, $"    {bindingLine}\n");
        await File.WriteAllTextAsync(servicesFile, updated, ct);
    }

    private static async Task EnsureNestJsAppModuleRegistrationAsync(string path, CancellationToken ct)
    {
        var appModuleFile = Path.Combine(path, "src", "app.module.js");
        if (!File.Exists(appModuleFile))
            return;

        var content = await File.ReadAllTextAsync(appModuleFile, ct);

        const string moduleRequire = "const { ProjectsModule } = require('./projects/projects.module');";
        if (!content.Contains(moduleRequire, StringComparison.Ordinal))
        {
            var marker = "const { AppService } = require('./app.service');";
            var insertAt = content.IndexOf(marker, StringComparison.Ordinal);
            if (insertAt >= 0)
                content = content.Insert(insertAt + marker.Length + 1, moduleRequire + Environment.NewLine);
        }

        if (content.Contains("imports: [ProjectsModule]", StringComparison.Ordinal))
        {
            await File.WriteAllTextAsync(appModuleFile, content, ct);
            return;
        }

        if (content.Contains("imports: [],", StringComparison.Ordinal))
        {
            content = content.Replace("imports: [],", "imports: [ProjectsModule],", StringComparison.Ordinal);
            await File.WriteAllTextAsync(appModuleFile, content, ct);
            return;
        }

        var importsMarker = "imports: [";
        var start = content.IndexOf(importsMarker, StringComparison.Ordinal);
        if (start >= 0)
        {
            var lineEnd = content.IndexOf('\n', start);
            if (lineEnd > start)
            {
                var updated = content.Insert(lineEnd + 1, "    ProjectsModule,\n");
                await File.WriteAllTextAsync(appModuleFile, updated, ct);
                return;
            }
        }

        await File.WriteAllTextAsync(appModuleFile, content, ct);
    }

    private static async Task EnsureNodeJsCqrsAppAsync(string path, CancellationToken ct)
    {
        var appFile = Path.Combine(path, "src", "app.js");
        await File.WriteAllTextAsync(appFile, """
const { createCqrsRuntime } = require('./cqrs/runtime');
const { CreateProjectCommand } = require('./cqrs/commands/create-project.command');
const { GetProjectQuery } = require('./cqrs/queries/get-project.query');
const { GetProjectsQuery } = require('./cqrs/queries/get-projects.query');

const runtime = createCqrsRuntime();

function readBody(req) {
  return new Promise(resolve => {
    let raw = '';
    req.on('data', chunk => {
      raw += chunk;
    });
    req.on('end', () => {
      if (!raw) {
        resolve({});
        return;
      }

      try {
        resolve(JSON.parse(raw));
      } catch {
        resolve({});
      }
    });
  });
}

function sendJson(res, statusCode, payload) {
  res.writeHead(statusCode, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(payload));
}

function createApp() {
  return async (req, res) => {
    const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);

    if (req.method === 'GET' && url.pathname === '/health') {
      sendJson(res, 200, { status: 'ok', app: 'node-cqrs' });
      return;
    }

    if (req.method === 'GET' && url.pathname === '/projects') {
      const projects = await runtime.queryBus.execute(new GetProjectsQuery());
      sendJson(res, 200, projects);
      return;
    }

    if (req.method === 'GET' && url.pathname.startsWith('/projects/')) {
      const id = Number(url.pathname.split('/')[2]);
      const project = await runtime.queryBus.execute(new GetProjectQuery(id));
      if (!project) {
        sendJson(res, 404, { message: 'Project not found' });
        return;
      }

      sendJson(res, 200, project);
      return;
    }

    if (req.method === 'POST' && url.pathname === '/projects') {
      const body = await readBody(req);
      const project = await runtime.commandBus.execute(new CreateProjectCommand(body));
      sendJson(res, 201, project);
      return;
    }

    sendJson(res, 404, { message: 'Not Found' });
  };
}

module.exports = createApp;
""", ct);
    }

    private static async Task EnsureExpressJsCqrsAppAsync(string path, CancellationToken ct)
    {
        var appFile = Path.Combine(path, "src", "app.js");
        await File.WriteAllTextAsync(appFile, """
const express = require('express');
const { createCqrsRuntime } = require('./cqrs/runtime');
const { CreateProjectCommand } = require('./cqrs/commands/create-project.command');
const { GetProjectQuery } = require('./cqrs/queries/get-project.query');
const { GetProjectsQuery } = require('./cqrs/queries/get-projects.query');

function createApp() {
  const app = express();
  const runtime = createCqrsRuntime();

  app.use(express.json());
  app.get('/health', (_req, res) => res.json({ status: 'ok', app: 'express-cqrs' }));
  app.get('/', (_req, res) => res.json({ app: 'express-cqrs', status: 'running' }));

  app.get('/projects', async (_req, res) => {
    const projects = await runtime.queryBus.execute(new GetProjectsQuery());
    res.json(projects);
  });

  app.get('/projects/:id', async (req, res) => {
    const project = await runtime.queryBus.execute(new GetProjectQuery(Number(req.params.id)));
    if (!project) {
      res.status(404).json({ message: 'Project not found' });
      return;
    }

    res.json(project);
  });

  app.post('/projects', async (req, res) => {
    const project = await runtime.commandBus.execute(new CreateProjectCommand(req.body ?? {}));
    res.status(201).json(project);
  });

  return app;
}

module.exports = createApp();
""", ct);
    }

    private static string ToClassName(string value)
    {
        var chars = value
            .Split(new[] { ' ', '-', '_', '.', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..])
            .ToArray();
        return string.Concat(chars);
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
            ["APP_PORT"] = GetDefaultAppPort(cfg.Architecture, cfg.Framework).ToString()
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

    private static int GetDefaultAppPort(ArchitectureType architecture, FrameworkType framework) =>
        architecture switch
        {
            ArchitectureType.JavaScript or ArchitectureType.TypeScript => 3000,
            ArchitectureType.Php => 8080,
            _ => framework switch
            {
                FrameworkType.NextJs or FrameworkType.NestJs or FrameworkType.NodeJs or FrameworkType.ExpressJs => 3000,
                _ => 8080
            }
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
