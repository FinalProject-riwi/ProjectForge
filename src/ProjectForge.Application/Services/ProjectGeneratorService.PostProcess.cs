using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Paso 3: Aplicar plantillas ───────────────────────────────────────────

    private async Task ApplyTemplatesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Templates", "📄 Aplicando plantillas de infraestructura...", ct: ct);
        var isPhpMicroservices = cfg.Architecture == ArchitectureType.Php &&
                                 HasSelectedPattern(cfg, "microservices");
        var isLaravelMicroservices = isPhpMicroservices && cfg.Framework == FrameworkType.Laravel;
        var isSymfonyMicroservices = isPhpMicroservices && cfg.Framework == FrameworkType.Symfony;

        // docker-compose.yml service/container names only allow [a-zA-Z0-9._-]+ — a project
        // name with other punctuation (quotes, &, etc.) would otherwise produce a compose file
        // that fails to parse or an image tag Docker rejects outright.
        var appNameSlug = System.Text.RegularExpressions.Regex.Replace(project.Name.ToLowerInvariant(), @"[^a-z0-9-]+", "-").Trim('-');
        if (string.IsNullOrEmpty(appNameSlug)) appNameSlug = "app";

        var vars = new Dictionary<string, string>
        {
            ["APP_NAME"] = appNameSlug,
            ["DB_NAME"] = $"{GetValidDatabaseName(project.Name)}_db",
            ["DB_PORT"] = GetDefaultDbPort(cfg.Database).ToString(),
            ["APP_PORT"] = GetDefaultAppPort(cfg.Architecture, cfg.Framework).ToString()
        };

        if (cfg.Infrastructure == InfrastructureType.DockerCompose && !isPhpMicroservices)
        {
            var tpl = await _templates.GetTemplateAsync(cfg.Architecture, "compose", cfg.Database, InfrastructureType.DockerCompose, cfg.Framework);
            if (tpl != null)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "docker-compose.yml"), InterpolateTemplate(tpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ docker-compose.yml generado", ct: ct);
            }

            // DotNet/Java/Python already write their own Dockerfile earlier in the pipeline
            // (ScaffoldDotNetBaseFilesAsync / ScaffoldJavaBaseFilesAsync / ScaffoldPythonBaseFilesAsync),
            // using the *real* assembly/entrypoint name and the *actually selected* runtime
            // version. This used to overwrite it unconditionally with the generic catalog
            // template, which hardcodes a mismatched name (APP_NAME here is lower-hyphenated,
            // e.g. "my-app", while .NET's real output is PascalCase, e.g. "MyApp.dll") and a
            // fixed SDK version — so every DotNet + Docker Compose container failed to start
            // with "Could not execute because the specified command or file was not found."
            var dockerfilePath = Path.Combine(path, "Dockerfile");
            if (!File.Exists(dockerfilePath))
            {
                var dockerfile = await _templates.GetTemplateAsync(cfg.Architecture, "dockerfile", cfg.Database, framework: cfg.Framework);
                if (dockerfile != null)
                {
                    await File.WriteAllTextAsync(dockerfilePath, InterpolateTemplate(dockerfile.Content, vars), ct);
                    await EmitLogAsync(project, "Templates", "✅ Dockerfile generado", ct: ct);
                }
            }
        }

        if (isPhpMicroservices)
        {
            await EmitLogAsync(project, "Templates", "ℹ️  Se omite el compose monolítico para microservices", ct: ct);
        }
        else if (cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.MySQL)
        {
            await ConfigureLaravelMySqlDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.PostgreSQL)
        {
            await ConfigureLaravelPostgreSqlDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.SQLite)
        {
            await ConfigureLaravelSqliteDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.MongoDB)
        {
            await ConfigureLaravelMongoDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.SqlServer)
        {
            await ConfigureLaravelSqlServerDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.Redis)
        {
            await ConfigureLaravelRedisDbAsync(project, path, vars, ct);
        }

        if (!isPhpMicroservices &&
            cfg.Architecture == ArchitectureType.Php && cfg.Framework == FrameworkType.Symfony)
        {
            await ConfigureSymfonyDatabaseAsync(project, path, cfg.Database, vars, ct);
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
        var ciPath = Path.Combine(ciDir, "ci.yml");
        if (ciTemplate != null && !File.Exists(ciPath))
        {
            await File.WriteAllTextAsync(ciPath, InterpolateTemplate(ciTemplate.Content, vars), ct);
            await EmitLogAsync(project, "Templates", "✅ .github/workflows/ci.yml generado", ct: ct);
        }

        // .env.example (don't overwrite if the framework already generated one)
        var dotenvTpl = await _templates.GetTemplateAsync(cfg.Architecture, "dotenv");
        if (dotenvTpl != null)
        {
            var envExamplePath = Path.Combine(path, ".env.example");
            if (!File.Exists(envExamplePath))
            {
                await File.WriteAllTextAsync(envExamplePath, InterpolateTemplate(dotenvTpl.Content, vars), ct);
                await EmitLogAsync(project, "Templates", "✅ .env.example generado", ct: ct);
            }
        }

        // Makefile
        var makefileTpl = await _templates.GetTemplateAsync(cfg.Architecture, "makefile");
        if (makefileTpl != null)
        {
            var makefilePath = Path.Combine(path, "Makefile");
            if (!File.Exists(makefilePath))
            {
                await File.WriteAllTextAsync(makefilePath, makefileTpl.Content, ct);
                await EmitLogAsync(project, "Templates", "✅ Makefile generado", ct: ct);
            }
        }

        // Secondary DB → docker-compose.override.yml (merged automatically by Docker Compose)
        if (cfg.Infrastructure == InfrastructureType.DockerCompose)
        {
            var secondaryDb = GetSecondaryDatabase(cfg);
            if (secondaryDb.HasValue)
            {
                var (secKey, secConnTemplate) = GetSecondaryDbConnectionInfo(secondaryDb.Value);
                var secConn = InterpolateTemplate(secConnTemplate, vars);
                var overrideContent = BuildSecondaryDbOverride(secondaryDb.Value, secKey, secConn, vars);
                await File.WriteAllTextAsync(Path.Combine(path, "docker-compose.override.yml"), overrideContent, ct);
                await EmitLogAsync(project, "Templates", $"✅ docker-compose.override.yml generado ({secondaryDb.Value})", ct: ct);
            }
        }
    }

    internal static DatabaseType? GetSecondaryDatabase(WizardConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.AdditionalOptionsJson))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(cfg.AdditionalOptionsJson);
            if (doc.RootElement.TryGetProperty("secondaryDatabase", out var el)
                && el.ValueKind == JsonValueKind.String
                && Enum.TryParse<DatabaseType>(el.GetString(), out var db))
                return db;
        }
        catch { }
        return null;
    }

    internal static (string EnvKey, string ConnStr) GetSecondaryDbConnectionInfo(DatabaseType db) => db switch
    {
        DatabaseType.Redis      => ("REDIS_URL",              "redis://db2:6379"),
        DatabaseType.MongoDB    => ("MONGODB_SECONDARY_URI",  "mongodb://admin:secret@db2:27017/{{DB_NAME}}_secondary"),
        DatabaseType.PostgreSQL => ("SECONDARY_DATABASE_URL", "Host=db2;Port=5432;Database={{DB_NAME}}_secondary;Username=postgres;Password=secret"),
        DatabaseType.MySQL      => ("SECONDARY_DATABASE_URL", "Server=db2;Port=3306;Database={{DB_NAME}}_secondary;Uid=root;Pwd=secret;"),
        DatabaseType.SqlServer  => ("SECONDARY_DATABASE_URL", "Server=db2,1433;Database={{DB_NAME}}_secondary;User Id=sa;Password=Secret1234!;"),
        DatabaseType.SQLite     => ("SECONDARY_DATABASE_URL", "Data Source=/data/{{DB_NAME}}_secondary.db"),
        _                       => ("SECONDARY_DATABASE_URL", "")
    };

    internal static string BuildSecondaryDbOverride(
        DatabaseType db, string envKey, string interpolatedConn, Dictionary<string, string> vars)
    {
        var serviceBlock = BuildSecondaryServiceBlock(db, vars);
        return
            "version: '3.9'\n" +
            "# This file is merged automatically with docker-compose.yml by Docker Compose.\n" +
            "# It adds the secondary database service.\n" +
            "services:\n" +
            "  app:\n" +
            "    environment:\n" +
            $"      - {envKey}={interpolatedConn}\n" +
            "    depends_on:\n" +
            "      - db2\n" +
            serviceBlock;
    }

    internal static string BuildSecondaryServiceBlock(DatabaseType db, Dictionary<string, string> vars)
    {
        var dbName = vars.GetValueOrDefault("DB_NAME", "app_db");
        return db switch
        {
            DatabaseType.PostgreSQL =>
                $"  db2:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {dbName}_secondary\n      POSTGRES_PASSWORD: secret\n    volumes:\n      - pgdata2:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata2:",
            DatabaseType.MySQL =>
                $"  db2:\n    image: mysql:8.0\n    environment:\n      MYSQL_DATABASE: {dbName}_secondary\n      MYSQL_ROOT_PASSWORD: secret\n    volumes:\n      - mysqldata2:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata2:",
            DatabaseType.Redis =>
                "  db2:\n    image: redis:7-alpine\n    volumes:\n      - redisdata2:/data\n    healthcheck:\n      test: [\"CMD\",\"redis-cli\",\"ping\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  redisdata2:",
            DatabaseType.MongoDB =>
                $"  db2:\n    image: mongo:7\n    environment:\n      MONGO_INITDB_ROOT_USERNAME: admin\n      MONGO_INITDB_ROOT_PASSWORD: secret\n      MONGO_INITDB_DATABASE: {dbName}_secondary\n    volumes:\n      - mongodata2:/data/db\nvolumes:\n  mongodata2:",
            DatabaseType.SqlServer =>
                "  db2:\n    image: mcr.microsoft.com/mssql/server:2022-latest\n    environment:\n      ACCEPT_EULA: Y\n      SA_PASSWORD: Secret1234!\n    volumes:\n      - mssqldata2:/var/opt/mssql\nvolumes:\n  mssqldata2:",
            _ => "volumes:\n  sqlitedata2:"
        };
    }

    private async Task ConfigureLaravelMongoDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "mongodb",
            ["DB_HOST"] = "mongo",
            ["DB_PORT"] = "27017",
            ["DB_DATABASE"] = dbName,
            ["DB_USERNAME"] = "",
            ["DB_PASSWORD"] = "",
            ["MONGODB_URI"] = $"mongodb://mongo:27017/{dbName}"
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await UpdateLaravelDatabaseConfigAsync(path, "mongodb", ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para MongoDB", ct: ct);
    }

    private async Task ConfigureLaravelMySqlDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "mysql",
            ["DB_HOST"] = "db",
            ["DB_PORT"] = "3306",
            ["DB_DATABASE"] = dbName,
            ["DB_USERNAME"] = "root",
            ["DB_PASSWORD"] = "secret"
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await UpdateLaravelDatabaseConfigAsync(path, "mysql", ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para MySQL", ct: ct);
    }

    private async Task ConfigureLaravelPostgreSqlDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "pgsql",
            ["DB_HOST"] = "db",
            ["DB_PORT"] = "5432",
            ["DB_DATABASE"] = dbName,
            ["DB_USERNAME"] = "postgres",
            ["DB_PASSWORD"] = "secret"
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await UpdateLaravelDatabaseConfigAsync(path, "pgsql", ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para PostgreSQL", ct: ct);
    }

    private async Task ConfigureLaravelSqlServerDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "sqlsrv",
            ["DB_HOST"] = "sqlserver",
            ["DB_PORT"] = "1433",
            ["DB_DATABASE"] = dbName,
            ["DB_USERNAME"] = "sa",
            ["DB_PASSWORD"] = "YourStrong!Passw0rd",
            ["DB_ENCRYPT"] = "false",
            ["DB_TRUST_SERVER_CERTIFICATE"] = "true"
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await UpdateLaravelDatabaseConfigAsync(path, "sqlsrv", ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para SQL Server", ct: ct);
    }

    private async Task ConfigureLaravelSqliteDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "sqlite",
            ["DB_DATABASE"] = "database/database.sqlite"
        };

        var databaseDir = Path.Combine(path, "database");
        Directory.CreateDirectory(databaseDir);

        var sqlitePath = Path.Combine(databaseDir, "database.sqlite");
        if (!File.Exists(sqlitePath))
            await File.WriteAllBytesAsync(sqlitePath, Array.Empty<byte>(), ct);

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para SQLite", ct: ct);
    }

    private async Task ConfigureLaravelRedisDbAsync(Project project, string path, Dictionary<string, string> vars, CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];
        var envValues = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "redis",
            ["DB_HOST"] = "redis",
            ["DB_PORT"] = "6379",
            ["DB_DATABASE"] = dbName,
            ["DB_USERNAME"] = "",
            ["DB_PASSWORD"] = "",
            ["CACHE_STORE"] = "redis",
            ["CACHE_DRIVER"] = "redis",
            ["QUEUE_CONNECTION"] = "redis",
            ["SESSION_DRIVER"] = "redis",
            ["REDIS_CLIENT"] = "phpredis",
            ["REDIS_HOST"] = "redis",
            ["REDIS_PORT"] = "6379",
            ["REDIS_PASSWORD"] = "null",
            ["REDIS_DB"] = "0",
            ["REDIS_CACHE_DB"] = "1",
            ["REDIS_URL"] = "redis://redis:6379"
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);
        await EmitLogAsync(project, "Templates", "✅ .env ajustado para Redis", ct: ct);
    }

    private async Task ConfigureSymfonyDatabaseAsync(
        Project project,
        string path,
        DatabaseType database,
        Dictionary<string, string> vars,
        CancellationToken ct)
    {
        var dbName = vars["DB_NAME"];

        switch (database)
        {
            case DatabaseType.MySQL:
                await ConfigureSymfonyRelationalDbAsync(project, path, dbName, $"""mysql://root:secret@db:3306/{dbName}?serverVersion=8.0&charset=utf8mb4""", ct);
                break;
            case DatabaseType.PostgreSQL:
                await ConfigureSymfonyRelationalDbAsync(project, path, dbName, $"""postgresql://postgres:secret@db:5432/{dbName}?serverVersion=16&charset=utf8""", ct);
                break;
            case DatabaseType.SqlServer:
                await ConfigureSymfonyRelationalDbAsync(project, path, dbName, $"""sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/{dbName}?encrypt=false&trustServerCertificate=true""", ct);
                break;
            case DatabaseType.SQLite:
                await ConfigureSymfonyRelationalDbAsync(project, path, dbName, $"""sqlite:///%kernel.project_dir%/var/{dbName}.sqlite""", ct);
                break;
            case DatabaseType.MongoDB:
                await ConfigureSymfonyMongoDbAsync(project, path, dbName, ct);
                break;
            case DatabaseType.Redis:
                await ConfigureSymfonyRedisDbAsync(project, path, dbName, ct);
                break;
            default:
                await ConfigureSymfonyRelationalDbAsync(project, path, dbName, $"""sqlite:///%kernel.project_dir%/var/{dbName}.sqlite""", ct);
                break;
        }
    }

    private async Task ConfigureSymfonyRelationalDbAsync(Project project, string path, string dbName, string databaseUrl, CancellationToken ct)
    {
        var envValues = new Dictionary<string, string>
        {
            ["DATABASE_URL"] = databaseUrl
        };

        if (databaseUrl.Contains("sqlite:///", StringComparison.Ordinal))
        {
            var varDir = Path.Combine(path, "var");
            Directory.CreateDirectory(varDir);
            var sqlitePath = Path.Combine(varDir, $"{dbName}.sqlite");
            if (!File.Exists(sqlitePath))
                await File.WriteAllBytesAsync(sqlitePath, Array.Empty<byte>(), ct);
        }

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);

        var doctrineConfig = $"""
doctrine:
    dbal:
        url: '%env(resolve:DATABASE_URL)%'
    orm:
        auto_generate_proxy_classes: true
        enable_lazy_ghost_objects: true
        report_fields_where_declared: true
        validate_xml_mapping: true
        naming_strategy: doctrine.orm.naming_strategy.underscore_number_aware
        auto_mapping: true
""";

        var doctrinePath = Path.Combine(path, "config", "packages");
        Directory.CreateDirectory(doctrinePath);
        await File.WriteAllTextAsync(Path.Combine(doctrinePath, "doctrine.yaml"), doctrineConfig, ct);
        await EmitLogAsync(project, "Templates", "✅ Symfony configurado para base relacional", ct: ct);
    }

    private async Task ConfigureSymfonyMongoDbAsync(Project project, string path, string dbName, CancellationToken ct)
    {
        var envValues = new Dictionary<string, string>
        {
            ["DATABASE_URL"] = $"""mongodb://mongo:27017/{dbName}""",
            ["MONGODB_URL"] = $"""mongodb://mongo:27017/{dbName}""",
            ["MONGODB_DB"] = dbName
        };

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);

        var configDir = Path.Combine(path, "config", "packages");
        Directory.CreateDirectory(configDir);

        var mongodbConfig = """
doctrine_mongodb:
    connections:
        default:
            server: '%env(resolve:MONGODB_URL)%'
            options: {}
    default_database: '%env(resolve:MONGODB_DB)%'
    document_managers:
        default:
            auto_mapping: true
""";

        await File.WriteAllTextAsync(Path.Combine(configDir, "doctrine_mongodb.yaml"), mongodbConfig, ct);
        await EmitLogAsync(project, "Templates", "✅ Symfony configurado para MongoDB", ct: ct);
    }

    private async Task ConfigureSymfonyRedisDbAsync(Project project, string path, string dbName, CancellationToken ct)
    {
        var envValues = new Dictionary<string, string>
        {
            ["DATABASE_URL"] = $"""sqlite:///%kernel.project_dir%/var/{dbName}.sqlite""",
            ["REDIS_URL"] = "redis://redis:6379",
            ["CACHE_DSN"] = "redis://redis:6379",
            ["MESSENGER_TRANSPORT_DSN"] = "redis://redis:6379/messages"
        };

        var varDir = Path.Combine(path, "var");
        Directory.CreateDirectory(varDir);
        var sqlitePath = Path.Combine(varDir, $"{dbName}.sqlite");
        if (!File.Exists(sqlitePath))
            await File.WriteAllBytesAsync(sqlitePath, Array.Empty<byte>(), ct);

        await UpdateLaravelEnvironmentAsync(path, ".env", envValues, ct);
        await UpdateLaravelEnvironmentAsync(path, ".env.example", envValues, ct);

        var packagesDir = Path.Combine(path, "config", "packages");
        Directory.CreateDirectory(packagesDir);

        await File.WriteAllTextAsync(Path.Combine(packagesDir, "cache.yaml"), """
framework:
    cache:
        app: cache.adapter.redis
        default_redis_provider: '%env(resolve:REDIS_URL)%'
""", ct);

        await File.WriteAllTextAsync(Path.Combine(packagesDir, "messenger.yaml"), """
framework:
    messenger:
        transports:
            async: '%env(resolve:MESSENGER_TRANSPORT_DSN)%'
        routing:
            'App\Message\*': async
""", ct);

        await EmitLogAsync(project, "Templates", "✅ Symfony configurado para Redis", ct: ct);
    }

    internal static bool HasSelectedPattern(WizardConfig cfg, string patternName)
    {
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        return selectedPatterns.Any(p => NormalizePatternToken(p) == NormalizePatternToken(patternName));
    }

    internal static async Task UpdateLaravelEnvironmentAsync(
        string path,
        string fileName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken ct)
    {
        var envPath = Path.Combine(path, fileName);
        if (!File.Exists(envPath))
            return;

        var env = await File.ReadAllTextAsync(envPath, ct);
        foreach (var (key, value) in values)
            env = SetEnvLine(env, key, value);

        await File.WriteAllTextAsync(envPath, env, ct);
    }

    internal static async Task UpdateLaravelDatabaseConfigAsync(string path, string defaultConnection, CancellationToken ct)
    {
        var configPath = Path.Combine(path, "config", "database.php");
        if (!File.Exists(configPath))
            return;

        var content = await File.ReadAllTextAsync(configPath, ct);
        content = content.Replace("env('DB_CONNECTION', 'sqlite')", $"env('DB_CONNECTION', '{defaultConnection}')", StringComparison.Ordinal);

        if (!content.Contains($"'{defaultConnection}' => [", StringComparison.Ordinal))
        {
            const string anchor = "        'mysql' => [";
            var insertAt = content.IndexOf(anchor, StringComparison.Ordinal);
            if (insertAt > 0)
            {
                var connectionBlock = defaultConnection switch
                {
                    "mongodb" => """
        'mongodb' => [
            'driver' => 'mongodb',
            'dsn' => env('MONGODB_URI', env('DB_URL')),
            'database' => env('DB_DATABASE', 'forge'),
        ],

""",
                    "sqlsrv" => """
        'sqlsrv' => [
            'driver' => 'sqlsrv',
            'url' => env('DB_URL'),
            'host' => env('DB_HOST', 'localhost'),
            'port' => env('DB_PORT', '1433'),
            'database' => env('DB_DATABASE', 'forge'),
            'username' => env('DB_USERNAME', 'sa'),
            'password' => env('DB_PASSWORD', ''),
            'charset' => env('DB_CHARSET', 'utf8'),
            'prefix' => '',
            'prefix_indexes' => true,
            'encrypt' => env('DB_ENCRYPT', 'false'),
            'trust_server_certificate' => env('DB_TRUST_SERVER_CERTIFICATE', 'true'),
        ],

""",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(connectionBlock))
                    content = content.Insert(insertAt, connectionBlock);
            }
        }

        await File.WriteAllTextAsync(configPath, content, ct);
    }

    internal static string SetEnvLine(string content, string key, string value)
    {
        var prefix = key + "=";
        var lines = content
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => line.StartsWith(prefix, StringComparison.Ordinal) ? prefix + value : line)
            .ToList();

        if (!lines.Any(line => line.StartsWith(prefix, StringComparison.Ordinal)))
            lines.Add(prefix + value);

        return string.Join(Environment.NewLine, lines);
    }

    // ─── Paso 4: Instalar dependencias ────────────────────────────────────────

    private async Task InstallLibrariesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var libs = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();
        var implicitLibraries = GetImplicitLibraries(cfg);
        foreach (var library in implicitLibraries)
        {
            if (!libs.Contains(library, StringComparer.OrdinalIgnoreCase))
                libs.Add(library);
        }

        if (!libs.Any())
        {
            await EmitLogAsync(project, "Dependencies", "ℹ️  Sin librerías adicionales seleccionadas", ct: ct);
            return;
        }

        await EmitLogAsync(project, "Dependencies", $"📦 Instalando {libs.Count} librerías...", ct: ct);

        foreach (var cmd in GetInstallCommands(cfg.Architecture, cfg.Framework, libs))
        {
            await EmitLogAsync(project, "Dependencies", $"$ {cmd}", ct: ct);
            var result = await _shell.RunAsync(cmd, path, ct);
            if (!result.Success)
            {
                await EmitLogAsync(project, "Dependencies", $"❌ Instalación falló: {result.Stderr}", isError: true, ct: ct);
                throw new InvalidOperationException($"Instalación de dependencias falló: {result.Stderr}");
            }
            await EmitLogAsync(project, "Dependencies", "✅ Instalado", ct: ct);
        }
    }

    internal static IEnumerable<string> GetInstallCommands(ArchitectureType arch, FrameworkType fw, List<string> libs) =>
        arch switch
        {
            ArchitectureType.DotNet => libs.Select(l => $"dotnet add package {l}"),
            // "python -m pip install" avoids relying on a bare "pip"/"pip3" executable being on
            // PATH separately from the interpreter — it's the same invocation the pre-flight
            // check in GetRequiredToolChecks verifies, so a passed check can't be followed by a
            // "pip: command not found" failure here.
            ArchitectureType.Python => new[] { $"{PythonExecutable} -m pip install {string.Join(" ", libs)}" },
            ArchitectureType.JavaScript or ArchitectureType.TypeScript => new[] { $"npm install {string.Join(" ", libs)}" },
            ArchitectureType.Php => new[] { $"composer require {string.Join(" ", libs)}" },
            // Java: Maven packages are declared in pom.xml; we log instructions instead of running mvn
            ArchitectureType.Java => libs.Select(l => $"echo 'Add to pom.xml: {l}'"),
            _ => Enumerable.Empty<string>()
        };

    internal static IEnumerable<string> GetImplicitLibraries(WizardConfig cfg)
    {
        var libraries = new List<string>();
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];

        // ── DotNet implicit packages ──────────────────────────────────────────
        if (cfg.Architecture == ArchitectureType.DotNet)
        {
            var needsEfCore = cfg.Database is DatabaseType.PostgreSQL or DatabaseType.MySQL
                                             or DatabaseType.SqlServer  or DatabaseType.SQLite;
            if (needsEfCore)
            {
                libraries.Add("Microsoft.EntityFrameworkCore");
                libraries.Add("Microsoft.EntityFrameworkCore.Design");
            }

            // DB-specific driver
            switch (cfg.Database)
            {
                case DatabaseType.PostgreSQL:
                    libraries.Add("Npgsql.EntityFrameworkCore.PostgreSQL");
                    break;
                case DatabaseType.MySQL:
                    libraries.Add("Pomelo.EntityFrameworkCore.MySql");
                    break;
                case DatabaseType.SqlServer:
                    libraries.Add("Microsoft.EntityFrameworkCore.SqlServer");
                    break;
                case DatabaseType.SQLite:
                    libraries.Add("Microsoft.EntityFrameworkCore.Sqlite");
                    break;
                case DatabaseType.MongoDB:
                    libraries.Add("MongoDB.Driver");
                    break;
                case DatabaseType.Redis:
                    libraries.Add("StackExchange.Redis");
                    break;
            }

            // CQRS, Mediator and Saga pattern files all generate code against MediatR
            // (IRequest/IRequestHandler/IPipelineBehavior/IMediator) — only CQRS used to trigger
            // the implicit package install, so picking "Mediator" or "Saga" alone produced a
            // project referencing a NuGet package that was never added, failing to build.
            if (selectedPatterns.Any(p => NormalizePatternToken(p) is "cqrs" or "mediator" or "saga"))
                libraries.Add("MediatR");

            return libraries;
        }

        // ── JavaScript/TypeScript implicit packages ─────────────────────────────
        // Previously there was no branch here at all, so npm never installed a database
        // driver regardless of which of the 6 databases was picked — see
        // BuildJavaScriptDbConfig (Scaffolding.cs), which now generates client code that
        // requires/imports exactly these packages.
        if (cfg.Architecture is ArchitectureType.JavaScript or ArchitectureType.TypeScript &&
            cfg.Framework is FrameworkType.NodeJs or FrameworkType.ExpressJs or FrameworkType.NestJs or FrameworkType.NestTs)
        {
            switch (cfg.Database)
            {
                case DatabaseType.PostgreSQL: libraries.Add("pg"); break;
                case DatabaseType.MySQL:      libraries.Add("mysql2"); break;
                case DatabaseType.SqlServer:  libraries.Add("mssql"); break;
                case DatabaseType.MongoDB:    libraries.Add("mongodb"); break;
                case DatabaseType.Redis:      libraries.Add("ioredis"); break;
                case DatabaseType.SQLite:     libraries.Add("better-sqlite3"); break;
            }

            return libraries;
        }

        // ── PHP implicit packages ─────────────────────────────────────────────
        if (cfg.Architecture != ArchitectureType.Php)
            return libraries;

        if (cfg.Framework == FrameworkType.Laravel &&
            selectedPatterns.Any(p => p.Contains("EventSourcing", StringComparison.OrdinalIgnoreCase)))
        {
            libraries.Add("spatie/laravel-event-sourcing");
        }

        if (cfg.Framework == FrameworkType.Laravel && cfg.Database == DatabaseType.MongoDB)
        {
            libraries.Add("mongodb/laravel-mongodb");
        }

        if (cfg.Framework == FrameworkType.Symfony &&
            cfg.Database is DatabaseType.MySQL or DatabaseType.PostgreSQL or DatabaseType.SqlServer or DatabaseType.SQLite)
        {
            libraries.Add("symfony/orm-pack");
        }

        if (cfg.Framework == FrameworkType.Symfony && cfg.Database == DatabaseType.MongoDB)
        {
            libraries.Add("doctrine/mongodb-odm-bundle");
        }

        if (cfg.Framework == FrameworkType.Symfony && cfg.Database == DatabaseType.Redis)
        {
            libraries.Add("symfony/cache");
            libraries.Add("symfony/messenger");
            libraries.Add("symfony/redis-messenger");
        }

        if (cfg.Framework == FrameworkType.Symfony &&
            selectedPatterns.Any(p => NormalizePatternToken(p) == "microservices"))
        {
            libraries.Add("symfony/http-client");
            libraries.Add("symfony/messenger");
        }

        return libraries;
    }

    // ─── Paso 5: README con IA ────────────────────────────────────────────────

    private async Task GenerateReadmeAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "README", "🤖 Generando README.md con IA (cache-first)...", ct: ct);

        var patterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? new();
        var libs = JsonSerializer.Deserialize<List<string>>(cfg.LibrariesJson) ?? new();

        var req = new ReadmeGenerationRequest(
            project.Name, project.Description,
            cfg.Framework, cfg.Database, cfg.Infrastructure,
            patterns, libs, project.RepositoryUrl);

        var readme = await _ai.GenerateReadmeAsync(req);
        project.GeneratedReadme = readme;
        await File.WriteAllTextAsync(Path.Combine(path, "README.md"), readme, ct);
        await EmitLogAsync(project, "README", "✅ README.md generado", ct: ct);
    }

    // ─── Paso 6: GitHub ───────────────────────────────────────────────────────

    private async Task<string> PushToGitHubAsync(Project project, string path, CancellationToken ct)
    {
        // El access_token está encriptado en BD — desencriptar antes de usar
        var encryptedToken = project.User.AccessToken;
        string token;
        try
        {
            token = _encryptionService.Decrypt(encryptedToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "No se pudo desencriptar el token de GitHub. El usuario debe volver a iniciar sesión.", ex);
        }

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("El token de acceso de GitHub está vacío. El usuario debe volver a iniciar sesión.");

        var repoUrl = await _github.CreateRepositoryAsync(token, project.Name, project.Description, ShouldCreatePrivateRepo(project.WizardConfig));
        await EmitLogAsync(project, "GitHub", $"✅ Repositorio creado: {repoUrl}", ct: ct);

        await EmitLogAsync(project, "GitHub", "$ git init && git add . && git commit", ct: ct);
        await RunGitCommandAsync(project, "git init", path, ct);
        await RunGitCommandAsync(project, "git config user.email \"projectforge@noreply.github.com\"", path, ct);
        await RunGitCommandAsync(project, "git config user.name \"ProjectForge\"", path, ct);
        await RunGitCommandAsync(project, "git add .", path, ct);
        await RunGitCommandAsync(project, "git commit -m \"chore: initial scaffold by ProjectForge\"", path, ct);
        await RunGitCommandAsync(project, $"git remote add origin {repoUrl}", path, ct);
        await RunGitCommandAsync(project, "git branch -M main", path, ct);

        await EmitLogAsync(project, "GitHub", "$ git push -u origin main", ct: ct);
        await _github.PushToRepositoryAsync(path, repoUrl, token);
        await EmitLogAsync(project, "GitHub", "🚀 Push completado. ¡Proyecto en GitHub!", ct: ct);

        return repoUrl;
    }

    private async Task RunGitCommandAsync(Project project, string command, string path, CancellationToken ct)
    {
        var result = await _shell.RunAsync(command, path, ct);
        if (!result.Success)
        {
            await EmitLogAsync(project, "GitHub", $"❌ Comando git falló: {result.Stderr}", isError: true, ct: ct);
            throw new InvalidOperationException($"Git command failed: {result.Stderr}");
        }
    }

    internal static bool ShouldCreatePrivateRepo(WizardConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.AdditionalOptionsJson))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(cfg.AdditionalOptionsJson);
            return doc.RootElement.TryGetProperty("createPrivateRepo", out var flag) &&
                   flag.ValueKind == JsonValueKind.True;
        }
        catch
        {
            return false;
        }
    }

    // ─── Utilidades ───────────────────────────────────────────────────────────

    internal static string InterpolateTemplate(string content, Dictionary<string, string> vars)
    {
        foreach (var (k, v) in vars)
            content = content.Replace($"{{{{{k}}}}}", v);
        return content;
    }

    internal static int GetDefaultDbPort(DatabaseType db) => db switch
    {
        DatabaseType.MySQL => 3306,
        DatabaseType.PostgreSQL => 5432,
        DatabaseType.SqlServer => 1433,
        DatabaseType.MongoDB => 27017,
        DatabaseType.Redis => 6379,
        _ => 5432
    };

    internal static int GetDefaultAppPort(ArchitectureType architecture, FrameworkType framework) =>
        architecture switch
        {
            ArchitectureType.JavaScript or ArchitectureType.TypeScript => 3000,
            ArchitectureType.Php => 8080,
            _ => framework switch
            {
                FrameworkType.NextJs or FrameworkType.NestJs or FrameworkType.NodeJs or FrameworkType.ExpressJs => 3000,
                _ => 8080
            }
        };

    /// <summary>
    /// Actualiza el estado del proyecto en DB y emite el cambio por SignalR.
    /// </summary>
    private async Task UpdateStatusAsync(Project project, ProjectStatus status, CancellationToken ct = default)
    {
        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;
        await _projects.UpdateAsync(project);
    }

    /// <summary>
    /// Emite el log al SignalR hub (visible en el browser) Y lo persiste en DB.
    /// Todos los parámetros opcionales van con nombre para evitar ambigüedad con CancellationToken.
    /// </summary>
    private async Task EmitLogAsync(
        Project project,
        string step,
        string message,
        bool isError = false,
        string? command = null,
        int? exitCode = null,
        CancellationToken ct = default)
    {
        await _hub.SendLogAsync(project.Id, step, message, isError);

        project.Logs.Add(new ProjectLog
        {
            Step = step,
            Message = message,
            IsError = isError,
            CommandExecuted = command,
            ExitCode = exitCode
        });
        await _projects.UpdateAsync(project);
    }
}
