using Microsoft.Extensions.Configuration;
<<<<<<< HEAD
=======
using System.Text;
>>>>>>> 0dc2a35 (complete java,python,typescript)
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

<<<<<<< HEAD
// ─── Contrato mínimo de hub que necesita la capa Application ─────────────────
// Evita dependencia directa de ProjectForge.Web desde Application.
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
public interface IGenerationHubNotifier
{
    Task SendLogAsync(int projectId, string step, string message, bool isError = false);
    Task SendStatusAsync(int projectId, string status);
}

<<<<<<< HEAD
/// <summary>
/// Orquesta la generación del proyecto: ejecuta comandos CLI locales,
/// aplica plantillas y sube el resultado a GitHub.
/// Emite logs en tiempo real al navegador via SignalR.
/// </summary>
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
public class ProjectGeneratorService : IProjectGeneratorService
{
    private readonly IProjectRepository _projects;
    private readonly ITemplateRepository _templates;
    private readonly IShellExecutor _shell;
    private readonly IGitHubService _github;
    private readonly IAiSuggestionService _ai;
    private readonly IConfiguration _config;
    private readonly IGenerationHubNotifier _hub;

    public ProjectGeneratorService(
        IProjectRepository projects,
        ITemplateRepository templates,
        IShellExecutor shell,
        IGitHubService github,
        IAiSuggestionService ai,
        IConfiguration config,
        IGenerationHubNotifier hub)
    {
        _projects  = projects;
        _templates = templates;
        _shell     = shell;
        _github    = github;
        _ai        = ai;
        _config    = config;
        _hub       = hub;
    }

    public async Task<GenerationResult> GenerateAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetFullAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        var cfg = project.WizardConfig;
        var workBase    = _config["Generation:WorkspacePath"] ?? Path.Combine(Path.GetTempPath(), "projectforge");
        var projectPath = Path.Combine(workBase, project.Name.ToLowerInvariant().Replace(" ", "-"));

        try
        {
            await UpdateStatusAsync(project, ProjectStatus.Generating, ct);
            await _hub.SendStatusAsync(projectId, "Generating");

<<<<<<< HEAD
            // 1. Crear carpeta de trabajo
            Directory.CreateDirectory(projectPath);
            await EmitLogAsync(project, "Scaffold", $"📁 Directorio de trabajo: {projectPath}", ct: ct);

            // 2. Scaffolding según arquitectura
            await ScaffoldProjectAsync(project, cfg, projectPath, ct);

            // 3. Aplicar plantillas de BD e infraestructura
            await ApplyTemplatesAsync(project, cfg, projectPath, ct);

            // 4. Instalar dependencias / librerías seleccionadas
            await InstallLibrariesAsync(project, cfg, projectPath, ct);

            // 5. Generar README con IA
            await GenerateReadmeAsync(project, cfg, projectPath, ct);

            // 6. Crear repo GitHub y hacer push
            await EmitLogAsync(project, "GitHub", "🔗 Creando repositorio en GitHub...", ct: ct);
            var repoUrl = await PushToGitHubAsync(project, projectPath, ct);

            project.LocalPath      = projectPath;
            project.RepositoryUrl  = repoUrl;
=======
            Directory.CreateDirectory(projectPath);
            await EmitLogAsync(project, "Scaffold", $"📁 Directorio de trabajo: {projectPath}", ct: ct);

            await ScaffoldProjectAsync(project, cfg, projectPath, ct);
            await ApplyTemplatesAsync(project, cfg, projectPath, ct);
            await InstallLibrariesAsync(project, cfg, projectPath, ct);
            await ApplyDesignPatternsAsync(project, cfg, projectPath, ct);
            await GenerateReadmeAsync(project, cfg, projectPath, ct);

            await EmitLogAsync(project, "GitHub", "🔗 Creando repositorio en GitHub...", ct: ct);
            var repoUrl = await PushToGitHubAsync(project, projectPath, ct);

            project.LocalPath     = projectPath;
            project.RepositoryUrl = repoUrl;
>>>>>>> 0dc2a35 (complete java,python,typescript)
            await UpdateStatusAsync(project, ProjectStatus.Published, ct);
            await _hub.SendStatusAsync(projectId, "Published");

            return new GenerationResult(true, null, projectPath, repoUrl);
        }
        catch (Exception ex)
        {
            await EmitLogAsync(project, "Error", $"❌ {ex.Message}", isError: true, ct: ct);
            project.ErrorMessage = ex.Message;
            await UpdateStatusAsync(project, ProjectStatus.Failed, ct);
            await _hub.SendStatusAsync(projectId, "Failed");
            return new GenerationResult(false, ex.Message, projectPath, null);
        }
    }

<<<<<<< HEAD
    // ─── Paso 2: Scaffold por arquitectura ────────────────────────────────────
=======
    // ── Paso 2: Scaffold ──────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private async Task ScaffoldProjectAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Scaffold", $"🏗️  Iniciando scaffold ({cfg.Architecture} / {cfg.Framework})...", ct: ct);
<<<<<<< HEAD
=======

        // Intentar scaffold con CLIs oficiales (dotnet/spring/npm/...).
        // Si fallan (CLI ausente, sin red en el VPS, etc.) NO abortamos: caemos al
        // fallback que escribe una estructura real de archivos en disco. Así el repo
        // nunca queda con solo el README.
        var cliOk = true;
>>>>>>> 0dc2a35 (complete java,python,typescript)
        var commands = GetScaffoldCommands(cfg, project.Name, path);

        foreach (var cmd in commands)
        {
            await EmitLogAsync(project, "Scaffold", $"$ {cmd.Command}", ct: ct);
<<<<<<< HEAD
            var result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);
=======
            ShellResult result;
            try
            {
                result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);
            }
            catch (Exception ex)
            {
                await EmitLogAsync(project, "Scaffold", $"⚠️ No se pudo ejecutar el CLI: {ex.Message}", isError: true, ct: ct);
                cliOk = false;
                break;
            }
>>>>>>> 0dc2a35 (complete java,python,typescript)

            if (!string.IsNullOrWhiteSpace(result.Stdout))
                await EmitLogAsync(project, "Scaffold", result.Stdout.Trim(), ct: ct);

            if (!result.Success)
            {
<<<<<<< HEAD
                await EmitLogAsync(project, "Scaffold", result.Stderr, isError: true, ct: ct);
                throw new InvalidOperationException($"Scaffold falló: {result.Stderr}");
=======
                await EmitLogAsync(project, "Scaffold",
                    $"⚠️ El comando falló, se usará la estructura base integrada. Detalle: {result.Stderr}",
                    isError: true, ct: ct);
                cliOk = false;
                break;
>>>>>>> 0dc2a35 (complete java,python,typescript)
            }

            await EmitLogAsync(project, "Scaffold", "✅ Comando completado", ct: ct);
        }
