using System.Text.Json;
using System.Text.Json.Nodes;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Java base file scaffold ──────────────────────────────────────────────

    private async Task ScaffoldJavaBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Java) return;

        // GetValidDatabaseName strips anything that isn't [a-z0-9_] — a raw
        // "{project.Name}.ToLower().Replace(" ", "_")" would let other punctuation straight
        // into JDBC connection strings (e.g. "jdbc:postgresql://db:5432/{dbName}"), where stray
        // characters like '&' or '?' can corrupt the URL or inject bogus query parameters.
        var dbName = $"{GetValidDatabaseName(project.Name)}_db";
        var libs   = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? [];

        await EmitLogAsync(project, "Scaffold", $"🏗️  Generando archivos base Java/{cfg.Framework}...", ct: ct);

        var resourcesPath = Path.Combine(path, "src", "main", "resources");
        Directory.CreateDirectory(resourcesPath);
        var pomPath = Path.Combine(path, "pom.xml");

        // Quarkus/Micronaut use entirely different config keys and Maven dependency
        // coordinates than Spring Boot (quarkus.*/datasources.default.* vs spring.datasource.*,
        // io.quarkus/io.micronaut.sql artifacts vs org.springframework.boot ones). Writing the
        // Spring-flavored application.properties/pom dependencies into a Quarkus or Micronaut
        // project used to leave the DB completely unconfigured — and for MongoDB/Redis it broke
        // the Maven build outright, since spring-boot-starter-data-mongodb/redis can't resolve
        // without the spring-boot-starter-parent BOM that a Quarkus/Micronaut pom.xml doesn't have.
        switch (cfg.Framework)
        {
            case FrameworkType.Quarkus:
                await File.WriteAllTextAsync(
                    Path.Combine(resourcesPath, "application.properties"),
                    BuildQuarkusApplicationProperties(cfg.Database, dbName), ct);
                await InjectQuarkusDbDependencyAsync(pomPath, cfg.Database, ct);
                await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildQuarkusEnvFile(cfg.Database, dbName), ct);
                break;

            case FrameworkType.Micronaut:
                await File.WriteAllTextAsync(
                    Path.Combine(resourcesPath, "application.properties"),
                    BuildMicronautApplicationProperties(cfg.Database, dbName), ct);
                await InjectMicronautDbDependencyAsync(pomPath, cfg.Database, ct);
                await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildMicronautEnvFile(cfg.Database, dbName), ct);
                break;

            default: // Spring Boot
                await File.WriteAllTextAsync(
                    Path.Combine(resourcesPath, "application.properties"),
                    BuildJavaApplicationProperties(cfg.Database, dbName), ct);

                // pom.xml — only if the generated project doesn't already have one
                if (!File.Exists(pomPath))
                {
                    await File.WriteAllTextAsync(pomPath, BuildJavaPomXml(project.Name, cfg.Database, libs), ct);
                    await EmitLogAsync(project, "Scaffold", "✅ pom.xml generado", ct: ct);
                }
                else
                {
                    // Inject DB dependency into existing pom.xml if not present
                    await InjectJavaDbDependencyAsync(pomPath, cfg.Database, ct);
                }

                await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildJavaEnvFile(cfg.Database, dbName), ct);
                break;
        }

        // Dockerfile
        var dockerfilePath = Path.Combine(path, "Dockerfile");
        if (!File.Exists(dockerfilePath))
        {
            await File.WriteAllTextAsync(dockerfilePath, BuildJavaDockerfile(), ct);
            await EmitLogAsync(project, "Scaffold", "✅ Dockerfile Java generado", ct: ct);
        }

        await EmitLogAsync(project, "Scaffold", "✅ Archivos base Java generados", ct: ct);
    }

    internal static string BuildJavaDockerfile() => """
FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /app
COPY . .
RUN mvn -q package -DskipTests

FROM eclipse-temurin:21-jre-alpine AS final
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
EXPOSE 8080
ENTRYPOINT ["java","-jar","app.jar"]
""";

    internal static string BuildJavaEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL  => $"SPRING_DATASOURCE_URL=jdbc:postgresql://db:5432/{dbName}\nSPRING_DATASOURCE_USERNAME=postgres\nSPRING_DATASOURCE_PASSWORD=secret\n",
        DatabaseType.MySQL       => $"SPRING_DATASOURCE_URL=jdbc:mysql://db:3306/{dbName}\nSPRING_DATASOURCE_USERNAME=root\nSPRING_DATASOURCE_PASSWORD=secret\n",
        DatabaseType.SqlServer   => $"SPRING_DATASOURCE_URL=jdbc:sqlserver://sqlserver:1433;databaseName={dbName};encrypt=false\nSPRING_DATASOURCE_USERNAME=sa\nSPRING_DATASOURCE_PASSWORD=YourStrong!Passw0rd\n",
        DatabaseType.MongoDB     => $"SPRING_DATA_MONGODB_URI=mongodb://mongo:27017/{dbName}\n",
        DatabaseType.Redis       => "SPRING_DATA_REDIS_HOST=redis\nSPRING_DATA_REDIS_PORT=6379\n",
        DatabaseType.SQLite      => $"SPRING_DATASOURCE_URL=jdbc:sqlite:/app/data/{dbName}.db\n",
        _                        => ""
    };

    internal static async Task InjectJavaDbDependencyAsync(string pomPath, DatabaseType db, CancellationToken ct)
    {
        var content = await File.ReadAllTextAsync(pomPath, ct);
        var marker  = "</dependencies>";
        if (!content.Contains(marker)) return;

        var snippet = db switch
        {
            DatabaseType.PostgreSQL  when !content.Contains("postgresql") => """
        <dependency>
            <groupId>org.postgresql</groupId>
            <artifactId>postgresql</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.MySQL       when !content.Contains("mysql-connector") => """
        <dependency>
            <groupId>com.mysql</groupId>
            <artifactId>mysql-connector-j</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.SqlServer   when !content.Contains("mssql-jdbc") => """
        <dependency>
            <groupId>com.microsoft.sqlserver</groupId>
            <artifactId>mssql-jdbc</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.MongoDB     when !content.Contains("data-mongodb") => """
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-mongodb</artifactId>
        </dependency>
""",
            DatabaseType.Redis       when !content.Contains("data-redis") => """
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-redis</artifactId>
        </dependency>
""",
            DatabaseType.SQLite      when !content.Contains("sqlite-jdbc") => """
        <dependency>
            <groupId>org.xerial</groupId>
            <artifactId>sqlite-jdbc</artifactId>
            <version>3.45.2.0</version>
        </dependency>
        <dependency>
            <groupId>org.hibernate.orm</groupId>
            <artifactId>hibernate-community-dialects</artifactId>
        </dependency>
""",
            _ => null
        };

        if (snippet != null)
            await File.WriteAllTextAsync(pomPath, content.Replace(marker, snippet + marker), ct);
    }

    // ─── Python base file scaffold ────────────────────────────────────────────

    private async Task ScaffoldPythonBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Python) return;

        var dbName = $"{GetValidDatabaseName(project.Name)}_db";
        var libs   = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? [];

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando archivos base Python...", ct: ct);

        // requirements.txt — always write the DB-aware version (overrides the basic one)
        var reqPath = Path.Combine(path, "requirements.txt");
        await File.WriteAllTextAsync(reqPath, BuildPythonRequirementsTxt(cfg.Framework, cfg.Database, libs), ct);

        // .env
        await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildPythonEnvFile(cfg.Database, dbName), ct);
        await File.WriteAllTextAsync(Path.Combine(path, ".env.example"), BuildPythonEnvFile(cfg.Database, dbName), ct);

        // app/database.py — Django owns its DB config in settings.py (see ScaffoldDjangoAsync),
        // so writing a SQLAlchemy database.py here would just be dead/misleading code for it.
        var appPath = Path.Combine(path, "app");
        Directory.CreateDirectory(appPath);

        if (cfg.Framework != FrameworkType.Django)
        {
            var dbConfigPath = Path.Combine(appPath, "database.py");
            if (!File.Exists(dbConfigPath))
                await File.WriteAllTextAsync(dbConfigPath, BuildPythonDatabaseConfig(cfg.Framework, cfg.Database, dbName), ct);
        }

        // app/main.py (framework entry point) — always write the DB-aware version
        var mainPath = Path.Combine(appPath, "main.py");
        {
            var mainContent = cfg.Framework switch
            {
                FrameworkType.FastAPI => BuildFastApiMain(cfg.Database, dbName),
                FrameworkType.Django  => "",  // Django's own manage.py/settings.py drive the app
                FrameworkType.Flask   => BuildFlaskMain(cfg.Database, dbName),
                _                     => BuildFastApiMain(cfg.Database, dbName)
            };
            if (!string.IsNullOrEmpty(mainContent))
                await File.WriteAllTextAsync(mainPath, mainContent, ct);
        }

        // app/models.py — same reasoning as database.py: Django models are declared per-app
        // using django.db.models, not the SQLAlchemy declarative Base used here.
        if (cfg.Framework != FrameworkType.Django)
        {
            var modelsPath = Path.Combine(appPath, "models.py");
            if (!File.Exists(modelsPath))
                await File.WriteAllTextAsync(modelsPath, BuildPythonModels(cfg.Framework, cfg.Database), ct);
        }

        // Dockerfile
        var dockerfilePath = Path.Combine(path, "Dockerfile");
        if (!File.Exists(dockerfilePath))
        {
            await File.WriteAllTextAsync(dockerfilePath, BuildPythonDockerfile(cfg.Framework), ct);
            await EmitLogAsync(project, "Scaffold", "✅ Dockerfile Python generado", ct: ct);
        }

        // tests/__init__.py
        var testsPath = Path.Combine(path, "tests");
        Directory.CreateDirectory(testsPath);
        var testInitPath = Path.Combine(testsPath, "__init__.py");
        if (!File.Exists(testInitPath))
            await File.WriteAllTextAsync(testInitPath, "", ct);

        var testMainPath = Path.Combine(testsPath, "test_main.py");
        if (!File.Exists(testMainPath))
            await File.WriteAllTextAsync(testMainPath, BuildPythonTestFile(cfg.Framework), ct);

        await EmitLogAsync(project, "Scaffold", "✅ Archivos base Python generados", ct: ct);
    }

    internal static string BuildFastApiMain(DatabaseType db, string dbName)
    {
        var dbImport = db is DatabaseType.MongoDB
            ? "from app.database import connect_db, close_db"
            : db is DatabaseType.Redis
                ? "from app.database import connect_redis, close_redis"
                : "from app.database import engine, Base";

        var startupCode = db == DatabaseType.MongoDB
            ? "    await connect_db()"
            : db == DatabaseType.Redis
                ? "    await connect_redis()"
                : "    async with engine.begin() as conn:\n        await conn.run_sync(Base.metadata.create_all)";

        var shutdownCode = db == DatabaseType.MongoDB
            ? "    await close_db()"
            : db == DatabaseType.Redis
                ? "    await close_redis()"
                : "";

        return
            "from fastapi import FastAPI\n" +
            "from contextlib import asynccontextmanager\n" +
            dbImport + "\n\n" +
            "@asynccontextmanager\n" +
            "async def lifespan(app: FastAPI):\n" +
            "    # Startup\n" +
            startupCode + "\n" +
            "    yield\n" +
            "    # Shutdown\n" +
            shutdownCode + "\n\n" +
            $"app = FastAPI(title=\"{dbName}\", lifespan=lifespan)\n\n" +
            "@app.get(\"/\")\n" +
            "async def root():\n" +
            $"    return {{\"message\": \"Hello from ProjectForge!\", \"db\": \"{db}\"}}\n\n" +
            "@app.get(\"/health\")\n" +
            "async def health():\n" +
            "    return {\"status\": \"ok\"}\n";
    }

    // Flask (WSGI, synchronous) needs framework-appropriate wiring per DB type — the old
    // implementation only ever set app.config["SQLALCHEMY_DATABASE_URI"] and never called
    // db.init_app()/db.create_all(), so the config value was dead and no table was ever
    // created; for MongoDB/Redis it routed a mongodb://... or redis://... URL through the
    // *relational* SQLALCHEMY_DATABASE_URI key, which SQLAlchemy can't parse at all.
    internal static string BuildFlaskMain(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => BuildFlaskMongoMain(db),
        DatabaseType.Redis   => BuildFlaskRedisMain(db),
        _                    => BuildFlaskRelationalMain(db, dbName)
    };

    internal static string BuildFlaskRelationalMain(DatabaseType db, string dbName)
    {
        string uriLine;
        if (db == DatabaseType.SQLite)
        {
            uriLine = $"app.config[\"SQLALCHEMY_DATABASE_URI\"] = os.getenv(\"DATABASE_URL\", \"sqlite:///{dbName}.db\")";
        }
        else
        {
            var (dialect, defaultHost, defaultPort, defaultUser, defaultPassword) = db switch
            {
                DatabaseType.PostgreSQL => ("postgresql+psycopg2", "db", "5432", "postgres", "secret"),
                DatabaseType.MySQL      => ("mysql+pymysql", "db", "3306", "root", "secret"),
                DatabaseType.SqlServer  => ("mssql+pyodbc", "sqlserver", "1433", "sa", "YourStrong!Passw0rd"),
                _                       => ("postgresql+psycopg2", "db", "5432", "postgres", "secret")
            };

            uriLine =
                "app.config[\"SQLALCHEMY_DATABASE_URI\"] = (\n" +
                $"    f\"{dialect}://{{os.getenv('DB_USER', '{defaultUser}')}}:{{os.getenv('DB_PASSWORD', '{defaultPassword}')}}\"\n" +
                $"    f\"@{{os.getenv('DB_HOST', '{defaultHost}')}}:{{os.getenv('DB_PORT', '{defaultPort}')}}/{{os.getenv('DB_NAME', '{dbName}')}}\"\n" +
                ")";
        }

        return
            "from flask import Flask, jsonify\n" +
            "from dotenv import load_dotenv\n" +
            "import os\n\n" +
            "from app.database import db\n\n" +
            "load_dotenv()\n\n" +
            "app = Flask(__name__)\n" +
            uriLine + "\n" +
            "app.config[\"SQLALCHEMY_TRACK_MODIFICATIONS\"] = False\n" +
            "db.init_app(app)\n\n" +
            "with app.app_context():\n" +
            "    db.create_all()\n\n" +
            "@app.route(\"/\")\n" +
            "def root():\n" +
            $"    return jsonify(message=\"Hello from ProjectForge!\", db=\"{db}\")\n\n" +
            "@app.route(\"/health\")\n" +
            "def health():\n" +
            "    return jsonify(status=\"ok\")\n\n" +
            "if __name__ == \"__main__\":\n" +
            "    app.run(host=\"0.0.0.0\", port=8000, debug=True)\n";
    }

    internal static string BuildFlaskMongoMain(DatabaseType db) =>
        "from flask import Flask, jsonify\n" +
        "from dotenv import load_dotenv\n" +
        "from app.database import db as mongo_db\n\n" +
        "load_dotenv()\n\n" +
        "app = Flask(__name__)\n\n" +
        "@app.route(\"/\")\n" +
        "def root():\n" +
        $"    return jsonify(message=\"Hello from ProjectForge!\", db=\"{db}\")\n\n" +
        "@app.route(\"/health\")\n" +
        "def health():\n" +
        "    return jsonify(status=\"ok\")\n\n" +
        "if __name__ == \"__main__\":\n" +
        "    app.run(host=\"0.0.0.0\", port=8000, debug=True)\n";

    internal static string BuildFlaskRedisMain(DatabaseType db) =>
        "from flask import Flask, jsonify\n" +
        "from dotenv import load_dotenv\n" +
        "from app.database import redis_client\n\n" +
        "load_dotenv()\n\n" +
        "app = Flask(__name__)\n\n" +
        "@app.route(\"/\")\n" +
        "def root():\n" +
        $"    return jsonify(message=\"Hello from ProjectForge!\", db=\"{db}\")\n\n" +
        "@app.route(\"/health\")\n" +
        "def health():\n" +
        "    redis_client.ping()\n" +
        "    return jsonify(status=\"ok\")\n\n" +
        "if __name__ == \"__main__\":\n" +
        "    app.run(host=\"0.0.0.0\", port=8000, debug=True)\n";

    internal static string BuildPythonModels(FrameworkType fw, DatabaseType db)
    {
        if (db == DatabaseType.MongoDB)
        {
            // beanie/motor are asyncio-only and can't be driven from sync Flask views.
            return fw == FrameworkType.Flask
                ? "# MongoDB is schemaless — use app.database.db.items directly (a pymongo Collection),\n" +
                  "# e.g. app.database.db.items.insert_one({...}). Kept as the conventional place\n" +
                  "# to add validation helpers if/when you need them.\n"
                : """
from beanie import Document
from pydantic import Field
from typing import Optional
from datetime import datetime

class Item(Document):
    name: str
    description: Optional[str] = None
    created_at: datetime = Field(default_factory=datetime.utcnow)

    class Settings:
        name = "items"
""";
        }

        if (db == DatabaseType.Redis)
        {
            return "# Redis is a key-value store — there's no relational \"model\" to declare here.\n" +
                   "# Use app.database.redis_client (Flask) / app.database.get_redis() (FastAPI) directly.\n";
        }

        return fw == FrameworkType.Flask
            ? """
from app.database import db

class Item(db.Model):
    __tablename__ = "items"

    id          = db.Column(db.Integer, primary_key=True, index=True)
    name        = db.Column(db.String(255), nullable=False)
    description = db.Column(db.String(1000), nullable=True)
    created_at  = db.Column(db.DateTime, server_default=db.func.now())
"""
            : """
from sqlalchemy import Column, Integer, String, DateTime, func
from app.database import Base

class Item(Base):
    __tablename__ = "items"

    id          = Column(Integer, primary_key=True, index=True)
    name        = Column(String(255), nullable=False)
    description = Column(String(1000), nullable=True)
    created_at  = Column(DateTime, server_default=func.now())
""";
    }

    internal static string BuildPythonDockerfile(FrameworkType fw)
    {
        var port = 8000;
        var cmd  = fw switch
        {
            FrameworkType.Django => $"CMD [\"python\", \"manage.py\", \"runserver\", \"0.0.0.0:{port}\"]",
            FrameworkType.Flask  => $"CMD [\"flask\", \"run\", \"--host=0.0.0.0\", \"--port={port}\"]",
            _                    => $"CMD [\"uvicorn\", \"app.main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"{port}\"]"
        };

        return $"""
FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
EXPOSE {port}
{cmd}
""";
    }

    internal static string BuildPythonTestFile(FrameworkType fw) => fw switch
    {
        FrameworkType.FastAPI => """
import pytest
from httpx import AsyncClient, ASGITransport
from app.main import app

@pytest.mark.asyncio
async def test_root():
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.get("/")
    assert response.status_code == 200

@pytest.mark.asyncio
async def test_health():
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "ok"
""",
        _ => """
def test_placeholder():
    assert True
"""
    };

    // ─── DotNet base file scaffold ────────────────────────────────────────────

    private async Task ScaffoldDotNetBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.DotNet) return;

        var safeName = GetValidDotNetProjectName(project.Name);
        var namespaceName = GetValidDotNetNamespace(project.Name);
        var dbName   = GetValidDatabaseName(project.Name);
        var sdkVersion = GetDotNetSdkVersion(cfg.FrameworkVersion);
        var isBlazorWasm = cfg.Framework == FrameworkType.BlazorWasm;

        await EmitLogAsync(project, "Scaffold", "🏗️  Configurando archivos .NET...", ct: ct);

        // Blazor WebAssembly runs entirely inside the browser sandbox: it can't open a raw
        // TCP/database connection, so wiring a DB connection string or EF Core context into it
        // is dead code at best. At worst it's a real credential leak — a WASM app's
        // appsettings.json ships inside wwwroot and is downloaded and readable by every visitor,
        // so embedding "Password=secret" there publishes the DB password to the whole internet.
        // Data access for BlazorWasm should go through an HTTP API instead (see the Repository/
        // Hexagonal pattern files, which emit an HttpClient-based implementation for it).
        if (!isBlazorWasm)
        {
            // ── 1. appsettings.json — inject ConnectionStrings.Default ────────────
            var appSettingsPath = Path.Combine(path, "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                var connString = cfg.Database switch
                {
                    DatabaseType.PostgreSQL => $"Host=db;Database={dbName};Username=postgres;Password=secret",
                    DatabaseType.MySQL      => $"Server=db;Database={dbName};User=root;Password=secret;",
                    DatabaseType.SqlServer  => $"Server=sqlserver;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;",
                    DatabaseType.SQLite     => $"Data Source={dbName}.db",
                    DatabaseType.MongoDB    => $"mongodb://mongo:27017/{dbName}",
                    DatabaseType.Redis      => "localhost:6379",
                    _                       => ""
                };

                if (!string.IsNullOrEmpty(connString))
                {
                    var json = await File.ReadAllTextAsync(appSettingsPath, ct);
                    try
                    {
                        var rootNode = JsonNode.Parse(json)?.AsObject();
                        if (rootNode != null && rootNode["ConnectionStrings"] == null)
                        {
                            var connectionNode = JsonNode.Parse($"{{\"Default\": \"{connString}\"}}")?.AsObject();
                            if (connectionNode != null)
                            {
                                rootNode["ConnectionStrings"] = connectionNode;
                                await File.WriteAllTextAsync(appSettingsPath,
                                    rootNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), ct);
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar si el JSON no es válido o no se puede parsear.
                    }
                }
            }

            // ── 2. .env (local override) ──────────────────────────────────────────
            var envContent = cfg.Database switch
            {
                DatabaseType.PostgreSQL => $"ConnectionStrings__Default=Host=localhost;Database={dbName};Username=postgres;Password=secret\n",
                DatabaseType.MySQL      => $"ConnectionStrings__Default=Server=localhost;Database={dbName};User=root;Password=secret;\n",
                DatabaseType.SqlServer  => $"ConnectionStrings__Default=Server=localhost;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;\n",
                DatabaseType.SQLite     => $"ConnectionStrings__Default=Data Source={dbName}.db\n",
                DatabaseType.MongoDB    => $"ConnectionStrings__Default=mongodb://localhost:27017/{dbName}\n",
                DatabaseType.Redis      => "ConnectionStrings__Default=localhost:6379\n",
                _                       => ""
            };
            if (!string.IsNullOrEmpty(envContent))
                await File.WriteAllTextAsync(Path.Combine(path, ".env"), envContent, ct);

            // ── 3. AppDbContext.cs (only for relational/EF Core targets) ──────────
            var needsEfCore = cfg.Database is DatabaseType.PostgreSQL or DatabaseType.MySQL
                                          or DatabaseType.SqlServer  or DatabaseType.SQLite;
            if (needsEfCore)
            {
                var projectFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
                var projectDir   = projectFiles.Length > 0
                    ? Path.GetDirectoryName(projectFiles[0])!
                    : path;

                var dataDir = Path.Combine(projectDir, "Data");
                Directory.CreateDirectory(dataDir);
                var ctxPath = Path.Combine(dataDir, "AppDbContext.cs");
                if (!File.Exists(ctxPath))
                {
                    await File.WriteAllTextAsync(ctxPath, $$"""
using Microsoft.EntityFrameworkCore;

namespace {{namespaceName}}.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Add your DbSet<TEntity> properties here
    // public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
""", ct);
                    await EmitLogAsync(project, "Scaffold", "✅ AppDbContext.cs generado", ct: ct);
                }

                // AppDbContext.cs used to be generated but never registered — the project would
                // compile, but DI would throw "Unable to resolve service for type AppDbContext"
                // the moment anything tried to inject it, and `dotnet ef migrations add` would
                // fail outright since it discovers the context through the DI registration.
                await EnsureDotNetDbContextRegistrationAsync(projectDir, namespaceName, cfg.Database, ct);
            }
        }

        // ── 4. Dockerfile ─────────────────────────────────────────────────────
        var dockerfilePath = Path.Combine(path, "Dockerfile");
        if (!File.Exists(dockerfilePath))
        {
            var projectFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            var csprojRelative = projectFiles.Length > 0
                ? Path.GetRelativePath(path, projectFiles[0]).Replace("\\", "/")
                : $"{safeName}/{safeName}.csproj";

            // Standalone Blazor WebAssembly has no runnable server entry point — `dotnet publish`
            // produces static files under wwwroot/ meant to be served by a plain web server, so
            // "ENTRYPOINT dotnet {name}.dll" (correct for every other .NET template here) would
            // build but the container would do nothing useful when started.
            var dockerfile = isBlazorWasm
                ? $"""
FROM mcr.microsoft.com/dotnet/sdk:{sdkVersion} AS build
WORKDIR /src
COPY . .
RUN dotnet restore "{csprojRelative}"
RUN dotnet publish "{csprojRelative}" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
"""
                : $"""
FROM mcr.microsoft.com/dotnet/sdk:{sdkVersion} AS build
WORKDIR /src
COPY . .
RUN dotnet restore "{csprojRelative}"
RUN dotnet publish "{csprojRelative}" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:{sdkVersion} AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [\"dotnet\", \"{safeName}.dll\"]
""";
            await File.WriteAllTextAsync(dockerfilePath, dockerfile, ct);
            await EmitLogAsync(project, "Scaffold", "✅ Dockerfile .NET generado", ct: ct);
        }

        await EmitLogAsync(project, "Scaffold", "✅ Archivos base .NET generados", ct: ct);
    }

    internal static async Task EnsureDotNetDbContextRegistrationAsync(
        string projectDir, string namespaceName, DatabaseType database, CancellationToken ct)
    {
        var programPath = Path.Combine(projectDir, "Program.cs");
        if (!File.Exists(programPath)) return;

        var content = await File.ReadAllTextAsync(programPath, ct);
        if (content.Contains("AddDbContext<AppDbContext>", StringComparison.Ordinal))
            return;

        const string marker = "var builder = WebApplication.CreateBuilder(args);";
        var idx = content.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return;

        var optionsExpression = database switch
        {
            DatabaseType.MySQL => "options.UseMySql(builder.Configuration.GetConnectionString(\"Default\"), " +
                                  "Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(builder.Configuration.GetConnectionString(\"Default\")))",
            DatabaseType.SqlServer => "options.UseSqlServer(builder.Configuration.GetConnectionString(\"Default\"))",
            DatabaseType.SQLite    => "options.UseSqlite(builder.Configuration.GetConnectionString(\"Default\"))",
            _                      => "options.UseNpgsql(builder.Configuration.GetConnectionString(\"Default\"))"
        };

        var insertAt = idx + marker.Length;
        var registration = $"\nbuilder.Services.AddDbContext<AppDbContext>(options => {optionsExpression});\n";
        content = content.Insert(insertAt, registration);

        if (!content.Contains("using Microsoft.EntityFrameworkCore;", StringComparison.Ordinal))
            content = $"using Microsoft.EntityFrameworkCore;\nusing {namespaceName}.Data;\n\n" + content;

        await File.WriteAllTextAsync(programPath, content, ct);
    }

    internal static string GetDotNetSdkVersion(string frameworkVersion)
    {
        if (string.IsNullOrWhiteSpace(frameworkVersion))
            return "10.0";

        var versionText = frameworkVersion.Trim();
        if (versionText.StartsWith("net", StringComparison.OrdinalIgnoreCase))
            versionText = versionText[3..];

        var dotIndex = versionText.IndexOf('.');
        if (dotIndex > 0)
            versionText = versionText[..(dotIndex + 2)];

        return versionText switch
        {
            "7" or "7.0" => "7.0",
            "8" or "8.0" => "8.0",
            "9" or "9.0" => "9.0",
            "10" or "10.0" => "10.0",
            _ => "10.0"
        };
    }

    internal static string GetValidDotNetNamespace(string projectName)
    {
        var candidate = System.Text.RegularExpressions.Regex.Replace(projectName.Trim(), @"[^\w]", "");
        if (string.IsNullOrWhiteSpace(candidate))
            return "ProjectForge";
        if (char.IsDigit(candidate[0]))
            candidate = "Project" + candidate;
        return candidate;
    }

    internal static string GetValidDatabaseName(string projectName)
    {
        var safeName = System.Text.RegularExpressions.Regex.Replace(projectName.Trim().ToLowerInvariant(), @"[^a-z0-9_]", "_");
        if (string.IsNullOrWhiteSpace(safeName))
            return "projectforge_db";
        return safeName;
    }
}
