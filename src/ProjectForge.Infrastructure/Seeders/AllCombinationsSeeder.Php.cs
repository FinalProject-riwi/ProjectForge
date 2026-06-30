using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5130-5169 (PHP ya tiene mucha cobertura; aquí añadimos k8s por DB y compose por framework)
    private static IEnumerable<ProjectTemplate> GetPhpAdditionalTemplates()
    {
        int id = 5130;

        // ── Dockerfiles por framework ────────────────────────────────────────────
        yield return T(id++, "Dockerfile Laravel", "dockerfile", ArchitectureType.Php,
            framework: FrameworkType.Laravel,
            desc: "Dockerfile optimizado para Laravel 11 con PHP 8.3",
            content:
            "FROM php:8.3-cli-alpine AS base\n" +
            "RUN apk add --no-cache git curl unzip libzip-dev icu-dev oniguruma-dev postgresql-dev \\\n" +
            "    && docker-php-ext-install pdo pdo_mysql pdo_pgsql zip intl bcmath mbstring\n" +
            "COPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\n" +
            "WORKDIR /var/www/html\n" +
            "COPY composer.* ./\n" +
            "RUN composer install --no-dev --optimize-autoloader --no-scripts --no-interaction\n" +
            "COPY . .\n" +
            "RUN php artisan config:cache && php artisan route:cache && php artisan view:cache \\\n" +
            "    && chmod -R 775 storage bootstrap/cache\n\n" +
            "EXPOSE 8080\n" +
            "CMD [\"php\", \"artisan\", \"serve\", \"--host=0.0.0.0\", \"--port=8080\"]");

        yield return T(id++, "Dockerfile Symfony", "dockerfile", ArchitectureType.Php,
            framework: FrameworkType.Symfony,
            desc: "Dockerfile para Symfony 7 con PHP 8.3 y FrankenPHP",
            content:
            "FROM php:8.3-cli-alpine AS base\n" +
            "RUN apk add --no-cache git curl unzip libzip-dev icu-dev oniguruma-dev postgresql-dev \\\n" +
            "    && docker-php-ext-install pdo pdo_mysql pdo_pgsql zip intl bcmath mbstring opcache\n" +
            "COPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\n" +
            "WORKDIR /app\n" +
            "COPY composer.* symfony.lock ./\n" +
            "RUN composer install --no-dev --optimize-autoloader --no-scripts --no-interaction\n" +
            "COPY . .\n" +
            "RUN composer dump-autoload --optimize\n\n" +
            "EXPOSE 8080\n" +
            "CMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\"]");

        // ── Compose Laravel × todas las DBs ─────────────────────────────────────
        var laravelConns = new[]
        {
            (DatabaseType.PostgreSQL, "DB_CONNECTION: pgsql\n      DB_HOST: db\n      DB_PORT: 5432\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: postgres\n      DB_PASSWORD: secret"),
            (DatabaseType.MySQL,      "DB_CONNECTION: mysql\n      DB_HOST: db\n      DB_PORT: 3306\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: root\n      DB_PASSWORD: secret"),
            (DatabaseType.SqlServer,  "DB_CONNECTION: sqlsrv\n      DB_HOST: db\n      DB_PORT: 1433\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: sa\n      DB_PASSWORD: Secret1234!"),
            (DatabaseType.MongoDB,    "DB_CONNECTION: mongodb\n      DB_HOST: db\n      DB_PORT: 27017\n      DB_DATABASE: {{DB_NAME}}\n      MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "DB_CONNECTION: redis\n      REDIS_HOST: db\n      REDIS_PORT: 6379\n      CACHE_STORE: redis\n      SESSION_DRIVER: redis"),
            (DatabaseType.SQLite,     "DB_CONNECTION: sqlite\n      DB_DATABASE: /var/www/html/database/database.sqlite"),
        };
        foreach (var (db, envBlock) in laravelConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/var/www/html/database\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Laravel + {db}", "compose", ArchitectureType.Php,
                framework: FrameworkType.Laravel, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Laravel + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                "    environment:\n" +
                "      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_KEY: base64:PLACEHOLDER\n" +
                $"      {envBlock}\n" +
                dep + vol + dbSec);
        }

        // ── Compose Symfony × todas las DBs ─────────────────────────────────────
        var symfonyConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATABASE_URL: pgsql://postgres:secret@db:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "DATABASE_URL: mssql://sa:Secret1234!@db:1433/{{DB_NAME}}"),
            (DatabaseType.MongoDB,    "MONGODB_URL: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URL: redis://db:6379"),
            (DatabaseType.SQLite,     "DATABASE_URL: sqlite:///app/var/data/{{DB_NAME}}.db"),
        };
        foreach (var (db, envLine) in symfonyConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc && db != DatabaseType.SqlServer && db != DatabaseType.MongoDB
                ? "    depends_on:\n      db:\n        condition: service_healthy\n"
                : (hasSvc ? "    depends_on:\n      - db\n" : "");
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app/var/data\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Symfony + {db}", "compose", ArchitectureType.Php,
                framework: FrameworkType.Symfony, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Symfony + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                "    environment:\n      APP_ENV: dev\n      APP_DEBUG: \"1\"\n" +
                $"      {envLine}\n" +
                dep + vol + dbSec);
        }

        // ── K8s PHP por DB ───────────────────────────────────────────────────────
        var phpK8s = new[]
        {
            (DatabaseType.PostgreSQL, "DB_HOST", "postgres-svc"),
            (DatabaseType.MySQL,      "DB_HOST", "mysql-svc"),
            (DatabaseType.SqlServer,  "DB_HOST", "sqlserver-svc"),
            (DatabaseType.MongoDB,    "MONGODB_URI", "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_HOST", "redis-svc"),
            (DatabaseType.SQLite,     "DB_DATABASE", "/data/{{DB_NAME}}.sqlite"),
        };
        foreach (var (db, envKey, connStr) in phpK8s)
        {
            yield return T(id++, $"K8s PHP + {db}", "k8s-deployment", ArchitectureType.Php,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para PHP con {db}",
                content: K8sManifest(envKey, connStr, 8080));
        }
    }

    // ─── Additional PHP Libraries (IDs 11060-11069) ───────────────────────────────
    private static IEnumerable<LibraryRecommendation> GetPhpLibraries() => new[]
    {
        new LibraryRecommendation { Id=11060, CreatedAt=SeedDate, Name="Predis", PackageName="predis/predis", Architecture=ArchitectureType.Php, Category="Cache", Description="Cliente Redis puro PHP", PopularityScore=88, InstallCommand="composer require predis/predis" },
        new LibraryRecommendation { Id=11061, CreatedAt=SeedDate, Name="Laravel MongoDB", PackageName="mongodb/laravel-mongodb", Architecture=ArchitectureType.Php, Framework=FrameworkType.Laravel, Category="Database", Description="Driver MongoDB oficial para Laravel", PopularityScore=84, InstallCommand="composer require mongodb/laravel-mongodb" },
        new LibraryRecommendation { Id=11062, CreatedAt=SeedDate, Name="Laravel Telescope", PackageName="laravel/telescope", Architecture=ArchitectureType.Php, Framework=FrameworkType.Laravel, Category="Observability", Description="Panel de depuración y monitoreo para Laravel", PopularityScore=87, InstallCommand="composer require laravel/telescope" },
        new LibraryRecommendation { Id=11063, CreatedAt=SeedDate, Name="Laravel Octane", PackageName="laravel/octane", Architecture=ArchitectureType.Php, Framework=FrameworkType.Laravel, Category="Performance", Description="Servidor de alto rendimiento para Laravel con FrankenPHP o Swoole", PopularityScore=85, InstallCommand="composer require laravel/octane" },
        new LibraryRecommendation { Id=11064, CreatedAt=SeedDate, Name="Symfony ApiPlatform", PackageName="api-platform/core", Architecture=ArchitectureType.Php, Framework=FrameworkType.Symfony, Category="API", Description="Framework REST/GraphQL para Symfony", PopularityScore=89, InstallCommand="composer require api-platform/core" },
        new LibraryRecommendation { Id=11065, CreatedAt=SeedDate, Name="Symfony Messenger", PackageName="symfony/messenger", Architecture=ArchitectureType.Php, Framework=FrameworkType.Symfony, Category="Messaging", Description="Bus de mensajes para Symfony", PopularityScore=87, InstallCommand="composer require symfony/messenger" },
        new LibraryRecommendation { Id=11066, CreatedAt=SeedDate, Name="PHPStan", PackageName="phpstan/phpstan", Architecture=ArchitectureType.Php, Category="Code Quality", Description="Análisis estático de PHP con tipos", PopularityScore=92, InstallCommand="composer require --dev phpstan/phpstan" },
        new LibraryRecommendation { Id=11067, CreatedAt=SeedDate, Name="Laravel Pint", PackageName="laravel/pint", Architecture=ArchitectureType.Php, Framework=FrameworkType.Laravel, Category="Code Quality", Description="Formateador de código PHP para Laravel", PopularityScore=88, InstallCommand="composer require --dev laravel/pint" },
        new LibraryRecommendation { Id=11068, CreatedAt=SeedDate, Name="PestPHP", PackageName="pestphp/pest", Architecture=ArchitectureType.Php, Category="Testing", Description="Framework de testing elegante para PHP", PopularityScore=90, InstallCommand="composer require --dev pestphp/pest" },
        new LibraryRecommendation { Id=11069, CreatedAt=SeedDate, Name="Nelmio CORS", PackageName="nelmio/cors-bundle", Architecture=ArchitectureType.Php, Framework=FrameworkType.Symfony, Category="Security", Description="CORS configuración para Symfony APIs", PopularityScore=83, InstallCommand="composer require nelmio/cors-bundle" },
    };

    // ─── Additional PHP Patterns (IDs 12010-12019) ───────────────────────────────
    private static IEnumerable<DesignPatternEntry> GetPhpPatterns() => new[]
    {
        new DesignPatternEntry { Id=12010, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Laravel)", Architecture=ArchitectureType.Php, Description="Repositorios sobre Eloquent en Laravel.", ImplementationNotes="Interfaz en app/Contracts/. Implementación en app/Repositories/. Bindear en AppServiceProvider.", ScaffoldCommandsJson="[\"mkdir -p app/Contracts/Repositories app/Repositories\"]" },
        new DesignPatternEntry { Id=12011, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Laravel)", Architecture=ArchitectureType.Php, Description="Capas Domain/Application/Infrastructure en Laravel.", ImplementationNotes="Domain sin imports de Illuminate. Eloquent solo en Infrastructure. UseCases en Application.", ScaffoldCommandsJson="[\"mkdir -p app/Domain/Entities app/Domain/Repositories app/Application/UseCases app/Infrastructure\"]" },
        new DesignPatternEntry { Id=12012, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (Symfony)", Architecture=ArchitectureType.Php, Description="Ports & Adapters con Symfony DI Container.", ImplementationNotes="Ports como interfaces PHP. Adapters decorados con #[AsService]. Symfony autowiring conecta automáticamente.", ScaffoldCommandsJson="[\"mkdir -p src/Core/Port src/Core/Domain src/Infrastructure/Adapter src/UI\"]" },
        new DesignPatternEntry { Id=12013, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS (Laravel)", Architecture=ArchitectureType.Php, Description="Separa Commands y Queries con Laravel Jobs o Handlers.", ImplementationNotes="Commands como Jobs de Laravel. Queries como clases simples con handle(). Opcional: usar Tactician Bus.", ScaffoldCommandsJson="[\"mkdir -p app/Application/Commands app/Application/Queries app/Application/Handlers\"]" },
        new DesignPatternEntry { Id=12014, CreatedAt=SeedDate, Pattern=DesignPattern.MVVM, Name="MVVM (Laravel/Livewire)", Architecture=ArchitectureType.Php, Description="Model-View-ViewModel con Livewire para interfaces reactivas.", ImplementationNotes="Livewire Component como ViewModel. Blade template como View. Eloquent Model como Model.", ScaffoldCommandsJson="[\"mkdir -p app/Http/Livewire resources/views/livewire\"]" },
        new DesignPatternEntry { Id=12015, CreatedAt=SeedDate, Pattern=DesignPattern.EventSourcing, Name="Event Sourcing (Laravel)", Architecture=ArchitectureType.Php, Description="Eventos de dominio con spatie/laravel-event-sourcing.", ImplementationNotes="Instalar spatie/laravel-event-sourcing. Aggregates extienden AggregateRoot. StoredEvents en BD.", ScaffoldCommandsJson="[\"mkdir -p app/Domain/Aggregates app/Domain/Events app/Projectors\"]" },
        new DesignPatternEntry { Id=12016, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (Symfony)", Architecture=ArchitectureType.Php, Description="DDD con Aggregates, Value Objects y Domain Events en Symfony.", ImplementationNotes="Symfony EventDispatcher para Domain Events. Doctrine Embeddables para Value Objects.", ScaffoldCommandsJson="[\"mkdir -p src/Domain/Aggregate src/Domain/ValueObject src/Domain/Event src/Application\"]" },
        new DesignPatternEntry { Id=12017, CreatedAt=SeedDate, Pattern=DesignPattern.Saga, Name="Saga (Laravel)", Architecture=ArchitectureType.Php, Description="Orquesta transacciones con Laravel Queues y Jobs compensatorios.", ImplementationNotes="Cada paso del saga como Job. On failure: dispatch compensatory job. Usar DB para estado.", ScaffoldCommandsJson="[\"mkdir -p app/Application/Sagas app/Jobs\"]" },
        new DesignPatternEntry { Id=12018, CreatedAt=SeedDate, Pattern=DesignPattern.Mediator, Name="Mediator (Symfony / Messenger)", Architecture=ArchitectureType.Php, Description="Patrón Mediator con Symfony Messenger como bus de mensajes.", ImplementationNotes="MessageBus como mediador. Commands y Queries como Messages. Handlers como MessageHandler.", ScaffoldCommandsJson="[\"mkdir -p src/Application/Command src/Application/Query src/Application/Handler\"]" },
        new DesignPatternEntry { Id=12019, CreatedAt=SeedDate, Pattern=DesignPattern.Microservices, Name="Microservices (PHP/Laravel)", Architecture=ArchitectureType.Php, Description="Servicios independientes Laravel comunicándose via HTTP/RabbitMQ.", ImplementationNotes="Cada servicio con su propio composer.json. Guzzle para HTTP. Laravel Horizon para queues.", ScaffoldCommandsJson="[\"mkdir -p services/gateway services/orders services/notifications\"]" },
    };
}