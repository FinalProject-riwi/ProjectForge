using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders.Php;

public static partial class PhpSeeder
{
    private static IEnumerable<DesignPatternEntry> GetDesignPatterns() =>
        new[]
        {
            new DesignPatternEntry
            {
                Id = 7,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Repository,
                Name = "Repository Pattern",
                Architecture = ArchitectureType.Php,
                Description = "Aisla la persistencia detras de un contrato y una implementacion Eloquent en Laravel.",
                ImplementationNotes = "Crear app/Repositories/Contracts, app/Repositories, app/Http/Controllers, app/Models y app/Providers; registrar el binding en un provider.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Repositories/Contracts app/Repositories app/Http/Controllers app/Models app/Providers database/migrations routes\"]"
            },
            new DesignPatternEntry
            {
                Id = 8,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.CleanArchitecture,
                Name = "Clean Architecture",
                Architecture = ArchitectureType.Php,
                Description = "Organiza Laravel o Symfony en capas para mantener la logica de negocio fuera del framework.",
                ImplementationNotes = "Separar app/Domain, app/Application, app/Http/Controllers y app/Infrastructure; exponer un comando o DTO de entrada y registrar el binding en un provider.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain app/Application/DTOs app/Application/Interfaces app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes\"]"
            },
            new DesignPatternEntry
            {
                Id = 9,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.HexagonalArchitecture,
                Name = "Hexagonal Architecture",
                Architecture = ArchitectureType.Php,
                Description = "Usa puertos y adaptadores para que Laravel o Symfony actuen solo como capa de entrada y salida.",
                ImplementationNotes = "Modelar puertos de entrada en app/Application/Ports/In y puertos de salida en app/Application/Ports/Out; los controladores consumen los puertos de entrada y los adaptadores Eloquent implementan los de salida.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain/Entities app/Application/DTOs app/Application/Ports/In app/Application/Ports/Out app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes\"]"
            },
            new DesignPatternEntry
            {
                Id = 10,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.DomainDrivenDesign,
                Name = "Domain-Driven Design",
                Architecture = ArchitectureType.Php,
                Description = "Estructura el codigo alrededor del dominio, no de los controladores o de la capa HTTP.",
                ImplementationNotes = "Crear aggregate roots, value objects, domain services, domain events, repositorios de dominio, casos de uso, controller HTTP y provider por bounded context.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain/Aggregates app/Domain/Entities app/Domain/Events app/Domain/Repositories app/Domain/Services app/Domain/ValueObjects app/Application/DTOs app/Application/UseCases app/Infrastructure/Persistence app/Http/Controllers app/Models app/Providers database/migrations routes\"]"
            },
            new DesignPatternEntry
            {
                Id = 11,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.EventSourcing,
                Name = "Event Sourcing",
                Architecture = ArchitectureType.Php,
                Description = "Registra los cambios como eventos y reconstruye el agregado por rehidratacion.",
                ImplementationNotes = "Crear stored_events, projectors, read models, event store, use cases y controlador HTTP; el agregado debe soportar replay y fromHistory.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain/Aggregates app/Domain/Events app/Domain/Repositories app/Application/DTOs app/Application/UseCases app/Infrastructure/EventStore app/Infrastructure/Projectors app/Models database/migrations routes\"]"
            },
            new DesignPatternEntry
            {
                Id = 12,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Microservices,
                Name = "Microservices",
                Architecture = ArchitectureType.Php,
                Description = "Divide el sistema en un gateway y servicios Laravel separados, con despliegue independiente y mensajeria asincrona.",
                ImplementationNotes = "Crear un gateway en la raiz, servicios en services/projects y services/notifications, un broker RabbitMQ y bases separadas por servicio.",
                ScaffoldCommandsJson = "[\"mkdir -p services/projects services/notifications shared/contracts\"]"
            },
            new DesignPatternEntry
            {
                Id = 13,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Repository,
                Name = "Repository Pattern (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Separa la persistencia del dominio usando controladores, casos de uso y repositorios Doctrine en Symfony.",
                ImplementationNotes = "Crear src/Domain, src/Application/DTOs, src/Application/UseCases, src/Controller y src/Infrastructure/Persistence; registrar los bindings en config/services.yaml y exponer las rutas por atributos.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 14,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.CleanArchitecture,
                Name = "Clean Architecture (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Organiza Symfony en capas para mantener la logica de negocio fuera de los controladores y de la capa HTTP.",
                ImplementationNotes = "Separar src/Domain, src/Application, src/Infrastructure y src/Controller; exponer DTOs, casos de uso y adaptadores de persistencia claramente definidos.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 15,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.HexagonalArchitecture,
                Name = "Hexagonal Architecture (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Usa puertos y adaptadores para aislar Symfony del dominio.",
                ImplementationNotes = "Modelar puertos de entrada en src/Application/Ports/In y puertos de salida en src/Application/Ports/Out; los controladores consumen los puertos de entrada y Doctrine implementa los de salida.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Application/Ports/In src/Application/Ports/Out src/Application/DTOs src/Application/UseCases src/Domain/Aggregates src/Domain/Entities src/Domain/Repositories src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 16,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.DomainDrivenDesign,
                Name = "Domain-Driven Design (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Estructura el codigo alrededor del dominio y no alrededor del framework.",
                ImplementationNotes = "Crear agregados, value objects, eventos, repositorios de dominio, casos de uso y un controlador por bounded context.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 17,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.EventSourcing,
                Name = "Event Sourcing (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Conserva el historial de cambios como eventos y reconstruye el agregado al rehidratarlo.",
                ImplementationNotes = "Crear stored_events, proyecciones, event store, projectors, casos de uso y controlador HTTP; el agregado debe poder reconstituirse desde el historial.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/Repositories src/Application/DTOs src/Application/UseCases src/Infrastructure/EventStore src/Infrastructure/Projectors src/Entity config/packages config/routes migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 18,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Microservices,
                Name = "Microservices (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Divide la aplicacion en un gateway y servicios Symfony separados, con despliegue independiente y mensajeria asincrona.",
                ImplementationNotes = "Crear un gateway en la raiz, servicios en services/projects y services/notifications, colas con Redis o RabbitMQ y compose con los servicios de apoyo.",
                ScaffoldCommandsJson = "[\"mkdir -p services/projects services/notifications shared/contracts\"]"
            }
        };
}
