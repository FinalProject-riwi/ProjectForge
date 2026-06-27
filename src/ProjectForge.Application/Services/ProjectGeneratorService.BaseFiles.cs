using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Java base file scaffold ──────────────────────────────────────────────

    private async Task ScaffoldJavaBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Java) return;

        var safeName = project.Name.Replace(" ", "");
        var dbName   = $"{project.Name.ToLower().Replace(" ", "_")}_db";
        var libs     = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? [];

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando archivos base Java/Spring Boot...", ct: ct);

        // src/main/resources/application.properties
        var resourcesPath = Path.Combine(path, "src", "main", "resources");
        Directory.CreateDirectory(resourcesPath);
        await File.WriteAllTextAsync(
            Path.Combine(resourcesPath, "application.properties"),
            BuildJavaApplicationProperties(cfg.Database, dbName), ct);

        // pom.xml — only if the generated project doesn't already have one
        // (Spring Initializr already creates one; for Quarkus/Micronaut too)
        var pomPath = Path.Combine(path, "pom.xml");
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

        // Dockerfile
        var dockerfilePath = Path.Combine(path, "Dockerfile");
        if (!File.Exists(dockerfilePath))
        {
            await File.WriteAllTextAsync(dockerfilePath, BuildJavaDockerfile(), ct);
            await EmitLogAsync(project, "Scaffold", "✅ Dockerfile Java generado", ct: ct);
        }

        // .env
        await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildJavaEnvFile(cfg.Database, dbName), ct);

        await EmitLogAsync(project, "Scaffold", "✅ Archivos base Java generados", ct: ct);
    }

    private static string BuildJavaDockerfile() => """
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

    private static string BuildJavaEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL  => $"SPRING_DATASOURCE_URL=jdbc:postgresql://db:5432/{dbName}\nSPRING_DATASOURCE_USERNAME=postgres\nSPRING_DATASOURCE_PASSWORD=secret\n",
        DatabaseType.MySQL       => $"SPRING_DATASOURCE_URL=jdbc:mysql://db:3306/{dbName}\nSPRING_DATASOURCE_USERNAME=root\nSPRING_DATASOURCE_PASSWORD=secret\n",
        DatabaseType.SqlServer   => $"SPRING_DATASOURCE_URL=jdbc:sqlserver://sqlserver:1433;databaseName={dbName};encrypt=false\nSPRING_DATASOURCE_USERNAME=sa\nSPRING_DATASOURCE_PASSWORD=YourStrong!Passw0rd\n",
        DatabaseType.MongoDB     => $"SPRING_DATA_MONGODB_URI=mongodb://mongo:27017/{dbName}\n",
        DatabaseType.Redis       => "SPRING_DATA_REDIS_HOST=redis\nSPRING_DATA_REDIS_PORT=6379\n",
        DatabaseType.SQLite      => $"SPRING_DATASOURCE_URL=jdbc:sqlite:/app/data/{dbName}.db\n",
        _                        => ""
    };

    private static async Task InjectJavaDbDependencyAsync(string pomPath, DatabaseType db, CancellationToken ct)
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

        var dbName = $"{project.Name.ToLower().Replace(" ", "_")}_db";
        var libs   = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? [];

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando archivos base Python...", ct: ct);

        // requirements.txt
        var reqPath = Path.Combine(path, "requirements.txt");
        if (!File.Exists(reqPath))
            await File.WriteAllTextAsync(reqPath, BuildPythonRequirementsTxt(cfg.Framework, cfg.Database, libs), ct);

        // .env
        await File.WriteAllTextAsync(Path.Combine(path, ".env"), BuildPythonEnvFile(cfg.Database, dbName), ct);
        await File.WriteAllTextAsync(Path.Combine(path, ".env.example"), BuildPythonEnvFile(cfg.Database, dbName), ct);

        // app/database.py
        var appPath = Path.Combine(path, "app");
        Directory.CreateDirectory(appPath);

        var dbConfigPath = Path.Combine(appPath, "database.py");
        if (!File.Exists(dbConfigPath))
            await File.WriteAllTextAsync(dbConfigPath, BuildPythonDatabaseConfig(cfg.Database, dbName), ct);

        // app/main.py (framework entry point)
        var mainPath = Path.Combine(appPath, "main.py");
        if (!File.Exists(mainPath))
        {
            var mainContent = cfg.Framework switch
            {
                FrameworkType.FastAPI => BuildFastApiMain(cfg.Database, dbName),
                FrameworkType.Django  => "",  // django-admin creates it
                FrameworkType.Flask   => BuildFlaskMain(cfg.Database, dbName),
                _                     => BuildFastApiMain(cfg.Database, dbName)
            };
            if (!string.IsNullOrEmpty(mainContent))
                await File.WriteAllTextAsync(mainPath, mainContent, ct);
        }

        // app/models.py
        var modelsPath = Path.Combine(appPath, "models.py");
        if (!File.Exists(modelsPath))
            await File.WriteAllTextAsync(modelsPath, BuildPythonModels(cfg.Database), ct);

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

    private static string BuildFastApiMain(DatabaseType db, string dbName)
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

    private static string BuildFlaskMain(DatabaseType db, string dbName) => $"""
from flask import Flask, jsonify
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)
app.config["SQLALCHEMY_DATABASE_URI"] = __import__("os").getenv("DATABASE_URL", "sqlite:///{dbName}.db")

@app.route("/")
def root():
    return jsonify(message="Hello from ProjectForge!", db="{db}")

@app.route("/health")
def health():
    return jsonify(status="ok")

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
""";

    private static string BuildPythonModels(DatabaseType db) => db switch
    {
        DatabaseType.MongoDB => """
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
""",
        _ => """
from sqlalchemy import Column, Integer, String, DateTime, func
from app.database import Base

class Item(Base):
    __tablename__ = "items"

    id          = Column(Integer, primary_key=True, index=True)
    name        = Column(String(255), nullable=False)
    description = Column(String(1000), nullable=True)
    created_at  = Column(DateTime, server_default=func.now())
"""
    };

    private static string BuildPythonDockerfile(FrameworkType fw)
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

    private static string BuildPythonTestFile(FrameworkType fw) => fw switch
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

        var dbName = $"{project.Name.ToLower().Replace(" ", "_")}_db";

        await EmitLogAsync(project, "Scaffold", "🏗️  Configurando archivos .NET...", ct: ct);

        // .env (for local override)
        var envContent = cfg.Database switch
        {
            DatabaseType.PostgreSQL  => $"ConnectionStrings__Default=Host=localhost;Database={dbName};Username=postgres;Password=secret\n",
            DatabaseType.MySQL       => $"ConnectionStrings__Default=Server=localhost;Database={dbName};User=root;Password=secret;\n",
            DatabaseType.SqlServer   => $"ConnectionStrings__Default=Server=localhost;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;\n",
            DatabaseType.SQLite      => $"ConnectionStrings__Default=Data Source={dbName}.db\n",
            DatabaseType.MongoDB     => $"ConnectionStrings__Default=mongodb://localhost:27017/{dbName}\n",
            DatabaseType.Redis       => "ConnectionStrings__Default=localhost:6379\n",
            _                        => ""
        };

        if (!string.IsNullOrEmpty(envContent))
            await File.WriteAllTextAsync(Path.Combine(path, ".env"), envContent, ct);

        await EmitLogAsync(project, "Scaffold", "✅ Archivos .env .NET generados", ct: ct);
    }
}
