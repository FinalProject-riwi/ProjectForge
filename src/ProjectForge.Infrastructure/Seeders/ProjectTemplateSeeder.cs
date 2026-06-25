using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Seeders;

public static class ProjectTemplateSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<ProjectTemplate>().HasData(
            new ProjectTemplate
            {
                Id = 1, CreatedAt = SeedDate,
                Name = "Dockerfile .NET", TemplateType = "dockerfile",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Description = "Multi-stage Dockerfile para ASP.NET Core",
                IsActive = true, Version = 1,
                Content =
                    "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\n" +
                    "WORKDIR /app\nEXPOSE 8080\n\n" +
                    "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\n" +
                    "WORKDIR /src\nCOPY . .\n" +
                    "RUN dotnet restore\n" +
                    "RUN dotnet publish -c Release -o /app/publish\n\n" +
                    "FROM base AS final\nWORKDIR /app\n" +
                    "COPY --from=build /app/publish .\n" +
                    "ENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]"
            },
            new ProjectTemplate
            {
                Id = 2, CreatedAt = SeedDate,
                Name = "Compose .NET + PostgreSQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Database = Core.Enums.DatabaseType.PostgreSQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para .NET + PostgreSQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n\n" +
                    "  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n" +
                    "    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  pgdata:"
            },
            new ProjectTemplate
            {
                Id = 3, CreatedAt = SeedDate,
                Name = "Compose .NET + MySQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Database = Core.Enums.DatabaseType.MySQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para .NET + MySQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};User=root;Password=secret;\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n\n" +
                    "  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n" +
                    "    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  mysqldata:"
            },
            new ProjectTemplate
            {
                Id = 4, CreatedAt = SeedDate,
                Name = "K8s App Deployment", TemplateType = "k8s-deployment",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Infrastructure = Core.Enums.InfrastructureType.Kubernetes,
                Description = "Kubernetes Deployment para la aplicación",
                IsActive = true, Version = 1,
                Content =
                    "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n" +
                    "  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n" +
                    "    spec:\n      containers:\n      - name: {{APP_NAME}}\n        image: {{APP_NAME}}:latest\n        ports:\n        - containerPort: 8080\n" +
                    "---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\n" +
                    "spec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n  - port: 80\n    targetPort: 8080\n  type: LoadBalancer"
            },
            new ProjectTemplate
            {
                Id = 5, CreatedAt = SeedDate,
                Name = ".gitignore .NET", TemplateType = "gitignore",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Description = "Gitignore para proyectos .NET",
                IsActive = true, Version = 1,
                Content = "[Dd]ebug/\n[Rr]elease/\n[Bb]in/\n[Oo]bj/\n*.user\n*.suo\n*.vs/\n.vscode/\n.env\n.env.*\n*.pfx\npackages/"
            },
            new ProjectTemplate
            {
                Id = 6, CreatedAt = SeedDate,
                Name = "CI .NET GitHub Actions", TemplateType = "ci",
                Architecture = Core.Enums.ArchitectureType.DotNet,
                Description = "Pipeline CI/CD para .NET con GitHub Actions",
                IsActive = true, Version = 1,
                Content =
                    "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  build:\n    runs-on: ubuntu-latest\n" +
                    "    steps:\n    - uses: actions/checkout@v4\n" +
                    "    - uses: actions/setup-dotnet@v4\n      with:\n        dotnet-version: '10.0.x'\n" +
                    "    - run: dotnet restore\n    - run: dotnet build --no-restore\n    - run: dotnet test --no-build"
            }
        );
    }
}
