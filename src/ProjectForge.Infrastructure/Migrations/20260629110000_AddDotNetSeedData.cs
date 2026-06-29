using Microsoft.EntityFrameworkCore.Migrations;
using ProjectForge.Infrastructure.Seeders.DotNet;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all C#/.NET data: Templates, Libraries y DesignPatterns.
/// Delega en DotNetSeeder para mantener la fuente de verdad en un único lugar.
/// </summary>
public partial class AddDotNetSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns (IDs 1–4, 1000–1005) ──────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1,N'DotNet','2026-01-01T00:00:00Z',N'Abstrae el acceso a datos detrás de interfaces, facilitando testing y mantenimiento.',N'Crear IRepository<T> en Domain, implementar con EF Core en Infrastructure.',N'Repository Pattern (.NET)',N'Repository',N'["mkdir -p src/Domain/Interfaces src/Infrastructure/Repositories"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 2)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (2,N'DotNet','2026-01-01T00:00:00Z',N'Separa las operaciones de lectura (Queries) de las de escritura (Commands).',N'Instalar MediatR. Crear Application/Commands y Application/Queries con handlers.',N'CQRS + MediatR (.NET)',N'CQRS',N'["mkdir -p src/Application/Commands src/Application/Queries src/Application/Handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 3)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (3,N'DotNet','2026-01-01T00:00:00Z',N'Capas: Domain, Application, Infrastructure, Presentation.',N'Domain no referencia nada. Application referencia Domain. Infrastructure implementa interfaces.',N'Clean Architecture (.NET)',N'CleanArchitecture',N'["mkdir -p src/Domain src/Application src/Infrastructure src/Presentation"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 4)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (4,N'DotNet','2026-01-01T00:00:00Z',N'Aggregates, Entities, Value Objects y Domain Events.',N'Modelar el dominio con entidades ricas. Usar eventos de dominio para comunicación entre aggregates.',N'Domain-Driven Design (.NET)',N'DomainDrivenDesign',N'["mkdir -p src/Domain/Aggregates src/Domain/Events src/Domain/ValueObjects src/Domain/Services"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1000)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1000,N'DotNet','2026-01-01T00:00:00Z',N'Ports & Adapters: el core no conoce infraestructura.',N'Definir ports (interfaces) en Core. Implementar adapters en Infrastructure.',N'Hexagonal Architecture (.NET)',N'HexagonalArchitecture',N'["mkdir -p src/Core/Ports src/Core/Domain src/Infrastructure/Adapters src/Api"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1001)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1001,N'DotNet','2026-01-01T00:00:00Z',N'Desacopla componentes con un mediador central (MediatR).',N'Usar IRequest<T> e IRequestHandler<T>. Registrar en DI con AddMediatR.',N'Mediator (.NET)',N'Mediator',N'["mkdir -p src/Application/Features"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1002)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1002,N'DotNet','2026-01-01T00:00:00Z',N'Estado derivado de secuencia de eventos inmutables.',N'Usar EventStore o Marten. Cada cambio se registra como evento.',N'Event Sourcing (.NET)',N'EventSourcing',N'["mkdir -p src/Domain/Events src/Infrastructure/EventStore src/Application/EventHandlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1003)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1003,N'DotNet','2026-01-01T00:00:00Z',N'Servicios independientes comunicándose por HTTP o mensajes.',N'Usar Ocelot como API Gateway. RabbitMQ o Azure Service Bus para mensajería.',N'Microservices (.NET)',N'Microservices',N'["mkdir -p services/gateway services/orders services/notifications"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1004)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1004,N'DotNet','2026-01-01T00:00:00Z',N'Coordina transacciones distribuidas con compensación ante fallos.',N'Implementar OrchestrationSaga con MediatR. Cada paso compensa los anteriores en caso de error.',N'Saga (.NET)',N'Saga',N'["mkdir -p src/Application/Sagas"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1005)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1005,N'DotNet','2026-01-01T00:00:00Z',N'Model-View-ViewModel para desacoplar UI de lógica (Blazor, WPF, MAUI).',N'ViewModel implementa INotifyPropertyChanged. Usar RelayCommand para comandos de UI.',N'MVVM (.NET)',N'MVVM',N'["mkdir -p src/Presentation/ViewModels src/Presentation/Commands"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries (IDs 1–7, 1000–1008) ───────────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1,N'DotNet',N'CQRS/Mediator','2026-01-01T00:00:00Z',N'Implementación del patrón Mediator para CQRS',NULL,N'dotnet add package MediatR',N'MediatR',N'MediatR',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 2)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (2,N'DotNet',N'ORM','2026-01-01T00:00:00Z',N'ORM oficial de Microsoft para .NET',NULL,N'dotnet add package Microsoft.EntityFrameworkCore',N'Entity Framework Core',N'Microsoft.EntityFrameworkCore',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 3)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (3,N'DotNet',N'Validation','2026-01-01T00:00:00Z',N'Validación fluida y expresiva',NULL,N'dotnet add package FluentValidation.AspNetCore',N'FluentValidation',N'FluentValidation.AspNetCore',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 4)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (4,N'DotNet',N'Logging','2026-01-01T00:00:00Z',N'Logging estructurado para .NET',NULL,N'dotnet add package Serilog.AspNetCore',N'Serilog',N'Serilog.AspNetCore',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 5)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (5,N'DotNet',N'Mapping','2026-01-01T00:00:00Z',N'Mapeo automático entre objetos',NULL,N'dotnet add package AutoMapper',N'AutoMapper',N'AutoMapper',94);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 6)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (6,N'DotNet',N'Documentation','2026-01-01T00:00:00Z',N'Generación automática de documentación OpenAPI',NULL,N'dotnet add package Swashbuckle.AspNetCore',N'Swashbuckle (Swagger)',N'Swashbuckle.AspNetCore',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 7)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (7,N'DotNet',N'Testing','2026-01-01T00:00:00Z',N'Framework de testing unitario para .NET',NULL,N'dotnet add package xunit',N'xUnit',N'xunit',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1000)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1000,N'DotNet',N'ORM','2026-01-01T00:00:00Z',N'Micro ORM rápido y ligero para .NET',NULL,N'dotnet add package Dapper',N'Dapper',N'Dapper',91);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1001)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1001,N'DotNet',N'Database','2026-01-01T00:00:00Z',N'Provider de PostgreSQL para EF Core',NULL,N'dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL',N'Npgsql EF Core',N'Npgsql.EntityFrameworkCore.PostgreSQL',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1002)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1002,N'DotNet',N'Database','2026-01-01T00:00:00Z',N'Provider de MySQL para EF Core',NULL,N'dotnet add package Pomelo.EntityFrameworkCore.MySql',N'Pomelo MySQL EF Core',N'Pomelo.EntityFrameworkCore.MySql',89);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1003)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1003,N'DotNet',N'Routing','2026-01-01T00:00:00Z',N'Módulos de rutas elegantes para Minimal API',N'MinimalApi',N'dotnet add package Carter',N'Carter',N'Carter',82);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1004)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1004,N'DotNet',N'Resilience','2026-01-01T00:00:00Z',N'Librería de resiliencia y manejo de fallos transitivos',NULL,N'dotnet add package Polly',N'Polly',N'Polly',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1005)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1005,N'DotNet',N'Testing','2026-01-01T00:00:00Z',N'Framework de testing alternativo para .NET',NULL,N'dotnet add package NUnit',N'NUnit',N'NUnit',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1006)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1006,N'DotNet',N'Testing','2026-01-01T00:00:00Z',N'Generador de datos falsos para tests',NULL,N'dotnet add package Bogus',N'Bogus',N'Bogus',86);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1007)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1007,N'DotNet',N'Background Jobs','2026-01-01T00:00:00Z',N'Jobs en background con panel de administración',NULL,N'dotnet add package Hangfire.AspNetCore',N'Hangfire',N'Hangfire.AspNetCore',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1008)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1008,N'DotNet',N'Cache','2026-01-01T00:00:00Z',N'Cliente Redis de alto rendimiento para .NET',NULL,N'dotnet add package StackExchange.Redis',N'StackExchange.Redis',N'StackExchange.Redis',92);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates (IDs 1–6) ───────────────────────────────────────────
        // dockerfile, compose (PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite), k8s, ci, gitignore
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1,N'DotNet',N'FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "{{APP_NAME}}.dll"]','2026-01-01T00:00:00Z',NULL,N'Multi-stage Dockerfile para ASP.NET Core',NULL,NULL,1,N'Dockerfile .NET',N'dockerfile',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 2)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (2,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para .NET + PostgreSQL',NULL,N'DockerCompose',1,N'Compose .NET + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 3)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (3,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
    depends_on:
      db:
        condition: service_healthy
  db:
    image: mysql:8.0
    environment:
      MYSQL_DATABASE: {{DB_NAME}}
      MYSQL_ROOT_PASSWORD: secret
    ports:
      - "{{DB_PORT}}:3306"
    volumes:
      - mysqldata:/var/lib/mysql
    healthcheck:
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para .NET + MySQL',NULL,N'DockerCompose',1,N'Compose .NET + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 4)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (4,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=Secret1234!;
    depends_on:
      - db
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: Secret1234!
    ports:
      - "{{DB_PORT}}:1433"
    volumes:
      - mssqldata:/var/opt/mssql
volumes:
  mssqldata:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para .NET + SQL Server',NULL,N'DockerCompose',1,N'Compose .NET + SQL Server',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 40)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (40,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para .NET + MongoDB',NULL,N'DockerCompose',1,N'Compose .NET + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 41)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (41,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
    depends_on:
      db:
        condition: service_healthy
      redis:
        condition: service_started
  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: {{DB_NAME}}
      POSTGRES_PASSWORD: secret
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
  redis:
    image: redis:7-alpine
    ports:
      - "{{DB_PORT}}:6379"
    volumes:
      - redis-data:/data
volumes:
  pgdata:
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para .NET + PostgreSQL + Redis',NULL,N'DockerCompose',1,N'Compose .NET + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 42)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (42,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para .NET + SQLite',NULL,N'DockerCompose',1,N'Compose .NET + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 5)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (5,N'DotNet',N'apiVersion: apps/v1
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
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para .NET',NULL,N'Kubernetes',1,N'K8s .NET Deployment',N'k8s-deployment',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 6)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (6,N'DotNet',N'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ''10.0.x''
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal','2026-01-01T00:00:00Z',NULL,N'Pipeline CI para .NET con GitHub Actions',NULL,NULL,1,N'CI .NET GitHub Actions',N'ci',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 43)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (43,N'DotNet',N'[Dd]ebug/
[Rr]elease/
[Bb]in/
[Oo]bj/
*.user
*.suo
.vs/
.vscode/
.env
.env.*
*.pfx
packages/','2026-01-01T00:00:00Z',NULL,N'Gitignore para proyectos .NET',NULL,NULL,1,N'.gitignore .NET',N'gitignore',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (1,2,3,4,1000,1001,1002,1003,1004,1005);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (1,2,3,4,5,6,7,1000,1001,1002,1003,1004,1005,1006,1007,1008);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (1,2,3,4,5,6,40,41,42,43);");
    }
}
