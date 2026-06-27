using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Adds missing design patterns for all architectures:
/// - DotNet: Saga, MVVM
/// - Java: Mediator, Saga, MVVM
/// - Python: Microservices, Mediator, Saga
/// - TypeScript: Mediator, Saga, MVVM
/// - JavaScript: EventSourcing, DomainDrivenDesign, Saga, MVVM
/// </summary>
public partial class AddMissingPatternsAndFixes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // DotNet: Saga (1004) and MVVM (1005)
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1004)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1004,N'DotNet','2026-01-01 00:00:00',N'Coordina transacciones distribuidas con compensacion ante fallos.',N'Implementar OrchestrationSaga con MediatR. Cada paso compensa los anteriores en caso de error.',N'Saga (.NET)',N'Saga',N'["mkdir -p src/Application/Sagas"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1005)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1005,N'DotNet','2026-01-01 00:00:00',N'Model-View-ViewModel para desacoplar UI de logica (Blazor, WPF, MAUI).',N'ViewModel implementa INotifyPropertyChanged. Usar RelayCommand para comandos de UI.',N'MVVM (.NET)',N'MVVM',N'["mkdir -p src/Presentation/ViewModels src/Presentation/Commands"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        // Java: Mediator (1102), Saga (1103), MVVM (1104)
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1102)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1102,N'Java','2026-01-01 00:00:00',N'Patron Mediator con ApplicationEventPublisher o Axon.',N'Usar ApplicationEventPublisher para desacoplar componentes. Handlers anotados con @EventListener.',N'Mediator (Spring Boot)',N'Mediator',N'["mkdir -p src/main/java/application/mediator"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1103)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1103,N'Java','2026-01-01 00:00:00',N'Orquesta transacciones distribuidas con compensacion.',N'Implementar saga de orquestacion con @Service. Cada paso tiene compensacion en caso de fallo.',N'Saga (Spring Boot)',N'Saga',N'["mkdir -p src/main/java/application/saga"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1104)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1104,N'Java','2026-01-01 00:00:00',N'Model-View-ViewModel para JavaFX o frontend desacoplado.',N'ViewModel expone ObservableValue. Usar Property Binding de JavaFX o patron Observer.',N'MVVM (Spring Boot / JavaFX)',N'MVVM',N'["mkdir -p src/main/java/presentation/viewmodel src/main/java/presentation/view"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        // Python: Microservices (1203), Mediator (1204), Saga (1205)
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1203)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1203,N'Python','2026-01-01 00:00:00',N'Servicios independientes con FastAPI comunicandose por HTTP.',N'Cada servicio tiene su propio main.py y requirements.txt. Usar httpx para comunicacion entre servicios.',N'Microservices (Python/FastAPI)',N'Microservices',N'["mkdir -p services/items services/notifications services/gateway"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1204)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1204,N'Python','2026-01-01 00:00:00',N'Mediador que desacopla handlers de mensajes en Python.',N'Clase Mediator que registra handlers por tipo. Compatible con FastAPI DI.',N'Mediator (Python)',N'Mediator',N'["mkdir -p app/application"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1205)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1205,N'Python','2026-01-01 00:00:00',N'Orquesta transacciones distribuidas con compensacion.',N'Clase OrderSaga con pasos async. Cada paso tiene compensacion en caso de fallo.',N'Saga (Python/FastAPI)',N'Saga',N'["mkdir -p app/application/sagas"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        // TypeScript: Mediator (507), Saga (508), MVVM (509)
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 507)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (507,N'TypeScript','2026-01-01 00:00:00',N'Mediator con @nestjs/cqrs CommandBus y QueryBus.',N'Usar CommandBus y QueryBus de @nestjs/cqrs. Registrar handlers en modulo.',N'Mediator (TypeScript/NestJS)',N'Mediator',N'["mkdir -p src/application/commands src/application/queries src/application/handlers"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 508)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (508,N'TypeScript','2026-01-01 00:00:00',N'Sagas reactivas con @nestjs/cqrs que orquestan flujos de eventos.',N'Usar @Saga() decorator con RxJS. Escucha eventos y despacha comandos compensatorios.',N'Saga (NestJS/TypeScript)',N'Saga',N'["mkdir -p src/application/sagas"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 509)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (509,N'TypeScript','2026-01-01 00:00:00',N'Separacion ViewModel-View con hooks o servicios de estado.',N'En React: custom hook como ViewModel. En Angular: servicio observable como ViewModel.',N'MVVM (TypeScript/React o Angular)',N'MVVM',N'["mkdir -p src/presentation/view-models src/presentation/views"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        // JavaScript: EventSourcing (26), DDD (27), Saga (28), MVVM (29)
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 26)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (26,N'JavaScript','2026-01-01 00:00:00',N'Estado derivado de una secuencia de eventos inmutables en Node.js.',N'DomainEvent base class, InMemoryEventStore, BaseAggregate con apply/pullEvents.',N'Event Sourcing (JavaScript)',N'EventSourcing',N'["mkdir -p src/domain/aggregates src/domain/events src/infrastructure"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 27)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (27,N'JavaScript','2026-01-01 00:00:00',N'Aggregates, Entities y Value Objects para Node.js.',N'BaseAggregate con domain events, ValueObject inmutable, DomainEvent con id y occurredAt.',N'Domain-Driven Design (JavaScript)',N'DomainDrivenDesign',N'["mkdir -p src/domain/aggregates src/domain/events src/domain/value-objects"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 28)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (28,N'JavaScript','2026-01-01 00:00:00',N'Orquesta flujos de trabajo con compensacion en Node.js.',N'OrderSaga con pasos async y compensacion. EventBus simple para subscripcion a eventos.',N'Saga (JavaScript)',N'Saga',N'["mkdir -p src/application/sagas"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 29)
            BEGIN
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (29,N'JavaScript','2026-01-01 00:00:00',N'Separacion de UI y logica con custom hooks como ViewModel.',N'Custom hook como ViewModel que expone estado y acciones. Componente View solo renderiza.',N'MVVM (JavaScript/React)',N'MVVM',N'["mkdir -p src/presentation/view-models src/presentation/views"]');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData("DesignPatterns", "Id", new[] { 1004, 1005, 1102, 1103, 1104, 1203, 1204, 1205, 507, 508, 509, 26, 27, 28, 29 });
    }
}
