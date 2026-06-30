using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5000-5029
    private static IEnumerable<ProjectTemplate> GetDotNetTemplates()
    {
        // ── Framework-specific Dockerfiles ──────────────────────────────────────
        yield return T(5000, "Dockerfile .NET / AspNetCoreMVC", "dockerfile", ArchitectureType.DotNet,
            framework: FrameworkType.AspNetCoreMVC,
            desc: "Multi-stage Dockerfile para ASP.NET Core MVC",
            content:
            "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\n" +
            "WORKDIR /app\nEXPOSE 8080\n\n" +
            "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
            "WORKDIR /src\nCOPY *.csproj .\nRUN dotnet restore\nCOPY . .\n" +
            "RUN dotnet publish -c Release -o /app/publish\n\n" +
            "FROM base AS final\nWORKDIR /app\n" +
            "COPY --from=build /app/publish .\n" +
            "ENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]");

        yield return T(5001, "Dockerfile .NET / BlazorServer", "dockerfile", ArchitectureType.DotNet,
            framework: FrameworkType.BlazorServer,
            desc: "Multi-stage Dockerfile para Blazor Server",
            content:
            "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\n" +
            "WORKDIR /app\nEXPOSE 8080\n\n" +
            "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
            "WORKDIR /src\nCOPY *.csproj .\nRUN dotnet restore\nCOPY . .\n" +
            "RUN dotnet publish -c Release -o /app/publish\n\n" +
            "FROM base AS final\nWORKDIR /app\n" +
            "COPY --from=build /app/publish .\n" +
            "ENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]");

        yield return T(5002, "Dockerfile .NET / BlazorWasm", "dockerfile", ArchitectureType.DotNet,
            framework: FrameworkType.BlazorWasm,
            desc: "Dockerfile Blazor WebAssembly servido con nginx",
            content:
            "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
            "WORKDIR /src\nCOPY *.csproj .\nRUN dotnet restore\nCOPY . .\n" +
            "RUN dotnet publish -c Release -o /app/publish\n\n" +
            "FROM nginx:alpine AS final\n" +
            "COPY --from=build /app/publish/wwwroot /usr/share/nginx/html\n" +
            "COPY nginx.conf /etc/nginx/nginx.conf\n" +
            "EXPOSE 80\nCMD [\"nginx\", \"-g\", \"daemon off;\"]");

        yield return T(5003, "Dockerfile .NET / MinimalApi", "dockerfile", ArchitectureType.DotNet,
            framework: FrameworkType.MinimalApi,
            desc: "Dockerfile optimizado para Minimal API .NET",
            content:
            "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\n" +
            "WORKDIR /app\nEXPOSE 8080\n\n" +
            "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
            "WORKDIR /src\nCOPY *.csproj .\nRUN dotnet restore\nCOPY . .\n" +
            "RUN dotnet publish -c Release -o /app/publish\n\n" +
            "FROM base AS final\nWORKDIR /app\n" +
            "COPY --from=build /app/publish .\n" +
            "ENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]");

        // ── Compose: nuevas DBs (ya existen PG, MySQL, MSSQL) ───────────────────
        yield return T(5004, "Compose .NET + MongoDB", "compose", ArchitectureType.DotNet,
            db: DatabaseType.MongoDB, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para .NET + MongoDB",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
            "    environment:\n      - ConnectionStrings__MongoDB=mongodb://admin:secret@db:27017/{{DB_NAME}}\n" +
            "    depends_on:\n      - db\n" +
            DbServiceBlock(DatabaseType.MongoDB));

        yield return T(5005, "Compose .NET + Redis", "compose", ArchitectureType.DotNet,
            db: DatabaseType.Redis, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para .NET + Redis",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
            "    environment:\n      - ConnectionStrings__Redis=db:6379\n" +
            "    depends_on:\n      db:\n        condition: service_healthy\n" +
            DbServiceBlock(DatabaseType.Redis));

        yield return T(5006, "Compose .NET + SQLite", "compose", ArchitectureType.DotNet,
            db: DatabaseType.SQLite, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para .NET con SQLite embebido",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
            "    environment:\n      - ConnectionStrings__Default=Data Source=/app/data/{{DB_NAME}}.db\n" +
            "    volumes:\n      - sqlitedata:/app/data\nvolumes:\n  sqlitedata:");

        // ── K8s por base de datos ────────────────────────────────────────────────
        var dotnetK8s = new[]
        {
            (DatabaseType.PostgreSQL, "ConnectionStrings__Default", "Host=postgres-svc;Database={{DB_NAME}};Username=postgres;Password=secret"),
            (DatabaseType.MySQL,      "ConnectionStrings__Default", "Server=mysql-svc;Database={{DB_NAME}};Uid=root;Pwd=secret;"),
            (DatabaseType.SqlServer,  "ConnectionStrings__Default", "Server=sqlserver-svc,1433;Database={{DB_NAME}};User Id=sa;Password=Secret1234!;"),
            (DatabaseType.MongoDB,    "ConnectionStrings__MongoDB",  "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "ConnectionStrings__Redis",    "redis-svc:6379"),
            (DatabaseType.SQLite,     "ConnectionStrings__Default", "Data Source=/data/{{DB_NAME}}.db"),
        };
        int id = 5007;
        foreach (var (db, envKey, connStr) in dotnetK8s)
        {
            yield return T(id++, $"K8s .NET + {db}", "k8s-deployment", ArchitectureType.DotNet,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para .NET con {db}",
                content: K8sManifest(envKey, connStr, 8080));
        }

        // ── .gitignore ───────────────────────────────────────────────────────────
        yield return T(5013, ".gitignore .NET", "gitignore", ArchitectureType.DotNet,
            desc: "Gitignore estándar para proyectos .NET",
            content:
            "bin/\nobj/\n*.user\n.vs/\n*.suo\n*.userprefs\nTestResults/\n*.nupkg\n.env\n.env.*\n" +
            "/publish/\n/artifacts/\n*.db\n*.db-shm\n*.db-wal\n.DS_Store\nThumbs.db");
    }

    // ─── Additional DotNet Libraries (IDs 11000-11009) ───────────────────────────
    private static IEnumerable<LibraryRecommendation> GetDotNetLibraries() => new[]
    {
        new LibraryRecommendation { Id=11000, CreatedAt=SeedDate, Name="MongoDB.Driver", PackageName="MongoDB.Driver", Architecture=ArchitectureType.DotNet, Category="Database", Description="Driver oficial de MongoDB para .NET", PopularityScore=90, InstallCommand="dotnet add package MongoDB.Driver" },
        new LibraryRecommendation { Id=11001, CreatedAt=SeedDate, Name="Microsoft.AspNetCore.Authentication.JwtBearer", PackageName="Microsoft.AspNetCore.Authentication.JwtBearer", Architecture=ArchitectureType.DotNet, Category="Auth", Description="Autenticación JWT para ASP.NET Core", PopularityScore=97, InstallCommand="dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer" },
        new LibraryRecommendation { Id=11002, CreatedAt=SeedDate, Name="OpenTelemetry", PackageName="OpenTelemetry.Extensions.Hosting", Architecture=ArchitectureType.DotNet, Category="Observability", Description="Trazabilidad distribuida y métricas para .NET", PopularityScore=88, InstallCommand="dotnet add package OpenTelemetry.Extensions.Hosting" },
        new LibraryRecommendation { Id=11003, CreatedAt=SeedDate, Name="MassTransit", PackageName="MassTransit.RabbitMQ", Architecture=ArchitectureType.DotNet, Category="Messaging", Description="Bus de mensajes con soporte RabbitMQ/Azure SB", PopularityScore=87, InstallCommand="dotnet add package MassTransit.RabbitMQ" },
        new LibraryRecommendation { Id=11004, CreatedAt=SeedDate, Name="Grpc.AspNetCore", PackageName="Grpc.AspNetCore", Architecture=ArchitectureType.DotNet, Category="RPC", Description="gRPC para servicios ASP.NET Core", PopularityScore=85, InstallCommand="dotnet add package Grpc.AspNetCore" },
        new LibraryRecommendation { Id=11005, CreatedAt=SeedDate, Name="Marten", PackageName="Marten", Architecture=ArchitectureType.DotNet, Category="Event Sourcing", Description="Event Sourcing y document DB sobre PostgreSQL", PopularityScore=83, InstallCommand="dotnet add package Marten" },
        new LibraryRecommendation { Id=11006, CreatedAt=SeedDate, Name="Microsoft.EntityFrameworkCore.Sqlite", PackageName="Microsoft.EntityFrameworkCore.Sqlite", Architecture=ArchitectureType.DotNet, Category="Database", Description="Provider SQLite para EF Core", PopularityScore=88, InstallCommand="dotnet add package Microsoft.EntityFrameworkCore.Sqlite" },
        new LibraryRecommendation { Id=11007, CreatedAt=SeedDate, Name="Microsoft.EntityFrameworkCore.SqlServer", PackageName="Microsoft.EntityFrameworkCore.SqlServer", Architecture=ArchitectureType.DotNet, Category="Database", Description="Provider SQL Server para EF Core", PopularityScore=96, InstallCommand="dotnet add package Microsoft.EntityFrameworkCore.SqlServer" },
        new LibraryRecommendation { Id=11008, CreatedAt=SeedDate, Name="NSwag.AspNetCore", PackageName="NSwag.AspNetCore", Architecture=ArchitectureType.DotNet, Category="Documentation", Description="Alternativa a Swashbuckle para OpenAPI", PopularityScore=84, InstallCommand="dotnet add package NSwag.AspNetCore" },
        new LibraryRecommendation { Id=11009, CreatedAt=SeedDate, Name="FluentResults", PackageName="FluentResults", Architecture=ArchitectureType.DotNet, Category="Error Handling", Description="Result pattern con errores tipados para .NET", PopularityScore=80, InstallCommand="dotnet add package FluentResults" },
    };

    // ─── Template factory helper ──────────────────────────────────────────────────
    private static ProjectTemplate T(int id, string name, string type, ArchitectureType arch,
        string content, string? desc = null,
        FrameworkType? framework = null, DatabaseType? db = null, InfrastructureType? infra = null) =>
        new()
        {
            Id = id, CreatedAt = SeedDate, Name = name, TemplateType = type,
            Architecture = arch, Framework = framework, Database = db,
            Infrastructure = infra, Description = desc, IsActive = true, Version = 1, Content = content
        };
}