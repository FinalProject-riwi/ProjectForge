using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders;

public static partial class AllCombinationsSeeder
{
    // IDs 5030-5069
    private static IEnumerable<ProjectTemplate> GetJavaTemplates()
    {
        // ── Dockerfiles por framework ────────────────────────────────────────────
        yield return T(5030, "Dockerfile Java / Quarkus", "dockerfile", ArchitectureType.Java,
            framework: FrameworkType.Quarkus,
            desc: "Dockerfile multi-stage JVM para Quarkus con Maven",
            content:
            "FROM maven:3.9-eclipse-temurin-21 AS build\n" +
            "WORKDIR /app\nCOPY . .\n" +
            "RUN mvn -q package -DskipTests -Dquarkus.package.type=jar\n\n" +
            "FROM eclipse-temurin:21-jre-alpine AS final\n" +
            "WORKDIR /app\n" +
            "COPY --from=build /app/target/quarkus-app/lib/ /app/lib/\n" +
            "COPY --from=build /app/target/quarkus-app/*.jar /app/\n" +
            "COPY --from=build /app/target/quarkus-app/app/ /app/app/\n" +
            "COPY --from=build /app/target/quarkus-app/quarkus/ /app/quarkus/\n" +
            "EXPOSE 8080\n" +
            "ENTRYPOINT [\"java\",\"-jar\",\"/app/quarkus-run.jar\"]");

        yield return T(5031, "Dockerfile Java / Micronaut", "dockerfile", ArchitectureType.Java,
            framework: FrameworkType.Micronaut,
            desc: "Dockerfile multi-stage para Micronaut con Gradle",
            content:
            "FROM gradle:8-jdk21 AS build\n" +
            "WORKDIR /app\nCOPY . .\n" +
            "RUN gradle shadowJar --no-daemon -q\n\n" +
            "FROM eclipse-temurin:21-jre-alpine AS final\n" +
            "WORKDIR /app\n" +
            "COPY --from=build /app/build/libs/*-all.jar app.jar\n" +
            "EXPOSE 8080\n" +
            "ENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]");

        // ── Compose: DBs adicionales para Spring Boot ────────────────────────────
        var springConnMap = new[]
        {
            (DatabaseType.SqlServer, "SPRING_DATASOURCE_URL: jdbc:sqlserver://db:1433;databaseName={{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: sa\n      SPRING_DATASOURCE_PASSWORD: Secret1234!"),
            (DatabaseType.MongoDB,   "SPRING_DATA_MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,     "SPRING_REDIS_HOST: db\n      SPRING_REDIS_PORT: 6379"),
            (DatabaseType.SQLite,    "SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/{{DB_NAME}}.db\n      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC"),
        };
        int id = 5032;
        foreach (var (db, envBlock) in springConnMap)
        {
            var hasSeparateService = db != DatabaseType.SQLite;
            var dependsOn = hasSeparateService
                ? (db == DatabaseType.SqlServer
                    ? "    depends_on:\n      - db\n"
                    : "    depends_on:\n      db:\n        condition: service_healthy\n")
                : "";
            var volumeMount = db == DatabaseType.SQLite
                ? "    volumes:\n      - sqlitedata:/app/data\n"
                : "";
            var dbSection = hasSeparateService ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Java + {db}", "compose", ArchitectureType.Java,
                framework: FrameworkType.SpringBoot, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Spring Boot + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                $"    environment:\n      {envBlock}\n" +
                dependsOn + volumeMount + dbSection);
        }

        // ── Compose: Quarkus × DB ────────────────────────────────────────────────
        var quarkusConns = new[]
        {
            (DatabaseType.PostgreSQL, "QUARKUS_DATASOURCE_JDBC_URL: jdbc:postgresql://db:5432/{{DB_NAME}}\n      QUARKUS_DATASOURCE_USERNAME: postgres\n      QUARKUS_DATASOURCE_PASSWORD: secret"),
            (DatabaseType.MySQL,      "QUARKUS_DATASOURCE_JDBC_URL: jdbc:mysql://db:3306/{{DB_NAME}}\n      QUARKUS_DATASOURCE_USERNAME: root\n      QUARKUS_DATASOURCE_PASSWORD: secret"),
            (DatabaseType.MongoDB,    "QUARKUS_MONGODB_CONNECTION_STRING: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "QUARKUS_REDIS_HOSTS: redis://db:6379"),
            (DatabaseType.SQLite,     "QUARKUS_DATASOURCE_JDBC_URL: jdbc:sqlite:/app/data/{{DB_NAME}}.db"),
        };
        foreach (var (db, envBlock) in quarkusConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc ? (db == DatabaseType.SqlServer ? "    depends_on:\n      - db\n" : "    depends_on:\n      db:\n        condition: service_healthy\n") : "";
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app/data\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Quarkus + {db}", "compose", ArchitectureType.Java,
                framework: FrameworkType.Quarkus, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Quarkus + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                $"    environment:\n      {envBlock}\n" +
                dep + vol + dbSec);
        }

        // ── Compose: Micronaut × DB ──────────────────────────────────────────────
        var micronautConns = new[]
        {
            (DatabaseType.PostgreSQL, "DATASOURCES_DEFAULT_URL: jdbc:postgresql://db:5432/{{DB_NAME}}\n      DATASOURCES_DEFAULT_USERNAME: postgres\n      DATASOURCES_DEFAULT_PASSWORD: secret"),
            (DatabaseType.MySQL,      "DATASOURCES_DEFAULT_URL: jdbc:mysql://db:3306/{{DB_NAME}}\n      DATASOURCES_DEFAULT_USERNAME: root\n      DATASOURCES_DEFAULT_PASSWORD: secret"),
            (DatabaseType.MongoDB,    "MONGODB_URI: mongodb://admin:secret@db:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "REDIS_URI: redis://db:6379"),
            (DatabaseType.SQLite,     "DATASOURCES_DEFAULT_URL: jdbc:sqlite:/app/data/{{DB_NAME}}.db"),
        };
        foreach (var (db, envBlock) in micronautConns)
        {
            var hasSvc = db != DatabaseType.SQLite;
            var dep = hasSvc ? "    depends_on:\n      db:\n        condition: service_healthy\n" : "";
            var vol = !hasSvc ? "    volumes:\n      - sqlitedata:/app/data\n" : "";
            var dbSec = hasSvc ? DbServiceBlock(db) : "volumes:\n  sqlitedata:";

            yield return T(id++, $"Compose Micronaut + {db}", "compose", ArchitectureType.Java,
                framework: FrameworkType.Micronaut, db: db, infra: InfrastructureType.DockerCompose,
                desc: $"Docker Compose para Micronaut + {db}",
                content:
                "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n" +
                $"    environment:\n      {envBlock}\n" +
                dep + vol + dbSec);
        }

        // ── K8s por DB ───────────────────────────────────────────────────────────
        var javaK8s = new[]
        {
            (DatabaseType.PostgreSQL, "SPRING_DATASOURCE_URL", "jdbc:postgresql://postgres-svc:5432/{{DB_NAME}}"),
            (DatabaseType.MySQL,      "SPRING_DATASOURCE_URL", "jdbc:mysql://mysql-svc:3306/{{DB_NAME}}"),
            (DatabaseType.SqlServer,  "SPRING_DATASOURCE_URL", "jdbc:sqlserver://sqlserver-svc:1433;databaseName={{DB_NAME}}"),
            (DatabaseType.MongoDB,    "SPRING_DATA_MONGODB_URI", "mongodb://admin:secret@mongo-svc:27017/{{DB_NAME}}"),
            (DatabaseType.Redis,      "SPRING_REDIS_HOST", "redis-svc"),
            (DatabaseType.SQLite,     "SPRING_DATASOURCE_URL", "jdbc:sqlite:/data/{{DB_NAME}}.db"),
        };
        foreach (var (db, envKey, connStr) in javaK8s)
        {
            yield return T(id++, $"K8s Java + {db}", "k8s-deployment", ArchitectureType.Java,
                db: db, infra: InfrastructureType.Kubernetes,
                desc: $"Kubernetes Deployment para Java con {db}",
                content: K8sManifest(envKey, connStr, 8080));
        }

        // ── .gitignore ───────────────────────────────────────────────────────────
        yield return T(id++, ".gitignore Java", "gitignore", ArchitectureType.Java,
            desc: "Gitignore estándar para proyectos Java/Maven/Gradle",
            content:
            "target/\nbuild/\n.gradle/\n*.class\n*.jar\n!**/src/main/**\n!**/src/test/**\n" +
            ".idea/\n*.iml\n.vscode/\n.env\n.env.*\n*.db\nlogs/\n*.log");
    }

    // ─── Additional Java Libraries (IDs 11010-11019) ─────────────────────────────
    private static IEnumerable<LibraryRecommendation> GetJavaLibraries() => new[]
    {
        new LibraryRecommendation { Id=11010, CreatedAt=SeedDate, Name="Spring Data MongoDB", PackageName="spring-boot-starter-data-mongodb", Architecture=ArchitectureType.Java, Category="Database", Description="Integración MongoDB con Spring Boot", PopularityScore=88, InstallCommand="<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-mongodb</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=11011, CreatedAt=SeedDate, Name="Spring Boot Actuator", PackageName="spring-boot-starter-actuator", Architecture=ArchitectureType.Java, Category="Observability", Description="Endpoints de salud y métricas para Spring Boot", PopularityScore=95, InstallCommand="<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-actuator</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=11012, CreatedAt=SeedDate, Name="Spring Cloud Gateway", PackageName="spring-cloud-starter-gateway", Architecture=ArchitectureType.Java, Framework=FrameworkType.SpringBoot, Category="API Gateway", Description="API Gateway reactivo con Spring Cloud", PopularityScore=87, InstallCommand="<dependency>\n  <groupId>org.springframework.cloud</groupId>\n  <artifactId>spring-cloud-starter-gateway</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=11013, CreatedAt=SeedDate, Name="Resilience4j", PackageName="resilience4j-spring-boot3", Architecture=ArchitectureType.Java, Category="Resilience", Description="Circuit breaker y rate limiter para Spring Boot", PopularityScore=87, InstallCommand="<dependency>\n  <groupId>io.github.resilience4j</groupId>\n  <artifactId>resilience4j-spring-boot3</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=11014, CreatedAt=SeedDate, Name="Liquibase", PackageName="liquibase-core", Architecture=ArchitectureType.Java, Category="Migrations", Description="Migraciones de BD alternativa a Flyway", PopularityScore=85, InstallCommand="<dependency>\n  <groupId>org.liquibase</groupId>\n  <artifactId>liquibase-core</artifactId>\n</dependency>" },
        new LibraryRecommendation { Id=11015, CreatedAt=SeedDate, Name="Quarkus REST", PackageName="quarkus-rest-jackson", Architecture=ArchitectureType.Java, Framework=FrameworkType.Quarkus, Category="HTTP", Description="Extensión REST con Jackson para Quarkus", PopularityScore=85, InstallCommand="./mvnw quarkus:add-extension -Dextensions='rest-jackson'" },
        new LibraryRecommendation { Id=11016, CreatedAt=SeedDate, Name="Quarkus Hibernate ORM Panache", PackageName="quarkus-hibernate-orm-panache", Architecture=ArchitectureType.Java, Framework=FrameworkType.Quarkus, Category="ORM", Description="Hibernate con patrón Active Record para Quarkus", PopularityScore=84, InstallCommand="./mvnw quarkus:add-extension -Dextensions='hibernate-orm-panache'" },
        new LibraryRecommendation { Id=11017, CreatedAt=SeedDate, Name="Micronaut Data JPA", PackageName="micronaut-data-hibernate-jpa", Architecture=ArchitectureType.Java, Framework=FrameworkType.Micronaut, Category="ORM", Description="Repositorios JPA para Micronaut Data", PopularityScore=82, InstallCommand="implementation(\"io.micronaut.data:micronaut-data-hibernate-jpa\")" },
        new LibraryRecommendation { Id=11018, CreatedAt=SeedDate, Name="TestContainers", PackageName="testcontainers", Architecture=ArchitectureType.Java, Category="Testing", Description="Contenedores Docker para tests de integración", PopularityScore=93, InstallCommand="<dependency>\n  <groupId>org.testcontainers</groupId>\n  <artifactId>testcontainers</artifactId>\n  <scope>test</scope>\n</dependency>" },
        new LibraryRecommendation { Id=11019, CreatedAt=SeedDate, Name="gRPC Spring Boot", PackageName="grpc-spring-boot-starter", Architecture=ArchitectureType.Java, Category="RPC", Description="gRPC para Spring Boot", PopularityScore=83, InstallCommand="<dependency>\n  <groupId>net.devh</groupId>\n  <artifactId>grpc-server-spring-boot-starter</artifactId>\n</dependency>" },
    };

    // ─── Java patterns per framework (IDs 12020-12029) ───────────────────────────
    private static IEnumerable<DesignPatternEntry> GetJavaPatternsByFw() => new[]
    {
        new DesignPatternEntry { Id=12020, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Quarkus)", Architecture=ArchitectureType.Java, Description="Capas domain/application/infrastructure con CDI de Quarkus.", ImplementationNotes="Usar @ApplicationScoped para servicios. Quarkus CDI inyecta implementaciones de ports.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters\"]" },
        new DesignPatternEntry { Id=12021, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (Quarkus / Panache)", Architecture=ArchitectureType.Java, Description="PanacheRepository para Active Record o Repository en Quarkus.", ImplementationNotes="Extender PanacheRepository<Entity>. Inyectar con @Inject.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence\"]" },
        new DesignPatternEntry { Id=12022, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (Micronaut)", Architecture=ArchitectureType.Java, Description="Capas domain/application/infrastructure con inyección Micronaut.", ImplementationNotes="Usar @Singleton para servicios. Micronaut AOT compila DI en tiempo de compilación.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain src/main/java/application src/main/java/infrastructure\"]" },
        new DesignPatternEntry { Id=12023, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (Quarkus)", Architecture=ArchitectureType.Java, Description="Ports & Adapters usando CDI de Quarkus.", ImplementationNotes="Ports como interfaces. Adapters con @ApplicationScoped.", ScaffoldCommandsJson="[\"mkdir -p src/main/java/domain/port src/main/java/infrastructure/adapter src/main/java/application\"]" },
    };
}