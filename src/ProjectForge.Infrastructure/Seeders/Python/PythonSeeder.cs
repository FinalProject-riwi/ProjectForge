using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Python;

public static class PythonSeeder
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
        await UpsertTemplatesAsync(db, ct);
        await UpsertLibrariesAsync(db, ct);
        await UpsertPatternsAsync(db, ct);
    }

    private static async Task UpsertTemplatesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var t in GetTemplates())
        {
            if (await db.Templates.AnyAsync(x => x.Id == t.Id || (x.Architecture == t.Architecture && x.TemplateType == t.TemplateType && x.Name == t.Name), ct))
                continue;
            var db2 = (t.Database.HasValue ? "N'" + t.Database.ToString() + "'" : "NULL");
            var infra = (t.Infrastructure.HasValue ? "N'" + t.Infrastructure.ToString() + "'" : "NULL");
            var fw = (t.Framework.HasValue ? "N'" + t.Framework.ToString() + "'" : "NULL");
            var sql = $"""
                SET IDENTITY_INSERT [Templates] ON;
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES ({t.Id},N'{t.Architecture}',N'{t.Content.Replace("'","''").Replace("{","{{").Replace("}","}}")}','{t.CreatedAt:yyyy-MM-dd HH:mm:ss}',{db2},N'{(t.Description??"").Replace("'","''")}',{fw},{infra},1,N'{t.Name.Replace("'","''")}',N'{t.TemplateType}',{t.Version});
                SET IDENTITY_INSERT [Templates] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
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
                VALUES ({l.Id},N'{l.Architecture}',N'{(l.Category??"").Replace("'","''")}','{l.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(l.Description??"").Replace("'","''")}',{fw},N'{(l.InstallCommand??"").Replace("'","''")}',N'{l.Name.Replace("'","''")}',N'{l.PackageName.Replace("'","''")}',{l.PopularityScore});
                SET IDENTITY_INSERT [Libraries] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
        }
    }

    private static async Task UpsertPatternsAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var p in GetDesignPatterns())
        {
            if (await db.DesignPatterns.AnyAsync(x => x.Id == p.Id || (x.Architecture == p.Architecture && x.Pattern == p.Pattern && x.Name == p.Name), ct))
                continue;
            var notes = (p.ImplementationNotes??"").Replace("'","''");
            var cmds  = (p.ScaffoldCommandsJson??"[]").Replace("'","''");
            var sql = $"""
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES ({p.Id},N'{p.Architecture}','{p.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(p.Description??"").Replace("'","''")}',N'{notes}',N'{p.Name.Replace("'","''")}',N'{p.Pattern}',N'{cmds}');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
        }
    }

    public static IEnumerable<ProjectTemplate> GetTemplates() => new[]
    {
        new ProjectTemplate { Id=800, CreatedAt=SeedDate, Name="Dockerfile Python / FastAPI", TemplateType="dockerfile", Architecture=ArchitectureType.Python, Description="Dockerfile para FastAPI con uvicorn", IsActive=true, Version=1,
            Content="FROM python:3.12-slim\nWORKDIR /app\nCOPY requirements.txt .\nRUN pip install --no-cache-dir -r requirements.txt\nCOPY . .\nEXPOSE 8000\nCMD [\"uvicorn\", \"app.main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8000\"]" },
        new ProjectTemplate { Id=801, CreatedAt=SeedDate, Name="Compose Python + PostgreSQL", TemplateType="compose", Architecture=ArchitectureType.Python, Database=DatabaseType.PostgreSQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para FastAPI/Django + PostgreSQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:" },
        new ProjectTemplate { Id=802, CreatedAt=SeedDate, Name="Compose Python + MySQL", TemplateType="compose", Architecture=ArchitectureType.Python, Database=DatabaseType.MySQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para Python + MySQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:" },
        new ProjectTemplate { Id=1200, CreatedAt=SeedDate, Name="CI Python GitHub Actions", TemplateType="ci", Architecture=ArchitectureType.Python, Description="Pipeline CI para Python con pytest", IsActive=true, Version=1,
            Content="name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-python@v5\n        with:\n          python-version: '3.12'\n      - run: pip install -r requirements.txt\n      - run: pytest --tb=short" },
    };

    public static IEnumerable<LibraryRecommendation> GetLibraries() => new[]
    {
        new LibraryRecommendation { Id=8, CreatedAt=SeedDate, Name="SQLAlchemy", PackageName="sqlalchemy", Architecture=ArchitectureType.Python, Category="ORM", Description="ORM más popular para Python, soporta sync y async", PopularityScore=98, InstallCommand="pip install sqlalchemy" },
        new LibraryRecommendation { Id=9, CreatedAt=SeedDate, Name="Pydantic", PackageName="pydantic", Architecture=ArchitectureType.Python, Category="Validation", Description="Validación de datos con type hints", PopularityScore=97, InstallCommand="pip install pydantic" },
        new LibraryRecommendation { Id=10, CreatedAt=SeedDate, Name="Alembic", PackageName="alembic", Architecture=ArchitectureType.Python, Category="Migrations", Description="Migraciones de base de datos para SQLAlchemy", PopularityScore=90, InstallCommand="pip install alembic" },
        new LibraryRecommendation { Id=800, CreatedAt=SeedDate, Name="httpx", PackageName="httpx", Architecture=ArchitectureType.Python, Category="HTTP Client", Description="Cliente HTTP moderno con soporte async", PopularityScore=89, InstallCommand="pip install httpx" },
        new LibraryRecommendation { Id=801, CreatedAt=SeedDate, Name="Celery", PackageName="celery", Architecture=ArchitectureType.Python, Category="Background Tasks", Description="Cola de tareas distribuidas, ideal con Redis", PopularityScore=93, InstallCommand="pip install celery" },
        new LibraryRecommendation { Id=802, CreatedAt=SeedDate, Name="pytest", PackageName="pytest", Architecture=ArchitectureType.Python, Category="Testing", Description="Framework de testing más popular de Python", PopularityScore=99, InstallCommand="pip install pytest pytest-asyncio" },
        new LibraryRecommendation { Id=1200, CreatedAt=SeedDate, Name="FastAPI", PackageName="fastapi", Architecture=ArchitectureType.Python, Framework=FrameworkType.FastAPI, Category="Framework", Description="Framework web moderno y rápido para APIs", PopularityScore=99, InstallCommand="pip install fastapi uvicorn[standard]" },
        new LibraryRecommendation { Id=1201, CreatedAt=SeedDate, Name="Django REST Framework", PackageName="djangorestframework", Architecture=ArchitectureType.Python, Framework=FrameworkType.Django, Category="API", Description="Extensión de Django para crear APIs REST", PopularityScore=96, InstallCommand="pip install djangorestframework" },
        new LibraryRecommendation { Id=1202, CreatedAt=SeedDate, Name="Flask-SQLAlchemy", PackageName="flask-sqlalchemy", Architecture=ArchitectureType.Python, Framework=FrameworkType.Flask, Category="ORM", Description="Integración de SQLAlchemy con Flask", PopularityScore=88, InstallCommand="pip install flask-sqlalchemy" },
        new LibraryRecommendation { Id=1203, CreatedAt=SeedDate, Name="aioredis", PackageName="redis", Architecture=ArchitectureType.Python, Category="Cache", Description="Cliente Redis async para Python", PopularityScore=87, InstallCommand="pip install redis[asyncio]" },
        new LibraryRecommendation { Id=1204, CreatedAt=SeedDate, Name="PyJWT", PackageName="pyjwt", Architecture=ArchitectureType.Python, Category="Autenticación", Description="Manejo de tokens JWT en Python", PopularityScore=92, InstallCommand="pip install pyjwt" },
        new LibraryRecommendation { Id=1205, CreatedAt=SeedDate, Name="Python-dotenv", PackageName="python-dotenv", Architecture=ArchitectureType.Python, Category="Config", Description="Carga variables de entorno desde .env", PopularityScore=96, InstallCommand="pip install python-dotenv" },
    };

    public static IEnumerable<DesignPatternEntry> GetDesignPatterns() => new[]
    {
        new DesignPatternEntry { Id=5, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Python/FastAPI)", Architecture=ArchitectureType.Python, Description="Aisla acceso a datos detrás de repositorios abstractos.", ImplementationNotes="Clase base AbstractRepository. Implementar con SQLAlchemy en infrastructure.", ScaffoldCommandsJson="[\"mkdir -p app/repositories app/domain app/services\"]" },
        new DesignPatternEntry { Id=800, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Python/FastAPI)", Architecture=ArchitectureType.Python, Description="Capas: domain, application, infrastructure y adapters.", ImplementationNotes="Domain no importa FastAPI. DI de FastAPI conecta las capas.", ScaffoldCommandsJson="[\"mkdir -p app/domain app/application/use_cases app/infrastructure app/adapters/api\"]" },
        new DesignPatternEntry { Id=801, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS (Python/FastAPI)", Architecture=ArchitectureType.Python, Description="Separa comandos y consultas con handlers.", ImplementationNotes="Dataclasses para Command/Query. Handlers en application/.", ScaffoldCommandsJson="[\"mkdir -p app/commands app/queries app/handlers\"]" },
        new DesignPatternEntry { Id=1200, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (Python)", Architecture=ArchitectureType.Python, Description="Ports como ABCs de Python, adapters como implementaciones concretas.", ImplementationNotes="Usar ABC para ports. Inyectar adapters en los servicios de aplicación.", ScaffoldCommandsJson="[\"mkdir -p app/core/ports app/core/domain app/infrastructure/adapters app/api\"]" },
        new DesignPatternEntry { Id=1201, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (Python)", Architecture=ArchitectureType.Python, Description="Aggregates, Entities y Value Objects con dataclasses.", ImplementationNotes="Usar @dataclass para Value Objects. Domain events con publish/subscribe.", ScaffoldCommandsJson="[\"mkdir -p app/domain/aggregates app/domain/events app/domain/value_objects app/application\"]" },
        new DesignPatternEntry { Id=1202, CreatedAt=SeedDate, Pattern=DesignPattern.EventSourcing, Name="Event Sourcing (Python)", Architecture=ArchitectureType.Python, Description="Estado reconstruido desde eventos inmutables.", ImplementationNotes="Usar EventStoreDB o PostgreSQL como event store. Proyecciones para read models.", ScaffoldCommandsJson="[\"mkdir -p app/domain/events app/infrastructure/event_store app/application/projections\"]" },
        new DesignPatternEntry { Id=1203, CreatedAt=SeedDate, Pattern=DesignPattern.Microservices, Name="Microservices (Python/FastAPI)", Architecture=ArchitectureType.Python, Description="Servicios independientes con FastAPI comunicándose por HTTP.", ImplementationNotes="Cada servicio tiene su propio main.py y requirements.txt. Usar httpx para comunicación entre servicios.", ScaffoldCommandsJson="[\"mkdir -p services/items services/notifications services/gateway\"]" },
        new DesignPatternEntry { Id=1204, CreatedAt=SeedDate, Pattern=DesignPattern.Mediator, Name="Mediator (Python)", Architecture=ArchitectureType.Python, Description="Mediador que desacopla handlers de mensajes en Python.", ImplementationNotes="Clase Mediator que registra handlers por tipo. Compatible con FastAPI DI.", ScaffoldCommandsJson="[\"mkdir -p app/application\"]" },
        new DesignPatternEntry { Id=1205, CreatedAt=SeedDate, Pattern=DesignPattern.Saga, Name="Saga (Python/FastAPI)", Architecture=ArchitectureType.Python, Description="Orquesta transacciones distribuidas con compensación.", ImplementationNotes="Clase OrderSaga con pasos async. Cada paso tiene compensación en caso de fallo.", ScaffoldCommandsJson="[\"mkdir -p app/application/sagas\"]" },
    };
}
