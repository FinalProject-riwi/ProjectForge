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
        [FromForm(Name = "__infra")]    string? infra,
        [FromForm(Name = "__patterns")] string? patternsJson,
        [FromForm(Name = "__libs")]     string? libsJson)
    {
        if (string.IsNullOrWhiteSpace(projectName) || projectName.Length < 2)
        {
            TempData["Error"] = "El nombre del proyecto es obligatorio (mínimo 2 caracteres).";
            return RedirectToAction("Index");
        }

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
        var architecture     = Enum.TryParse<ArchitectureType>(arch, out var a)        ? a : ArchitectureType.DotNet;
        var framework        = Enum.TryParse<FrameworkType>(fw, out var f)              ? f : FrameworkType.AspNetCoreWebApi;
        var database         = Enum.TryParse<DatabaseType>(db, out var d)               ? d : DatabaseType.PostgreSQL;
        var infrastructure   = Enum.TryParse<InfrastructureType>(infra, out var i)      ? i : InfrastructureType.None;

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
            Name          = projectName.Trim(),
            Description   = description?.Trim() ?? "",
            UserId        = userId,
            WizardConfigId = config.Id,
            Status        = ProjectStatus.Draft,
            CreatedAt     = DateTime.UtcNow,
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

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
        // Lanzar en background para que el HTTP response vuelva inmediatamente.
        // Los logs y el estado final llegan al browser por SignalR.
        _ = Task.Run(async () =>
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<IProjectGeneratorService>();
            await generator.GenerateAsync(id);
        });

        return Json(new { queued = true });
    }

    // ── API para el wizard frontend ────────────────────────────────────────────
    [HttpGet("api/frameworks/{architecture}")]
    public IActionResult GetFrameworks(string architecture)
    {
        if (!Enum.TryParse<ArchitectureType>(architecture, out var arch))
            return Ok(Array.Empty<object>());

        return Ok(GetFrameworkOptions(arch));
    }

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

    private static List<FrameworkOptionDto> GetFrameworkOptions(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => new()
        {
            new() { Value = "AspNetCoreWebApi", Label = "ASP.NET Core Web API",  AvailableVersions = ["10.0", "8.0", "7.0"] },
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
            new() { Value = "SpringBoot", Label = "Spring Boot", AvailableVersions = ["3.3", "3.2", "2.7"] },
            new() { Value = "Quarkus",    Label = "Quarkus",     AvailableVersions = ["3.x"] },
        },
        ArchitectureType.Laravel => new()
        {
            new() { Value = "Laravel", Label = "Laravel", AvailableVersions = ["11.x", "10.x"] },
        },
        _ => new()
    };
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
