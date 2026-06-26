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
                Description = "Aisla Eloquent o Doctrine detras de contratos, util para pruebas y desacoplamiento.",
                ImplementationNotes = "Crear app/Contracts, app/Repositories, app/Models y una migracion; registrar bindings en un service provider.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Contracts app/Repositories app/Models app/Providers database/migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 8,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.CleanArchitecture,
                Name = "Clean Architecture",
                Architecture = ArchitectureType.Php,
                Description = "Organiza Laravel o Symfony en capas para mantener la logica de negocio fuera del framework.",
                ImplementationNotes = "Separar app/Domain, app/Application y app/Infrastructure; agregar contratos, modelo y provider para el binding.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain app/Application app/Infrastructure app/Contracts app/Models app/Providers database/migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 9,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.HexagonalArchitecture,
                Name = "Hexagonal Architecture",
                Architecture = ArchitectureType.Php,
                Description = "Usa puertos y adaptadores para que Laravel o Symfony actuen solo como capa de entrada y salida.",
                ImplementationNotes = "Modelar puertos en app/Ports, casos de uso en app/Application, entidad en Domain y adaptadores Eloquent en app/Adapters; registrar bindings en un provider de Laravel.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Ports app/Adapters/Persistence app/Domain/Entities app/Application/UseCases app/Models app/Providers database/migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 10,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.DomainDrivenDesign,
                Name = "Domain-Driven Design",
                Architecture = ArchitectureType.Php,
                Description = "Estructura el codigo alrededor del dominio, no de los controladores o de la capa HTTP.",
                ImplementationNotes = "Crear entidades, value objects y servicios de aplicacion por dominio; agrupar por bounded context.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Domain/Entities app/Domain/ValueObjects app/Application\"]"
            },
            new DesignPatternEntry
            {
                Id = 11,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.EventSourcing,
                Name = "Event Sourcing",
                Architecture = ArchitectureType.Php,
                Description = "Registra cambios como eventos para auditoria y reconstruccion del estado.",
                ImplementationNotes = "Separar eventos, listeners y jobs; considerar una libreria de event sourcing si el dominio lo requiere.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Events app/Listeners app/Jobs\"]"
            },
            new DesignPatternEntry
            {
                Id = 12,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Microservices,
                Name = "Microservices",
                Architecture = ArchitectureType.Php,
                Description = "Divide el sistema en servicios pequenos solo cuando hay limites de dominio y despliegue claros.",
                ImplementationNotes = "Separar integraciones externas, colas y contratos; evitar sobrecargar un monolito sin necesidad.",
                ScaffoldCommandsJson = "[\"mkdir -p app/Services app/Jobs app/Integrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 13,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Repository,
                Name = "Repository Pattern (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Separa la persistencia del dominio usando repositorios de Symfony con Doctrine o DBAL.",
                ImplementationNotes = "Crear src/Entity, src/Contract, src/Application/UseCase y src/Repository; registrar el binding en config/services.yaml y agregar la migracion.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Entity src/Contract src/Application/UseCase src/Repository migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 14,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.CleanArchitecture,
                Name = "Clean Architecture (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Organiza Symfony en capas para mantener la logica de negocio fuera de los controladores y bundles.",
                ImplementationNotes = "Separar src/Domain, src/Application, src/Entity y src/Infrastructure; registrar el contrato en config/services.yaml y crear la migracion.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Entity src/Contract src/Application/UseCase src/Entity src/Infrastructure/Persistence migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 15,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.HexagonalArchitecture,
                Name = "Hexagonal Architecture (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Usa puertos y adaptadores para aislar Symfony del dominio.",
                ImplementationNotes = "Modelar puertos en src/Port, casos de uso en src/Application, entidad Doctrine en src/Entity y adaptadores en src/Adapters; registrar el binding en config/services.yaml y crear la migracion.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Port src/Application/UseCase src/Domain/Entity src/Entity src/Adapters/Persistence migrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 16,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.DomainDrivenDesign,
                Name = "Domain-Driven Design (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Estructura el codigo alrededor del dominio y no alrededor del framework.",
                ImplementationNotes = "Crear src/Domain/Entity, src/Domain/ValueObject y src/Application por bounded context.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Domain/Entity src/Domain/ValueObject src/Application\"]"
            },
            new DesignPatternEntry
            {
                Id = 17,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.EventSourcing,
                Name = "Event Sourcing (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Conserva el historial de cambios como eventos para auditoria y reconstruccion del estado.",
                ImplementationNotes = "Crear src/Event, src/EventListener y src/MessageHandler; valorar Messenger para asincronia.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Event src/EventListener src/MessageHandler\"]"
            },
            new DesignPatternEntry
            {
                Id = 18,
                CreatedAt = SeedDate,
                Pattern = DesignPattern.Microservices,
                Name = "Microservices (Symfony)",
                Architecture = ArchitectureType.Php,
                Description = "Divide la aplicacion solo cuando existan limites claros entre servicios.",
                ImplementationNotes = "Separar clientes HTTP, mensajes y contratos de integracion; usar Messenger o colas para desacoplar.",
                ScaffoldCommandsJson = "[\"mkdir -p src/Service src/MessageHandler src/Integration\"]"
            }
        };
}
