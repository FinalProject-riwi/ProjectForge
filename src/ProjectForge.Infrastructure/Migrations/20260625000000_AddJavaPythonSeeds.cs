using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJavaPythonSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Patrones Java ─────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 7,  "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patrón de repositorio con Spring Data JPA para Java.",                                  null, null, "Repository Pattern",     "Repository",           null, null },
                    { 8,  "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separación de comandos y consultas en aplicaciones Spring Boot.",                       null, null, "CQRS",                   "CQRS",                 null, null },
                    { 9,  "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ports & Adapters: aísla el dominio de la infraestructura. Muy usado en Java empresarial.", null, null, "Hexagonal Architecture", "HexagonalArchitecture", null, null },
                    { 10, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura de microservicios con Spring Boot y Spring Cloud.",                         null, null, "Microservices",          "Microservices",         null, null },
                    // Python extra
                    { 11, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capas bien definidas (domain, application, infrastructure) para proyectos Python.",   null, null, "Clean Architecture",     "CleanArchitecture",     null, null }
                });

            // ── Librerías Python extra + Java ─────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    // Python extra
                    { 11, "Python", "Testing",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing para Python",               null,          "pip install pytest",                                                       "pytest",              "pytest",                                              98, null, null },
                    { 12, "Python", "HTTP Client",new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente HTTP async para tests e integraciones",  null,          "pip install httpx",                                                        "httpx",               "httpx",                                               88, null, null },
                    { 13, "Python", "Config",     new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carga variables de entorno desde .env",           null,          "pip install python-dotenv",                                                "python-dotenv",       "python-dotenv",                                       95, null, null },
                    // Java
                    { 14, "Java",   "ORM",        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Repositorios JPA con Spring Data",               "SpringBoot",  "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-data-jpa", "Spring Data JPA",  "org.springframework.boot:spring-boot-starter-data-jpa",  99, null, null },
                    { 15, "Java",   "Security",   new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticación y autorización para Spring Boot",  "SpringBoot",  "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-security",  "Spring Security",  "org.springframework.boot:spring-boot-starter-security",  97, null, null },
                    { 16, "Java",   "Mapping",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mapeo entre objetos Java en tiempo de compilación", null,        "mvn dependency:get -Dartifact=org.mapstruct:mapstruct:1.5.5.Final",       "MapStruct",          "org.mapstruct:mapstruct",                               91, null, null },
                    { 17, "Java",   "Boilerplate",new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reduce boilerplate con anotaciones (@Getter, @Builder...)", null, "mvn dependency:get -Dartifact=org.projectlombok:lombok:1.18.32",          "Lombok",             "org.projectlombok:lombok",                              98, null, null },
                    { 18, "Java",   "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Swagger UI / OpenAPI 3 para Spring Boot 3",  "SpringBoot",  "mvn dependency:get -Dartifact=org.springdoc:springdoc-openapi-starter-webmvc-ui:2.5.0", "springdoc-openapi", "org.springdoc:springdoc-openapi-starter-webmvc-ui", 93, null, null },
                    { 19, "Java",   "Testing",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing unitario para Java",         null,          "mvn dependency:get -Dartifact=org.junit.jupiter:junit-jupiter:5.10.0",   "JUnit 5",            "org.junit.jupiter:junit-jupiter",                       99, null, null },
                    { 20, "Java",   "Migrations", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Migraciones de base de datos para Java",          null,          "mvn dependency:get -Dartifact=org.flywaydb:flyway-core:10.0.0",          "Flyway",             "org.flywaydb:flyway-core",                              92, null, null }
                });

            // ── Templates Java ────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "UpdatedAt", "VariablesSchemaJson", "Version" },
                values: new object[,]
                {
                    {
                        7, "Java",
                        "FROM eclipse-temurin:21-jdk-alpine AS build\nWORKDIR /app\nCOPY . .\nRUN ./mvnw -q package -DskipTests\n\nFROM eclipse-temurin:21-jre-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/target/*.jar app.jar\nEXPOSE 8080\nENTRYPOINT [\"java\",\"-jar\",\"app.jar\"]",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Multi-stage Dockerfile para Spring Boot con Maven", null, null, true, "Dockerfile Java Spring Boot", "dockerfile", null, null, 1
                    },
                    {
                        8, "Java",
                        "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - SPRING_DATASOURCE_URL=jdbc:postgresql://db:5432/{{DB_NAME}}\n      - SPRING_DATASOURCE_USERNAME=postgres\n      - SPRING_DATASOURCE_PASSWORD=secret\n      - SPRING_JPA_HIBERNATE_DDL_AUTO=update\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "PostgreSQL", "Docker Compose para Spring Boot + PostgreSQL", null, "DockerCompose", true, "Compose Java + PostgreSQL", "compose", null, null, 1
                    },
                    {
                        9, "Java",
                        "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - SPRING_DATASOURCE_URL=jdbc:mysql://db:3306/{{DB_NAME}}\n      - SPRING_DATASOURCE_USERNAME=root\n      - SPRING_DATASOURCE_PASSWORD=secret\n      - SPRING_JPA_HIBERNATE_DDL_AUTO=update\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "MySQL", "Docker Compose para Spring Boot + MySQL", null, "DockerCompose", true, "Compose Java + MySQL", "compose", null, null, 1
                    },
                    {
                        10, "Java",
                        "target/\n*.class\n*.jar\n*.war\n*.ear\n.mvn/\n!.mvn/wrapper/\n.idea/\n*.iml\n.vscode/\n.env\n.env.*\n*.log\nspring-shell.log\nmvnw\n!mvnw\nmvnw.cmd\n!mvnw.cmd",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Gitignore para proyectos Java/Maven/Spring Boot", null, null, true, ".gitignore Java", "gitignore", null, null, 1
                    },
                    {
                        11, "Java",
                        "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v4\n    - uses: actions/setup-java@v4\n      with:\n        java-version: '21'\n        distribution: 'temurin'\n        cache: maven\n    - run: ./mvnw -q verify\n    - name: Build Docker image\n      run: docker build -t {{APP_NAME}}:latest .",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Pipeline CI/CD para Java + Maven con GitHub Actions", null, null, true, "CI Java GitHub Actions", "ci", null, null, 1
                    },
                    // ── Templates Python ──────────────────────────────────────
                    {
                        12, "Python",
                        "FROM python:3.12-slim AS base\nWORKDIR /app\n\nCOPY requirements.txt .\nRUN pip install --no-cache-dir -r requirements.txt\n\nCOPY . .\nEXPOSE 8080\n\nCMD [\"uvicorn\", \"main:app\", \"--host\", \"0.0.0.0\", \"--port\", \"8080\"]",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Dockerfile para FastAPI/Flask con Python 3.12", null, null, true, "Dockerfile Python FastAPI", "dockerfile", null, null, 1
                    },
                    {
                        13, "Python",
                        "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - DATABASE_URL=postgresql://postgres:secret@db:5432/{{DB_NAME}}\n      - DEBUG=False\n    depends_on:\n      db:\n        condition: service_healthy\n    volumes:\n      - .:/app\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "PostgreSQL", "Docker Compose para FastAPI/Django + PostgreSQL", null, "DockerCompose", true, "Compose Python + PostgreSQL", "compose", null, null, 1
                    },
                    {
                        14, "Python",
                        "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - DATABASE_URL=mysql+pymysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n    volumes:\n      - .:/app\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "MySQL", "Docker Compose para FastAPI/Django + MySQL", null, "DockerCompose", true, "Compose Python + MySQL", "compose", null, null, 1
                    },
                    {
                        15, "Python",
                        "__pycache__/\n*.py[cod]\n*.pyo\n.env\n.env.*\nvenv/\n.venv/\n*.egg-info/\ndist/\nbuild/\n.pytest_cache/\n.mypy_cache/\n.coverage\nhtmlcov/\n*.sqlite3\n.vscode/\n.idea/",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Gitignore para proyectos Python", null, null, true, ".gitignore Python", "gitignore", null, null, 1
                    },
                    {
                        16, "Python",
                        "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v4\n    - uses: actions/setup-python@v5\n      with:\n        python-version: '3.12'\n        cache: pip\n    - run: pip install -r requirements.txt\n    - run: python -m pytest tests/ -v\n    - name: Build Docker image\n      run: docker build -t {{APP_NAME}}:latest .",
                        new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        null, "Pipeline CI/CD para Python con GitHub Actions", null, null, true, "CI Python GitHub Actions", "ci", null, null, 1
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar templates Java (7-11) y Python (12-16)
            for (int i = 7; i <= 16; i++)
                migrationBuilder.DeleteData(table: "Templates", keyColumn: "Id", keyValue: i);

            // Eliminar librerías Python extra (11-13) y Java (14-20)
            for (int i = 11; i <= 20; i++)
                migrationBuilder.DeleteData(table: "Libraries", keyColumn: "Id", keyValue: i);

            // Eliminar patrones Java (7-10) y Python extra (11)
            for (int i = 7; i <= 11; i++)
                migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValue: i);
        }
    }
}
