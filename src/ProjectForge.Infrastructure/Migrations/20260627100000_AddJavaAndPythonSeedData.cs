using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJavaAndPythonSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── Java Templates ───────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Templates",
                columns: ["Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "Version"],
                values: new object[,]
                {
                    { 900, "Java", "FROM maven:3.9-eclipse-temurin-21 AS build\nWORKDIR /app\nCOPY . .\nRUN mvn -q package -DskipTests\n\nFROM eclipse-temurin:21-jre-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/target/*.jar app.jar\nEXPOSE 8080\nENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Multi-stage Dockerfile para Spring Boot con Maven", null, null, true, "Dockerfile Java / Spring Boot", "dockerfile", 1 },
                    { 901, "Java", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: postgres\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para Spring Boot + PostgreSQL", null, "DockerCompose", true, "Compose Java + PostgreSQL", "compose", 1 },
                    { 902, "Java", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      SPRING_DATASOURCE_URL: jdbc:mysql://db:3306/{{DB_NAME}}\n      SPRING_DATASOURCE_USERNAME: root\n      SPRING_DATASOURCE_PASSWORD: secret\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Spring Boot + MySQL", null, "DockerCompose", true, "Compose Java + MySQL", "compose", 1 }
                });

            // ─── Python Templates ─────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Templates",
                columns: ["Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "Version"],
                values: new object[,]
                {
                    { 800, "Python", "FROM python:3.12-slim\nWORKDIR /app\nCOPY requirements.txt .\nRUN pip install --no-cache-dir -r requirements.txt\nCOPY . .\nEXPOSE 8000\nCMD [\"uvicorn\", \"app.main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8000\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile para FastAPI con uvicorn", null, null, true, "Dockerfile Python / FastAPI", "dockerfile", 1 },
                    { 801, "Python", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para FastAPI/Django + PostgreSQL", null, "DockerCompose", true, "Compose Python + PostgreSQL", "compose", 1 },
                    { 802, "Python", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8000\"\n    environment:\n      DATABASE_URL: mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8\n    environment:\n      MYSQL_DATABASE: {{DB_NAME}}\n      MYSQL_ROOT_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Python + MySQL", null, "DockerCompose", true, "Compose Python + MySQL", "compose", 1 }
                });

            // ─── Java Libraries ───────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 900, "Java", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM de Spring basado en JPA/Hibernate para acceso a datos relacional", null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-data-jpa</artifactId>\n</dependency>", null, "Spring Data JPA", "spring-boot-starter-data-jpa", 99 },
                    { 901, "Java", "Seguridad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticación y autorización para aplicaciones Spring", null, "<dependency>\n  <groupId>org.springframework.boot</groupId>\n  <artifactId>spring-boot-starter-security</artifactId>\n</dependency>", null, "Spring Security", "spring-boot-starter-security", 98 },
                    { 902, "Java", "Productividad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elimina código boilerplate con anotaciones (@Getter, @Builder, etc.)", null, "<dependency>\n  <groupId>org.projectlombok</groupId>\n  <artifactId>lombok</artifactId>\n  <optional>true</optional>\n</dependency>", null, "Lombok", "lombok", 95 },
                    { 903, "Java", "Mapeo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mapeo de objetos en tiempo de compilación sin reflexión", null, "<dependency>\n  <groupId>org.mapstruct</groupId>\n  <artifactId>mapstruct</artifactId>\n  <version>1.5.5.Final</version>\n</dependency>", null, "MapStruct", "mapstruct", 88 },
                    { 904, "Java", "Migraciones", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gestión de migraciones de base de datos para proyectos Java", null, "<dependency>\n  <groupId>org.flywaydb</groupId>\n  <artifactId>flyway-core</artifactId>\n</dependency>", null, "Flyway", "flyway-core", 92 },
                    { 905, "Java", "Documentación", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Genera documentación Swagger/OpenAPI automáticamente para Spring Boot", null, "<dependency>\n  <groupId>org.springdoc</groupId>\n  <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>\n  <version>2.5.0</version>\n</dependency>", null, "SpringDoc OpenAPI", "springdoc-openapi-starter-webmvc-ui", 90 }
                });

            // ─── Python extra Libraries ───────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 800, "Python", "HTTP Client", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente HTTP moderno con soporte async para Python", null, "pip install httpx", null, "httpx", "httpx", 89 },
                    { 801, "Python", "Background Tasks", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cola de tareas distribuidas para Python: ideal con Redis o RabbitMQ", null, "pip install celery", null, "Celery", "celery", 93 },
                    { 802, "Python", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing más popular del ecosistema Python", null, "pip install pytest pytest-asyncio", null, "pytest", "pytest", 99 }
                });

            // ─── Java Design Patterns ─────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 900, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Extiende JpaRepository para aislar el acceso a datos de la lógica de negocio.", "Crear interfaces en domain/repository y implementaciones en infrastructure/persistence.", null, "Repository Pattern (Spring Boot)", "Repository", "[\"mkdir -p src/main/java/domain/repository src/main/java/infrastructure/persistence\"]" },
                    { 901, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza Spring Boot en capas: domain, application, infrastructure y adapters.", "La capa domain no depende de Spring. Usar puertos e interfaces para invertir dependencias.", null, "Clean Architecture (Spring Boot)", "CleanArchitecture", "[\"mkdir -p src/main/java/domain src/main/java/application/usecase src/main/java/infrastructure src/main/java/adapters/web\"]" },
                    { 902, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa commands y queries en Spring Boot usando @Service handlers.", "Crear paquetes command/ y query/ con sus respectivos handlers y DTOs.", null, "CQRS (Spring Boot)", "CQRS", "[\"mkdir -p src/main/java/application/command src/main/java/application/query\"]" },
                    { 903, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports & Adapters para Spring Boot: el core de negocio no conoce el framework.", "Definir puertos (interfaces Java) en el dominio e implementar adaptadores en infrastructure.", null, "Hexagonal Architecture (Spring Boot)", "HexagonalArchitecture", "[\"mkdir -p src/main/java/domain/port src/main/java/application/service src/main/java/infrastructure/adapter\"]" },
                    { 904, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Estructura el proyecto alrededor de Aggregates, Entities y Value Objects de Java.", "Usar @Entity para entidades JPA y separar Domain Objects de Persistence Entities mediante mappers.", null, "Domain-Driven Design (Spring Boot)", "DomainDrivenDesign", "[\"mkdir -p src/main/java/domain/model src/main/java/domain/service src/main/java/domain/event src/main/java/application\"]" }
                });

            // ─── Python extra Design Patterns ─────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 800, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza FastAPI en capas: domain, application, infrastructure y adapters.", "La capa domain no importa FastAPI. Usar inyección de dependencias de FastAPI para conectar capas.", null, "Clean Architecture (Python/FastAPI)", "CleanArchitecture", "[\"mkdir -p app/domain app/application/use_cases app/infrastructure app/adapters/api\"]" },
                    { 801, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas con handlers explícitos en Python.", "Crear app/commands/ y app/queries/ con dataclasses para cada comando/consulta y handlers asociados.", null, "CQRS (Python/FastAPI)", "CQRS", "[\"mkdir -p app/commands app/queries app/handlers\"]" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Java Templates
            migrationBuilder.DeleteData(table: "Templates", keyColumn: "Id", keyValues: [900, 901, 902]);
            // Python Templates
            migrationBuilder.DeleteData(table: "Templates", keyColumn: "Id", keyValues: [800, 801, 802]);
            // Java Libraries
            migrationBuilder.DeleteData(table: "Libraries", keyColumn: "Id", keyValues: [900, 901, 902, 903, 904, 905]);
            // Python extra Libraries
            migrationBuilder.DeleteData(table: "Libraries", keyColumn: "Id", keyValues: [800, 801, 802]);
            // Java Patterns
            migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValues: [900, 901, 902, 903, 904]);
            // Python extra Patterns
            migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValues: [800, 801]);
        }
    }
}
