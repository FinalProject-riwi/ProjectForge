using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

public partial class AddPhpSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;
            INSERT INTO [DesignPatterns] ([Id], [Architecture], [CreatedAt], [Description], [ImplementationNotes], [LibraryRecommendationId], [Name], [Pattern], [ScaffoldCommandsJson], [UpdatedAt])
            VALUES
            (7, N'Php', '2026-01-01T00:00:00.0000000Z', N'Aisla Eloquent o Doctrine detras de contratos, util para pruebas y desacoplamiento.', N'Crear app/Contracts y app/Repositories; registrar bindings en un service provider.', NULL, N'Repository Pattern', N'Repository', N'["mkdir -p app/Contracts app/Repositories"]', NULL),
            (8, N'Php', '2026-01-01T00:00:00.0000000Z', N'Organiza Laravel o Symfony en capas para mantener la logica de negocio fuera del framework.', N'Separar app/Domain, app/Application y app/Infrastructure; dejar Http/Console como adaptadores.', NULL, N'Clean Architecture', N'CleanArchitecture', N'["mkdir -p app/Domain app/Application app/Infrastructure app/Contracts"]', NULL),
            (9, N'Php', '2026-01-01T00:00:00.0000000Z', N'Usa puertos y adaptadores para que Laravel o Symfony actuen solo como capa de entrada y salida.', N'Modelar puertos en app/Ports y adaptadores en app/Adapters; inyectar dependencias via container.', NULL, N'Hexagonal Architecture', N'HexagonalArchitecture', N'["mkdir -p app/Ports app/Adapters app/Domain"]', NULL),
            (10, N'Php', '2026-01-01T00:00:00.0000000Z', N'Estructura el codigo alrededor del dominio, no de los controladores o de la capa HTTP.', N'Crear entidades, value objects y servicios de aplicacion por dominio; agrupar por bounded context.', NULL, N'Domain-Driven Design', N'DomainDrivenDesign', N'["mkdir -p app/Domain/Entities app/Domain/ValueObjects app/Application"]', NULL),
            (11, N'Php', '2026-01-01T00:00:00.0000000Z', N'Registra cambios como eventos para auditoria y reconstruccion del estado.', N'Separar eventos, listeners y jobs; considerar una libreria de event sourcing si el dominio lo requiere.', NULL, N'Event Sourcing', N'EventSourcing', N'["mkdir -p app/Events app/Listeners app/Jobs"]', NULL),
            (12, N'Php', '2026-01-01T00:00:00.0000000Z', N'Divide el sistema en servicios pequenos solo cuando hay limites de dominio y despliegue claros.', N'Separar integraciones externas, colas y contratos; evitar sobrecargar un monolito sin necesidad.', NULL, N'Microservices', N'Microservices', N'["mkdir -p app/Services app/Jobs app/Integrations"]', NULL),
            (13, N'Php', '2026-01-01T00:00:00.0000000Z', N'Separa la persistencia del dominio usando repositorios de Symfony con Doctrine o DBAL.', N'Crear src/Contract y src/Repository; registrar servicios en services.yaml o con autowiring.', NULL, N'Repository Pattern (Symfony)', N'Repository', N'["mkdir -p src/Contract src/Repository src/Service"]', NULL),
            (14, N'Php', '2026-01-01T00:00:00.0000000Z', N'Organiza Symfony en capas para mantener la logica de negocio fuera de los controladores y bundles.', N'Separar src/Domain, src/Application y src/Infrastructure; dejar Controller y Command como adaptadores.', NULL, N'Clean Architecture (Symfony)', N'CleanArchitecture', N'["mkdir -p src/Domain src/Application src/Infrastructure src/Controller"]', NULL),
            (15, N'Php', '2026-01-01T00:00:00.0000000Z', N'Usa puertos y adaptadores para aislar Symfony del dominio.', N'Modelar puertos en src/Port y adaptadores en src/Adapter; inyectar dependencias via container.', NULL, N'Hexagonal Architecture (Symfony)', N'HexagonalArchitecture', N'["mkdir -p src/Port src/Adapter src/Domain"]', NULL),
            (16, N'Php', '2026-01-01T00:00:00.0000000Z', N'Estructura el codigo alrededor del dominio y no alrededor del framework.', N'Crear src/Domain/Entity, src/Domain/ValueObject y src/Application por bounded context.', NULL, N'Domain-Driven Design (Symfony)', N'DomainDrivenDesign', N'["mkdir -p src/Domain/Entity src/Domain/ValueObject src/Application"]', NULL),
            (17, N'Php', '2026-01-01T00:00:00.0000000Z', N'Conserva el historial de cambios como eventos para auditoria y reconstruccion del estado.', N'Crear src/Event, src/EventListener y src/MessageHandler; valorar Messenger para asincronia.', NULL, N'Event Sourcing (Symfony)', N'EventSourcing', N'["mkdir -p src/Event src/EventListener src/MessageHandler"]', NULL),
            (18, N'Php', '2026-01-01T00:00:00.0000000Z', N'Divide la aplicacion solo cuando existan limites claros entre servicios.', N'Separar clientes HTTP, mensajes y contratos de integracion; usar Messenger o colas para desacoplar.', NULL, N'Microservices (Symfony)', N'Microservices', N'["mkdir -p src/Service src/MessageHandler src/Integration"]', NULL);
            SET IDENTITY_INSERT [DesignPatterns] OFF;

            SET IDENTITY_INSERT [Libraries] ON;
            INSERT INTO [Libraries] ([Id], [Architecture], [Category], [CreatedAt], [Description], [Framework], [InstallCommand], [Name], [PackageName], [PopularityScore], [UpdatedAt], [Version])
            VALUES
            (11, N'Php', N'Authentication', '2026-01-01T00:00:00.0000000Z', N'Autenticacion ligera para APIs y SPAs en Laravel', N'Laravel', N'composer require laravel/sanctum', N'Laravel Sanctum', N'laravel/sanctum', 96, NULL, NULL),
            (12, N'Php', N'Authorization', '2026-01-01T00:00:00.0000000Z', N'Roles y permisos robustos para Laravel', N'Laravel', N'composer require spatie/laravel-permission', N'Spatie Permission', N'spatie/laravel-permission', 95, NULL, NULL),
            (13, N'Php', N'Auditing', '2026-01-01T00:00:00.0000000Z', N'Auditoria y trazabilidad de cambios en modelos', N'Laravel', N'composer require spatie/laravel-activitylog', N'Spatie Activitylog', N'spatie/laravel-activitylog', 92, NULL, NULL),
            (14, N'Php', N'Queues', '2026-01-01T00:00:00.0000000Z', N'Panel y supervision de colas Redis en Laravel', N'Laravel', N'composer require laravel/horizon', N'Laravel Horizon', N'laravel/horizon', 91, NULL, NULL),
            (15, N'Php', N'HTTP Client', '2026-01-01T00:00:00.0000000Z', N'Cliente HTTP estandar para integraciones y consumo de APIs', NULL, N'composer require guzzlehttp/guzzle', N'Guzzle HTTP', N'guzzlehttp/guzzle', 98, NULL, NULL),
            (21, N'Php', N'ORM', '2026-01-01T00:00:00.0000000Z', N'Stack de Doctrine ORM listo para Symfony', N'Symfony', N'composer require symfony/orm-pack', N'Symfony ORM Pack', N'symfony/orm-pack', 97, NULL, NULL),
            (22, N'Php', N'Validation', '2026-01-01T00:00:00.0000000Z', N'Validacion de objetos y DTOs en Symfony', N'Symfony', N'composer require symfony/validator', N'Symfony Validator', N'symfony/validator', 95, NULL, NULL),
            (23, N'Php', N'Security', '2026-01-01T00:00:00.0000000Z', N'Autenticacion y autorizacion para Symfony', N'Symfony', N'composer require symfony/security-bundle', N'Symfony Security Bundle', N'symfony/security-bundle', 96, NULL, NULL),
            (24, N'Php', N'Queues', '2026-01-01T00:00:00.0000000Z', N'Mensajeria y colas para tareas asincronas en Symfony', N'Symfony', N'composer require symfony/messenger', N'Symfony Messenger', N'symfony/messenger', 93, NULL, NULL),
            (25, N'Php', N'Serialization', '2026-01-01T00:00:00.0000000Z', N'Serializacion y normalizacion de datos en Symfony', N'Symfony', N'composer require symfony/serializer-pack', N'Symfony Serializer', N'symfony/serializer-pack', 94, NULL, NULL);
            SET IDENTITY_INSERT [Libraries] OFF;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM [DesignPatterns] WHERE [Id] BETWEEN 7 AND 18;
            DELETE FROM [Libraries] WHERE [Id] IN (11,12,13,14,15,21,22,23,24,25);
            """);
    }
}
