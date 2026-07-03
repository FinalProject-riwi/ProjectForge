using System.Runtime.CompilerServices;
using ProjectForge.Application.Services;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Tests.Fakes;

internal sealed class FakeTemplateRepository : ITemplateRepository
{
    // Empty by default — matches production behavior when the catalog has no matching row
    // (ApplyTemplatesAsync just skips writing that file rather than failing). Tests that need to
    // prove a specific template interaction (e.g. the Dockerfile-clobbering regression) can add
    // canned rows here.
    public List<ProjectTemplate> Templates { get; } = new();

    public Task<ProjectTemplate?> GetByIdAsync(int id) => Task.FromResult<ProjectTemplate?>(null);
    public Task<IEnumerable<ProjectTemplate>> GetAllAsync() => Task.FromResult<IEnumerable<ProjectTemplate>>(Templates);
    public Task<ProjectTemplate> AddAsync(ProjectTemplate entity) { Templates.Add(entity); return Task.FromResult(entity); }
    public Task UpdateAsync(ProjectTemplate entity) => Task.CompletedTask;
    public Task DeleteAsync(int id) => Task.CompletedTask;
    public Task<IEnumerable<ProjectTemplate>> GetByArchitectureAsync(ArchitectureType arch) =>
        Task.FromResult(Templates.Where(t => t.Architecture == arch));

    public Task<ProjectTemplate?> GetTemplateAsync(
        ArchitectureType arch, string templateType, DatabaseType? db = null,
        InfrastructureType? infra = null, FrameworkType? framework = null)
    {
        var candidates = Templates.Where(t => t.Architecture == arch && t.TemplateType == templateType && t.IsActive);

        var best = candidates
            .OrderByDescending(t => db.HasValue && t.Database == db)
            .ThenByDescending(t => infra.HasValue && t.Infrastructure == infra)
            .ThenByDescending(t => framework.HasValue && t.Framework == framework)
            .ThenByDescending(t => t.Version)
            .ThenByDescending(t => t.Id)
            .FirstOrDefault();

        return Task.FromResult(best);
    }

    public Task<IEnumerable<ProjectTemplate>> GetInfraTemplatesAsync(InfrastructureType infra, DatabaseType db) =>
        Task.FromResult(Templates.Where(t => t.Infrastructure == infra && (t.Database == db || t.Database == null)));
}

internal sealed class FakeDesignPatternRepository : IDesignPatternRepository
{
    public Task<DesignPatternEntry?> GetByIdAsync(int id) => Task.FromResult<DesignPatternEntry?>(null);
    public Task<IEnumerable<DesignPatternEntry>> GetAllAsync() => Task.FromResult<IEnumerable<DesignPatternEntry>>(Array.Empty<DesignPatternEntry>());
    public Task<DesignPatternEntry> AddAsync(DesignPatternEntry entity) => Task.FromResult(entity);
    public Task UpdateAsync(DesignPatternEntry entity) => Task.CompletedTask;
    public Task DeleteAsync(int id) => Task.CompletedTask;
    public Task<IEnumerable<DesignPatternEntry>> GetByArchitectureAsync(ArchitectureType arch) =>
        Task.FromResult<IEnumerable<DesignPatternEntry>>(Array.Empty<DesignPatternEntry>());
}

internal sealed class FakeGitHubService : IGitHubService
{
    public Task<string> CreateRepositoryAsync(string accessToken, string repoName, string description, bool isPrivate) =>
        Task.FromResult($"https://github.com/fake-user/{repoName}.git");

    public Task PushToRepositoryAsync(string localPath, string repoUrl, string accessToken) => Task.CompletedTask;

    public Task<bool> ValidateTokenAsync(string accessToken) => Task.FromResult(true);
}

internal sealed class FakeAiSuggestionService : IAiSuggestionService
{
    public Task<AiSuggestionResult> SuggestAsync(WizardSuggestionRequest request) =>
        Task.FromResult(new AiSuggestionResult(Array.Empty<string>(), Array.Empty<string>(), string.Empty));

    public Task<string> GenerateReadmeAsync(ReadmeGenerationRequest request) =>
        Task.FromResult("# Fake README\n");
}

internal sealed class FakeGenerationHubNotifier : IGenerationHubNotifier
{
    public Task SendLogAsync(int projectId, string step, string message, bool isError = false) => Task.CompletedTask;
    public Task SendStatusAsync(int projectId, string status) => Task.CompletedTask;
}

