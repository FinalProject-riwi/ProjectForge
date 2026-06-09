using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Core.Interfaces;

// ─── Repositorios genéricos ───────────────────────────────────────────────────

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// ─── Repositorios específicos ─────────────────────────────────────────────────

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByUserIdAsync(int userId);
    Task<Project?> GetWithLogsAsync(int projectId);
    Task<Project?> GetFullAsync(int projectId);
}

public interface ITemplateRepository : IRepository<ProjectTemplate>
{
    Task<IEnumerable<ProjectTemplate>> GetByArchitectureAsync(ArchitectureType arch);
    Task<ProjectTemplate?> GetTemplateAsync(ArchitectureType arch, string templateType, DatabaseType? db = null, InfrastructureType? infra = null);
    Task<IEnumerable<ProjectTemplate>> GetInfraTemplatesAsync(InfrastructureType infra, DatabaseType db);
}

public interface ILibraryRepository : IRepository<LibraryRecommendation>
{
    Task<IEnumerable<LibraryRecommendation>> GetByArchitectureAndFrameworkAsync(ArchitectureType arch, FrameworkType framework);
}

public interface IDesignPatternRepository : IRepository<DesignPatternEntry>
{
    Task<IEnumerable<DesignPatternEntry>> GetByArchitectureAsync(ArchitectureType arch);
}

// ─── Servicios de dominio ─────────────────────────────────────────────────────

public interface IShellExecutor
{
    Task<ShellResult> RunAsync(string command, string workingDirectory, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamAsync(string command, string workingDirectory, CancellationToken ct = default);
}

public interface IGitHubService
{
    Task<string> CreateRepositoryAsync(string accessToken, string repoName, string description, bool isPrivate);
    Task PushToRepositoryAsync(string localPath, string repoUrl, string accessToken);
    Task<bool> ValidateTokenAsync(string accessToken);
}

public interface IAiSuggestionService
{
    Task<AiSuggestionResult> SuggestAsync(WizardSuggestionRequest request);
    Task<string> GenerateReadmeAsync(ReadmeGenerationRequest request);
}

public interface IProjectGeneratorService
{
    Task<GenerationResult> GenerateAsync(int projectId, CancellationToken ct = default);
}

public interface IVpsDeploymentService
{
    Task<DeployResult> DeployAsync(int projectId, CancellationToken ct = default);
}

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

// ─── Value Objects / Results ──────────────────────────────────────────────────

public record ShellResult(int ExitCode, string Stdout, string Stderr)
{
    public bool Success => ExitCode == 0;
}

public record GenerationResult(bool Success, string? ErrorMessage, string? LocalPath, string? RepositoryUrl);

public record DeployResult(bool Success, string? ErrorMessage, IEnumerable<string> Logs);

public record AiSuggestionResult(
    IEnumerable<string> SuggestedPatterns,
    IEnumerable<string> SuggestedLibraries,
    string Rationale
);

public record WizardSuggestionRequest(
    ArchitectureType Architecture,
    FrameworkType Framework,
    DatabaseType Database,
    InfrastructureType Infrastructure,
    IEnumerable<string> AlreadySelectedPatterns
);

public record ReadmeGenerationRequest(
    string ProjectName,
    string Description,
    FrameworkType Framework,
    DatabaseType Database,
    InfrastructureType Infrastructure,
    IEnumerable<string> DesignPatterns,
    IEnumerable<string> Libraries,
    string? RepositoryUrl
);

public interface IAiSuggestionCacheRepository : IRepository<AiSuggestionCache>
{
    Task<AiSuggestionCache?> GetByCacheKeyAsync(string cacheKey);
}