<<<<<<< HEAD
=======

        // Siempre garantizamos la estructura base. WriteFallbackStructureAsync NO
        // sobreescribe archivos que el CLI ya haya creado correctamente; solo rellena
        // lo que falte (p. ej. src/index.ts y tsconfig.json cuando solo corrió `npm init`).
        // Si el CLI falló por completo, esto reconstruye todo el proyecto.
        if (!cliOk)
        {
            await EmitLogAsync(project, "Scaffold",
                "🧱 El CLI falló: generando estructura base completa (modo integrado)...", ct: ct);
        }
        else if (!ScaffoldProducedFiles(path))
        {
            await EmitLogAsync(project, "Scaffold",
                "🧱 Generando estructura base de archivos (modo integrado)...", ct: ct);
        }
        else
        {
            await EmitLogAsync(project, "Scaffold",
                "🧱 Completando estructura del proyecto (archivos faltantes)...", ct: ct);
        }

        await WriteFallbackStructureAsync(cfg, project.Name, path, ct);
        await EmitLogAsync(project, "Scaffold", "✅ Estructura del proyecto lista", ct: ct);
    }

    /// <summary>True si el directorio tiene archivos de código reales (no solo README/.git).</summary>
    private static bool ScaffoldProducedFiles(string path)
    {
        if (!Directory.Exists(path)) return false;
        var ignore = new[] { "readme.md", ".gitignore", ".git" };
        return Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.Combine(path, ".git")))
            .Select(p => Path.GetFileName(p).ToLowerInvariant())
            .Any(name => !ignore.Contains(name) && !string.IsNullOrEmpty(Path.GetExtension(name)));
    }

    /// <summary>
    /// Escribe una estructura de proyecto completa y funcional directamente en disco,
    /// sin depender de ningún CLI externo. Cubre Python, Java y TypeScript.
    /// </summary>
    private async Task WriteFallbackStructureAsync(WizardConfig cfg, string projectName, string path, CancellationToken ct)
    {
        var safeName = projectName.Replace(" ", "");
        var lower    = safeName.ToLowerInvariant();
        var files    = new Dictionary<string, string>();

        switch (cfg.Architecture)
        {
            // ─────────────────────────── PYTHON ───────────────────────────
            case ArchitectureType.Python when cfg.Framework == FrameworkType.Django:
                files[$"{lower}/__init__.py"] = "";
                files[$"{lower}/settings.py"] =
                    "from pathlib import Path\n\n" +
                    "BASE_DIR = Path(__file__).resolve().parent.parent\n" +
                    "SECRET_KEY = 'change-me-in-production'\n" +
                    "DEBUG = True\n" +
                    "ALLOWED_HOSTS = ['*']\n\n" +
                    "INSTALLED_APPS = [\n" +
                    "    'django.contrib.admin',\n    'django.contrib.auth',\n" +
                    "    'django.contrib.contenttypes',\n    'django.contrib.sessions',\n" +
                    "    'django.contrib.messages',\n    'django.contrib.staticfiles',\n" +
                    "    'rest_framework',\n    'core',\n]\n\n" +
                    "ROOT_URLCONF = '" + lower + ".urls'\n" +
                    "WSGI_APPLICATION = '" + lower + ".wsgi.application'\n\n" +
                    "DATABASES = {\n    'default': {\n        'ENGINE': 'django.db.backends.sqlite3',\n" +
                    "        'NAME': BASE_DIR / 'db.sqlite3',\n    }\n}\n";
                files[$"{lower}/urls.py"] =
                    "from django.contrib import admin\nfrom django.urls import path\n" +
                    "from core.views import health\n\n" +
                    "urlpatterns = [\n    path('admin/', admin.site.urls),\n" +
                    "    path('health/', health),\n]\n";
                files[$"{lower}/wsgi.py"] =
                    "import os\nfrom django.core.wsgi import get_wsgi_application\n\n" +
                    $"os.environ.setdefault('DJANGO_SETTINGS_MODULE', '{lower}.settings')\n" +
                    "application = get_wsgi_application()\n";
                files["core/__init__.py"] = "";
                files["core/apps.py"] =
                    "from django.apps import AppConfig\n\n" +
                    "class CoreConfig(AppConfig):\n    name = 'core'\n";
                files["core/models.py"] = "from django.db import models\n\n# Define your models here\n";
                files["core/views.py"] =
                    "from django.http import JsonResponse\n\n" +
                    "def health(request):\n    return JsonResponse({'status': 'ok'})\n";
                files["manage.py"] =
                    "#!/usr/bin/env python\nimport os\nimport sys\n\n" +
                    "if __name__ == '__main__':\n" +
                    $"    os.environ.setdefault('DJANGO_SETTINGS_MODULE', '{lower}.settings')\n" +
                    "    from django.core.management import execute_from_command_line\n" +
                    "    execute_from_command_line(sys.argv)\n";
                files["requirements.txt"] =
                    "Django>=5.0.0\ndjangorestframework>=3.15.0\npsycopg2-binary>=2.9.0\npython-dotenv>=1.0.0\n";
                break;

            case ArchitectureType.Python when cfg.Framework == FrameworkType.Flask:
                files["app/__init__.py"] =
                    "from flask import Flask\nfrom app.blueprints.main import main_bp\n\n" +
                    "def create_app():\n    app = Flask(__name__)\n" +
                    "    app.register_blueprint(main_bp)\n    return app\n";
                files["app/blueprints/__init__.py"] = "";
                files["app/blueprints/main.py"] =
                    "from flask import Blueprint, jsonify\n\n" +
                    "main_bp = Blueprint('main', __name__)\n\n" +
                    "@main_bp.route('/health')\ndef health():\n    return jsonify({'status': 'ok'})\n";
                files["app/models/__init__.py"]   = "";
                files["app/services/__init__.py"] = "";
                files["tests/__init__.py"]        = "";
                files["tests/test_health.py"] =
                    "from app import create_app\n\n" +
                    "def test_health():\n    client = create_app().test_client()\n" +
                    "    resp = client.get('/health')\n    assert resp.status_code == 200\n";
                files["run.py"] =
                    "from app import create_app\n\napp = create_app()\n\n" +
                    "if __name__ == '__main__':\n" +
                    "    app.run(host='0.0.0.0', port=8080, debug=True)\n";
                files["requirements.txt"] =
                    "Flask>=3.0.0\nFlask-SQLAlchemy>=3.1.0\nFlask-Migrate>=4.0.0\npython-dotenv>=1.0.0\n";
                break;

            case ArchitectureType.Python: // FastAPI (default)
                files["main.py"] =
                    "from fastapi import FastAPI\nfrom app.api.v1.endpoints import router\n\n" +
                    $"app = FastAPI(title=\"{safeName}\", version=\"1.0.0\")\n\n" +
                    "app.include_router(router, prefix=\"/api/v1\")\n";
                files["app/__init__.py"]                       = "";
                files["app/api/__init__.py"]                   = "";
                files["app/api/v1/__init__.py"]                = "";
                files["app/api/v1/endpoints/__init__.py"] =
                    "from fastapi import APIRouter\n" +
                    "from app.api.v1.endpoints import health\n\n" +
                    "router = APIRouter()\n" +
                    "router.include_router(health.router, tags=[\"health\"])\n";
                files["app/api/v1/endpoints/health.py"] =
                    "from fastapi import APIRouter\n\nrouter = APIRouter()\n\n" +
                    "@router.get(\"/health\")\nasync def health():\n    return {\"status\": \"ok\"}\n";
                files["app/models/__init__.py"]       = "";
                files["app/schemas/__init__.py"]      = "";
                files["app/services/__init__.py"]     = "";
                files["app/repositories/__init__.py"] = "";
                files["app/core/__init__.py"]         = "";
                files["app/core/config.py"] =
                    "import os\n\nclass Settings:\n" +
                    $"    APP_NAME: str = \"{safeName}\"\n" +
                    "    DATABASE_URL: str = os.getenv(\"DATABASE_URL\", \"sqlite:///./app.db\")\n\n" +
                    "settings = Settings()\n";
                files["tests/__init__.py"] = "";
                files["tests/test_health.py"] =
                    "from fastapi.testclient import TestClient\nfrom main import app\n\n" +
                    "def test_health():\n    client = TestClient(app)\n" +
                    "    resp = client.get(\"/api/v1/health\")\n    assert resp.status_code == 200\n";
                files["requirements.txt"] =
                    $"fastapi=={cfg.FrameworkVersion}\nuvicorn[standard]>=0.29.0\npydantic>=2.0.0\n" +
                    "sqlalchemy>=2.0.0\nalembic>=1.13.0\npython-dotenv>=1.0.0\n";
                files[".env.example"] =
                    $"DATABASE_URL=postgresql://user:password@localhost:5432/{lower}_db\n" +
                    "DEBUG=True\nSECRET_KEY=change-me-in-production\n";
                break;

            // ──────────────────────────── JAVA ────────────────────────────
            case ArchitectureType.Java:
            {
                var pkgPath = $"src/main/java/com/projectforge/{lower}";
                var testPath = $"src/test/java/com/projectforge/{lower}";
                files["pom.xml"] = BuildSpringPom(lower);
                files[$"{pkgPath}/Application.java"] =
                    $"package com.projectforge.{lower};\n\n" +
                    "import org.springframework.boot.SpringApplication;\n" +
                    "import org.springframework.boot.autoconfigure.SpringBootApplication;\n\n" +
                    "@SpringBootApplication\n" +
                    "public class Application {\n" +
                    "    public static void main(String[] args) {\n" +
                    "        SpringApplication.run(Application.class, args);\n" +
                    "    }\n}\n";
                files[$"{pkgPath}/controller/HealthController.java"] =
                    $"package com.projectforge.{lower}.controller;\n\n" +
                    "import org.springframework.web.bind.annotation.GetMapping;\n" +
                    "import org.springframework.web.bind.annotation.RequestMapping;\n" +
                    "import org.springframework.web.bind.annotation.RestController;\n" +
                    "import java.util.Map;\n\n" +
                    "@RestController\n@RequestMapping(\"/api/v1\")\n" +
                    "public class HealthController {\n" +
                    "    @GetMapping(\"/health\")\n" +
                    "    public Map<String, String> health() {\n" +
                    "        return Map.of(\"status\", \"ok\");\n" +
                    "    }\n}\n";
                files["src/main/resources/application.properties"] =
                    $"spring.application.name={lower}\nserver.port=8080\n" +
                    "management.endpoints.web.exposure.include=health,info\n";
                files[$"{testPath}/ApplicationTests.java"] =
                    $"package com.projectforge.{lower};\n\n" +
                    "import org.junit.jupiter.api.Test;\n" +
                    "import org.springframework.boot.test.context.SpringBootTest;\n\n" +
                    "@SpringBootTest\nclass ApplicationTests {\n" +
                    "    @Test\n    void contextLoads() {}\n}\n";
                break;
            }

            // ──────────────────── TYPESCRIPT / JAVASCRIPT ────────────────────
            case ArchitectureType.TypeScript:
            case ArchitectureType.JavaScript:
            {
                var isTs = cfg.Architecture == ArchitectureType.TypeScript;
                var ext  = isTs ? "ts" : "js";
                files["package.json"] = BuildPackageJson(lower, isTs);
                if (isTs)
                {
                    files["tsconfig.json"] =
                        "{\n  \"compilerOptions\": {\n" +
                        "    \"target\": \"ES2022\",\n    \"module\": \"commonjs\",\n" +
                        "    \"rootDir\": \"./src\",\n    \"outDir\": \"./dist\",\n" +
                        "    \"esModuleInterop\": true,\n    \"strict\": true,\n" +
                        "    \"skipLibCheck\": true\n  },\n" +
                        "  \"include\": [\"src/**/*\"]\n}\n";
                }
                files[$"src/index.{ext}"] = isTs
                    ? "import express, { Request, Response } from 'express';\n" +
                      "import { healthRouter } from './routes/health';\n\n" +
                      "const app = express();\napp.use(express.json());\n" +
                      "app.use('/api/v1', healthRouter);\n\n" +
                      "const PORT = process.env.PORT || 8080;\n" +
                      "app.listen(PORT, () => console.log(`Server running on port ${PORT}`));\n\n" +
                      "export default app;\n"
                    : "const express = require('express');\n" +
                      "const { healthRouter } = require('./routes/health');\n\n" +
                      "const app = express();\napp.use(express.json());\n" +
                      "app.use('/api/v1', healthRouter);\n\n" +
                      "const PORT = process.env.PORT || 8080;\n" +
                      "app.listen(PORT, () => console.log(`Server running on port ${PORT}`));\n\n" +
                      "module.exports = app;\n";
                files[$"src/routes/health.{ext}"] = isTs
                    ? "import { Router, Request, Response } from 'express';\n\n" +
                      "export const healthRouter = Router();\n\n" +
                      "healthRouter.get('/health', (_req: Request, res: Response) => {\n" +
                      "  res.json({ status: 'ok' });\n});\n"
                    : "const { Router } = require('express');\n\n" +
                      "const healthRouter = Router();\n\n" +
                      "healthRouter.get('/health', (_req, res) => {\n" +
                      "  res.json({ status: 'ok' });\n});\n\n" +
                      "module.exports = { healthRouter };\n";
                files[$"src/services/.gitkeep"] = "";
                files[$"src/models/.gitkeep"]   = "";
                files[".env.example"] = "PORT=8080\nNODE_ENV=development\n";
                break;
            }

            // ───────────────────────── GENÉRICO ─────────────────────────
            default:
                files["src/.gitkeep"]   = "";
                files["tests/.gitkeep"] = "";
                break;
        }

        // .gitignore garantizado por arquitectura. Crítico para que node_modules,
        // venv, target/, etc. NUNCA terminen en el repo.
        files[".gitignore"] = BuildGitignore(cfg.Architecture);

        foreach (var (relPath, content) in files)
        {
            var full = Path.Combine(path, relPath);

            // NO sobreescribir archivos que el CLI ya generó correctamente
            // (p. ej. package.json de NestJS, pom.xml de Spring Initializr).
            // Excepción: .gitignore siempre se (re)escribe para garantizar exclusiones.
            if (File.Exists(full) && relPath != ".gitignore")
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            await File.WriteAllTextAsync(full, content, ct);
        }

        // Si el CLI ya había hecho commit/instalado node_modules en un intento previo,
        // nos aseguramos de que git deje de rastrearlo (el .gitignore solo afecta a archivos no rastreados).
        // Esto se ejecuta en el push; aquí basta con dejar el .gitignore correcto.
    }

    private static string BuildGitignore(ArchitectureType arch)
    {
        var common = "# OS / IDE\n.DS_Store\nThumbs.db\n.idea/\n.vscode/\n*.log\n.env\n.env.local\n\n";

        return arch switch
        {
            ArchitectureType.JavaScript or ArchitectureType.TypeScript =>
                common +
                "# Node\nnode_modules/\ndist/\nbuild/\ncoverage/\n*.tsbuildinfo\nnpm-debug.log*\nyarn-debug.log*\nyarn-error.log*\n",

            ArchitectureType.Python =>
                common +
                "# Python\n__pycache__/\n*.py[cod]\n*.egg-info/\n.eggs/\nvenv/\n.venv/\nenv/\n.pytest_cache/\n.mypy_cache/\ndb.sqlite3\n*.db\n",

            ArchitectureType.Java =>
                common +
                "# Java / Maven\ntarget/\n*.class\n*.jar\n*.war\n.mvn/\n!.mvn/wrapper/maven-wrapper.jar\n",

            ArchitectureType.DotNet =>
                common +
                "# .NET\nbin/\nobj/\n*.user\n",

            ArchitectureType.Laravel =>
                common +
                "# Laravel\n/vendor/\n/node_modules/\n/public/storage\n/storage/*.key\n",

            _ => common + "node_modules/\nvendor/\ntarget/\nbin/\nobj/\n__pycache__/\n"
        };
    }

    private static string BuildSpringPom(string artifactId) =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
        "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\n" +
        "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
        "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\n" +
        "    <modelVersion>4.0.0</modelVersion>\n" +
        "    <parent>\n" +
        "        <groupId>org.springframework.boot</groupId>\n" +
        "        <artifactId>spring-boot-starter-parent</artifactId>\n" +
        "        <version>3.5.3</version>\n" +
        "        <relativePath/>\n" +
        "    </parent>\n" +
        "    <groupId>com.projectforge</groupId>\n" +
        $"    <artifactId>{artifactId}</artifactId>\n" +
        "    <version>1.0.0</version>\n" +
        $"    <name>{artifactId}</name>\n" +
        "    <properties>\n        <java.version>21</java.version>\n    </properties>\n" +
        "    <dependencies>\n" +
        "        <dependency>\n            <groupId>org.springframework.boot</groupId>\n" +
        "            <artifactId>spring-boot-starter-web</artifactId>\n        </dependency>\n" +
        "        <dependency>\n            <groupId>org.springframework.boot</groupId>\n" +
        "            <artifactId>spring-boot-starter-actuator</artifactId>\n        </dependency>\n" +
        "        <dependency>\n            <groupId>org.springframework.boot</groupId>\n" +
        "            <artifactId>spring-boot-starter-data-jpa</artifactId>\n        </dependency>\n" +
        "        <dependency>\n            <groupId>org.springframework.boot</groupId>\n" +
        "            <artifactId>spring-boot-starter-validation</artifactId>\n        </dependency>\n" +
        "        <dependency>\n            <groupId>org.springframework.boot</groupId>\n" +
        "            <artifactId>spring-boot-starter-test</artifactId>\n" +
        "            <scope>test</scope>\n        </dependency>\n" +
        "    </dependencies>\n" +
        "    <build>\n        <plugins>\n            <plugin>\n" +
        "                <groupId>org.springframework.boot</groupId>\n" +
        "                <artifactId>spring-boot-maven-plugin</artifactId>\n" +
        "            </plugin>\n        </plugins>\n    </build>\n" +
        "</project>\n";

    private static string BuildPackageJson(string name, bool isTs)
    {
        if (isTs)
            return "{\n" +
                $"  \"name\": \"{name}\",\n  \"version\": \"1.0.0\",\n" +
                "  \"main\": \"dist/index.js\",\n" +
                "  \"scripts\": {\n" +
                "    \"build\": \"tsc\",\n" +
                "    \"start\": \"node dist/index.js\",\n" +
                "    \"dev\": \"ts-node src/index.ts\",\n" +
                "    \"test\": \"echo \\\"no tests yet\\\" && exit 0\"\n  },\n" +
                "  \"dependencies\": {\n    \"express\": \"^4.19.2\"\n  },\n" +
                "  \"devDependencies\": {\n" +
                "    \"@types/express\": \"^4.17.21\",\n" +
                "    \"@types/node\": \"^20.11.0\",\n" +
                "    \"ts-node\": \"^10.9.2\",\n" +
                "    \"typescript\": \"^5.4.0\"\n  }\n}\n";

        return "{\n" +
            $"  \"name\": \"{name}\",\n  \"version\": \"1.0.0\",\n" +
            "  \"main\": \"src/index.js\",\n" +
            "  \"scripts\": {\n" +
            "    \"start\": \"node src/index.js\",\n" +
            "    \"test\": \"echo \\\"no tests yet\\\" && exit 0\"\n  },\n" +
            "  \"dependencies\": {\n    \"express\": \"^4.19.2\"\n  }\n}\n";
>>>>>>> 0dc2a35 (complete java,python,typescript)
    }

    private static IEnumerable<(string Command, string? WorkingDir)> GetScaffoldCommands(
        WizardConfig cfg, string projectName, string path)
    {
        var safeName = projectName.Replace(" ", "");

        return cfg.Architecture switch
        {
            ArchitectureType.DotNet => cfg.Framework switch
            {
                FrameworkType.AspNetCoreWebApi => new[]
                {
                    ($"dotnet new webapi -n {safeName} -o {path} --no-https false", (string?)null),
                    ($"dotnet new sln -n {safeName}", path),
                    ($"dotnet sln add {safeName}.csproj", path),
                },
                FrameworkType.AspNetCoreMVC => new[]
                {
                    ($"dotnet new mvc -n {safeName} -o {path}", (string?)null),
                },
                FrameworkType.BlazorServer => new[]
                {
                    ($"dotnet new blazorserver -n {safeName} -o {path}", (string?)null),
                },
                _ => new[] { ($"dotnet new webapi -n {safeName} -o {path}", (string?)null) }
            },

            ArchitectureType.Python => cfg.Framework switch
            {
                FrameworkType.FastAPI => new[]
                {
<<<<<<< HEAD
                    ($"mkdir -p {path}/app/api/v1 {path}/app/models {path}/app/services {path}/tests", (string?)null),
                    ($"python3 -m venv {path}/venv", null),
                },
                FrameworkType.Django => new[]
                {
                    ($"django-admin startproject {safeName} {path}", (string?)null),
=======
                    ($"mkdir -p {path}/app/api/v1/endpoints {path}/app/models " +
                     $"{path}/app/schemas {path}/app/services {path}/app/repositories " +
                     $"{path}/app/core {path}/tests", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'from fastapi import FastAPI' " +
                     $"'from app.api.v1.endpoints import router' " +
                     $"'' " +
                     $"'app = FastAPI(title=\"{safeName}\", version=\"1.0.0\")' " +
                     $"'' " +
                     $"\"app.include_router(router, prefix='/api/v1')\" " +
                     $"> {path}/main.py", (string?)null),
                    ($"touch {path}/app/__init__.py " +
                     $"{path}/app/api/__init__.py " +
                     $"{path}/app/api/v1/__init__.py " +
                     $"{path}/app/api/v1/endpoints/__init__.py " +
                     $"{path}/app/models/__init__.py " +
                     $"{path}/app/schemas/__init__.py " +
                     $"{path}/app/services/__init__.py " +
                     $"{path}/app/repositories/__init__.py " +
                     $"{path}/app/core/__init__.py " +
                     $"{path}/tests/__init__.py", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'from fastapi import APIRouter' " +
                     $"'' " +
                     $"'router = APIRouter()' " +
                     $"'' " +
                     $"'@router.get(\"/health\")' " +
                     $"'async def health():' " +
                     $"'    return {{\"status\": \"ok\"}}' " +
                     $"> {path}/app/api/v1/endpoints/health.py && " +
                     $"printf '%s\\n' " +
                     $"'from fastapi import APIRouter' " +
                     $"'from app.api.v1.endpoints import health' " +
                     $"'' " +
                     $"'router = APIRouter()' " +
                     $"'router.include_router(health.router, tags=[\"health\"])' " +
                     $"> {path}/app/api/v1/endpoints/__init__.py", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'fastapi=={cfg.FrameworkVersion}' " +
                     $"'uvicorn[standard]>=0.29.0' " +
                     $"'pydantic>=2.0.0' " +
                     $"'sqlalchemy>=2.0.0' " +
                     $"'alembic>=1.13.0' " +
                     $"'python-dotenv>=1.0.0' " +
                     $"> {path}/requirements.txt", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'DATABASE_URL=postgresql://user:password@localhost:5432/{safeName.ToLower()}_db' " +
                     $"'DEBUG=True' " +
                     $"'SECRET_KEY=change-me-in-production' " +
                     $"> {path}/.env.example", (string?)null),
                    ($"python3 -m venv {path}/venv", (string?)null),
                },
                FrameworkType.Django => new[]
                {
                    ($"pip install django --quiet --break-system-packages && django-admin startproject {safeName} {path}", (string?)null),
                    ($"cd {path} && python manage.py startapp core", path),
                    ($"printf '%s\\n' " +
                     $"'Django>=5.0.0' " +
                     $"'djangorestframework>=3.15.0' " +
                     $"'psycopg2-binary>=2.9.0' " +
                     $"'python-dotenv>=1.0.0' " +
                     $"> {path}/requirements.txt", (string?)null),
                },
                FrameworkType.Flask => new[]
                {
                    ($"mkdir -p {path}/app/blueprints {path}/app/models " +
                     $"{path}/app/services {path}/tests {path}/migrations", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'from flask import Flask' " +
                     $"'from app.blueprints.main import main_bp' " +
                     $"'' " +
                     $"'def create_app():' " +
                     $"'    app = Flask(__name__)' " +
                     $"'    app.register_blueprint(main_bp)' " +
                     $"'    return app' " +
                     $"> {path}/app/__init__.py", (string?)null),
                    ($"touch {path}/app/blueprints/__init__.py " +
                     $"{path}/app/models/__init__.py " +
                     $"{path}/app/services/__init__.py " +
                     $"{path}/tests/__init__.py", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'from flask import Blueprint, jsonify' " +
                     $"'' " +
                     $"'main_bp = Blueprint(\"main\", __name__)' " +
                     $"'' " +
                     $"\"@main_bp.route('/health')\" " +
                     $"'def health():' " +
                     $"'    return jsonify({{\"status\": \"ok\"}})' " +
                     $"> {path}/app/blueprints/main.py", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'from app import create_app' " +
                     $"'' " +
                     $"'app = create_app()' " +
                     $"'' " +
                     $"'if __name__ == \"__main__\":' " +
                     $"'    app.run(host=\"0.0.0.0\", port=8080, debug=True)' " +
                     $"> {path}/run.py", (string?)null),
                    ($"printf '%s\\n' " +
                     $"'Flask>=3.0.0' " +
                     $"'Flask-SQLAlchemy>=3.1.0' " +
                     $"'Flask-Migrate>=4.0.0' " +
                     $"'python-dotenv>=1.0.0' " +
                     $"> {path}/requirements.txt", (string?)null),
                    ($"python3 -m venv {path}/venv", (string?)null),
>>>>>>> 0dc2a35 (complete java,python,typescript)
                },
                _ => new[] { ($"mkdir -p {path}/src {path}/tests", (string?)null) }
            },

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => new[]
            {
                ($"npm init -y", path),
                cfg.Framework == FrameworkType.NestJs
                    ? ($"npm i -g @nestjs/cli && nest new {safeName} --directory . --skip-git", path)
                    : ($"npm install express", path),
            },

<<<<<<< HEAD
            ArchitectureType.Java => new[]
            {
                ($"curl -s https://start.spring.io/starter.zip " +
                 $"-d type=maven-project -d language=java -d bootVersion={cfg.FrameworkVersion} " +
                 $"-d artifactId={safeName.ToLower()} -d packaging=jar " +
                 $"-d dependencies=web,actuator -o {path}/project.zip && " +
                 $"cd {path} && unzip -q project.zip && rm project.zip", (string?)null),
=======
            ArchitectureType.Java => cfg.Framework switch
            {
                FrameworkType.SpringBoot => new[]
{
    ($"rm -rf {path}/* {path}/.* 2>/dev/null || true && " +
     $"BOOT_VER=3.5.3 && " +
     $"echo \"Descargando Spring Boot $BOOT_VER desde start.spring.io...\" && " +
     $"curl -f -L --max-time 60 " +
     $"\"https://start.spring.io/starter.zip?type=maven-project&language=java&bootVersion=$BOOT_VER&artifactId={safeName.ToLower()}&groupId=com.projectforge&packaging=jar&javaVersion=21&dependencies=web,actuator,data-jpa,validation\" " +
     $"-o {path}/project.zip 2>&1 && " +
     $"unzip -tq {path}/project.zip > /dev/null 2>&1 || " +
     $"(echo 'ERROR: el archivo descargado no es un ZIP valido' && cat {path}/project.zip && rm -f {path}/project.zip && exit 1) && " +
     $"cd {path} && unzip -qo project.zip && rm project.zip && " +
     $"ls {path}/pom.xml > /dev/null 2>&1 || " +
     $"(echo 'ERROR: pom.xml no encontrado tras descomprimir, revisando contenido:' && ls -la {path}/ && exit 1)", (string?)null),
},
                FrameworkType.Quarkus => new[]
                {
                    ($"mkdir -p {path}/src/main/java/com/projectforge/{safeName.ToLower()} " +
                     $"{path}/src/main/resources {path}/src/test/java/com/projectforge/{safeName.ToLower()}", (string?)null),
                    ($"curl -f -L --max-time 60 " +
                     $"\"https://code.quarkus.io/api/download?artifactId={safeName.ToLower()}&groupId=com.projectforge&extensions=quarkus-resteasy-reactive,quarkus-smallrye-health\" " +
                     $"-o {path}/quarkus.zip 2>&1 && " +
                     $"unzip -tq {path}/quarkus.zip > /dev/null 2>&1 || " +
                     $"(echo 'ERROR: descarga de Quarkus invalida' && rm -f {path}/quarkus.zip && exit 1) && " +
                     $"cd {path} && unzip -q quarkus.zip -d . && rm quarkus.zip && " +
                     $"mv {safeName.ToLower()}/* . 2>/dev/null || true && rmdir {safeName.ToLower()} 2>/dev/null || true", (string?)null),
                },
                _ => new[]
                {
                    ($"BOOT_VER=3.5.3 && " +
                     $"echo \"Descargando Spring Boot $BOOT_VER...\" && " +
                     $"curl -f -L --max-time 60 " +
                     $"\"https://start.spring.io/starter.zip?type=maven-project&language=java&bootVersion=$BOOT_VER&artifactId={safeName.ToLower()}&groupId=com.projectforge&packaging=jar&javaVersion=21&dependencies=web,actuator\" " +
                     $"-o {path}/project.zip 2>&1 && " +
                     $"unzip -tq {path}/project.zip > /dev/null 2>&1 || " +
                     $"(echo 'ERROR: el archivo descargado no es un ZIP valido' && cat {path}/project.zip && rm -f {path}/project.zip && exit 1) && " +
                     $"cd {path} && unzip -qo project.zip && rm project.zip", (string?)null),
                }
>>>>>>> 0dc2a35 (complete java,python,typescript)
            },

            ArchitectureType.Laravel => new[]
            {
                ($"composer create-project laravel/laravel {path}", (string?)null),
            },

            _ => Array.Empty<(string, string?)>()
        };
    }

<<<<<<< HEAD
    // ─── Paso 3: Aplicar plantillas ───────────────────────────────────────────
=======
    // ── Paso 3: Plantillas ────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private async Task ApplyTemplatesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Templates", "📄 Aplicando plantillas de infraestructura...", ct: ct);

        var vars = new Dictionary<string, string>
        {
            ["APP_NAME"] = project.Name.ToLower().Replace(" ", "-"),
            ["DB_NAME"]  = $"{project.Name.ToLower().Replace(" ", "_")}_db",
            ["DB_PORT"]  = GetDefaultDbPort(cfg.Database).ToString(),
            ["APP_PORT"] = "8080"
        };

        if (cfg.Infrastructure == InfrastructureType.DockerCompose)
        {
            var tpl = await _templates.GetTemplateAsync(cfg.Architecture, "compose", cfg.Database, InfrastructureType.DockerCompose);
            if (tpl != null)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "docker-compose.yml"), InterpolateTemplate(tpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ docker-compose.yml generado", ct: ct);
            }

            var dockerfile = await _templates.GetTemplateAsync(cfg.Architecture, "dockerfile");
            if (dockerfile != null)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "Dockerfile"), InterpolateTemplate(dockerfile.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ Dockerfile generado", ct: ct);
            }
        }

        if (cfg.Infrastructure == InfrastructureType.Kubernetes)
        {
            var k8sDir = Path.Combine(path, "k8s");
            Directory.CreateDirectory(k8sDir);
            var templates = await _templates.GetInfraTemplatesAsync(InfrastructureType.Kubernetes, cfg.Database);
            foreach (var tpl in templates)
            {
                var filename = $"{tpl.Name.ToLower().Replace(" ", "-")}.yaml";
                await File.WriteAllTextAsync(Path.Combine(k8sDir, filename), InterpolateTemplate(tpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", $"✅ k8s/{filename} generado", ct: ct);
            }
        }

        var gitignore = await _templates.GetTemplateAsync(cfg.Architecture, "gitignore");
        if (gitignore != null)
        {
            await File.WriteAllTextAsync(Path.Combine(path, ".gitignore"), gitignore.Content, ct);
            await EmitLogAsync(project, "Templates", "✅ .gitignore generado", ct: ct);
        }

        var ciDir = Path.Combine(path, ".github", "workflows");
        Directory.CreateDirectory(ciDir);
        var ciTemplate = await _templates.GetTemplateAsync(cfg.Architecture, "ci");
        if (ciTemplate != null)
        {
            await File.WriteAllTextAsync(Path.Combine(ciDir, "ci.yml"), InterpolateTemplate(ciTemplate.Content, vars), ct);
            await EmitLogAsync(project, "Templates", "✅ .github/workflows/ci.yml generado", ct: ct);
        }
    }

<<<<<<< HEAD
    // ─── Paso 4: Instalar dependencias ────────────────────────────────────────
=======
    // ── Paso 4: Dependencias ──────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private async Task InstallLibrariesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var libs = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();
        if (!libs.Any())
        {
            await EmitLogAsync(project, "Dependencies", "ℹ️  Sin librerías adicionales seleccionadas", ct: ct);
            return;
        }

        await EmitLogAsync(project, "Dependencies", $"📦 Instalando {libs.Count} librerías...", ct: ct);

<<<<<<< HEAD
        foreach (var cmd in GetInstallCommands(cfg.Architecture, cfg.Framework, libs))
=======
        foreach (var cmd in GetInstallCommands(cfg.Architecture, cfg.Framework, libs, path))
>>>>>>> 0dc2a35 (complete java,python,typescript)
        {
            await EmitLogAsync(project, "Dependencies", $"$ {cmd}", ct: ct);
            var result = await _shell.RunAsync(cmd, path, ct);
            if (result.Success)
                await EmitLogAsync(project, "Dependencies", "✅ Instalado", ct: ct);
            else
                await EmitLogAsync(project, "Dependencies", $"⚠️ Advertencia: {result.Stderr}", isError: true, ct: ct);
        }
    }

