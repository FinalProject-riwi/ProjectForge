using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProjectForge.Core.Entities;
using ProjectForge.Infrastructure.Data;
using System.Security.Claims;

namespace ProjectForge.Web.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

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
        // Intentar leer el external cookie de GitHub que el middleware OAuth dejó
        var externalResult = await HttpContext.AuthenticateAsync("GitHub");

        if (externalResult.Succeeded)
        {
            // Venimos del flujo OAuth — procesar y crear cookie de aplicación
            var principal   = externalResult.Principal!;
            var githubId    = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var username    = principal.FindFirstValue(ClaimTypes.Name) ?? "";
            var email       = principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var avatar      = principal.FindFirstValue("urn:github:avatar") ?? "";
            var accessToken = externalResult.Properties?.GetTokenValue("access_token") ?? "";

            // Upsert usuario en BD
            var user = _db.Users.FirstOrDefault(u => u.GitHubId == githubId);
            if (user == null)
            {
                user = new ApplicationUser { GitHubId = githubId, CreatedAt = DateTime.UtcNow };
                _db.Users.Add(user);
            }
            user.Username    = username;
            user.Email       = email;
            user.AvatarUrl   = avatar;
            user.AccessToken = accessToken;
            user.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Crear cookie de aplicación con el Id real de la BD
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           username),
                new(ClaimTypes.Email,          email),
                new("avatar_url",              avatar),
                new("github_id",               githubId),
            };
            var identity     = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var appPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                appPrincipal,
                new AuthenticationProperties { IsPersistent = true });

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

    [HttpPost("/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
