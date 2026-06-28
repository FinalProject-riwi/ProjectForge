using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.DotNet;

public static class DotNetSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<ProjectTemplate>().HasData(GetTemplates().ToArray());
        mb.Entity<LibraryRecommendation>().HasData(GetLibraries().ToArray());
        mb.Entity<DesignPatternEntry>().HasData(GetDesignPatterns().ToArray());
    }

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // Use raw SQL with IDENTITY_INSERT to safely insert explicit IDs
        await UpsertTemplatesAsync(db, ct);
        await UpsertLibrariesAsync(db, ct);
        await UpsertPatternsAsync(db, ct);
        await FixSharedLibraryFrameworksAsync(db, ct);
    }

    /// <summary>
    /// Sets Framework = NULL for DotNet libraries that are cross-framework
    /// (EF Core, MediatR, Serilog, etc. work on any ASP.NET Core framework variant).
    /// </summary>
    private static async Task FixSharedLibraryFrameworksAsync(AppDbContext db, CancellationToken ct)
    {
        // Package names that should NOT be restricted to a specific framework
        var sharedPackages = new[]
        {
            "MediatR",
            "Microsoft.EntityFrameworkCore",
            "Serilog.AspNetCore",
            "AutoMapper",
            "xunit",
            "NUnit",
            "Bogus",
            "Polly",
            "Dapper",
            "StackExchange.Redis",
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "Pomelo.EntityFrameworkCore.MySql",
            "Hangfire.AspNetCore",
        };

        foreach (var pkg in sharedPackages)
        {
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE [Libraries] SET [Framework] = NULL WHERE [Architecture] = N'DotNet' AND [PackageName] = N'{pkg}' AND [Framework] IS NOT NULL",
                ct);
        }
    }

    private static async Task UpsertTemplatesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var t in GetTemplates())
        {
            if (await db.Templates.AnyAsync(x => x.Id == t.Id || (x.Architecture == t.Architecture && x.TemplateType == t.TemplateType && x.Name == t.Name), ct))
                continue;
            var sql = $"""
                SET IDENTITY_INSERT [Templates] ON;
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Description],[IsActive],[Name],[TemplateType],[Version])
                VALUES ({t.Id},N'{t.Architecture}',N'{t.Content.Replace("'","''")}','{t.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(t.Description ?? "").Replace("'","''")}',1,N'{t.Name.Replace("'","''")}',N'{t.TemplateType}',{t.Version});
                SET IDENTITY_INSERT [Templates] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    private static async Task UpsertLibrariesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var l in GetLibraries())
        {
            if (await db.Libraries.AnyAsync(x => x.Id == l.Id || (x.Architecture == l.Architecture && x.PackageName == l.PackageName), ct))
                continue;
            var fw = l.Framework.HasValue ? $"N'{l.Framework}'" : "NULL";
            var sql = $"""
                SET IDENTITY_INSERT [Libraries] ON;
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES ({l.Id},N'{l.Architecture}',N'{(l.Category ?? "").Replace("'","''")}','{l.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(l.Description ?? "").Replace("'","''")}',{fw},N'{(l.InstallCommand ?? "").Replace("'","''")}',N'{l.Name.Replace("'","''")}',N'{l.PackageName.Replace("'","''")}',{l.PopularityScore});
                SET IDENTITY_INSERT [Libraries] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    private static async Task UpsertPatternsAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var p in GetDesignPatterns())
        {
            if (await db.DesignPatterns.AnyAsync(x => x.Id == p.Id || (x.Architecture == p.Architecture && x.Pattern == p.Pattern && x.Name == p.Name), ct))
                continue;
            var notes = (p.ImplementationNotes ?? "").Replace("'","''");
            var cmds  = (p.ScaffoldCommandsJson ?? "[]").Replace("'","''");
            var sql = $"""
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES ({p.Id},N'{p.Architecture}','{p.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(p.Description ?? "").Replace("'","''")}',N'{notes}',N'{p.Name.Replace("'","''")}',N'{p.Pattern}',N'{cmds}');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    public static IEnumerable<ProjectTemplate> GetTemplates() => new[]
    {
        new ProjectTemplate { Id=1, CreatedAt=SeedDate, Name="Dockerfile .NET", TemplateType="dockerfile", Architecture=ArchitectureType.DotNet, Description="Multi-stage Dockerfile para ASP.NET Core", IsActive=true, Version=1,
            Content="FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\nWORKDIR /app\nEXPOSE 8080\n\nFROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\nWORKDIR /src\nCOPY . .\nRUN dotnet restore\nRUN dotnet publish -c Release -o /app/publish\n\nFROM base AS final\nWORKDIR /app\nCOPY --from=build /app/publish .\nENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]" },
        new ProjectTemplate { Id=2, CreatedAt=SeedDate, Name="Compose .NET + PostgreSQL", TemplateType="compose", Architecture=ArchitectureType.DotNet, Database=DatabaseType.PostgreSQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para .NET + PostgreSQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:" },
        new ProjectTemplate { Id=3, CreatedAt=SeedDate, Name="Compose .NET + MySQL", TemplateType="compose", Architecture=ArchitectureType.DotNet, Database=DatabaseType.MySQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para .NET + MySQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:" },
        new ProjectTemplate { Id=4, CreatedAt=SeedDate, Name="Compose .NET + SQL Server", TemplateType="compose", Architecture=ArchitectureType.DotNet, Database=DatabaseType.SqlServer, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para .NET + SQL Server", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=Secret1234!;\n    depends_on:\n      - db\n  db:\n    image: mcr.microsoft.com/mssql/server:2022-latest\n    environment:\n      ACCEPT_EULA: Y\n      SA_PASSWORD: Secret1234!\n    ports:\n      - \"{{DB_PORT}}:1433\"\n    volumes:\n      - mssqldata:/var/opt/mssql\nvolumes:\n  mssqldata:" },
        new ProjectTemplate { Id=5, CreatedAt=SeedDate, Name="K8s .NET Deployment", TemplateType="k8s-deployment", Architecture=ArchitectureType.DotNet, Infrastructure=InfrastructureType.Kubernetes, Description="Kubernetes Deployment para .NET", IsActive=true, Version=1,
            Content="apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n        - name: {{APP_NAME}}\n          image: {{APP_NAME}}:latest\n          ports:\n            - containerPort: 8080\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n    - port: 80\n      targetPort: 8080\n  type: LoadBalancer" },
        new ProjectTemplate { Id=6, CreatedAt=SeedDate, Name="CI .NET GitHub Actions", TemplateType="ci", Architecture=ArchitectureType.DotNet, Description="Pipeline CI para .NET", IsActive=true, Version=1,
            Content="name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '10.0.x'\n      - run: dotnet restore\n      - run: dotnet build --no-restore\n      - run: dotnet test --no-build --verbosity normal" },
    };

    public static IEnumerable<LibraryRecommendation> GetLibraries() => new[]
    {
        new LibraryRecommendation { Id=1, CreatedAt=SeedDate, Name="MediatR", PackageName="MediatR", Architecture=ArchitectureType.DotNet, Framework=FrameworkType.AspNetCoreWebApi, Category="CQRS/Mediator", Description="Implementación del patrón Mediator para CQRS", PopularityScore=95, InstallCommand="dotnet add package MediatR" },
        new LibraryRecommendation { Id=2, CreatedAt=SeedDate, Name="Entity Framework Core", PackageName="Microsoft.EntityFrameworkCore", Architecture=ArchitectureType.DotNet, Framework=FrameworkType.AspNetCoreWebApi, Category="ORM", Description="ORM oficial de Microsoft para .NET", PopularityScore=99, InstallCommand="dotnet add package Microsoft.EntityFrameworkCore" },
        new LibraryRecommendation { Id=3, CreatedAt=SeedDate, Name="FluentValidation", PackageName="FluentValidation.AspNetCore", Architecture=ArchitectureType.DotNet, Category="Validation", Description="Validación fluida y expresiva", PopularityScore=92, InstallCommand="dotnet add package FluentValidation.AspNetCore" },
        new LibraryRecommendation { Id=4, CreatedAt=SeedDate, Name="Serilog", PackageName="Serilog.AspNetCore", Architecture=ArchitectureType.DotNet, Category="Logging", Description="Logging estructurado para .NET", PopularityScore=97, InstallCommand="dotnet add package Serilog.AspNetCore" },
        new LibraryRecommendation { Id=5, CreatedAt=SeedDate, Name="AutoMapper", PackageName="AutoMapper", Architecture=ArchitectureType.DotNet, Category="Mapping", Description="Mapeo automático entre objetos", PopularityScore=94, InstallCommand="dotnet add package AutoMapper" },
        new LibraryRecommendation { Id=6, CreatedAt=SeedDate, Name="Swashbuckle (Swagger)", PackageName="Swashbuckle.AspNetCore", Architecture=ArchitectureType.DotNet, Framework=FrameworkType.AspNetCoreWebApi, Category="Documentation", Description="Generación automática de documentación OpenAPI", PopularityScore=98, InstallCommand="dotnet add package Swashbuckle.AspNetCore" },
        new LibraryRecommendation { Id=7, CreatedAt=SeedDate, Name="xUnit", PackageName="xunit", Architecture=ArchitectureType.DotNet, Category="Testing", Description="Framework de testing unitario para .NET", PopularityScore=96, InstallCommand="dotnet add package xunit" },
        new LibraryRecommendation { Id=1000, CreatedAt=SeedDate, Name="Dapper", PackageName="Dapper", Architecture=ArchitectureType.DotNet, Category="ORM", Description="Micro ORM rápido y ligero para .NET", PopularityScore=91, InstallCommand="dotnet add package Dapper" },
        new LibraryRecommendation { Id=1001, CreatedAt=SeedDate, Name="Npgsql EF Core", PackageName="Npgsql.EntityFrameworkCore.PostgreSQL", Architecture=ArchitectureType.DotNet, Category="Database", Description="Provider de PostgreSQL para EF Core", PopularityScore=93, InstallCommand="dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL" },
        new LibraryRecommendation { Id=1002, CreatedAt=SeedDate, Name="Pomelo MySQL EF Core", PackageName="Pomelo.EntityFrameworkCore.MySql", Architecture=ArchitectureType.DotNet, Category="Database", Description="Provider de MySQL para EF Core", PopularityScore=89, InstallCommand="dotnet add package Pomelo.EntityFrameworkCore.MySql" },
        new LibraryRecommendation { Id=1003, CreatedAt=SeedDate, Name="Carter", PackageName="Carter", Architecture=ArchitectureType.DotNet, Framework=FrameworkType.MinimalApi, Category="Routing", Description="Módulos de rutas elegantes para Minimal API", PopularityScore=82, InstallCommand="dotnet add package Carter" },
        new LibraryRecommendation { Id=1004, CreatedAt=SeedDate, Name="Polly", PackageName="Polly", Architecture=ArchitectureType.DotNet, Category="Resilience", Description="Librería de resiliencia y manejo de fallos transitivos", PopularityScore=93, InstallCommand="dotnet add package Polly" },
        new LibraryRecommendation { Id=1005, CreatedAt=SeedDate, Name="NUnit", PackageName="NUnit", Architecture=ArchitectureType.DotNet, Category="Testing", Description="Framework de testing alternativo para .NET", PopularityScore=88, InstallCommand="dotnet add package NUnit" },
        new LibraryRecommendation { Id=1006, CreatedAt=SeedDate, Name="Bogus", PackageName="Bogus", Architecture=ArchitectureType.DotNet, Category="Testing", Description="Generador de datos falsos para tests", PopularityScore=86, InstallCommand="dotnet add package Bogus" },
        new LibraryRecommendation { Id=1007, CreatedAt=SeedDate, Name="Hangfire", PackageName="Hangfire.AspNetCore", Architecture=ArchitectureType.DotNet, Category="Background Jobs", Description="Jobs en background con panel de administración", PopularityScore=90, InstallCommand="dotnet add package Hangfire.AspNetCore" },
        new LibraryRecommendation { Id=1008, CreatedAt=SeedDate, Name="StackExchange.Redis", PackageName="StackExchange.Redis", Architecture=ArchitectureType.DotNet, Category="Cache", Description="Cliente Redis de alto rendimiento para .NET", PopularityScore=92, InstallCommand="dotnet add package StackExchange.Redis" },
    };

    public static IEnumerable<DesignPatternEntry> GetDesignPatterns() => new[]
    {
        new DesignPatternEntry { Id=1, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (.NET)", Architecture=ArchitectureType.DotNet, Description="Abstrae el acceso a datos detrás de interfaces.", ImplementationNotes="Crear IRepository<T> en Domain, implementar con EF Core en Infrastructure.", ScaffoldCommandsJson="[\"mkdir -p src/Domain/Interfaces src/Infrastructure/Repositories\"]" },
        new DesignPatternEntry { Id=2, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS + MediatR (.NET)", Architecture=ArchitectureType.DotNet, Description="Separa comandos y consultas con MediatR.", ImplementationNotes="Instalar MediatR. Crear Application/Commands y Application/Queries con handlers.", ScaffoldCommandsJson="[\"mkdir -p src/Application/Commands src/Application/Queries src/Application/Handlers\"]" },
        new DesignPatternEntry { Id=3, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (.NET)", Architecture=ArchitectureType.DotNet, Description="Capas: Domain, Application, Infrastructure, Presentation.", ImplementationNotes="Domain no referencia nada. Application referencia Domain. Infrastructure implementa interfaces.", ScaffoldCommandsJson="[\"mkdir -p src/Domain src/Application src/Infrastructure src/Presentation\"]" },
        new DesignPatternEntry { Id=4, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (.NET)", Architecture=ArchitectureType.DotNet, Description="Aggregates, Entities, Value Objects y Domain Events.", ImplementationNotes="Modelar el dominio con entidades ricas. Usar eventos de dominio para comunicación entre aggregates.", ScaffoldCommandsJson="[\"mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/ValueObjects src/Domain/Services\"]" },
        new DesignPatternEntry { Id=1000, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (.NET)", Architecture=ArchitectureType.DotNet, Description="Ports & Adapters: el core no conoce infraestructura.", ImplementationNotes="Definir ports (interfaces) en Core. Implementar adapters en Infrastructure.", ScaffoldCommandsJson="[\"mkdir -p src/Core/Ports src/Core/Domain src/Infrastructure/Adapters src/Api\"]" },
        new DesignPatternEntry { Id=1001, CreatedAt=SeedDate, Pattern=DesignPattern.Mediator, Name="Mediator (.NET)", Architecture=ArchitectureType.DotNet, Description="Desacopla componentes con un mediador central (MediatR).", ImplementationNotes="Usar IRequest<T> e IRequestHandler<T>. Registrar en DI con AddMediatR.", ScaffoldCommandsJson="[\"mkdir -p src/Application/Features\"]" },
        new DesignPatternEntry { Id=1002, CreatedAt=SeedDate, Pattern=DesignPattern.EventSourcing, Name="Event Sourcing (.NET)", Architecture=ArchitectureType.DotNet, Description="Estado derivado de secuencia de eventos inmutables.", ImplementationNotes="Usar EventStore o Marten. Cada cambio se registra como evento.", ScaffoldCommandsJson="[\"mkdir -p src/Domain/Events src/Infrastructure/EventStore src/Application/EventHandlers\"]" },
        new DesignPatternEntry { Id=1003, CreatedAt=SeedDate, Pattern=DesignPattern.Microservices, Name="Microservices (.NET)", Architecture=ArchitectureType.DotNet, Description="Servicios independientes comunicándose por HTTP o mensajes.", ImplementationNotes="Usar Ocelot como API Gateway. RabbitMQ o Azure Service Bus para mensajería.", ScaffoldCommandsJson="[\"mkdir -p services/gateway services/orders services/notifications\"]" },
        new DesignPatternEntry { Id=1004, CreatedAt=SeedDate, Pattern=DesignPattern.Saga, Name="Saga (.NET)", Architecture=ArchitectureType.DotNet, Description="Coordina transacciones distribuidas con compensación ante fallos.", ImplementationNotes="Implementar OrchestrationSaga con MediatR. Cada paso compensa los anteriores en caso de error.", ScaffoldCommandsJson="[\"mkdir -p src/Application/Sagas\"]" },
        new DesignPatternEntry { Id=1005, CreatedAt=SeedDate, Pattern=DesignPattern.MVVM, Name="MVVM (.NET)", Architecture=ArchitectureType.DotNet, Description="Model-View-ViewModel para desacoplar UI de lógica (Blazor, WPF, MAUI).", ImplementationNotes="ViewModel implementa INotifyPropertyChanged. Usar RelayCommand para comandos de UI.", ScaffoldCommandsJson="[\"mkdir -p src/Presentation/ViewModels src/Presentation/Commands\"]" },
    };
}