<<<<<<< HEAD
    private static IEnumerable<string> GetInstallCommands(ArchitectureType arch, FrameworkType fw, List<string> libs) =>
        arch switch
        {
            ArchitectureType.DotNet                                     => libs.Select(l => $"dotnet add package {l}"),
            ArchitectureType.Python                                     => new[] { $"pip install {string.Join(" ", libs)}" },
            ArchitectureType.JavaScript or ArchitectureType.TypeScript  => new[] { $"npm install {string.Join(" ", libs)}" },
            ArchitectureType.Laravel                                    => new[] { $"composer require {string.Join(" ", libs)}" },
            _                                                           => Enumerable.Empty<string>()
        };

    // ─── Paso 5: README con IA ────────────────────────────────────────────────
=======
    private static IEnumerable<string> GetInstallCommands(ArchitectureType arch, FrameworkType fw, List<string> libs, string projectPath) =>
        arch switch
        {
            ArchitectureType.DotNet                                    => libs.Select(l => $"dotnet add package {l}"),
            // Python: añadimos cada lib a requirements.txt (sin duplicar) y, si hay venv, la instalamos.
            ArchitectureType.Python                                    => libs.SelectMany(l => new[]
            {
                $"grep -qxF '{l}' \"{projectPath}/requirements.txt\" 2>/dev/null || echo '{l}' >> \"{projectPath}/requirements.txt\"",
                $"[ -f \"{projectPath}/venv/bin/pip\" ] && \"{projectPath}/venv/bin/pip\" install {l} || pip install --break-system-packages {l} || true",
            }),
            // npm install --save escribe la dependencia en package.json automáticamente.
            ArchitectureType.JavaScript or ArchitectureType.TypeScript => new[] { $"npm install --save {string.Join(" ", libs)}" },
            ArchitectureType.Laravel                                   => new[] { $"composer require {string.Join(" ", libs)}" },
            // Java: insertamos cada dependencia (groupId:artifactId) en el <dependencies> del pom.xml.
            ArchitectureType.Java                                      => libs.Select(l => BuildMavenInsertCommand(l, projectPath)),
            _                                                          => Enumerable.Empty<string>()
        };

    /// <summary>
    /// Inserta una dependencia Maven (formato "groupId:artifactId" o "groupId:artifactId:version")
    /// dentro del bloque &lt;dependencies&gt; del pom.xml. El script Python que hace la edición
    /// se transporta en base64 para evitar cualquier conflicto de comillas con el wrapper
    /// `bash -c "..."` del ShellExecutor. Si el artifactId ya existe, no la duplica.
    /// </summary>
    private static string BuildMavenInsertCommand(string artifact, string projectPath)
    {
        var parts = artifact.Split(':');
        var groupId    = parts.Length > 0 ? parts[0] : artifact;
        var artifactId = parts.Length > 1 ? parts[1] : artifact;
        var version    = parts.Length > 2 ? parts[2] : "";

        var pom = $"{projectPath}/pom.xml";

        // Script Python con comillas dobles internas (no chocan: va en base64).
        var py =
            "import os\n" +
            $"pom = r\"{pom}\"\n" +
            $"gid, aid, ver = \"{groupId}\", \"{artifactId}\", \"{version}\"\n" +
            "src = open(pom, encoding=\"utf-8\").read() if os.path.exists(pom) else \"\"\n" +
            "present = (\"<artifactId>\" + aid + \"</artifactId>\") in src\n" +
            "vt = (\"    <version>\" + ver + \"</version>\\n\") if ver else \"\"\n" +
            "dep = \"        <dependency>\\n            <groupId>\" + gid + \"</groupId>\\n            <artifactId>\" + aid + \"</artifactId>\\n\" + vt + \"        </dependency>\\n\"\n" +
            "if src and not present and \"</dependencies>\" in src:\n" +
            "    out = src.replace(\"</dependencies>\", dep + \"    </dependencies>\", 1)\n" +
            "    open(pom, \"w\", encoding=\"utf-8\").write(out)\n" +
            "    print(\"pom.xml: + \" + aid)\n" +
            "else:\n" +
            "    print(\"pom.xml: \" + aid + \" omitido (ya presente o sin <dependencies>)\")\n";

        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(py));
        // base64 no contiene comillas ni metacaracteres de shell → seguro dentro de bash -c.
        return $"echo {b64} | base64 -d | python3 -";
    }

    // ── Paso 4.5: Aplicar patrones de diseño ──────────────────────────────────

    /// <summary>
    /// Crea estructura de carpetas y archivos de ejemplo según el patrón de diseño
    /// seleccionado, para que la carpeta generada refleje realmente la elección del
    /// usuario (y no quede solo con el scaffold base del framework).
    /// </summary>
    private async Task ApplyDesignPatternsAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var patterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? new();
        if (!patterns.Any())
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  Sin patrones de diseño seleccionados", ct: ct);
            return;
        }

        await EmitLogAsync(project, "Patterns", $"🧩 Aplicando {patterns.Count} patrón(es) de diseño...", ct: ct);

        var sb = new StringBuilder();
        sb.AppendLine($"# Patrones de Diseño aplicados — {project.Name}");
        sb.AppendLine();
        sb.AppendLine($"Lenguaje/Arquitectura: **{cfg.Architecture}** · Framework: **{cfg.Framework}**");
        sb.AppendLine();

        foreach (var pattern in patterns)
        {
            var (dirs, notes) = GetPatternScaffold(cfg.Architecture, pattern);

            foreach (var dir in dirs)
            {
                var full = Path.Combine(path, dir);
                try
                {
                    Directory.CreateDirectory(full);
                    // .gitkeep para que las carpetas vacías se versionen en Git.
                    var keep = Path.Combine(full, ".gitkeep");
                    if (!File.Exists(keep)) await File.WriteAllTextAsync(keep, "", ct);
                }
                catch (Exception ex)
                {
                    await EmitLogAsync(project, "Patterns",
                        $"⚠️ No se pudo crear {dir}: {ex.Message}", isError: true, ct: ct);
                }
            }

            sb.AppendLine($"## {pattern}");
            sb.AppendLine();
            sb.AppendLine(notes);
            if (dirs.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Estructura creada:");
                foreach (var d in dirs) sb.AppendLine($"- `{d}/`");
            }
            sb.AppendLine();

            await EmitLogAsync(project, "Patterns", $"✅ Patrón aplicado: {pattern}", ct: ct);
        }

        await File.WriteAllTextAsync(Path.Combine(path, "PATTERNS.md"), sb.ToString(), ct);
        await EmitLogAsync(project, "Patterns", "✅ PATTERNS.md generado", ct: ct);
    }

    /// <summary>
    /// Devuelve las carpetas a crear y una nota explicativa para un patrón concreto,
    /// adaptadas al lenguaje. Las rutas son relativas a la raíz del proyecto generado.
    /// </summary>
    private static (string[] Dirs, string Notes) GetPatternScaffold(ArchitectureType arch, string pattern)
    {
        var p = pattern.ToLowerInvariant();

        // Base de paquetes Java
        const string javaBase = "src/main/java/com/projectforge";

        // Repository
        if (p.Contains("repository"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/domain/model", $"{javaBase}/domain/repository", $"{javaBase}/infrastructure/persistence" },
                                                 "Patrón Repository: las interfaces de repositorio viven en `domain/repository` y sus implementaciones (Spring Data JPA) en `infrastructure/persistence`."),
                ArchitectureType.Python     => (new[] { "app/domain", "app/repositories", "app/infrastructure" },
                                                 "Patrón Repository: define interfaces en `app/repositories` y conecta SQLAlchemy en `app/infrastructure`, manteniendo el dominio aislado."),
                ArchitectureType.TypeScript => (new[] { "src/domain", "src/repositories", "src/infrastructure" },
                                                 "Patrón Repository: interfaces en `src/repositories` e implementaciones (Prisma/TypeORM) en `src/infrastructure`."),
                _                            => (new[] { "src/repositories" },
                                                 "Patrón Repository: abstrae el acceso a datos detrás de interfaces."),
            };

        // Clean Architecture
        if (p.Contains("clean"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/domain", $"{javaBase}/application", $"{javaBase}/infrastructure", $"{javaBase}/presentation" },
                                                 "Clean Architecture: capas concéntricas (domain → application → infrastructure/presentation) con dependencias hacia el centro."),
                ArchitectureType.Python     => (new[] { "app/domain", "app/application", "app/infrastructure", "app/presentation" },
                                                 "Clean Architecture: capas domain/application/infrastructure/presentation."),
                ArchitectureType.TypeScript => (new[] { "src/domain", "src/application", "src/infrastructure", "src/presentation" },
                                                 "Clean Architecture: capas domain/application/infrastructure/presentation."),
                _                            => (new[] { "src/domain", "src/application", "src/infrastructure" },
                                                 "Clean Architecture: separación en capas."),
            };

        // DDD
        if (p.Contains("domain-driven") || p.Contains("ddd"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/domain/model", $"{javaBase}/domain/event", $"{javaBase}/domain/service", $"{javaBase}/application" },
                                                 "DDD: Aggregates y Entities en `domain/model`, Domain Events en `domain/event`, Domain Services en `domain/service`."),
                ArchitectureType.Python     => (new[] { "app/domain/model", "app/domain/events", "app/domain/services", "app/application" },
                                                 "DDD: agregados, entidades, value objects y domain events."),
                ArchitectureType.TypeScript => (new[] { "src/domain/model", "src/domain/events", "src/domain/services", "src/application" },
                                                 "DDD: agregados, entidades, value objects y domain events."),
                _                            => (new[] { "src/domain" }, "Domain-Driven Design."),
            };

        // CQRS
        if (p.Contains("cqrs"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/application/command", $"{javaBase}/application/query" },
                                                 "CQRS: comandos (escritura) en `application/command` y queries (lectura) en `application/query`."),
                ArchitectureType.Python     => (new[] { "app/application/commands", "app/application/queries" },
                                                 "CQRS: separa comandos y consultas."),
                ArchitectureType.TypeScript => (new[] { "src/application/commands", "src/application/queries" },
                                                 "CQRS: separa comandos y consultas."),
                _                            => (new[] { "src/commands", "src/queries" }, "CQRS."),
            };

        // Hexagonal
        if (p.Contains("hexagonal") || p.Contains("ports"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/domain", $"{javaBase}/application/port/in", $"{javaBase}/application/port/out", $"{javaBase}/adapter/in", $"{javaBase}/adapter/out" },
                                                 "Hexagonal (Ports & Adapters): puertos en `application/port` y adaptadores en `adapter/in` (driving) y `adapter/out` (driven)."),
                ArchitectureType.TypeScript => (new[] { "src/domain", "src/application/ports", "src/adapters/in", "src/adapters/out" },
                                                 "Hexagonal (Ports & Adapters): puertos y adaptadores entrantes/salientes."),
                ArchitectureType.Python     => (new[] { "app/domain", "app/ports", "app/adapters" },
                                                 "Hexagonal (Ports & Adapters)."),
                _                            => (new[] { "src/ports", "src/adapters" }, "Hexagonal."),
            };

        // Microservices
        if (p.Contains("microservice"))
            return (new[] { "services", "gateway", "shared" },
                    "Microservicios: cada servicio independiente en `services/`, un `gateway/` de entrada y código común en `shared/`.");

        // MVC / MVT
        if (p.Contains("mvc") || p.Contains("model-view"))
            return arch switch
            {
                ArchitectureType.Java       => (new[] { $"{javaBase}/controller", $"{javaBase}/model", $"{javaBase}/view", $"{javaBase}/service" },
                                                 "MVC: controllers, models, views y services separados."),
                ArchitectureType.Python     => (new[] { "app/models", "app/views", "app/templates", "app/controllers" },
                                                 "MVT/MVC: modelos, vistas, templates y controladores."),
                _                            => (new[] { "src/controllers", "src/models", "src/views" }, "MVC."),
            };

        // Dependency Injection / Singleton / Factory / Decorator / Observer / Mediator / SOLID / Component-Based
        if (p.Contains("dependency injection") || p.Contains("inversion"))
            return (new[] { arch == ArchitectureType.Java ? $"{javaBase}/config" : "src/config" },
                    "Dependency Injection: la configuración del contenedor de IoC vive en `config`.");
        if (p.Contains("singleton"))
            return (new[] { arch == ArchitectureType.Java ? $"{javaBase}/config" : "src/config" },
                    "Singleton: beans/instancias únicas configuradas en `config`.");
        if (p.Contains("factory"))
            return (new[] { arch == ArchitectureType.Python ? "app/factories" : "src/factories" },
                    "Factory Method: fábricas para crear objetos sin acoplar al tipo concreto.");
        if (p.Contains("decorator"))
            return (new[] { arch == ArchitectureType.Python ? "app/decorators" : "src/decorators" },
                    "Decorator: añade comportamiento de forma dinámica mediante decoradores.");
        if (p.Contains("observer"))
            return (new[] { "src/events", "src/observers" },
                    "Observer: emisores y observadores desacoplados (RxJS en TypeScript).");
        if (p.Contains("mediator"))
            return (new[] { arch == ArchitectureType.Java ? $"{javaBase}/mediator" : "src/mediator" },
                    "Mediator: centraliza la comunicación entre componentes.");
        if (p.Contains("component-based"))
            return (new[] { "src/components", "src/modules" },
                    "Component-Based: organización por componentes/módulos reutilizables.");
        if (p.Contains("solid"))
            return (new[] { arch == ArchitectureType.Java ? $"{javaBase}/interfaces" : "src/interfaces" },
                    "SOLID: interfaces segregadas y dependencias por abstracción.");

        // Genérico
        return (Array.Empty<string>(),
                $"Patrón '{pattern}' marcado para este proyecto. Revisa la documentación para su implementación específica.");
    }

    // ── Paso 5: README ────────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private async Task GenerateReadmeAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "README", "🤖 Generando README.md con IA (cache-first)...", ct: ct);

        var patterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? new();
        var libs     = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();

        var req = new ReadmeGenerationRequest(
            project.Name, project.Description,
            cfg.Framework, cfg.Database, cfg.Infrastructure,
            patterns, libs, project.RepositoryUrl);

