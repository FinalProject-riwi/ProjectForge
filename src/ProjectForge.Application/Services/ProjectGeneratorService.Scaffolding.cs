using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Paso 2: Scaffold por arquitectura ────────────────────────────────────

    private async Task ScaffoldProjectAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Scaffold", $"🏗️  Iniciando scaffold ({cfg.Architecture} / {cfg.Framework})...", ct: ct);

        foreach (var cmd in GetScaffoldCommands(cfg, project.Name, path))
        {
            await EmitLogAsync(project, "Scaffold", $"$ {cmd.Command}", ct: ct);
            var result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);

            if (!string.IsNullOrWhiteSpace(result.Stdout))
                await EmitLogAsync(project, "Scaffold", result.Stdout.Trim(), ct: ct);

            if (!result.Success)
            {
                await EmitLogAsync(project, "Scaffold", result.Stderr, isError: true, ct: ct);
                throw new InvalidOperationException($"Scaffold falló: {result.Stderr}");
            }
        }
    }

    internal static IEnumerable<(string Command, string? WorkingDir)> GetScaffoldCommands(
        WizardConfig cfg, string projectName, string path)
    {
        var safeName = GetValidDotNetProjectName(projectName);

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
                FrameworkType.AspNetCoreMVC  => new[] { ($"dotnet new mvc -n {safeName} -o {path}", (string?)null) },
                // .NET 8+ SDKs dropped the standalone "blazorserver" template short name — it was
                // folded into the unified "blazor" (Blazor Web App) template, which picks its
                // render mode via --interactivity instead of a separate template.
                FrameworkType.BlazorServer   => new[] { ($"dotnet new blazor --interactivity Server -n {safeName} -o {path}", (string?)null) },
                FrameworkType.BlazorWasm     => new[] { ($"dotnet new blazorwasm -n {safeName} -o {path}", (string?)null) },
                FrameworkType.MinimalApi     => new[]
                {
                    ($"dotnet new web -n {safeName} -o {path} --no-https false", (string?)null),
                    ($"dotnet new sln -n {safeName}", path),
                    ($"dotnet sln add {safeName}.csproj", path),
                },
                _ => new[] { ($"dotnet new webapi -n {safeName} -o {path}", (string?)null) }
            },

            // Python: file-based scaffolding handled by ScaffoldPythonFilesInternalAsync
            ArchitectureType.Python => Array.Empty<(string, string?)>(),

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => cfg.Framework switch
            {
                FrameworkType.NodeJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm pkg set 'scripts.start=node src/index.js'", (string?)path),
                },
                FrameworkType.ExpressJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm install express", (string?)path),
                    ($"npm pkg set 'scripts.start=node src/server.js'", (string?)path),
                },
                // No --skip-install: InstallLibrariesAsync only runs npm at all when the wizard's
                // library-selection step isn't empty, so a project with zero extra libraries
                // (a perfectly normal choice) used to end up with package.json but no
                // node_modules at all — "npm run build"/"npm start" would fail immediately.
                FrameworkType.NestJs => new[] { ($"nest new {safeName} --language JS --package-manager npm --skip-git --directory .", (string?)path) },
                FrameworkType.NextJs => new[] { ($"npx create-next-app@latest . --js --app --eslint --src-dir --import-alias=@/* --no-git", (string?)path) },
                FrameworkType.NestTs => new[] { ($"nest new {safeName} --language TS --package-manager npm --skip-git --directory .", (string?)path) },
                FrameworkType.NextTs => new[] { ($"npx create-next-app@latest . --ts --app --eslint --src-dir --import-alias=@/* --no-git", (string?)path) },
                _ => new[] { ($"npm init -y", (string?)path) }
            },

            // Java: file-based scaffolding handled by ScaffoldJavaFilesInternalAsync
            ArchitectureType.Java => Array.Empty<(string, string?)>(),

            ArchitectureType.Php => GetPhpScaffoldCommands(cfg),

            _ => Array.Empty<(string, string?)>()
        };
    }

    // Runs "composer create-project" for the base Laravel/Symfony skeleton — same role as
    // "dotnet new"/"nest new" above. This was previously missing entirely: nothing in the
    // pipeline ever created composer.json/artisan/public/index.php (or bin/console for
    // Symfony), so ScaffoldPhpBaseFilesAsync's DDD files, the design-pattern files, and
    // ApplyTemplatesAsync's .env/database.php edits were all being layered onto an empty
    // folder — every non-microservices PHP project generated a broken, non-runnable repo.
    // Microservices workspaces create their own per-service projects instead (see
    // ScaffoldLaravelMicroservicesWorkspaceAsync/ScaffoldSymfonyMicroservicesWorkspaceAsync),
    // so the base project is skipped there to avoid creating an unused top-level skeleton.
    private static IEnumerable<(string Command, string? WorkingDir)> GetPhpScaffoldCommands(WizardConfig cfg)
    {
        if (HasSelectedPattern(cfg, "microservices"))
            return Array.Empty<(string, string?)>();

        return cfg.Framework switch
        {
            FrameworkType.Laravel => new[] { ("composer create-project laravel/laravel . --no-interaction --prefer-dist", (string?)null) },
            FrameworkType.Symfony => new[] { ("composer create-project symfony/skeleton . --no-interaction", (string?)null) },
            _ => Array.Empty<(string, string?)>()
        };
    }

    // ─── Paso 2b: Base files de JS/TS ─────────────────────────────────────────

    internal static string GetValidDotNetProjectName(string projectName)
    {
        var safeName = System.Text.RegularExpressions.Regex.Replace(projectName.Trim(), @"[^\w]", "");
        if (string.IsNullOrEmpty(safeName))
            return "ProjectForgeApp";
        if (char.IsDigit(safeName[0]))
            safeName = "Project" + safeName;
        return safeName;
    }

    private async Task ScaffoldJavaScriptBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture is not (ArchitectureType.JavaScript or ArchitectureType.TypeScript))
            return;

        var src = Path.Combine(path, "src");
        Directory.CreateDirectory(src);

        var isTs = cfg.Architecture == ArchitectureType.TypeScript;
        var dbName = GetValidDatabaseName(project.Name);

        // Every other language this generator supports (DotNet/Java/Python/PHP) gets a DB
        // driver dependency, a connection-config file and a .env with the connection string for
        // whatever database was picked in the wizard. JavaScript/TypeScript used to get none of
        // that for ANY framework/database combination — no npm package, no client, no env var —
        // regardless of which of the 6 databases was selected.
        if (cfg.Framework is FrameworkType.NodeJs or FrameworkType.ExpressJs or FrameworkType.NestJs or FrameworkType.NestTs)
        {
            var dbFileName = isTs ? "db.ts" : "db.js";
            await File.WriteAllTextAsync(Path.Combine(src, dbFileName), BuildJavaScriptDbConfig(cfg.Database, dbName, isTs), ct);
        }

        var envContent = BuildJavaScriptDbEnvFile(cfg.Database, dbName);
        await File.WriteAllTextAsync(Path.Combine(path, ".env"), envContent, ct);
        await File.WriteAllTextAsync(Path.Combine(path, ".env.example"), envContent, ct);

        switch (cfg.Framework)
        {
            case FrameworkType.NodeJs:
                await File.WriteAllTextAsync(Path.Combine(src, "index.js"), """
console.log('ProjectForge');

// See src/db.js for the database client configured for this project.
require('./db');
""", ct);
                break;
            case FrameworkType.ExpressJs:
                await File.WriteAllTextAsync(Path.Combine(src, "server.js"), BuildExpressServerJs(cfg.Database), ct);
                break;
            case FrameworkType.NestJs:
            case FrameworkType.NestTs:
                await File.WriteAllTextAsync(Path.Combine(src, "main.ts"), """
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

// See src/db.ts / src/db.js for the database client configured for this project — wire it
// into a Nest provider (e.g. in AppModule) wherever you need it injected.

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  await app.listen(process.env.PORT || 3000);
}
bootstrap();
""", ct);
                break;
            case FrameworkType.NextJs:
            case FrameworkType.NextTs:
                Directory.CreateDirectory(Path.Combine(src, "app"));
                await File.WriteAllTextAsync(Path.Combine(src, "app", "page.js"), "export default function Page(){ return null; }\n", ct);
                break;
        }
    }

    internal static string BuildExpressServerJs(DatabaseType db)
    {
        var (healthCheck, extraRequire) = db switch
        {
            DatabaseType.PostgreSQL or DatabaseType.MySQL =>
                ("await pool.query('SELECT 1');", "const { pool } = require('./db');"),
            DatabaseType.SqlServer =>
                ("await (await getPool()).request().query('SELECT 1');", "const { getPool } = require('./db');"),
            DatabaseType.MongoDB =>
                ("await connectDb();", "const { connectDb } = require('./db');"),
            DatabaseType.Redis =>
                ("await redis.ping();", "const { redis } = require('./db');"),
            _ =>
                ("db.prepare('SELECT 1').get();", "const { db } = require('./db');")
        };

        return $$"""
const express = require('express');
{{extraRequire}}
const app = express();

app.get('/', (_, res) => res.json({ ok: true }));

app.get('/health', async (_, res) => {
  try {
    {{healthCheck}}
    res.json({ status: 'ok' });
  } catch (err) {
    res.status(500).json({ status: 'error', message: err.message });
  }
});

app.listen(process.env.PORT || 3000);
""";
    }

    internal static string BuildJavaScriptDbConfig(DatabaseType db, string dbName, bool isTypeScript) =>
        isTypeScript ? BuildTypeScriptDbConfig(db, dbName) : BuildPlainJavaScriptDbConfig(db, dbName);

    internal static string BuildPlainJavaScriptDbConfig(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL => """
const { Pool } = require('pg');

const pool = new Pool({ connectionString: process.env.DATABASE_URL });

module.exports = { pool };
""",
        DatabaseType.MySQL => """
const mysql = require('mysql2/promise');

const pool = mysql.createPool(process.env.DATABASE_URL);

module.exports = { pool };
""",
        DatabaseType.SqlServer => """
const sql = require('mssql');

let poolPromise;
function getPool() {
  if (!poolPromise) poolPromise = sql.connect(process.env.DATABASE_URL);
  return poolPromise;
}

module.exports = { getPool, sql };
""",
        DatabaseType.MongoDB => $$"""
const { MongoClient } = require('mongodb');

const client = new MongoClient(process.env.MONGODB_URL || 'mongodb://mongo:27017/{{dbName}}');
let db = null;

async function connectDb() {
  if (!db) {
    await client.connect();
    db = client.db(process.env.DB_NAME || '{{dbName}}');
  }
  return db;
}

module.exports = { connectDb };
""",
        DatabaseType.Redis => """
const Redis = require('ioredis');

const redis = new Redis(process.env.REDIS_URL || 'redis://redis:6379');

module.exports = { redis };
""",
        _ => """
const Database = require('better-sqlite3');

const db = new Database(process.env.SQLITE_PATH || './app.db');

module.exports = { db };
"""
    };

    internal static string BuildTypeScriptDbConfig(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL => """
import { Pool } from 'pg';

export const pool = new Pool({ connectionString: process.env.DATABASE_URL });
""",
        DatabaseType.MySQL => """
import mysql from 'mysql2/promise';

export const pool = mysql.createPool(process.env.DATABASE_URL as string);
""",
        DatabaseType.SqlServer => """
import sql from 'mssql';

let poolPromise: Promise<sql.ConnectionPool> | undefined;

export function getPool(): Promise<sql.ConnectionPool> {
  if (!poolPromise) poolPromise = sql.connect(process.env.DATABASE_URL as string);
  return poolPromise;
}
""",
        DatabaseType.MongoDB => $$"""
import { MongoClient, Db } from 'mongodb';

const client = new MongoClient(process.env.MONGODB_URL || 'mongodb://mongo:27017/{{dbName}}');
let db: Db | null = null;

export async function connectDb(): Promise<Db> {
  if (!db) {
    await client.connect();
    db = client.db(process.env.DB_NAME || '{{dbName}}');
  }
  return db;
}
""",
        DatabaseType.Redis => """
import Redis from 'ioredis';

export const redis = new Redis(process.env.REDIS_URL || 'redis://redis:6379');
""",
        _ => """
import Database from 'better-sqlite3';

export const db = new Database(process.env.SQLITE_PATH || './app.db');
"""
    };

    internal static string BuildJavaScriptDbEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL => $"DATABASE_URL=postgresql://postgres:secret@db:5432/{dbName}\nPORT=3000\n",
        DatabaseType.MySQL      => $"DATABASE_URL=mysql://root:secret@db:3306/{dbName}\nPORT=3000\n",
        DatabaseType.SqlServer  => $"DATABASE_URL=Server=sqlserver,1433;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;Encrypt=false;TrustServerCertificate=true\nPORT=3000\n",
        DatabaseType.MongoDB    => $"MONGODB_URL=mongodb://mongo:27017/{dbName}\nDB_NAME={dbName}\nPORT=3000\n",
        DatabaseType.Redis      => "REDIS_URL=redis://redis:6379\nPORT=3000\n",
        DatabaseType.SQLite     => $"SQLITE_PATH=./{dbName}.db\nPORT=3000\n",
        _                       => "PORT=3000\n"
    };

    private async Task ScaffoldPhpBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Php || cfg.Framework != FrameworkType.Symfony)
            return;

        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Any(p => NormalizePatternToken(p) == "microservices"))
            return;

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando base Symfony...", ct: ct);

        foreach (var (relativePath, content) in BuildPhpDddPatternFiles(FrameworkType.Symfony))
        {
            var filePath = Path.Combine(path, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, content, ct);
        }

        await EmitLogAsync(project, "Scaffold", "✅ Base Symfony generada", ct: ct);
    }

    // ─── Paso 3: Patrones de diseño ──────────────────────────────────────────

    private async Task ScaffoldDesignPatternsAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Count == 0)
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  Sin patrones seleccionados", ct: ct);
            return;
        }

        foreach (var pattern in selectedPatterns)
        {
            var files = cfg.Architecture switch
            {
                ArchitectureType.Php => BuildPhpPatternFiles(cfg, pattern),
                ArchitectureType.JavaScript or ArchitectureType.TypeScript => BuildJavaScriptPatternFiles(cfg.Framework, pattern),
                ArchitectureType.DotNet => BuildDotNetPatternFiles(cfg.Framework, cfg.Database, pattern),
                ArchitectureType.Java => BuildJavaPatternFiles(cfg.Framework, cfg.Database, pattern),
                ArchitectureType.Python => BuildPythonPatternFiles(cfg.Framework, cfg.Database, pattern),
                _ => Array.Empty<(string RelativePath, string Content)>()
            };

            if (files.Count == 0)
            {
                await EmitLogAsync(project, "Patterns", $"ℹ️  Patrón no soportado: {pattern}", ct: ct);
                continue;
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                NormalizePatternToken(pattern) == "microservices")
            {
                if (cfg.Framework == FrameworkType.Laravel)
                {
                    await ScaffoldLaravelMicroservicesWorkspaceAsync(path, ct);
                }
                else if (cfg.Framework == FrameworkType.Symfony)
                {
                    await ScaffoldSymfonyMicroservicesWorkspaceAsync(path, ct);
                }
            }

            foreach (var (relativePath, content) in files)
            {
                var filePath = Path.Combine(path, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllTextAsync(filePath, content, ct);
            }

            await EmitLogAsync(project, "Patterns", $"✅ Patrón generado: {pattern}", ct: ct);

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "hexagonalarchitecture")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\HexagonalServiceProvider::class", ct);
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "repository")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\RepositoryServiceProvider::class", ct);
            }

            if (cfg.Architecture == ArchitectureType.DotNet &&
                NormalizePatternToken(pattern) == "microservices")
            {
                await ExcludeDotNetSubprojectFromMainCsprojAsync(path, "gateway", ct);
            }
        }
    }

    // The "Microservices" pattern writes gateway/gateway.csproj + gateway/Program.cs as a
    // separate project — but SDK-style .csproj files glob **/*.cs from their own directory by
    // default, with no automatic exclusion for subfolders that happen to contain another .csproj.
    // Without this, the main project's own compilation picks up gateway/Program.cs too, and two
    // files with top-level statements in the same compilation is CS8802 ("only one compilation
    // unit can have top-level statements") — every DotNet + Microservices project failed to build.
    internal static async Task ExcludeDotNetSubprojectFromMainCsprojAsync(string path, string subfolder, CancellationToken ct)
    {
        var mainCsproj = Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (mainCsproj == null) return;

        var content = await File.ReadAllTextAsync(mainCsproj, ct);
        var marker = "</Project>";
        if (!content.Contains(marker)) return;

        var exclusion = $"""
              <ItemGroup>
                <Compile Remove="{subfolder}/**" />
                <Content Remove="{subfolder}/**" />
                <None Remove="{subfolder}/**" />
              </ItemGroup>

            """;
        await File.WriteAllTextAsync(mainCsproj, content.Replace(marker, exclusion + marker), ct);
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpPatternFiles(
        WizardConfig cfg,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "domaindrivendesign" => BuildPhpDddPatternFiles(cfg.Framework),
            "cleanarchitecture" => BuildPhpCleanArchitecturePatternFiles(cfg.Framework),
            "hexagonalarchitecture" => BuildPhpHexagonalPatternFiles(cfg.Framework),
            "repository" => BuildPhpRepositoryPatternFiles(cfg.Framework),
            "cqrs" => BuildPhpCqrsPatternFiles(cfg.Framework),
            "eventsourcing" => BuildPhpEventSourcingPatternFiles(cfg.Framework),
            "mediator" => BuildPhpMediatorPatternFiles(cfg.Framework),
            "saga" => BuildPhpSagaPatternFiles(cfg.Framework),
            "microservices" => BuildPhpMicroservicesPatternFiles(cfg.Framework, cfg.Database),
            _ => Array.Empty<(string, string)>()
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptPatternFiles(
        FrameworkType framework,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "cqrs" => BuildJavaScriptCqrsPatternFiles(framework),
            "eventsourcing" => BuildJavaScriptEventSourcingPatternFiles(framework),
            "mediator" => BuildJavaScriptMediatorPatternFiles(framework),
            "repository" => BuildJavaScriptRepositoryPatternFiles(framework),
            "cleanarchitecture" => BuildJavaScriptCleanArchitecturePatternFiles(framework),
            "hexagonalarchitecture" => BuildJavaScriptHexagonalPatternFiles(framework),
            "domaindrivendesign" => BuildJavaScriptDddPatternFiles(framework),
            "microservices" => BuildJavaScriptMicroservicesPatternFiles(framework),
            "saga" => BuildJavaScriptSagaPatternFiles(framework),
            _ => Array.Empty<(string, string)>()
        };
    }

    internal static async Task EnsureLaravelProviderRegistrationAsync(string path, string providerEntry, CancellationToken ct)
    {
        var providersFile = Path.Combine(path, "bootstrap", "providers.php");
        if (!File.Exists(providersFile))
            return;

        var content = await File.ReadAllTextAsync(providersFile, ct);
        if (content.Contains(providerEntry, StringComparison.Ordinal))
            return;

        var insertMarker = "return [";
        var idx = content.IndexOf(insertMarker, StringComparison.Ordinal);
        if (idx < 0)
            return;

        var insertAt = content.IndexOf('\n', idx);
        if (insertAt < 0)
            return;

        var updated = content.Insert(insertAt + 1, $"    {providerEntry},\n");
        await File.WriteAllTextAsync(providersFile, updated, ct);
    }

    internal async Task ScaffoldLaravelMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios: {result.Stderr}");
        }
    }

    internal async Task ScaffoldSymfonyMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios de Symfony: {result.Stderr}");
        }
    }

    // Used to spawn its own raw Process with FileName="composer", bypassing IShellExecutor
    // entirely — which meant composer would still need to be installed locally even after every
    // other command got routed to per-language worker containers. Routing through _shell.RunAsync
    // sends it to the php-worker like every other PHP command.
    internal async Task<ShellResult> RunComposerCommandAsync(IEnumerable<string> arguments, string workingDirectory, CancellationToken ct)
    {
        var command = "composer " + string.Join(" ", arguments.Select(QuoteShellArgument));
        return await _shell.RunAsync(command, workingDirectory, ct);
    }

    private static string QuoteShellArgument(string argument) =>
        argument.Contains(' ') ? $"\"{argument.Replace("\"", "\\\"")}\"" : argument;

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptCqrsPatternFiles(FrameworkType framework)
    {
        var isNest = framework is FrameworkType.NestJs or FrameworkType.NestTs;
        if (isNest)
        {
            return new[]
            {
                ("src/application/commands/create-item.command.ts", """
export class CreateItemCommand {
  constructor(
    public readonly name: string,
    public readonly description: string,
  ) {}
}
"""),
                ("src/application/handlers/create-item.handler.ts", """
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { CreateItemCommand } from '../commands/create-item.command';

@CommandHandler(CreateItemCommand)
export class CreateItemHandler implements ICommandHandler<CreateItemCommand> {
  async execute(command: CreateItemCommand): Promise<void> {
    // TODO: inject repository and save item
    console.log('Creating item:', command.name);
  }
}
"""),
                ("src/application/queries/get-all-items.query.ts", """
export class GetAllItemsQuery {}
"""),
            };
        }

        return new[]
        {
            ("src/application/commands/createItem.js", """
class CreateItemCommand {
  constructor(name, description) {
    this.name = name;
    this.description = description;
  }
}
module.exports = { CreateItemCommand };
"""),
            ("src/application/handlers/createItemHandler.js", """
const { CreateItemCommand } = require('../commands/createItem');

class CreateItemHandler {
  constructor(repository) {
    this.repository = repository;
  }

  async handle(command) {
    if (!(command instanceof CreateItemCommand))
      throw new Error('Invalid command');
    return this.repository.save({ name: command.name, description: command.description });
  }
}
module.exports = { CreateItemHandler };
"""),
            ("src/application/queries/getAllItems.js", """
class GetAllItemsQuery {}

class GetAllItemsHandler {
  constructor(repository) {
    this.repository = repository;
  }

  async handle() {
    return this.repository.findAll();
  }
}
module.exports = { GetAllItemsQuery, GetAllItemsHandler };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptEventSourcingPatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("src/domain/events/domainEvent.js", """
class DomainEvent {
  constructor(aggregateId, type, payload) {
    this.id = crypto.randomUUID();
    this.aggregateId = aggregateId;
    this.type = type;
    this.payload = payload;
    this.occurredAt = new Date().toISOString();
  }
}
module.exports = { DomainEvent };
"""),
            ("src/infrastructure/eventStore.js", """
class InMemoryEventStore {
  constructor() {
    this.events = [];
  }

  async append(event) {
    this.events.push(event);
  }

  async getByAggregateId(aggregateId) {
    return this.events.filter(e => e.aggregateId === aggregateId);
  }
}
module.exports = { InMemoryEventStore };
"""),
            ("src/domain/aggregates/baseAggregate.js", """
const { DomainEvent } = require('../events/domainEvent');

class BaseAggregate {
  constructor(id) {
    this.id = id;
    this._pendingEvents = [];
  }

  apply(type, payload) {
    const event = new DomainEvent(this.id, type, payload);
    this._pendingEvents.push(event);
    this._handleEvent(event);
    return event;
  }

  _handleEvent(event) {} // override in subclasses

  pullEvents() {
    const events = [...this._pendingEvents];
    this._pendingEvents = [];
    return events;
  }
}
module.exports = { BaseAggregate };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptMediatorPatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("src/application/mediator.js", """
class Mediator {
  constructor() {
    this._handlers = new Map();
  }

  register(requestType, handler) {
    this._handlers.set(requestType, handler);
    return this;
  }

  async send(request) {
    const handler = this._handlers.get(request.constructor);
    if (!handler) throw new Error(`No handler for ${request.constructor.name}`);
    return handler.handle(request);
  }
}
module.exports = { Mediator };
"""),
            ("src/application/messages/index.js", """
class CreateItemRequest {
  constructor(name, description) {
    this.name = name;
    this.description = description;
  }
}

class GetAllItemsRequest {}

module.exports = { CreateItemRequest, GetAllItemsRequest };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptRepositoryPatternFiles(FrameworkType framework)
    {
        var isNest = framework is FrameworkType.NestJs or FrameworkType.NestTs;
        if (isNest)
        {
            return new[]
            {
                ("src/domain/repositories/item.repository.interface.ts", """
export interface IItemRepository {
  findById(id: number): Promise<any | null>;
  findAll(): Promise<any[]>;
  save(item: any): Promise<any>;
  delete(id: number): Promise<void>;
}
"""),
                ("src/infrastructure/repositories/item.repository.ts", """
import { Injectable } from '@nestjs/common';
import { IItemRepository } from '../../domain/repositories/item.repository.interface';

@Injectable()
export class ItemRepository implements IItemRepository {
  private items: any[] = [];

  async findById(id: number) { return this.items.find(i => i.id === id) ?? null; }
  async findAll() { return this.items; }
  async save(item: any) { this.items.push(item); return item; }
  async delete(id: number) { this.items = this.items.filter(i => i.id !== id); }
}
"""),
            };
        }

        return new[]
        {
            ("src/domain/repositories/itemRepository.js", """
class ItemRepository {
  constructor() { this.items = []; this._nextId = 1; }

  async findById(id) { return this.items.find(i => i.id === id) ?? null; }
  async findAll() { return [...this.items]; }
  async save(item) {
    if (!item.id) item.id = this._nextId++;
    const idx = this.items.findIndex(i => i.id === item.id);
    if (idx >= 0) this.items[idx] = item; else this.items.push(item);
    return item;
  }
  async delete(id) { this.items = this.items.filter(i => i.id !== id); }
}
module.exports = { ItemRepository };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptCleanArchitecturePatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("src/domain/entities/item.js", """
class Item {
  constructor({ id = null, name, description, createdAt = new Date() } = {}) {
    if (!name) throw new Error('name is required');
    this.id = id;
    this.name = name;
    this.description = description;
    this.createdAt = createdAt;
  }

  static create(name, description) {
    return new Item({ name, description });
  }
}
module.exports = { Item };
"""),
            ("src/application/use-cases/createItem.js", """
const { Item } = require('../../domain/entities/item');

class CreateItemUseCase {
  constructor(itemRepository) {
    this.itemRepository = itemRepository;
  }

  async execute({ name, description }) {
    const item = Item.create(name, description);
    return this.itemRepository.save(item);
  }
}
module.exports = { CreateItemUseCase };
"""),
            ("src/infrastructure/index.js", """
// Infrastructure bootstrap — wire repositories and use-cases here
module.exports = {};
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptHexagonalPatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("src/core/ports/itemPort.js", """
/**
 * Port (interface) for item persistence.
 * Implement this in src/infrastructure/adapters/.
 */
class ItemPort {
  async findById(id) { throw new Error('Not implemented'); }
  async findAll() { throw new Error('Not implemented'); }
  async save(item) { throw new Error('Not implemented'); }
  async delete(id) { throw new Error('Not implemented'); }
}
module.exports = { ItemPort };
"""),
            ("src/infrastructure/adapters/inMemoryItemAdapter.js", """
const { ItemPort } = require('../../core/ports/itemPort');

class InMemoryItemAdapter extends ItemPort {
  constructor() { super(); this.items = []; this._nextId = 1; }

  async findById(id) { return this.items.find(i => i.id === id) ?? null; }
  async findAll() { return [...this.items]; }
  async save(item) {
    if (!item.id) item.id = this._nextId++;
    const idx = this.items.findIndex(i => i.id === item.id);
    if (idx >= 0) this.items[idx] = item; else this.items.push(item);
    return item;
  }
  async delete(id) { this.items = this.items.filter(i => i.id !== id); }
}
module.exports = { InMemoryItemAdapter };
"""),
            ("src/core/domain/item.js", """
class Item {
  constructor(name, description) {
    this.id = null;
    this.name = name;
    this.description = description;
    this.createdAt = new Date();
  }
}
module.exports = { Item };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptDddPatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("src/domain/aggregates/baseAggregate.js", """
class BaseAggregate {
  constructor(id) {
    this.id = id;
    this._domainEvents = [];
  }

  addDomainEvent(event) { this._domainEvents.push(event); }
  pullDomainEvents() { const ev = [...this._domainEvents]; this._domainEvents = []; return ev; }
}
module.exports = { BaseAggregate };
"""),
            ("src/domain/events/domainEvent.js", """
class DomainEvent {
  constructor(name, payload) {
    this.id = Math.random().toString(36).slice(2);
    this.name = name;
    this.payload = payload;
    this.occurredAt = new Date();
  }
}
module.exports = { DomainEvent };
"""),
            ("src/domain/value-objects/valueObject.js", """
class ValueObject {
  equals(other) {
    return JSON.stringify(this) === JSON.stringify(other);
  }
}
module.exports = { ValueObject };
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptMicroservicesPatternFiles(FrameworkType framework)
    {
        return new[]
        {
            ("services/gateway/index.js", """
const http = require('http');

const services = {
  items: 'http://localhost:3001',
  notifications: 'http://localhost:3002',
};

// Simple proxy gateway
const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ gateway: true, services: Object.keys(services) }));
});

server.listen(process.env.PORT || 3000, () =>
  console.log(`Gateway running on port ${process.env.PORT || 3000}`)
);
"""),
            ("services/items/index.js", """
const http = require('http');

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ service: 'items', items: [] }));
});

