using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Java;

public static class JavaSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<ProjectTemplate>().HasData(GetTemplates().ToArray());
        mb.Entity<LibraryRecommendation>().HasData(GetLibraries().ToArray());
        mb.Entity<DesignPatternEntry>().HasData(GetDesignPatterns().ToArray());
    }

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await UpsertTemplatesAsync(db, ct);
        await UpsertLibrariesAsync(db, ct);
        await UpsertPatternsAsync(db, ct);
    }

    private static async Task UpsertTemplatesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var t in GetTemplates())
        {
            if (await db.Templates.AnyAsync(x => x.Id == t.Id || (x.Architecture == t.Architecture && x.TemplateType == t.TemplateType && x.Name == t.Name), ct))
                continue;
            var db2 = (t.Database.HasValue ? "N'" + t.Database.ToString() + "'" : "NULL");
            var infra = (t.Infrastructure.HasValue ? "N'" + t.Infrastructure.ToString() + "'" : "NULL");
            var fw = (t.Framework.HasValue ? "N'" + t.Framework.ToString() + "'" : "NULL");
            var sql = $"""
                SET IDENTITY_INSERT [Templates] ON;
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES ({t.Id},N'{t.Architecture}',N'{t.Content.Replace("'","''")}','{t.CreatedAt:yyyy-MM-dd HH:mm:ss}',{db2},N'{(t.Description??"").Replace("'","''")}',{fw},{infra},1,N'{t.Name.Replace("'","''")}',N'{t.TemplateType}',{t.Version});
                SET IDENTITY_INSERT [Templates] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    private static async Task UpsertLibrariesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var l in GetLibraries())
        {
            if (await db.Libraries.AnyAsync(x => x.Id == l.Id || (x.Architecture == l.Architecture && x.PackageName == l.PackageName), ct))
                continue;
            var fw = l.Framework.HasValue ? $"N'{l.Framework}'" : "NULL";
            var sql = $"""
                SET IDENTITY_INSERT [Libraries] ON;
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES ({l.Id},N'{l.Architecture}',N'{(l.Category??"").Replace("'","''")}','{l.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(l.Description??"").Replace("'","''")}',{fw},N'{(l.InstallCommand??"").Replace("'","''")}',N'{l.Name.Replace("'","''")}',N'{l.PackageName.Replace("'","''")}',{l.PopularityScore});
                SET IDENTITY_INSERT [Libraries] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    private static async Task UpsertPatternsAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var p in GetDesignPatterns())
        {
            if (await db.DesignPatterns.AnyAsync(x => x.Id == p.Id || (x.Architecture == p.Architecture && x.Pattern == p.Pattern && x.Name == p.Name), ct))
                continue;
            var notes = (p.ImplementationNotes??"").Replace("'","''");
            var cmds  = (p.ScaffoldCommandsJson??"[]").Replace("'","''");
            var sql = $"""
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES ({p.Id},N'{p.Architecture}','{p.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(p.Description??"").Replace("'","''")}',N'{notes}',N'{p.Name.Replace("'","''")}',N'{p.Pattern}',N'{cmds}');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    public static IEnumerable<ProjectTemplate> GetTemplates() => new[]
    {
        new ProjectTemplate { Id=900, CreatedAt=SeedDate, Name="Dockerfile Java / Spring Boot", TemplateType="dockerfile", Architecture=ArchitectureType.Java, Description="Multi-stage Dockerfile para Spring Boot con Maven", IsActive=true, Version=1,
            Content="FROM maven:3.9-eclipse-temurin-21 AS build\nWORKDIR /app\nCOPY . .\nRUN mvn -q package -DskipTests\n\nFROM eclipse-temurin:21-jre-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/target/*.jar app.jar\nEXPOSE 8080\nENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]" },
        new ProjectTemplate { Id=901, CreatedAt=SeedDate, Name="Compose Java + PostgreSQL", TemplateType="compose", Architecture=ArchitectureType.Java, Database=DatabaseType.PostgreSQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para Spring Boot + PostgreSQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: postgres\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  pgdata:" },
        new ProjectTemplate { Id=902, CreatedAt=SeedDate, Name="Compose Java + MySQL", TemplateType="compose", Architecture=ArchitectureType.Java, Database=DatabaseType.MySQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para Spring Boot + MySQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:mysql://db:3306/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: root\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\nvolumes:\n  mysqldata:" },
        new ProjectTemplate { Id=1100, CreatedAt=SeedDate, Name="CI Java GitHub Actions", TemplateType="ci", Architecture=ArchitectureType.Java, Description="Pipeline CI para Maven/Spring Boot", IsActive=true, Version=1,
            Content="name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-java@v4\n        with:\n          java-version: '21'\n          distribution: temurin\n          cache: maven\n      - run: mvn -B package --no-transfer-progress" },
    };

    public static IEnumerable<LibraryRecommendation> GetLibraries() => new[]
    {
        new LibraryRecommendation { Id=900, CreatedAt=SeedDate, Name="Spring Data JPA", PackageName="spring-boot-starter-data-jpa", Architecture=ArchitectureType.Java, Category="ORM", Description="ORM de Spring basado en JPA/Hibernate", PopularityScore=99, InstallCommand="<!-- pom.xml -->\n<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-jpa</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=901, CreatedAt=SeedDate, Name="Spring Security", PackageName="spring-boot-starter-security", Architecture=ArchitectureType.Java, Category="Seguridad", Description="Autenticación y autorización para Spring", PopularityScore=98, InstallCommand="<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-security</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=902, CreatedAt=SeedDate, Name="Lombok", PackageName="lombok", Architecture=ArchitectureType.Java, Category="Productividad", Description="Elimina boilerplate con anotaciones", PopularityScore=95, InstallCommand="<dependency>\n  <groupId>org.projectlombok</groupId>\n  <artifactId>lombok</artifactId>\n  <optional>true</optional>\n</dependency>" },
        new LibraryRecommendation { Id=903, CreatedAt=SeedDate, Name="MapStruct", PackageName="mapstruct", Architecture=ArchitectureType.Java, Category="Mapeo", Description="Mapeo de objetos en tiempo de compilación", PopularityScore=88, InstallCommand="<dependency>\n  <groupId>org.mapstruct</groupId>\n  <artifactId>mapstruct</artifactId>\n  <version>1.5.5.Final</version>\n</dependency>" },
        new LibraryRecommendation { Id=904, CreatedAt=SeedDate, Name="Flyway", PackageName="flyway-core", Architecture=ArchitectureType.Java, Category="Migraciones", Description="Migraciones de base de datos para Java", PopularityScore=92, InstallCommand="<dependency>\n  <groupId>org.flywaydb</groupId>\n  <artifactId>flyway-core</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=905, CreatedAt=SeedDate, Name="SpringDoc OpenAPI", PackageName="springdoc-openapi-starter-webmvc-ui", Architecture=ArchitectureType.Java, Category="Documentación", Description="Documentación Swagger automática para Spring Boot", PopularityScore=90, InstallCommand="<dependency>\n  <groupId>org.springdoc</groupId>\n  <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>\n  <version>2.5.0</version>\n</dependency>" },
        new LibraryRecommendation { Id=1100, CreatedAt=SeedDate, Name="Spring WebFlux", PackageName="spring-boot-starter-webflux", Architecture=ArchitectureType.Java, Category="Reactive", Description="Programación reactiva con Spring WebFlux", PopularityScore=85, InstallCommand="<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-webflux</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=1101, CreatedAt=SeedDate, Name="Spring Kafka", PackageName="spring-kafka", Architecture=ArchitectureType.Java, Category="Mensajería", Description="Integración con Apache Kafka", PopularityScore=87, InstallCommand="<dependency>\n  <groupId>org.springframework.kafka</groupId>\n  <artifactId>spring-kafka</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=1102, CreatedAt=SeedDate, Name="Spring Data Redis", PackageName="spring-boot-starter-data-redis", Architecture=ArchitectureType.Java, Category="Cache", Description="Integración con Redis para caching", PopularityScore=88, InstallCommand="<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-redis</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=1103, CreatedAt=SeedDate, Name="JUnit 5", PackageName="junit-5", Architecture=ArchitectureType.Java, Category="Testing", Description="Framework de testing moderno para Java", PopularityScore=99, InstallCommand="<dependency>\n  <groupId>org.junit.jupiter</groupId>\n  <artifactId>junit-jupiter</artifactId>\n  <scope>test</scope>\n</dependency>" },
        new LibraryRecommendation { Id=1104, CreatedAt=SeedDate, Name="Mockito", PackageName="mockito-core", Architecture=ArchitectureType.Java, Category="Testing", Description="Framework de mocking para tests unitarios Java", PopularityScore=97, InstallCommand="<dependency>\n  <groupId>org.mockito</groupId>\n  <artifactId>mockito-core</artifactId>\n  <scope>test</scope>\n</dependency>" },
    };

    public static IEnumerable<DesignPatternEntry> GetDesignPatterns() => new[]
    {
        new DesignPatternEntry { Id=900, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Spring Boot)", Architecture=ArchitectureType.Java, Description="JpaRepository para aislar acceso a datos.", ImplementationNotes="Extender JpaRepository<Entity, Id>. Usar @Repository en implementaciones.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence\"]" },
        new DesignPatternEntry { Id=901, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Spring Boot)", Architecture=ArchitectureType.Java, Description="Capas: domain, application, infrastructure y adapters.", ImplementationNotes="Domain sin dependencia de Spring. Usar puertos e interfaces para invertir dependencias.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters/web\"]" },
        new DesignPatternEntry { Id=902, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS (Spring Boot)", Architecture=ArchitectureType.Java, Description="Separa comandos y consultas con handlers @Service.", ImplementationNotes="Paquetes command/ y query/ con handlers. Usar ApplicationEventPublisher para eventos.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/application/command src/main/java/application/query\"]" },
        new DesignPatternEntry { Id=903, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (Spring Boot)", Architecture=ArchitectureType.Java, Description="Ports & Adapters: el negocio no conoce el framework.", ImplementationNotes="Ports como interfaces Java en domain. Adapters en infrastructure implementan los ports.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/port src/main/java/application/service src/main/java/infrastructure/adapter\"]" },
        new DesignPatternEntry { Id=904, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (Spring Boot)", Architecture=ArchitectureType.Java, Description="Aggregates, Entities y Value Objects en Java.", ImplementationNotes="Separar @Entity JPA de Domain Objects. Usar mappers para conversión.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/model src/main/java/domain/service src/main/java/domain/event src/main/java/application\"]" },
        new DesignPatternEntry { Id=1100, CreatedAt=SeedDate, Pattern=DesignPattern.EventSourcing, Name="Event Sourcing (Spring Boot)", Architecture=ArchitectureType.Java, Description="Estado derivado de eventos con ApplicationEventPublisher.", ImplementationNotes="Usar Spring Events o Axon Framework. Guardar eventos en EventStore.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/event src/main/java/application/eventhandler src/main/java/infrastructure/eventstore\"]" },
        new DesignPatternEntry { Id=1101, CreatedAt=SeedDate, Pattern=DesignPattern.Microservices, Name="Microservices (Spring Boot)", Architecture=ArchitectureType.Java, Description="Servicios independientes con Spring Cloud.", ImplementationNotes="Usar Eureka para discovery, Gateway para routing, Feign para comunicación entre servicios.", ScaffoldCommandsJson="[\"mkdir -p services/gateway services/discovery services/orders\"]" },
    };
}