internal sealed class FakeEncryptionService : IEncryptionService
{
    // Passthrough — good enough for tests, none of them exercise real secrets.
    public string Encrypt(string plainText) => plainText;
    public string Decrypt(string cipherText) => cipherText;
}

internal sealed class FakeProjectRepositoryForGeneration : IProjectRepository
{
    private Project? _project;

    public FakeProjectRepositoryForGeneration(Project project) => _project = project;

    public Task<Project?> GetByIdAsync(int id) => Task.FromResult(_project?.Id == id ? _project : null);
    public Task<IEnumerable<Project>> GetAllAsync() => Task.FromResult<IEnumerable<Project>>(_project is null ? Array.Empty<Project>() : new[] { _project });
    public Task<Project> AddAsync(Project entity) { _project = entity; return Task.FromResult(entity); }
    public Task UpdateAsync(Project entity) { _project = entity; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _project = null; return Task.CompletedTask; }
    public Task<IEnumerable<Project>> GetByUserIdAsync(int userId) => Task.FromResult<IEnumerable<Project>>(Array.Empty<Project>());
    public Task<Project?> GetWithLogsAsync(int projectId) => Task.FromResult(_project?.Id == projectId ? _project : null);
    public Task<Project?> GetFullAsync(int projectId) => Task.FromResult(_project?.Id == projectId ? _project : null);
}

/// <summary>
/// Simulates just enough of "dotnet new" / "npm init" / "nest new" / "create-next-app"'s
/// file-creation side effects for the downstream pipeline (AppDbContext DI registration,
/// Dockerfile csproj lookup, provider registration, ...) to have real files to work with,
/// without needing those CLIs installed wherever tests run. Everything else (git, tool
/// version checks) is treated as a successful no-op.
/// </summary>
internal sealed class SimulatingShellExecutor : IShellExecutor
{
    public List<string> Commands { get; } = new();

    public Task<ShellResult> RunAsync(string command, string workingDirectory, CancellationToken ct = default)
    {
        Commands.Add(command);

        if (command.StartsWith("dotnet new", StringComparison.Ordinal) && !command.Contains(" sln", StringComparison.Ordinal))
            SimulateDotNetNew(command, workingDirectory);
        else if (command.StartsWith("dotnet sln add", StringComparison.Ordinal))
        {
            // no-op — the .csproj already exists on disk from SimulateDotNetNew
        }
        else if (command.StartsWith("npm init", StringComparison.Ordinal))
            WritePackageJson(workingDirectory);
        else if (command.StartsWith("nest new", StringComparison.Ordinal))
            SimulateNestNew(workingDirectory);
        else if (command.Contains("create-next-app", StringComparison.Ordinal))
            WritePackageJson(workingDirectory);

        return Task.FromResult(new ShellResult(0, string.Empty, string.Empty));
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string command, string workingDirectory, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await RunAsync(command, workingDirectory, ct);
        yield break;
    }

    private static void SimulateDotNetNew(string command, string workingDirectory)
    {
        var name = ExtractArg(command, "-n") ?? "App";
        var outputDir = ExtractArg(command, "-o") ?? workingDirectory;
        Directory.CreateDirectory(outputDir);

        File.WriteAllText(Path.Combine(outputDir, $"{name}.csproj"), """
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
""");
        File.WriteAllText(Path.Combine(outputDir, "Program.cs"), """
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "ok");
app.Run();
""");
        File.WriteAllText(Path.Combine(outputDir, "appsettings.json"), "{\n}\n");
    }

    private static void SimulateNestNew(string workingDirectory)
    {
        WritePackageJson(workingDirectory);
        var srcDir = Path.Combine(workingDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(srcDir, "app.module.ts"), "export class AppModule {}\n");
    }

    private static void WritePackageJson(string workingDirectory)
    {
        Directory.CreateDirectory(workingDirectory);
        File.WriteAllText(Path.Combine(workingDirectory, "package.json"), "{\n  \"name\": \"app\",\n  \"version\": \"1.0.0\"\n}\n");
    }

    // Naive space-split arg parsing — matches a known limitation of GetScaffoldCommands itself
    // (ShellExecutor.BuildStartInfo doesn't quote arguments either); fine for a test double as
    // long as test fixtures use temp paths without spaces.
    private static string? ExtractArg(string command, string flag)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length - 1; i++)
            if (parts[i] == flag) return parts[i + 1];
        return null;
    }
}
