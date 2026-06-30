using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders;

/// <summary>
/// Siembra TODAS las combinaciones de plantillas, librerías y patrones por lenguaje × framework × DB × infraestructura.
/// IDs de plantillas: 5000-5499 | Librerías: 11000-11099 | Patrones: 12000-12049
/// </summary>
public static partial class AllCombinationsSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await UpsertTemplatesAsync(db, GetAllTemplates(), ct);
        await UpsertLibrariesAsync(db, GetAllLibraries(), ct);
        await UpsertPatternsAsync(db, GetAllPatterns(), ct);
    }

    // ─── Orchestration ───────────────────────────────────────────────────────────

    private static IEnumerable<ProjectTemplate> GetAllTemplates()
    {
        foreach (var t in GetDotNetTemplates())           yield return t;
        foreach (var t in GetJavaTemplates())              yield return t;
        foreach (var t in GetPythonTemplates())            yield return t;
        foreach (var t in GetPhpAdditionalTemplates())     yield return t;
        foreach (var t in GetJsAdditionalTemplates())      yield return t;
        foreach (var t in GetTsTemplates())                yield return t;
        foreach (var t in GetVersionDockerfiles())         yield return t;
        foreach (var t in GetEnvAndMakefileTemplates())   yield return t;
    }

    private static IEnumerable<LibraryRecommendation> GetAllLibraries()
    {
        foreach (var l in GetDotNetLibraries())   yield return l;
        foreach (var l in GetJavaLibraries())      yield return l;
        foreach (var l in GetPythonLibraries())    yield return l;
        foreach (var l in GetPhpLibraries())       yield return l;
        foreach (var l in GetJsLibraries())        yield return l;
        foreach (var l in GetTsLibraries())        yield return l;
    }

    private static IEnumerable<DesignPatternEntry> GetAllPatterns()
    {
        foreach (var p in GetPhpPatterns())        yield return p;
        foreach (var p in GetJavaPatternsByFw())   yield return p;
        foreach (var p in GetPythonPatternsByFw()) yield return p;
    }

    // ─── Upsert helpers ──────────────────────────────────────────────────────────

    private static async Task UpsertTemplatesAsync(AppDbContext db, IEnumerable<ProjectTemplate> templates, CancellationToken ct)
    {
        foreach (var t in templates)
        {
            if (await db.Templates.AnyAsync(x =>
                x.Id == t.Id ||
                (x.Architecture == t.Architecture && x.TemplateType == t.TemplateType && x.Name == t.Name), ct))
                continue;

            var fw    = t.Framework.HasValue      ? $"N'{t.Framework}'"      : "NULL";
            var dbv   = t.Database.HasValue       ? $"N'{t.Database}'"       : "NULL";
            var infra = t.Infrastructure.HasValue ? $"N'{t.Infrastructure}'" : "NULL";
            var cnt   = t.Content.Replace("'", "''").Replace("{", "{{").Replace("}", "}}");
            var desc  = (t.Description ?? "").Replace("'", "''");
            var name  = t.Name.Replace("'", "''");

            await db.Database.ExecuteSqlRawAsync($"""
                SET IDENTITY_INSERT [Templates] ON;
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES ({t.Id},N'{t.Architecture}',N'{cnt}','{t.CreatedAt:yyyy-MM-dd HH:mm:ss}',{dbv},N'{desc}',{fw},{infra},1,N'{name}',N'{t.TemplateType}',{t.Version});
                SET IDENTITY_INSERT [Templates] OFF;
                """, Array.Empty<object>());
        }
    }

    private static async Task UpsertLibrariesAsync(AppDbContext db, IEnumerable<LibraryRecommendation> libraries, CancellationToken ct)
    {
        foreach (var l in libraries)
        {
            if (await db.Libraries.AnyAsync(x =>
                x.Id == l.Id ||
                (x.Architecture == l.Architecture && x.PackageName == l.PackageName), ct))
                continue;

            var fw   = l.Framework.HasValue ? $"N'{l.Framework}'" : "NULL";
            var cat  = (l.Category ?? "").Replace("'", "''");
            var desc = (l.Description ?? "").Replace("'", "''");
            var ic   = (l.InstallCommand ?? "").Replace("'", "''");
            var name = l.Name.Replace("'", "''");
            var pkg  = l.PackageName.Replace("'", "''");

            await db.Database.ExecuteSqlRawAsync($"""
                SET IDENTITY_INSERT [Libraries] ON;
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES ({l.Id},N'{l.Architecture}',N'{cat}','{l.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{desc}',{fw},N'{ic}',N'{name}',N'{pkg}',{l.PopularityScore});
                SET IDENTITY_INSERT [Libraries] OFF;
                """, Array.Empty<object>());
        }
    }

    private static async Task UpsertPatternsAsync(AppDbContext db, IEnumerable<DesignPatternEntry> patterns, CancellationToken ct)
    {
        foreach (var p in patterns)
        {
            if (await db.DesignPatterns.AnyAsync(x =>
                x.Id == p.Id ||
                (x.Architecture == p.Architecture && x.Pattern == p.Pattern && x.Name == p.Name), ct))
                continue;

            var notes = (p.ImplementationNotes ?? "").Replace("'", "''");
            var cmds  = (p.ScaffoldCommandsJson ?? "[]").Replace("'", "''");
            var desc  = (p.Description ?? "").Replace("'", "''");
            var name  = p.Name.Replace("'", "''");

            await db.Database.ExecuteSqlRawAsync($"""
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES ({p.Id},N'{p.Architecture}','{p.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{desc}',N'{notes}',N'{name}',N'{p.Pattern}',N'{cmds}');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
                """, Array.Empty<object>());
        }
    }

    // ─── Shared content helpers ───────────────────────────────────────────────────

    private static string DbImage(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => "postgres:16-alpine",
        DatabaseType.MySQL      => "mysql:8.0",
        DatabaseType.SqlServer  => "mcr.microsoft.com/mssql/server:2022-latest",
        DatabaseType.MongoDB    => "mongo:7",
        DatabaseType.Redis      => "redis:7-alpine",
        DatabaseType.SQLite     => "",
        _ => "postgres:16-alpine"
    };

    private static string DbVolume(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => "pgdata",
        DatabaseType.MySQL      => "mysqldata",
        DatabaseType.SqlServer  => "mssqldata",
        DatabaseType.MongoDB    => "mongodata",
        DatabaseType.Redis      => "redisdata",
        DatabaseType.SQLite     => "sqlitedata",
        _ => "dbdata"
    };

    private static string DbPort(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => "5432",
        DatabaseType.MySQL      => "3306",
        DatabaseType.SqlServer  => "1433",
        DatabaseType.MongoDB    => "27017",
        DatabaseType.Redis      => "6379",
        DatabaseType.SQLite     => "0",
        _ => "5432"
    };

    private static string DbServiceBlock(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL =>
            "  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:",
        DatabaseType.MySQL =>
            "  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:",
        DatabaseType.SqlServer =>
            "  db:\n    image: mcr.microsoft.com/mssql/server:2022-latest\n    environment:\n      ACCEPT_EULA: Y\n      SA_PASSWORD: Secret1234!\n    ports:\n      - \"{{DB_PORT}}:1433\"\n    volumes:\n      - mssqldata:/var/opt/mssql\nvolumes:\n  mssqldata:",
        DatabaseType.MongoDB =>
            "  db:\n    image: mongo:7\n    environment:\n      MONGO_INITDB_ROOT_USERNAME: admin\n      MONGO_INITDB_ROOT_PASSWORD: secret\n      MONGO_INITDB_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:27017\"\n    volumes:\n      - mongodata:/data/db\nvolumes:\n  mongodata:",
        DatabaseType.Redis =>
            "  db:\n    image: redis:7-alpine\n    ports:\n      - \"{{DB_PORT}}:6379\"\n    volumes:\n      - redisdata:/data\n    healthcheck:\n      test: [\"CMD\",\"redis-cli\",\"ping\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  redisdata:",
        DatabaseType.SQLite => "",
        _ => ""
    };

    // Generates Kubernetes manifest with connection string env var + liveness/readiness probes
    private static string K8sManifest(string connEnvKey, string connEnvValue, int containerPort) =>
        "apiVersion: apps/v1\n" +
        "kind: Deployment\n" +
        "metadata:\n" +
        "  name: {{APP_NAME}}\n" +
        "spec:\n" +
        "  replicas: 2\n" +
        "  selector:\n" +
        "    matchLabels:\n" +
        "      app: {{APP_NAME}}\n" +
        "  template:\n" +
        "    metadata:\n" +
        "      labels:\n" +
        "        app: {{APP_NAME}}\n" +
        "    spec:\n" +
        "      containers:\n" +
        "        - name: {{APP_NAME}}\n" +
        "          image: {{APP_NAME}}:latest\n" +
        "          ports:\n" +
        $"            - containerPort: {containerPort}\n" +
        "          env:\n" +
        $"            - name: {connEnvKey}\n" +
        $"              value: \"{connEnvValue}\"\n" +
        "          livenessProbe:\n" +
        "            httpGet:\n" +
        "              path: /health\n" +
        $"              port: {containerPort}\n" +
        "            initialDelaySeconds: 30\n" +
        "            periodSeconds: 10\n" +
        "            failureThreshold: 3\n" +
        "          readinessProbe:\n" +
        "            httpGet:\n" +
        "              path: /health\n" +
        $"              port: {containerPort}\n" +
        "            initialDelaySeconds: 5\n" +
        "            periodSeconds: 5\n" +
        "            successThreshold: 1\n" +
        "---\n" +
        "apiVersion: v1\n" +
        "kind: Service\n" +
        "metadata:\n" +
        "  name: {{APP_NAME}}-svc\n" +
        "spec:\n" +
        "  selector:\n" +
        "    app: {{APP_NAME}}\n" +
        "  ports:\n" +
        $"    - port: 80\n" +
        $"      targetPort: {containerPort}\n" +
        "  type: LoadBalancer";
}