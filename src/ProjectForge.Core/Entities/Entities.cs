using ProjectForge.Core.Enums;
using ProjectForge.Core.Exceptions;

namespace ProjectForge.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    // Sin = DateTime.UtcNow — EF Core 10 lo detecta como valor dinámico en HasData y falla
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ApplicationUser : BaseEntity
{
    public string GitHubId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RepositoryUrl { get; set; }
    public string? LocalPath { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public string? ErrorMessage { get; set; }
    public int WizardConfigId { get; set; }
    public WizardConfig WizardConfig { get; set; } = null!;
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ProjectLog> Logs { get; set; } = new List<ProjectLog>();
    public string? GeneratedReadme { get; set; }

    public static Project Create(string name, string? description, int userId, int wizardConfigId)
    {
        var project = new Project
        {
            UserId = RequirePositiveId(userId, nameof(userId)),
            WizardConfigId = RequirePositiveId(wizardConfigId, nameof(wizardConfigId)),
            Status = ProjectStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        project.Rename(name);
        project.UpdateDescription(description);
        return project;
    }

    public void Rename(string name)
    {
        var normalized = NormalizeName(name);
        if (normalized.Length < 3)
            throw new DomainException("Project name must contain at least 3 characters.");

        Name = normalized;
        Touch();
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        Touch();
    }

    public void MarkGenerating()
    {
        Status = ProjectStatus.Generating;
        Touch();
    }

    public void MarkPublished(string repositoryUrl, string localPath)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            throw new DomainException("Repository URL cannot be empty.");

        RepositoryUrl = repositoryUrl.Trim();
        LocalPath = string.IsNullOrWhiteSpace(localPath) ? null : localPath.Trim();
        Status = ProjectStatus.Published;
        ErrorMessage = null;
        Touch();
    }

    public void MarkFailed(string errorMessage)
    {
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage.Trim();
        Status = ProjectStatus.Failed;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static int RequirePositiveId(int value, string parameterName) =>
        value > 0 ? value : throw new DomainException($"{parameterName} must be greater than zero.");

    private static string NormalizeName(string name) => string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
}

public class WizardConfig : BaseEntity
{
    public ArchitectureType Architecture { get; set; }
    public FrameworkType Framework { get; set; }
    public string FrameworkVersion { get; set; } = string.Empty;
    public DatabaseType Database { get; set; }
    public InfrastructureType Infrastructure { get; set; }
    public DeploymentTarget DeploymentTarget { get; set; } = DeploymentTarget.Local;
    public string DesignPatternsJson { get; set; } = "[]";
    public string LibrariesJson { get; set; } = "[]";
    public string AdditionalOptionsJson { get; set; } = "{}";
    public ICollection<VpsCredential> VpsCredentials { get; set; } = new List<VpsCredential>();
}

public class ProjectTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArchitectureType Architecture { get; set; }
    public FrameworkType? Framework { get; set; }
    public DatabaseType? Database { get; set; }
    public InfrastructureType? Infrastructure { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VariablesSchemaJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
}

public class LibraryRecommendation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string Description { get; set; } = string.Empty;
    public ArchitectureType Architecture { get; set; }
    public FrameworkType? Framework { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PopularityScore { get; set; }
    public string? InstallCommand { get; set; }
    // Navegación que genera la shadow property LibraryRecommendationId en DesignPatternEntry
    // — debe estar presente para que el modelo coincida con la migración generada
    public ICollection<DesignPatternEntry> SuggestedWithPatterns { get; set; } = new List<DesignPatternEntry>();
}

public class DesignPatternEntry : BaseEntity
{
    public DesignPattern Pattern { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArchitectureType Architecture { get; set; }
    public string? ImplementationNotes { get; set; }
    public string? ScaffoldCommandsJson { get; set; }
}

public class ProjectLog : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Step { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public string? CommandExecuted { get; set; }
    public int? ExitCode { get; set; }
}

public class VpsCredential : BaseEntity
{
    public int WizardConfigId { get; set; }
    public WizardConfig WizardConfig { get; set; } = null!;
    public string Label { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public string? PrivateKeyPath { get; set; }
    public string Role { get; set; } = "worker";
}

public class AiSuggestionCache : BaseEntity
{
    // Clave de lookup: hash de la combinación arch+fw+db+infra+patterns
    public string CacheKey { get; set; } = string.Empty;
    public string PatternsJson { get; set; } = "[]";
    public string LibrariesJson { get; set; } = "[]";
    public string Rationale { get; set; } = string.Empty;
    // Para expirar entradas viejas (opcional)
    public DateTime ExpiresAt { get; set; }
}
