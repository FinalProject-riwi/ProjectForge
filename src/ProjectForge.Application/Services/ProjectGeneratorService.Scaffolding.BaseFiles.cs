using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Python file-based scaffolding ────────────────────────────────────────

    private async Task ScaffoldPythonFilesInternalAsync(
        Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Python) return;

        var safeName = project.Name.Replace(" ", "_").ToLowerInvariant();

        switch (cfg.Framework)
        {
            case FrameworkType.FastAPI:
                await ScaffoldFastApiAsync(path, safeName, ct);
                break;
            case FrameworkType.Django:
                await ScaffoldDjangoAsync(path, safeName, ct);
                break;
            case FrameworkType.Flask:
                await ScaffoldFlaskAsync(path, safeName, ct);
                break;
            default:
                Directory.CreateDirectory(Path.Combine(path, "src"));
                Directory.CreateDirectory(Path.Combine(path, "tests"));
                break;
        }

        await EmitLogAsync(project, "Scaffold", "✅ Estructura Python generada", ct: ct);
    }

    private static async Task ScaffoldFastApiAsync(string path, string name, CancellationToken ct)
    {
        var appDir = Path.Combine(path, "app");
        Directory.CreateDirectory(Path.Combine(appDir, "api", "v1"));
        Directory.CreateDirectory(Path.Combine(appDir, "models"));
        Directory.CreateDirectory(Path.Combine(appDir, "services"));
        Directory.CreateDirectory(Path.Combine(path, "tests"));

        await WriteAsync(Path.Combine(path, "requirements.txt"), ct,
            "fastapi>=0.115.0\n" +
            "uvicorn[standard]>=0.30.0\n" +
            "sqlalchemy>=2.0.0\n" +
            "alembic>=1.13.0\n" +
            "pydantic>=2.0.0\n" +
            "python-dotenv>=1.0.0\n");

        await WriteAsync(Path.Combine(appDir, "main.py"), ct,
            "from fastapi import FastAPI\n\n" +
            "app = FastAPI(title=\"" + name + " API\")\n\n" +
            "@app.get(\"/\")\n" +
            "def health():\n" +
            "    return {\"status\": \"ok\"}\n\n" +
            "@app.get(\"/api/v1/items\")\n" +
            "def list_items():\n" +
            "    return []\n");

        await WriteAsync(Path.Combine(appDir, "__init__.py"), ct, "");
        await WriteAsync(Path.Combine(path, "tests", "__init__.py"), ct, "");
        await WriteAsync(Path.Combine(path, ".env"), ct, "APP_NAME=" + name + "\nDEBUG=true\n");
        await WriteAsync(Path.Combine(path, ".gitignore"), ct,
            "__pycache__/\n*.pyc\nvenv/\n.env\n.env.*\n*.egg-info/\ndist/\n");
    }

    private static async Task ScaffoldDjangoAsync(string path, string name, CancellationToken ct)
    {
        var projectDir = Path.Combine(path, name);
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(path, "apps"));

        await WriteAsync(Path.Combine(path, "requirements.txt"), ct,
            "Django>=5.0\n" +
            "djangorestframework>=3.15.0\n" +
            "python-dotenv>=1.0.0\n");

        await WriteAsync(Path.Combine(path, "manage.py"), ct,
            "#!/usr/bin/env python\n" +
            "import os, sys\n\n" +
            "def main():\n" +
            "    os.environ.setdefault('DJANGO_SETTINGS_MODULE', '" + name + ".settings')\n" +
            "    from django.core.management import execute_from_command_line\n" +
            "    execute_from_command_line(sys.argv)\n\n" +
            "if __name__ == '__main__':\n" +
            "    main()\n");

        await WriteAsync(Path.Combine(projectDir, "__init__.py"), ct, "");

        // settings.py — no f-string in C#, use Replace pattern
        var settings =
            "from pathlib import Path\n" +
            "from dotenv import load_dotenv\n" +
            "import os\n\n" +
            "load_dotenv()\n" +
            "BASE_DIR = Path(__file__).resolve().parent.parent\n" +
            "SECRET_KEY = os.getenv('SECRET_KEY', 'change-me')\n" +
            "DEBUG = os.getenv('DEBUG', 'True') == 'True'\n" +
            "ALLOWED_HOSTS = ['*']\n\n" +
            "INSTALLED_APPS = [\n" +
            "    'django.contrib.admin',\n" +
            "    'django.contrib.auth',\n" +
            "    'django.contrib.contenttypes',\n" +
            "    'django.contrib.sessions',\n" +
            "    'django.contrib.messages',\n" +
            "    'django.contrib.staticfiles',\n" +
            "    'rest_framework',\n" +
            "]\n\n" +
            "MIDDLEWARE = [\n" +
            "    'django.middleware.security.SecurityMiddleware',\n" +
            "    'django.contrib.sessions.middleware.SessionMiddleware',\n" +
            "    'django.middleware.common.CommonMiddleware',\n" +
            "    'django.middleware.csrf.CsrfViewMiddleware',\n" +
            "    'django.contrib.auth.middleware.AuthenticationMiddleware',\n" +
            "    'django.contrib.messages.middleware.MessageMiddleware',\n" +
            "]\n\n" +
            "ROOT_URLCONF = '" + name + ".urls'\n" +
            "DEFAULT_AUTO_FIELD = 'django.db.models.BigAutoField'\n" +
            "DATABASES = {\n" +
            "    'default': {\n" +
            "        'ENGINE': 'django.db.backends.sqlite3',\n" +
            "        'NAME': BASE_DIR / 'db.sqlite3',\n" +
            "    }\n" +
            "}\n" +
            "STATIC_URL = '/static/'\n";
        await WriteAsync(Path.Combine(projectDir, "settings.py"), ct, settings);

        await WriteAsync(Path.Combine(projectDir, "urls.py"), ct,
            "from django.contrib import admin\n" +
            "from django.urls import path, include\n\n" +
            "urlpatterns = [\n" +
            "    path('admin/', admin.site.urls),\n" +
            "]\n");

        await WriteAsync(Path.Combine(path, ".env"), ct,
            "SECRET_KEY=change-me-in-production\nDEBUG=True\n");
        await WriteAsync(Path.Combine(path, ".gitignore"), ct,
            "__pycache__/\n*.pyc\nvenv/\n.env\ndb.sqlite3\n");
    }

    private static async Task ScaffoldFlaskAsync(string path, string name, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "app", "routes"));
        Directory.CreateDirectory(Path.Combine(path, "app", "models"));
        Directory.CreateDirectory(Path.Combine(path, "tests"));

        await WriteAsync(Path.Combine(path, "requirements.txt"), ct,
            "Flask>=3.1.0\nflask-sqlalchemy>=3.1.0\npython-dotenv>=1.0.0\n");

        await WriteAsync(Path.Combine(path, "app", "__init__.py"), ct,
            "from flask import Flask\n\n" +
            "def create_app():\n" +
            "    app = Flask(__name__)\n" +
            "    from app.routes.main import main\n" +
            "    app.register_blueprint(main)\n" +
            "    return app\n");

        await WriteAsync(Path.Combine(path, "app", "routes", "__init__.py"), ct, "");
        await WriteAsync(Path.Combine(path, "app", "routes", "main.py"), ct,
            "from flask import Blueprint, jsonify\n\n" +
            "main = Blueprint('main', __name__)\n\n" +
            "@main.route('/')\n" +
            "def index():\n" +
            "    return jsonify({'status': 'ok'})\n");

        await WriteAsync(Path.Combine(path, "run.py"), ct,
            "from app import create_app\n" +
            "app = create_app()\n" +
            "if __name__ == '__main__':\n" +
            "    app.run(debug=True, host='0.0.0.0', port=5000)\n");

        await WriteAsync(Path.Combine(path, ".gitignore"), ct,
            "__pycache__/\n*.pyc\nvenv/\n.env\n");
    }

    // ─── Java file-based scaffolding ──────────────────────────────────────────

    private async Task ScaffoldJavaFilesInternalAsync(
        Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Java) return;

        var className = string.Concat(
            project.Name.Split(' ', '-')
                .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
        var artifact = project.Name.ToLowerInvariant().Replace(" ", "-");
        var bootVersion = cfg.FrameworkVersion ?? "3.3.5";

        switch (cfg.Framework)
        {
            case FrameworkType.Quarkus:
                await ScaffoldQuarkusAsync(path, className, artifact, ct);
                break;
            case FrameworkType.Micronaut:
                await ScaffoldMicronautAsync(path, className, artifact, ct);
                break;
            default:
                await ScaffoldSpringBootAsync(path, className, artifact, bootVersion, ct);
                break;
        }

        await EmitLogAsync(project, "Scaffold", "✅ Estructura Java generada", ct: ct);
    }

    private static async Task ScaffoldSpringBootAsync(
        string path, string className, string artifact, string bootVersion, CancellationToken ct)
    {
        var pkg = "com.example." + artifact.Replace("-", "");
        var pkgPath = pkg.Replace(".", Path.DirectorySeparatorChar.ToString());
        var srcMain = Path.Combine(path, "src", "main", "java", pkgPath);
        var srcTest = Path.Combine(path, "src", "test", "java", pkgPath);
        var resources = Path.Combine(path, "src", "main", "resources");

        Directory.CreateDirectory(srcMain);
        Directory.CreateDirectory(srcTest);
        Directory.CreateDirectory(resources);

        var pom =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\n" +
            "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
            "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\n" +
            "    <modelVersion>4.0.0</modelVersion>\n" +
            "    <parent>\n" +
            "        <groupId>org.springframework.boot</groupId>\n" +
            "        <artifactId>spring-boot-starter-parent</artifactId>\n" +
            "        <version>" + bootVersion + "</version>\n" +
            "    </parent>\n" +
            "    <groupId>com.example</groupId>\n" +
            "    <artifactId>" + artifact + "</artifactId>\n" +
            "    <version>0.0.1-SNAPSHOT</version>\n" +
            "    <name>" + className + "</name>\n" +
            "    <properties><java.version>21</java.version></properties>\n" +
            "    <dependencies>\n" +
            "        <dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-web</artifactId></dependency>\n" +
            "        <dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-actuator</artifactId></dependency>\n" +
            "        <dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-test</artifactId><scope>test</scope></dependency>\n" +
            "    </dependencies>\n" +
            "    <build><plugins><plugin>\n" +
            "        <groupId>org.springframework.boot</groupId>\n" +
            "        <artifactId>spring-boot-maven-plugin</artifactId>\n" +
            "    </plugin></plugins></build>\n" +
            "</project>\n";
        await WriteAsync(Path.Combine(path, "pom.xml"), ct, pom);

        var app =
            "package " + pkg + ";\n\n" +
            "import org.springframework.boot.SpringApplication;\n" +
            "import org.springframework.boot.autoconfigure.SpringBootApplication;\n\n" +
            "@SpringBootApplication\n" +
            "public class " + className + "Application {\n" +
            "    public static void main(String[] args) {\n" +
            "        SpringApplication.run(" + className + "Application.class, args);\n" +
            "    }\n" +
            "}\n";
        await WriteAsync(Path.Combine(srcMain, className + "Application.java"), ct, app);

        await WriteAsync(Path.Combine(resources, "application.properties"), ct,
            "spring.application.name=" + artifact + "\nserver.port=8080\n");
        await WriteAsync(Path.Combine(path, ".gitignore"), ct,
            "target/\n*.class\n*.jar\n*.war\n.idea/\n*.iml\n");
    }

    private static async Task ScaffoldQuarkusAsync(
        string path, string className, string artifact, CancellationToken ct)
    {
        var pkg = "com.example." + artifact.Replace("-", "");
        var pkgPath = pkg.Replace(".", Path.DirectorySeparatorChar.ToString());
        Directory.CreateDirectory(Path.Combine(path, "src", "main", "java", pkgPath));
        Directory.CreateDirectory(Path.Combine(path, "src", "main", "resources"));

        var pom =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\n" +
            "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
            "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\n" +
            "    <modelVersion>4.0.0</modelVersion>\n" +
            "    <groupId>com.example</groupId>\n" +
            "    <artifactId>" + artifact + "</artifactId>\n" +
            "    <version>1.0.0-SNAPSHOT</version>\n" +
            "    <properties>\n" +
            "        <compiler-plugin.version>3.13.0</compiler-plugin.version>\n" +
            "        <maven.compiler.release>21</maven.compiler.release>\n" +
            "        <quarkus.platform.version>3.9.5</quarkus.platform.version>\n" +
            "    </properties>\n" +
            "    <dependencyManagement>\n" +
            "        <dependencies>\n" +
            "            <dependency>\n" +
            "                <groupId>io.quarkus.platform</groupId>\n" +
            "                <artifactId>quarkus-bom</artifactId>\n" +
            "                <version>${quarkus.platform.version}</version>\n" +
            "                <type>pom</type><scope>import</scope>\n" +
            "            </dependency>\n" +
            "        </dependencies>\n" +
            "    </dependencyManagement>\n" +
            "    <dependencies>\n" +
            "        <dependency><groupId>io.quarkus</groupId><artifactId>quarkus-resteasy-reactive</artifactId></dependency>\n" +
            "        <dependency><groupId>io.quarkus</groupId><artifactId>quarkus-resteasy-reactive-jackson</artifactId></dependency>\n" +
            "    </dependencies>\n" +
            "</project>\n";
        await WriteAsync(Path.Combine(path, "pom.xml"), ct, pom);

        var resource =
            "package " + pkg + ";\n\n" +
            "import jakarta.ws.rs.GET;\n" +
            "import jakarta.ws.rs.Path;\n" +
            "import jakarta.ws.rs.Produces;\n" +
            "import jakarta.ws.rs.core.MediaType;\n\n" +
            "@Path(\"/hello\")\n" +
            "public class GreetingResource {\n" +
            "    @GET\n" +
            "    @Produces(MediaType.TEXT_PLAIN)\n" +
            "    public String hello() {\n" +
            "        return \"Hello from " + className + "!\";\n" +
            "    }\n" +
            "}\n";
        await WriteAsync(Path.Combine(path, "src", "main", "java", pkgPath, "GreetingResource.java"), ct, resource);
    }

    private static async Task ScaffoldMicronautAsync(
        string path, string className, string artifact, CancellationToken ct)
    {
        var pkg = "com.example." + artifact.Replace("-", "");
        var pkgPath = pkg.Replace(".", Path.DirectorySeparatorChar.ToString());
        Directory.CreateDirectory(Path.Combine(path, "src", "main", "java", pkgPath));

        var pom =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\n" +
            "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
            "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\n" +
            "    <modelVersion>4.0.0</modelVersion>\n" +
            "    <groupId>com.example</groupId>\n" +
            "    <artifactId>" + artifact + "</artifactId>\n" +
            "    <version>0.1</version>\n" +
            "    <properties>\n" +
            "        <java.version>21</java.version>\n" +
            "        <micronaut.version>4.4.0</micronaut.version>\n" +
            "    </properties>\n" +
            "    <dependencyManagement>\n" +
            "        <dependencies>\n" +
            "            <dependency>\n" +
            "                <groupId>io.micronaut.platform</groupId>\n" +
            "                <artifactId>micronaut-parent</artifactId>\n" +
            "                <version>${micronaut.version}</version>\n" +
            "                <type>pom</type><scope>import</scope>\n" +
            "            </dependency>\n" +
            "        </dependencies>\n" +
            "    </dependencyManagement>\n" +
            "    <dependencies>\n" +
            "        <dependency><groupId>io.micronaut</groupId><artifactId>micronaut-http-server-netty</artifactId></dependency>\n" +
            "        <dependency><groupId>io.micronaut</groupId><artifactId>micronaut-jackson-databind</artifactId></dependency>\n" +
            "    </dependencies>\n" +
            "</project>\n";
        await WriteAsync(Path.Combine(path, "pom.xml"), ct, pom);

        var app =
            "package " + pkg + ";\n\n" +
            "import io.micronaut.runtime.Micronaut;\n\n" +
            "public class Application {\n" +
            "    public static void main(String[] args) {\n" +
            "        Micronaut.run(Application.class, args);\n" +
            "    }\n" +
            "}\n";
        await WriteAsync(Path.Combine(path, "src", "main", "java", pkgPath, "Application.java"), ct, app);
    }

    // ─── Utility ──────────────────────────────────────────────────────────────

    private static async Task WriteAsync(string filePath, CancellationToken ct, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, content, ct);
    }
}