server.listen(process.env.PORT || 3001, () =>
  console.log(`Items service running on port ${process.env.PORT || 3001}`)
);
"""),
            ("services/notifications/index.js", """
const http = require('http');

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ service: 'notifications' }));
});

server.listen(process.env.PORT || 3002, () =>
  console.log(`Notifications service running on port ${process.env.PORT || 3002}`)
);
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptSagaPatternFiles(FrameworkType framework)
    {
        var isNest = framework is FrameworkType.NestJs or FrameworkType.NestTs;
        if (isNest)
        {
            return new[]
            {
                ("src/application/sagas/order.saga.ts", """
import { Injectable } from '@nestjs/common';
import { Saga, ICommand, ofType } from '@nestjs/cqrs';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class OrderSaga {
  @Saga()
  orderCreated = (events$: Observable<any>): Observable<ICommand> => {
    return events$.pipe(
      ofType('OrderCreatedEvent'),
      map(event => {
        console.log('Saga: OrderCreatedEvent received', event);
        // Return next command to execute
        return { type: 'SendConfirmationCommand', payload: event };
      }),
    );
  };
}
"""),
            };
        }

        return new[]
        {
            ("src/application/sagas/orderSaga.js", """
class OrderSaga {
  constructor(eventBus, commandBus) {
    this.eventBus = eventBus;
    this.commandBus = commandBus;
    this._subscribe();
  }

  _subscribe() {
    this.eventBus.on('OrderCreated', async (event) => {
      console.log('Saga: OrderCreated', event);
      await this.commandBus.dispatch({ type: 'SendConfirmation', orderId: event.orderId });
    });
    this.eventBus.on('PaymentFailed', async (event) => {
      console.log('Saga: PaymentFailed — compensating', event);
      await this.commandBus.dispatch({ type: 'CancelOrder', orderId: event.orderId });
    });
  }
}
module.exports = { OrderSaga };
"""),
        };
    }

    internal static string NormalizePatternToken(string value)
        => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
