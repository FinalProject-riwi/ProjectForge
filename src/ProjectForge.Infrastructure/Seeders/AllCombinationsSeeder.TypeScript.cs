using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5230-5279
    private static IEnumerable<ProjectTemplate> GetTsTemplates()
    {
        int id = 5230;

        // ── Dockerfile NextTS ────────────────────────────────────────────────────
        yield return T(id++, "Dockerfile NextTS", "dockerfile", ArchitectureType.TypeScript,
            framework: FrameworkType.NextTs,
            desc: "Dockerfile multi-stage para Next.js con TypeScript (standalone output)",
            content:
            "FROM node:22-alpine AS deps\n" +
            "WORKDIR /app\n" +
            "COPY package*.json ./\n" +
            "RUN npm ci\n\n" +
            "FROM node:22-alpine AS build\n" +
            "WORKDIR /app\n" +
            "COPY --from=deps /app/node_modules ./node_modules\n" +
            "COPY . .\n" +
            "RUN npm run build\n\n" +
            "FROM node:22-alpine AS final\n" +
            "WORKDIR /app\n" +
            "ENV NODE_ENV=production\n" +
            "COPY --from=build /app/.next/standalone ./\n" +
            "COPY --from=build /app/.next/static ./.next/static\n" +
            "COPY --from=build /app/public ./public\n" +
            "EXPOSE 3000\n" +
            "CMD [\"node\", \"server.js\"]");

        // ── Compose NestTS × todas las DBs ──────────────────────────────────────
        var nestTsConns = new[]
        {
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_HOST: db\n      REDIS_PORT: 6379"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in nestTsConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose NestTS + {db}", "compose", ArchitectureType.TypeScript,
                framework: FrameworkType.NestTs, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para NestJS TypeScript + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
                "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
                $"      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── Compose NextTS × todas las DBs ──────────────────────────────────────
        var nextTsConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL: redis://db:6379"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in nextTsConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose NextTS + {db}", "compose", ArchitectureType.TypeScript,
                framework: FrameworkType.NextTs, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Next.js TypeScript + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
                "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
                $"      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── K8s TypeScript por DB ────────────────────────────────────────────────
        var tsK8s = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL", "postgresql://postgres:secret@postgres-svc:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL", "mysql://root:secret@mysql-svc:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL", "sqlserver://sqlserver-svc:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI",  "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL",    "redis://redis-svc:6379"),
            (DatabaseType.SQLite,     "DATABASE_URL", "sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envKey, connStr) in tsK8s)
        {
            yield return T(id++, $"K8s TypeScript + {db}", "k8s-deployment", ArchitectureType.TypeScript,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para TypeScript con {db}",
                content: K8sManifest(envKey, connStr, 3000));
        }
    }

    // ─── Additional TypeScript Libraries (IDs 11084-11094) ───────────────────────
    private static IEnumerable<LibraryRecommendation> GetTsLibraries() => new[]
    {
        new LibraryRecommendation { Id=11084, CreatedAt=SeedDate, Name="@nestjs/graphql", PackageName="@nestjs/graphql", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="API", Description="GraphQL con code-first para NestJS", PopularityScore=88, InstallCommand="npm install @nestjs/graphql @nestjs/apollo graphql apollo-server-express" },
        new LibraryRecommendation { Id=11085, CreatedAt=SeedDate, Name="@nestjs/mongoose", PackageName="@nestjs/mongoose", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="ODM", Description="Integración Mongoose oficial para NestJS", PopularityScore=87, InstallCommand="npm install @nestjs/mongoose mongoose" },
        new LibraryRecommendation { Id=11086, CreatedAt=SeedDate, Name="@nestjs/config", PackageName="@nestjs/config", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="Configuration", Description="Gestión de configuración por entorno para NestJS", PopularityScore=96, InstallCommand="npm install @nestjs/config" },
        new LibraryRecommendation { Id=11087, CreatedAt=SeedDate, Name="Drizzle ORM (TS)", PackageName="drizzle-orm", Architecture=ArchitectureType.TypeScript, Category="ORM", Description="ORM TypeScript-first sin código generado en runtime", PopularityScore=90, InstallCommand="npm install drizzle-orm drizzle-kit" },
        new LibraryRecommendation { Id=11088, CreatedAt=SeedDate, Name="tRPC (TS)", PackageName="@trpc/server", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NextTs, Category="API", Description="APIs type-safe E2E para Next.js TypeScript", PopularityScore=93, InstallCommand="npm install @trpc/server @trpc/client @trpc/react-query" },
        new LibraryRecommendation { Id=11089, CreatedAt=SeedDate, Name="class-transformer", PackageName="class-transformer", Architecture=ArchitectureType.TypeScript, Category="Serialization", Description="Transformación de objetos planos a clases TypeScript", PopularityScore=91, InstallCommand="npm install class-transformer reflect-metadata" },
        new LibraryRecommendation { Id=11090, CreatedAt=SeedDate, Name="@nestjs/bull", PackageName="@nestjs/bull", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="Background Jobs", Description="Integración BullMQ/Bull para NestJS", PopularityScore=87, InstallCommand="npm install @nestjs/bull bull" },
        new LibraryRecommendation { Id=11091, CreatedAt=SeedDate, Name="Supertest", PackageName="supertest", Architecture=ArchitectureType.TypeScript, Category="Testing", Description="Tests de integración HTTP para APIs TypeScript", PopularityScore=92, InstallCommand="npm install --save-dev supertest @types/supertest" },
        new LibraryRecommendation { Id=11092, CreatedAt=SeedDate, Name="Zod (TS)", PackageName="zod", Architecture=ArchitectureType.TypeScript, Category="Validation", Description="Validación de esquemas TypeScript con inferencia de tipos", PopularityScore=98, InstallCommand="npm install zod" },
        new LibraryRecommendation { Id=11093, CreatedAt=SeedDate, Name="Pino", PackageName="pino", Architecture=ArchitectureType.TypeScript, Category="Logging", Description="Logger JSON de alto rendimiento para Node.js", PopularityScore=91, InstallCommand="npm install pino pino-pretty" },
        new LibraryRecommendation { Id=11094, CreatedAt=SeedDate, Name="@nestjs/cache-manager", PackageName="@nestjs/cache-manager", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="Cache", Description="Módulo de caché oficial para NestJS", PopularityScore=86, InstallCommand="npm install @nestjs/cache-manager cache-manager" },
    };
}