using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all Java / Spring Boot data: Templates, Libraries y DesignPatterns.
/// Frameworks: Spring Boot 3.x, Quarkus, Micronaut.
/// Databases: PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite.
/// </summary>
public partial class AddJavaSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns Java (IDs 900–904, 1100–1104) ─────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 900)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (900,N'Java','2026-01-01T00:00:00Z',N'JpaRepository para aislar acceso a datos.',N'Extender JpaRepository<Entity, Id>. Usar @Repository en implementaciones.',N'Repository Pattern (Spring Boot)',N'Repository',N'["mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 901)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (901,N'Java','2026-01-01T00:00:00Z',N'Capas: domain, application, infrastructure y adapters.',N'Domain sin dependencia de Spring. Usar puertos e interfaces para invertir dependencias.',N'Clean Architecture (Spring Boot)',N'CleanArchitecture',N'["mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters/web"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 902)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (902,N'Java','2026-01-01T00:00:00Z',N'Separa comandos y consultas con handlers @Service.',N'Paquetes command/ y query/ con handlers. Usar ApplicationEventPublisher para eventos.',N'CQRS (Spring Boot)',N'CQRS',N'["mkdir -p src/main/java/application/command src/main/java/application/query"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 903)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (903,N'Java','2026-01-01T00:00:00Z',N'Ports & Adapters: el negocio no conoce el framework.',N'Ports como interfaces Java en domain. Adapters en infrastructure implementan los ports.',N'Hexagonal Architecture (Spring Boot)',N'HexagonalArchitecture',N'["mkdir -p src/main/java/domain/port src/main/java/application/service src/main/java/infrastructure/adapter"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 904)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (904,N'Java','2026-01-01T00:00:00Z',N'Aggregates, Entities y Value Objects en Java.',N'Separar @Entity JPA de Domain Objects. Usar mappers para conversión.',N'Domain-Driven Design (Spring Boot)',N'DomainDrivenDesign',N'["mkdir -p src/main/java/domain/model src/main/java/domain/service src/main/java/domain/event src/main/java/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1100)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1100,N'Java','2026-01-01T00:00:00Z',N'Estado derivado de eventos con ApplicationEventPublisher.',N'Usar Spring Events o Axon Framework. Guardar eventos en EventStore.',N'Event Sourcing (Spring Boot)',N'EventSourcing',N'["mkdir -p src/main/java/domain/event src/main/java/application/eventhandler src/main/java/infrastructure/eventstore"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1101)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1101,N'Java','2026-01-01T00:00:00Z',N'Servicios independientes con Spring Cloud.',N'Usar Eureka para discovery, Gateway para routing, Feign para comunicación entre servicios.',N'Microservices (Spring Boot)',N'Microservices',N'["mkdir -p services/gateway services/discovery services/orders"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1102)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1102,N'Java','2026-01-01T00:00:00Z',N'Patrón Mediator con ApplicationEventPublisher o Axon.',N'Usar ApplicationEventPublisher para desacoplar componentes. Handlers anotados con @EventListener.',N'Mediator (Spring Boot)',N'Mediator',N'["mkdir -p src/main/java/application/mediator"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1103)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1103,N'Java','2026-01-01T00:00:00Z',N'Orquesta transacciones distribuidas con compensación.',N'Implementar saga de orquestación con @Service. Cada paso compensa los anteriores en caso de fallo.',N'Saga (Spring Boot)',N'Saga',N'["mkdir -p src/main/java/application/saga"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1104)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1104,N'Java','2026-01-01T00:00:00Z',N'Model-View-ViewModel para JavaFX o frontend desacoplado.',N'ViewModel expone ObservableValue. Usar Property Binding de JavaFX o patrón Observer.',N'MVVM (Spring Boot / JavaFX)',N'MVVM',N'["mkdir -p src/main/java/presentation/viewmodel src/main/java/presentation/view"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries Java (IDs 900–905, 1100–1104) ──────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 900)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (900,N'Java',N'ORM','2026-01-01T00:00:00Z',N'ORM de Spring basado en JPA/Hibernate',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-data-jpa</artifactId></dependency>',N'Spring Data JPA',N'spring-boot-starter-data-jpa',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 901)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (901,N'Java',N'Security','2026-01-01T00:00:00Z',N'Autenticación y autorización para Spring',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-security</artifactId></dependency>',N'Spring Security',N'spring-boot-starter-security',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 902)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (902,N'Java',N'Productividad','2026-01-01T00:00:00Z',N'Elimina boilerplate con anotaciones',NULL,N'<dependency><groupId>org.projectlombok</groupId><artifactId>lombok</artifactId><optional>true</optional></dependency>',N'Lombok',N'lombok',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 903)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (903,N'Java',N'Mapping','2026-01-01T00:00:00Z',N'Mapeo de objetos en tiempo de compilación',NULL,N'<dependency><groupId>org.mapstruct</groupId><artifactId>mapstruct</artifactId><version>1.5.5.Final</version></dependency>',N'MapStruct',N'mapstruct',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 904)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (904,N'Java',N'Migrations','2026-01-01T00:00:00Z',N'Migraciones de base de datos para Java',NULL,N'<dependency><groupId>org.flywaydb</groupId><artifactId>flyway-core</artifactId></dependency>',N'Flyway',N'flyway-core',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 905)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (905,N'Java',N'Documentation','2026-01-01T00:00:00Z',N'Documentación Swagger automática para Spring Boot',NULL,N'<dependency><groupId>org.springdoc</groupId><artifactId>springdoc-openapi-starter-webmvc-ui</artifactId><version>2.5.0</version></dependency>',N'SpringDoc OpenAPI',N'springdoc-openapi-starter-webmvc-ui',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1100)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1100,N'Java',N'Reactive','2026-01-01T00:00:00Z',N'Programación reactiva con Spring WebFlux',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-webflux</artifactId></dependency>',N'Spring WebFlux',N'spring-boot-starter-webflux',85);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1101)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1101,N'Java',N'Messaging','2026-01-01T00:00:00Z',N'Integración con Apache Kafka',NULL,N'<dependency><groupId>org.springframework.kafka</groupId><artifactId>spring-kafka</artifactId></dependency>',N'Spring Kafka',N'spring-kafka',87);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1102)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1102,N'Java',N'Cache','2026-01-01T00:00:00Z',N'Integración con Redis para caching',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-data-redis</artifactId></dependency>',N'Spring Data Redis',N'spring-boot-starter-data-redis',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1103)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1103,N'Java',N'Testing','2026-01-01T00:00:00Z',N'Framework de testing moderno para Java',NULL,N'<dependency><groupId>org.junit.jupiter</groupId><artifactId>junit-jupiter</artifactId><scope>test</scope></dependency>',N'JUnit 5',N'junit-5',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1104)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1104,N'Java',N'Testing','2026-01-01T00:00:00Z',N'Framework de mocking para tests unitarios Java',NULL,N'<dependency><groupId>org.mockito</groupId><artifactId>mockito-core</artifactId><scope>test</scope></dependency>',N'Mockito',N'mockito-core',97);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates Java (IDs 900–902, 1100–1106) ──────────────────────
        // Completa todas las combinaciones de DB que faltaban
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 900)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (900,N'Java',N'FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /app
COPY . .
RUN mvn -q package -DskipTests

FROM eclipse-temurin:21-jre-alpine AS final
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
EXPOSE 8080
ENTRYPOINT ["java","-jar","app.jar"]','2026-01-01T00:00:00Z',NULL,N'Multi-stage Dockerfile para Spring Boot con Maven',NULL,NULL,1,N'Dockerfile Java / Spring Boot',N'dockerfile',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 901)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (901,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: postgres
      SPRING_DATASOURCE_PASSWORD: secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Spring Boot + PostgreSQL',NULL,N'DockerCompose',1,N'Compose Java + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 902)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (902,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:mysql://db:3306/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: root
      SPRING_DATASOURCE_PASSWORD: secret
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Spring Boot + MySQL',NULL,N'DockerCompose',1,N'Compose Java + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1100)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1100,N'Java',N'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-java@v4
        with:
          java-version: ''21''
          distribution: temurin
          cache: maven
      - run: mvn -B package --no-transfer-progress','2026-01-01T00:00:00Z',NULL,N'Pipeline CI para Maven/Spring Boot',NULL,NULL,1,N'CI Java GitHub Actions',N'ci',1);

            -- SQL Server
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1101)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1101,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlserver://db:1433;databaseName={{DB_NAME}};encrypt=false
      SPRING_DATASOURCE_USERNAME: sa
      SPRING_DATASOURCE_PASSWORD: YourStrong!Passw0rd
    depends_on:
      - db
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd
    ports:
      - "{{DB_PORT}}:1433"
    volumes:
      - mssql-data:/var/opt/mssql
