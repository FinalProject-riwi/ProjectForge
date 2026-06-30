using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5180-5229
    private static IEnumerable<ProjectTemplate> GetJsAdditionalTemplates()
    {
        int id = 5180;

        // ── Compose genérico: DBs faltantes (ya existen PG, MySQL, SQLite) ───────
        yield return T(id++, "Compose JavaScript + SqlServer", "compose", ArchitectureType.JavaScript,
            db: DatabaseType.SqlServer, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para Node.js con SQL Server",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
            "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
            "      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=Secret1234!;encrypt=false\n" +
            "    depends_on:\n      - db\n" +
            DbServiceBlock(DatabaseType.SqlServer));

        yield return T(id++, "Compose JavaScript + MongoDB", "compose", ArchitectureType.JavaScript,
            db: DatabaseType.MongoDB, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para Node.js con MongoDB",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
            "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
            "      MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}\n" +
            "    depends_on:\n      - db\n" +
            DbServiceBlock(DatabaseType.MongoDB));

        yield return T(id++, "Compose JavaScript + Redis", "compose", ArchitectureType.JavaScript,
            db: DatabaseType.Redis, infra: InfrastructureType.DockerCompose,
            desc: "Docker Compose para Node.js con Redis",
            content:
            "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
            "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
            "      REDIS_URL: redis://db:6379\n" +
            "    depends_on:\n      db:\n        condition: service_healthy\n" +
            DbServiceBlock(DatabaseType.Redis));

        // ── Dockerfile framework-specific ────────────────────────────────────────
        yield return T(id++, "Dockerfile NestJS", "dockerfile", ArchitectureType.JavaScript,
            framework: FrameworkType.NestJs,
            desc: "Dockerfile multi-stage optimizado para NestJS",
            content:
            "FROM node:22-alpine AS build\n" +
            "WORKDIR /app\n" +
            "COPY package*.json ./\n" +
            "RUN npm ci\n" +
            "COPY . .\n" +
            "RUN npm run build\n\n" +
            "FROM node:22-alpine AS final\n" +
            "WORKDIR /app\n" +
            "COPY --from=build /app/dist ./dist\n" +
            "COPY --from=build /app/node_modules ./node_modules\n" +
            "COPY package.json ./\n" +
            "EXPOSE 3000\n" +
            "CMD [\"node\", \"dist/main\"]");

        yield return T(id++, "Dockerfile NextJS", "dockerfile", ArchitectureType.JavaScript,
            framework: FrameworkType.NextJs,
            desc: "Dockerfile multi-stage optimizado para Next.js con output standalone",
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

        // ── Compose NestJS × todas las DBs ──────────────────────────────────────
        var nestConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: postgres://postgres:secret@db:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_HOST: db\n      REDIS_PORT: 6379"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in nestConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose NestJS + {db}", "compose", ArchitectureType.JavaScript,
                framework: FrameworkType.NestJs, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para NestJS + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
                "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
                $"      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── Compose NextJS × todas las DBs ──────────────────────────────────────
        var nextConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL: redis://db:6379"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in nextConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose NextJS + {db}", "compose", ArchitectureType.JavaScript,
                framework: FrameworkType.NextJs, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Next.js + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n" +
                "    environment:\n      NODE_ENV: development\n      PORT: 3000\n" +
                $"      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── K8s por DB ───────────────────────────────────────────────────────────
        var jsK8s = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL", "postgres://postgres:secret@postgres-svc:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL", "mysql://root:secret@mysql-svc:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL", "sqlserver://sqlserver-svc:1433;database={{DB_NAME}};user=sa;password=Secret1234!"),
            (DatabaseType.MongoDB,    "MONGODB_URI",  "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL",    "redis://redis-svc:6379"),
            (DatabaseType.SQLite,     "DATABASE_URL", "sqlite:./{{DB_NAME}}.db"),
        };
        foreach (var (db, envKey, connStr) in jsK8s)
        {
            yield return T(id++, $"K8s JavaScript + {db}", "k8s-deployment", ArchitectureType.JavaScript,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para Node.js con {db}",
                content: K8sManifest(envKey, connStr, 3000));
        }
    }

    // ─── Additional JS Libraries (IDs 11070-11083) ───────────────────────────────
    private static IEnumerable<LibraryRecommendation> GetJsLibraries() => new[]
    {
        new LibraryRecommendation { Id=11070, CreatedAt=SeedDate, Name="Prisma", PackageName="prisma", Architecture=ArchitectureType.JavaScript, Category="ORM", Description="ORM type-safe con soporte multi-DB para Node.js", PopularityScore=97, InstallCommand="npm install prisma @prisma/client" },
        new LibraryRecommendation { Id=11071, CreatedAt=SeedDate, Name="Mongoose", PackageName="mongoose", Architecture=ArchitectureType.JavaScript, Category="ODM", Description="ODM para MongoDB en Node.js", PopularityScore=94, InstallCommand="npm install mongoose" },
        new LibraryRecommendation { Id=11072, CreatedAt=SeedDate, Name="Bull", PackageName="bull", Architecture=ArchitectureType.JavaScript, Category="Background Jobs", Description="Cola de tareas Redis para Node.js", PopularityScore=92, InstallCommand="npm install bull" },
        new LibraryRecommendation { Id=11073, CreatedAt=SeedDate, Name="Socket.io", PackageName="socket.io", Architecture=ArchitectureType.JavaScript, Category="WebSocket", Description="WebSockets en tiempo real para Node.js", PopularityScore=96, InstallCommand="npm install socket.io" },
        new LibraryRecommendation { Id=11074, CreatedAt=SeedDate, Name="Passport.js", PackageName="passport", Architecture=ArchitectureType.JavaScript, Framework=FrameworkType.ExpressJs, Category="Auth", Description="Middleware de autenticación para Express", PopularityScore=91, InstallCommand="npm install passport passport-jwt passport-local" },
        new LibraryRecommendation { Id=11075, CreatedAt=SeedDate, Name="TypeORM (JS)", PackageName="typeorm", Architecture=ArchitectureType.JavaScript, Category="ORM", Description="ORM para Node.js con soporte decoradores", PopularityScore=88, InstallCommand="npm install typeorm reflect-metadata" },
        new LibraryRecommendation { Id=11076, CreatedAt=SeedDate, Name="@nestjs/throttler", PackageName="@nestjs/throttler", Architecture=ArchitectureType.JavaScript, Framework=FrameworkType.NestJs, Category="Security", Description="Rate limiting para NestJS", PopularityScore=87, InstallCommand="npm install @nestjs/throttler" },
        new LibraryRecommendation { Id=11077, CreatedAt=SeedDate, Name="@nestjs/typeorm", PackageName="@nestjs/typeorm", Architecture=ArchitectureType.JavaScript, Framework=FrameworkType.NestJs, Category="ORM", Description="Integración TypeORM oficial para NestJS", PopularityScore=91, InstallCommand="npm install @nestjs/typeorm typeorm" },
        new LibraryRecommendation { Id=11078, CreatedAt=SeedDate, Name="SWR", PackageName="swr", Architecture=ArchitectureType.JavaScript, Framework=FrameworkType.NextJs, Category="Data Fetching", Description="Data fetching con revalidación para Next.js", PopularityScore=90, InstallCommand="npm install swr" },
        new LibraryRecommendation { Id=11079, CreatedAt=SeedDate, Name="ioredis (JS)", PackageName="ioredis", Architecture=ArchitectureType.JavaScript, Category="Cache", Description="Cliente Redis robusto para Node.js", PopularityScore=91, InstallCommand="npm install ioredis" },
        new LibraryRecommendation { Id=11080, CreatedAt=SeedDate, Name="Winston", PackageName="winston", Architecture=ArchitectureType.JavaScript, Category="Logging", Description="Logger universal para Node.js", PopularityScore=95, InstallCommand="npm install winston" },
        new LibraryRecommendation { Id=11081, CreatedAt=SeedDate, Name="Vitest", PackageName="vitest", Architecture=ArchitectureType.JavaScript, Category="Testing", Description="Framework de testing ultrarrápido compatible con Vite", PopularityScore=94, InstallCommand="npm install --save-dev vitest" },
        new LibraryRecommendation { Id=11082, CreatedAt=SeedDate, Name="Drizzle ORM", PackageName="drizzle-orm", Architecture=ArchitectureType.JavaScript, Category="ORM", Description="ORM TypeScript-first headless y liviano", PopularityScore=89, InstallCommand="npm install drizzle-orm drizzle-kit" },
        new LibraryRecommendation { Id=11083, CreatedAt=SeedDate, Name="tRPC", PackageName="@trpc/server", Architecture=ArchitectureType.JavaScript, Framework=FrameworkType.NextJs, Category="API", Description="APIs type-safe de extremo a extremo sin schema", PopularityScore=92, InstallCommand="npm install @trpc/server @trpc/client" },
    };
}