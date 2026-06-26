using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using ProjectForge.Application.DTOs;
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
    private readonly IProjectGeneratorService _generator;
    private readonly ILibraryRepository _libraries;
    private readonly IDesignPatternRepository _patterns;

    public WizardController(
        AppDbContext db, IAiSuggestionService ai,
        IProjectGeneratorService generator,
        ILibraryRepository libraries, IDesignPatternRepository patterns)
    {
        _db = db; _ai = ai; _generator = generator;
        _libraries = libraries; _patterns = patterns;
    }

<<<<<<< HEAD
    // ── GET /wizard ────────────────────────────────────────────────────────────
    [HttpGet("")]
    public IActionResult Index() => View();

    // ── Step 5: recibe todo el estado del wizard desde el JS ──────────────────
=======
    [HttpGet("")]
    public IActionResult Index() => View();

>>>>>>> 0dc2a35 (complete java,python,typescript)
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
        [FromForm(Name = "__infra")]    string? infra,
        [FromForm(Name = "__patterns")] string? patternsJson,
        [FromForm(Name = "__libs")]     string? libsJson)
    {
        if (string.IsNullOrWhiteSpace(projectName) || projectName.Length < 2)
        {
            TempData["Error"] = "El nombre del proyecto es obligatorio (mínimo 2 caracteres).";
            return RedirectToAction("Index");
        }

<<<<<<< HEAD
        // Validar que el userId del claim existe en BD.
        // Si la cookie se firmó con claves Data Protection rotadas (ej: reinicio de contenedor
        // sin volumen persistente), el claim puede traer un ID que ya no existe → FK violation.
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
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

<<<<<<< HEAD
        // Parsear enums con fallback seguro
        var architecture     = Enum.TryParse<ArchitectureType>(arch, out var a)        ? a : ArchitectureType.DotNet;
        var framework        = Enum.TryParse<FrameworkType>(fw, out var f)              ? f : FrameworkType.AspNetCoreWebApi;
        var database         = Enum.TryParse<DatabaseType>(db, out var d)               ? d : DatabaseType.PostgreSQL;
        var infrastructure   = Enum.TryParse<InfrastructureType>(infra, out var i)      ? i : InfrastructureType.None;
=======
        var architecture   = Enum.TryParse<ArchitectureType>(arch, out var a)     ? a : ArchitectureType.DotNet;
        var framework      = Enum.TryParse<FrameworkType>(fw, out var f)           ? f : FrameworkType.AspNetCoreWebApi;
        var database       = Enum.TryParse<DatabaseType>(db, out var d)            ? d : DatabaseType.PostgreSQL;
        var infrastructure = Enum.TryParse<InfrastructureType>(infra, out var i)   ? i : InfrastructureType.None;
>>>>>>> 0dc2a35 (complete java,python,typescript)

        var patterns = TryParseJson<List<string>>(patternsJson) ?? new List<string>();
        var libs     = TryParseJson<List<string>>(libsJson)     ?? new List<string>();

        var config = new WizardConfig
        {
            Architecture       = architecture,
            Framework          = framework,
            FrameworkVersion   = fwv ?? "latest",
            Database           = database,
            Infrastructure     = infrastructure,
            DeploymentTarget   = DeploymentTarget.Local,
            DesignPatternsJson = JsonSerializer.Serialize(patterns),
            LibrariesJson      = JsonSerializer.Serialize(libs),
            CreatedAt          = DateTime.UtcNow,
        };

        _db.WizardConfigs.Add(config);
        await _db.SaveChangesAsync();

        var project = new Project
        {
<<<<<<< HEAD
            Name          = projectName.Trim(),
            Description   = description?.Trim() ?? "",
            UserId        = userId,
            WizardConfigId = config.Id,
            Status        = ProjectStatus.Draft,
            CreatedAt     = DateTime.UtcNow,
=======
            Name           = projectName.Trim(),
            Description    = description?.Trim() ?? "",
            UserId         = userId,
            WizardConfigId = config.Id,
            Status         = ProjectStatus.Draft,
            CreatedAt      = DateTime.UtcNow,
>>>>>>> 0dc2a35 (complete java,python,typescript)
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return RedirectToAction("Generate", new { id = project.Id });
    }

<<<<<<< HEAD
    // ── Generación ─────────────────────────────────────────────────────────────
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
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
<<<<<<< HEAD
        // Lanzar en background para que el HTTP response vuelva inmediatamente.
        // Los logs y el estado final llegan al browser por SignalR.
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
        _ = Task.Run(async () =>
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<IProjectGeneratorService>();
            await generator.GenerateAsync(id);
        });

        return Json(new { queued = true });
    }