<<<<<<< HEAD
        var readme = await _ai.GenerateReadmeAsync(req);
=======
        string readme;
        try
        {
            readme = await _ai.GenerateReadmeAsync(req);
            if (string.IsNullOrWhiteSpace(readme))
                throw new InvalidOperationException("README vacío");
        }
        catch (Exception ex)
        {
            // Si TODOS los proveedores de IA fallan, no abortamos la generación:
            // escribimos un README base. El proyecto y su estructura igual se publican.
            await EmitLogAsync(project, "README",
                $"⚠️ IA no disponible ({ex.Message}). Usando README base.", isError: true, ct: ct);
            readme = BuildFallbackReadme(project, cfg, patterns, libs);
        }

>>>>>>> 0dc2a35 (complete java,python,typescript)
        project.GeneratedReadme = readme;
        await File.WriteAllTextAsync(Path.Combine(path, "README.md"), readme, ct);
        await EmitLogAsync(project, "README", "✅ README.md generado", ct: ct);
    }

<<<<<<< HEAD
    // ─── Paso 6: GitHub ───────────────────────────────────────────────────────
=======
    private static string BuildFallbackReadme(
        Project project, WizardConfig cfg, List<string> patterns, List<string> libs)
    {
        var run = cfg.Architecture switch
        {
            ArchitectureType.Python     => "pip install -r requirements.txt\nuvicorn main:app --reload   # o el comando de tu framework",
            ArchitectureType.Java       => "./mvnw spring-boot:run   # o: mvn spring-boot:run",
            ArchitectureType.TypeScript => "npm install\nnpm run dev",
            ArchitectureType.JavaScript => "npm install\nnpm start",
            _                           => "# Ver documentación del framework"
        };

        return
            $"# {project.Name}\n\n" +
            $"{project.Description}\n\n" +
            "## Stack\n\n" +
            $"- **Arquitectura:** {cfg.Architecture}\n" +
            $"- **Framework:** {cfg.Framework}\n" +
            $"- **Base de datos:** {cfg.Database}\n" +
            $"- **Infraestructura:** {cfg.Infrastructure}\n\n" +
            (patterns.Any() ? $"**Patrones:** {string.Join(", ", patterns)}\n\n" : "") +
            (libs.Any()     ? $"**Librerías:** {string.Join(", ", libs)}\n\n" : "") +
            "## Instalación\n\n```bash\n" + run + "\n```\n\n" +
            "## Variables de entorno\n\n" +
            "Copia `.env.example` a `.env` y completa los valores.\n\n" +
            "```bash\ncp .env.example .env\n```\n\n" +
            "## Estructura\n\n" +
            "El proyecto fue generado con ProjectForge siguiendo una estructura modular estándar.\n";
    }

    // ── Paso 6: GitHub ────────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private async Task<string> PushToGitHubAsync(Project project, string path, CancellationToken ct)
    {
        var token   = project.User.AccessToken;
        var repoUrl = await _github.CreateRepositoryAsync(token, project.Name, project.Description, false);
        await EmitLogAsync(project, "GitHub", $"✅ Repositorio creado: {repoUrl}", ct: ct);

        await EmitLogAsync(project, "GitHub", "$ git init && git add . && git commit", ct: ct);
        await _shell.RunAsync("git init", path, ct);
<<<<<<< HEAD
        await _shell.RunAsync("git add .", path, ct);
=======

        // Salvaguarda: garantizar que exista un .gitignore antes de añadir nada.
        // Si por cualquier motivo no se generó, escribimos uno mínimo aquí mismo.
        var gitignorePath = Path.Combine(path, ".gitignore");
        if (!File.Exists(gitignorePath))
        {
            await File.WriteAllTextAsync(gitignorePath,
                "node_modules/\nvenv/\n.venv/\ntarget/\nbin/\nobj/\ndist/\nbuild/\n__pycache__/\n.env\n", ct);
        }

        await _shell.RunAsync("git add .", path, ct);

        // Quitar del índice directorios pesados que nunca deben ir al repo,
        // por si quedaron rastreados (p. ej. node_modules instalado antes del .gitignore).
        await _shell.RunAsync(
            "git rm -r --cached --quiet node_modules venv .venv target bin obj dist build __pycache__ 2>/dev/null || true",
            path, ct);

>>>>>>> 0dc2a35 (complete java,python,typescript)
        await _shell.RunAsync($"git commit -m \"chore: initial scaffold by ProjectForge\"", path, ct);
        await _shell.RunAsync($"git remote add origin {repoUrl}", path, ct);
        await _shell.RunAsync("git branch -M main", path, ct);

        await EmitLogAsync(project, "GitHub", "$ git push -u origin main", ct: ct);
        await _github.PushToRepositoryAsync(path, repoUrl, token);
        await EmitLogAsync(project, "GitHub", "🚀 Push completado. ¡Proyecto en GitHub!", ct: ct);

        return repoUrl;
    }

