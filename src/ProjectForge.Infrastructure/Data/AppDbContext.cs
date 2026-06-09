using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WizardConfig> WizardConfigs => Set<WizardConfig>();
    public DbSet<ProjectTemplate> Templates => Set<ProjectTemplate>();
    public DbSet<LibraryRecommendation> Libraries => Set<LibraryRecommendation>();
    public DbSet<DesignPatternEntry> DesignPatterns => Set<DesignPatternEntry>();
    public DbSet<ProjectLog> ProjectLogs => Set<ProjectLog>();
    public DbSet<VpsCredential> VpsCredentials => Set<VpsCredential>();
    public DbSet<AiSuggestionCache> AiSuggestionCaches => Set<AiSuggestionCache>();

    // Fecha fija para todos los seeds — requerido por EF Core 10 (no valores dinámicos)
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.GitHubId).IsUnique();
            e.Property(u => u.AccessToken).HasMaxLength(512);
        });

        // Project
        modelBuilder.Entity<Project>(e =>
        {
            e.HasOne(p => p.User).WithMany(u => u.Projects).HasForeignKey(p => p.UserId);
            e.HasOne(p => p.WizardConfig).WithOne().HasForeignKey<Project>(p => p.WizardConfigId);
            e.HasMany(p => p.Logs).WithOne(l => l.Project).HasForeignKey(l => l.ProjectId);
            e.Property(p => p.Status).HasConversion<string>();
        });

        // WizardConfig
        modelBuilder.Entity<WizardConfig>(e =>
        {
            e.Property(w => w.Architecture).HasConversion<string>();
            e.Property(w => w.Framework).HasConversion<string>();
            e.Property(w => w.Database).HasConversion<string>();
            e.Property(w => w.Infrastructure).HasConversion<string>();
            e.Property(w => w.DeploymentTarget).HasConversion<string>();
            e.HasMany(w => w.VpsCredentials).WithOne(v => v.WizardConfig).HasForeignKey(v => v.WizardConfigId);
        });

        // ProjectTemplate
        modelBuilder.Entity<ProjectTemplate>(e =>
        {
            e.Property(t => t.Architecture).HasConversion<string>();
            e.Property(t => t.Framework).HasConversion<string>();
            e.Property(t => t.Database).HasConversion<string>();
            e.Property(t => t.Infrastructure).HasConversion<string>();
            e.HasIndex(t => new { t.Architecture, t.TemplateType, t.Database, t.Infrastructure });
        });

        // LibraryRecommendation — mantener la navegación SuggestedWithPatterns
        // para que EF genere la misma shadow property LibraryRecommendationId que ya está en la migración
        modelBuilder.Entity<LibraryRecommendation>(e =>
        {
            e.Property(l => l.Architecture).HasConversion<string>();
            e.Property(l => l.Framework).HasConversion<string>();
        });

        // DesignPatternEntry — tiene FK opcional a LibraryRecommendation (shadow property)
        modelBuilder.Entity<DesignPatternEntry>(e =>
        {
            e.Property(d => d.Pattern).HasConversion<string>();
            e.Property(d => d.Architecture).HasConversion<string>();
        });

        // VpsCredential
        modelBuilder.Entity<VpsCredential>(e =>
        {
            e.Property(v => v.EncryptedPassword).HasMaxLength(1024);
        });

        // AiSuggestionCache
        modelBuilder.Entity<AiSuggestionCache>(e =>
        {
            e.HasIndex(c => c.CacheKey).IsUnique();
        });

        SeedTemplates(modelBuilder);
        SeedLibraries(modelBuilder);
        SeedPatterns(modelBuilder);
    }

    private static void SeedTemplates(ModelBuilder mb)
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

    private static void SeedLibraries(ModelBuilder mb)
    {
        mb.Entity<LibraryRecommendation>().HasData(
            new LibraryRecommendation { Id = 1,  CreatedAt = SeedDate, Name = "MediatR",              PackageName = "MediatR",                       Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "CQRS/Mediator",  Description = "Implementación del patrón Mediator para CQRS",          PopularityScore = 95, InstallCommand = "dotnet add package MediatR" },
            new LibraryRecommendation { Id = 2,  CreatedAt = SeedDate, Name = "Entity Framework Core", PackageName = "Microsoft.EntityFrameworkCore",  Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "ORM",           Description = "ORM oficial de Microsoft para .NET",                   PopularityScore = 99, InstallCommand = "dotnet add package Microsoft.EntityFrameworkCore" },
            new LibraryRecommendation { Id = 3,  CreatedAt = SeedDate, Name = "FluentValidation",      PackageName = "FluentValidation.AspNetCore",    Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Validation",    Description = "Validación fluida y expresiva",                        PopularityScore = 92, InstallCommand = "dotnet add package FluentValidation.AspNetCore" },
            new LibraryRecommendation { Id = 4,  CreatedAt = SeedDate, Name = "Serilog",               PackageName = "Serilog.AspNetCore",             Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Logging",       Description = "Logging estructurado para .NET",                       PopularityScore = 97, InstallCommand = "dotnet add package Serilog.AspNetCore" },
            new LibraryRecommendation { Id = 5,  CreatedAt = SeedDate, Name = "AutoMapper",            PackageName = "AutoMapper",                    Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Mapping",       Description = "Mapeo automático entre objetos",                       PopularityScore = 94, InstallCommand = "dotnet add package AutoMapper" },
            new LibraryRecommendation { Id = 6,  CreatedAt = SeedDate, Name = "Swashbuckle (Swagger)", PackageName = "Swashbuckle.AspNetCore",         Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "Documentation", Description = "Generación automática de documentación OpenAPI",        PopularityScore = 98, InstallCommand = "dotnet add package Swashbuckle.AspNetCore" },
            new LibraryRecommendation { Id = 7,  CreatedAt = SeedDate, Name = "xUnit",                 PackageName = "xunit",                         Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Testing",       Description = "Framework de testing unitario para .NET",              PopularityScore = 96, InstallCommand = "dotnet add package xunit" },
            new LibraryRecommendation { Id = 8,  CreatedAt = SeedDate, Name = "SQLAlchemy",            PackageName = "sqlalchemy",                    Architecture = Core.Enums.ArchitectureType.Python,   Category = "ORM",           Description = "ORM más popular para Python",                          PopularityScore = 98, InstallCommand = "pip install sqlalchemy" },
            new LibraryRecommendation { Id = 9,  CreatedAt = SeedDate, Name = "Pydantic",              PackageName = "pydantic",                      Architecture = Core.Enums.ArchitectureType.Python,   Category = "Validation",    Description = "Validación de datos con type hints",                   PopularityScore = 97, InstallCommand = "pip install pydantic" },
            new LibraryRecommendation { Id = 10, CreatedAt = SeedDate, Name = "Alembic",               PackageName = "alembic",                       Architecture = Core.Enums.ArchitectureType.Python,   Category = "Migrations",    Description = "Migraciones de base de datos para SQLAlchemy",         PopularityScore = 90, InstallCommand = "pip install alembic" }
        );
    }

    private static void SeedPatterns(ModelBuilder mb)
    {
        mb.Entity<DesignPatternEntry>().HasData(
            new DesignPatternEntry { Id = 1, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Abstrae el acceso a datos detrás de interfaces, facilitando testing y mantenimiento." },
            new DesignPatternEntry { Id = 2, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CQRS,              Name = "CQRS",               Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Separa las operaciones de lectura (Queries) de las de escritura (Commands)." },
            new DesignPatternEntry { Id = 3, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CleanArchitecture, Name = "Clean Architecture", Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Arquitectura en capas concéntricas con dependencias hacia el centro." },
            new DesignPatternEntry { Id = 4, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Mediator,          Name = "Mediator",           Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Reduce el acoplamiento directo entre componentes usando un mediador." },
            new DesignPatternEntry { Id = 5, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.Python,      Description = "Patrón de repositorio adaptado para Python/FastAPI." },
            new DesignPatternEntry { Id = 6, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Microservices,     Name = "Microservices",      Architecture = Core.Enums.ArchitectureType.JavaScript,   Description = "Arquitectura de microservicios para aplicaciones Node.js." }
        );
    }
}
