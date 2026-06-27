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
            (7, N'Php', '2026-01-01T00:00:00.0000000Z', N'Aisla la persistencia detras de un contrato y una implementacion Eloquent en Laravel.', N'Crear app/Repositories/Contracts, app/Repositories, app/Http/Controllers, app/Models y app/Providers; registrar el binding en un provider.', NULL, N'Repository Pattern', N'Repository', N'["mkdir -p app/Repositories/Contracts app/Repositories app/Http/Controllers app/Models app/Providers database/migrations routes"]', NULL),
            (8, N'Php', '2026-01-01T00:00:00.0000000Z', N'Organiza Laravel o Symfony en capas para mantener la logica de negocio fuera del framework.', N'Separar app/Domain, app/Application y app/Infrastructure; dejar Http/Console como adaptadores.', NULL, N'Clean Architecture', N'CleanArchitecture', N'["mkdir -p app/Domain app/Application app/Infrastructure app/Contracts"]', NULL),
            (9, N'Php', '2026-01-01T00:00:00.0000000Z', N'Usa puertos y adaptadores para que Laravel o Symfony actuen solo como capa de entrada y salida.', N'Modelar puertos de entrada en app/Application/Ports/In y puertos de salida en app/Application/Ports/Out; los controladores consumen los puertos de entrada y los adaptadores Eloquent implementan los de salida.', NULL, N'Hexagonal Architecture', N'HexagonalArchitecture', N'["mkdir -p app/Domain/Entities app/Application/DTOs app/Application/Ports/In app/Application/Ports/Out app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes"]', NULL),
            (10, N'Php', '2026-01-01T00:00:00.0000000Z', N'Estructura el codigo alrededor del dominio, no de los controladores o de la capa HTTP.', N'Crear aggregate roots, value objects, domain services, domain events y repositorios de dominio por bounded context.', NULL, N'Domain-Driven Design', N'DomainDrivenDesign', N'["mkdir -p app/Domain/Aggregates app/Domain/Entities app/Domain/Events app/Domain/Repositories app/Domain/Services app/Domain/ValueObjects app/Application"]', NULL),
            (11, N'Php', '2026-01-01T00:00:00.0000000Z', N'Registra los cambios como eventos y reconstruye el agregado por rehidratacion.', N'Crear stored_events, projectors, read models, event store, use cases y controlador HTTP; el agregado debe soportar replay y fromHistory.', NULL, N'Event Sourcing', N'EventSourcing', N'["mkdir -p app/Domain/Aggregates app/Domain/Events app/Domain/Repositories app/Application/DTOs app/Application/UseCases app/Infrastructure/EventStore app/Infrastructure/Projectors app/Models database/migrations routes"]', NULL),
            (12, N'Php', '2026-01-01T00:00:00.0000000Z', N'Divide el sistema en un gateway y servicios Laravel separados, con despliegue independiente y mensajeria asincrona.', N'Crear un gateway en la raiz, servicios en services/projects y services/notifications, un broker RabbitMQ y bases separadas por servicio.', NULL, N'Microservices', N'Microservices', N'["mkdir -p services/projects services/notifications shared/contracts"]', NULL),
            (13, N'Php', '2026-01-01T00:00:00.0000000Z', N'Separa la persistencia del dominio usando controladores, casos de uso y repositorios Doctrine en Symfony.', N'Crear src/Domain, src/Application/DTOs, src/Application/UseCases, src/Controller y src/Infrastructure/Persistence; registrar los bindings en config/services.yaml y exponer las rutas por atributos.', NULL, N'Repository Pattern (Symfony)', N'Repository', N'["mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]', NULL),
            (14, N'Php', '2026-01-01T00:00:00.0000000Z', N'Organiza Symfony en capas para mantener la logica de negocio fuera de los controladores y de la capa HTTP.', N'Separar src/Domain, src/Application, src/Infrastructure y src/Controller; exponer DTOs, casos de uso y adaptadores de persistencia claramente definidos.', NULL, N'Clean Architecture (Symfony)', N'CleanArchitecture', N'["mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]', NULL),
            (15, N'Php', '2026-01-01T00:00:00.0000000Z', N'Usa puertos y adaptadores para aislar Symfony del dominio.', N'Modelar puertos de entrada en src/Application/Ports/In y puertos de salida en src/Application/Ports/Out; los controladores consumen los puertos de entrada y Doctrine implementa los de salida.', NULL, N'Hexagonal Architecture (Symfony)', N'HexagonalArchitecture', N'["mkdir -p src/Application/Ports/In src/Application/Ports/Out src/Application/DTOs src/Application/UseCases src/Domain/Aggregates src/Domain/Entities src/Domain/Repositories src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]', NULL),
            (16, N'Php', '2026-01-01T00:00:00.0000000Z', N'Estructura el codigo alrededor del dominio y no alrededor del framework.', N'Crear agregados, value objects, eventos, repositorios de dominio, casos de uso y un controlador por bounded context.', NULL, N'Domain-Driven Design (Symfony)', N'DomainDrivenDesign', N'["mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations"]', NULL),
            (17, N'Php', '2026-01-01T00:00:00.0000000Z', N'Conserva el historial de cambios como eventos y reconstruye el agregado al rehidratarlo.', N'Crear stored_events, proyecciones, event store, projectors, casos de uso y controlador HTTP; el agregado debe poder reconstituirse desde el historial.', NULL, N'Event Sourcing (Symfony)', N'EventSourcing', N'["mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/Repositories src/Application/DTOs src/Application/UseCases src/Infrastructure/EventStore src/Infrastructure/Projectors src/Entity config/packages config/routes migrations"]', NULL),
            (18, N'Php', '2026-01-01T00:00:00.0000000Z', N'Divide la aplicacion en un gateway y servicios Symfony separados, con despliegue independiente y mensajeria asincrona.', N'Crear un gateway en la raiz, servicios en services/projects y services/notifications, colas con Redis o RabbitMQ y compose con los servicios de apoyo.', NULL, N'Microservices (Symfony)', N'Microservices', N'["mkdir -p services/projects services/notifications shared/contracts"]', NULL);
            SET IDENTITY_INSERT [DesignPatterns] OFF;

            SET IDENTITY_INSERT [Libraries] ON;
            INSERT INTO [Libraries] ([Id], [Architecture], [Category], [CreatedAt], [Description], [Framework], [InstallCommand], [Name], [PackageName], [PopularityScore], [UpdatedAt], [Version])
            VALUES
            (11, N'Php', N'Authentication', '2026-01-01T00:00:00.0000000Z', N'Autenticacion ligera para APIs y SPAs en Laravel', N'Laravel', N'composer require laravel/sanctum', N'Laravel Sanctum', N'laravel/sanctum', 96, NULL, NULL),
            (12, N'Php', N'Authorization', '2026-01-01T00:00:00.0000000Z', N'Roles y permisos robustos para Laravel', N'Laravel', N'composer require spatie/laravel-permission', N'Spatie Permission', N'spatie/laravel-permission', 95, NULL, NULL),
            (13, N'Php', N'Auditing', '2026-01-01T00:00:00.0000000Z', N'Auditoria y trazabilidad de cambios en modelos', N'Laravel', N'composer require spatie/laravel-activitylog', N'Spatie Activitylog', N'spatie/laravel-activitylog', 92, NULL, NULL),
            (14, N'Php', N'Queues', '2026-01-01T00:00:00.0000000Z', N'Panel y supervision de colas Redis en Laravel', N'Laravel', N'composer require laravel/horizon', N'Laravel Horizon', N'laravel/horizon', 91, NULL, NULL),
            (15, N'Php', N'HTTP Client', '2026-01-01T00:00:00.0000000Z', N'Cliente HTTP estandar para integraciones y consumo de APIs', NULL, N'composer require guzzlehttp/guzzle', N'Guzzle HTTP', N'guzzlehttp/guzzle', 98, NULL, NULL),
            (16, N'Php', N'Event Sourcing', '2026-01-01T00:00:00.0000000Z', N'Event store, projectors y replay para Laravel', N'Laravel', N'composer require spatie/laravel-event-sourcing', N'Laravel Event Sourcing', N'spatie/laravel-event-sourcing', 94, NULL, NULL),
            (17, N'Php', N'Database', '2026-01-01T00:00:00.0000000Z', N'Driver oficial para usar MongoDB con Laravel', N'Laravel', N'composer require mongodb/laravel-mongodb', N'Laravel MongoDB', N'mongodb/laravel-mongodb', 93, NULL, NULL),
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
            DELETE FROM [Libraries] WHERE [Id] IN (11,12,13,14,15,16,17,21,22,23,24,25);
            """);
    }
}