<<<<<<< HEAD
    // ─── Utilidades ───────────────────────────────────────────────────────────
=======
    // ── Utilidades ────────────────────────────────────────────────────────────
>>>>>>> 0dc2a35 (complete java,python,typescript)

    private static string InterpolateTemplate(string content, Dictionary<string, string> vars)
    {
        foreach (var (k, v) in vars)
            content = content.Replace($"{{{{{k}}}}}", v);
        return content;
    }

    private static int GetDefaultDbPort(DatabaseType db) => db switch
    {
        DatabaseType.MySQL      => 3306,
        DatabaseType.PostgreSQL => 5432,
        DatabaseType.SqlServer  => 1433,
        DatabaseType.MongoDB    => 27017,
        DatabaseType.Redis      => 6379,
        _                       => 5432
    };

<<<<<<< HEAD
    /// <summary>
    /// Actualiza el estado del proyecto en DB y emite el cambio por SignalR.
    /// </summary>
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
    private async Task UpdateStatusAsync(Project project, ProjectStatus status, CancellationToken ct = default)
    {
        project.Status    = status;
        project.UpdatedAt = DateTime.UtcNow;
        await _projects.UpdateAsync(project);
    }

<<<<<<< HEAD
    /// <summary>
    /// Emite el log al SignalR hub (visible en el browser) Y lo persiste en DB.
    /// Todos los parámetros opcionales van con nombre para evitar ambigüedad con CancellationToken.
    /// </summary>
=======
>>>>>>> 0dc2a35 (complete java,python,typescript)
    private async Task EmitLogAsync(
        Project project,
        string step,
        string message,
        bool isError       = false,
        string? command    = null,
        int? exitCode      = null,
        CancellationToken ct = default)
    {
<<<<<<< HEAD
        // 1. Enviar al navegador en tiempo real
        await _hub.SendLogAsync(project.Id, step, message, isError);

        // 2. Persistir en DB
=======
        await _hub.SendLogAsync(project.Id, step, message, isError);

>>>>>>> 0dc2a35 (complete java,python,typescript)
        project.Logs.Add(new ProjectLog
        {
            Step            = step,
            Message         = message,
            IsError         = isError,
            CommandExecuted = command,
            ExitCode        = exitCode
        });
        await _projects.UpdateAsync(project);
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 0dc2a35 (complete java,python,typescript)
