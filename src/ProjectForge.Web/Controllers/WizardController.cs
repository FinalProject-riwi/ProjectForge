using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using ProjectForge.Application.DTOs;
using ProjectForge.Application.Services;
using ProjectForge.Application.UseCases.Projects;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Web.Controllers;

[Authorize]
[Route("wizard")]
public class WizardController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAiSuggestionService _ai;
    private readonly ICreateProjectUseCase _createProject;
    private readonly IProjectGeneratorService _generator;

    public WizardController(
        AppDbContext db, IAiSuggestionService ai,
        ICreateProjectUseCase createProject,
        IProjectGeneratorService generator)
    {
        _db = db; _ai = ai; _createProject = createProject;
        _generator = generator;
    }

    // ── GET /wizard ────────────────────────────────────────────────────────────
    [HttpGet("")]
    public IActionResult Index() => View();

    // ── Step 5: recibe todo el estado del wizard desde el JS ──────────────────
    [HttpPost("step5")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step5Post(
        [FromForm] string projectName,
        [FromForm] string? description,
        [FromForm] bool createPrivateRepo,
        [FromForm(Name = "__arch")]     string? arch,
        [FromForm(Name = "__fw")]       string? fw,
        [FromForm(Name = "__fwv")]      string? fwv,
        [FromForm(Name = "__db")]       string? db,
        [FromForm(Name = "__db2")]      string? db2,
        [FromForm(Name = "__infra")]    string? infra,
        [FromForm(Name = "__patterns")] string? patternsJson,
        [FromForm(Name = "__libs")]     string? libsJson)
    {
        if (string.IsNullOrWhiteSpace(projectName) || projectName.Length < 2)
        {
            TempData["Error"] = "El nombre del proyecto es obligatorio (mínimo 2 caracteres).";
            return RedirectToAction("Index");
        }

        // Sanitizar el nombre para que sea válido como nombre de carpeta y repo Git.
        // Reemplaza cualquier carácter que no sea letra, dígito o guión.
        var safeProjectName = System.Text.RegularExpressions.Regex.Replace(
            projectName.Trim(), @"[^\w\-]", "-");
        if (safeProjectName.Length < 2)
        {
            TempData["Error"] = "El nombre del proyecto contiene solo caracteres inválidos. Usa letras, números o guiones.";
            return RedirectToAction("Index");
        }
        // Usar el nombre sanitizado de aquí en adelante
        projectName = safeProjectName;

        // Validar que el userId del claim existe en BD.
        // Si la cookie se firmó con claves Data Protection rotadas (ej: reinicio de contenedor
        // sin volumen persistente), el claim puede traer un ID que ya no existe → FK violation.
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Error"] = "Sesión inválida. Por favor vuelve a iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Error"] = "Tu sesión expiró o fue invalidada. Por favor vuelve a iniciar sesión con GitHub.";
            return RedirectToAction("Login", "Auth");
        }

        // Parsear enums con fallback seguro
        var architecture     = TryParseArchitecture(arch, out var a)                   ? a : ArchitectureType.DotNet;
        var framework        = Enum.TryParse<FrameworkType>(fw, out var f)              ? f : FrameworkType.AspNetCoreWebApi;
        var database         = Enum.TryParse<DatabaseType>(db, out var d)               ? d : DatabaseType.PostgreSQL;
        var infrastructure   = Enum.TryParse<InfrastructureType>(infra, out var i)      ? i : InfrastructureType.None;

        var patterns = (TryParseJson<List<string>>(patternsJson) ?? new List<string>())
            .Take(1)
            .ToList();
        var libs     = TryParseJson<List<string>>(libsJson)     ?? new List<string>();

        // Secondary DB (optional — stored in AdditionalOptionsJson)
        var secondaryDatabase = Enum.TryParse<DatabaseType>(db2, out var sd) && sd != database
            ? (DatabaseType?)sd
            : null;

        // Validaciones async para evitar bloquear el pool de conexiones
        // (GetPatternOptions y GetLibraryOptions consultan la BD)
        var patternOptions = await GetPatternOptionsAsync(architecture, framework);
        var libraryOptions = await GetLibraryOptionsAsync(architecture, framework);

        if (!IsValidArchitecture(architecture) ||
            !IsValidFramework(architecture, framework) ||
            !IsValidDatabase(architecture, framework, database) ||
            !IsValidInfrastructure(architecture, framework, database, infrastructure) ||
            patterns.Any(p => !patternOptions.Any(o => o.Value.Equals(NormalizePatternValue(p), StringComparison.OrdinalIgnoreCase))) ||
            libs.Any(l => !libraryOptions.Any(o => o.Value.Equals(l, StringComparison.OrdinalIgnoreCase))))
        {
            TempData["Error"] = "La configuración seleccionada ya no es válida. Recarga el wizard y selecciona una opción disponible.";
            return RedirectToAction("Index");
        }

        // Compatibility validation — block on hard errors only; warnings are non-blocking
        var compatIssues = WizardCompatibilityValidator.Validate(architecture, framework, database, infrastructure, patterns);
        var compatErrors = compatIssues.Where(i => i.IsError).ToList();
        if (compatErrors.Count > 0)
        {
            TempData["Error"] = string.Join(" | ", compatErrors.Select(e => e.Message));
            return RedirectToAction("Index");
        }

        var config = new WizardConfig
        {
            Architecture       = architecture,
            Framework          = framework,
            // "latest" is not a real Maven/npm/pip version string — for Spring Boot in particular
            // it gets written verbatim into pom.xml's <parent><version>, which Maven Central
            // rejects outright ("Non-resolvable parent POM ... spring-boot-starter-parent:pom:latest").
            FrameworkVersion   = string.IsNullOrWhiteSpace(fwv) ? GetDefaultFrameworkVersion(framework) : fwv,
            Database           = database,
            Infrastructure     = infrastructure,
            DeploymentTarget   = DeploymentTarget.Local,
            DesignPatternsJson = JsonSerializer.Serialize(patterns),
            LibrariesJson      = JsonSerializer.Serialize(libs),
            AdditionalOptionsJson = JsonSerializer.Serialize(new
            {
                createPrivateRepo,
                secondaryDatabase = secondaryDatabase?.ToString()
            }),
            CreatedAt          = DateTime.UtcNow,
        };

        _db.WizardConfigs.Add(config);
        await _db.SaveChangesAsync();

        var project = await _createProject.ExecuteAsync(new CreateProjectRequest(
            userId,
            config.Id,
            projectName.Trim(),
            description?.Trim()), HttpContext.RequestAborted);

        return RedirectToAction("Generate", new { id = project.Id });
    }

    // ── Generación ─────────────────────────────────────────────────────────────
    [HttpGet("generate/{id:int}")]
    public IActionResult Generate(int id)
    {
        ViewBag.ProjectId = id;
        return View();
    }

    [HttpPost("generate/{id:int}/start")]
    [IgnoreAntiforgeryToken]
    public IActionResult StartGeneration(int id)
    {
        // Capturar IServiceProvider ANTES de entrar al Task.Run.
        // HttpContext puede haber sido liberado cuando el task ejecute
        // (el request ya terminó), causando ObjectDisposedException.
        var services = HttpContext.RequestServices;

        _ = Task.Run(async () =>
        {
            using var scope = services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<IProjectGeneratorService>();
            await generator.GenerateAsync(id);
        });

        return Json(new { queued = true });
    }

    // ── API para el wizard frontend ────────────────────────────────────────────
    [HttpGet("api/frameworks/{architecture}")]
    public IActionResult GetFrameworks(string architecture)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return Ok(Array.Empty<object>());

        return Ok(GetFrameworkOptions(arch));
    }

    [HttpGet("api/databases/{architecture}/{framework}")]
    public IActionResult GetDatabases(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch) || !Enum.TryParse<FrameworkType>(framework, out var fw))
            return Ok(Array.Empty<object>());

        return Ok(GetDatabaseOptions(arch, fw));
    }

    [HttpGet("api/infrastructure/{architecture}/{framework}/{database}")]
    public IActionResult GetInfrastructure(string architecture, string framework, string database)
    {
        if (!TryParseArchitecture(architecture, out var arch) ||
            !Enum.TryParse<FrameworkType>(framework, out var fw) ||
            !Enum.TryParse<DatabaseType>(database, out var db))
        {
            return Ok(Array.Empty<object>());
        }

        return Ok(GetInfrastructureOptions(arch, fw, db));
    }

    [HttpGet("api/patterns/{architecture}/{framework}")]
    public IActionResult GetPatterns(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch) || !Enum.TryParse<FrameworkType>(framework, out var fw))
            return Ok(Array.Empty<object>());

        return Ok(GetPatternOptions(arch, fw));
    }

    [HttpGet("api/libraries/{architecture}/{framework}")]
    public IActionResult GetLibraries(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch) || !Enum.TryParse<FrameworkType>(framework, out var fw))
            return Ok(Array.Empty<object>());

        return Ok(GetLibraryOptions(arch, fw));
    }

    // ── Validate endpoint (real-time compatibility check) ──────────────────────
    [HttpPost("api/validate")]
    [IgnoreAntiforgeryToken]
    public IActionResult ValidateConfig([FromBody] ValidateRequestDto req)
    {
        if (!TryParseArchitecture(req.Architecture, out var arch))
            return BadRequest(new { error = "Invalid architecture" });
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            return BadRequest(new { error = "Invalid framework" });
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            return BadRequest(new { error = "Invalid database" });
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

        var issues = WizardCompatibilityValidator.Validate(arch, fw, db, infra, req.Patterns ?? Array.Empty<string>());
        return Ok(new
        {
            hasErrors = issues.Any(i => i.IsError),
            issues    = issues.Select(i => new { i.Code, i.Message, i.IsError })
        });
    }

    // ── Secondary DB options (exclude primary) ─────────────────────────────────
    [HttpGet("api/databases/{architecture}/{framework}/secondary")]
    public IActionResult GetSecondaryDatabases(string architecture, string framework, [FromQuery] string? primaryDb)
    {
        if (!TryParseArchitecture(architecture, out var arch) || !Enum.TryParse<FrameworkType>(framework, out var fw))
            return Ok(Array.Empty<object>());

        Enum.TryParse<DatabaseType>(primaryDb, out var primary);

        var options = GetDatabaseOptions(arch, fw)
            .Where(o => !string.Equals(o.Value, primaryDb, StringComparison.OrdinalIgnoreCase))
            .Select(o => o.Value switch
            {
                "Redis"   => o with { Badge = "Cache recomendado" },
                "MongoDB" => o with { Badge = "NoSQL secundario" },
                _         => o
            })
            .ToList();

        return Ok(options);
    }

    // ── File-tree preview (no I/O, returns expected paths) ────────────────────
    [HttpPost("api/preview")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetPreview([FromBody] PreviewRequestDto req)
    {
        if (!TryParseArchitecture(req.Architecture, out var arch))
            return BadRequest(new { error = "Invalid architecture" });
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            return BadRequest(new { error = "Invalid framework" });
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            return BadRequest(new { error = "Invalid database" });
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

        var projectName = string.IsNullOrWhiteSpace(req.ProjectName) ? "my-project" : req.ProjectName;
        var files = _generator.PreviewFiles(arch, fw, db, infra, req.Patterns ?? Array.Empty<string>(), projectName);
        return Ok(new { projectName, files });
    }

    [HttpPost("api/suggest")]
    public async Task<IActionResult> GetSuggestions([FromBody] WizardSuggestionRequestDto req)
    {
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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static T? TryParseJson<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json); }
        catch { return default; }
    }

    private static List<OptionDto> GetArchitectureOptions() => new()
    {
        new OptionDto(ArchitectureType.DotNet.ToString(), GetArchitectureLabel(ArchitectureType.DotNet)),
        new OptionDto(ArchitectureType.Java.ToString(), GetArchitectureLabel(ArchitectureType.Java)),
        new OptionDto(ArchitectureType.Python.ToString(), GetArchitectureLabel(ArchitectureType.Python)),
        new OptionDto(ArchitectureType.Php.ToString(), GetArchitectureLabel(ArchitectureType.Php)),
        new OptionDto(ArchitectureType.JavaScript.ToString(), GetArchitectureLabel(ArchitectureType.JavaScript)),
        new OptionDto(ArchitectureType.TypeScript.ToString(), GetArchitectureLabel(ArchitectureType.TypeScript)),
    };

    private static List<FrameworkOptionDto> GetFrameworkOptions(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => new()
        {
            new() { Value = "AspNetCoreWebApi", Label = "ASP.NET Core Web API", AvailableVersions = ["10.0", "8.0", "7.0"] },
            new() { Value = "AspNetCoreMVC", Label = "ASP.NET Core MVC", AvailableVersions = ["10.0", "8.0"] },
            new() { Value = "BlazorServer", Label = "Blazor Server", AvailableVersions = ["10.0", "8.0"] },
            new() { Value = "BlazorWasm", Label = "Blazor WebAssembly", AvailableVersions = ["10.0", "8.0"] },
            new() { Value = "MinimalApi", Label = "Minimal API", AvailableVersions = ["10.0", "8.0"] },
        },
        ArchitectureType.Java => new()
        {
            new() { Value = "SpringBoot", Label = "Spring Boot", AvailableVersions = ["3.3", "3.2", "2.7"] },
            new() { Value = "Quarkus", Label = "Quarkus", AvailableVersions = ["3.x"] },
            new() { Value = "Micronaut", Label = "Micronaut", AvailableVersions = ["4.x"] },
        },
        ArchitectureType.Python => new()
        {
            new() { Value = "FastAPI", Label = "FastAPI", AvailableVersions = ["0.115", "0.110"] },
            new() { Value = "Django", Label = "Django", AvailableVersions = ["5.0", "4.2"] },
            new() { Value = "Flask", Label = "Flask", AvailableVersions = ["3.0", "2.3"] },
        },
        ArchitectureType.JavaScript => new()
        {
            new() { Value = "NodeJs", Label = "Node.js", AvailableVersions = ["22.x", "20.x"] },
            new() { Value = "ExpressJs", Label = "Express.js", AvailableVersions = ["5.x", "4.x"] },
            new() { Value = "NestJs", Label = "NestJS", AvailableVersions = ["10.x"] },
            new() { Value = "NextJs", Label = "Next.js", AvailableVersions = ["14.x"] },
        },
        ArchitectureType.TypeScript => new()
        {
            new() { Value = "NestTs", Label = "NestJS (TypeScript)", AvailableVersions = ["10.x"] },
            new() { Value = "NextTs", Label = "Next.js (TypeScript)", AvailableVersions = ["14.x"] },
        },
        ArchitectureType.Php => new()
        {
            new() { Value = "Laravel", Label = "Laravel", AvailableVersions = ["11.x", "10.x"] },
            new() { Value = "Symfony", Label = "Symfony", AvailableVersions = ["7.x", "6.x"] },
        },
        _ => new()
    };

    private static List<OptionDto> GetDatabaseOptions(ArchitectureType arch, FrameworkType framework) => new()
    {
        new OptionDto(DatabaseType.PostgreSQL.ToString(), "PostgreSQL", "Recomendado"),
        new OptionDto(DatabaseType.MySQL.ToString(), "MySQL", "Popular"),
        new OptionDto(DatabaseType.SqlServer.ToString(), "SQL Server", "Empresarial"),
        new OptionDto(DatabaseType.MongoDB.ToString(), "MongoDB", "NoSQL"),
        new OptionDto(DatabaseType.Redis.ToString(), "Redis", "Cache/Cola"),
        new OptionDto(DatabaseType.SQLite.ToString(), "SQLite", "Desarrollo"),
    };

    private static List<OptionDto> GetInfrastructureOptions(ArchitectureType arch, FrameworkType framework, DatabaseType db) => new()
    {
        new OptionDto(InfrastructureType.None.ToString(), "Sin contenedores", null, "Solo el código del proyecto"),
        new OptionDto(InfrastructureType.DockerCompose.ToString(), "Docker Compose", null, "Ideal para desarrollo local y un solo servidor"),
        new OptionDto(InfrastructureType.Kubernetes.ToString(), "Kubernetes", null, "Escalado horizontal, múltiples VPS"),
    };

    private List<OptionDto> GetPatternOptions(ArchitectureType arch, FrameworkType framework)
    {
        var patterns = _db.DesignPatterns
            .Where(p => p.Architecture == arch)
            .OrderBy(p => p.Name)
            .ToList();

        return patterns
            .GroupBy(p => NormalizePatternValue(p.Pattern))
            .Select(g => SelectPatternForFramework(g.ToList(), arch, framework))
            .Where(p => p != null)
            .Select(p => p!)
            .Select(p => new OptionDto(NormalizePatternValue(p.Pattern), p.Name))
            .ToList();
    }

    // Versión async para validaciones en Step5Post (evita bloquear el pool de conexiones)
    private async Task<List<OptionDto>> GetPatternOptionsAsync(ArchitectureType arch, FrameworkType framework)
    {
        var patterns = await _db.DesignPatterns
            .Where(p => p.Architecture == arch)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return patterns
            .GroupBy(p => NormalizePatternValue(p.Pattern))
            .Select(g => SelectPatternForFramework(g.ToList(), arch, framework))
            .Where(p => p != null)
            .Select(p => p!)
            .Select(p => new OptionDto(NormalizePatternValue(p.Pattern), p.Name))
            .ToList();
    }

    private static DesignPatternEntry? SelectPatternForFramework(
        IReadOnlyList<DesignPatternEntry> patterns,
        ArchitectureType arch,
        FrameworkType framework)
    {
        if (patterns.Count == 0)
            return null;

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
                p.Name.Contains("NestJs", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
                ?? patterns.First();
        }

        return patterns.FirstOrDefault(p =>
                !p.Name.Contains("NestJS", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Contains("NestJs", StringComparison.OrdinalIgnoreCase) &&
                !p.Name.Contains("Nest", StringComparison.OrdinalIgnoreCase))
            ?? patterns.First();
    }

    private List<OptionDto> GetLibraryOptions(ArchitectureType arch, FrameworkType framework)
    {
        var libraries = _db.Libraries
            .Where(l => l.Architecture == arch && (l.Framework == null || l.Framework == framework))
            .OrderByDescending(l => l.PopularityScore)
            .ThenBy(l => l.Name)
            .ToList();

        return libraries
            .Select(l => new OptionDto(l.PackageName, l.Name, l.Category))
            .ToList();
    }

    // Versión async para validaciones en Step5Post
    private async Task<List<OptionDto>> GetLibraryOptionsAsync(ArchitectureType arch, FrameworkType framework)
    {
        var libraries = await _db.Libraries
            .Where(l => l.Architecture == arch && (l.Framework == null || l.Framework == framework))
            .OrderByDescending(l => l.PopularityScore)
            .ThenBy(l => l.Name)
            .ToListAsync();

        return libraries
            .Select(l => new OptionDto(l.PackageName, l.Name, l.Category))
            .ToList();
    }

    private bool IsValidArchitecture(ArchitectureType architecture) =>
        GetArchitectureOptions().Any(o => string.Equals(o.Value, architecture.ToString(), StringComparison.OrdinalIgnoreCase));

    private bool IsValidFramework(ArchitectureType architecture, FrameworkType framework) =>
        GetFrameworkOptions(architecture).Any(o => string.Equals(o.Value, framework.ToString(), StringComparison.OrdinalIgnoreCase));

    private bool IsValidDatabase(ArchitectureType architecture, FrameworkType framework, DatabaseType database) =>
        GetDatabaseOptions(architecture, framework).Any(o => string.Equals(o.Value, database.ToString(), StringComparison.OrdinalIgnoreCase));

    private bool IsValidInfrastructure(ArchitectureType architecture, FrameworkType framework, DatabaseType database, InfrastructureType infrastructure) =>
        GetInfrastructureOptions(architecture, framework, database).Any(o => string.Equals(o.Value, infrastructure.ToString(), StringComparison.OrdinalIgnoreCase));

    private bool IsValidPattern(ArchitectureType architecture, FrameworkType framework, string pattern)
    {
        var normalized = NormalizePatternValue(pattern);
        return GetPatternOptions(architecture, framework).Any(o => string.Equals(o.Value, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidLibrary(ArchitectureType architecture, FrameworkType framework, string library) =>
        GetLibraryOptions(architecture, framework).Any(o => string.Equals(o.Value, library, StringComparison.OrdinalIgnoreCase));

    private static string NormalizePatternValue(DesignPattern pattern) => pattern switch
    {
        DesignPattern.DomainDrivenDesign => "DomainDrivenDesign",
        DesignPattern.CleanArchitecture => "CleanArchitecture",
        DesignPattern.HexagonalArchitecture => "HexagonalArchitecture",
        DesignPattern.EventSourcing => "EventSourcing",
        DesignPattern.Microservices => "Microservices",
        DesignPattern.CQRS => "CQRS",
        DesignPattern.Mediator => "Mediator",
        DesignPattern.Saga => "Saga",
        DesignPattern.Repository => "Repository",
        _ => pattern.ToString()
    };

    private static string NormalizePatternValue(string pattern) =>
        new string((pattern ?? string.Empty)
            .Trim()
            .Where(char.IsLetterOrDigit)
            .ToArray());

    private static string GetArchitectureLabel(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => "C# / .NET",
        ArchitectureType.Java => "Java",
        ArchitectureType.Python => "Python",
        ArchitectureType.Php => "PHP",
        ArchitectureType.JavaScript => "JavaScript",
        ArchitectureType.TypeScript => "TypeScript",
        _ => arch.ToString()
    };

    private static bool TryParseArchitecture(string? value, out ArchitectureType architecture)
    {
        if (Enum.TryParse(value, ignoreCase: true, out architecture))
            return true;

        if (string.Equals(value, "Laravel", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "PHP", StringComparison.OrdinalIgnoreCase))
        {
            architecture = ArchitectureType.Php;
            return true;
        }

        architecture = ArchitectureType.DotNet;
        return false;
    }

    private static string GetDefaultFrameworkVersion(FrameworkType fw) => fw switch
    {
        FrameworkType.AspNetCoreWebApi => "10.0",
        FrameworkType.AspNetCoreMVC    => "10.0",
        FrameworkType.BlazorServer     => "10.0",
        FrameworkType.BlazorWasm       => "10.0",
        FrameworkType.MinimalApi       => "10.0",
        FrameworkType.SpringBoot       => "3.3",
        FrameworkType.Quarkus          => "3.x",
        FrameworkType.Micronaut        => "4.x",
        FrameworkType.FastAPI          => "0.115",
        FrameworkType.Django           => "5.0",
        FrameworkType.Flask            => "3.0",
        FrameworkType.Laravel          => "11.x",
        FrameworkType.Symfony          => "7.x",
        FrameworkType.NodeJs           => "22.x",
        FrameworkType.ExpressJs        => "5.x",
        FrameworkType.NestJs           => "10.x",
        FrameworkType.NextJs           => "14.x",
        FrameworkType.NestTs           => "10.x",
        FrameworkType.NextTs           => "14.x",
        _                              => "latest"
    };

    private sealed record OptionDto(string Value, string Label, string? Badge = null, string? Description = null);
}

// DTO para el endpoint de sugerencias (recibe strings del JS)
public class WizardSuggestionRequestDto
{
    public string Architecture { get; set; } = "";
    public string Framework    { get; set; } = "";
    public string Database     { get; set; } = "";
    public string Infrastructure { get; set; } = "";
    public IEnumerable<string>? AlreadySelectedPatterns { get; set; }
}

public class ValidateRequestDto
{
    public string Architecture  { get; set; } = "";
    public string Framework     { get; set; } = "";
    public string Database      { get; set; } = "";
    public string Infrastructure { get; set; } = "";
    public IEnumerable<string>? Patterns { get; set; }
}

public class PreviewRequestDto
{
    public string Architecture  { get; set; } = "";
    public string Framework     { get; set; } = "";
    public string Database      { get; set; } = "";
    public string Infrastructure { get; set; } = "";
    public string? ProjectName  { get; set; }
    public IEnumerable<string>? Patterns { get; set; }
}
