using ProjectForge.Application.Services;

namespace ProjectForge.API;

/// <summary>
/// Implementación vacía de IGenerationHubNotifier para el proyecto API.
/// La API no usa SignalR — los clientes deben hacer polling en GET /projects/{id}
/// para conocer el estado de generación.
/// </summary>
public class NoOpGenerationHubNotifier : IGenerationHubNotifier
{
    public Task SendLogAsync(int projectId, string step, string message, bool isError = false)
        => Task.CompletedTask;

    public Task SendStatusAsync(int projectId, string status)
        => Task.CompletedTask;
}
