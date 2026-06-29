using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using ProjectForge.Application.DTOs;
using ProjectForge.Application.UseCases.Projects;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Web.Controllers;

/// <summary>
/// API v1 — diseñada para ser consumida por la app Flutter de TabBuilder.
/// Autenticación: Bearer token (GitHub access_token del usuario almacenado en BD).
/// Todos los endpoints retornan JSON.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiV1Controller : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiSuggestionService _ai;
    private readonly ICreateProjectUseCase _createProject;
    private readonly IProjectGeneratorService _generator;
    private readonly IProjectRepository _projects;
    private readonly IEncryptionService _encryption;

    public ApiV1Controller(
        AppDbContext db,
        IAiSuggestionService ai,
        ICreateProjectUseCase createProject,
        IProjectGeneratorService generator,
        IProjectRepository projects,
        IEncryptionService encryption)
    {
        _db = db;
        _ai = ai;
        _createProject = createProject;
        _generator = generator;
        _projects = projects;
        _encryption = encryption;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AUTH MIDDLEWARE: resolve user from Bearer token
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;
        var token = authHeader["Bearer ".Length..].Trim();

        // El access_token se almacena encriptado en BD — hay que desencriptar
        // cada registro para comparar. Para no afectar el pool de conexiones,
        // traemos solo los IDs + tokens encriptados (sin cargar toda la entidad).
        var users = await _db.Users
            .Select(u => new { u.Id, u.AccessToken })
            .ToListAsync();

        var matched = users.FirstOrDefault(u =>
        {
            if (string.IsNullOrWhiteSpace(u.AccessToken)) return false;
            try { return _encryption.Decrypt(u.AccessToken) == token; }
            catch { return false; }
        });

        if (matched == null) return null;
        return await _db.Users.FindAsync(matched.Id);
    }

    private IActionResult Unauthorized401(string message = "Token inválido o expirado") =>
        Unauthorized(new ApiError(401, message));

    private static IActionResult NotFound404(string message) =>
        new NotFoundObjectResult(new ApiError(404, message));

    // ─────────────────────────────────────────────────────────────────────────
    // AUTH
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/auth/me — Información del usuario autenticado.</summary>
    [HttpGet("auth/me")]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();
        return Ok(MapUser(user));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WIZARD CATALOG
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/catalog/architectures — Lista de arquitecturas soportadas.</summary>
    [HttpGet("catalog/architectures")]
    public IActionResult GetArchitectures()
    {
        var result = new[]
        {
            new ArchitectureDto("DotNet",     "C# / .NET",   "/img/logos/languages/csharp.png",     "ASP.NET Core, Blazor, Minimal API"),
            new ArchitectureDto("Java",       "Java",        "/img/logos/languages/java.png",        "Spring Boot, Quarkus, Micronaut"),
            new ArchitectureDto("Python",     "Python",      "/img/logos/languages/python.png",      "FastAPI, Django, Flask"),
            new ArchitectureDto("Php",        "PHP",         "/img/logos/languages/php.png",         "Laravel, Symfony"),
            new ArchitectureDto("JavaScript", "JavaScript",  "/img/logos/languages/javascript.png",  "Node.js, Express, NestJS, Next.js"),
            new ArchitectureDto("TypeScript", "TypeScript",  "/img/logos/languages/typescript.png",  "NestJS (TS), Next.js (TS)"),
        };
        return Ok(result);
    }

    /// <summary>GET /api/v1/catalog/frameworks/{architecture} — Frameworks para una arquitectura.</summary>
    [HttpGet("catalog/frameworks/{architecture}")]
    public IActionResult GetFrameworks(string architecture)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiError(400, "Arquitectura inválida"));

        return Ok(GetFrameworkOptions(arch));
    }

    /// <summary>GET /api/v1/catalog/databases — Bases de datos disponibles.</summary>
    [HttpGet("catalog/databases")]
    public IActionResult GetDatabases()
    {
        var dbs = new[]
        {
            new CatalogItemDto("PostgreSQL", "PostgreSQL", "Recomendado", "/img/logos/databases/postgresql.png"),
            new CatalogItemDto("MySQL",      "MySQL",      "Popular",     "/img/logos/databases/mysql.png"),
            new CatalogItemDto("SqlServer",  "SQL Server", "Empresarial", "/img/logos/databases/sqlserver.png"),
            new CatalogItemDto("MongoDB",    "MongoDB",    "NoSQL",       "/img/logos/databases/mongodb.png"),
            new CatalogItemDto("Redis",      "Redis",      "Cache/Cola",  "/img/logos/databases/redis.png"),
            new CatalogItemDto("SQLite",     "SQLite",     "Desarrollo",  "/img/logos/databases/sqlite.png"),
        };
        return Ok(dbs);
    }

    /// <summary>GET /api/v1/catalog/infrastructure — Opciones de infraestructura.</summary>
    [HttpGet("catalog/infrastructure")]
    public IActionResult GetInfrastructure()
    {
        var infra = new[]
        {
            new CatalogItemDto("None",          "Sin contenedores", "Básico",      null),
            new CatalogItemDto("DockerCompose",  "Docker Compose",   "Recomendado", "/img/logos/infra/docker.png"),
            new CatalogItemDto("Kubernetes",     "Kubernetes",       "Avanzado",    "/img/logos/infra/kubernetes.png"),
        };
        return Ok(infra);
    }

    /// <summary>GET /api/v1/catalog/patterns/{architecture}/{framework} — Patrones de diseño.</summary>
    [HttpGet("catalog/patterns/{architecture}/{framework}")]
    public IActionResult GetPatterns(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiError(400, "Arquitectura inválida"));
        if (!Enum.TryParse<FrameworkType>(framework, out var fw))
            return BadRequest(new ApiError(400, "Framework inválido"));

        var patterns = _db.DesignPatterns
            .Where(p => p.Architecture == arch)
            .OrderBy(p => p.Name)
            .ToList()
            .GroupBy(p => NormalizePatternValue(p.Pattern))
            .Select(g => SelectPatternForFramework(g.ToList(), arch, fw))
            .Where(p => p != null)
            .Select(p => new CatalogItemDto(NormalizePatternValue(p!.Pattern), p.Name, null, null))
            .ToList();

        return Ok(patterns);
    }

    /// <summary>GET /api/v1/catalog/libraries/{architecture}/{framework} — Librerías disponibles.</summary>
    [HttpGet("catalog/libraries/{architecture}/{framework}")]
    public IActionResult GetLibraries(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiError(400, "Arquitectura inválida"));
        if (!Enum.TryParse<FrameworkType>(framework, out var fw))
            return BadRequest(new ApiError(400, "Framework inválido"));

        var libraries = _db.Libraries
            .Where(l => l.Architecture == arch && (l.Framework == null || l.Framework == fw))
            .OrderByDescending(l => l.PopularityScore)
            .ThenBy(l => l.Name)
            .ToList()
            .Select(l => new LibraryDto(l.PackageName, l.Name, l.Category, l.Description ?? string.Empty, l.PopularityScore))
            .ToList();

        return Ok(libraries);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AI SUGGESTIONS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>POST /api/v1/ai/suggest — Sugerencias de IA para el stack seleccionado.</summary>
    [HttpPost("ai/suggest")]
    public async Task<IActionResult> Suggest([FromBody] ApiSuggestRequestDto req)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        if (!TryParseArchitecture(req.Architecture, out var arch))
            arch = ArchitectureType.DotNet;
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            fw = FrameworkType.AspNetCoreWebApi;
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            db = DatabaseType.PostgreSQL;
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

        var result = await _ai.SuggestAsync(new WizardSuggestionRequest(
            arch, fw, db, infra, req.AlreadySelectedPatterns ?? Array.Empty<string>()));

        return Ok(new
        {
            suggestedPatterns  = result.SuggestedPatterns,
            suggestedLibraries = result.SuggestedLibraries,
            rationale          = result.Rationale
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PROJECTS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/projects — Lista proyectos del usuario.</summary>
    [HttpGet("projects")]
    public async Task<IActionResult> ListProjects()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var list = await _projects.GetByUserIdAsync(user.Id);
        return Ok(list.Select(MapProject));
    }

    /// <summary>GET /api/v1/projects/{id} — Detalle de un proyecto.</summary>
    [HttpGet("projects/{id:int}")]
    public async Task<IActionResult> GetProject(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetFullAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        return Ok(MapProjectFull(project));
    }

    /// <summary>GET /api/v1/projects/{id}/logs — Logs de generación de un proyecto.</summary>
    [HttpGet("projects/{id:int}/logs")]
    public async Task<IActionResult> GetProjectLogs(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetWithLogsAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        return Ok(project.Logs.OrderBy(l => l.CreatedAt).Select(l => new
        {
            l.Id,
            l.Message,
            l.CreatedAt,
            Level = l.IsError ? "Error" : "Info"
        }));
    }

    /// <summary>POST /api/v1/projects — Crea un nuevo proyecto.</summary>
    [HttpPost("projects")]
    public async Task<IActionResult> CreateProject([FromBody] ApiCreateProjectDto req, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        if (string.IsNullOrWhiteSpace(req.ProjectName) || req.ProjectName.Length < 2)
            return BadRequest(new ApiError(400, "El nombre del proyecto es obligatorio (mínimo 2 caracteres)"));

        if (!TryParseArchitecture(req.Architecture, out var arch))
            return BadRequest(new ApiError(400, "Arquitectura inválida"));
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            return BadRequest(new ApiError(400, "Framework inválido"));
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            return BadRequest(new ApiError(400, "Base de datos inválida"));
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

        var patterns = (req.Patterns ?? new List<string>()).Take(1).ToList();
        var libs = req.Libraries ?? new List<string>();

        var config = new WizardConfig
        {
            Architecture       = arch,
            Framework          = fw,
            FrameworkVersion   = req.FrameworkVersion ?? "latest",
            Database           = db,
            Infrastructure     = infra,
            DeploymentTarget   = DeploymentTarget.Local,
            DesignPatternsJson = JsonSerializer.Serialize(patterns),
            LibrariesJson      = JsonSerializer.Serialize(libs),
            AdditionalOptionsJson = JsonSerializer.Serialize(new { createPrivateRepo = req.CreatePrivateRepo }),
            CreatedAt          = DateTime.UtcNow,
        };

        _db.WizardConfigs.Add(config);
        await _db.SaveChangesAsync(ct);

        var project = await _createProject.ExecuteAsync(new CreateProjectRequest(
            user.Id,
            config.Id,
            req.ProjectName.Trim(),
            req.Description?.Trim()), ct);

        return Created($"/api/v1/projects/{project.Id}", MapProject(project));
    }

    /// <summary>POST /api/v1/projects/{id}/generate — Inicia la generación de un proyecto.</summary>
    [HttpPost("projects/{id:int}/generate")]
    public async Task<IActionResult> StartGeneration(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetFullAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        // Capturar IServiceProvider ANTES del Task.Run — HttpContext puede liberarse
        // después de que el response salga (el request ya terminó).
        var services = HttpContext.RequestServices;

        _ = Task.Run(async () =>
        {
            using var scope = services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<IProjectGeneratorService>();
            await generator.GenerateAsync(id);
        });

        return Accepted(new { message = "Generación iniciada. Usa SignalR o polling en /api/v1/projects/{id} para seguir el estado.", projectId = id });
    }

    /// <summary>DELETE /api/v1/projects/{id} — Elimina un proyecto.</summary>
    [HttpDelete("projects/{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetFullAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        await _projects.DeleteAsync(id);
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private static object MapUser(ApplicationUser user) => new
    {
        user.Id,
        user.Username,
        user.Email,
        user.AvatarUrl,
        user.GitHubId,
        user.CreatedAt
    };

    private static object MapProject(Project p) => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.Status,
        StatusLabel = p.Status.ToString(),
        p.RepositoryUrl,
        p.LocalPath,
        p.ErrorMessage,
        p.CreatedAt,
        p.UpdatedAt,
        Architecture = p.WizardConfig?.Architecture.ToString(),
        Framework    = p.WizardConfig?.Framework.ToString(),
        Database     = p.WizardConfig?.Database.ToString(),
        Infrastructure = p.WizardConfig?.Infrastructure.ToString(),
    };

    private static object MapProjectFull(Project p) => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.Status,
        StatusLabel = p.Status.ToString(),
        p.RepositoryUrl,
        p.LocalPath,
        p.ErrorMessage,
        p.GeneratedReadme,
        p.CreatedAt,
        p.UpdatedAt,
        Config = p.WizardConfig == null ? null : new
        {
            p.WizardConfig.Architecture,
            p.WizardConfig.Framework,
            p.WizardConfig.FrameworkVersion,
            p.WizardConfig.Database,
            p.WizardConfig.Infrastructure,
            p.WizardConfig.DeploymentTarget,
            DesignPatterns = JsonSerializer.Deserialize<List<string>>(p.WizardConfig.DesignPatternsJson ?? "[]") ?? new(),
            Libraries      = JsonSerializer.Deserialize<List<string>>(p.WizardConfig.LibrariesJson ?? "[]") ?? new(),
        },
        LogCount = p.Logs?.Count ?? 0
    };

    private static List<object> GetFrameworkOptions(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => new()
        {
            fw("AspNetCoreWebApi", "ASP.NET Core Web API", "/img/logos/frameworks/aspdotnet.png", new[]{"10.0","8.0","7.0"}),
            fw("AspNetCoreMVC",    "ASP.NET Core MVC",     "/img/logos/frameworks/aspdotnet.png", new[]{"10.0","8.0"}),
            fw("BlazorServer",     "Blazor Server",        "/img/logos/frameworks/aspdotnet.png", new[]{"10.0","8.0"}),
            fw("BlazorWasm",       "Blazor WebAssembly",   "/img/logos/frameworks/aspdotnet.png", new[]{"10.0","8.0"}),
            fw("MinimalApi",       "Minimal API",          "/img/logos/frameworks/aspdotnet.png", new[]{"10.0","8.0"}),
        },
        ArchitectureType.Java => new()
        {
            fw("SpringBoot", "Spring Boot", "/img/logos/frameworks/springboot.png", new[]{"3.3","3.2","2.7"}),
            fw("Quarkus",    "Quarkus",     "/img/logos/frameworks/quarkus.png",    new[]{"3.x"}),
            fw("Micronaut",  "Micronaut",   "/img/logos/frameworks/micronaut.png",  new[]{"4.x"}),
        },
        ArchitectureType.Python => new()
        {
            fw("FastAPI", "FastAPI", "/img/logos/frameworks/fastapi.png", new[]{"0.115","0.110"}),
            fw("Django",  "Django",  "/img/logos/frameworks/django.png",  new[]{"5.0","4.2"}),
            fw("Flask",   "Flask",   "/img/logos/frameworks/flask.png",   new[]{"3.0","2.3"}),
        },
        ArchitectureType.Php => new()
        {
            fw("Laravel",  "Laravel",  "/img/logos/frameworks/laravel.png",  new[]{"11.x","10.x"}),
            fw("Symfony",  "Symfony",  "/img/logos/frameworks/symfony.png",  new[]{"7.x","6.x"}),
        },
        ArchitectureType.JavaScript => new()
        {
            fw("NodeJs",    "Node.js",    "/img/logos/frameworks/nodejs.png",    new[]{"22.x","20.x"}),
            fw("ExpressJs", "Express.js", "/img/logos/frameworks/expressjs.png", new[]{"5.x","4.x"}),
            fw("NestJs",    "NestJS",     "/img/logos/frameworks/nestjs.png",    new[]{"10.x"}),
            fw("NextJs",    "Next.js",    "/img/logos/frameworks/nextjs.png",    new[]{"14.x"}),
        },
        ArchitectureType.TypeScript => new()
        {
            fw("NestTs", "NestJS (TypeScript)", "/img/logos/frameworks/nestts.png", new[]{"10.x"}),
            fw("NextTs", "Next.js (TypeScript)", "/img/logos/frameworks/nextts.png", new[]{"14.x"}),
        },
        _ => new()
    };

    private static object fw(string value, string label, string logo, string[] versions) =>
        new { value, label, logo, versions };

    private static string NormalizePatternValue(DesignPattern pattern) => pattern switch
    {
        DesignPattern.DomainDrivenDesign    => "DomainDrivenDesign",
        DesignPattern.CleanArchitecture     => "CleanArchitecture",
        DesignPattern.HexagonalArchitecture => "HexagonalArchitecture",
        DesignPattern.EventSourcing         => "EventSourcing",
        DesignPattern.Microservices         => "Microservices",
        DesignPattern.CQRS                  => "CQRS",
        DesignPattern.Mediator              => "Mediator",
        DesignPattern.Saga                  => "Saga",
        DesignPattern.Repository            => "Repository",
        _ => pattern.ToString()
    };

    private static DesignPatternEntry? SelectPatternForFramework(
        IReadOnlyList<DesignPatternEntry> patterns,
        ArchitectureType arch,
        FrameworkType framework)
    {
        if (patterns.Count == 0) return null;
        if (arch == ArchitectureType.Php)
        {
            return framework == FrameworkType.Symfony
                ? patterns.FirstOrDefault(p => p.Name.Contains("Symfony", StringComparison.OrdinalIgnoreCase))
                : patterns.FirstOrDefault(p => !p.Name.Contains("Symfony", StringComparison.OrdinalIgnoreCase))
                  ?? patterns.First();
        }
        if (framework is FrameworkType.NestJs or FrameworkType.NestTs)
        {
            return patterns.FirstOrDefault(p =>
                p.Name.Contains("NestJS", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
                ?? patterns.First();
        }
        return patterns.FirstOrDefault(p =>
            !p.Name.Contains("NestJS", StringComparison.OrdinalIgnoreCase) &&
            !p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
            ?? patterns.First();
    }

    private static bool TryParseArchitecture(string? value, out ArchitectureType architecture)
    {
        if (Enum.TryParse(value, ignoreCase: true, out architecture)) return true;
        if (string.Equals(value, "PHP", StringComparison.OrdinalIgnoreCase))
        { architecture = ArchitectureType.Php; return true; }
        architecture = ArchitectureType.DotNet;
        return false;
    }

    // ─── Tool Prerequisites Check ─────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/tools/check-prerequisites?architecture=Java
    /// Detecta si las herramientas necesarias están instaladas en el servidor/máquina.
    /// No requiere autenticación. Seguro de llamar antes de generar el proyecto.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("tools/check-prerequisites")]
    public async Task<IActionResult> CheckPrerequisites([FromQuery] string? architecture)
    {
        var requirements = GetToolRequirements(architecture);
        var results = new List<ToolCheckResultDto>();

        foreach (var req in requirements)
        {
            var installed = await IsToolInstalledAsync(req.Command);
            results.Add(new ToolCheckResultDto(req.Name, req.Command, installed, req.InstallUrl, req.Description));
        }

        return Ok(new
        {
            allInstalled = results.All(r => r.Installed),
            architecture = architecture ?? "any",
            tools = results
        });
    }

    private static async Task<bool> IsToolInstalledAsync(string command)
    {
        try
        {
            var isWindows = OperatingSystem.IsWindows();
            using var proc = new System.Diagnostics.Process();
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName    = isWindows ? "cmd.exe" : "/bin/bash",
                Arguments   = isWindows ? $"/c where {command}" : $"-c \"which {command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute  = false,
                CreateNoWindow   = true
            };
            proc.Start();
            await proc.WaitForExitAsync();
            return proc.ExitCode == 0;
        }
        catch { return false; }
    }

    private static IEnumerable<ToolRequirementDto> GetToolRequirements(string? architecture)
    {
        // Herramientas universales siempre requeridas
        var tools = new List<ToolRequirementDto>
        {
            new("Git",    "git",    "https://git-scm.com/downloads",        "Control de versiones (requerido para push a GitHub)"),
            new("Docker", "docker", "https://docs.docker.com/get-docker/",  "Contenedores — necesario para Dockerfile y Docker Compose"),
        };

        switch (architecture?.ToLowerInvariant())
        {
            case "dotnet":
                tools.Add(new(".NET SDK",  "dotnet", "https://dotnet.microsoft.com/download", "SDK de .NET para compilar y ejecutar la aplicación"));
                break;
            case "java":
                tools.Add(new("Java JDK 21+", "java", "https://adoptium.net/",                       "JDK para compilar y ejecutar aplicaciones Java"));
                tools.Add(new("Maven (mvn)",   "mvn",  "https://maven.apache.org/download.cgi",        "Gestor de dependencias y build para proyectos Java"));
                break;
            case "python":
                tools.Add(new("Python 3.10+", "python3", "https://www.python.org/downloads/",           "Intérprete de Python"));
                tools.Add(new("pip3",          "pip3",    "https://pip.pypa.io/en/stable/installation/", "Gestor de paquetes de Python"));
                break;
            case "php":
                tools.Add(new("PHP 8.2+",  "php",      "https://www.php.net/downloads",           "Intérprete de PHP"));
                tools.Add(new("Composer",  "composer", "https://getcomposer.org/download/",        "Gestor de dependencias para PHP (Laravel, Symfony)"));
                break;
            case "javascript":
            case "typescript":
                tools.Add(new("Node.js 20+", "node", "https://nodejs.org/en/download/", "Runtime de Node.js"));
                tools.Add(new("npm",          "npm",  "https://nodejs.org/en/download/", "Gestor de paquetes de Node.js"));
                break;
        }

        return tools;
    }
}

// ─── DTOs for Flutter API ─────────────────────────────────────────────────────

public sealed record ApiError(int StatusCode, string Message);

public sealed record ArchitectureDto(
    string Value,
    string Label,
    string? LogoUrl,
    string Description
);

public sealed record CatalogItemDto(
    string Value,
    string Label,
    string? Badge,
    string? LogoUrl
);

public sealed record LibraryDto(
    string PackageName,
    string Name,
    string? Category,
    string Description,
    int PopularityScore
);

public sealed class ApiSuggestRequestDto
{
    public string Architecture { get; set; } = "";
    public string Framework    { get; set; } = "";
    public string Database     { get; set; } = "";
    public string Infrastructure { get; set; } = "";
    public IEnumerable<string>? AlreadySelectedPatterns { get; set; }
}

public sealed class ApiCreateProjectDto
{
    public string ProjectName      { get; set; } = "";
    public string? Description     { get; set; }
    public string Architecture     { get; set; } = "";
    public string Framework        { get; set; } = "";
    public string? FrameworkVersion { get; set; }
    public string Database         { get; set; } = "";
    public string Infrastructure   { get; set; } = "None";
    public List<string>? Patterns  { get; set; }
    public List<string>? Libraries { get; set; }
    public bool CreatePrivateRepo  { get; set; }
}

/// <summary>Herramienta requerida por arquitectura (uso interno del controller).</summary>
internal sealed record ToolRequirementDto(
    string Name,
    string Command,
    string InstallUrl,
    string Description);

/// <summary>Resultado del check de una herramienta (devuelto al cliente).</summary>
public sealed record ToolCheckResultDto(
    string Name,
    string Command,
    bool   Installed,
    string InstallUrl,
    string Description);
