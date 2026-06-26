using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Data;

/// <summary>
/// Seeder idempotente que se ejecuta en cada arranque DESPUÉS de las migraciones.
/// Garantiza que TODOS los patrones de diseño y librerías recomendadas existan en la
/// base de datos para Python, Java y TypeScript (además de .NET), sin depender del
/// historial de migraciones de EF ni del estado del volumen de Docker.
///
/// Esto soluciona el problema clásico de "solo aparece Repository Pattern": ocurría
/// cuando el volumen de SQL Server era anterior a las migraciones de seeds, de modo que
/// EF creía que ya estaba todo aplicado y nunca insertaba los datos nuevos.
///
/// El seeder hace UPSERT por (Architecture + Name): si la fila no existe la crea, y si
/// existe actualiza su descripción/categoría. Nunca duplica ni borra datos del usuario.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // La migración AddAiSuggestionCache no tiene su archivo .Designer.cs, por lo que
        // EF Core no la reconoce en la cadena de migraciones y nunca crea la tabla
        // 'AiSuggestionCaches'. Eso provoca el error "Invalid object name 'AiSuggestionCaches'"
        // al usar la IA (el caché). Aquí garantizamos la tabla con SQL idempotente, sin
        // depender del estado de las migraciones de EF.
        await EnsureAiSuggestionCacheTableAsync(db, ct);

        await SeedPatternsAsync(db, ct);
        await SeedLibrariesAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureAiSuggestionCacheTableAsync(AppDbContext db, CancellationToken ct)
    {
        const string sql = @"
IF OBJECT_ID(N'[dbo].[AiSuggestionCaches]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiSuggestionCaches] (
        [Id]            INT IDENTITY(1,1) NOT NULL,
        [CacheKey]      NVARCHAR(450)  NOT NULL,
        [PatternsJson]  NVARCHAR(MAX)  NOT NULL,
        [LibrariesJson] NVARCHAR(MAX)  NOT NULL,
        [Rationale]     NVARCHAR(MAX)  NOT NULL,
        [ExpiresAt]     DATETIME2      NOT NULL,
        [CreatedAt]     DATETIME2      NOT NULL,
        [UpdatedAt]     DATETIME2      NULL,
        CONSTRAINT [PK_AiSuggestionCaches] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_AiSuggestionCaches_CacheKey]
        ON [dbo].[AiSuggestionCaches] ([CacheKey]);
END";
        await db.Database.ExecuteSqlRawAsync(sql, ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PATRONES DE DISEÑO
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedPatternsAsync(AppDbContext db, CancellationToken ct)
    {
        var desired = new List<(ArchitectureType Arch, DesignPattern Pattern, string Name, string Description)>
        {
            // ── .NET ──────────────────────────────────────────────────────────
            (ArchitectureType.DotNet, DesignPattern.Repository,            "Repository Pattern", "Abstrae el acceso a datos detrás de interfaces, facilitando testing y mantenimiento."),
            (ArchitectureType.DotNet, DesignPattern.CQRS,                  "CQRS",               "Separa las operaciones de lectura (Queries) de las de escritura (Commands)."),
            (ArchitectureType.DotNet, DesignPattern.CleanArchitecture,     "Clean Architecture", "Arquitectura en capas concéntricas con dependencias hacia el centro."),
            (ArchitectureType.DotNet, DesignPattern.Mediator,              "Mediator",           "Reduce el acoplamiento directo entre componentes usando un mediador."),

            // ── Python (coincide con las imágenes de referencia) ───────────────
            (ArchitectureType.Python, DesignPattern.Repository,            "Model-View-Template (MVT)", "Patrón MVT de Django: Model-View-Template para separar lógica, datos y presentación."),
            (ArchitectureType.Python, DesignPattern.Microservices,         "Microservices",             "Arquitectura de microservicios con FastAPI: servicios pequeños e independientes comunicados vía HTTP/async."),
            (ArchitectureType.Python, DesignPattern.Repository,            "Factory Method",            "Crea objetos sin especificar la clase exacta. Muy usado en Django para forms, serializers y conexiones."),
            (ArchitectureType.Python, DesignPattern.Repository,            "Repository Pattern",        "Abstrae el acceso a datos detrás de interfaces. Con SQLAlchemy separa la lógica de la BD de tu código."),
            (ArchitectureType.Python, DesignPattern.CleanArchitecture,     "Decorator Pattern",         "Añade comportamiento a funciones/clases de forma dinámica usando @decoradores nativos de Python."),
            (ArchitectureType.Python, DesignPattern.CleanArchitecture,     "Clean Architecture",        "Capas bien definidas (domain, application, infrastructure) para proyectos Python."),

            // ── Java (coincide con las imágenes de referencia) ─────────────────
            (ArchitectureType.Java,   DesignPattern.Repository,            "MVC (Model-View-Controller)", "Patrón MVC con Spring MVC: separa lógica de negocio, presentación y control de flujo."),
            (ArchitectureType.Java,   DesignPattern.CleanArchitecture,     "Clean Architecture",          "Arquitectura en capas concéntricas (Domain, Application, Infrastructure, Presentation) con Spring Boot."),
            (ArchitectureType.Java,   DesignPattern.DomainDrivenDesign,    "Domain-Driven Design (DDD)",  "DDD con Aggregates, Entities, Value Objects y Domain Events en Spring Boot."),
            (ArchitectureType.Java,   DesignPattern.CQRS,                  "CQRS",                        "Separación de comandos y consultas en aplicaciones Spring Boot."),
            (ArchitectureType.Java,   DesignPattern.Repository,            "Singleton Pattern",           "Garantiza una única instancia de una clase (ej: conexiones a BD). Implementado con @Bean en Spring."),
            (ArchitectureType.Java,   DesignPattern.CleanArchitecture,     "Dependency Injection",        "Inversión de control (IoC) con el contenedor de Spring: @Autowired, @Inject, @Component."),
            (ArchitectureType.Java,   DesignPattern.Repository,            "Repository Pattern",          "Patrón de repositorio con Spring Data JPA: implementación casi automática con interfaces."),
            (ArchitectureType.Java,   DesignPattern.HexagonalArchitecture, "Hexagonal Architecture",      "Ports & Adapters: aísla el dominio de la infraestructura. Muy usado en Java empresarial."),
            (ArchitectureType.Java,   DesignPattern.Microservices,         "Microservices",               "Arquitectura de microservicios con Spring Boot y Spring Cloud."),

            // ── TypeScript (coincide con las imágenes de referencia) ───────────
            (ArchitectureType.TypeScript, DesignPattern.HexagonalArchitecture, "Hexagonal Architecture (Ports & Adapters)", "Aísla el dominio de la infraestructura con puertos y adaptadores. Ideal para NestJS."),
            (ArchitectureType.TypeScript, DesignPattern.Repository,            "Component-Based Architecture",              "Organización basada en componentes reutilizables, clave en Next.js y NestJS."),
            (ArchitectureType.TypeScript, DesignPattern.MVVM,                  "Observer Pattern",                          "Comunicación reactiva entre componentes desacoplados usando observadores (RxJS)."),
            (ArchitectureType.TypeScript, DesignPattern.Mediator,              "Mediator Pattern",                          "Centraliza la comunicación entre módulos TypeScript evitando dependencias directas."),
            (ArchitectureType.TypeScript, DesignPattern.CleanArchitecture,     "SOLID Principles Integration",              "Aplicación de los 5 principios SOLID al diseño de módulos TypeScript/NestJS."),

            // ── JavaScript ─────────────────────────────────────────────────────
            (ArchitectureType.JavaScript, DesignPattern.Microservices,         "Microservices",                             "Arquitectura de microservicios para aplicaciones Node.js."),
            (ArchitectureType.JavaScript, DesignPattern.Repository,            "Repository Pattern",                        "Abstrae el acceso a datos detrás de interfaces en Express/Node.js."),
        };

        var existing = await db.DesignPatterns.ToListAsync(ct);

        foreach (var d in desired)
        {
            var row = existing.FirstOrDefault(e => e.Architecture == d.Arch && e.Name == d.Name);
            if (row is null)
            {
                db.DesignPatterns.Add(new DesignPatternEntry
                {
                    Architecture = d.Arch,
                    Pattern      = d.Pattern,
                    Name         = d.Name,
                    Description  = d.Description,
                    CreatedAt    = DateTime.UtcNow,
                });
            }
            else if (string.IsNullOrWhiteSpace(row.Description) || row.Description != d.Description)
            {
                row.Description = d.Description;
                row.UpdatedAt   = DateTime.UtcNow;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LIBRERÍAS RECOMENDADAS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedLibrariesAsync(AppDbContext db, CancellationToken ct)
    {
        var desired = new List<LibraryRecommendation>
        {
            // ── Python ─────────────────────────────────────────────────────────
            New(ArchitectureType.Python, "Pydantic",   "pydantic",                "Validation",  99,
                "Validación de datos con tipado. Cuando una API mande un JSON con campos inesperados, lanza un error decente en lugar de que tu app colapse silenciosamente.",
                "pip install pydantic"),
            New(ArchitectureType.Python, "SQLAlchemy", "sqlalchemy",              "ORM",         97,
                "El ORM por excelencia para implementar el patrón Repository. Separa la lógica de la base de datos de tu código principal.",
                "pip install sqlalchemy"),
            New(ArchitectureType.Python, "FastAPI",    "fastapi",                 "Framework",   98,
                "Ideal si haces Microservicios. Rápido, moderno y se integra perfectamente con Pydantic.",
                "pip install fastapi uvicorn"),
            New(ArchitectureType.Python, "Alembic",    "alembic",                 "Migrations",  90,
                "Migraciones de base de datos para SQLAlchemy: versiona el esquema de tu BD de forma controlada.",
                "pip install alembic"),
            New(ArchitectureType.Python, "pytest",     "pytest",                  "Testing",     92,
                "El estándar de testing en Python: fixtures, parametrización y un ecosistema enorme de plugins.",
                "pip install pytest"),
            New(ArchitectureType.Python, "httpx",      "httpx",                   "HTTP",        85,
                "Cliente HTTP async/sync moderno, ideal para consumir otras APIs desde tus microservicios.",
                "pip install httpx"),

            // ── Java ─────────────────────────────────────────────────────────────
            New(ArchitectureType.Java, "Spring Boot",     "org.springframework.boot:spring-boot-starter-web",      "Framework", 100,
                "El estándar absoluto para Java empresarial. Trae el contenedor de inyección de dependencias y la estructura MVC lista para usar.",
                "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-web", FrameworkType.SpringBoot),
            New(ArchitectureType.Java, "Spring Data JPA", "org.springframework.boot:spring-boot-starter-data-jpa", "ORM",       99,
                "Facilita enormemente implementar el patrón Repository de forma casi automática con unas pocas interfaces.",
                "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-data-jpa", FrameworkType.SpringBoot),
            New(ArchitectureType.Java, "MapStruct",       "org.mapstruct:mapstruct",                               "Mapping",   91,
                "Si usas Clean Architecture o DDD, mapea datos de tu BD a DTOs automáticamente. Genera el código por ti para no escribir getters/setters a mano.",
                "mvn dependency:get -Dartifact=org.mapstruct:mapstruct:1.5.5.Final"),
            New(ArchitectureType.Java, "Lombok",          "org.projectlombok:lombok",                              "Utility",   88,
                "Elimina código repetitivo (getters, setters, constructores, builders) con anotaciones.",
                "mvn dependency:get -Dartifact=org.projectlombok:lombok"),
            New(ArchitectureType.Java, "JUnit 5",         "org.junit.jupiter:junit-jupiter",                       "Testing",   90,
                "El framework de testing moderno para Java: assertions expresivas y extensiones potentes.",
                "mvn dependency:get -Dartifact=org.junit.jupiter:junit-jupiter"),
            New(ArchitectureType.Java, "Flyway",          "org.flywaydb:flyway-core",                              "Migrations",84,
                "Migraciones de base de datos versionadas para mantener el esquema bajo control.",
                "mvn dependency:get -Dartifact=org.flywaydb:flyway-core"),

            // ── TypeScript ───────────────────────────────────────────────────────
            New(ArchitectureType.TypeScript, "Zod",                         "zod",                    "Validation",  97,
                "Tu mejor amigo. Define esquemas y valida que el JSON que recibes tenga exactamente la estructura que esperas. Si no cumple, explota antes de dañar tu interfaz.",
                "npm install zod"),
            New(ArchitectureType.TypeScript, "TanStack Query (React Query)", "@tanstack/react-query", "State/Fetch", 96,
                "Maneja el estado de carga (isLoading), los errores y la caché automáticamente al consumir tu API. Así no tienes excusa si la UI se cuelga.",
                "npm install @tanstack/react-query"),
            New(ArchitectureType.TypeScript, "InversifyJS",                 "inversify",              "DI",          85,
                "Para aplicar Inyección de Dependencias en TypeScript de forma estricta, como en un entorno corporativo.",
                "npm install inversify reflect-metadata"),
            New(ArchitectureType.TypeScript, "Prisma",                      "prisma",                 "ORM",         93,
                "ORM type-safe de nueva generación con migraciones y un cliente autogenerado a partir de tu esquema.",
                "npm install prisma @prisma/client"),
            New(ArchitectureType.TypeScript, "class-validator",             "class-validator",        "Validation",  82,
                "Validación basada en decoradores, integración perfecta con los DTOs de NestJS.",
                "npm install class-validator class-transformer"),
            New(ArchitectureType.TypeScript, "RxJS",                        "rxjs",                   "Reactive",    80,
                "Programación reactiva con observables. Base del patrón Observer y núcleo de Angular/NestJS.",
                "npm install rxjs"),
        };

        var existing = await db.Libraries.ToListAsync(ct);

        foreach (var lib in desired)
        {
            var row = existing.FirstOrDefault(e => e.Architecture == lib.Architecture && e.Name == lib.Name);
            if (row is null)
            {
                lib.CreatedAt = DateTime.UtcNow;
                db.Libraries.Add(lib);
            }
            else
            {
                row.PackageName     = lib.PackageName;
                row.Category        = lib.Category;
                row.Description     = lib.Description;
                row.InstallCommand  = lib.InstallCommand;
                row.PopularityScore = lib.PopularityScore;
                row.Framework       = lib.Framework;
                row.UpdatedAt       = DateTime.UtcNow;
            }
        }
    }

    private static LibraryRecommendation New(
        ArchitectureType arch, string name, string pkg, string category, int score,
        string desc, string installCmd, FrameworkType? framework = null) => new()
    {
        Architecture    = arch,
        Name            = name,
        PackageName     = pkg,
        Category        = category,
        PopularityScore = score,
        Description     = desc,
        InstallCommand  = installCmd,
        Framework       = framework,
    };
}
