using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

/// <summary>
/// Dockerfiles y CI alternativos por versión de runtime (LTS anteriores y actuales).
/// Permite seleccionar .NET 8/9, Java 17, Python 3.11/3.13, Node 18/20, PHP 8.1/8.2.
/// IDs 5280-5349
/// </summary>
public static partial class AllCombinationsSeeder
{
    private static IEnumerable<ProjectTemplate> GetVersionDockerfiles()
    {
        int id = 5280;

        // ── .NET versiones ────────────────────────────────────────────────────────
        foreach (var (ver, tag) in new[] { ("8", "8.0"), ("9", "9.0") })
        {
            foreach (var (fw, fwLabel) in new[]
            {
                (FrameworkType.AspNetCoreWebApi, "AspNetCoreWebApi"),
                (FrameworkType.AspNetCoreMVC,    "AspNetCoreMVC"),
                (FrameworkType.MinimalApi,       "MinimalApi"),
            })
            {
                yield return T(id++, $"Dockerfile .NET {ver} / {fwLabel}", "dockerfile", ArchitectureType.DotNet,
                    framework: fw,
                    desc: $"Multi-stage Dockerfile para {fwLabel} con .NET {ver}",
                    content:
                    $"FROM mcr.microsoft.com/dotnet/aspnet:{tag} AS base\n" +
                    "WORKDIR /app\nEXPOSE 8080\n\n" +
                    $"FROM mcr.microsoft.com/dotnet/sdk:{tag} AS build\n" +
                    "WORKDIR /src\nCOPY *.csproj .\n" +
                    $"RUN dotnet restore --framework net{ver}.0\n" +
                    "COPY . .\n" +
                    $"RUN dotnet publish -c Release -f net{ver}.0 -o /app/publish\n\n" +
                    "FROM base AS final\nWORKDIR /app\n" +
                    "COPY --from=build /app/publish .\n" +
                    "ENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]");
            }

            yield return T(id++, $"Dockerfile .NET {ver} / BlazorWasm", "dockerfile", ArchitectureType.DotNet,
                framework: FrameworkType.BlazorWasm,
                desc: $"Dockerfile Blazor WebAssembly con .NET {ver} y nginx",
                content:
                $"FROM mcr.microsoft.com/dotnet/sdk:{tag} AS build\n" +
                "WORKDIR /src\nCOPY *.csproj .\n" +
                $"RUN dotnet restore --framework net{ver}.0\nCOPY . .\n" +
                $"RUN dotnet publish -c Release -f net{ver}.0 -o /app/publish\n\n" +
                "FROM nginx:alpine AS final\n" +
                "COPY --from=build /app/publish/wwwroot /usr/share/nginx/html\n" +
                "EXPOSE 80\nCMD [\"nginx\", \"-g\", \"daemon off;\"]");
        }

        // ── Java 17 (LTS anterior) ───────────────────────────────────────────────
        foreach (var (fw, fwLabel, buildCmd) in new[]
        {
            (FrameworkType.SpringBoot, "SpringBoot", "mvn -q package -DskipTests"),
            (FrameworkType.Quarkus,   "Quarkus",    "mvn -q package -DskipTests -Dquarkus.package.type=jar"),
            (FrameworkType.Micronaut, "Micronaut",  "gradle shadowJar --no-daemon -q"),
        })
        {
            var (builderImage, runtimeImage, copyCmd, entrypoint) = fw == FrameworkType.Micronaut
                ? ("gradle:8-jdk17", "eclipse-temurin:17-jre-alpine",
                   "COPY --from=build /app/build/libs/*-all.jar app.jar",
                   "ENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]")
                : ("maven:3.9-eclipse-temurin-17", "eclipse-temurin:17-jre-alpine",
                   "COPY --from=build /app/target/*.jar app.jar",
                   "ENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]");

            yield return T(id++, $"Dockerfile Java 17 / {fwLabel}", "dockerfile", ArchitectureType.Java,
                framework: fw,
                desc: $"Dockerfile multi-stage Java 17 LTS para {fwLabel}",
                content:
                $"FROM {builderImage} AS build\n" +
                "WORKDIR /app\nCOPY . .\n" +
                $"RUN {buildCmd}\n\n" +
                $"FROM {runtimeImage} AS final\n" +
                "WORKDIR /app\n" +
                copyCmd + "\n" +
                "EXPOSE 8080\n" +
                entrypoint);
        }

        // ── Java 24 (latest) ─────────────────────────────────────────────────────
        yield return T(id++, "Dockerfile Java 24 / SpringBoot", "dockerfile", ArchitectureType.Java,
            framework: FrameworkType.SpringBoot,
            desc: "Dockerfile Spring Boot con Java 24 (latest)",
            content:
            "FROM maven:3.9-eclipse-temurin-24 AS build\n" +
            "WORKDIR /app\nCOPY . .\n" +
            "RUN mvn -q package -DskipTests\n\n" +
            "FROM eclipse-temurin:24-jre-alpine AS final\n" +
            "WORKDIR /app\n" +
            "COPY --from=build /app/target/*.jar app.jar\n" +
            "EXPOSE 8080\n" +
            "ENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]");

        // ── Python versiones (3.11, 3.13) ────────────────────────────────────────
        foreach (var pyVer in new[] { "3.11", "3.13" })
        {
            yield return T(id++, $"Dockerfile Python {pyVer} / FastAPI", "dockerfile", ArchitectureType.Python,
                framework: FrameworkType.FastAPI,
                desc: $"Dockerfile FastAPI con Python {pyVer}",
                content:
                $"FROM python:{pyVer}-slim\n" +
                "ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1\n" +
                "WORKDIR /app\nCOPY requirements.txt .\n" +
                "RUN pip install --no-cache-dir -r requirements.txt\n" +
                "COPY . .\nEXPOSE 8000\n" +
                "CMD [\"uvicorn\", \"app.main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8000\"]");

            yield return T(id++, $"Dockerfile Python {pyVer} / Django", "dockerfile", ArchitectureType.Python,
                framework: FrameworkType.Django,
                desc: $"Dockerfile Django con Python {pyVer}",
                content:
                $"FROM python:{pyVer}-slim\n" +
                "ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1\n" +
                "WORKDIR /app\nCOPY requirements.txt .\n" +
                "RUN pip install --no-cache-dir -r requirements.txt\n" +
                "COPY . .\nRUN python manage.py collectstatic --noinput\n" +
                "EXPOSE 8000\n" +
                "CMD [\"gunicorn\", \"{{APP_NAME}}.wsgi:application\", \"--bind\", \"0.0.0.0:8000\"]");

            yield return T(id++, $"Dockerfile Python {pyVer} / Flask", "dockerfile", ArchitectureType.Python,
                framework: FrameworkType.Flask,
                desc: $"Dockerfile Flask con Python {pyVer}",
                content:
                $"FROM python:{pyVer}-slim\n" +
                "ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1\n" +
                "WORKDIR /app\nCOPY requirements.txt .\n" +
                "RUN pip install --no-cache-dir -r requirements.txt\n" +
                "COPY . .\nEXPOSE 5000\n" +
                "CMD [\"gunicorn\", \"app:create_app()\", \"--bind\", \"0.0.0.0:5000\"]");
        }

        // ── PHP versiones (8.1, 8.2) ─────────────────────────────────────────────
        foreach (var phpVer in new[] { "8.1", "8.2" })
        {
            yield return T(id++, $"Dockerfile PHP {phpVer} / Laravel", "dockerfile", ArchitectureType.Php,
                framework: FrameworkType.Laravel,
                desc: $"Dockerfile Laravel con PHP {phpVer}",
                content:
                $"FROM php:{phpVer}-cli-alpine AS base\n" +
                "RUN apk add --no-cache git curl unzip libzip-dev icu-dev oniguruma-dev postgresql-dev \\\n" +
                "    && docker-php-ext-install pdo pdo_mysql pdo_pgsql zip intl bcmath mbstring\n" +
                "COPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\n" +
                "WORKDIR /var/www/html\n" +
                "COPY composer.* ./\n" +
                "RUN composer install --no-dev --optimize-autoloader --no-scripts --no-interaction\n" +
                "COPY . .\n" +
                "RUN chmod -R 775 storage bootstrap/cache\n" +
                "EXPOSE 8080\n" +
                "CMD [\"php\", \"artisan\", \"serve\", \"--host=0.0.0.0\", \"--port=8080\"]");

            yield return T(id++, $"Dockerfile PHP {phpVer} / Symfony", "dockerfile", ArchitectureType.Php,
                framework: FrameworkType.Symfony,
                desc: $"Dockerfile Symfony con PHP {phpVer}",
                content:
                $"FROM php:{phpVer}-cli-alpine AS base\n" +
                "RUN apk add --no-cache git curl unzip libzip-dev icu-dev oniguruma-dev postgresql-dev \\\n" +
                "    && docker-php-ext-install pdo pdo_mysql pdo_pgsql zip intl bcmath mbstring opcache\n" +
                "COPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\n" +
                "WORKDIR /app\n" +
                "COPY composer.* symfony.lock ./\n" +
                "RUN composer install --no-dev --optimize-autoloader --no-scripts --no-interaction\n" +
                "COPY . .\n" +
                "EXPOSE 8080\n" +
                "CMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\"]");
        }

        // ── Node versiones (18 LTS, 20 LTS) ─────────────────────────────────────
        foreach (var (nodeVer, nodeLabel) in new[] { ("18", "18 LTS"), ("20", "20 LTS") })
        {
            yield return T(id++, $"Dockerfile Node {nodeVer} / Express", "dockerfile", ArchitectureType.JavaScript,
                framework: FrameworkType.ExpressJs,
                desc: $"Dockerfile Express.js con Node.js {nodeLabel}",
                content:
                $"FROM node:{nodeVer}-alpine\n" +
                "WORKDIR /app\n" +
                "COPY package*.json ./\n" +
                "RUN npm ci --omit=dev\n" +
                "COPY . .\n" +
                "EXPOSE 3000\n" +
                "CMD [\"node\", \"src/index.js\"]");

            yield return T(id++, $"Dockerfile Node {nodeVer} / NestJS", "dockerfile", ArchitectureType.JavaScript,
                framework: FrameworkType.NestJs,
                desc: $"Dockerfile multi-stage NestJS con Node.js {nodeLabel}",
                content:
                $"FROM node:{nodeVer}-alpine AS build\n" +
                "WORKDIR /app\nCOPY package*.json ./\nRUN npm ci\nCOPY . .\nRUN npm run build\n\n" +
                $"FROM node:{nodeVer}-alpine AS final\n" +
                "WORKDIR /app\n" +
                "COPY --from=build /app/dist ./dist\n" +
                "COPY --from=build /app/node_modules ./node_modules\n" +
                "EXPOSE 3000\nCMD [\"node\", \"dist/main\"]");

            yield return T(id++, $"Dockerfile Node {nodeVer} / NestTS", "dockerfile", ArchitectureType.TypeScript,
                framework: FrameworkType.NestTs,
                desc: $"Dockerfile multi-stage NestJS TypeScript con Node.js {nodeLabel}",
                content:
                $"FROM node:{nodeVer}-alpine AS build\n" +
                "WORKDIR /app\nCOPY package*.json ./\nRUN npm ci\nCOPY . .\nRUN npm run build\n\n" +
                $"FROM node:{nodeVer}-alpine AS final\n" +
                "WORKDIR /app\n" +
                "COPY --from=build /app/dist ./dist\n" +
                "COPY --from=build /app/node_modules ./node_modules\n" +
                "EXPOSE 3000\nCMD [\"node\", \"dist/main\"]");
        }

        // ── CI por versión de lenguaje ───────────────────────────────────────────
        yield return T(id++, "CI .NET 8 GitHub Actions", "ci", ArchitectureType.DotNet,
            desc: "Pipeline CI para proyectos .NET 8",
            content:
            "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n" +
            "jobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n" +
            "      - uses: actions/checkout@v4\n" +
            "      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '8.0.x'\n" +
            "      - run: dotnet restore\n      - run: dotnet build --no-restore\n" +
            "      - run: dotnet test --no-build --verbosity normal");

        yield return T(id++, "CI .NET 9 GitHub Actions", "ci", ArchitectureType.DotNet,
            desc: "Pipeline CI para proyectos .NET 9",
            content:
            "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n" +
            "jobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n" +
            "      - uses: actions/checkout@v4\n" +
            "      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '9.0.x'\n" +
            "      - run: dotnet restore\n      - run: dotnet build --no-restore\n" +
            "      - run: dotnet test --no-build --verbosity normal");

        yield return T(id++, "CI Java 17 GitHub Actions", "ci", ArchitectureType.Java,
            desc: "Pipeline CI para Spring Boot / Quarkus con Java 17",
            content:
            "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n" +
            "jobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n" +
            "      - uses: actions/checkout@v4\n" +
            "      - uses: actions/setup-java@v4\n        with:\n          java-version: '17'\n          distribution: temurin\n          cache: maven\n" +
            "      - run: mvn -B package --no-transfer-progress");

        yield return T(id++, "CI Node 20 GitHub Actions", "ci", ArchitectureType.JavaScript,
            desc: "Pipeline CI para Node.js 20 LTS",
            content:
            "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n" +
            "jobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n" +
            "      - uses: actions/checkout@v4\n" +
            "      - uses: actions/setup-node@v4\n        with:\n          node-version: '20'\n          cache: npm\n" +
            "      - run: npm ci\n      - run: npm run build --if-present\n      - run: npm test --if-present");

        yield return T(id++, "CI Python 3.11 GitHub Actions", "ci", ArchitectureType.Python,
            desc: "Pipeline CI para Python 3.11",
            content:
            "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n" +
            "jobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n" +
            "      - uses: actions/checkout@v4\n" +
            "      - uses: actions/setup-python@v5\n        with:\n          python-version: '3.11'\n" +
            "      - run: pip install -r requirements.txt\n      - run: pytest --tb=short");
    }
}