volumes:
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Spring Boot + SQL Server',NULL,N'DockerCompose',1,N'Compose Java + SQL Server',N'compose',1);

            -- MongoDB
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1102)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1102,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATA_MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Spring Boot + MongoDB',NULL,N'DockerCompose',1,N'Compose Java + MongoDB',N'compose',1);

            -- Redis
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1103)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1103,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: postgres
      SPRING_DATASOURCE_PASSWORD: secret
      SPRING_REDIS_HOST: redis
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Spring Boot + PostgreSQL + Redis',NULL,N'DockerCompose',1,N'Compose Java + Redis',N'compose',1);

            -- SQLite (H2 en prod no es recomendable, pero se cubre para dev)
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1104)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1104,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/app.db
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC
      SPRING_JPA_DATABASE_PLATFORM: org.hibernate.community.dialect.SQLiteDialect
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Spring Boot + SQLite',NULL,N'DockerCompose',1,N'Compose Java + SQLite',N'compose',1);

            -- Kubernetes
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1105)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1105,N'Java',N'apiVersion: apps/v1
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
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para Spring Boot',NULL,N'Kubernetes',1,N'K8s Java Deployment',N'k8s-deployment',1);

            -- .gitignore
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1106)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1106,N'Java',N'target/
*.class
*.jar
*.war
*.ear
.mvn/
.idea/
*.iml
.classpath
.project
.settings/
.env
.env.*
*.log','2026-01-01T00:00:00Z',NULL,N'Gitignore para proyectos Java/Maven',NULL,NULL,1,N'.gitignore Java',N'gitignore',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (900,901,902,903,904,1100,1101,1102,1103,1104);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (900,901,902,903,904,905,1100,1101,1102,1103,1104);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (900,901,902,1100,1101,1102,1103,1104,1105,1106);");
    }
}
