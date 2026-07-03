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

        // Must be a valid Python identifier — Django's manage.py imports it as a module
        // (DJANGO_SETTINGS_MODULE = '{name}.settings'), which fails for names containing
        // hyphens or starting with a digit.
        var safeName = ToPythonIdentifier(project.Name);

        switch (cfg.Framework)
        {
            case FrameworkType.FastAPI:
                await ScaffoldFastApiAsync(path, safeName, ct);
                break;
            case FrameworkType.Django:
                await ScaffoldDjangoAsync(path, safeName, cfg.Database, ct);
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
        Directory.CreateDirectory(Path.Combine(appDir, "domain"));           // for DDD/CleanArch patterns
        Directory.CreateDirectory(Path.Combine(appDir, "models"));
        Directory.CreateDirectory(Path.Combine(appDir, "services"));
        Directory.CreateDirectory(Path.Combine(appDir, "repositories"));     // for Repository pattern
        Directory.CreateDirectory(Path.Combine(appDir, "commands"));         // for CQRS pattern
        Directory.CreateDirectory(Path.Combine(appDir, "queries"));          // for CQRS pattern
        Directory.CreateDirectory(Path.Combine(appDir, "handlers"));         // for CQRS/Mediator patterns
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

    private static async Task ScaffoldDjangoAsync(string path, string name, DatabaseType db, CancellationToken ct)
    {
        var projectDir = Path.Combine(path, name);
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(path, "apps"));
        Directory.CreateDirectory(Path.Combine(path, "app", "domain"));
        Directory.CreateDirectory(Path.Combine(path, "app", "repositories"));
        Directory.CreateDirectory(Path.Combine(path, "app", "commands"));
        Directory.CreateDirectory(Path.Combine(path, "app", "queries"));
        Directory.CreateDirectory(Path.Combine(path, "app", "handlers"));

        await WriteAsync(Path.Combine(path, "requirements.txt"), ct,
            BuildDjangoRequirementsTxt(db));

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
            BuildDjangoDatabasesBlock(db) +
            BuildDjangoExtrasBlock(db) +
            "STATIC_URL = '/static/'\n";
        await WriteAsync(Path.Combine(projectDir, "settings.py"), ct, settings);

        await WriteAsync(Path.Combine(projectDir, "urls.py"), ct,
            "from django.contrib import admin\n" +
            "from django.urls import path, include\n\n" +
            "urlpatterns = [\n" +
            "    path('admin/', admin.site.urls),\n" +
            "]\n");

        await WriteAsync(Path.Combine(path, ".env"), ct, BuildDjangoEnvFile(db));
        await WriteAsync(Path.Combine(path, ".gitignore"), ct,
            "__pycache__/\n*.pyc\nvenv/\n.env\ndb.sqlite3\n");
    }

    private static async Task ScaffoldFlaskAsync(string path, string name, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "app", "routes"));
        Directory.CreateDirectory(Path.Combine(path, "app", "models"));
        Directory.CreateDirectory(Path.Combine(path, "app", "domain"));
        Directory.CreateDirectory(Path.Combine(path, "app", "repositories"));
        Directory.CreateDirectory(Path.Combine(path, "app", "commands"));
        Directory.CreateDirectory(Path.Combine(path, "app", "queries"));
        Directory.CreateDirectory(Path.Combine(path, "app", "handlers"));
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
        // Project names can contain punctuation ("Foo & Bar", "café!") that Split(' ', '-')
        // doesn't remove — strip anything that isn't a valid Java identifier character before
        // writing it into "public class {className}Application", and a class name can't start
        // with a digit either (e.g. project "3-tier-app" → "3TierApp" is invalid Java).
        className = new string(className.Where(char.IsLetterOrDigit).ToArray());
        if (className.Length == 0 || char.IsDigit(className[0]))
            className = "App" + className;
        // Maven artifactId convention: lowercase letters, digits and hyphens only.
        var artifact = System.Text.RegularExpressions.Regex.Replace(project.Name.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(artifact)) artifact = "app";
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
        var pkg = "com.example." + ToJavaPackageSegment(artifact);
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
        var pkg = "com.example." + ToJavaPackageSegment(artifact);
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
        var pkg = "com.example." + ToJavaPackageSegment(artifact);
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

    // A Java package segment can't start with a digit (e.g. artifact "3-tier-app" → "3tierapp"
    // would produce the illegal package "com.example.3tierapp").
    private static string ToJavaPackageSegment(string artifact)
    {
        var segment = artifact.Replace("-", "");
        if (segment.Length == 0 || char.IsDigit(segment[0]))
            segment = "app" + segment;
        return segment;
    }

    // ─── Django: DB-aware settings ────────────────────────────────────────────

    private static string BuildDjangoRequirementsTxt(DatabaseType db)
    {
        var packages = new List<string> { "Django>=5.0", "djangorestframework>=3.15.0", "python-dotenv>=1.0.0" };
        packages.AddRange(GetDjangoDbPackages(db));
        return string.Join("\n", packages) + "\n";
    }

    private static IReadOnlyList<string> GetDjangoDbPackages(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => new[] { "psycopg2-binary>=2.9.9" },
        DatabaseType.MySQL      => new[] { "mysqlclient>=2.2.0" },
        DatabaseType.SqlServer  => new[] { "mssql-django>=1.5", "pyodbc>=5.0.0" },
        // Django's ORM has no first-party MongoDB/Redis backend — 'default' stays on SQLite
        // and these are used directly (pymongo client / django-redis cache), see
        // BuildDjangoDatabasesBlock/BuildDjangoExtrasBlock below.
        DatabaseType.MongoDB    => new[] { "pymongo>=4.7.0" },
        DatabaseType.Redis      => new[] { "django-redis>=5.4.0", "redis>=5.0.0" },
        _                       => Array.Empty<string>()
    };

    // Fixes a real generation bug: settings.py used to hardcode sqlite3 regardless of the
    // database picked in the wizard, so selecting Postgres/MySQL/SqlServer for a Django
    // project silently produced a SQLite app instead.
    private static string BuildDjangoDatabasesBlock(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL =>
            "DATABASES = {\n" +
            "    'default': {\n" +
            "        'ENGINE': 'django.db.backends.postgresql',\n" +
            "        'NAME': os.getenv('DB_NAME', 'app_db'),\n" +
            "        'USER': os.getenv('DB_USER', 'postgres'),\n" +
            "        'PASSWORD': os.getenv('DB_PASSWORD', 'secret'),\n" +
            "        'HOST': os.getenv('DB_HOST', 'db'),\n" +
            "        'PORT': os.getenv('DB_PORT', '5432'),\n" +
            "    }\n" +
            "}\n",
        DatabaseType.MySQL =>
            "DATABASES = {\n" +
            "    'default': {\n" +
            "        'ENGINE': 'django.db.backends.mysql',\n" +
            "        'NAME': os.getenv('DB_NAME', 'app_db'),\n" +
            "        'USER': os.getenv('DB_USER', 'root'),\n" +
            "        'PASSWORD': os.getenv('DB_PASSWORD', 'secret'),\n" +
            "        'HOST': os.getenv('DB_HOST', 'db'),\n" +
            "        'PORT': os.getenv('DB_PORT', '3306'),\n" +
            "    }\n" +
            "}\n",
        DatabaseType.SqlServer =>
            "DATABASES = {\n" +
            "    'default': {\n" +
            "        'ENGINE': 'mssql',  # pip package: mssql-django\n" +
            "        'NAME': os.getenv('DB_NAME', 'app_db'),\n" +
            "        'USER': os.getenv('DB_USER', 'sa'),\n" +
            "        'PASSWORD': os.getenv('DB_PASSWORD', 'YourStrong!Passw0rd'),\n" +
            "        'HOST': os.getenv('DB_HOST', 'sqlserver'),\n" +
            "        'PORT': os.getenv('DB_PORT', '1433'),\n" +
            "        'OPTIONS': {'driver': 'ODBC Driver 18 for SQL Server', 'extra_params': 'TrustServerCertificate=yes'},\n" +
            "    }\n" +
            "}\n",
        // MongoDB/Redis aren't relational stores Django's ORM can target — 'default' keeps
        // using SQLite for Django's own auth/sessions/admin tables (see BuildDjangoExtrasBlock
        // for the actual MongoDB/Redis wiring).
        _ =>
            "DATABASES = {\n" +
            "    'default': {\n" +
            "        'ENGINE': 'django.db.backends.sqlite3',\n" +
            "        'NAME': BASE_DIR / 'db.sqlite3',\n" +
            "    }\n" +
            "}\n"
    };

    private static string BuildDjangoExtrasBlock(DatabaseType db) => db switch
    {
        DatabaseType.MongoDB =>
            "\n# MongoDB is used as a document store via pymongo; Django's ORM (DATABASES above)\n" +
            "# keeps using SQLite for its own auth/sessions/admin tables.\n" +
            "MONGODB_URL = os.getenv('MONGODB_URL', 'mongodb://mongo:27017/app_db')\n\n",
        DatabaseType.Redis =>
            "\nCACHES = {\n" +
            "    'default': {\n" +
            "        'BACKEND': 'django_redis.cache.RedisCache',\n" +
            "        'LOCATION': os.getenv('REDIS_URL', 'redis://redis:6379/0'),\n" +
            "        'OPTIONS': {'CLIENT_CLASS': 'django_redis.client.DefaultClient'},\n" +
            "    }\n" +
            "}\n\n",
        _ => "\n"
    };

    private static string BuildDjangoEnvFile(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => "SECRET_KEY=change-me-in-production\nDEBUG=True\nDB_HOST=db\nDB_PORT=5432\nDB_NAME=app_db\nDB_USER=postgres\nDB_PASSWORD=secret\n",
        DatabaseType.MySQL      => "SECRET_KEY=change-me-in-production\nDEBUG=True\nDB_HOST=db\nDB_PORT=3306\nDB_NAME=app_db\nDB_USER=root\nDB_PASSWORD=secret\n",
        DatabaseType.SqlServer  => "SECRET_KEY=change-me-in-production\nDEBUG=True\nDB_HOST=sqlserver\nDB_PORT=1433\nDB_NAME=app_db\nDB_USER=sa\nDB_PASSWORD=YourStrong!Passw0rd\n",
        DatabaseType.MongoDB    => "SECRET_KEY=change-me-in-production\nDEBUG=True\nMONGODB_URL=mongodb://mongo:27017/app_db\n",
        DatabaseType.Redis      => "SECRET_KEY=change-me-in-production\nDEBUG=True\nREDIS_URL=redis://redis:6379/0\n",
        _                       => "SECRET_KEY=change-me-in-production\nDEBUG=True\n"
    };

    // Turns a project name into a valid Python module/package identifier: letters, digits and
    // underscores only, never starting with a digit. Project names are allowed to contain
    // hyphens (e.g. "my-app"), but "import my-app" / "my-app.settings" is a Python syntax error —
    // Django's manage.py builds exactly that import path from this name.
    private static string ToPythonIdentifier(string name, string fallback = "app")
    {
        var chars = name.Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '_').ToArray();
        var collapsed = System.Text.RegularExpressions.Regex.Replace(new string(chars), "_+", "_").Trim('_');
        if (collapsed.Length == 0) return fallback;
        return char.IsDigit(collapsed[0]) ? "_" + collapsed : collapsed;
    }
}
