using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectForge.Application.AI;
using ProjectForge.Application.Services;
using ProjectForge.Application.UseCases.Projects;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure;
using ProjectForge.Infrastructure.Data;
using ProjectForge.Infrastructure.Repositories;
using ProjectForge.Infrastructure.Seeders.DotNet;
using ProjectForge.Infrastructure.Seeders.TypeScript;
using ProjectForge.Infrastructure.Seeders.Java;
using ProjectForge.Infrastructure.Seeders.JavaScript;
using ProjectForge.Infrastructure.Seeders.Python;
using ProjectForge.Infrastructure.Seeders.Php;

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null))
    .ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ─── Repositorios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
builder.Services.AddScoped<IDesignPatternRepository, DesignPatternRepository>();
builder.Services.AddScoped<IAiSuggestionCacheRepository, AiSuggestionCacheRepository>();

// ─── IGenerationHubNotifier no-op (API no usa SignalR) ────────────────────────
builder.Services.AddScoped<IGenerationHubNotifier, NoOpGenerationHubNotifier>();

// ─── Servicios ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IShellExecutor, ShellExecutor>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IAiSuggestionService, MultiProviderAiSuggestionService>();
builder.Services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
builder.Services.AddScoped<IProjectGeneratorService, ProjectGeneratorService>();
builder.Services.AddScoped<IVpsDeploymentService, VpsDeploymentService>();

// ─── HttpClients para proveedores de IA ──────────────────────────────────────
builder.Services.AddHttpClient("Anthropic", c =>
{
    c.BaseAddress = new Uri("https://api.anthropic.com");
    c.Timeout = TimeSpan.FromMinutes(3);
});
builder.Services.AddHttpClient("OpenAI", c =>
{
    c.BaseAddress = new Uri("https://api.openai.com");
    c.Timeout = TimeSpan.FromMinutes(3);
});
builder.Services.AddHttpClient("Gemini", c =>
{
    c.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
    c.Timeout = TimeSpan.FromMinutes(3);
});

// ─── Autenticación Bearer ─────────────────────────────────────────────────────
// El token es un GitHub OAuth access_token (opaque), no un JWT firmado.
// La validación real se hace en cada controller contra la BD.
// JwtBearer se configura sin validación estricta para que el header sea leído.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireSignedTokens = false,
            SignatureValidator = (token, _) =>
                new System.IdentityModel.Tokens.Jwt.JwtSecurityToken()
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = _ => Task.CompletedTask,
            OnAuthenticationFailed = ctx => { ctx.NoResult(); return Task.CompletedTask; }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProjectForge API",
        Version = "v1",
        Description =
            "API REST de ProjectForge para generar proyectos de software con IA.\n\n" +
            "**Autenticación:** Header `Authorization: Bearer <github_access_token>`\n\n" +
            "**Flujo básico:**\n" +
            "1. `GET /api/v1/catalog/architectures` → elige arquitectura\n" +
            "2. `GET /api/v1/catalog/frameworks/{arch}` → elige framework\n" +
            "3. `POST /api/v1/ai/suggest` → sugerencias de IA\n" +
            "4. `POST /api/v1/projects` → crea proyecto\n" +
            "5. `POST /api/v1/projects/{id}/generate` → inicia generación\n" +
            "6. `GET /api/v1/projects/{id}` → consulta estado",
        Contact = new OpenApiContact
        {
            Name = "ProjectForge",
            Url = new Uri("https://github.com/tzerk-last")
        }
    });

    // Seguridad Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "GitHub OAuth Token",
        In = ParameterLocation.Header,
        Description = "Ingresa tu GitHub access_token.\n\nEjemplo: `Bearer gho_xxxx`"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML docs (generados por <GenerateDocumentationFile>true</GenerateDocumentationFile>)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ─── Migraciones automáticas ──────────────────────────────────────────────────
{
    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
    const int maxAttempts = 15;
    const int delaySeconds = 5;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            startupLogger.LogInformation("Intento {Attempt}/{Max}: conectando a SQL Server...", attempt, maxAttempts);
            await db.Database.CanConnectAsync();
            await db.Database.MigrateAsync();

            // Ejecutar cada seeder individualmente — un fallo no bloquea los demás
            foreach (var (name, seeder) in new (string, Func<Task>)[]
            {
                ("DotNet",     () => DotNetSeeder.SeedAsync(db)),
                ("TypeScript", () => TypeScriptSeeder.SeedAsync(db)),
                ("Php",        () => PhpSeeder.SeedAsync(db)),
                ("Python",     () => PythonSeeder.SeedAsync(db)),
                ("Java",       () => JavaSeeder.SeedAsync(db)),
                ("JavaScript", () => JavaScriptSeeder.SeedAsync(db)),
            })
            {
                try   { await seeder(); }
                catch (Exception seedEx)
                {
                    startupLogger.LogError(seedEx,
                        "⚠️  Seeder {Seeder} falló: {Msg}", name, seedEx.Message);
                }
            }

            startupLogger.LogInformation("✅ Migraciones y seeders completados.");
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            startupLogger.LogWarning(
                "⏳ SQL Server aún no disponible (intento {Attempt}/{Max}): {Message}. Reintentando en {Delay}s...",
                attempt, maxAttempts, ex.Message, delaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "❌ Error fatal al aplicar migraciones tras {Max} intentos.", maxAttempts);
        }
    }
}

// ─── Middleware ───────────────────────────────────────────────────────────────
// Swagger activo siempre (Development y Production)
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectForge API v1");
    c.RoutePrefix = string.Empty;   // UI en la raíz: http://localhost:5001/
    c.DocumentTitle = "ProjectForge API";
    c.DefaultModelsExpandDepth(-1); // Ocultar schemas por defecto
    c.EnableTryItOutByDefault();
});

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

// ─── Clases de soporte ────────────────────────────────────────────────────────

/// <summary>
/// Implementación vacía de IGenerationHubNotifier para el proyecto API.
/// La API no usa SignalR — los clientes hacen polling en GET /projects/{id}.
/// </summary>
public class NoOpGenerationHubNotifier : ProjectForge.Application.Services.IGenerationHubNotifier
{
    public Task SendLogAsync(int projectId, string step, string message, bool isError = false)
        => Task.CompletedTask;

    public Task SendStatusAsync(int projectId, string status)
        => Task.CompletedTask;
}
