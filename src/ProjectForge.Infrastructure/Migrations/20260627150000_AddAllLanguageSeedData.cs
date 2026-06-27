using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1814

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllLanguageSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── CI Templates ────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Templates",
                columns: ["Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "Version"],
                values: new object[,]
                {
                    { 1100, "Java", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-java@v4\n        with:\n          java-version: '21'\n          distribution: temurin\n          cache: maven\n      - run: mvn -B package --no-transfer-progress", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), null, "Pipeline CI para Maven/Spring Boot", null, null, true, "CI Java GitHub Actions", "ci", 1 },
                    { 1200, "Python", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-python@v5\n        with:\n          python-version: '3.12'\n      - run: pip install -r requirements.txt\n      - run: pytest --tb=short", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), null, "Pipeline CI para Python con pytest", null, null, true, "CI Python GitHub Actions", "ci", 1 },
                });

            // ─── .NET Extra Libraries ─────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 1000, "DotNet", "ORM",            new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Micro ORM rápido y ligero para .NET",                         null, "dotnet add package Dapper",                                  null, "Dapper",              "Dapper",                                        91 },
                    { 1001, "DotNet", "Database",        new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Provider de PostgreSQL para EF Core",                          null, "dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL",   null, "Npgsql EF Core",      "Npgsql.EntityFrameworkCore.PostgreSQL",         93 },
                    { 1002, "DotNet", "Database",        new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Provider de MySQL para EF Core",                               null, "dotnet add package Pomelo.EntityFrameworkCore.MySql",       null, "Pomelo MySQL EF Core","Pomelo.EntityFrameworkCore.MySql",              89 },
                    { 1003, "DotNet", "Routing",         new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Módulos de rutas elegantes para Minimal API",                  "MinimalApi", "dotnet add package Carter",                         null, "Carter",              "Carter",                                        82 },
                    { 1004, "DotNet", "Resilience",      new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Resiliencia y manejo de fallos transitivos",                   null, "dotnet add package Polly",                                  null, "Polly",               "Polly",                                         93 },
                    { 1005, "DotNet", "Testing",         new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Framework de testing alternativo para .NET",                   null, "dotnet add package NUnit",                                  null, "NUnit",               "NUnit",                                         88 },
                    { 1006, "DotNet", "Testing",         new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Generador de datos falsos para tests",                         null, "dotnet add package Bogus",                                  null, "Bogus",               "Bogus",                                         86 },
                    { 1007, "DotNet", "Background Jobs", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Jobs en background con panel de administración",               null, "dotnet add package Hangfire.AspNetCore",                    null, "Hangfire",            "Hangfire.AspNetCore",                           90 },
                    { 1008, "DotNet", "Cache",           new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Cliente Redis de alto rendimiento para .NET",                  null, "dotnet add package StackExchange.Redis",                    null, "StackExchange.Redis", "StackExchange.Redis",                           92 },
                });

            // ─── Java Extra Libraries ────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 1100, "Java", "Reactive",    new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Programación reactiva con Spring WebFlux",    null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-webflux</artifactId>\n</dependency>", null, "Spring WebFlux",      "spring-boot-starter-webflux",        85 },
                    { 1101, "Java", "Mensajería",  new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Integración con Apache Kafka",                 null, "<dependency>\n  <groupId>org.springframework.kafka</groupId>\n  <artifactId>spring-kafka</artifactId>\n</dependency>",            null, "Spring Kafka",        "spring-kafka",                       87 },
                    { 1102, "Java", "Cache",       new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Integración con Redis para caching",           null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-redis</artifactId>\n</dependency>", null, "Spring Data Redis",   "spring-boot-starter-data-redis",     88 },
                    { 1103, "Java", "Testing",     new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Framework de testing moderno para Java",       null, "<dependency>\n  <groupId>org.junit.jupiter</groupId>\n  <artifactId>junit-jupiter</artifactId>\n  <scope>test</scope>\n</dependency>", null, "JUnit 5",             "junit-5",                            99 },
                    { 1104, "Java", "Testing",     new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Framework de mocking para tests unitarios",    null, "<dependency>\n  <groupId>org.mockito</groupId>\n  <artifactId>mockito-core</artifactId>\n  <scope>test</scope>\n</dependency>",   null, "Mockito",             "mockito-core",                       97 },
                });

            // ─── Python Extra Libraries ──────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 1200, "Python", "Framework",      new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Framework web moderno y rápido para APIs",           "FastAPI", "pip install fastapi uvicorn[standard]", null, "FastAPI",             "fastapi",             99 },
                    { 1201, "Python", "API",            new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Extensión de Django para crear APIs REST",            "Django",  "pip install djangorestframework",      null, "Django REST Framework","djangorestframework", 96 },
                    { 1202, "Python", "ORM",            new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Integración de SQLAlchemy con Flask",                 "Flask",   "pip install flask-sqlalchemy",          null, "Flask-SQLAlchemy",    "flask-sqlalchemy",    88 },
                    { 1203, "Python", "Cache",          new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Cliente Redis async para Python",                    null,      "pip install redis[asyncio]",            null, "aioredis",            "redis",                87 },
                    { 1204, "Python", "Autenticación",  new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Manejo de tokens JWT en Python",                     null,      "pip install pyjwt",                     null, "PyJWT",               "pyjwt",                92 },
                    { 1205, "Python", "Config",         new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Carga variables de entorno desde .env",              null,      "pip install python-dotenv",             null, "Python-dotenv",       "python-dotenv",        96 },
                });

            // ─── .NET Extra Design Patterns ──────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 1000, "DotNet", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Ports & Adapters: el core no conoce infraestructura.",    "Definir ports (interfaces) en Core. Implementar adapters en Infrastructure.",                                               null, "Hexagonal Architecture (.NET)",  "HexagonalArchitecture", "[\"mkdir -p src/Core/Ports src/Core/Domain src/Infrastructure/Adapters src/Api\"]" },
                    { 1001, "DotNet", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Desacopla componentes con un mediador central (MediatR).", "Usar IRequest<T> e IRequestHandler<T>. Registrar en DI con AddMediatR.",                                                    null, "Mediator (.NET)",                "Mediator",              "[\"mkdir -p src/Application/Features\"]" },
                    { 1002, "DotNet", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Estado derivado de secuencia de eventos inmutables.",      "Usar EventStore o Marten. Cada cambio se registra como evento.",                                                            null, "Event Sourcing (.NET)",          "EventSourcing",         "[\"mkdir -p src/Domain/Events src/Infrastructure/EventStore src/Application/EventHandlers\"]" },
                    { 1003, "DotNet", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Servicios independientes comunicándose por HTTP o mensajes.","Usar Ocelot como API Gateway. RabbitMQ o Azure Service Bus para mensajería.",                                             null, "Microservices (.NET)",           "Microservices",         "[\"mkdir -p services/gateway services/orders services/notifications\"]" },
                });

            // ─── Java Extra Design Patterns ──────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 1100, "Java", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Estado derivado de eventos con ApplicationEventPublisher.", "Usar Spring Events o Axon Framework. Guardar eventos en EventStore.",                         null, "Event Sourcing (Spring Boot)",    "EventSourcing",  "[\"mkdir -p src/main/java/domain/event src/main/java/application/eventhandler src/main/java/infrastructure/eventstore\"]" },
                    { 1101, "Java", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Servicios independientes con Spring Cloud.",                "Usar Eureka para discovery, Gateway para routing, Feign para comunicación.",                   null, "Microservices (Spring Boot)",     "Microservices",  "[\"mkdir -p services/gateway services/discovery services/orders\"]" },
                });

            // ─── Python Extra Design Patterns ────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 1200, "Python", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Ports como ABCs de Python, adapters como implementaciones concretas.", "Usar ABC para ports. Inyectar adapters en los servicios de aplicación.", null, "Hexagonal Architecture (Python)", "HexagonalArchitecture", "[\"mkdir -p app/core/ports app/core/domain app/infrastructure/adapters app/api\"]" },
                    { 1201, "Python", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Aggregates, Entities y Value Objects con dataclasses.",               "Usar @dataclass para Value Objects. Domain events con publish/subscribe.",  null, "Domain-Driven Design (Python)",   "DomainDrivenDesign",    "[\"mkdir -p app/domain/aggregates app/domain/events app/domain/value_objects app/application\"]" },
                    { 1202, "Python", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Estado reconstruido desde eventos inmutables.",                        "Usar EventStoreDB o PostgreSQL como event store.",                         null, "Event Sourcing (Python)",         "EventSourcing",         "[\"mkdir -p app/domain/events app/infrastructure/event_store app/application/projections\"]" },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Templates",      keyColumn: "Id", keyValues: [1100, 1200]);
            migrationBuilder.DeleteData(table: "Libraries",      keyColumn: "Id", keyValues: [1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1100, 1101, 1102, 1103, 1104, 1200, 1201, 1202, 1203, 1204, 1205]);
            migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValues: [1000, 1001, 1002, 1003, 1100, 1101, 1200, 1201, 1202]);
        }
    }
}
