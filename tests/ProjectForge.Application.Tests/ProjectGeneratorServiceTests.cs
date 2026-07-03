using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ProjectForge.Application.Services;
using ProjectForge.Application.Tests.Fakes;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using Xunit;

namespace ProjectForge.Application.Tests;

/// <summary>
/// End-to-end GenerateAsync tests (one per architecture, plus dedicated regressions for bugs
/// found in earlier review sessions) using a SimulatingShellExecutor instead of real CLIs, and a
/// temp directory as the workspace. These catch orchestration-level bugs — wrong step ordering,
/// files clobbering each other, missing DI wiring — that the direct PatternGeneratorTests can't
/// see since they only exercise one generator function in isolation.
/// </summary>
public class ProjectGeneratorServiceTests : IDisposable
{
    private readonly List<string> _tempDirs = new();

    [Fact]
    public async Task GenerateAsync_dotnet_webapi_postgres_succeeds_and_registers_dbcontext()
    {
        var fixture = CreateGenerator(
            ArchitectureType.DotNet, FrameworkType.AspNetCoreWebApi, DatabaseType.PostgreSQL);

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var programCs = File.ReadAllText(Path.Combine(fixture.Workspace, "Program.cs"));
        Assert.Contains("AddDbContext<AppDbContext>", programCs);
        Assert.Contains("UseNpgsql", programCs);
    }

    [Fact]
    public async Task GenerateAsync_dotnet_microservices_pattern_creates_an_isolated_gateway_project()
    {
        // Regression: a second top-level-statements gateway/Program.cs used to land in the SAME
        // compilation as the main Program.cs (SDK default globbing), causing CS8802. Giving the
        // gateway its own .csproj fixes that; assert the .csproj exists rather than trying to
        // actually compile the output here.
        var fixture = CreateGenerator(
            ArchitectureType.DotNet, FrameworkType.AspNetCoreWebApi, DatabaseType.SQLite,
            patterns: new[] { "Microservices" });

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.True(File.Exists(Path.Combine(fixture.Workspace, "gateway", "gateway.csproj")));
    }

    [Fact]
    public async Task GenerateAsync_does_not_let_a_generic_catalog_dockerfile_clobber_the_language_specific_one()
    {
        // Regression: ApplyTemplatesAsync used to unconditionally overwrite the Dockerfile that
        // ScaffoldDotNetBaseFilesAsync already wrote (correct .dll name, correct SDK version)
        // with a generic catalog template using a mismatched name — every DotNet + Docker
        // Compose container failed to start. Seed a deliberately wrong "dockerfile" template to
        // prove it's ignored once a real one already exists on disk.
        var fixture = CreateGenerator(
            ArchitectureType.DotNet, FrameworkType.AspNetCoreWebApi, DatabaseType.PostgreSQL,
            infrastructure: InfrastructureType.DockerCompose);

        fixture.Templates.Templates.Add(new ProjectTemplate
        {
            Id = 1,
            Architecture = ArchitectureType.DotNet,
            TemplateType = "dockerfile",
            Content = "ENTRYPOINT [\"dotnet\", \"WRONG-NAME.dll\"]",
            IsActive = true
        });

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var dockerfile = File.ReadAllText(Path.Combine(fixture.Workspace, "Dockerfile"));
        Assert.DoesNotContain("WRONG-NAME.dll", dockerfile);
    }

    [Theory]
    [InlineData("Mediator")]
    [InlineData("Saga")]
    public async Task GenerateAsync_dotnet_installs_mediatr_for_mediator_and_saga_patterns(string pattern)
    {
        var fixture = CreateGenerator(
            ArchitectureType.DotNet, FrameworkType.AspNetCoreWebApi, DatabaseType.SQLite,
            patterns: new[] { pattern });

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Contains(fixture.Shell.Commands, c => c.Contains("dotnet add package MediatR"));
    }

    [Fact]
    public async Task GenerateAsync_java_quarkus_mongo_wires_mongo_config_not_spring()
    {
        var fixture = CreateGenerator(
            ArchitectureType.Java, FrameworkType.Quarkus, DatabaseType.MongoDB);

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var props = File.ReadAllText(Path.Combine(fixture.Workspace, "src", "main", "resources", "application.properties"));
        Assert.Contains("quarkus.mongodb", props);
        Assert.DoesNotContain("spring.", props);
    }