<<<<<<< HEAD
    // ── API para el wizard frontend ────────────────────────────────────────────
=======
    // ── API ───────────────────────────────────────────────────────────────────

>>>>>>> 0dc2a35 (complete java,python,typescript)
    [HttpGet("api/frameworks/{architecture}")]
    public IActionResult GetFrameworks(string architecture)
    {
        if (!Enum.TryParse<ArchitectureType>(architecture, out var arch))
            return Ok(Array.Empty<object>());

        return Ok(GetFrameworkOptions(arch));
    }

<<<<<<< HEAD
=======
    [HttpGet("api/patterns/{architecture}")]
    public async Task<IActionResult> GetPatterns(string architecture)
    {
        if (!Enum.TryParse<ArchitectureType>(architecture, out var arch))
            return Ok(Array.Empty<object>());

        var patterns = await _patterns.GetByArchitectureAsync(arch);

        return Ok(patterns.Select(p => new
        {
            p.Name,
            p.Description
        }));
    }

    [HttpGet("api/libraries/{architecture}")]
    public async Task<IActionResult> GetLibraries(string architecture)
    {
        if (!Enum.TryParse<ArchitectureType>(architecture, out var arch))
            return Ok(Array.Empty<object>());

        var libs = await _db.Set<LibraryRecommendation>()
            .Where(l => l.Architecture == arch)
            .OrderByDescending(l => l.PopularityScore)
            .ToListAsync();

        return Ok(libs.Select(l => new
        {
            l.Name,
            l.PackageName,
            l.Category,
            l.Description,
            l.InstallCommand
        }));
    }

>>>>>>> 0dc2a35 (complete java,python,typescript)
    [HttpPost("api/suggest")]
    public async Task<IActionResult> GetSuggestions([FromBody] WizardSuggestionRequestDto req)
    {
        if (!Enum.TryParse<ArchitectureType>(req.Architecture, out var arch))
            arch = ArchitectureType.DotNet;
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            fw = FrameworkType.AspNetCoreWebApi;
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            db = DatabaseType.PostgreSQL;
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

<<<<<<< HEAD
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
=======
        try
        {
            var result = await _ai.SuggestAsync(new WizardSuggestionRequest(
                arch, fw, db, infra, req.AlreadySelectedPatterns ?? Array.Empty<string>()));

            return Ok(new
            {
                ok                 = true,
                suggestedPatterns  = result.SuggestedPatterns,
                suggestedLibraries = result.SuggestedLibraries,
                rationale          = result.Rationale
            });
        }
        catch (Exception ex)
        {
            // Devolvemos 200 con ok=false y el motivo real, para que el wizard pueda
            // mostrar el error exacto y caer al fallback de BD sin romper la UI.
            return Ok(new
            {
                ok                 = false,
                suggestedPatterns  = Array.Empty<string>(),
                suggestedLibraries = Array.Empty<string>(),
                rationale          = "IA no disponible: " + ex.Message
            });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private static T? TryParseJson<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json); }
        catch { return default; }
    }

    private static List<FrameworkOptionDto> GetFrameworkOptions(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => new()
        {
<<<<<<< HEAD
            new() { Value = "AspNetCoreWebApi", Label = "ASP.NET Core Web API",  AvailableVersions = ["10.0", "8.0", "7.0"] },
=======
            new() { Value = "AspNetCoreWebApi", Label = "ASP.NET Core Web API",  AvailableVersions = ["10.0", "8.0"] },
>>>>>>> 0dc2a35 (complete java,python,typescript)
            new() { Value = "AspNetCoreMVC",    Label = "ASP.NET Core MVC",      AvailableVersions = ["10.0", "8.0"] },
            new() { Value = "BlazorServer",     Label = "Blazor Server",         AvailableVersions = ["10.0", "8.0"] },
            new() { Value = "MinimalApi",       Label = "Minimal API",           AvailableVersions = ["10.0", "8.0"] },
        },
        ArchitectureType.Python => new()
        {
            new() { Value = "FastAPI", Label = "FastAPI", AvailableVersions = ["0.115", "0.110"] },
            new() { Value = "Django",  Label = "Django",  AvailableVersions = ["5.0", "4.2"] },
            new() { Value = "Flask",   Label = "Flask",   AvailableVersions = ["3.0", "2.3"] },
        },
        ArchitectureType.JavaScript => new()
        {
            new() { Value = "ExpressJs", Label = "Express.js", AvailableVersions = ["4.x", "5.x"] },
            new() { Value = "NestJs",    Label = "NestJS",     AvailableVersions = ["10.x"] },
            new() { Value = "NextJs",    Label = "Next.js",    AvailableVersions = ["14.x"] },
        },
        ArchitectureType.TypeScript => new()
        {
            new() { Value = "NestTs", Label = "NestJS (TypeScript)", AvailableVersions = ["10.x"] },
            new() { Value = "NextTs", Label = "Next.js (TypeScript)", AvailableVersions = ["14.x"] },
        },
        ArchitectureType.Java => new()
        {
<<<<<<< HEAD
            new() { Value = "SpringBoot", Label = "Spring Boot", AvailableVersions = ["3.3", "3.2", "2.7"] },
=======
            new() { Value = "SpringBoot", Label = "Spring Boot", AvailableVersions = ["3.5.3"] },
>>>>>>> 0dc2a35 (complete java,python,typescript)
            new() { Value = "Quarkus",    Label = "Quarkus",     AvailableVersions = ["3.x"] },
        },
        ArchitectureType.Laravel => new()
        {
            new() { Value = "Laravel", Label = "Laravel", AvailableVersions = ["11.x", "10.x"] },
        },
        _ => new()
    };
