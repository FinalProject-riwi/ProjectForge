using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ProjectForge.Application.DTOs;
using ProjectForge.Application.UseCases.Projects;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.API.Controllers;

/// <summary>
/// Controlador principal de la API REST de ProjectForge.
/// Autenticación: Header <c>Authorization: Bearer &lt;github_access_token&gt;</c>
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ProjectForgeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiSuggestionService _ai;
    private readonly ICreateProjectUseCase _createProject;
    private readonly IProjectGeneratorService _generator;
    private readonly IProjectRepository _projects;
    private readonly IEncryptionService _encryption;

    public ProjectForgeController(
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

    // ─── Auth helper ──────────────────────────────────────────────────────────

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;
        var token = authHeader["Bearer ".Length..].Trim();

        // El access_token está encriptado en BD — comparar desencriptando
        var users = await _db.Users
            .Select(u => new { u.Id, u.AccessToken })
            .ToListAsync();

        var matched = users.FirstOrDefault(u =>
        {
            if (string.IsNullOrWhiteSpace(u.AccessToken)) return false;
            try { return _encryption.Decrypt(u.AccessToken) == token; }
            catch { return false; }
        });

        return matched == null ? null : await _db.Users.FindAsync(matched.Id);
    }

    private IActionResult Unauthorized401(string message = "Token inválido o expirado") =>
        Unauthorized(new ApiErrorResponse(401, message));

    private static IActionResult NotFound404(string message) =>
        new NotFoundObjectResult(new ApiErrorResponse(404, message));

    // ─── AUTH ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene la información del usuario autenticado.
    /// </summary>
    /// <remarks>
    /// Verifica que el Bearer token corresponda a un usuario registrado en la BD.
    /// El token es el GitHub access_token obtenido durante el login OAuth en la app web.
    /// </remarks>
    /// <response code="200">Datos del usuario autenticado</response>
    /// <response code="401">Token inválido o no proporcionado</response>
    [HttpGet("auth/me")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();
        return Ok(MapUser(user));
    }

    // ─── CATALOG ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Lista las arquitecturas / lenguajes disponibles.
    /// </summary>
    /// <response code="200">Lista de arquitecturas soportadas</response>
    [HttpGet("catalog/architectures")]
    [ProducesResponseType(typeof(IEnumerable<ArchitectureResponse>), 200)]
    public IActionResult GetArchitectures()
    {
        var result = new[]
        {
            new ArchitectureResponse("DotNet",     "C# / .NET",   "/img/logos/languages/csharp.png",     "ASP.NET Core, Blazor, Minimal API"),
            new ArchitectureResponse("Java",       "Java",        "/img/logos/languages/java.png",        "Spring Boot, Quarkus, Micronaut"),
            new ArchitectureResponse("Python",     "Python",      "/img/logos/languages/python.png",      "FastAPI, Django, Flask"),
            new ArchitectureResponse("Php",        "PHP",         "/img/logos/languages/php.png",         "Laravel, Symfony"),
            new ArchitectureResponse("JavaScript", "JavaScript",  "/img/logos/languages/javascript.png",  "Node.js, Express, NestJS, Next.js"),
            new ArchitectureResponse("TypeScript", "TypeScript",  "/img/logos/languages/typescript.png",  "NestJS (TS), Next.js (TS)"),
        };
        return Ok(result);
    }

    /// <summary>
    /// Lista los frameworks disponibles para una arquitectura específica.
    /// </summary>
    /// <param name="architecture">Valor de arquitectura (DotNet, Java, Python, Php, JavaScript, TypeScript)</param>
    /// <response code="200">Lista de frameworks con versiones disponibles</response>
    /// <response code="400">Arquitectura inválida</response>
    [HttpGet("catalog/frameworks/{architecture}")]
    [ProducesResponseType(typeof(IEnumerable<FrameworkResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public IActionResult GetFrameworks(string architecture)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiErrorResponse(400, $"Arquitectura inválida: '{architecture}'. Valores válidos: DotNet, Java, Python, Php, JavaScript, TypeScript"));

        return Ok(GetFrameworkOptions(arch));
    }

    /// <summary>
    /// Lista las bases de datos disponibles.
    /// </summary>
    /// <response code="200">Lista de bases de datos disponibles</response>
    [HttpGet("catalog/databases")]
    [ProducesResponseType(typeof(IEnumerable<CatalogItemResponse>), 200)]
    public IActionResult GetDatabases()
    {
        var dbs = new[]
        {
            new CatalogItemResponse("PostgreSQL", "PostgreSQL", "Recomendado", "/img/logos/databases/postgresql.png"),
            new CatalogItemResponse("MySQL",      "MySQL",      "Popular",     "/img/logos/databases/mysql.png"),
            new CatalogItemResponse("SqlServer",  "SQL Server", "Empresarial", "/img/logos/databases/sqlserver.png"),
            new CatalogItemResponse("MongoDB",    "MongoDB",    "NoSQL",       "/img/logos/databases/mongodb.png"),
            new CatalogItemResponse("Redis",      "Redis",      "Cache/Cola",  "/img/logos/databases/redis.png"),
            new CatalogItemResponse("SQLite",     "SQLite",     "Desarrollo",  "/img/logos/databases/sqlite.png"),
        };
        return Ok(dbs);
    }

    /// <summary>
    /// Lista las opciones de infraestructura disponibles.
    /// </summary>
    /// <response code="200">Lista de opciones de infraestructura</response>
    [HttpGet("catalog/infrastructure")]
    [ProducesResponseType(typeof(IEnumerable<CatalogItemResponse>), 200)]
    public IActionResult GetInfrastructure()
    {
        var infra = new[]
        {
            new CatalogItemResponse("None",         "Sin contenedores", "Básico",      null),
            new CatalogItemResponse("DockerCompose", "Docker Compose",  "Recomendado", "/img/logos/infra/docker.png"),
            new CatalogItemResponse("Kubernetes",   "Kubernetes",       "Avanzado",    "/img/logos/infra/kubernetes.png"),
        };
        return Ok(infra);
    }

    /// <summary>
    /// Lista los patrones de diseño disponibles para una combinación arquitectura/framework.
    /// </summary>
    /// <param name="architecture">Arquitectura base del proyecto</param>
    /// <param name="framework">Framework seleccionado</param>
    /// <response code="200">Lista de patrones de diseño disponibles</response>
    /// <response code="400">Arquitectura o framework inválido</response>
    [HttpGet("catalog/patterns/{architecture}/{framework}")]
    [ProducesResponseType(typeof(IEnumerable<CatalogItemResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public IActionResult GetPatterns(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiErrorResponse(400, "Arquitectura inválida"));
        if (!Enum.TryParse<FrameworkType>(framework, out var fw))
            return BadRequest(new ApiErrorResponse(400, "Framework inválido"));

        var patterns = _db.DesignPatterns
            .Where(p => p.Architecture == arch)
            .OrderBy(p => p.Name)
            .ToList()
            .GroupBy(p => NormalizePatternValue(p.Pattern))
            .Select(g => SelectPatternForFramework(g.ToList(), arch, fw))
            .Where(p => p != null)
            .Select(p => new CatalogItemResponse(NormalizePatternValue(p!.Pattern), p.Name, null, null))
            .ToList();

        return Ok(patterns);
    }

    /// <summary>
    /// Lista las librerías recomendadas para una combinación arquitectura/framework.
    /// </summary>
    /// <param name="architecture">Arquitectura base del proyecto</param>
    /// <param name="framework">Framework seleccionado</param>
    /// <response code="200">Lista de librerías con categoría y puntuación de popularidad</response>
    /// <response code="400">Arquitectura o framework inválido</response>
    [HttpGet("catalog/libraries/{architecture}/{framework}")]
    [ProducesResponseType(typeof(IEnumerable<LibraryResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public IActionResult GetLibraries(string architecture, string framework)
    {
        if (!TryParseArchitecture(architecture, out var arch))
            return BadRequest(new ApiErrorResponse(400, "Arquitectura inválida"));
        if (!Enum.TryParse<FrameworkType>(framework, out var fw))
            return BadRequest(new ApiErrorResponse(400, "Framework inválido"));

        var libraries = _db.Libraries
            .Where(l => l.Architecture == arch && (l.Framework == null || l.Framework == fw))
            .OrderByDescending(l => l.PopularityScore)
            .ThenBy(l => l.Name)
            .ToList()
            .Select(l => new LibraryResponse(l.PackageName, l.Name, l.Category, l.Description ?? string.Empty, l.PopularityScore))
            .ToList();

        return Ok(libraries);
    }

    // ─── AI ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene sugerencias de IA para el stack seleccionado.
    /// </summary>
    /// <remarks>
    /// Requiere autenticación. La IA (Anthropic/OpenAI/Gemini según disponibilidad)
    /// analiza el stack y sugiere patrones de diseño y librerías complementarias.
    /// Los resultados se cachean en BD para evitar llamadas redundantes.
    /// </remarks>
    /// <response code="200">Sugerencias de patrones, librerías y justificación</response>
    /// <response code="401">Token inválido</response>
    [HttpPost("ai/suggest")]
    [ProducesResponseType(typeof(AiSuggestionResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> Suggest([FromBody] AiSuggestRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        if (!TryParseArchitecture(req.Architecture, out var arch)) arch = ArchitectureType.DotNet;
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw)) fw = FrameworkType.AspNetCoreWebApi;
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db)) db = DatabaseType.PostgreSQL;
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra)) infra = InfrastructureType.None;

        var result = await _ai.SuggestAsync(new WizardSuggestionRequest(
            arch, fw, db, infra, req.AlreadySelectedPatterns ?? Array.Empty<string>()));

        return Ok(new AiSuggestionResponse(
            result.SuggestedPatterns.ToList(),
            result.SuggestedLibraries.ToList(),
            result.Rationale));
    }

    // ─── PROJECTS ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lista todos los proyectos del usuario autenticado.
    /// </summary>
    /// <response code="200">Lista de proyectos del usuario</response>
    /// <response code="401">Token inválido</response>
    [HttpGet("projects")]
    [ProducesResponseType(typeof(IEnumerable<ProjectSummaryResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> ListProjects()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var list = await _projects.GetByUserIdAsync(user.Id);
        return Ok(list.Select(MapProject));
    }

    /// <summary>
    /// Obtiene el detalle completo de un proyecto, incluyendo config y README generado.
    /// </summary>
    /// <param name="id">ID del proyecto</param>
    /// <response code="200">Detalle completo del proyecto</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Proyecto no encontrado o no pertenece al usuario</response>
    [HttpGet("projects/{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetProject(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetFullAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        return Ok(MapProjectFull(project));
    }

    /// <summary>
    /// Obtiene los logs de generación de un proyecto.
    /// </summary>
    /// <param name="id">ID del proyecto</param>
    /// <response code="200">Lista de logs ordenados por fecha</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Proyecto no encontrado</response>
    [HttpGet("projects/{id:int}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LogEntryResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetProjectLogs(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetWithLogsAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        return Ok(project.Logs.OrderBy(l => l.CreatedAt).Select(l => new LogEntryResponse(
            l.Id, l.Message, l.CreatedAt, l.IsError ? "Error" : "Info")));
    }

    /// <summary>
    /// Crea un nuevo proyecto con la configuración del wizard.
    /// </summary>
    /// <remarks>
    /// Crea el registro del proyecto en BD. Para iniciar la generación del código
    /// usa `POST /projects/{id}/generate` después.
    /// 
    /// Solo se permite **un patrón de diseño** por proyecto (el primero del array se usa).
    /// </remarks>
    /// <response code="201">Proyecto creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">Token inválido</response>
    [HttpPost("projects")]
    [ProducesResponseType(typeof(ProjectSummaryResponse), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        if (string.IsNullOrWhiteSpace(req.ProjectName) || req.ProjectName.Length < 2)
            return BadRequest(new ApiErrorResponse(400, "El nombre del proyecto es obligatorio (mínimo 2 caracteres)"));

        if (!TryParseArchitecture(req.Architecture, out var arch))
            return BadRequest(new ApiErrorResponse(400, $"Arquitectura inválida: '{req.Architecture}'"));
        if (!Enum.TryParse<FrameworkType>(req.Framework, out var fw))
            return BadRequest(new ApiErrorResponse(400, $"Framework inválido: '{req.Framework}'"));
        if (!Enum.TryParse<DatabaseType>(req.Database, out var db))
            return BadRequest(new ApiErrorResponse(400, $"Base de datos inválida: '{req.Database}'"));
        if (!Enum.TryParse<InfrastructureType>(req.Infrastructure, out var infra))
            infra = InfrastructureType.None;

        var patterns = (req.Patterns ?? new List<string>()).Take(1).ToList();
        var libs     = req.Libraries ?? new List<string>();

        var config = new WizardConfig
        {
            Architecture          = arch,
            Framework             = fw,
            FrameworkVersion      = req.FrameworkVersion ?? "latest",
            Database              = db,
            Infrastructure        = infra,
            DeploymentTarget      = DeploymentTarget.Local,
            DesignPatternsJson    = JsonSerializer.Serialize(patterns),
            LibrariesJson         = JsonSerializer.Serialize(libs),
            AdditionalOptionsJson = JsonSerializer.Serialize(new { createPrivateRepo = req.CreatePrivateRepo }),
            CreatedAt             = DateTime.UtcNow,
        };

        _db.WizardConfigs.Add(config);
        await _db.SaveChangesAsync(ct);

        var project = await _createProject.ExecuteAsync(new ProjectForge.Application.UseCases.Projects.CreateProjectRequest(
            user.Id,
            config.Id,
            req.ProjectName.Trim(),
            req.Description?.Trim()), ct);

        return Created($"/api/v1/projects/{project.Id}", MapProject(project));
    }

    /// <summary>
    /// Inicia la generación de código de un proyecto existente.
    /// </summary>
    /// <remarks>
    /// La generación corre en background. Como la API no usa SignalR,
    /// consulta periódicamente `GET /projects/{id}` para ver el estado.
    /// 
    /// Estados posibles: `Draft` → `Generating` → `Generated` → `Pushing` → `Published` | `Failed`
    /// </remarks>
    /// <param name="id">ID del proyecto a generar</param>
    /// <response code="202">Generación iniciada en background</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Proyecto no encontrado</response>
    [HttpPost("projects/{id:int}/generate")]
    [ProducesResponseType(typeof(GenerationStartedResponse), 202)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> StartGeneration(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized401();

        var project = await _projects.GetFullAsync(id);
        if (project == null || project.UserId != user.Id)
            return NotFound404("Proyecto no encontrado");

        // Capturar IServiceProvider ANTES del Task.Run
        var apiServices = HttpContext.RequestServices;
        _ = Task.Run(async () =>
        {
            using var scope = apiServices.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<IProjectGeneratorService>();
            await generator.GenerateAsync(id);
        });

        return Accepted(new GenerationStartedResponse(
            "Generación iniciada en background.",
            "Consulta GET /api/v1/projects/{id} para ver el estado.",
            id));
    }

    /// <summary>
    /// Elimina un proyecto y sus logs asociados.
    /// </summary>
    /// <param name="id">ID del proyecto a eliminar</param>
    /// <response code="204">Proyecto eliminado exitosamente</response>
    /// <response code="401">Token inválido</response>
    /// <response code="404">Proyecto no encontrado</response>
    [HttpDelete("projects/{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
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

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private static UserResponse MapUser(ApplicationUser user) => new(
        user.Id, user.Username, user.Email, user.AvatarUrl, user.GitHubId, user.CreatedAt);

    private static ProjectSummaryResponse MapProject(Project p) => new(
        p.Id, p.Name, p.Description, p.Status.ToString(),
        p.RepositoryUrl, p.LocalPath, p.ErrorMessage, p.CreatedAt, p.UpdatedAt,
        p.WizardConfig?.Architecture.ToString(),
        p.WizardConfig?.Framework.ToString(),
        p.WizardConfig?.Database.ToString(),
        p.WizardConfig?.Infrastructure.ToString());

    private static ProjectDetailResponse MapProjectFull(Project p) => new(
        p.Id, p.Name, p.Description, p.Status.ToString(),
        p.RepositoryUrl, p.LocalPath, p.ErrorMessage, p.GeneratedReadme,
        p.CreatedAt, p.UpdatedAt,
        p.WizardConfig == null ? null : new ProjectConfigResponse(
            p.WizardConfig.Architecture.ToString(),
            p.WizardConfig.Framework.ToString(),
            p.WizardConfig.FrameworkVersion,
            p.WizardConfig.Database.ToString(),
            p.WizardConfig.Infrastructure.ToString(),
            p.WizardConfig.DeploymentTarget.ToString(),
            JsonSerializer.Deserialize<List<string>>(p.WizardConfig.DesignPatternsJson ?? "[]") ?? new(),
            JsonSerializer.Deserialize<List<string>>(p.WizardConfig.LibrariesJson ?? "[]") ?? new()),
        p.Logs?.Count ?? 0);

    private static List<FrameworkResponse> GetFrameworkOptions(ArchitectureType arch) => arch switch
    {
        ArchitectureType.DotNet => new()
        {
            new("AspNetCoreWebApi", "ASP.NET Core Web API", new[] { "10.0", "8.0", "7.0" }),
            new("AspNetCoreMVC",    "ASP.NET Core MVC",     new[] { "10.0", "8.0" }),
            new("BlazorServer",     "Blazor Server",         new[] { "10.0", "8.0" }),
            new("BlazorWasm",       "Blazor WebAssembly",    new[] { "10.0", "8.0" }),
            new("MinimalApi",       "Minimal API",           new[] { "10.0", "8.0" }),
        },
        ArchitectureType.Java => new()
        {
            new("SpringBoot", "Spring Boot", new[] { "3.3", "3.2", "2.7" }),
            new("Quarkus",    "Quarkus",     new[] { "3.x" }),
            new("Micronaut",  "Micronaut",   new[] { "4.x" }),
        },
        ArchitectureType.Python => new()
        {
            new("FastAPI", "FastAPI", new[] { "0.115", "0.110" }),
            new("Django",  "Django",  new[] { "5.0", "4.2" }),
            new("Flask",   "Flask",   new[] { "3.0", "2.3" }),
        },
        ArchitectureType.Php => new()
        {
            new("Laravel", "Laravel", new[] { "11.x", "10.x" }),
            new("Symfony", "Symfony", new[] { "7.x", "6.x" }),
        },
        ArchitectureType.JavaScript => new()
        {
            new("NodeJs",    "Node.js",    new[] { "22.x", "20.x" }),
            new("ExpressJs", "Express.js", new[] { "5.x", "4.x" }),
            new("NestJs",    "NestJS",     new[] { "10.x" }),
            new("NextJs",    "Next.js",    new[] { "14.x" }),
        },
        ArchitectureType.TypeScript => new()
        {
            new("NestTs", "NestJS (TypeScript)", new[] { "10.x" }),
            new("NextTs", "Next.js (TypeScript)", new[] { "14.x" }),
        },
        _ => new()
    };

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
        IReadOnlyList<DesignPatternEntry> patterns, ArchitectureType arch, FrameworkType framework)
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
}

// ─── Response Records ─────────────────────────────────────────────────────────

/// <summary>Respuesta de error estándar de la API.</summary>
public sealed record ApiErrorResponse(int StatusCode, string Message);

/// <summary>Datos del usuario autenticado.</summary>
public sealed record UserResponse(int Id, string Username, string? Email, string? AvatarUrl, string? GitHubId, DateTime CreatedAt);

/// <summary>Arquitectura / lenguaje disponible.</summary>
public sealed record ArchitectureResponse(string Value, string Label, string? LogoUrl, string Description);

/// <summary>Elemento del catálogo (base de datos, infraestructura, patrón).</summary>
public sealed record CatalogItemResponse(string Value, string Label, string? Badge, string? LogoUrl);

/// <summary>Framework con versiones disponibles.</summary>
public sealed record FrameworkResponse(string Value, string Label, string[] AvailableVersions);

/// <summary>Librería recomendada.</summary>
public sealed record LibraryResponse(string PackageName, string Name, string? Category, string Description, int PopularityScore);

/// <summary>Respuesta de sugerencias de IA.</summary>
public sealed record AiSuggestionResponse(List<string> SuggestedPatterns, List<string> SuggestedLibraries, string Rationale);

/// <summary>Resumen de un proyecto.</summary>
public sealed record ProjectSummaryResponse(
    int Id, string Name, string? Description, string Status,
    string? RepositoryUrl, string? LocalPath, string? ErrorMessage,
    DateTime CreatedAt, DateTime? UpdatedAt,
    string? Architecture, string? Framework, string? Database, string? Infrastructure);

/// <summary>Detalle completo de un proyecto.</summary>
public sealed record ProjectDetailResponse(
    int Id, string Name, string? Description, string Status,
    string? RepositoryUrl, string? LocalPath, string? ErrorMessage, string? GeneratedReadme,
    DateTime CreatedAt, DateTime? UpdatedAt,
    ProjectConfigResponse? Config, int LogCount);

/// <summary>Configuración del wizard que dio origen al proyecto.</summary>
public sealed record ProjectConfigResponse(
    string Architecture, string Framework, string FrameworkVersion,
    string Database, string Infrastructure, string DeploymentTarget,
    List<string> DesignPatterns, List<string> Libraries);

/// <summary>Entrada de log de generación.</summary>
public sealed record LogEntryResponse(int Id, string Message, DateTime CreatedAt, string Level);

/// <summary>Confirmación de inicio de generación.</summary>
public sealed record GenerationStartedResponse(string Message, string PollingNote, int ProjectId);

// ─── Request Bodies ───────────────────────────────────────────────────────────

/// <summary>Cuerpo para solicitar sugerencias de IA.</summary>
public sealed class AiSuggestRequest
{
    /// <example>DotNet</example>
    public string Architecture { get; set; } = "";
    /// <example>AspNetCoreWebApi</example>
    public string Framework    { get; set; } = "";
    /// <example>PostgreSQL</example>
    public string Database     { get; set; } = "";
    /// <example>DockerCompose</example>
    public string Infrastructure { get; set; } = "";
    /// <summary>Patrones ya seleccionados (para excluirlos de las sugerencias).</summary>
    public IEnumerable<string>? AlreadySelectedPatterns { get; set; }
}

/// <summary>Cuerpo para crear un nuevo proyecto.</summary>
public sealed class CreateProjectRequest
{
    /// <summary>Nombre del proyecto. Mínimo 2 caracteres.</summary>
    /// <example>MiApiRest</example>
    public string ProjectName { get; set; } = "";
    /// <example>API REST con autenticación JWT y PostgreSQL</example>
    public string? Description { get; set; }
    /// <example>DotNet</example>
    public string Architecture { get; set; } = "";
    /// <example>AspNetCoreWebApi</example>
    public string Framework { get; set; } = "";
    /// <example>10.0</example>
    public string? FrameworkVersion { get; set; }
    /// <example>PostgreSQL</example>
    public string Database { get; set; } = "";
    /// <example>DockerCompose</example>
    public string Infrastructure { get; set; } = "None";
    /// <summary>Lista de patrones de diseño (solo se aplica el primero).</summary>
    public List<string>? Patterns { get; set; }
    /// <summary>Lista de librerías a instalar.</summary>
    public List<string>? Libraries { get; set; }
    /// <summary>Si es true, el repositorio GitHub se crea como privado.</summary>
    public bool CreatePrivateRepo { get; set; }
}
