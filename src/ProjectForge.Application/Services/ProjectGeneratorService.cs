using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

// ─── Contrato mínimo de hub que necesita la capa Application ─────────────────
// Evita dependencia directa de ProjectForge.Web desde Application.
public interface IGenerationHubNotifier
{
    Task SendLogAsync(int projectId, string step, string message, bool isError = false);
    Task SendStatusAsync(int projectId, string status);
}

/// <summary>
/// Orquesta la generación del proyecto: ejecuta comandos CLI locales,
/// aplica plantillas y sube el resultado a GitHub.
/// Emite logs en tiempo real al navegador via SignalR.
/// </summary>
public partial class ProjectGeneratorService : IProjectGeneratorService
{
    private readonly IProjectRepository _projects;
    private readonly ITemplateRepository _templates;
    private readonly IDesignPatternRepository _designPatterns;
    private readonly IShellExecutor _shell;
    private readonly IGitHubService _github;
    private readonly IAiSuggestionService _ai;
    private readonly IConfiguration _config;
    private readonly IGenerationHubNotifier _hub;

    public ProjectGeneratorService(
        IProjectRepository projects,
        ITemplateRepository templates,
        IDesignPatternRepository designPatterns,
        IShellExecutor shell,
        IGitHubService github,
        IAiSuggestionService ai,
        IConfiguration config,
        IGenerationHubNotifier hub)
    {
        _projects  = projects;
        _templates = templates;
        _designPatterns = designPatterns;
        _shell     = shell;
        _github    = github;
        _ai        = ai;
        _config    = config;
        _hub       = hub;
    }

    public async Task<GenerationResult> GenerateAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetFullAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        var cfg = project.WizardConfig;
        var workBase    = _config["Generation:WorkspacePath"] ?? Path.Combine(Path.GetTempPath(), "projectforge");
        var projectPath = Path.Combine(workBase, project.Name.ToLowerInvariant().Replace(" ", "-"));

        try
        {
            await UpdateStatusAsync(project, ProjectStatus.Generating, ct);
            await _hub.SendStatusAsync(projectId, "Generating");

            // 1. Crear carpeta de trabajo
            Directory.CreateDirectory(projectPath);
            await EmitLogAsync(project, "Scaffold", $"📁 Directorio de trabajo: {projectPath}", ct: ct);

            // 2. Scaffolding según arquitectura
            await ScaffoldProjectAsync(project, cfg, projectPath, ct);
            await ScaffoldPhpBaseFilesAsync(project, cfg, projectPath, ct);
            await ScaffoldJavaScriptBaseFilesAsync(project, cfg, projectPath, ct);

            // 3. Scaffold adicional según patrón de diseño
            await ScaffoldDesignPatternsAsync(project, cfg, projectPath, ct);

            // 4. Aplicar plantillas de BD e infraestructura
            await ApplyTemplatesAsync(project, cfg, projectPath, ct);

            // 5. Instalar dependencias / librerías seleccionadas
            await InstallLibrariesAsync(project, cfg, projectPath, ct);

            // 6. Generar README con IA
            await GenerateReadmeAsync(project, cfg, projectPath, ct);

            // 7. Crear repo GitHub y hacer push
            await EmitLogAsync(project, "GitHub", "🔗 Creando repositorio en GitHub...", ct: ct);
            var repoUrl = await PushToGitHubAsync(project, projectPath, ct);

            project.LocalPath      = projectPath;
            project.RepositoryUrl  = repoUrl;
            await UpdateStatusAsync(project, ProjectStatus.Published, ct);
            await _hub.SendStatusAsync(projectId, "Published");

            return new GenerationResult(true, null, projectPath, repoUrl);
        }
        catch (Exception ex)
        {
            await EmitLogAsync(project, "Error", $"❌ {ex.Message}", isError: true, ct: ct);
            project.ErrorMessage = ex.Message;
            await UpdateStatusAsync(project, ProjectStatus.Failed, ct);
            await _hub.SendStatusAsync(projectId, "Failed");
            return new GenerationResult(false, ex.Message, projectPath, null);
        }
    }

}
