using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Web.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController : Controller
{
    private readonly IProjectRepository _projects;

    public DashboardController(IProjectRepository projects) => _projects = projects;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var projects = await _projects.GetByUserIdAsync(userId);
        return View(projects);
    }
}

[Authorize]
[Route("projects")]
public class ProjectsController : Controller
{
    private readonly IProjectRepository _projects;
    private readonly IVpsDeploymentService _vps;

    public ProjectsController(IProjectRepository projects, IVpsDeploymentService vps)
    {
        _projects = projects;
        _vps = vps;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var project = await _projects.GetFullAsync(id);
        if (project == null) return NotFound();
        return View(project);
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> Logs(int id)
    {
        var project = await _projects.GetWithLogsAsync(id);
        if (project == null) return NotFound();
        return Json(project.Logs.OrderBy(l => l.CreatedAt));
    }

    [HttpPost("{id:int}/deploy")]
    public async Task<IActionResult> Deploy(int id, CancellationToken ct)
    {
        var result = await _vps.DeployAsync(id, ct);
        return Json(new { result.Success, result.ErrorMessage, result.Logs });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _projects.DeleteAsync(id);
        return Ok();
    }
}