    [Fact]
    public async Task GenerateAsync_python_django_postgres_configures_postgres_not_sqlite()
    {
        var fixture = CreateGenerator(
            ArchitectureType.Python, FrameworkType.Django, DatabaseType.PostgreSQL);

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var settingsFiles = Directory.GetFiles(fixture.Workspace, "settings.py", SearchOption.AllDirectories);
        Assert.Single(settingsFiles);
        Assert.Contains("django.db.backends.postgresql", File.ReadAllText(settingsFiles[0]));
    }

    [Fact]
    public async Task GenerateAsync_php_laravel_repository_pattern_succeeds()
    {
        // Laravel's "Microservices" pattern shells out to a real `composer create-project`
        // (RunComposerCommandAsync bypasses IShellExecutor today), so it's deliberately not
        // covered by this fake-shell-based test; Repository is pure file generation.
        var fixture = CreateGenerator(
            ArchitectureType.Php, FrameworkType.Laravel, DatabaseType.MySQL,
            patterns: new[] { "Repository" });

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.True(File.Exists(Path.Combine(fixture.Workspace, "app", "Repositories", "EloquentProjectRepository.php")));
    }

    [Fact]
    public async Task GenerateAsync_javascript_express_mongo_wires_the_mongo_driver()
    {
        var fixture = CreateGenerator(
            ArchitectureType.JavaScript, FrameworkType.ExpressJs, DatabaseType.MongoDB);

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var dbConfig = File.ReadAllText(Path.Combine(fixture.Workspace, "src", "db.js"));
        Assert.Contains("require('mongodb')", dbConfig);
    }

    [Fact]
    public async Task GenerateAsync_typescript_nest_postgres_writes_a_typescript_db_client()
    {
        var fixture = CreateGenerator(
            ArchitectureType.TypeScript, FrameworkType.NestTs, DatabaseType.PostgreSQL);

        var result = await fixture.Generator.GenerateAsync(fixture.Project.Id);

        Assert.True(result.Success, result.ErrorMessage);
        var dbConfig = File.ReadAllText(Path.Combine(fixture.Workspace, "src", "db.ts"));
        Assert.Contains("import { Pool } from 'pg';", dbConfig);
    }

    // ── Test scaffolding ──────────────────────────────────────────────────────────────────

    private sealed record GeneratorFixture(
        ProjectGeneratorService Generator,
        Project Project,
        string Workspace,
        FakeTemplateRepository Templates,
        SimulatingShellExecutor Shell);

    private GeneratorFixture CreateGenerator(
        ArchitectureType architecture,
        FrameworkType framework,
        DatabaseType database,
        InfrastructureType infrastructure = InfrastructureType.None,
        IEnumerable<string>? patterns = null)
    {
        var workspaceRoot = Path.Combine(Path.GetTempPath(), "pf-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspaceRoot);
        _tempDirs.Add(workspaceRoot);

        var cfg = new WizardConfig
        {
            Id = 1,
            Architecture = architecture,
            Framework = framework,
            FrameworkVersion = "latest",
            Database = database,
            Infrastructure = infrastructure,
            DesignPatternsJson = JsonSerializer.Serialize(patterns ?? Array.Empty<string>()),
            LibrariesJson = "[]",
            AdditionalOptionsJson = "{}"
        };

        var project = new Project
        {
            Id = 42,
            Name = "Test App",
            Description = "Generated by a test",
            WizardConfigId = cfg.Id,
            WizardConfig = cfg,
            UserId = 1,
            User = new ApplicationUser { Id = 1, Username = "tester", AccessToken = "fake-token" }
        };

        // Every test uses the same Project.Id/Name, so GenerateAsync's own slug ("test-app-42")
        // is identical across tests too — WorkspacePath must be *this test's* unique temp
        // directory (not its parent), otherwise parallel test runs would all write into the
        // same folder and stomp on each other.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Generation:WorkspacePath"] = workspaceRoot
            })
            .Build();

        var templates = new FakeTemplateRepository();
        var shell = new SimulatingShellExecutor();
        var generator = new ProjectGeneratorService(
            new FakeProjectRepositoryForGeneration(project),
            templates,
            new FakeDesignPatternRepository(),
            shell,
            new FakeGitHubService(),
            new FakeAiSuggestionService(),
            configuration,
            new FakeGenerationHubNotifier(),
            new FakeEncryptionService());

        var workspace = Path.Combine(workspaceRoot, $"test-app-{project.Id}");
        return new GeneratorFixture(generator, project, workspace, templates, shell);
    }

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* best effort cleanup */ }
        }
    }
}
