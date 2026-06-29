using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all PHP data: Templates, Libraries y DesignPatterns.
/// Frameworks: Laravel 11, Symfony 7.
/// Databases: PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite.
/// NOTA: Esta migración reemplaza a la 20260625210000_AddPhpSeedData original
/// que ya está aplicada. Solo inserta los IDs que no existan (IF NOT EXISTS).
/// </summary>
public partial class AddPhpSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns PHP (IDs 7–18) ─────────────────────────────────
        // Laravel (7–12) + Symfony (13–18)
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 7)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (7,N'Php','2026-01-01T00:00:00Z',N'Aisla la persistencia detrás de un contrato y una implementación Eloquent en Laravel.',N'Crear app/Repositories/Contracts, app/Repositories, app/Http/Controllers, app/Models y app/Providers; registrar el binding en un provider.',N'Repository Pattern (Laravel)',N'Repository',N'["mkdir -p app/Repositories/Contracts app/Repositories app/Http/Controllers app/Models app/Providers database/migrations routes"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 8)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (8,N'Php','2026-01-01T00:00:00Z',N'Organiza Laravel en capas para mantener la lógica de negocio fuera del framework.',N'Separar app/Domain, app/Application y app/Infrastructure; dejar Http/Console como adaptadores.',N'Clean Architecture (Laravel)',N'CleanArchitecture',N'["mkdir -p app/Domain app/Application app/Infrastructure app/Contracts"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 9)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (9,N'Php','2026-01-01T00:00:00Z',N'Usa puertos y adaptadores para que Laravel actúe solo como capa de entrada y salida.',N'Modelar puertos de entrada en app/Application/Ports/In y puertos de salida en app/Application/Ports/Out; los controladores consumen los puertos de entrada.',N'Hexagonal Architecture (Laravel)',N'HexagonalArchitecture',N'["mkdir -p app/Domain/Entities app/Application/DTOs app/Application/Ports/In app/Application/Ports/Out app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 10)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (10,N'Php','2026-01-01T00:00:00Z',N'Estructura el código alrededor del dominio, no de los controladores o de la capa HTTP.',N'Crear aggregate roots, value objects, domain services, domain events y repositorios de dominio por bounded context.',N'Domain-Driven Design (Laravel)',N'DomainDrivenDesign',N'["mkdir -p app/Domain/Aggregates app/Domain/Entities app/Domain/Events app/Domain/Repositories app/Domain/Services app/Domain/ValueObjects app/Application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 11)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (11,N'Php','2026-01-01T00:00:00Z',N'Registra los cambios como eventos y reconstruye el agregado por rehidratación.',N'Crear stored_events, projectors, read models, event store, use cases y controlador HTTP.',N'Event Sourcing (Laravel)',N'EventSourcing',N'["mkdir -p app/Domain/Aggregates app/Domain/Events app/Domain/Repositories app/Application/DTOs app/Application/UseCases app/Infrastructure/EventStore app/Infrastructure/Projectors app/Models database/migrations routes"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 12)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (12,N'Php','2026-01-01T00:00:00Z',N'Divide el sistema en un gateway y servicios Laravel separados con mensajería asíncrona.',N'Crear un gateway en la raíz, servicios en services/projects y services/notifications, un broker RabbitMQ y bases separadas por servicio.',N'Microservices (Laravel)',N'Microservices',N'["mkdir -p services/projects services/notifications shared/contracts"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 13)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (13,N'Php','2026-01-01T00:00:00Z',N'Separa la persistencia del dominio usando controladores, casos de uso y repositorios Doctrine en Symfony.',N'Crear src/Domain, src/Application, src/Controller y src/Infrastructure/Persistence; registrar bindings en config/services.yaml.',N'Repository Pattern (Symfony)',N'Repository',N'["mkdir -p src/Domain/Entities src/Domain/Repositories src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 14)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (14,N'Php','2026-01-01T00:00:00Z',N'Organiza Symfony en capas para mantener la lógica de negocio fuera del framework.',N'Separar src/Domain, src/Application, src/Infrastructure y src/Controller.',N'Clean Architecture (Symfony)',N'CleanArchitecture',N'["mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 15)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (15,N'Php','2026-01-01T00:00:00Z',N'Usa puertos y adaptadores para aislar Symfony del dominio.',N'Modelar puertos de entrada en src/Application/Ports/In y puertos de salida en src/Application/Ports/Out; Doctrine implementa los de salida.',N'Hexagonal Architecture (Symfony)',N'HexagonalArchitecture',N'["mkdir -p src/Application/Ports/In src/Application/Ports/Out src/Application/DTOs src/Application/UseCases src/Domain/Aggregates src/Domain/Entities src/Domain/Repositories src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 16)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (16,N'Php','2026-01-01T00:00:00Z',N'Estructura el código alrededor del dominio en Symfony.',N'Crear agregados, value objects, eventos, repositorios de dominio, casos de uso y un controlador por bounded context.',N'Domain-Driven Design (Symfony)',N'DomainDrivenDesign',N'["mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 17)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (17,N'Php','2026-01-01T00:00:00Z',N'Conserva el historial de cambios como eventos y reconstruye el agregado al rehidratarlo en Symfony.',N'Crear stored_events, proyecciones, event store, projectors, casos de uso y controlador HTTP.',N'Event Sourcing (Symfony)',N'EventSourcing',N'["mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/Repositories src/Application/DTOs src/Application/UseCases src/Infrastructure/EventStore src/Infrastructure/Projectors src/Entity config/packages config/routes migrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18,N'Php','2026-01-01T00:00:00Z',N'Divide la aplicación en un gateway y servicios Symfony separados con mensajería asíncrona.',N'Crear un gateway en la raíz, servicios en services/projects y services/notifications, colas con Redis o RabbitMQ.',N'Microservices (Symfony)',N'Microservices',N'["mkdir -p services/projects services/notifications shared/contracts"]');

            -- Patrones adicionales para Laravel
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18100)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18100,N'Php','2026-01-01T00:00:00Z',N'Separa comandos y consultas en Laravel usando Bus de Comandos.',N'Usar artisan make:command y make:job. Bus de comandos mediante Jobs y el dispatcher de Laravel.',N'CQRS (Laravel)',N'CQRS',N'["mkdir -p app/Commands app/Queries app/Handlers app/Http/Controllers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18101)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18101,N'Php','2026-01-01T00:00:00Z',N'Reduce el acoplamiento usando el Event System de Laravel.',N'Usar Event/Listener de Laravel. Registrar listeners en EventServiceProvider.',N'Mediator (Laravel)',N'Mediator',N'["mkdir -p app/Events app/Listeners app/Http/Controllers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18102)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18102,N'Php','2026-01-01T00:00:00Z',N'Orquesta transacciones distribuidas con compensación en Laravel.',N'Implementar clase Saga con pasos y compensaciones. Usar Jobs de Laravel para cada paso.',N'Saga (Laravel)',N'Saga',N'["mkdir -p app/Sagas app/Jobs"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18103)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18103,N'Php','2026-01-01T00:00:00Z',N'Separa comandos y consultas en Symfony usando Messenger.',N'Usar Symfony Messenger para Command/Query bus. Handlers anotados con AsMessageHandler.',N'CQRS (Symfony)',N'CQRS',N'["mkdir -p src/Application/Command src/Application/Query src/Application/Handler src/Controller"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18104)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18104,N'Php','2026-01-01T00:00:00Z',N'Reduce el acoplamiento usando Symfony Messenger como mediador.',N'Symfony Messenger actúa de mediador. Mensajes enrutados a handlers mediante bus.',N'Mediator (Symfony)',N'Mediator',N'["mkdir -p src/Application/Message src/Application/Handler src/Controller"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 18105)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (18105,N'Php','2026-01-01T00:00:00Z',N'Orquesta transacciones distribuidas con compensación en Symfony.',N'Implementar clase Saga con pasos async. Usar Messenger para comunicación entre pasos.',N'Saga (Symfony)',N'Saga',N'["mkdir -p src/Application/Saga src/Application/Handler"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries PHP (IDs 11–17, 21–25) ─────────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 11)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (11,N'Php',N'Authentication','2026-01-01T00:00:00Z',N'Autenticación ligera para APIs y SPAs en Laravel',N'Laravel',N'composer require laravel/sanctum',N'Laravel Sanctum',N'laravel/sanctum',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 12)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (12,N'Php',N'Authorization','2026-01-01T00:00:00Z',N'Roles y permisos robustos para Laravel',N'Laravel',N'composer require spatie/laravel-permission',N'Spatie Permission',N'spatie/laravel-permission',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 13)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (13,N'Php',N'Auditing','2026-01-01T00:00:00Z',N'Auditoría y trazabilidad de cambios en modelos',N'Laravel',N'composer require spatie/laravel-activitylog',N'Spatie Activitylog',N'spatie/laravel-activitylog',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 14)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (14,N'Php',N'Queues','2026-01-01T00:00:00Z',N'Panel y supervisión de colas Redis en Laravel',N'Laravel',N'composer require laravel/horizon',N'Laravel Horizon',N'laravel/horizon',91);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 15)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (15,N'Php',N'HTTP Client','2026-01-01T00:00:00Z',N'Cliente HTTP estándar para integraciones y consumo de APIs',NULL,N'composer require guzzlehttp/guzzle',N'Guzzle HTTP',N'guzzlehttp/guzzle',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 16)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (16,N'Php',N'Event Sourcing','2026-01-01T00:00:00Z',N'Event store, projectors y replay para Laravel',N'Laravel',N'composer require spatie/laravel-event-sourcing',N'Laravel Event Sourcing',N'spatie/laravel-event-sourcing',94);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 17)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (17,N'Php',N'Database','2026-01-01T00:00:00Z',N'Driver oficial para usar MongoDB con Laravel',N'Laravel',N'composer require mongodb/laravel-mongodb',N'Laravel MongoDB',N'mongodb/laravel-mongodb',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 21)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (21,N'Php',N'ORM','2026-01-01T00:00:00Z',N'Stack de Doctrine ORM listo para Symfony',N'Symfony',N'composer require symfony/orm-pack',N'Symfony ORM Pack',N'symfony/orm-pack',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 22)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (22,N'Php',N'Validation','2026-01-01T00:00:00Z',N'Validación de objetos y DTOs en Symfony',N'Symfony',N'composer require symfony/validator',N'Symfony Validator',N'symfony/validator',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 23)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (23,N'Php',N'Security','2026-01-01T00:00:00Z',N'Autenticación y autorización para Symfony',N'Symfony',N'composer require symfony/security-bundle',N'Symfony Security Bundle',N'symfony/security-bundle',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 24)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (24,N'Php',N'Queues','2026-01-01T00:00:00Z',N'Mensajería y colas para tareas asíncronas en Symfony',N'Symfony',N'composer require symfony/messenger',N'Symfony Messenger',N'symfony/messenger',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 25)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (25,N'Php',N'Serialization','2026-01-01T00:00:00Z',N'Serialización y normalización de datos en Symfony',N'Symfony',N'composer require symfony/serializer-pack',N'Symfony Serializer',N'symfony/serializer-pack',94);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates PHP (IDs 7–19, ver migraciones ya aplicadas) ───────
        // Todos los IDs 7–19 ya están en la migración 20260629100000_AddJavaPythonPhpSeedData
        // Solo agregamos los que pudieran faltar y los nuevos
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 7)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (7,N'Php',N'FROM php:8.3-cli
WORKDIR /var/www/html
RUN apt-get update && apt-get install -y --no-install-recommends git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev && docker-php-ext-install pdo pdo_mysql pdo_pgsql pdo_sqlite mbstring zip intl bcmath && rm -rf /var/lib/apt/lists/*
COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .
RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi && mkdir -p storage bootstrap/cache database && touch database/database.sqlite && chmod -R 775 storage bootstrap/cache database
EXPOSE 8080
CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile genérico para apps PHP modernas (Laravel y Symfony)',NULL,NULL,1,N'Dockerfile PHP',N'dockerfile',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 8)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (8,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: mysql
      DB_HOST: db
      DB_PORT: 3306
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: root
      DB_PASSWORD: secret
    depends_on:
      db:
        condition: service_healthy
  db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: secret
      MYSQL_DATABASE: {{DB_NAME}}
    ports:
      - "{{DB_PORT}}:3306"
    volumes:
      - mysqldata:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para PHP/Laravel o Symfony con MySQL',NULL,N'DockerCompose',1,N'Compose PHP + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 9)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (9,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: pgsql
      DB_HOST: db
      DB_PORT: 5432
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: postgres
      DB_PASSWORD: secret
    depends_on:
      db:
        condition: service_healthy
  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: {{DB_NAME}}
      POSTGRES_PASSWORD: secret
    ports:
      - "{{DB_PORT}}:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD", "pg_isready", "-U", "postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para PHP/Laravel o Symfony con PostgreSQL',NULL,N'DockerCompose',1,N'Compose PHP + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: sqlite
      DB_DATABASE: /var/www/html/database/database.sqlite
    volumes:
      - sqlite-data:/var/www/html/database
volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para PHP/Laravel o Symfony usando SQLite',NULL,N'DockerCompose',1,N'Compose PHP + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 11)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (11,N'Php',N'/vendor/
/node_modules/
/.env
/.env.*
/.phpunit.result.cache
/public/build/
/bootstrap/cache/*.php
/storage/app/*.sqlite
/storage/framework/cache/*
/storage/framework/sessions/*
/storage/framework/testing/*
/storage/framework/views/*
/storage/logs/*
/var/','2026-01-01T00:00:00Z',NULL,N'Gitignore base para proyectos PHP modernos',NULL,NULL,1,N'PHP .gitignore',N'gitignore',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 12)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (12,N'Php',N'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup PHP
        uses: shivammathur/setup-php@v2
        with:
          php-version: ''8.3''
          extensions: mbstring, xml, curl, zip, intl, pdo_sqlite
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress
      - name: Run tests
        run: |
          if [ -f artisan ]; then
            php artisan test
          elif [ -f vendor/bin/phpunit ]; then
            vendor/bin/phpunit
          fi','2026-01-01T00:00:00Z',NULL,N'Pipeline CI/CD para PHP con Composer y GitHub Actions',NULL,NULL,1,N'CI PHP GitHub Actions',N'ci',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 13)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (13,N'Php',N'apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{APP_NAME}}
spec:
  replicas: 2
  selector:
    matchLabels:
      app: {{APP_NAME}}
  template:
    metadata:
      labels:
        app: {{APP_NAME}}
    spec:
      containers:
        - name: {{APP_NAME}}
          image: {{APP_NAME}}:latest
          ports:
            - containerPort: 8080
---
apiVersion: v1
kind: Service
metadata:
  name: {{APP_NAME}}-svc
spec:
  selector:
    app: {{APP_NAME}}
  ports:
    - port: 80
      targetPort: 8080
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para apps PHP',NULL,N'Kubernetes',1,N'K8s PHP Deployment',N'k8s-deployment',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 15)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (15,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: mongodb
      DB_HOST: mongo
      DB_PORT: 27017
      DB_DATABASE: {{DB_NAME}}
      MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para PHP/Laravel o Symfony con MongoDB',NULL,N'DockerCompose',1,N'Compose PHP + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 17)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (17,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      CACHE_DRIVER: redis
      QUEUE_CONNECTION: redis
      SESSION_DRIVER: redis
      REDIS_HOST: redis
      REDIS_PORT: 6379
    depends_on:
      - redis
  redis:
    image: redis:7-alpine
    ports:
      - "{{DB_PORT}}:6379"
    volumes:
      - redis-data:/data
volumes:
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para PHP/Laravel o Symfony con Redis',NULL,N'DockerCompose',1,N'Compose PHP + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 19)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (19,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: sqlserver
      DB_PORT: 1433
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
    depends_on:
      sqlserver:
        condition: service_started
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      MSSQL_PID: Developer
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd
    ports:
      - "{{DB_PORT}}:1433"
    volumes:
      - mssql-data:/var/opt/mssql
volumes:
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para PHP/Laravel o Symfony con SQL Server',NULL,N'DockerCompose',1,N'Compose PHP + SQL Server',N'compose',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (7,8,9,10,11,12,13,14,15,16,17,18,18100,18101,18102,18103,18104,18105);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (11,12,13,14,15,16,17,21,22,23,24,25);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (7,8,9,10,11,12,13,15,17,19);");
    }
}
