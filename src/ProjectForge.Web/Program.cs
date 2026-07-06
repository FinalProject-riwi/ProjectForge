using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
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
using ProjectForge.Infrastructure.Seeders;
using ProjectForge.Web.Hubs;

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

// ─── Data Protection (persistir claves entre reinicios de contenedor) ─────────
// La ruta se inyecta via env var ASPNETCORE_DataProtection__KeysPath desde docker-compose.
// Si no está configurada, .NET usa el directorio default (no persistente).
var dpKeysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_DataProtection__KeysPath");
if (!string.IsNullOrWhiteSpace(dpKeysPath))
{
    Directory.CreateDirectory(dpKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
        .SetApplicationName("ProjectForge");
}
else
{
    builder.Services.AddDataProtection()
        .SetApplicationName("ProjectForge");
}

// ─── Repositorios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
builder.Services.AddScoped<IDesignPatternRepository, DesignPatternRepository>();
builder.Services.AddScoped<IAiSuggestionCacheRepository, AiSuggestionCacheRepository>();
builder.Services.AddScoped<IGenerationHubNotifier, SignalRGenerationHubNotifier>();

// ─── Servicios ────────────────────────────────────────────────────────────────
// ShellExecutor stays registered as itself (concrete type) — RoutedShellExecutor uses it as the
// local-process fallback for tools with no worker configured (git) or when no Workers:* URLs are
// set at all (e.g. local "dotnet run" dev without Docker Compose).
builder.Services.AddScoped<ShellExecutor>();
builder.Services.AddHttpClient("projectforge-worker");
builder.Services.AddScoped<IShellExecutor, RoutedShellExecutor>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IAiSuggestionService, MultiProviderAiSuggestionService>();
builder.Services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
builder.Services.AddScoped<IProjectGeneratorService, ProjectGeneratorService>();
builder.Services.AddScoped<IVpsDeploymentService, VpsDeploymentService>();

// ─── HttpClients para proveedores de IA (Anthropic → OpenAI → Gemini) ────────
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
// ─── Sesión ───────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(30);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
    opts.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ProjectForge.Antiforgery.v2";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ─── Autenticación GitHub OAuth ───────────────────────────────────────────────
// NOTA: No hacemos throw si los valores no están — la app arranca igualmente
// y muestra error solo si el usuario intenta hacer login sin configurar las credenciales.
var githubClientId = builder.Configuration["GitHub:ClientId"] ?? "PLACEHOLDER";
var githubClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? "PLACEHOLDER";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
// Cookie principal de la aplicación
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/denied";
    options.Cookie.Name = "ProjectForge.Auth.v2";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
})
// Cookie temporal para el ticket externo de GitHub OAuth
// (scheme separado para evitar colisión con la cookie de app)
.AddCookie("ExternalCookies", options =>
{
    options.Cookie.Name = "ProjectForge.External";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
})
.AddGitHub("GitHub", options =>
{
    options.ClientId = githubClientId;
    options.ClientSecret = githubClientSecret;
    // El ticket externo se almacena en la cookie "ExternalCookies",
    // no en la cookie principal de la app — evita el redirect loop.
    options.SignInScheme = "ExternalCookies";
    options.Scope.Add("repo");
    options.Scope.Add("workflow");
    options.Scope.Add("user:email");
    options.SaveTokens = true;
    options.CallbackPath = "/auth/github/callback";
    options.ClaimActions.MapJsonKey("avatar_url", "avatar_url");
    options.ClaimActions.MapJsonKey("github_login", "login");
    options.Events.OnRemoteFailure = context =>
    {
        context.HandleResponse();
        var reason = string.IsNullOrWhiteSpace(context.Failure?.Message)
            ? "No se pudo completar la autenticación con GitHub."
            : "Has cancelado o denegado el acceso con GitHub.";
        context.Response.Redirect($"/auth/denied?message={Uri.EscapeDataString(reason)}");
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// ─── CORS para SignalR en producción ────────────────────────────────────────
// Permite la conexión desde el mismo origen y WebSocket
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policyBuilder =>
    {
        policyBuilder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()  // ← CRÍTICO para SignalR con autenticación
            .WithOrigins(
                "https://tabbuilderr.duckdns.org",
                "http://localhost:5000",
                "http://localhost:3000",
                "https://localhost:5001"
            );
    });
});

builder.Services.AddControllersWithViews();

// ─── SignalR con configuración para producción ──────────────────────────────
builder.Services.AddSignalR(opts =>
{
    opts.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32 MB para archivos grandes
    opts.KeepAliveInterval = TimeSpan.FromSeconds(15);
    opts.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    opts.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddOpenApi();

var app = builder.Build();

// ─── Migraciones automáticas (con reintentos para esperar SQL Server) ─────────
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

            // Esperar a que el servidor acepte conexiones
            await db.Database.CanConnectAsync();

            await db.Database.MigrateAsync();

            // Ejecutar cada seeder individualmente — un fallo no bloquea los demás
            foreach (var (name, seeder) in new (string, Func<Task>)[]
            {
                ("DotNet",        () => DotNetSeeder.SeedAsync(db)),
                ("TypeScript",    () => TypeScriptSeeder.SeedAsync(db)),
                ("Php",           () => PhpSeeder.SeedAsync(db)),
                ("Python",        () => PythonSeeder.SeedAsync(db)),
                ("Java",          () => JavaSeeder.SeedAsync(db)),
                ("JavaScript",    () => JavaScriptSeeder.SeedAsync(db)),
                ("AllCombinations", () => AllCombinationsSeeder.SeedAsync(db)),
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

var httpsPort = app.Configuration["ASPNETCORE_HTTPS_PORT"]
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrWhiteSpace(httpsPort))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// ─── CORS debe aplicarse después de UseRouting pero antes de los endpoints ──
app.UseCors("SignalRCorsPolicy");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); // Maps [ApiController] attribute-based routes (e.g. ApiV1Controller)
app.MapHub<ProjectForge.Web.Hubs.GenerationHub>("/hubs/generation", opts =>
{
    opts.Transports = HttpTransportType.WebSockets 
                     | HttpTransportType.LongPolling;
});

await app.RunAsync();
