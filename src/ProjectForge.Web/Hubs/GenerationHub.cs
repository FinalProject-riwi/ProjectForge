using Microsoft.AspNetCore.SignalR;
using ProjectForge.Application.Services;

namespace ProjectForge.Web.Hubs;

/// <summary>
/// Hub de SignalR para transmitir logs de generación en tiempo real al browser.
/// </summary>
public class GenerationHub : Hub
{
    public async Task JoinProjectRoom(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }

    public async Task LeaveProjectRoom(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }
}

/// <summary>
/// Implementación de IGenerationHubNotifier que delega en IHubContext<GenerationHub>.
/// Se registra como Scoped en DI y se inyecta en ProjectGeneratorService.
/// </summary>
public class SignalRGenerationHubNotifier : IGenerationHubNotifier
{
    private readonly IHubContext<GenerationHub> _hub;

    public SignalRGenerationHubNotifier(IHubContext<GenerationHub> hub) => _hub = hub;

    public async Task SendLogAsync(int projectId, string step, string message, bool isError = false)
    {
        await _hub.Clients.Group($"project-{projectId}").SendAsync("ReceiveLog", new
        {
            step,
            message,
            isError,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendStatusAsync(int projectId, string status)
    {
        await _hub.Clients.Group($"project-{projectId}").SendAsync("StatusChanged", status);
    }
}
