using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace ProjectForge.Web.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthController> _logger;
    private readonly IEncryptionService _encryptionService;

    public AuthController(
        AppDbContext db,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthController> logger,
        IEncryptionService encryptionService)
    {
        _db = db;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    [HttpGet("/auth/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // Este endpoint inicia el flujo OAuth — el middleware de GitHub
    // maneja el callback en /auth/github/callback automáticamente.
    // NO registrar una acción GitHubCallback: causaría conflicto con el middleware.
    [HttpGet("/auth/github")]
    public IActionResult GitHubLogin(string? returnUrl = null)
    {
        // RedirectUri: a dónde ir DESPUÉS de que el middleware haya procesado el callback.
        // Usamos /auth/complete para separarlo del CallbackPath del middleware.
        var redirectAfterLogin = returnUrl ?? "/dashboard";
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action("Complete", "Auth", new { returnUrl = redirectAfterLogin }),
        };
        return Challenge(props, "GitHub");
    }

    // Este endpoint se ejecuta DESPUÉS de que el middleware OAuth procese el callback.
    [HttpGet("/auth/complete")]
    public async Task<IActionResult> Complete(string? returnUrl = null)
    {
        // Leer el ticket externo de GitHub desde la cookie "ExternalCookies"
        // (scheme separado configurado en Program.cs con options.SignInScheme = "ExternalCookies")
        var externalResult = await HttpContext.AuthenticateAsync("ExternalCookies");

        if (externalResult.Succeeded)
        {
            // Venimos del flujo OAuth — procesar y crear cookie de aplicación
            var principal   = externalResult.Principal!;
            var githubId    = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var username    = principal.FindFirstValue(ClaimTypes.Name) ?? "";
            var email       = principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var avatar      = principal.FindFirstValue("avatar_url")
                             ?? principal.FindFirstValue("urn:github:avatar")
                             ?? "";
            var accessToken = externalResult.Properties?.GetTokenValue("access_token") ?? "";

            // Upsert usuario en BD
            // Guardar el accessToken encriptado en BD para seguridad
            var user = _db.Users.FirstOrDefault(u => u.GitHubId == githubId);
            if (user == null)
            {
                user = new ApplicationUser { GitHubId = githubId, CreatedAt = DateTime.UtcNow };
                _db.Users.Add(user);
            }
            user.Username    = username;
            user.Email       = email;
            user.AvatarUrl   = string.IsNullOrWhiteSpace(avatar) ? user.AvatarUrl : avatar;
            // Encriptar el access_token antes de persistir en BD
            user.AccessToken = !string.IsNullOrWhiteSpace(accessToken)
                ? _encryptionService.Encrypt(accessToken)
                : user.AccessToken;
            user.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Crear cookie de aplicación con el Id real de la BD
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           username),
                new(ClaimTypes.Email,          email),
                new("avatar_url",              user.AvatarUrl),
                new("github_id",               githubId),
            };
            var identity     = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var appPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                appPrincipal,
                new AuthenticationProperties { IsPersistent = true });

            // Limpiar la cookie temporal externa (ya no la necesitamos)
            await HttpContext.SignOutAsync("ExternalCookies");

            return LocalRedirect(returnUrl ?? "/dashboard");
        }

        // No hay cookie externa de GitHub — verificar si ya tiene cookie de app válida
        var appResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (appResult.Succeeded)
        {
            // Verificar que el usuario del claim todavía existe en BD
            if (int.TryParse(appResult.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)
                && _db.Users.Any(u => u.Id == uid))
            {
                return LocalRedirect(returnUrl ?? "/dashboard");
            }
            // Cookie de app válida pero usuario borrado de BD — forzar re-login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Sin autenticación válida — ir al login
        return RedirectToAction("Login");
    }

    [HttpGet("/auth/denied")]
    public IActionResult Denied(string? message = null)
    {
        TempData["Error"] = string.IsNullOrWhiteSpace(message)
            ? "No se pudo completar la autenticación con GitHub."
            : message;

        return RedirectToAction(nameof(Login));
    }

    [HttpPost("/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await RevokeGitHubAuthorizationAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task RevokeGitHubAuthorizationAsync()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return;

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user is null || string.IsNullOrWhiteSpace(user.AccessToken))
            return;

        var clientId = _configuration["GitHub:ClientId"];
        var clientSecret = _configuration["GitHub:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            _logger.LogWarning("No se pudo revocar el grant de GitHub porque faltan credenciales de configuración.");
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"https://api.github.com/applications/{Uri.EscapeDataString(clientId)}/grant");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
            request.Headers.UserAgent.ParseAdd("ProjectForge");
            // Desencriptar antes de enviar a la API de GitHub
            string plainToken;
            try { plainToken = _encryptionService.Decrypt(user.AccessToken); }
            catch { plainToken = user.AccessToken; } // fallback si ya está en plano (tokens legacy)
            request.Content = JsonContent.Create(new { access_token = plainToken });

            using var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.NoContent && !response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "GitHub devolvió {StatusCode} al revocar el grant del usuario {UserId}: {Body}",
                    (int)response.StatusCode,
                    userId,
                    responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo revocar el grant de GitHub para el usuario {UserId}.", userId);
        }
        try
        {
            user.AccessToken = string.Empty;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo limpiar el token local de GitHub para el usuario {UserId}.", userId);
        }
    }
}
