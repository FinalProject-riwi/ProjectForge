using ProjectForge.Core.Entities;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.UseCases.Projects;

public sealed record CreateProjectRequest(
    int UserId,
    int WizardConfigId,
    string Name,
    string? Description);

public interface ICreateProjectUseCase
{
    Task<Project> ExecuteAsync(CreateProjectRequest request, CancellationToken ct = default);
}

public sealed class CreateProjectUseCase : ICreateProjectUseCase
{
    private readonly IProjectRepository _projects;

    public CreateProjectUseCase(IProjectRepository projects)
    {
        _projects = projects;
    }

    public Task<Project> ExecuteAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var project = Project.Create(request.Name, request.Description, request.UserId, request.WizardConfigId);
        return _projects.AddAsync(project);
    }
}
