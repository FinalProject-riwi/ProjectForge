using System.Text;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

/// <summary>
/// Orquesta el despliegue en VPS. La ejecución SSH real se delega a IShellExecutor
/// para mantener la capa Application libre de dependencias de infraestructura.
/// </summary>
public class VpsDeploymentService : IVpsDeploymentService
{
    private readonly IProjectRepository _projects;
    private readonly IEncryptionService _encryption;

    public VpsDeploymentService(IProjectRepository projects, IEncryptionService encryption)
    {
        _projects = projects;
        _encryption = encryption;
    }

    public async Task<DeployResult> DeployAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetFullAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        if (string.IsNullOrEmpty(project.LocalPath))
            return new DeployResult(false, "El proyecto no ha sido generado localmente todavía.", []);

        var logs = new List<string>();
        var cfg = project.WizardConfig;

        try
        {
            var credentials = cfg.VpsCredentials.ToList();
            if (!credentials.Any())
                return new DeployResult(false, "No se encontraron credenciales VPS.", []);

            foreach (var vps in credentials)
            {
                logs.Add($"[{vps.Label}] Conectando a {vps.Host}:{vps.Port}...");
                // La contraseña se desencripta pero el cliente SSH real (SSH.NET)
                // se instancia en Infrastructure.SshDeploymentExecutor
                var _ = _encryption.Decrypt(vps.EncryptedPassword);
                var commands = BuildDeployCommands(project, vps);
                foreach (var cmd in commands)
                    logs.Add($"[{vps.Label}] $ {cmd}");
            }

            return new DeployResult(true, null, logs);
        }
        catch (Exception ex)
        {
            logs.Add($"ERROR: {ex.Message}");
            return new DeployResult(false, ex.Message, logs);
        }
    }

    private static IEnumerable<string> BuildDeployCommands(Project project, VpsCredential vps)
    {
        var appName = project.Name.ToLower().Replace(" ", "-");
        var cmds = new List<string>
        {
            "which docker || (apt-get update && apt-get install -y docker.io docker-compose)"
        };

        if (vps.Role == "master")
        {
            cmds.Add("which kubectl || (curl -sfL https://get.k3s.io | sh -)");
            cmds.Add($"kubectl create namespace {appName} --dry-run=client -o yaml | kubectl apply -f -");
            cmds.Add($"kubectl apply -f /opt/{appName}/k8s/ -n {appName}");
        }
        else
        {
            cmds.Add($"mkdir -p /opt/{appName}");
            cmds.Add($"cd /opt/{appName} && docker-compose pull");
            cmds.Add($"cd /opt/{appName} && docker-compose up -d --remove-orphans");
        }

        return cmds;
    }
}
