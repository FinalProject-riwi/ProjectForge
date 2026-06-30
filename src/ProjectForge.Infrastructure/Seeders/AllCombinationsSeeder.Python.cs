using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5080-5129
    private static IEnumerable<ProjectTemplate> GetPythonTemplates()
    {
        int id = 5080;

        // ── Dockerfiles por framework ────────────────────────────────────────────
        yield return T(id++, "Dockerfile Python / Django", "dockerfile", ArchitectureType.Python,
            framework: FrameworkType.Django,
            desc: "Dockerfile para Django con gunicorn",
            content:
            "FROM python:3.12-slim\n" +
            "ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1\n" +
            "WORKDIR /app\n" +
            "COPY requirements.txt .\n" +
            "RUN pip install --no-cache-dir -r requirements.txt\n" +
            "COPY . .\n" +
            "RUN python manage.py collectstatic --noinput\n" +
            "EXPOSE 8000\n" +
            "CMD [\"gunicorn\", \"{{APP_NAME}}.wsgi:application\", \"--bind\", \"0.0.0.0:8000\", \"--workers\", \"4\"]");

        yield return T(id++, "Dockerfile Python / Flask", "dockerfile", ArchitectureType.Python,
            framework: FrameworkType.Flask,
            desc: "Dockerfile para Flask con gunicorn",
            content:
            "FROM python:3.12-slim\n" +
            "ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1\n" +
            "WORKDIR /app\n" +
            "COPY requirements.txt .\n" +
            "RUN pip install --no-cache-dir -r requirements.txt\n" +
            "COPY . .\n" +
            "EXPOSE 5000\n" +
            "CMD [\"gunicorn\", \"app:create_app()\", \"--bind\", \"0.0.0.0:5000\", \"--workers\", \"4\"]");

        // ── Compose FastAPI × todas las DBs ─────────────────────────────────────
        var fastapiConns = new[]
        {
            (DatabaseType.SqlServer, "DATABASE_URL: mssql+pyodbc://sa:Secret1234!@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server"),
            (DatabaseType.MongoDB,   "MONGODB_URL: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,     "REDIS_URL: redis://db:6379/0"),
            (DatabaseType.SQLite,    "DATABASE_URL: sqlite+aiosqlite:///./{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in fastapiConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep  = hasSvc ? "    depends_on:\n      db:\n        condition: service_healthy\n" : "";
            if (db == DatabaseType.SqlServer) dep = "    depends_on:\n      - db\n";
            var vol  = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose FastAPI + {db}", "compose", ArchitectureType.Python,
                framework: FrameworkType.FastAPI, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para FastAPI + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n" +
                $"    environment:\n      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── Compose Django × todas las DBs ──────────────────────────────────────
        var djangoConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
            (DatabaseType.SqlServer,  "DATABASE_URL: mssql://sa:Secret1234!@db:1433/{{DB_NAME}}\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
            (DatabaseType.MongoDB,    "MONGODB_URL: mongodb://admin:secret@db:27017/{{DB_NAME}}\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
            (DatabaseType.Redis,      "REDIS_URL: redis://db:6379/0\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:///./{{DB_NAME}}.db\n      DJANGO_SETTINGS_MODULE: {{APP_NAME}}.settings"),
        };
        foreach (var (db, envBlock) in djangoConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol  = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Django + {db}", "compose", ArchitectureType.Python,
                framework: FrameworkType.Django, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Django + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n" +
                $"    environment:\n      {envBlock}\n" +
                dep + vol + dbSec);
        }

        // ── Compose Flask × todas las DBs ───────────────────────────────────────
        var flaskConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n      FLASK_ENV: development"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n      FLASK_ENV: development"),
            (DatabaseType.SqlServer,  "DATABASE_URL: mssql+pyodbc://sa:Secret1234!@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server\n      FLASK_ENV: development"),
            (DatabaseType.MongoDB,    "MONGODB_URL: mongodb://admin:secret@db:27017/{{DB_NAME}}\n      FLASK_ENV: development"),
            (DatabaseType.Redis,      "REDIS_URL: redis://db:6379/0\n      FLASK_ENV: development"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:///{{DB_NAME}}.db\n      FLASK_ENV: development"),
        };
        foreach (var (db, envBlock) in flaskConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol  = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Flask + {db}", "compose", ArchitectureType.Python,
                framework: FrameworkType.Flask, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Flask + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:5000\"\n" +
                $"    environment:\n      {envBlock}\n" +
                dep + vol + dbSec);
        }

        // ── K8s por DB ───────────────────────────────────────────────────────────
        var pythonK8s = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL", "postgresql://postgres:secret@postgres-svc:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL", "mysql+pymysql://root:secret@mysql-svc:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL", "mssql+pyodbc://sa:Secret1234!@sqlserver-svc:1433/{{DB_NAME}}"),
            (DatabaseType.MongoDB,    "MONGODB_URL",  "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL",    "redis://redis-svc:6379/0"),
            (DatabaseType.SQLite,     "DATABASE_URL", "sqlite:///data/{{DB_NAME}}.db"),
        };
        foreach (var (db, envKey, connStr) in pythonK8s)
        {
            yield return T(id++, $"K8s Python + {db}", "k8s-deployment", ArchitectureType.Python,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para Python con {db}",
                content: K8sManifest(envKey, connStr, 8000));
        }

        // ── .gitignore ───────────────────────────────────────────────────────────
        yield return T(id++, ".gitignore Python", "gitignore", ArchitectureType.Python,
            desc: "Gitignore estándar para proyectos Python",
            content:
            "__pycache__/\n*.py[cod]\n*$py.class\n*.egg-info/\n/dist/\n/build/\n.eggs/\n" +
            ".env\n.env.*\n.venv/\nvenv/\nENV/\n*.db\n*.sqlite3\n.pytest_cache/\n" +
            ".mypy_cache/\n.ruff_cache/\ncoverage.xml\nhtmlcov/\n.DS_Store");
    }

    // ─── Additional Python Libraries (IDs 11040-11049) ───────────────────────────
    private static IEnumerable<LibraryRecommendation> GetPythonLibraries() => new[]
    {
        new LibraryRecommendation { Id=11040, CreatedAt=SeedDate, Name="Motor (MongoDB async)", PackageName="motor", Architecture=ArchitectureType.Python, Category="Database", Description="Driver MongoDB async para FastAPI/asyncio", PopularityScore=86, InstallCommand="pip install motor" },
        new LibraryRecommendation { Id=11041, CreatedAt=SeedDate, Name="Beanie (MongoDB ODM)", PackageName="beanie", Architecture=ArchitectureType.Python, Category="ODM", Description="ODM async para MongoDB basado en Motor + Pydantic", PopularityScore=83, InstallCommand="pip install beanie" },
        new LibraryRecommendation { Id=11042, CreatedAt=SeedDate, Name="Tortoise ORM", PackageName="tortoise-orm", Architecture=ArchitectureType.Python, Category="ORM", Description="ORM async para Python con soporte Django-like", PopularityScore=82, InstallCommand="pip install tortoise-orm aerich" },
        new LibraryRecommendation { Id=11043, CreatedAt=SeedDate, Name="Pytest-asyncio", PackageName="pytest-asyncio", Architecture=ArchitectureType.Python, Category="Testing", Description="Plugin pytest para tests async en FastAPI/Python", PopularityScore=91, InstallCommand="pip install pytest-asyncio" },
        new LibraryRecommendation { Id=11044, CreatedAt=SeedDate, Name="Structlog", PackageName="structlog", Architecture=ArchitectureType.Python, Category="Logging", Description="Logging estructurado para Python", PopularityScore=87, InstallCommand="pip install structlog" },
        new LibraryRecommendation { Id=11045, CreatedAt=SeedDate, Name="Dependency Injector", PackageName="dependency-injector", Architecture=ArchitectureType.Python, Category="DI", Description="Framework de inyección de dependencias para Python", PopularityScore=84, InstallCommand="pip install dependency-injector" },
        new LibraryRecommendation { Id=11046, CreatedAt=SeedDate, Name="Boto3 (AWS SDK)", PackageName="boto3", Architecture=ArchitectureType.Python, Category="Cloud", Description="SDK de AWS para Python", PopularityScore=95, InstallCommand="pip install boto3" },
        new LibraryRecommendation { Id=11047, CreatedAt=SeedDate, Name="Dramatiq", PackageName="dramatiq", Architecture=ArchitectureType.Python, Category="Background Tasks", Description="Cola de tareas async alternativa a Celery", PopularityScore=79, InstallCommand="pip install dramatiq[rabbitmq]" },
        new LibraryRecommendation { Id=11048, CreatedAt=SeedDate, Name="Loguru", PackageName="loguru", Architecture=ArchitectureType.Python, Category="Logging", Description="Logger moderno y simple para Python", PopularityScore=90, InstallCommand="pip install loguru" },
        new LibraryRecommendation { Id=11049, CreatedAt=SeedDate, Name="Psycopg3", PackageName="psycopg[binary]", Architecture=ArchitectureType.Python, Category="Database", Description="Driver PostgreSQL async moderno para Python", PopularityScore=88, InstallCommand="pip install psycopg[binary] psycopg-pool" },
    };

    // ─── Python patterns per framework (IDs 12000-12009) ─────────────────────────
    private static IEnumerable<DesignPatternEntry> GetPythonPatternsByFw() => new[]
    {
        new DesignPatternEntry { Id=12000, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Django)", Architecture=ArchitectureType.Python, Description="Repositorios sobre QuerySets de Django para aislar lógica de acceso.", ImplementationNotes="Crear clase abstracta BaseRepository. Implementar con .objects del model Django.", ScaffoldCommandsJson="[\"mkdir -p app/repositories app/domain\"]" },
        new DesignPatternEntry { Id=12001, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Django)", Architecture=ArchitectureType.Python, Description="Separar domain/application/infrastructure en proyectos Django.", ImplementationNotes="Domain sin imports de Django. Views solo como adaptadores de entrada.", ScaffoldCommandsJson="[\"mkdir -p app/domain app/application/use_cases app/infrastructure app/interfaces\"]" },
        new DesignPatternEntry { Id=12002, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Flask)", Architecture=ArchitectureType.Python, Description="Repositorios sobre SQLAlchemy en proyectos Flask.", ImplementationNotes="BaseRepository abstracto. SQLAlchemy Session inyectada en constructores.", ScaffoldCommandsJson="[\"mkdir -p app/repositories app/models app/services\"]" },
        new DesignPatternEntry { Id=12003, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Flask)", Architecture=ArchitectureType.Python, Description="Capas domain/application/infrastructure en Flask con Blueprints.", ImplementationNotes="Blueprints como adaptadores de entrada. Domain sin imports de Flask.", ScaffoldCommandsJson="[\"mkdir -p app/domain app/application app/infrastructure app/interfaces\"]" },
        new DesignPatternEntry { Id=12004, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS (Django)", Architecture=ArchitectureType.Python, Description="Separa comandos y queries usando Services en Django.", ImplementationNotes="CommandService con métodos que mutan estado. QueryService con métodos de lectura.", ScaffoldCommandsJson="[\"mkdir -p app/commands app/queries app/services\"]" },
        new DesignPatternEntry { Id=12005, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (Django)", Architecture=ArchitectureType.Python, Description="Ports como ABCs, adapters como views y repositories Django.", ImplementationNotes="Ports en core/ports/. Django Views como adapters de entrada. Django ORM como adapter de salida.", ScaffoldCommandsJson="[\"mkdir -p app/core/ports app/core/domain app/adapters\"]" },
        new DesignPatternEntry { Id=12006, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (Flask)", Architecture=ArchitectureType.Python, Description="Aggregates y Value Objects con dataclasses en Flask.", ImplementationNotes="Dataclasses inmutables para VOs. SQLAlchemy entities separadas de domain objects.", ScaffoldCommandsJson="[\"mkdir -p app/domain/aggregates app/domain/value_objects app/domain/events app/application\"]" },
    };
}