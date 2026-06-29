using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

public partial class AddJavaPythonPhpSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ═══════════════════════════════════════════════════════════════════
        // JAVA — DesignPatterns (IDs 900–904, 1100–1104)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 900)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (900,N'Java','2026-01-01T00:00:00.0000000Z',N'JpaRepository para aislar acceso a datos.',N'Extender JpaRepository<Entity, Id>. Usar @Repository en implementaciones.',N'Repository Pattern (Spring Boot)',N'Repository',N'["mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 901)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (901,N'Java','2026-01-01T00:00:00.0000000Z',N'Capas: domain, application, infrastructure y adapters.',N'Domain sin dependencia de Spring. Usar puertos e interfaces para invertir dependencias.',N'Clean Architecture (Spring Boot)',N'CleanArchitecture',N'["mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters/web"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 902)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (902,N'Java','2026-01-01T00:00:00.0000000Z',N'Separa comandos y consultas con handlers @Service.',N'Paquetes command/ y query/ con handlers. Usar ApplicationEventPublisher para eventos.',N'CQRS (Spring Boot)',N'CQRS',N'["mkdir -p src/main/java/application/command src/main/java/application/query"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 903)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (903,N'Java','2026-01-01T00:00:00.0000000Z',N'Ports & Adapters: el negocio no conoce el framework.',N'Ports como interfaces Java en domain. Adapters en infrastructure implementan los ports.',N'Hexagonal Architecture (Spring Boot)',N'HexagonalArchitecture',N'["mkdir -p src/main/java/domain/port src/main/java/application/service src/main/java/infrastructure/adapter"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 904)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (904,N'Java','2026-01-01T00:00:00.0000000Z',N'Aggregates, Entities y Value Objects en Java.',N'Separar @Entity JPA de Domain Objects. Usar mappers para conversion.',N'Domain-Driven Design (Spring Boot)',N'DomainDrivenDesign',N'["mkdir -p src/main/java/domain/model src/main/java/domain/service src/main/java/domain/event src/main/java/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1100)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1100,N'Java','2026-01-01T00:00:00.0000000Z',N'Estado derivado de eventos con ApplicationEventPublisher.',N'Usar Spring Events o Axon Framework. Guardar eventos en EventStore.',N'Event Sourcing (Spring Boot)',N'EventSourcing',N'["mkdir -p src/main/java/domain/event src/main/java/application/eventhandler src/main/java/infrastructure/eventstore"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1101)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1101,N'Java','2026-01-01T00:00:00.0000000Z',N'Servicios independientes con Spring Cloud.',N'Usar Eureka para discovery, Gateway para routing, Feign para comunicacion entre servicios.',N'Microservices (Spring Boot)',N'Microservices',N'["mkdir -p services/gateway services/discovery services/orders"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1102)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1102,N'Java','2026-01-01T00:00:00.0000000Z',N'Patron Mediator con ApplicationEventPublisher o Axon.',N'Usar ApplicationEventPublisher para desacoplar componentes. Handlers anotados con @EventListener.',N'Mediator (Spring Boot)',N'Mediator',N'["mkdir -p src/main/java/application/mediator"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1103)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1103,N'Java','2026-01-01T00:00:00.0000000Z',N'Orquesta transacciones distribuidas con compensacion.',N'Implementar saga de orquestacion con @Service. Cada paso compensa los anteriores en caso de fallo.',N'Saga (Spring Boot)',N'Saga',N'["mkdir -p src/main/java/application/saga"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1104)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1104,N'Java','2026-01-01T00:00:00.0000000Z',N'Model-View-ViewModel para JavaFX o frontend desacoplado.',N'ViewModel expone ObservableValue. Usar Property Binding de JavaFX o patron Observer.',N'MVVM (Spring Boot / JavaFX)',N'MVVM',N'["mkdir -p src/main/java/presentation/viewmodel src/main/java/presentation/view"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // PYTHON — DesignPatterns (IDs 5, 800–801, 1200–1205)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 5)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (5,N'Python','2026-01-01T00:00:00.0000000Z',N'Aisla acceso a datos detras de repositorios abstractos.',N'Clase base AbstractRepository. Implementar con SQLAlchemy en infrastructure.',N'Repository Pattern (Python/FastAPI)',N'Repository',N'["mkdir -p app/repositories app/domain app/services"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 800)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (800,N'Python','2026-01-01T00:00:00.0000000Z',N'Capas: domain, application, infrastructure y adapters.',N'Domain no importa FastAPI. DI de FastAPI conecta las capas.',N'Clean Architecture (Python/FastAPI)',N'CleanArchitecture',N'["mkdir -p app/domain app/application/use_cases app/infrastructure app/adapters/api"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 801)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (801,N'Python','2026-01-01T00:00:00.0000000Z',N'Separa comandos y consultas con handlers.',N'Dataclasses para Command/Query. Handlers en application/.',N'CQRS (Python/FastAPI)',N'CQRS',N'["mkdir -p app/commands app/queries app/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1200)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1200,N'Python','2026-01-01T00:00:00.0000000Z',N'Ports como ABCs de Python, adapters como implementaciones concretas.',N'Usar ABC para ports. Inyectar adapters en los servicios de aplicacion.',N'Hexagonal Architecture (Python)',N'HexagonalArchitecture',N'["mkdir -p app/core/ports app/core/domain app/infrastructure/adapters app/api"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1201)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1201,N'Python','2026-01-01T00:00:00.0000000Z',N'Aggregates, Entities y Value Objects con dataclasses.',N'Usar @dataclass para Value Objects. Domain events con publish/subscribe.',N'Domain-Driven Design (Python)',N'DomainDrivenDesign',N'["mkdir -p app/domain/aggregates app/domain/events app/domain/value_objects app/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1202)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1202,N'Python','2026-01-01T00:00:00.0000000Z',N'Estado reconstruido desde eventos inmutables.',N'Usar EventStoreDB o PostgreSQL como event store. Proyecciones para read models.',N'Event Sourcing (Python)',N'EventSourcing',N'["mkdir -p app/domain/events app/infrastructure/event_store app/application/projections"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1203)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1203,N'Python','2026-01-01T00:00:00.0000000Z',N'Servicios independientes con FastAPI comunicandose por HTTP.',N'Cada servicio tiene su propio main.py y requirements.txt. Usar httpx para comunicacion entre servicios.',N'Microservices (Python/FastAPI)',N'Microservices',N'["mkdir -p services/items services/notifications services/gateway"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1204)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1204,N'Python','2026-01-01T00:00:00.0000000Z',N'Mediador que desacopla handlers de mensajes en Python.',N'Clase Mediator que registra handlers por tipo. Compatible con FastAPI DI.',N'Mediator (Python)',N'Mediator',N'["mkdir -p app/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1205)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1205,N'Python','2026-01-01T00:00:00.0000000Z',N'Orquesta transacciones distribuidas con compensacion.',N'Clase OrderSaga con pasos async. Cada paso tiene compensacion en caso de fallo.',N'Saga (Python/FastAPI)',N'Saga',N'["mkdir -p app/application/sagas"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // JAVA — Libraries (IDs 900–905, 1100–1104)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 900)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (900,N'Java',N'ORM','2026-01-01T00:00:00.0000000Z',N'ORM de Spring basado en JPA/Hibernate',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-data-jpa</artifactId></dependency>',N'Spring Data JPA',N'spring-boot-starter-data-jpa',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 901)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (901,N'Java',N'Seguridad','2026-01-01T00:00:00.0000000Z',N'Autenticacion y autorizacion para Spring',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-security</artifactId></dependency>',N'Spring Security',N'spring-boot-starter-security',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 902)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (902,N'Java',N'Productividad','2026-01-01T00:00:00.0000000Z',N'Elimina boilerplate con anotaciones',NULL,N'<dependency><groupId>org.projectlombok</groupId><artifactId>lombok</artifactId><optional>true</optional></dependency>',N'Lombok',N'lombok',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 903)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (903,N'Java',N'Mapeo','2026-01-01T00:00:00.0000000Z',N'Mapeo de objetos en tiempo de compilacion',NULL,N'<dependency><groupId>org.mapstruct</groupId><artifactId>mapstruct</artifactId><version>1.5.5.Final</version></dependency>',N'MapStruct',N'mapstruct',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 904)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (904,N'Java',N'Migraciones','2026-01-01T00:00:00.0000000Z',N'Migraciones de base de datos para Java',NULL,N'<dependency><groupId>org.flywaydb</groupId><artifactId>flyway-core</artifactId></dependency>',N'Flyway',N'flyway-core',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 905)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (905,N'Java',N'Documentacion','2026-01-01T00:00:00.0000000Z',N'Documentacion Swagger automatica para Spring Boot',NULL,N'<dependency><groupId>org.springdoc</groupId><artifactId>springdoc-openapi-starter-webmvc-ui</artifactId><version>2.5.0</version></dependency>',N'SpringDoc OpenAPI',N'springdoc-openapi-starter-webmvc-ui',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1100)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1100,N'Java',N'Reactive','2026-01-01T00:00:00.0000000Z',N'Programacion reactiva con Spring WebFlux',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-webflux</artifactId></dependency>',N'Spring WebFlux',N'spring-boot-starter-webflux',85);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1101)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1101,N'Java',N'Mensajeria','2026-01-01T00:00:00.0000000Z',N'Integracion con Apache Kafka',NULL,N'<dependency><groupId>org.springframework.kafka</groupId><artifactId>spring-kafka</artifactId></dependency>',N'Spring Kafka',N'spring-kafka',87);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1102)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1102,N'Java',N'Cache','2026-01-01T00:00:00.0000000Z',N'Integracion con Redis para caching',NULL,N'<dependency><groupId>org.springframework.boot</groupId><artifactId>spring-boot-starter-data-redis</artifactId></dependency>',N'Spring Data Redis',N'spring-boot-starter-data-redis',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1103)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1103,N'Java',N'Testing','2026-01-01T00:00:00.0000000Z',N'Framework de testing moderno para Java',NULL,N'<dependency><groupId>org.junit.jupiter</groupId><artifactId>junit-jupiter</artifactId><scope>test</scope></dependency>',N'JUnit 5',N'junit-5',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1104)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1104,N'Java',N'Testing','2026-01-01T00:00:00.0000000Z',N'Framework de mocking para tests unitarios Java',NULL,N'<dependency><groupId>org.mockito</groupId><artifactId>mockito-core</artifactId><scope>test</scope></dependency>',N'Mockito',N'mockito-core',97);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // PYTHON — Libraries (IDs 8–10, 800–802, 1200–1205)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 8)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (8,N'Python',N'ORM','2026-01-01T00:00:00.0000000Z',N'ORM mas popular para Python, soporta sync y async',NULL,N'pip install sqlalchemy',N'SQLAlchemy',N'sqlalchemy',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 9)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (9,N'Python',N'Validation','2026-01-01T00:00:00.0000000Z',N'Validacion de datos con type hints',NULL,N'pip install pydantic',N'Pydantic',N'pydantic',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 10)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (10,N'Python',N'Migrations','2026-01-01T00:00:00.0000000Z',N'Migraciones de base de datos para SQLAlchemy',NULL,N'pip install alembic',N'Alembic',N'alembic',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 800)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (800,N'Python',N'HTTP Client','2026-01-01T00:00:00.0000000Z',N'Cliente HTTP moderno con soporte async',NULL,N'pip install httpx',N'httpx',N'httpx',89);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 801)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (801,N'Python',N'Background Tasks','2026-01-01T00:00:00.0000000Z',N'Cola de tareas distribuidas, ideal con Redis',NULL,N'pip install celery',N'Celery',N'celery',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 802)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (802,N'Python',N'Testing','2026-01-01T00:00:00.0000000Z',N'Framework de testing mas popular de Python',NULL,N'pip install pytest pytest-asyncio',N'pytest',N'pytest',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1200)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1200,N'Python',N'Framework','2026-01-01T00:00:00.0000000Z',N'Framework web moderno y rapido para APIs',N'FastAPI',N'pip install fastapi uvicorn[standard]',N'FastAPI',N'fastapi',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1201)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1201,N'Python',N'API','2026-01-01T00:00:00.0000000Z',N'Extension de Django para crear APIs REST',N'Django',N'pip install djangorestframework',N'Django REST Framework',N'djangorestframework',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1202)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1202,N'Python',N'ORM','2026-01-01T00:00:00.0000000Z',N'Integracion de SQLAlchemy con Flask',N'Flask',N'pip install flask-sqlalchemy',N'Flask-SQLAlchemy',N'flask-sqlalchemy',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1203)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1203,N'Python',N'Cache','2026-01-01T00:00:00.0000000Z',N'Cliente Redis async para Python',NULL,N'pip install redis[asyncio]',N'aioredis',N'redis',87);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1204)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1204,N'Python',N'Autenticacion','2026-01-01T00:00:00.0000000Z',N'Manejo de tokens JWT en Python',NULL,N'pip install pyjwt',N'PyJWT',N'pyjwt',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1205)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1205,N'Python',N'Config','2026-01-01T00:00:00.0000000Z',N'Carga variables de entorno desde .env',NULL,N'pip install python-dotenv',N'Python-dotenv',N'python-dotenv',96);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // JAVA — Templates (IDs 900–902, 1100)
        // ═══════════════════════════════════════════════════════════════════

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
            ENTRYPOINT ["java","-jar","app.jar"]','2026-01-01T00:00:00.0000000Z',NULL,N'Multi-stage Dockerfile para Spring Boot con Maven',NULL,NULL,1,N'Dockerfile Java / Spring Boot',N'dockerfile',1);

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
              pgdata:','2026-01-01T00:00:00.0000000Z',N'PostgreSQL',N'Docker Compose para Spring Boot + PostgreSQL',NULL,N'DockerCompose',1,N'Compose Java + PostgreSQL',N'compose',1);

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
                image: mysql:8
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
              mysqldata:','2026-01-01T00:00:00.0000000Z',N'MySQL',N'Docker Compose para Spring Boot + MySQL',NULL,N'DockerCompose',1,N'Compose Java + MySQL',N'compose',1);

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
                  - run: mvn -B package --no-transfer-progress','2026-01-01T00:00:00.0000000Z',NULL,N'Pipeline CI para Maven/Spring Boot',NULL,NULL,1,N'CI Java GitHub Actions',N'ci',1);

                        SET IDENTITY_INSERT [Templates] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // PYTHON — Templates (IDs 800–802, 1200)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 800)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (800,N'Python',N'FROM python:3.12-slim
            WORKDIR /app
            COPY requirements.txt .
            RUN pip install --no-cache-dir -r requirements.txt
            COPY . .
            EXPOSE 8000
            CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]','2026-01-01T00:00:00.0000000Z',NULL,N'Dockerfile para FastAPI con uvicorn',NULL,NULL,1,N'Dockerfile Python / FastAPI',N'dockerfile',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 801)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (801,N'Python',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8000"
                environment:
                  DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}
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
              pgdata:','2026-01-01T00:00:00.0000000Z',N'PostgreSQL',N'Docker Compose para FastAPI/Django + PostgreSQL',NULL,N'DockerCompose',1,N'Compose Python + PostgreSQL',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 802)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (802,N'Python',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8000"
                environment:
                  DATABASE_URL: mysql+pymysql://root:secret@db:3306/{{DB_NAME}}
                depends_on:
                  db:
                    condition: service_healthy
              db:
                image: mysql:8
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
              mysqldata:','2026-01-01T00:00:00.0000000Z',N'MySQL',N'Docker Compose para Python + MySQL',NULL,N'DockerCompose',1,N'Compose Python + MySQL',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1200)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (1200,N'Python',N'name: CI
            on:
              push:
                branches: [main]
              pull_request:
            jobs:
              test:
                runs-on: ubuntu-latest
                steps:
                  - uses: actions/checkout@v4
                  - uses: actions/setup-python@v5
                    with:
                      python-version: ''3.12''
                  - run: pip install -r requirements.txt
                  - run: pytest --tb=short','2026-01-01T00:00:00.0000000Z',NULL,N'Pipeline CI para Python con pytest',NULL,NULL,1,N'CI Python GitHub Actions',N'ci',1);

                        SET IDENTITY_INSERT [Templates] OFF;
            """);

        // ═══════════════════════════════════════════════════════════════════
        // PHP — Templates (IDs 7–19)
        // ═══════════════════════════════════════════════════════════════════

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 7)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (7,N'Php',N'FROM php:8.3-cli
            WORKDIR /var/www/html
            RUN apt-get update && apt-get install -y --no-install-recommends git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev && docker-php-ext-install pdo pdo_mysql pdo_pgsql pdo_sqlite mbstring zip intl bcmath && rm -rf /var/lib/apt/lists/*
            COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
            COPY . .
            RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi && mkdir -p storage bootstrap/cache database && touch database/database.sqlite && chmod -R 775 storage bootstrap/cache database
            EXPOSE 8080
            CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]','2026-01-01T00:00:00.0000000Z',NULL,N'Dockerfile generico para apps PHP modernas como Laravel y Symfony',NULL,NULL,1,N'Dockerfile PHP',N'dockerfile',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 8)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (8,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  DB_CONNECTION: mysql
                  DB_HOST: db
                  DB_PORT: 3306
                  DB_DATABASE: {{DB_NAME}}
                  DB_USERNAME: root
                  DB_PASSWORD: secret
                depends_on:
                  db:
                    condition: service_healthy
              db:
                image: mysql:8.0
                environment:
                  MYSQL_ROOT_PASSWORD: secret
                  MYSQL_DATABASE: {{DB_NAME}}
                ports:
                  - "{{DB_PORT}}:3306"
                volumes:
                  - mysqldata:/var/lib/mysql
                healthcheck:
                  test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
                  interval: 10s
                  timeout: 5s
                  retries: 5
            volumes:
              mysqldata:','2026-01-01T00:00:00.0000000Z',N'MySQL',N'Docker Compose para PHP/Laravel o Symfony con MySQL',NULL,N'DockerCompose',1,N'Compose PHP + MySQL',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 9)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (9,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  DB_CONNECTION: pgsql
                  DB_HOST: db
                  DB_PORT: 5432
                  DB_DATABASE: {{DB_NAME}}
                  DB_USERNAME: postgres
                  DB_PASSWORD: secret
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
                  test: ["CMD", "pg_isready", "-U", "postgres"]
                  interval: 10s
                  timeout: 5s
                  retries: 5
            volumes:
              pgdata:','2026-01-01T00:00:00.0000000Z',N'PostgreSQL',N'Docker Compose para PHP/Laravel o Symfony con PostgreSQL',NULL,N'DockerCompose',1,N'Compose PHP + PostgreSQL',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (10,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  DB_CONNECTION: sqlite
                  DB_DATABASE: /var/www/html/database/database.sqlite
                volumes:
                  - sqlite-data:/var/www/html/database
            volumes:
              sqlite-data:','2026-01-01T00:00:00.0000000Z',N'SQLite',N'Docker Compose para PHP/Laravel o Symfony usando SQLite',NULL,N'DockerCompose',1,N'Compose PHP + SQLite',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 11)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (11,N'Php',N'/vendor/
            /node_modules/
            /.env
            /.env.*
            /.phpunit.result.cache
            /public/build/
            /bootstrap/cache/*.php
            /storage/app/*.sqlite
            /storage/framework/cache/*
            /storage/framework/sessions/*
            /storage/framework/testing/*
            /storage/framework/views/*
            /storage/logs/*
            /var/','2026-01-01T00:00:00.0000000Z',NULL,N'Gitignore base para proyectos PHP modernos',NULL,NULL,1,N'PHP .gitignore',N'gitignore',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 12)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (12,N'Php',N'name: CI
            on:
              push:
                branches: [main]
              pull_request:
            jobs:
              test:
                runs-on: ubuntu-latest
                steps:
                  - uses: actions/checkout@v4
                  - name: Setup PHP
                    uses: shivammathur/setup-php@v2
                    with:
                      php-version: ''8.3''
                      extensions: mbstring, xml, curl, zip, intl, pdo_sqlite
                      coverage: none
                  - name: Install dependencies
                    run: composer install --no-interaction --prefer-dist --no-progress
                  - name: Run tests
                    run: |
                      if [ -f artisan ]; then
                        php artisan test
                      elif [ -f vendor/bin/phpunit ]; then
                        vendor/bin/phpunit
                      fi','2026-01-01T00:00:00.0000000Z',NULL,N'Pipeline CI/CD para PHP con Composer y GitHub Actions',NULL,NULL,1,N'CI PHP GitHub Actions',N'ci',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 13)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (13,N'Php',N'apiVersion: apps/v1
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
              type: LoadBalancer','2026-01-01T00:00:00.0000000Z',NULL,N'Kubernetes Deployment y Service para apps PHP',NULL,N'Kubernetes',1,N'K8s PHP Deployment',N'k8s-deployment',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 14)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (14,N'Php',N'FROM php:8.3-cli
            WORKDIR /var/www/html
            RUN apt-get update && apt-get install -y --no-install-recommends git curl unzip libzip-dev libssl-dev pkg-config && pecl install mongodb && docker-php-ext-enable mongodb && docker-php-ext-install pdo mbstring zip intl bcmath && rm -rf /var/lib/apt/lists/*
            COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
            COPY . .
            RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi && mkdir -p storage bootstrap/cache && chmod -R 775 storage bootstrap/cache
            EXPOSE 8080
            CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]','2026-01-01T00:00:00.0000000Z',N'MongoDB',N'Dockerfile para PHP/Laravel o Symfony con soporte MongoDB',NULL,NULL,1,N'Dockerfile PHP + MongoDB',N'dockerfile',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 15)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (15,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  DB_CONNECTION: mongodb
                  DB_HOST: mongo
                  DB_PORT: 27017
                  DB_DATABASE: {{DB_NAME}}
                  MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
                depends_on:
                  - mongo
              mongo:
                image: mongo:7
                ports:
                  - "{{DB_PORT}}:27017"
                volumes:
                  - mongodb-data:/data/db
            volumes:
              mongodb-data:','2026-01-01T00:00:00.0000000Z',N'MongoDB',N'Docker Compose para PHP/Laravel o Symfony con MongoDB',NULL,N'DockerCompose',1,N'Compose PHP + MongoDB',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 16)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (16,N'Php',N'FROM php:8.3-cli
            WORKDIR /var/www/html
            RUN apt-get update && apt-get install -y --no-install-recommends git curl unzip libzip-dev libssl-dev pkg-config && pecl install redis && docker-php-ext-enable redis && docker-php-ext-install pdo mbstring zip intl bcmath && rm -rf /var/lib/apt/lists/*
            COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
            COPY . .
            RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi && mkdir -p storage bootstrap/cache && chmod -R 775 storage bootstrap/cache
            EXPOSE 8080
            CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]','2026-01-01T00:00:00.0000000Z',N'Redis',N'Dockerfile para PHP/Laravel o Symfony con soporte Redis',NULL,NULL,1,N'Dockerfile PHP + Redis',N'dockerfile',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 17)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (17,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  CACHE_DRIVER: redis
                  QUEUE_CONNECTION: redis
                  SESSION_DRIVER: redis
                  REDIS_HOST: redis
                  REDIS_PORT: 6379
                depends_on:
                  - redis
              redis:
                image: redis:7-alpine
                ports:
                  - "{{DB_PORT}}:6379"
                volumes:
                  - redis-data:/data
            volumes:
              redis-data:','2026-01-01T00:00:00.0000000Z',N'Redis',N'Docker Compose para PHP/Laravel o Symfony con Redis',NULL,N'DockerCompose',1,N'Compose PHP + Redis',N'compose',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 18)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (18,N'Php',N'FROM php:8.3-cli-bookworm
            WORKDIR /var/www/html
            RUN apt-get update && apt-get install -y --no-install-recommends curl gnupg unixodbc-dev libgssapi-krb5-2 libicu-dev libzip-dev libpng-dev libonig-dev libxml2-dev libssl-dev pkg-config $PHPIZE_DEPS && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /usr/share/keyrings/microsoft.gpg >/dev/null && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/microsoft-prod.list && apt-get update && ACCEPT_EULA=Y apt-get install -y msodbcsql18 && pecl install sqlsrv pdo_sqlsrv && docker-php-ext-enable sqlsrv pdo_sqlsrv && docker-php-ext-install pdo mbstring zip intl bcmath && rm -rf /var/lib/apt/lists/*
            COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
            COPY . .
            RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi && mkdir -p storage bootstrap/cache && chmod -R 775 storage bootstrap/cache
            EXPOSE 8080
            CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]','2026-01-01T00:00:00.0000000Z',N'SqlServer',N'Dockerfile para PHP/Laravel o Symfony con soporte SQL Server',NULL,NULL,1,N'Dockerfile PHP + SQL Server',N'dockerfile',1);

                        IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 19)
                            INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                            VALUES (19,N'Php',N'version: ''3.9''
            services:
              app:
                build: .
                ports:
                  - "{{APP_PORT}}:8080"
                environment:
                  DB_CONNECTION: sqlsrv
                  DB_HOST: sqlserver
                  DB_PORT: 1433
                  DB_DATABASE: {{DB_NAME}}
                  DB_USERNAME: sa
                  DB_PASSWORD: YourStrong!Passw0rd
                depends_on:
                  sqlserver:
                    condition: service_started
              sqlserver:
                image: mcr.microsoft.com/mssql/server:2022-latest
                environment:
                  ACCEPT_EULA: Y
                  MSSQL_PID: Developer
                  MSSQL_SA_PASSWORD: YourStrong!Passw0rd
                ports:
                  - "{{DB_PORT}}:1433"
                volumes:
                  - mssql-data:/var/opt/mssql
            volumes:
              mssql-data:','2026-01-01T00:00:00.0000000Z',N'SqlServer',N'Docker Compose para PHP/Laravel o Symfony con SQL Server',NULL,N'DockerCompose',1,N'Compose PHP + SQL Server',N'compose',1);

                        SET IDENTITY_INSERT [Templates] OFF;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (900,901,902,903,904,1100,1101,1102,1103,1104);");
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (5,800,801,1200,1201,1202,1203,1204,1205);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (900,901,902,903,904,905,1100,1101,1102,1103,1104);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (8,9,10,800,801,802,1200,1201,1202,1203,1204,1205);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (900,901,902,1100);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (800,801,802,1200);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (7,8,9,10,11,12,13,14,15,16,17,18,19);");
    }
}