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
<<<<<<< HEAD
=======
            },
            // ── Java ──────────────────────────────────────────────────────────
            new ProjectTemplate
            {
                Id = 7, CreatedAt = SeedDate,
                Name = "Dockerfile Java Spring Boot", TemplateType = "dockerfile",
                Architecture = Core.Enums.ArchitectureType.Java,
                Description = "Multi-stage Dockerfile para Spring Boot con Maven",
                IsActive = true, Version = 1,
                Content =
                    "FROM eclipse-temurin:21-jdk-alpine AS build\n" +
                    "WORKDIR /app\nCOPY . .\n" +
                    "RUN ./mvnw -q package -DskipTests\n\n" +
                    "FROM eclipse-temurin:21-jre-alpine AS final\n" +
                    "WORKDIR /app\n" +
                    "COPY --from=build /app/target/*.jar app.jar\n" +
                    "EXPOSE 8080\n" +
                    "ENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]"
            },
            new ProjectTemplate
            {
                Id = 8, CreatedAt = SeedDate,
                Name = "Compose Java + PostgreSQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.Java,
                Database = Core.Enums.DatabaseType.PostgreSQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para Spring Boot + PostgreSQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - SPRING_DATASOURCE_URL=jdbc:postgresql://db:5432/{{DB_NAME}}\n" +
                    "      - SPRING_DATASOURCE_USERNAME=postgres\n      - SPRING_DATASOURCE_PASSWORD=secret\n" +
                    "      - SPRING_JPA_HIBERNATE_DDL_AUTO=update\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n\n" +
                    "  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n" +
                    "    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  pgdata:"
            },
            new ProjectTemplate
            {
                Id = 9, CreatedAt = SeedDate,
                Name = "Compose Java + MySQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.Java,
                Database = Core.Enums.DatabaseType.MySQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para Spring Boot + MySQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - SPRING_DATASOURCE_URL=jdbc:mysql://db:3306/{{DB_NAME}}\n" +
                    "      - SPRING_DATASOURCE_USERNAME=root\n      - SPRING_DATASOURCE_PASSWORD=secret\n" +
                    "      - SPRING_JPA_HIBERNATE_DDL_AUTO=update\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n\n" +
                    "  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n" +
                    "    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  mysqldata:"
            },
            new ProjectTemplate
            {
                Id = 10, CreatedAt = SeedDate,
                Name = ".gitignore Java", TemplateType = "gitignore",
                Architecture = Core.Enums.ArchitectureType.Java,
                Description = "Gitignore para proyectos Java/Maven/Spring Boot",
                IsActive = true, Version = 1,
                Content =
                    "target/\n*.class\n*.jar\n*.war\n*.ear\n.mvn/\n!.mvn/wrapper/\n" +
                    ".idea/\n*.iml\n.vscode/\n.env\n.env.*\n" +
                    "*.log\nspring-shell.log\nmvnw\n!mvnw\nmvnw.cmd\n!mvnw.cmd"
            },
            new ProjectTemplate
            {
                Id = 11, CreatedAt = SeedDate,
                Name = "CI Java GitHub Actions", TemplateType = "ci",
                Architecture = Core.Enums.ArchitectureType.Java,
                Description = "Pipeline CI/CD para Java + Maven con GitHub Actions",
                IsActive = true, Version = 1,
                Content =
                    "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  build:\n    runs-on: ubuntu-latest\n" +
                    "    steps:\n    - uses: actions/checkout@v4\n" +
                    "    - uses: actions/setup-java@v4\n      with:\n        java-version: '21'\n        distribution: 'temurin'\n        cache: maven\n" +
                    "    - run: ./mvnw -q verify\n" +
                    "    - name: Build Docker image\n      run: docker build -t {{APP_NAME}}:latest ."
            },
            // ── Python ────────────────────────────────────────────────────────
            new ProjectTemplate
            {
                Id = 12, CreatedAt = SeedDate,
                Name = "Dockerfile Python FastAPI", TemplateType = "dockerfile",
                Architecture = Core.Enums.ArchitectureType.Python,
                Description = "Dockerfile para FastAPI/Flask con Python 3.12",
                IsActive = true, Version = 1,
                Content =
                    "FROM python:3.12-slim AS base\n" +
                    "WORKDIR /app\n\n" +
                    "COPY requirements.txt .\n" +
                    "RUN pip install --no-cache-dir -r requirements.txt\n\n" +
                    "COPY . .\n" +
                    "EXPOSE 8080\n\n" +
                    "CMD [\"uvicorn\", \"main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8080\"]"
            },
            new ProjectTemplate
            {
                Id = 13, CreatedAt = SeedDate,
                Name = "Compose Python + PostgreSQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.Python,
                Database = Core.Enums.DatabaseType.PostgreSQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para FastAPI/Django + PostgreSQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - DATABASE_URL=postgresql://postgres:secret@db:5432/{{DB_NAME}}\n" +
                    "      - DEBUG=False\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n    volumes:\n      - .:/app\n\n" +
                    "  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n" +
                    "    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  pgdata:"
            },
            new ProjectTemplate
            {
                Id = 14, CreatedAt = SeedDate,
                Name = "Compose Python + MySQL", TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.Python,
                Database = Core.Enums.DatabaseType.MySQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para FastAPI/Django + MySQL",
                IsActive = true, Version = 1,
                Content =
                    "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                    "    environment:\n      - DATABASE_URL=mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n" +
                    "    depends_on:\n      db:\n        condition: service_healthy\n    volumes:\n      - .:/app\n\n" +
                    "  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n" +
                    "    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n" +
                    "    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\n" +
                    "volumes:\n  mysqldata:"
            },
            new ProjectTemplate
            {
                Id = 15, CreatedAt = SeedDate,
                Name = ".gitignore Python", TemplateType = "gitignore",
                Architecture = Core.Enums.ArchitectureType.Python,
                Description = "Gitignore para proyectos Python",
                IsActive = true, Version = 1,
                Content =
                    "__pycache__/\n*.py[cod]\n*.pyo\n.env\n.env.*\nvenv/\n.venv/\n" +
                    "*.egg-info/\ndist/\nbuild/\n.pytest_cache/\n.mypy_cache/\n" +
                    ".coverage\nhtmlcov/\n*.sqlite3\n.vscode/\n.idea/"
            },
            new ProjectTemplate
            {
                Id = 16, CreatedAt = SeedDate,
                Name = "CI Python GitHub Actions", TemplateType = "ci",
                Architecture = Core.Enums.ArchitectureType.Python,
                Description = "Pipeline CI/CD para Python con GitHub Actions",
                IsActive = true, Version = 1,
                Content =
                    "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  test:\n    runs-on: ubuntu-latest\n" +
                    "    steps:\n    - uses: actions/checkout@v4\n" +
                    "    - uses: actions/setup-python@v5\n      with:\n        python-version: '3.12'\n        cache: pip\n" +
                    "    - run: pip install -r requirements.txt\n" +
                    "    - run: python -m pytest tests/ -v\n" +
                    "    - name: Build Docker image\n      run: docker build -t {{APP_NAME}}:latest ."
>>>>>>> 0dc2a35 (complete java,python,typescript)
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
<<<<<<< HEAD
            new LibraryRecommendation { Id = 10, CreatedAt = SeedDate, Name = "Alembic",               PackageName = "alembic",                       Architecture = Core.Enums.ArchitectureType.Python,   Category = "Migrations",    Description = "Migraciones de base de datos para SQLAlchemy",         PopularityScore = 90, InstallCommand = "pip install alembic" }
=======
            new LibraryRecommendation { Id = 10, CreatedAt = SeedDate, Name = "Alembic",               PackageName = "alembic",                       Architecture = Core.Enums.ArchitectureType.Python,   Category = "Migrations",    Description = "Migraciones de base de datos para SQLAlchemy",         PopularityScore = 90, InstallCommand = "pip install alembic" },
            // Python extra
            new LibraryRecommendation { Id = 11, CreatedAt = SeedDate, Name = "pytest",                PackageName = "pytest",                        Architecture = Core.Enums.ArchitectureType.Python,   Category = "Testing",       Description = "Framework de testing para Python",                     PopularityScore = 98, InstallCommand = "pip install pytest" },
            new LibraryRecommendation { Id = 12, CreatedAt = SeedDate, Name = "httpx",                 PackageName = "httpx",                         Architecture = Core.Enums.ArchitectureType.Python,   Category = "HTTP Client",   Description = "Cliente HTTP async para tests e integraciones",        PopularityScore = 88, InstallCommand = "pip install httpx" },
            new LibraryRecommendation { Id = 13, CreatedAt = SeedDate, Name = "python-dotenv",         PackageName = "python-dotenv",                 Architecture = Core.Enums.ArchitectureType.Python,   Category = "Config",        Description = "Carga variables de entorno desde .env",               PopularityScore = 95, InstallCommand = "pip install python-dotenv" },
            // Java
            new LibraryRecommendation { Id = 14, CreatedAt = SeedDate, Name = "Spring Data JPA",       PackageName = "org.springframework.boot:spring-boot-starter-data-jpa", Architecture = Core.Enums.ArchitectureType.Java, Framework = Core.Enums.FrameworkType.SpringBoot, Category = "ORM", Description = "Repositorios JPA con Spring Data", PopularityScore = 99, InstallCommand = "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-data-jpa" },
            new LibraryRecommendation { Id = 15, CreatedAt = SeedDate, Name = "Spring Security",       PackageName = "org.springframework.boot:spring-boot-starter-security",  Architecture = Core.Enums.ArchitectureType.Java, Framework = Core.Enums.FrameworkType.SpringBoot, Category = "Security", Description = "Autenticación y autorización para Spring Boot", PopularityScore = 97, InstallCommand = "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-security" },
            new LibraryRecommendation { Id = 16, CreatedAt = SeedDate, Name = "MapStruct",             PackageName = "org.mapstruct:mapstruct",                               Architecture = Core.Enums.ArchitectureType.Java, Category = "Mapping",   Description = "Mapeo entre objetos Java en tiempo de compilación",  PopularityScore = 91, InstallCommand = "mvn dependency:get -Dartifact=org.mapstruct:mapstruct:1.5.5.Final" },
            new LibraryRecommendation { Id = 17, CreatedAt = SeedDate, Name = "Lombok",                PackageName = "org.projectlombok:lombok",                              Architecture = Core.Enums.ArchitectureType.Java, Category = "Boilerplate", Description = "Reduce boilerplate con anotaciones (@Getter, @Builder...)", PopularityScore = 98, InstallCommand = "mvn dependency:get -Dartifact=org.projectlombok:lombok:1.18.32" },
            new LibraryRecommendation { Id = 18, CreatedAt = SeedDate, Name = "springdoc-openapi",    PackageName = "org.springdoc:springdoc-openapi-starter-webmvc-ui",    Architecture = Core.Enums.ArchitectureType.Java, Framework = Core.Enums.FrameworkType.SpringBoot, Category = "Documentation", Description = "Swagger UI / OpenAPI 3 para Spring Boot 3", PopularityScore = 93, InstallCommand = "mvn dependency:get -Dartifact=org.springdoc:springdoc-openapi-starter-webmvc-ui:2.5.0" },
            new LibraryRecommendation { Id = 19, CreatedAt = SeedDate, Name = "JUnit 5",              PackageName = "org.junit.jupiter:junit-jupiter",                       Architecture = Core.Enums.ArchitectureType.Java, Category = "Testing",   Description = "Framework de testing unitario para Java",             PopularityScore = 99, InstallCommand = "mvn dependency:get -Dartifact=org.junit.jupiter:junit-jupiter:5.10.0" },
            new LibraryRecommendation { Id = 20, CreatedAt = SeedDate, Name = "Flyway",               PackageName = "org.flywaydb:flyway-core",                              Architecture = Core.Enums.ArchitectureType.Java, Category = "Migrations", Description = "Migraciones de base de datos para Java",             PopularityScore = 92, InstallCommand = "mvn dependency:get -Dartifact=org.flywaydb:flyway-core:10.0.0" },
            // TypeScript
            new LibraryRecommendation { Id = 21, CreatedAt = SeedDate, Name = "Zod",              PackageName = "zod",              Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "Validation",   Description = "Validación de esquemas con inferencia de tipos TypeScript",    PopularityScore = 97, InstallCommand = "npm install zod" },
            new LibraryRecommendation { Id = 22, CreatedAt = SeedDate, Name = "Prisma",           PackageName = "prisma",           Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "ORM",          Description = "ORM moderno con tipado automático para TypeScript/Node.js",   PopularityScore = 96, InstallCommand = "npm install prisma @prisma/client" },
            new LibraryRecommendation { Id = 23, CreatedAt = SeedDate, Name = "TypeORM",          PackageName = "typeorm",          Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "ORM",          Description = "ORM basado en decoradores, nativo para TypeScript y NestJS",  PopularityScore = 91, InstallCommand = "npm install typeorm reflect-metadata" },
            new LibraryRecommendation { Id = 24, CreatedAt = SeedDate, Name = "Jest",             PackageName = "jest",             Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "Testing",      Description = "Framework de testing rápido con soporte nativo para TypeScript", PopularityScore = 98, InstallCommand = "npm install --save-dev jest ts-jest @types/jest" },
            new LibraryRecommendation { Id = 25, CreatedAt = SeedDate, Name = "RxJS",             PackageName = "rxjs",             Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "Reactive",     Description = "Librería para programación reactiva y manejo de streams",     PopularityScore = 94, InstallCommand = "npm install rxjs" },
            new LibraryRecommendation { Id = 26, CreatedAt = SeedDate, Name = "class-validator",  PackageName = "class-validator",  Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "Validation",   Description = "Decoradores de validación para clases TypeScript (ideal NestJS)", PopularityScore = 93, InstallCommand = "npm install class-validator class-transformer" },
            new LibraryRecommendation { Id = 27, CreatedAt = SeedDate, Name = "dotenv",           PackageName = "dotenv",           Architecture = Core.Enums.ArchitectureType.TypeScript, Category = "Config",       Description = "Carga variables de entorno desde .env en Node.js/TypeScript", PopularityScore = 99, InstallCommand = "npm install dotenv" }
>>>>>>> 0dc2a35 (complete java,python,typescript)
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
<<<<<<< HEAD
            new DesignPatternEntry { Id = 6, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Microservices,     Name = "Microservices",      Architecture = Core.Enums.ArchitectureType.JavaScript,   Description = "Arquitectura de microservicios para aplicaciones Node.js." }
=======
            new DesignPatternEntry { Id = 6, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Microservices,     Name = "Microservices",      Architecture = Core.Enums.ArchitectureType.JavaScript,   Description = "Arquitectura de microservicios para aplicaciones Node.js." },
            // Java
            new DesignPatternEntry { Id = 7, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.Java,        Description = "Patrón de repositorio con Spring Data JPA para Java." },
            new DesignPatternEntry { Id = 8, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CQRS,              Name = "CQRS",               Architecture = Core.Enums.ArchitectureType.Java,        Description = "Separación de comandos y consultas en aplicaciones Spring Boot." },
            new DesignPatternEntry { Id = 9, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.HexagonalArchitecture, Name = "Hexagonal Architecture", Architecture = Core.Enums.ArchitectureType.Java, Description = "Ports & Adapters: aísla el dominio de la infraestructura. Muy usado en Java empresarial." },
            new DesignPatternEntry { Id = 10, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Microservices,    Name = "Microservices",      Architecture = Core.Enums.ArchitectureType.Java,        Description = "Arquitectura de microservicios con Spring Boot y Spring Cloud." },
            // Python extra
            new DesignPatternEntry { Id = 11, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CleanArchitecture, Name = "Clean Architecture", Architecture = Core.Enums.ArchitectureType.Python,    Description = "Capas bien definidas (domain, application, infrastructure) para proyectos Python." },
            // TypeScript
            new DesignPatternEntry { Id = 12, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.HexagonalArchitecture, Name = "Hexagonal Architecture (Ports & Adapters)", Architecture = Core.Enums.ArchitectureType.TypeScript, Description = "Aísla el dominio de la infraestructura con puertos y adaptadores. Ideal para NestJS." },
            new DesignPatternEntry { Id = 13, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Component-Based Architecture",    Architecture = Core.Enums.ArchitectureType.TypeScript, Description = "Organización basada en componentes reutilizables, clave en Next.js y NestJS." },
            new DesignPatternEntry { Id = 14, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.MVVM,              Name = "Observer Pattern",                Architecture = Core.Enums.ArchitectureType.TypeScript, Description = "Comunicación reactiva entre componentes desacoplados usando observadores." },
            new DesignPatternEntry { Id = 15, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Mediator,          Name = "Mediator Pattern",                Architecture = Core.Enums.ArchitectureType.TypeScript, Description = "Centraliza la comunicación entre módulos TypeScript evitando dependencias directas." },
            new DesignPatternEntry { Id = 16, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CleanArchitecture, Name = "SOLID Principles Integration",    Architecture = Core.Enums.ArchitectureType.TypeScript, Description = "Aplicación de los 5 principios SOLID al diseño de módulos TypeScript/NestJS." }
>>>>>>> 0dc2a35 (complete java,python,typescript)
        );
    }
}
