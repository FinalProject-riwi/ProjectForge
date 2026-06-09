using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ProjectForge.Application.AI;
using ProjectForge.Application.Services;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure;
using ProjectForge.Infrastructure.Data;
using ProjectForge.Infrastructure.Repositories;
using ProjectForge.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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
builder.Services.AddScoped<IShellExecutor, ShellExecutor>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IAiSuggestionService, MultiProviderAiSuggestionService>();
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
.AddCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/denied";
    options.Cookie.Name = "ProjectForge.Auth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
})
.AddGitHub("GitHub", options =>
{
    options.ClientId = githubClientId;
    options.ClientSecret = githubClientSecret;
    options.Scope.Add("repo");
    options.Scope.Add("workflow");
    options.Scope.Add("user:email");
    options.SaveTokens = true;
    options.CallbackPath = "/auth/github/callback";
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

var app = builder.Build();

// ─── Migraciones automáticas ──────────────────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Migraciones aplicadas correctamente.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al aplicar migraciones. La app continuará pero la BD puede no estar lista.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ProjectForge.Web.Hubs.GenerationHub>("/hubs/generation");

await app.RunAsync();
