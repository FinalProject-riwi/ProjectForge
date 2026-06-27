using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProjectForge.Application.UseCases.Projects;
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
    private readonly ICreateProjectUseCase _createProject;

    public ProjectsController(
        IProjectRepository projects,
        IVpsDeploymentService vps,
        ICreateProjectUseCase createProject)
    {
        _projects = projects;
        _vps = vps;
        _createProject = createProject;
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateProjectFormDto request, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (request.WizardConfigId <= 0)
            return BadRequest("WizardConfigId is required.");

        var project = await _createProject.ExecuteAsync(new CreateProjectRequest(
            userId,
            request.WizardConfigId,
            request.Name,
            request.Description), ct);

        return CreatedAtAction(nameof(Detail), new { id = project.Id }, new
        {
            project.Id,
            project.Name,
            project.Description
        });
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

public sealed class CreateProjectFormDto
{
    public int WizardConfigId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
