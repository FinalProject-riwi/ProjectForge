using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

/// <summary>
/// .env.example and Makefile templates for each supported architecture.
/// IDs 5350-5361
/// </summary>
public static partial class AllCombinationsSeeder
{
    private static IEnumerable<ProjectTemplate> GetEnvAndMakefileTemplates()
    {
        int id = 5350;

        // ── .env.example per architecture ───────────────────────────────────────
        yield return T(id++, ".env.example .NET", "dotenv", ArchitectureType.DotNet,
            desc: "Variables de entorno ejemplo para proyectos .NET",
            content:
            "# Application\n" +
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "ASPNETCORE_ENVIRONMENT=Development\n" +
            "ASPNETCORE_URLS=http://+:8080\n\n" +
            "# Database\n" +
            "ConnectionStrings__Default=Host=localhost;Port={{DB_PORT}};Database={{DB_NAME}};Username=postgres;Password=your_password\n" +
            "ConnectionStrings__Redis=localhost:6379\n\n" +
            "# Auth (opcional)\n" +
            "JWT_SECRET=your-256-bit-secret-key-here\n" +
            "JWT_ISSUER={{APP_NAME}}\n" +
            "JWT_AUDIENCE={{APP_NAME}}-client");

        yield return T(id++, ".env.example Java", "dotenv", ArchitectureType.Java,
            desc: "Variables de entorno ejemplo para proyectos Java/Spring",
            content:
            "# Application\n" +
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "SPRING_PROFILES_ACTIVE=dev\n\n" +
            "# Database\n" +
            "SPRING_DATASOURCE_URL=jdbc:postgresql://localhost:{{DB_PORT}}/{{DB_NAME}}\n" +
            "SPRING_DATASOURCE_USERNAME=postgres\n" +
            "SPRING_DATASOURCE_PASSWORD=your_password\n" +
            "SPRING_JPA_HIBERNATE_DDL_AUTO=update\n\n" +
            "# JWT (opcional)\n" +
            "JWT_SECRET=your-256-bit-secret-key-here\n" +
            "JWT_EXPIRATION=86400000");

        yield return T(id++, ".env.example Python", "dotenv", ArchitectureType.Python,
            desc: "Variables de entorno ejemplo para proyectos Python",
            content:
            "# Application\n" +
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "ENVIRONMENT=development\n" +
            "DEBUG=True\n" +
            "SECRET_KEY=your-secret-key-change-in-production\n\n" +
            "# Database\n" +
            "DATABASE_URL=postgresql://postgres:your_password@localhost:{{DB_PORT}}/{{DB_NAME}}\n" +
            "REDIS_URL=redis://localhost:6379\n\n" +
            "# CORS\n" +
            "ALLOWED_ORIGINS=http://localhost:3000,http://localhost:8080");

        yield return T(id++, ".env.example PHP", "dotenv", ArchitectureType.Php,
            desc: "Variables de entorno ejemplo para proyectos PHP/Laravel/Symfony",
            content:
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "APP_ENV=local\n" +
            "APP_DEBUG=true\n" +
            "APP_URL=http://localhost:{{APP_PORT}}\n" +
            "APP_KEY=\n\n" +
            "# Database\n" +
            "DB_CONNECTION=pgsql\n" +
            "DB_HOST=localhost\n" +
            "DB_PORT={{DB_PORT}}\n" +
            "DB_DATABASE={{DB_NAME}}\n" +
            "DB_USERNAME=postgres\n" +
            "DB_PASSWORD=your_password\n\n" +
            "# Cache\n" +
            "CACHE_STORE=redis\n" +
            "REDIS_HOST=localhost\n" +
            "REDIS_PORT=6379\n" +
            "SESSION_DRIVER=redis\n" +
            "QUEUE_CONNECTION=redis");

        yield return T(id++, ".env.example JavaScript", "dotenv", ArchitectureType.JavaScript,
            desc: "Variables de entorno ejemplo para proyectos Node.js/JavaScript",
            content:
            "# Application\n" +
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "NODE_ENV=development\n" +
            "PORT={{APP_PORT}}\n\n" +
            "# Database\n" +
            "DATABASE_URL=postgresql://postgres:your_password@localhost:{{DB_PORT}}/{{DB_NAME}}\n" +
            "MONGODB_URI=mongodb://localhost:27017/{{DB_NAME}}\n" +
            "REDIS_URL=redis://localhost:6379\n\n" +
            "# Auth\n" +
            "JWT_SECRET=your-secret-key\n" +
            "JWT_EXPIRES_IN=7d");

        yield return T(id++, ".env.example TypeScript", "dotenv", ArchitectureType.TypeScript,
            desc: "Variables de entorno ejemplo para proyectos TypeScript/NestTS/NextTS",
            content:
            "# Application\n" +
            "APP_NAME={{APP_NAME}}\n" +
            "APP_PORT={{APP_PORT}}\n" +
            "NODE_ENV=development\n" +
            "PORT={{APP_PORT}}\n\n" +
            "# Database\n" +
            "DATABASE_URL=postgresql://postgres:your_password@localhost:{{DB_PORT}}/{{DB_NAME}}\n" +
            "MONGODB_URI=mongodb://localhost:27017/{{DB_NAME}}\n" +
            "REDIS_URL=redis://localhost:6379\n\n" +
            "# Auth\n" +
            "JWT_SECRET=your-secret-key\n" +
            "JWT_EXPIRES_IN=7d\n\n" +
            "# Next.js public vars (prefix NEXT_PUBLIC_)\n" +
            "NEXT_PUBLIC_API_URL=http://localhost:{{APP_PORT}}");

        // ── Makefile per architecture ────────────────────────────────────────────
        yield return T(id++, "Makefile .NET", "makefile", ArchitectureType.DotNet,
            desc: "Makefile con comandos comunes para proyectos .NET",
            content:
            ".PHONY: build run watch test clean docker-up docker-down migrate add-migration publish\n\n" +
            "build:\n\tdotnet build\n\n" +
            "run:\n\tdotnet run\n\n" +
            "watch:\n\tdotnet watch run\n\n" +
            "test:\n\tdotnet test --no-build --logger trx\n\n" +
            "clean:\n\tdotnet clean\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v\n\n" +
            "migrate:\n\tdotnet ef database update\n\n" +
            "add-migration:\n\tdotnet ef migrations add $(name)\n\n" +
            "publish:\n\tdotnet publish -c Release -o publish/");

        yield return T(id++, "Makefile Java", "makefile", ArchitectureType.Java,
            desc: "Makefile con comandos comunes para proyectos Java/Spring",
            content:
            ".PHONY: build run test package clean docker-up docker-down lint\n\n" +
            "build:\n\tmvn compile\n\n" +
            "run:\n\tmvn spring-boot:run\n\n" +
            "test:\n\tmvn test\n\n" +
            "package:\n\tmvn package -DskipTests\n\n" +
            "clean:\n\tmvn clean\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v\n\n" +
            "lint:\n\tmvn checkstyle:check");

        yield return T(id++, "Makefile Python", "makefile", ArchitectureType.Python,
            desc: "Makefile con comandos comunes para proyectos Python",
            content:
            ".PHONY: install run dev test lint format docker-up docker-down migrate migration\n\n" +
            "install:\n\tpip install -r requirements.txt\n\n" +
            "run:\n\tuvicorn app.main:app --host 0.0.0.0 --port 8000\n\n" +
            "dev:\n\tuvicorn app.main:app --reload --port 8000\n\n" +
            "test:\n\tpytest --tb=short -v\n\n" +
            "lint:\n\truff check .\n\n" +
            "format:\n\truff format .\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v\n\n" +
            "migrate:\n\talembic upgrade head\n\n" +
            "migration:\n\talembic revision --autogenerate -m \"$(name)\"");

        yield return T(id++, "Makefile PHP", "makefile", ArchitectureType.Php,
            desc: "Makefile con comandos comunes para proyectos PHP/Laravel/Symfony",
            content:
            ".PHONY: install run test lint docker-up docker-down migrate fresh key\n\n" +
            "install:\n\tcomposer install\n\n" +
            "run:\n\tphp artisan serve\n\n" +
            "test:\n\tphp artisan test\n\n" +
            "lint:\n\t./vendor/bin/pint\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v\n\n" +
            "migrate:\n\tphp artisan migrate\n\n" +
            "fresh:\n\tphp artisan migrate:fresh --seed\n\n" +
            "key:\n\tphp artisan key:generate");

        yield return T(id++, "Makefile JavaScript", "makefile", ArchitectureType.JavaScript,
            desc: "Makefile con comandos comunes para proyectos JavaScript/Node.js",
            content:
            ".PHONY: install dev build test lint clean docker-up docker-down\n\n" +
            "install:\n\tnpm install\n\n" +
            "dev:\n\tnpm run dev\n\n" +
            "build:\n\tnpm run build\n\n" +
            "test:\n\tnpm test\n\n" +
            "lint:\n\tnpm run lint\n\n" +
            "clean:\n\trm -rf node_modules dist .next\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v");

        yield return T(id++, "Makefile TypeScript", "makefile", ArchitectureType.TypeScript,
            desc: "Makefile con comandos comunes para proyectos TypeScript",
            content:
            ".PHONY: install dev build test lint type-check clean docker-up docker-down\n\n" +
            "install:\n\tnpm install\n\n" +
            "dev:\n\tnpm run dev\n\n" +
            "build:\n\tnpm run build\n\n" +
            "test:\n\tnpm test\n\n" +
            "lint:\n\tnpm run lint\n\n" +
            "type-check:\n\ttsc --noEmit\n\n" +
            "clean:\n\trm -rf node_modules dist .next\n\n" +
            "docker-up:\n\tdocker-compose up -d\n\n" +
            "docker-down:\n\tdocker-compose down -v");
    }
}