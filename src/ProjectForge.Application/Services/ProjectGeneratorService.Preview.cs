using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Preview: returns expected file tree without any I/O ─────────────────

    public IReadOnlyList<string> PreviewFiles(
        ArchitectureType arch,
        FrameworkType framework,
        DatabaseType db,
        InfrastructureType infra,
        IEnumerable<string> patterns,
        string projectName)
    {
        var files = new List<string>();
        var safe = System.Text.RegularExpressions.Regex.Replace(projectName.Trim(), @"[^\w\-]", "-");
        var patternSet = patterns.Select(PreviewTok).ToHashSet();

        // Infrastructure files
        switch (infra)
        {
            case InfrastructureType.DockerCompose:
                files.Add("Dockerfile");
                files.Add("docker-compose.yml");
                break;
            case InfrastructureType.Kubernetes:
                files.Add("Dockerfile");
                files.Add("k8s/deployment.yaml");
                break;
        }

        // Always present
        files.Add(".gitignore");
        files.Add(".github/workflows/ci.yml");
        files.Add(".env.example");
        files.Add("Makefile");
        files.Add("README.md");

        // Framework-specific source files
        files.AddRange(GetFrameworkFileList(arch, framework, safe));

        // Pattern-specific files
        files.AddRange(GetPatternFileList(arch, framework, patternSet, safe));

        return files.Distinct().OrderBy(f => f).ToList().AsReadOnly();
    }

    private static IEnumerable<string> GetFrameworkFileList(ArchitectureType arch, FrameworkType fw, string name) =>
        arch switch
        {
            ArchitectureType.DotNet => fw switch
            {
                FrameworkType.AspNetCoreWebApi => new[]
                {
                    $"{name}.csproj", $"{name}.sln",
                    "Program.cs", "appsettings.json", "appsettings.Development.json",
                    "Controllers/WeatherForecastController.cs",
                    "Properties/launchSettings.json",
                },
                FrameworkType.AspNetCoreMVC => new[]
                {
                    $"{name}.csproj",
                    "Program.cs", "appsettings.json",
                    "Controllers/HomeController.cs",
                    "Models/ErrorViewModel.cs",
                    "Views/Home/Index.cshtml", "Views/Shared/_Layout.cshtml",
                    "wwwroot/css/site.css", "wwwroot/js/site.js",
                },
                FrameworkType.BlazorServer => new[]
                {
                    $"{name}.csproj",
                    "Program.cs", "appsettings.json",
                    "App.razor", "Routes.razor",
                    "Components/Pages/Home.razor",
                    "Components/Layout/MainLayout.razor",
                },
                FrameworkType.BlazorWasm => new[]
                {
                    $"{name}.csproj",
                    "Program.cs",
                    "App.razor",
                    "wwwroot/index.html", "wwwroot/css/app.css",
                    "Pages/Home.razor", "Layout/MainLayout.razor",
                },
                FrameworkType.MinimalApi => new[]
                {
                    $"{name}.csproj", $"{name}.sln",
                    "Program.cs", "appsettings.json",
                },
                _ => new[] { "Program.cs", $"{name}.csproj" }
            },
            ArchitectureType.Java => fw switch
            {
                FrameworkType.SpringBoot => new[]
                {
                    "pom.xml",
                    "src/main/java/com/example/Application.java",
                    "src/main/resources/application.yml",
                    "src/test/java/com/example/ApplicationTests.java",
                },
                FrameworkType.Quarkus => new[]
                {
                    "pom.xml",
                    "src/main/java/com/example/GreetingResource.java",
                    "src/main/resources/application.properties",
                    "src/test/java/com/example/GreetingResourceTest.java",
                },
                FrameworkType.Micronaut => new[]
                {
                    "build.gradle",
                    "src/main/java/com/example/Application.java",
                    "src/main/resources/application.yml",
                    "src/test/java/com/example/HelloControllerTest.java",
                },
                _ => new[] { "pom.xml", "src/main/java/com/example/Application.java" }
            },
            ArchitectureType.Python => fw switch
            {
                FrameworkType.FastAPI => new[]
                {
                    "requirements.txt",
                    "app/__init__.py", "app/main.py",
                    "app/api/v1/router.py",
                    "app/core/config.py",
                    "tests/__init__.py", "tests/test_main.py",
                },
                FrameworkType.Django => new[]
                {
                    "requirements.txt", "manage.py",
                    $"{name}/settings.py", $"{name}/urls.py", $"{name}/wsgi.py",
                    "apps/core/__init__.py", "apps/core/models.py", "apps/core/views.py",
                },
                FrameworkType.Flask => new[]
                {
                    "requirements.txt",
                    "app/__init__.py", "app/main.py",
                    "app/config.py",
                    "tests/__init__.py", "tests/test_app.py",
                },
                _ => new[] { "requirements.txt", "main.py" }
            },
            ArchitectureType.Php => fw switch
            {
                FrameworkType.Laravel => new[]
                {
                    "composer.json",
                    "artisan",
                    "app/Http/Controllers/Controller.php",
                    "app/Models/User.php",
                    "routes/web.php", "routes/api.php",
                    "database/migrations/.gitkeep",
                    ".env", ".env.example",
                },
                FrameworkType.Symfony => new[]
                {
                    "composer.json",
                    "bin/console",
                    "config/packages/framework.yaml",
                    "src/Controller/DefaultController.php",
                    "src/Entity/User.php",
                    "templates/base.html.twig",
                },
                _ => new[] { "composer.json", "public/index.php" }
            },
            ArchitectureType.JavaScript => fw switch
            {
                FrameworkType.NodeJs => new[] { "package.json", "src/index.js" },
                FrameworkType.ExpressJs => new[]
                {
                    "package.json",
                    "src/server.js",
                    "src/routes/index.js",
                    "src/middlewares/errorHandler.js",
                },
                FrameworkType.NestJs => new[]
                {
                    "package.json", "tsconfig.json", "nest-cli.json",
                    "src/main.js", "src/app.module.js",
                    "src/app.controller.js", "src/app.service.js",
                    "test/app.e2e-spec.js",
                },
                FrameworkType.NextJs => new[]
                {
                    "package.json", "next.config.js",
                    "src/app/page.js", "src/app/layout.js",
                    "src/app/globals.css",
                    "public/favicon.ico",
                },
                _ => new[] { "package.json", "src/index.js" }
            },
            ArchitectureType.TypeScript => fw switch
            {
                FrameworkType.NestTs => new[]
                {
                    "package.json", "tsconfig.json", "nest-cli.json",
                    "src/main.ts", "src/app.module.ts",
                    "src/app.controller.ts", "src/app.service.ts",
                    "test/app.e2e-spec.ts",
                },
                FrameworkType.NextTs => new[]
                {
                    "package.json", "tsconfig.json", "next.config.ts",
                    "src/app/page.tsx", "src/app/layout.tsx",
                    "src/app/globals.css",
                    "public/favicon.ico",
                },
                _ => new[] { "package.json", "tsconfig.json", "src/index.ts" }
            },
            _ => Array.Empty<string>()
        };

    private static IEnumerable<string> GetPatternFileList(
        ArchitectureType arch, FrameworkType fw, HashSet<string> patterns, string name)
    {
        var files = new List<string>();

        if (patterns.Contains("repository") || patterns.Contains("cleanarchitecture"))
        {
            files.AddRange(arch switch
            {
                ArchitectureType.DotNet => new[]
                {
                    $"src/{name}.Core/Entities/BaseEntity.cs",
                    $"src/{name}.Core/Interfaces/IRepository.cs",
                    $"src/{name}.Infrastructure/Repositories/BaseRepository.cs",
                    $"src/{name}.Application/Services/BaseService.cs",
                },
                ArchitectureType.Java => new[]
                {
                    "src/main/java/com/example/domain/repository/BaseRepository.java",
                    "src/main/java/com/example/infrastructure/persistence/JpaBaseRepository.java",
                },
                ArchitectureType.Python => new[]
                {
                    "app/domain/repositories/base_repository.py",
                    "app/infrastructure/repositories/sqlalchemy_repository.py",
                },
                ArchitectureType.Php => new[]
                {
                    "app/Repositories/BaseRepository.php",
                    "app/Interfaces/RepositoryInterface.php",
                },
                _ => Array.Empty<string>()
            });
        }

        if (patterns.Contains("cqrs"))
        {
            files.AddRange(arch switch
            {
                ArchitectureType.DotNet => new[]
                {
                    "Application/Commands/CreateSampleCommand.cs",
                    "Application/Queries/GetSampleQuery.cs",
                    "Application/Handlers/CreateSampleCommandHandler.cs",
                    "Application/Handlers/GetSampleQueryHandler.cs",
                },
                ArchitectureType.Java => new[]
                {
                    "src/main/java/com/example/command/CreateSampleCommand.java",
                    "src/main/java/com/example/query/GetSampleQuery.java",
                    "src/main/java/com/example/handler/CreateSampleCommandHandler.java",
                },
                ArchitectureType.Python => new[]
                {
                    "app/commands/create_sample.py",
                    "app/queries/get_sample.py",
                    "app/handlers/create_sample_handler.py",
                },
                _ => Array.Empty<string>()
            });
        }

        if (patterns.Contains("microservices"))
        {
            files.AddRange(new[]
            {
                "services/api-gateway/Dockerfile",
                "services/service-a/Dockerfile",
                "services/service-b/Dockerfile",
                "docker-compose.microservices.yml",
            });
        }

        if (patterns.Contains("hexagonalarchitecture"))
        {
            files.AddRange(arch switch
            {
                ArchitectureType.DotNet => new[]
                {
                    $"src/{name}.Domain/Ports/IUserPort.cs",
                    $"src/{name}.Application/UseCases/CreateUserUseCase.cs",
                    $"src/{name}.Infrastructure/Adapters/UserAdapter.cs",
                },
                ArchitectureType.Java => new[]
                {
                    "src/main/java/com/example/domain/port/UserPort.java",
                    "src/main/java/com/example/application/usecase/CreateUserUseCase.java",
                    "src/main/java/com/example/infrastructure/adapter/UserAdapter.java",
                },
                _ => Array.Empty<string>()
            });
        }

        if (patterns.Contains("eventsourcing"))
        {
            files.AddRange(arch switch
            {
                ArchitectureType.DotNet => new[]
                {
                    "Domain/Events/BaseEvent.cs",
                    "Domain/Aggregates/BaseAggregate.cs",
                    "Infrastructure/EventStore/EventStoreRepository.cs",
                },
                ArchitectureType.Java => new[]
                {
                    "src/main/java/com/example/domain/event/BaseEvent.java",
                    "src/main/java/com/example/domain/aggregate/BaseAggregate.java",
                },
                _ => Array.Empty<string>()
            });
        }

        return files;
    }

    private static string PreviewTok(string s) =>
        new string((s ?? string.Empty).Trim().ToLower().Where(char.IsLetterOrDigit).ToArray());
}