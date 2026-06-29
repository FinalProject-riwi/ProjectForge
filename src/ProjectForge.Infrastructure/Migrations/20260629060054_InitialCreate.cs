using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSuggestionCaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PatternsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibrariesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSuggestionCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PopularityScore = table.Column<int>(type: "int", nullable: false),
                    InstallCommand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Database = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Infrastructure = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TemplateType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VariablesSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GitHubId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WizardConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrameworkVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Infrastructure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeploymentTarget = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DesignPatternsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibrariesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WizardConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DesignPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImplementationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScaffoldCommandsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LibraryRecommendationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignPatterns_Libraries_LibraryRecommendationId",
                        column: x => x.LibraryRecommendationId,
                        principalTable: "Libraries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WizardConfigId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GeneratedReadme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_WizardConfigs_WizardConfigId",
                        column: x => x.WizardConfigId,
                        principalTable: "WizardConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VpsCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WizardConfigId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PrivateKeyPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpsCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VpsCredentials_WizardConfigs_WizardConfigId",
                        column: x => x.WizardConfigId,
                        principalTable: "WizardConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Step = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    CommandExecuted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitCode = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Abstrae el acceso a datos detrás de interfaces.", "Crear IRepository<T> en Domain, implementar con EF Core en Infrastructure.", null, "Repository Pattern (.NET)", "Repository", "[\"mkdir -p src/Domain/Interfaces src/Infrastructure/Repositories\"]", null },
                    { 2, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas con MediatR.", "Instalar MediatR. Crear Application/Commands y Application/Queries con handlers.", null, "CQRS + MediatR (.NET)", "CQRS", "[\"mkdir -p src/Application/Commands src/Application/Queries src/Application/Handlers\"]", null },
                    { 3, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capas: Domain, Application, Infrastructure, Presentation.", "Domain no referencia nada. Application referencia Domain. Infrastructure implementa interfaces.", null, "Clean Architecture (.NET)", "CleanArchitecture", "[\"mkdir -p src/Domain src/Application src/Infrastructure src/Presentation\"]", null },
                    { 4, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aggregates, Entities, Value Objects y Domain Events.", "Modelar el dominio con entidades ricas. Usar eventos de dominio para comunicación entre aggregates.", null, "Domain-Driven Design (.NET)", "DomainDrivenDesign", "[\"mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/ValueObjects src/Domain/Services\"]", null },
                    { 5, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aisla acceso a datos detrás de repositorios abstractos.", "Clase base AbstractRepository. Implementar con SQLAlchemy en infrastructure.", null, "Repository Pattern (Python/FastAPI)", "Repository", "[\"mkdir -p app/repositories app/domain app/services\"]", null },
                    { 7, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aisla la persistencia detras de un contrato y una implementacion Eloquent en Laravel.", "Crear app/Repositories/Contracts, app/Repositories, app/Http/Controllers, app/Models y app/Providers; registrar el binding en un provider.", null, "Repository Pattern", "Repository", "[\"mkdir -p app/Repositories/Contracts app/Repositories app/Http/Controllers app/Models app/Providers database/migrations routes\"]", null },
                    { 8, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza Laravel o Symfony en capas para mantener la logica de negocio fuera del framework.", "Separar app/Domain, app/Application, app/Http/Controllers y app/Infrastructure; exponer un comando o DTO de entrada y registrar el binding en un provider.", null, "Clean Architecture", "CleanArchitecture", "[\"mkdir -p app/Domain app/Application/DTOs app/Application/Interfaces app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes\"]", null },
                    { 9, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usa puertos y adaptadores para que Laravel o Symfony actuen solo como capa de entrada y salida.", "Modelar puertos de entrada en app/Application/Ports/In y puertos de salida en app/Application/Ports/Out; los controladores consumen los puertos de entrada y los adaptadores Eloquent implementan los de salida.", null, "Hexagonal Architecture", "HexagonalArchitecture", "[\"mkdir -p app/Domain/Entities app/Application/DTOs app/Application/Ports/In app/Application/Ports/Out app/Application/UseCases app/Http/Controllers app/Infrastructure/Persistence app/Models app/Providers database/migrations routes\"]", null },
                    { 10, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estructura el codigo alrededor del dominio, no de los controladores o de la capa HTTP.", "Crear aggregate roots, value objects, domain services, domain events, repositorios de dominio, casos de uso, controller HTTP y provider por bounded context.", null, "Domain-Driven Design", "DomainDrivenDesign", "[\"mkdir -p app/Domain/Aggregates app/Domain/Entities app/Domain/Events app/Domain/Repositories app/Domain/Services app/Domain/ValueObjects app/Application/DTOs app/Application/UseCases app/Infrastructure/Persistence app/Http/Controllers app/Models app/Providers database/migrations routes\"]", null },
                    { 11, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Registra los cambios como eventos y reconstruye el agregado por rehidratacion.", "Crear stored_events, projectors, read models, event store, use cases y controlador HTTP; el agregado debe soportar replay y fromHistory.", null, "Event Sourcing", "EventSourcing", "[\"mkdir -p app/Domain/Aggregates app/Domain/Events app/Domain/Repositories app/Application/DTOs app/Application/UseCases app/Infrastructure/EventStore app/Infrastructure/Projectors app/Models database/migrations routes\"]", null },
                    { 12, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Divide el sistema en un gateway y servicios Laravel separados, con despliegue independiente y mensajeria asincrona.", "Crear un gateway en la raiz, servicios en services/projects y services/notifications, un broker RabbitMQ y bases separadas por servicio.", null, "Microservices", "Microservices", "[\"mkdir -p services/projects services/notifications shared/contracts\"]", null },
                    { 13, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa la persistencia del dominio usando controladores, casos de uso y repositorios Doctrine en Symfony.", "Crear src/Domain, src/Application/DTOs, src/Application/UseCases, src/Controller y src/Infrastructure/Persistence; registrar los bindings en config/services.yaml y exponer las rutas por atributos.", null, "Repository Pattern (Symfony)", "Repository", "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]", null },
                    { 14, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza Symfony en capas para mantener la logica de negocio fuera de los controladores y de la capa HTTP.", "Separar src/Domain, src/Application, src/Infrastructure y src/Controller; exponer DTOs, casos de uso y adaptadores de persistencia claramente definidos.", null, "Clean Architecture (Symfony)", "CleanArchitecture", "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]", null },
                    { 15, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usa puertos y adaptadores para aislar Symfony del dominio.", "Modelar puertos de entrada en src/Application/Ports/In y puertos de salida en src/Application/Ports/Out; los controladores consumen los puertos de entrada y Doctrine implementa los de salida.", null, "Hexagonal Architecture (Symfony)", "HexagonalArchitecture", "[\"mkdir -p src/Application/Ports/In src/Application/Ports/Out src/Application/DTOs src/Application/UseCases src/Domain/Aggregates src/Domain/Entities src/Domain/Repositories src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]", null },
                    { 16, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estructura el codigo alrededor del dominio y no alrededor del framework.", "Crear agregados, value objects, eventos, repositorios de dominio, casos de uso y un controlador por bounded context.", null, "Domain-Driven Design (Symfony)", "DomainDrivenDesign", "[\"mkdir -p src/Domain/Aggregates src/Domain/Entities src/Domain/Events src/Domain/Repositories src/Domain/Services src/Domain/ValueObjects src/Application/DTOs src/Application/UseCases src/Infrastructure/Persistence src/Controller config/packages config/routes migrations\"]", null },
                    { 17, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Conserva el historial de cambios como eventos y reconstruye el agregado al rehidratarlo.", "Crear stored_events, proyecciones, event store, projectors, casos de uso y controlador HTTP; el agregado debe poder reconstituirse desde el historial.", null, "Event Sourcing (Symfony)", "EventSourcing", "[\"mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/Repositories src/Application/DTOs src/Application/UseCases src/Infrastructure/EventStore src/Infrastructure/Projectors src/Entity config/packages config/routes migrations\"]", null },
                    { 18, "Php", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Divide la aplicacion en un gateway y servicios Symfony separados, con despliegue independiente y mensajeria asincrona.", "Crear un gateway en la raiz, servicios en services/projects y services/notifications, colas con Redis o RabbitMQ y compose con los servicios de apoyo.", null, "Microservices (Symfony)", "Microservices", "[\"mkdir -p services/projects services/notifications shared/contracts\"]", null },
                    { 19, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aisla el acceso a datos detras de un contrato y un repositorio concreto para Node.js.", "Crear src/domain, src/application, src/ports y src/infrastructure; registrar el repo en el contenedor de la app.", null, "Repository Pattern (Node.js)", "Repository", "[\"mkdir -p src/domain src/application src/ports src/infrastructure\"]", null },
                    { 20, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza Node.js en capas para mantener la logica de negocio fuera del framework.", "Separar src/domain, src/application, src/interfaces y src/infrastructure; exponer el punto de entrada en src/server.js.", null, "Clean Architecture (Node.js)", "CleanArchitecture", "[\"mkdir -p src/domain src/application src/interfaces src/infrastructure\"]", null },
                    { 21, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usa puertos y adaptadores para aislar el dominio en aplicaciones Node.js.", "Modelar puertos en src/ports y adaptadores en src/adapters; dejar el dominio libre de dependencias externas.", null, "Hexagonal Architecture (Node.js)", "HexagonalArchitecture", "[\"mkdir -p src/domain src/application src/ports src/adapters\"]", null },
                    { 22, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas para simplificar la evolucion del backend Node.js.", "Separar commands, queries y handlers en src/application; usar un bus ligero o funciones puras para coordinarlos.", null, "CQRS (JavaScript)", "CQRS", "[\"mkdir -p src/application/commands src/application/queries src/application/handlers\"]", null },
                    { 23, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Centraliza la orquestacion de mensajes para reducir el acoplamiento entre handlers.", "Crear un mediador liviano en src/application y separar los mensajes en src/application/messages.", null, "Mediator (JavaScript)", "Mediator", "[\"mkdir -p src/application src/application/messages src/application/handlers\"]", null },
                    { 24, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Divide la solucion en servicios Node.js desacoplados cuando existan limites claros de dominio.", "Separar src/services, src/events, src/workers y src/integrations; usar HTTP o colas para desacoplar.", null, "Microservices (JavaScript)", "Microservices", "[\"mkdir -p src/services src/events src/workers src/integrations\"]", null },
                    { 25, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Implementa CQRS con el paquete oficial de NestJS usando CommandBus, QueryBus y EventBus.", "Crear módulos, comandos, consultas, eventos de dominio y handlers; registrar CqrsModule en el modulo raiz.", null, "CQRS (NestJS)", "CQRS", "[\"npm install @nestjs/cqrs\"]", null },
                    { 26, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estado derivado de una secuencia de eventos inmutables en Node.js.", "DomainEvent base class, InMemoryEventStore, BaseAggregate con apply/pullEvents.", null, "Event Sourcing (JavaScript)", "EventSourcing", "[\"mkdir -p src/domain/aggregates src/domain/events src/infrastructure\"]", null },
                    { 27, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aggregates, Entities y Value Objects para Node.js.", "BaseAggregate con domain events, ValueObject inmutable, DomainEvent con id y occurredAt.", null, "Domain-Driven Design (JavaScript)", "DomainDrivenDesign", "[\"mkdir -p src/domain/aggregates src/domain/events src/domain/value-objects\"]", null },
                    { 28, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orquesta flujos de trabajo con compensacion en Node.js.", "OrderSaga con pasos async y compensacion. EventBus simple para subscripcion a eventos.", null, "Saga (JavaScript)", "Saga", "[\"mkdir -p src/application/sagas\"]", null },
                    { 29, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separacion de UI y logica con custom hooks como ViewModel.", "Custom hook como ViewModel que expone estado y acciones. Componente View solo renderiza.", null, "MVVM (JavaScript/React)", "MVVM", "[\"mkdir -p src/presentation/view-models src/presentation/views\"]", null },
                    { 500, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Repositorios tipados con TypeORM o Prisma en NestJS.", "Crear interfaces de repositorio en domain/. Implementar con TypeORM @InjectRepository o PrismaService.", null, "Repository Pattern (NestJS/TypeScript)", "Repository", "[\"mkdir -p src/domain/repositories src/infrastructure/repositories src/domain/entities\"]", null },
                    { 501, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CQRS con @nestjs/cqrs: CommandBus, QueryBus, EventBus.", "Instalar @nestjs/cqrs. Crear commands/, queries/, events/ con handlers.", null, "CQRS (NestJS/TypeScript)", "CQRS", "[\"mkdir -p src/application/commands src/application/queries src/application/events src/application/handlers\"]", null },
                    { 502, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capas: domain, application, infrastructure, presentation.", "Domain puro sin dependencias de NestJS. Inyección de dependencias mediante interfaces.", null, "Clean Architecture (TypeScript)", "CleanArchitecture", "[\"mkdir -p src/domain src/application/use-cases src/infrastructure src/presentation\"]", null },
                    { 503, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports & Adapters: interfaces TypeScript como ports.", "Ports como interfaces en core/. Adapters en infrastructure/. NestJS inyecta los adapters.", null, "Hexagonal Architecture (TypeScript)", "HexagonalArchitecture", "[\"mkdir -p src/core/ports src/core/domain src/infrastructure/adapters src/presentation\"]", null },
                    { 504, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aggregates, Entities y Value Objects con TypeScript.", "Usar clases inmutables para Value Objects. Domain Events con EventEmitter2 de NestJS.", null, "Domain-Driven Design (TypeScript)", "DomainDrivenDesign", "[\"mkdir -p src/domain/aggregates src/domain/value-objects src/domain/events src/domain/services src/application\"]", null },
                    { 505, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estado derivado de eventos con @nestjs/cqrs y EventBus.", "Usar AggregateRoot de @nestjs/cqrs. Guardar eventos en EventStore.", null, "Event Sourcing (NestJS/TypeScript)", "EventSourcing", "[\"mkdir -p src/domain/events src/infrastructure/event-store src/application/sagas\"]", null },
                    { 506, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Microservicios NestJS comunicándose por TCP, Redis o NATS.", "Usar @nestjs/microservices. ClientProxy para comunicación. API Gateway como entry point.", null, "Microservices (NestJS/TypeScript)", "Microservices", "[\"mkdir -p services/gateway services/users services/orders\"]", null },
                    { 507, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mediator con @nestjs/cqrs CommandBus y QueryBus.", "Usar CommandBus y QueryBus de @nestjs/cqrs. Registrar handlers en modulo.", null, "Mediator (TypeScript/NestJS)", "Mediator", "[\"mkdir -p src/application/commands src/application/queries src/application/handlers\"]", null },
                    { 508, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sagas reactivas con @nestjs/cqrs que orquestan flujos de eventos.", "Usar @Saga() decorator con RxJS. Escucha eventos y despacha comandos compensatorios.", null, "Saga (NestJS/TypeScript)", "Saga", "[\"mkdir -p src/application/sagas\"]", null },
                    { 509, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separacion ViewModel-View con hooks o servicios de estado.", "En React: custom hook como ViewModel. En Angular: servicio observable como ViewModel.", null, "MVVM (TypeScript/React o Angular)", "MVVM", "[\"mkdir -p src/presentation/view-models src/presentation/views\"]", null },
                    { 800, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capas: domain, application, infrastructure y adapters.", "Domain no importa FastAPI. DI de FastAPI conecta las capas.", null, "Clean Architecture (Python/FastAPI)", "CleanArchitecture", "[\"mkdir -p app/domain app/application/use_cases app/infrastructure app/adapters/api\"]", null },
                    { 801, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas con handlers.", "Dataclasses para Command/Query. Handlers en application/.", null, "CQRS (Python/FastAPI)", "CQRS", "[\"mkdir -p app/commands app/queries app/handlers\"]", null },
                    { 900, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "JpaRepository para aislar acceso a datos.", "Extender JpaRepository<Entity, Id>. Usar @Repository en implementaciones.", null, "Repository Pattern (Spring Boot)", "Repository", "[\"mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence\"]", null },
                    { 901, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capas: domain, application, infrastructure y adapters.", "Domain sin dependencia de Spring. Usar puertos e interfaces para invertir dependencias.", null, "Clean Architecture (Spring Boot)", "CleanArchitecture", "[\"mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters/web\"]", null },
                    { 902, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas con handlers @Service.", "Paquetes command/ y query/ con handlers. Usar ApplicationEventPublisher para eventos.", null, "CQRS (Spring Boot)", "CQRS", "[\"mkdir -p src/main/java/application/command src/main/java/application/query\"]", null },
                    { 903, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports & Adapters: el negocio no conoce el framework.", "Ports como interfaces Java en domain. Adapters en infrastructure implementan los ports.", null, "Hexagonal Architecture (Spring Boot)", "HexagonalArchitecture", "[\"mkdir -p src/main/java/domain/port src/main/java/application/service src/main/java/infrastructure/adapter\"]", null },
                    { 904, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aggregates, Entities y Value Objects en Java.", "Separar @Entity JPA de Domain Objects. Usar mappers para conversión.", null, "Domain-Driven Design (Spring Boot)", "DomainDrivenDesign", "[\"mkdir -p src/main/java/domain/model src/main/java/domain/service src/main/java/domain/event src/main/java/application\"]", null },
                    { 1000, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports & Adapters: el core no conoce infraestructura.", "Definir ports (interfaces) en Core. Implementar adapters en Infrastructure.", null, "Hexagonal Architecture (.NET)", "HexagonalArchitecture", "[\"mkdir -p src/Core/Ports src/Core/Domain src/Infrastructure/Adapters src/Api\"]", null },
                    { 1001, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Desacopla componentes con un mediador central (MediatR).", "Usar IRequest<T> e IRequestHandler<T>. Registrar en DI con AddMediatR.", null, "Mediator (.NET)", "Mediator", "[\"mkdir -p src/Application/Features\"]", null },
                    { 1002, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estado derivado de secuencia de eventos inmutables.", "Usar EventStore o Marten. Cada cambio se registra como evento.", null, "Event Sourcing (.NET)", "EventSourcing", "[\"mkdir -p src/Domain/Events src/Infrastructure/EventStore src/Application/EventHandlers\"]", null },
                    { 1003, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servicios independientes comunicándose por HTTP o mensajes.", "Usar Ocelot como API Gateway. RabbitMQ o Azure Service Bus para mensajería.", null, "Microservices (.NET)", "Microservices", "[\"mkdir -p services/gateway services/orders services/notifications\"]", null },
                    { 1004, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coordina transacciones distribuidas con compensación ante fallos.", "Implementar OrchestrationSaga con MediatR. Cada paso compensa los anteriores en caso de error.", null, "Saga (.NET)", "Saga", "[\"mkdir -p src/Application/Sagas\"]", null },
                    { 1005, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Model-View-ViewModel para desacoplar UI de lógica (Blazor, WPF, MAUI).", "ViewModel implementa INotifyPropertyChanged. Usar RelayCommand para comandos de UI.", null, "MVVM (.NET)", "MVVM", "[\"mkdir -p src/Presentation/ViewModels src/Presentation/Commands\"]", null },
                    { 1100, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estado derivado de eventos con ApplicationEventPublisher.", "Usar Spring Events o Axon Framework. Guardar eventos en EventStore.", null, "Event Sourcing (Spring Boot)", "EventSourcing", "[\"mkdir -p src/main/java/domain/event src/main/java/application/eventhandler src/main/java/infrastructure/eventstore\"]", null },
                    { 1101, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servicios independientes con Spring Cloud.", "Usar Eureka para discovery, Gateway para routing, Feign para comunicación entre servicios.", null, "Microservices (Spring Boot)", "Microservices", "[\"mkdir -p services/gateway services/discovery services/orders\"]", null },
                    { 1102, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patrón Mediator con ApplicationEventPublisher o Axon.", "Usar ApplicationEventPublisher para desacoplar componentes. Handlers anotados con @EventListener.", null, "Mediator (Spring Boot)", "Mediator", "[\"mkdir -p src/main/java/application/mediator\"]", null },
                    { 1103, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orquesta transacciones distribuidas con compensación.", "Implementar saga de orquestación con @Service. Cada paso compensa los anteriores en caso de fallo.", null, "Saga (Spring Boot)", "Saga", "[\"mkdir -p src/main/java/application/saga\"]", null },
                    { 1104, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Model-View-ViewModel para JavaFX o frontend desacoplado.", "ViewModel expone ObservableValue. Usar Property Binding de JavaFX o patrón Observer.", null, "MVVM (Spring Boot / JavaFX)", "MVVM", "[\"mkdir -p src/main/java/presentation/viewmodel src/main/java/presentation/view\"]", null },
                    { 1200, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports como ABCs de Python, adapters como implementaciones concretas.", "Usar ABC para ports. Inyectar adapters en los servicios de aplicación.", null, "Hexagonal Architecture (Python)", "HexagonalArchitecture", "[\"mkdir -p app/core/ports app/core/domain app/infrastructure/adapters app/api\"]", null },
                    { 1201, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aggregates, Entities y Value Objects con dataclasses.", "Usar @dataclass para Value Objects. Domain events con publish/subscribe.", null, "Domain-Driven Design (Python)", "DomainDrivenDesign", "[\"mkdir -p app/domain/aggregates app/domain/events app/domain/value_objects app/application\"]", null },
                    { 1202, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estado reconstruido desde eventos inmutables.", "Usar EventStoreDB o PostgreSQL como event store. Proyecciones para read models.", null, "Event Sourcing (Python)", "EventSourcing", "[\"mkdir -p app/domain/events app/infrastructure/event_store app/application/projections\"]", null },
                    { 1203, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servicios independientes con FastAPI comunicándose por HTTP.", "Cada servicio tiene su propio main.py y requirements.txt. Usar httpx para comunicación entre servicios.", null, "Microservices (Python/FastAPI)", "Microservices", "[\"mkdir -p services/items services/notifications services/gateway\"]", null },
                    { 1204, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mediador que desacopla handlers de mensajes en Python.", "Clase Mediator que registra handlers por tipo. Compatible con FastAPI DI.", null, "Mediator (Python)", "Mediator", "[\"mkdir -p app/application\"]", null },
                    { 1205, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orquesta transacciones distribuidas con compensación.", "Clase OrderSaga con pasos async. Cada paso tiene compensación en caso de fallo.", null, "Saga (Python/FastAPI)", "Saga", "[\"mkdir -p app/application/sagas\"]", null }
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 1, "DotNet", "CQRS/Mediator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Implementación del patrón Mediator para CQRS", "AspNetCoreWebApi", "dotnet add package MediatR", "MediatR", "MediatR", 95, null, null },
                    { 2, "DotNet", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM oficial de Microsoft para .NET", "AspNetCoreWebApi", "dotnet add package Microsoft.EntityFrameworkCore", "Entity Framework Core", "Microsoft.EntityFrameworkCore", 99, null, null },
                    { 3, "DotNet", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación fluida y expresiva", null, "dotnet add package FluentValidation.AspNetCore", "FluentValidation", "FluentValidation.AspNetCore", 92, null, null },
                    { 4, "DotNet", "Logging", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logging estructurado para .NET", null, "dotnet add package Serilog.AspNetCore", "Serilog", "Serilog.AspNetCore", 97, null, null },
                    { 5, "DotNet", "Mapping", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mapeo automático entre objetos", null, "dotnet add package AutoMapper", "AutoMapper", "AutoMapper", 94, null, null },
                    { 6, "DotNet", "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generación automática de documentación OpenAPI", "AspNetCoreWebApi", "dotnet add package Swashbuckle.AspNetCore", "Swashbuckle (Swagger)", "Swashbuckle.AspNetCore", 98, null, null },
                    { 7, "DotNet", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing unitario para .NET", null, "dotnet add package xunit", "xUnit", "xunit", 96, null, null },
                    { 8, "Python", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM más popular para Python, soporta sync y async", null, "pip install sqlalchemy", "SQLAlchemy", "sqlalchemy", 98, null, null },
                    { 9, "Python", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación de datos con type hints", null, "pip install pydantic", "Pydantic", "pydantic", 97, null, null },
                    { 10, "Python", "Migrations", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Migraciones de base de datos para SQLAlchemy", null, "pip install alembic", "Alembic", "alembic", 90, null, null },
                    { 11, "Php", "Authentication", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticacion ligera para APIs y SPAs en Laravel", "Laravel", "composer require laravel/sanctum", "Laravel Sanctum", "laravel/sanctum", 96, null, null },
                    { 12, "Php", "Authorization", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Roles y permisos robustos para Laravel", "Laravel", "composer require spatie/laravel-permission", "Spatie Permission", "spatie/laravel-permission", 95, null, null },
                    { 13, "Php", "Auditing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auditoria y trazabilidad de cambios en modelos", "Laravel", "composer require spatie/laravel-activitylog", "Spatie Activitylog", "spatie/laravel-activitylog", 92, null, null },
                    { 14, "Php", "Queues", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Panel y supervision de colas Redis en Laravel", "Laravel", "composer require laravel/horizon", "Laravel Horizon", "laravel/horizon", 91, null, null },
                    { 15, "Php", "HTTP Client", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente HTTP estandar para integraciones y consumo de APIs", null, "composer require guzzlehttp/guzzle", "Guzzle HTTP", "guzzlehttp/guzzle", 98, null, null },
                    { 16, "Php", "Event Sourcing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Event store, projectors y replay para Laravel", "Laravel", "composer require spatie/laravel-event-sourcing", "Laravel Event Sourcing", "spatie/laravel-event-sourcing", 94, null, null },
                    { 17, "Php", "Database", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Driver oficial para usar MongoDB con Laravel", "Laravel", "composer require mongodb/laravel-mongodb", "Laravel MongoDB", "mongodb/laravel-mongodb", 93, null, null },
                    { 21, "Php", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stack de Doctrine ORM listo para Symfony", "Symfony", "composer require symfony/orm-pack", "Symfony ORM Pack", "symfony/orm-pack", 97, null, null },
                    { 22, "Php", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validacion de objetos y DTOs en Symfony", "Symfony", "composer require symfony/validator", "Symfony Validator", "symfony/validator", 95, null, null },
                    { 23, "Php", "Security", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticacion y autorizacion para Symfony", "Symfony", "composer require symfony/security-bundle", "Symfony Security Bundle", "symfony/security-bundle", 96, null, null },
                    { 24, "Php", "Queues", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mensajeria y colas para tareas asincronas en Symfony", "Symfony", "composer require symfony/messenger", "Symfony Messenger", "symfony/messenger", 93, null, null },
                    { 25, "Php", "Serialization", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Serializacion y normalizacion de datos en Symfony", "Symfony", "composer require symfony/serializer-pack", "Symfony Serializer", "symfony/serializer-pack", 94, null, null },
                    { 26, "JavaScript", "Configuration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carga variables de entorno desde archivos .env", null, "npm install dotenv", "dotenv", "dotenv", 99, null, null },
                    { 27, "JavaScript", "HTTP Framework", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework minimalista y flexible para APIs Node.js", "ExpressJs", "npm install express", "Express", "express", 98, null, null },
                    { 28, "JavaScript", "Security", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cabeceras HTTP seguras para aplicaciones Node.js", null, "npm install helmet", "Helmet", "helmet", 94, null, null },
                    { 29, "JavaScript", "Logging", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logger HTTP simple para Express", "ExpressJs", "npm install morgan", "Morgan", "morgan", 90, null, null },
                    { 30, "JavaScript", "Configuration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gestion de configuracion por entorno para NestJS", "NestJs", "npm install @nestjs/config", "@nestjs/config", "@nestjs/config", 96, null, null },
                    { 31, "JavaScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validacion declarativa basada en decoradores", "NestJs", "npm install class-validator", "class-validator", "class-validator", 95, null, null },
                    { 32, "JavaScript", "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generacion de OpenAPI y Swagger para NestJS", "NestJs", "npm install @nestjs/swagger swagger-ui-express", "@nestjs/swagger", "@nestjs/swagger", 93, null, null },
                    { 33, "JavaScript", "Authentication", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticacion lista para Next.js", "NextJs", "npm install next-auth", "NextAuth", "next-auth", 97, null, null },
                    { 34, "JavaScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Esquemas de validacion y parseo para Node y Next", null, "npm install zod", "Zod", "zod", 98, null, null },
                    { 35, "JavaScript", "Data Fetching", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cache y sincronizacion de estado servidor para Next.js", "NextJs", "npm install @tanstack/react-query", "React Query", "@tanstack/react-query", 92, null, null },
                    { 36, "JavaScript", "CQRS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CommandBus, QueryBus y EventBus oficiales para NestJS", "NestJs", "npm install @nestjs/cqrs", "@nestjs/cqrs", "@nestjs/cqrs", 97, null, null },
                    { 500, "TypeScript", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM para TypeScript con decoradores", null, "npm install typeorm @nestjs/typeorm reflect-metadata", "TypeORM", "typeorm", 91, null, null },
                    { 501, "TypeScript", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM type-safe de última generación para TypeScript", null, "npm install prisma @prisma/client", "Prisma", "prisma", 97, null, null },
                    { 502, "TypeScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decoradores de validación para clases TypeScript", null, "npm install class-validator class-transformer", "class-validator", "class-validator", 94, null, null },
                    { 503, "TypeScript", "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Integración OpenAPI/Swagger para NestJS", "NestTs", "npm install @nestjs/swagger swagger-ui-express", "@nestjs/swagger", "@nestjs/swagger", 93, null, null },
                    { 504, "TypeScript", "Autenticación", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Módulo JWT para NestJS", "NestTs", "npm install @nestjs/jwt @nestjs/passport passport passport-jwt", "@nestjs/jwt", "@nestjs/jwt", 92, null, null },
                    { 505, "TypeScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación de esquemas TypeScript con inferencia de tipos", null, "npm install zod", "zod", "zod", 98, null, null },
                    { 506, "TypeScript", "Cache", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente Redis robusto para Node.js/TypeScript", null, "npm install ioredis", "ioredis", "ioredis", 91, null, null },
                    { 507, "TypeScript", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Testing para TypeScript con cobertura", null, "npm install --save-dev jest ts-jest @types/jest", "jest + ts-jest", "jest", 99, null, null },
                    { 508, "TypeScript", "Background Jobs", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cola de trabajos en background basada en Redis", null, "npm install bullmq @nestjs/bull", "BullMQ", "bullmq", 89, null, null },
                    { 800, "Python", "HTTP Client", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente HTTP moderno con soporte async", null, "pip install httpx", "httpx", "httpx", 89, null, null },
                    { 801, "Python", "Background Tasks", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cola de tareas distribuidas, ideal con Redis", null, "pip install celery", "Celery", "celery", 93, null, null },
                    { 802, "Python", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing más popular de Python", null, "pip install pytest pytest-asyncio", "pytest", "pytest", 99, null, null },
                    { 900, "Java", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM de Spring basado en JPA/Hibernate", null, "<!-- pom.xml -->\n<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-jpa</artifactId>\n</dependency>", "Spring Data JPA", "spring-boot-starter-data-jpa", 99, null, null },
                    { 901, "Java", "Seguridad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticación y autorización para Spring", null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-security</artifactId>\n</dependency>", "Spring Security", "spring-boot-starter-security", 98, null, null },
                    { 902, "Java", "Productividad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elimina boilerplate con anotaciones", null, "<dependency>\n  <groupId>org.projectlombok</groupId>\n  <artifactId>lombok</artifactId>\n  <optional>true</optional>\n</dependency>", "Lombok", "lombok", 95, null, null },
                    { 903, "Java", "Mapeo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mapeo de objetos en tiempo de compilación", null, "<dependency>\n  <groupId>org.mapstruct</groupId>\n  <artifactId>mapstruct</artifactId>\n  <version>1.5.5.Final</version>\n</dependency>", "MapStruct", "mapstruct", 88, null, null },
                    { 904, "Java", "Migraciones", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Migraciones de base de datos para Java", null, "<dependency>\n  <groupId>org.flywaydb</groupId>\n  <artifactId>flyway-core</artifactId>\n</dependency>", "Flyway", "flyway-core", 92, null, null },
                    { 905, "Java", "Documentación", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Documentación Swagger automática para Spring Boot", null, "<dependency>\n  <groupId>org.springdoc</groupId>\n  <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>\n  <version>2.5.0</version>\n</dependency>", "SpringDoc OpenAPI", "springdoc-openapi-starter-webmvc-ui", 90, null, null },
                    { 1000, "DotNet", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Micro ORM rápido y ligero para .NET", null, "dotnet add package Dapper", "Dapper", "Dapper", 91, null, null },
                    { 1001, "DotNet", "Database", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Provider de PostgreSQL para EF Core", null, "dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL", "Npgsql EF Core", "Npgsql.EntityFrameworkCore.PostgreSQL", 93, null, null },
                    { 1002, "DotNet", "Database", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Provider de MySQL para EF Core", null, "dotnet add package Pomelo.EntityFrameworkCore.MySql", "Pomelo MySQL EF Core", "Pomelo.EntityFrameworkCore.MySql", 89, null, null },
                    { 1003, "DotNet", "Routing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Módulos de rutas elegantes para Minimal API", "MinimalApi", "dotnet add package Carter", "Carter", "Carter", 82, null, null },
                    { 1004, "DotNet", "Resilience", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Librería de resiliencia y manejo de fallos transitivos", null, "dotnet add package Polly", "Polly", "Polly", 93, null, null },
                    { 1005, "DotNet", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing alternativo para .NET", null, "dotnet add package NUnit", "NUnit", "NUnit", 88, null, null },
                    { 1006, "DotNet", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generador de datos falsos para tests", null, "dotnet add package Bogus", "Bogus", "Bogus", 86, null, null },
                    { 1007, "DotNet", "Background Jobs", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jobs en background con panel de administración", null, "dotnet add package Hangfire.AspNetCore", "Hangfire", "Hangfire.AspNetCore", 90, null, null },
                    { 1008, "DotNet", "Cache", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente Redis de alto rendimiento para .NET", null, "dotnet add package StackExchange.Redis", "StackExchange.Redis", "StackExchange.Redis", 92, null, null },
                    { 1100, "Java", "Reactive", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Programación reactiva con Spring WebFlux", null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-webflux</artifactId>\n</dependency>", "Spring WebFlux", "spring-boot-starter-webflux", 85, null, null },
                    { 1101, "Java", "Mensajería", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Integración con Apache Kafka", null, "<dependency>\n  <groupId>org.springframework.kafka</groupId>\n  <artifactId>spring-kafka</artifactId>\n</dependency>", "Spring Kafka", "spring-kafka", 87, null, null },
                    { 1102, "Java", "Cache", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Integración con Redis para caching", null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-redis</artifactId>\n</dependency>", "Spring Data Redis", "spring-boot-starter-data-redis", 88, null, null },
                    { 1103, "Java", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing moderno para Java", null, "<dependency>\n  <groupId>org.junit.jupiter</groupId>\n  <artifactId>junit-jupiter</artifactId>\n  <scope>test</scope>\n</dependency>", "JUnit 5", "junit-5", 99, null, null },
                    { 1104, "Java", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de mocking para tests unitarios Java", null, "<dependency>\n  <groupId>org.mockito</groupId>\n  <artifactId>mockito-core</artifactId>\n  <scope>test</scope>\n</dependency>", "Mockito", "mockito-core", 97, null, null },
                    { 1200, "Python", "Framework", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework web moderno y rápido para APIs", "FastAPI", "pip install fastapi uvicorn[standard]", "FastAPI", "fastapi", 99, null, null },
                    { 1201, "Python", "API", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Extensión de Django para crear APIs REST", "Django", "pip install djangorestframework", "Django REST Framework", "djangorestframework", 96, null, null },
                    { 1202, "Python", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Integración de SQLAlchemy con Flask", "Flask", "pip install flask-sqlalchemy", "Flask-SQLAlchemy", "flask-sqlalchemy", 88, null, null },
                    { 1203, "Python", "Cache", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente Redis async para Python", null, "pip install redis[asyncio]", "aioredis", "redis", 87, null, null },
                    { 1204, "Python", "Autenticación", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manejo de tokens JWT en Python", null, "pip install pyjwt", "PyJWT", "pyjwt", 92, null, null },
                    { 1205, "Python", "Config", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carga variables de entorno desde .env", null, "pip install python-dotenv", "Python-dotenv", "python-dotenv", 96, null, null }
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "UpdatedAt", "VariablesSchemaJson", "Version" },
                values: new object[,]
                {
                    { 1, "DotNet", "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\nWORKDIR /app\nEXPOSE 8080\n\nFROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\nWORKDIR /src\nCOPY . .\nRUN dotnet restore\nRUN dotnet publish -c Release -o /app/publish\n\nFROM base AS final\nWORKDIR /app\nCOPY --from=build /app/publish .\nENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Multi-stage Dockerfile para ASP.NET Core", null, null, true, "Dockerfile .NET", "dockerfile", null, null, 1 },
                    { 2, "DotNet", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para .NET + PostgreSQL", null, "DockerCompose", true, "Compose .NET + PostgreSQL", "compose", null, null, 1 },
                    { 3, "DotNet", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para .NET + MySQL", null, "DockerCompose", true, "Compose .NET + MySQL", "compose", null, null, 1 },
                    { 4, "DotNet", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=Secret1234!;\n    depends_on:\n      - db\n  db:\n    image: mcr.microsoft.com/mssql/server:2022-latest\n    environment:\n      ACCEPT_EULA: Y\n      SA_PASSWORD: Secret1234!\n    ports:\n      - \"{{DB_PORT}}:1433\"\n    volumes:\n      - mssqldata:/var/opt/mssql\nvolumes:\n  mssqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SqlServer", "Docker Compose para .NET + SQL Server", null, "DockerCompose", true, "Compose .NET + SQL Server", "compose", null, null, 1 },
                    { 5, "DotNet", "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n        - name: {{APP_NAME}}\n          image: {{APP_NAME}}:latest\n          ports:\n            - containerPort: 8080\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n    - port: 80\n      targetPort: 8080\n  type: LoadBalancer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kubernetes Deployment para .NET", null, "Kubernetes", true, "K8s .NET Deployment", "k8s-deployment", null, null, 1 },
                    { 6, "DotNet", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '10.0.x'\n      - run: dotnet restore\n      - run: dotnet build --no-restore\n      - run: dotnet test --no-build --verbosity normal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI para .NET", null, null, true, "CI .NET GitHub Actions", "ci", null, null, 1 },
                    { 7, "Php", "FROM php:8.3-cli\n\nWORKDIR /var/www/html\n\nRUN apt-get update && apt-get install -y --no-install-recommends \\\n    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev \\\n    && docker-php-ext-install pdo pdo_mysql pdo_pgsql pdo_sqlite mbstring zip intl bcmath \\\n    && rm -rf /var/lib/apt/lists/*\n\nCOPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\nCOPY . .\n\nRUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \\\n    && mkdir -p storage bootstrap/cache database \\\n    && touch database/database.sqlite \\\n    && chmod -R 775 storage bootstrap/cache database\n\nRUN printf '%s\\n' \\\n    '<?php' \\\n    'if (php_sapi_name() === '\"'\"'\\'\"'\"''\"'\"'cli-server'\"'\"'\\'\"'\"''\"'\"') {' \\\n    '    $path = parse_url($_SERVER['\"'\"'\\'\"'\"''\"'\"'REQUEST_URI'\"'\"'\\'\"'\"''\"'\"'], PHP_URL_PATH);' \\\n    '    $file = __DIR__ . '\"'\"'\\'\"'\"''\"'\"'/public'\"'\"'\\'\"'\"''\"'\"' . $path;' \\\n    '    if ($path !== '\"'\"'\\'\"'\"''\"'\"'/'\\'\"'\"''\"'\"' && is_file($file)) {' \\\n    '        return false;' \\\n    '    }' \\\n    '}' \\\n    'require __DIR__ . '\"'\"'\\'\"'\"''\"'\"'/public/index.php'\"'\"'\\'\"'\"''\"'\"';' \\\n    > /usr/local/bin/router.php\n\nEXPOSE 8080\n\nCMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\", \"/usr/local/bin/router.php\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile generico para apps PHP modernas como Laravel y Symfony", null, null, true, "Dockerfile PHP", "dockerfile", null, null, 1 },
                    { 8, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: mysql\n      DB_HOST: db\n      DB_PORT: 3306\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: root\n      DB_PASSWORD: secret\n      DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\", \"mysqladmin\", \"ping\", \"-h\", \"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para PHP/Laravel o Symfony con MySQL", null, "DockerCompose", true, "Compose PHP + MySQL", "compose", null, null, 1 },
                    { 9, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: pgsql\n      DB_HOST: db\n      DB_PORT: 5432\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: postgres\n      DB_PASSWORD: secret\n      DATABASE_URL: pgsql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\", \"pg_isready\", \"-U\", \"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para PHP/Laravel o Symfony con PostgreSQL", null, "DockerCompose", true, "Compose PHP + PostgreSQL", "compose", null, null, 1 },
                    { 10, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: sqlite\n      DB_DATABASE: /var/www/html/database/database.sqlite\n      DATABASE_URL: sqlite:///var/www/html/database/database.sqlite\n    volumes:\n      - sqlite-data:/var/www/html/database\n\nvolumes:\n  sqlite-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SQLite", "Docker Compose para PHP/Laravel o Symfony usando SQLite", null, "DockerCompose", true, "Compose PHP + SQLite", "compose", null, null, 1 },
                    { 11, "Php", "/vendor/\n/node_modules/\n/.env\n/.env.*\n/.phpunit.result.cache\n/public/build/\n/bootstrap/cache/*.php\n/storage/app/*.sqlite\n/storage/framework/cache/*\n/storage/framework/sessions/*\n/storage/framework/testing/*\n/storage/framework/views/*\n/storage/logs/*\n/var/", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gitignore base para proyectos PHP modernos", null, null, true, "PHP .gitignore", "gitignore", null, null, 1 },
                    { 12, "Php", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - name: Setup PHP\n        uses: shivammathur/setup-php@v2\n        with:\n          php-version: '8.3'\n          extensions: mbstring, xml, curl, zip, intl, pdo_sqlite\n          coverage: none\n      - name: Install dependencies\n        run: composer install --no-interaction --prefer-dist --no-progress\n      - name: Run tests\n        run: |\n          if [ -f artisan ]; then\n            php artisan test\n          elif [ -f bin/console ]; then\n            if [ -f vendor/bin/phpunit ]; then\n              vendor/bin/phpunit\n            elif [ -f bin/phpunit ]; then\n              php bin/phpunit\n            fi\n          elif [ -f vendor/bin/phpunit ]; then\n            vendor/bin/phpunit\n          fi", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI/CD para PHP con Composer y GitHub Actions", null, null, true, "CI PHP GitHub Actions", "ci", null, null, 1 },
                    { 13, "Php", "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n        - name: {{APP_NAME}}\n          image: {{APP_NAME}}:latest\n          ports:\n            - containerPort: 8080\n          env:\n            - name: APP_ENV\n              value: production\n            - name: APP_DEBUG\n              value: \"false\"\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n    - port: 80\n      targetPort: 8080\n  type: LoadBalancer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kubernetes Deployment y Service para apps PHP", null, "Kubernetes", true, "K8s PHP Deployment", "k8s-deployment", null, null, 1 },
                    { 14, "Php", "FROM php:8.3-cli\n\nWORKDIR /var/www/html\n\nRUN apt-get update && apt-get install -y --no-install-recommends \\\n    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev libssl-dev pkg-config \\\n    && pecl install mongodb \\\n    && docker-php-ext-enable mongodb \\\n    && docker-php-ext-install pdo mbstring zip intl bcmath \\\n    && rm -rf /var/lib/apt/lists/*\n\nCOPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\nCOPY . .\n\nRUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \\\n    && mkdir -p storage bootstrap/cache \\\n    && chmod -R 775 storage bootstrap/cache\n\nRUN printf '%s\\n' \\\n    '<?php' \\\n    'if (php_sapi_name() === '\"'\"'\"'\"'\"'\"'cli-server'\"'\"'\"'\"'\"'\"') {' \\\n    '    $path = parse_url($_SERVER['\"'\"'\"'\"'\"'\"'REQUEST_URI'\"'\"'\"'\"'\"'\"'], PHP_URL_PATH);' \\\n    '    $file = __DIR__ . '\"'\"'\"'\"'\"'\"'/public'\"'\"'\"'\"'\"'\"' . $path;' \\\n    '    if ($path !== '\"'\"'\"'\"'\"'\"'/'\"'\"'\"'\"'\"'\"' && is_file($file)) {' \\\n    '        return false;' \\\n    '    }' \\\n    '}' \\\n    'require __DIR__ . '\"'\"'\"'\"'\"'\"'/public/index.php'\"'\"'\"'\"'\"'\"';' \\\n    > /usr/local/bin/router.php\n\nEXPOSE 8080\n\nCMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\", \"/usr/local/bin/router.php\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MongoDB", "Dockerfile para PHP/Laravel o Symfony con soporte MongoDB", null, null, true, "Dockerfile PHP + MongoDB", "dockerfile", null, null, 1 },
                    { 15, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: mongodb\n      DB_HOST: mongo\n      DB_PORT: 27017\n      DB_DATABASE: {{DB_NAME}}\n      MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}\n    depends_on:\n      - mongo\n\n  mongo:\n    image: mongo:7\n    ports:\n      - \"{{DB_PORT}}:27017\"\n    volumes:\n      - mongodb-data:/data/db\n\nvolumes:\n  mongodb-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MongoDB", "Docker Compose para PHP/Laravel o Symfony con MongoDB", null, "DockerCompose", true, "Compose PHP + MongoDB", "compose", null, null, 1 },
                    { 16, "Php", "FROM php:8.3-cli\n\nWORKDIR /var/www/html\n\nRUN apt-get update && apt-get install -y --no-install-recommends \\\n    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev libssl-dev pkg-config \\\n    && pecl install redis \\\n    && docker-php-ext-enable redis \\\n    && docker-php-ext-install pdo mbstring zip intl bcmath \\\n    && rm -rf /var/lib/apt/lists/*\n\nCOPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\nCOPY . .\n\nRUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \\\n    && mkdir -p storage bootstrap/cache \\\n    && chmod -R 775 storage bootstrap/cache\n\nRUN printf '%s\\n' \\\n    '<?php' \\\n    'if (php_sapi_name() === '\"'\"'\"'\"'\"'\"'cli-server'\"'\"'\"'\"'\"'\"') {' \\\n    '    $path = parse_url($_SERVER['\"'\"'\"'\"'\"'\"'REQUEST_URI'\"'\"'\"'\"'\"'\"'], PHP_URL_PATH);' \\\n    '    $file = __DIR__ . '\"'\"'\"'\"'\"'\"'/public'\"'\"'\"'\"'\"'\"' . $path;' \\\n    '    if ($path !== '\"'\"'\"'\"'\"'\"'/'\"'\"'\"'\"'\"'\"' && is_file($file)) {' \\\n    '        return false;' \\\n    '    }' \\\n    '}' \\\n    'require __DIR__ . '\"'\"'\"'\"'\"'\"'/public/index.php'\"'\"'\"'\"'\"';' \\\n    > /usr/local/bin/router.php\n\nEXPOSE 8080\n\nCMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\", \"/usr/local/bin/router.php\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Redis", "Dockerfile para PHP/Laravel o Symfony con soporte Redis", null, null, true, "Dockerfile PHP + Redis", "dockerfile", null, null, 1 },
                    { 17, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: redis\n      DB_HOST: redis\n      DB_PORT: 6379\n      DB_DATABASE: {{DB_NAME}}\n      CACHE_STORE: redis\n      CACHE_DRIVER: redis\n      QUEUE_CONNECTION: redis\n      SESSION_DRIVER: redis\n      REDIS_CLIENT: phpredis\n      REDIS_HOST: redis\n      REDIS_PORT: 6379\n      REDIS_PASSWORD: null\n    depends_on:\n      - redis\n\n  redis:\n    image: redis:7-alpine\n    ports:\n      - \"{{DB_PORT}}:6379\"\n    volumes:\n      - redis-data:/data\n\nvolumes:\n  redis-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Redis", "Docker Compose para PHP/Laravel o Symfony con Redis", null, "DockerCompose", true, "Compose PHP + Redis", "compose", null, null, 1 },
                    { 18, "Php", "FROM php:8.3-cli-bookworm\n\nWORKDIR /var/www/html\n\nRUN apt-get update && apt-get install -y --no-install-recommends \\\n    curl gnupg unixodbc-dev libgssapi-krb5-2 libicu-dev libzip-dev libpng-dev libonig-dev libxml2-dev libssl-dev pkg-config $PHPIZE_DEPS \\\n    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /usr/share/keyrings/microsoft.gpg >/dev/null \\\n    && echo \"deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main\" > /etc/apt/sources.list.d/microsoft-prod.list \\\n    && apt-get update \\\n    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 \\\n    && pecl install sqlsrv pdo_sqlsrv \\\n    && docker-php-ext-enable sqlsrv pdo_sqlsrv \\\n    && docker-php-ext-install pdo mbstring zip intl bcmath \\\n    && rm -rf /var/lib/apt/lists/*\n\nCOPY --from=composer:2 /usr/bin/composer /usr/bin/composer\n\nCOPY . .\n\nRUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \\\n    && mkdir -p storage bootstrap/cache \\\n    && chmod -R 775 storage bootstrap/cache\n\nRUN printf '%s\\n' \\\n    '<?php' \\\n    'if (php_sapi_name() === '\"'\"'\"'\"'\"'\"'cli-server'\"'\"'\"'\"'\"'\"') {' \\\n    '    $path = parse_url($_SERVER['\"'\"'\"'\"'\"'\"'REQUEST_URI'\"'\"'\"'\"'\"'\"'], PHP_URL_PATH);' \\\n    '    $file = __DIR__ . '\"'\"'\"'\"'\"'\"'/public'\"'\"'\"'\"'\"'\"' . $path;' \\\n    '    if ($path !== '\"'\"'\"'\"'\"'\"'/'\"'\"'\"'\"'\"'\"' && is_file($file)) {' \\\n    '        return false;' \\\n    '    }' \\\n    '}' \\\n    'require __DIR__ . '\"'\"'\"'\"'\"'\"'/public/index.php'\"'\"'\"'\"'\"';' \\\n    > /usr/local/bin/router.php\n\nEXPOSE 8080\n\nCMD [\"php\", \"-S\", \"0.0.0.0:8080\", \"-t\", \"public\", \"/usr/local/bin/router.php\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SqlServer", "Dockerfile para PHP/Laravel o Symfony con soporte SQL Server", null, null, true, "Dockerfile PHP + SQL Server", "dockerfile", null, null, 1 },
                    { 19, "Php", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      APP_ENV: local\n      APP_DEBUG: \"true\"\n      APP_URL: http://localhost:{{APP_PORT}}\n      DB_CONNECTION: sqlsrv\n      DB_HOST: sqlserver\n      DB_PORT: 1433\n      DB_DATABASE: {{DB_NAME}}\n      DB_USERNAME: sa\n      DB_PASSWORD: YourStrong!Passw0rd\n      DB_ENCRYPT: \"false\"\n      DB_TRUST_SERVER_CERTIFICATE: \"true\"\n      DATABASE_URL: sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/{{DB_NAME}}\n    depends_on:\n      sqlserver:\n        condition: service_started\n\n  sqlserver:\n    image: mcr.microsoft.com/mssql/server:2022-latest\n    environment:\n      ACCEPT_EULA: Y\n      MSSQL_PID: Developer\n      MSSQL_SA_PASSWORD: YourStrong!Passw0rd\n    ports:\n      - \"{{DB_PORT}}:1433\"\n    volumes:\n      - mssql-data:/var/opt/mssql\n\nvolumes:\n  mssql-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SqlServer", "Docker Compose para PHP/Laravel o Symfony con SQL Server", null, "DockerCompose", true, "Compose PHP + SQL Server", "compose", null, null, 1 },
                    { 200, "JavaScript", "FROM node:22-alpine\n\nWORKDIR /app\n\nCOPY package*.json ./\nRUN if [ -f package-lock.json ]; then npm ci --omit=dev; else npm install --omit=dev; fi\n\nCOPY . .\n\nEXPOSE 3000\n\nCMD [\"npm\", \"start\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile generico para Node.js, Express, NestJS y Next.js", null, null, true, "Dockerfile JavaScript", "dockerfile", null, null, 1 },
                    { 201, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: postgres://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\", \"pg_isready\", \"-U\", \"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para Node.js con PostgreSQL", null, "DockerCompose", true, "Compose JavaScript + PostgreSQL", "compose", null, null, 1 },
                    { 202, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\", \"mysqladmin\", \"ping\", \"-h\", \"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Node.js con MySQL", null, "DockerCompose", true, "Compose JavaScript + MySQL", "compose", null, null, 1 },
                    { 203, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: sqlite:///app/data/app.sqlite\n    volumes:\n      - sqlite-data:/app/data\n\nvolumes:\n  sqlite-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SQLite", "Docker Compose para Node.js usando SQLite", null, "DockerCompose", true, "Compose JavaScript + SQLite", "compose", null, null, 1 },
                    { 204, "JavaScript", "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n        - name: {{APP_NAME}}\n          image: {{APP_NAME}}:latest\n          ports:\n            - containerPort: 3000\n          env:\n            - name: NODE_ENV\n              value: production\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n    - port: 80\n      targetPort: 3000\n  type: LoadBalancer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kubernetes Deployment y Service para Node.js", null, "Kubernetes", true, "K8s JavaScript Deployment", "k8s-deployment", null, null, 1 },
                    { 205, "JavaScript", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - name: Setup Node\n        uses: actions/setup-node@v4\n        with:\n          node-version: '22'\n          cache: npm\n      - name: Install dependencies\n        run: npm ci\n      - name: Build\n        run: npm run build --if-present\n      - name: Test\n        run: npm test --if-present", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI/CD para Node.js, NestJS y Next.js", null, null, true, "CI JavaScript GitHub Actions", "ci", null, null, 1 },
                    { 206, "JavaScript", "/node_modules/\n/dist/\n/.next/\n/.turbo/\n/coverage/\n/.env\n/.env.*\n/npm-debug.log*\n/yarn-debug.log*\n/pnpm-debug.log*", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gitignore base para proyectos Node.js modernos", null, null, true, "JavaScript .gitignore", "gitignore", null, null, 1 },
                    { 500, "TypeScript", "FROM node:22-alpine AS build\nWORKDIR /app\nCOPY package*.json .\nRUN npm ci\nCOPY . .\nRUN npm run build\n\nFROM node:22-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/dist ./dist\nCOPY --from=build /app/node_modules ./node_modules\nEXPOSE 3000\nCMD [\"node\", \"dist/main\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile multi-stage para NestJS con TypeScript", null, null, true, "Dockerfile NestJS TypeScript", "dockerfile", null, null, 1 },
                    { 501, "TypeScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      retries: 5\n    volumes:\n      - pgdata:/var/lib/postgresql/data\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para NestJS TypeScript + PostgreSQL", null, "DockerCompose", true, "Compose NestTS + PostgreSQL", "compose", null, null, 1 },
                    { 502, "TypeScript", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-node@v4\n        with:\n          node-version: '22'\n          cache: npm\n      - run: npm ci\n      - run: npm run build\n      - run: npm test", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI para NestJS/NextJS TypeScript", null, null, true, "CI TypeScript GitHub Actions", "ci", null, null, 1 },
                    { 503, "TypeScript", "node_modules/\ndist/\n.next/\nbuild/\n.env\n.env.*\n*.log\ncoverage/\n.DS_Store", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, ".gitignore para proyectos TypeScript", null, null, true, "TypeScript .gitignore", "gitignore", null, null, 1 },
                    { 800, "Python", "FROM python:3.12-slim\nWORKDIR /app\nCOPY requirements.txt .\nRUN pip install --no-cache-dir -r requirements.txt\nCOPY . .\nEXPOSE 8000\nCMD [\"uvicorn\", \"app.main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8000\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile para FastAPI con uvicorn", null, null, true, "Dockerfile Python / FastAPI", "dockerfile", null, null, 1 },
                    { 801, "Python", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para FastAPI/Django + PostgreSQL", null, "DockerCompose", true, "Compose Python + PostgreSQL", "compose", null, null, 1 },
                    { 802, "Python", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Python + MySQL", null, "DockerCompose", true, "Compose Python + MySQL", "compose", null, null, 1 },
                    { 900, "Java", "FROM maven:3.9-eclipse-temurin-21 AS build\nWORKDIR /app\nCOPY . .\nRUN mvn -q package -DskipTests\n\nFROM eclipse-temurin:21-jre-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/target/*.jar app.jar\nEXPOSE 8080\nENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Multi-stage Dockerfile para Spring Boot con Maven", null, null, true, "Dockerfile Java / Spring Boot", "dockerfile", null, null, 1 },
                    { 901, "Java", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: postgres\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para Spring Boot + PostgreSQL", null, "DockerCompose", true, "Compose Java + PostgreSQL", "compose", null, null, 1 },
                    { 902, "Java", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:mysql://db:3306/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: root\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Spring Boot + MySQL", null, "DockerCompose", true, "Compose Java + MySQL", "compose", null, null, 1 },
                    { 1100, "Java", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-java@v4\n        with:\n          java-version: '21'\n          distribution: temurin\n          cache: maven\n      - run: mvn -B package --no-transfer-progress", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI para Maven/Spring Boot", null, null, true, "CI Java GitHub Actions", "ci", null, null, 1 },
                    { 1200, "Python", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-python@v5\n        with:\n          python-version: '3.12'\n      - run: pip install -r requirements.txt\n      - run: pytest --tb=short", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI para Python con pytest", null, null, true, "CI Python GitHub Actions", "ci", null, null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiSuggestionCaches_CacheKey",
                table: "AiSuggestionCaches",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignPatterns_LibraryRecommendationId",
                table: "DesignPatterns",
                column: "LibraryRecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLogs_ProjectId",
                table: "ProjectLogs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WizardConfigId",
                table: "Projects",
                column: "WizardConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Architecture_TemplateType_Database_Infrastructure",
                table: "Templates",
                columns: new[] { "Architecture", "TemplateType", "Database", "Infrastructure" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GitHubId",
                table: "Users",
                column: "GitHubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VpsCredentials_WizardConfigId",
                table: "VpsCredentials",
                column: "WizardConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSuggestionCaches");

            migrationBuilder.DropTable(
                name: "DesignPatterns");

            migrationBuilder.DropTable(
                name: "ProjectLogs");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "VpsCredentials");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WizardConfigs");
        }
    }
}
