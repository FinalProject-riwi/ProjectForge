using Microsoft.AspNetCore.Mvc;

namespace ProjectForge.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [Route("/api-docs")]
    public IActionResult ApiDocs() => View();

    [Route("/privacy")]
    public IActionResult Privacy() => View();

    [Route("/terms")]
    public IActionResult Terms() => View();

    [Route("/download")]
    public IActionResult Download() => View();

    [Route("/error")]
    public IActionResult Error() => View();
}