<<<<<<< HEAD
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
=======
    [HttpPost("api/chat")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto req)
    {
        if (req?.Messages == null || !req.Messages.Any())
            return BadRequest(new { error = "No messages provided" });

        var systemPrompt =
            "Eres un asistente experto en arquitectura de software integrado en ProjectForge. " +
            "Ayudas a los desarrolladores a entender patrones de diseño (Repository, CQRS, Clean Architecture, DDD, " +
            "Hexagonal Architecture, Microservices, MVT, Factory Method, Decorator, Observer, Mediator, Singleton, " +
            "Dependency Injection, SOLID) y librerías recomendadas para Python (Pydantic, SQLAlchemy, FastAPI, pytest, httpx), " +
            "Java (Spring Boot, Spring Data JPA, MapStruct, Lombok, JUnit 5, Flyway) y " +
            "TypeScript (Zod, Prisma, TypeORM, RxJS, TanStack Query, InversifyJS, class-validator). " +
            "Responde en español, de forma concisa (máx 3 párrafos), con ejemplos prácticos cuando sea útil. " +
            "Enfócate en el contexto del wizard: el usuario está eligiendo stack para su proyecto.";

        var apiKeyAnthropic = HttpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["Anthropic:ApiKey"];
        var apiKeyOpenAi = HttpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["OpenAI:ApiKey"];
        var apiKeyGemini = HttpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["Gemini:ApiKey"];

        bool HasKey(string? k) => !string.IsNullOrWhiteSpace(k) && k != "PLACEHOLDER";

        if (!HasKey(apiKeyAnthropic) && !HasKey(apiKeyOpenAi) && !HasKey(apiKeyGemini))
            return Ok(new { reply = "El chatbot requiere configurar al menos una API Key (Anthropic, OpenAI o Gemini) en el archivo .env." });

        try
        {
            // Usa la cadena de fallback Anthropic → OpenAI → Gemini.
            var recentMessages = req.Messages
                .TakeLast(10)
                .Select(m => (Role: m.Role, Content: m.Content));

            var reply = await _ai.ChatAsync(systemPrompt, recentMessages);
            return Ok(new { reply = string.IsNullOrWhiteSpace(reply) ? "Sin respuesta." : reply });
        }
        catch (Exception ex)
        {
            return Ok(new { reply = "Error al conectar con la IA: " + ex.Message });
        }
    }


}

public class WizardSuggestionRequestDto
{
    public string Architecture   { get; set; } = "";
    public string Framework      { get; set; } = "";
    public string Database       { get; set; } = "";
    public string Infrastructure { get; set; } = "";
    public IEnumerable<string>? AlreadySelectedPatterns { get; set; }
}
public class ChatMessageDto
{
    public string Role    { get; set; } = "";
    public string Content { get; set; } = "";
}

public class ChatRequestDto
{
    public List<ChatMessageDto>? Messages { get; set; }
}
>>>>>>> 0dc2a35 (complete java,python,typescript)
