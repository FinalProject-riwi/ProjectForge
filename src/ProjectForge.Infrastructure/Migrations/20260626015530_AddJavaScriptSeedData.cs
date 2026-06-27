using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJavaScriptSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 19, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aisla el acceso a datos detras de un contrato y un repositorio concreto para Node.js.", "Crear src/domain, src/application, src/ports y src/infrastructure; registrar el repo en el contenedor de la app.", null, "Repository Pattern (Node.js)", "Repository", "[\"mkdir -p src/domain src/application src/ports src/infrastructure\"]", null },
                    { 20, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organiza Node.js en capas para mantener la logica de negocio fuera del framework.", "Separar src/domain, src/application, src/interfaces y src/infrastructure; exponer el punto de entrada en src/server.js.", null, "Clean Architecture (Node.js)", "CleanArchitecture", "[\"mkdir -p src/domain src/application src/interfaces src/infrastructure\"]", null },
                    { 21, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usa puertos y adaptadores para aislar el dominio en aplicaciones Node.js.", "Modelar puertos en src/ports y adaptadores en src/adapters; dejar el dominio libre de dependencias externas.", null, "Hexagonal Architecture (Node.js)", "HexagonalArchitecture", "[\"mkdir -p src/domain src/application src/ports src/adapters\"]", null },
                    { 22, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa comandos y consultas para simplificar la evolucion del backend Node.js.", "Separar commands, queries y handlers en src/application; usar un bus ligero o funciones puras para coordinarlos.", null, "CQRS (JavaScript)", "CQRS", "[\"mkdir -p src/application/commands src/application/queries src/application/handlers\"]", null },
                    { 23, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Centraliza la orquestacion de mensajes para reducir el acoplamiento entre handlers.", "Crear un mediador liviano en src/application y separar los mensajes en src/application/messages.", null, "Mediator (JavaScript)", "Mediator", "[\"mkdir -p src/application src/application/messages src/application/handlers\"]", null },
                    { 24, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Divide la solucion en servicios Node.js desacoplados cuando existan limites claros de dominio.", "Separar src/services, src/events, src/workers y src/integrations; usar HTTP o colas para desacoplar.", null, "Microservices (JavaScript)", "Microservices", "[\"mkdir -p src/services src/events src/workers src/integrations\"]", null }
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 26, "JavaScript", "Configuration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carga variables de entorno desde archivos .env", null, "npm install dotenv", "dotenv", "dotenv", 99, null, null },
                    { 27, "JavaScript", "HTTP Framework", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework minimalista y flexible para APIs Node.js", "ExpressJs", "npm install express", "Express", "express", 98, null, null },
                    { 28, "JavaScript", "Security", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cabeceras HTTP seguras para aplicaciones Node.js", null, "npm install helmet", "Helmet", "helmet", 94, null, null },
                    { 29, "JavaScript", "Logging", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logger HTTP simple para Express", "ExpressJs", "npm install morgan", "Morgan", "morgan", 90, null, null },
                    { 30, "JavaScript", "Configuration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gestion de configuracion por entorno para NestJS", "NestJs", "npm install @nestjs/config", "@nestjs/config", "@nestjs/config", 96, null, null },
                    { 31, "JavaScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validacion declarativa basada en decoradores", "NestJs", "npm install class-validator", "class-validator", "class-validator", 95, null, null },
                    { 32, "JavaScript", "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generacion de OpenAPI y Swagger para NestJS", "NestJs", "npm install @nestjs/swagger swagger-ui-express", "@nestjs/swagger", "@nestjs/swagger", 93, null, null },
                    { 33, "JavaScript", "Authentication", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Autenticacion lista para Next.js", "NextJs", "npm install next-auth", "NextAuth", "next-auth", 97, null, null },
                    { 34, "JavaScript", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Esquemas de validacion y parseo para Node y Next", null, "npm install zod", "Zod", "zod", 98, null, null },
                    { 35, "JavaScript", "Data Fetching", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cache y sincronizacion de estado servidor para Next.js", "NextJs", "npm install @tanstack/react-query", "React Query", "@tanstack/react-query", 92, null, null }
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "UpdatedAt", "VariablesSchemaJson", "Version" },
                values: new object[,]
                {
                    { 14, "JavaScript", "FROM node:22-alpine\n\nWORKDIR /app\n\nCOPY package*.json ./\nRUN if [ -f package-lock.json ]; then npm ci --omit=dev; else npm install --omit=dev; fi\n\nCOPY . .\n\nEXPOSE 3000\n\nCMD [\"npm\", \"start\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dockerfile generico para Node.js, Express, NestJS y Next.js", null, null, true, "Dockerfile JavaScript", "dockerfile", null, null, 1 },
                    { 15, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: postgres://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\", \"pg_isready\", \"-U\", \"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para Node.js con PostgreSQL", null, "DockerCompose", true, "Compose JavaScript + PostgreSQL", "compose", null, null, 1 },
                    { 16, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\", \"mysqladmin\", \"ping\", \"-h\", \"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para Node.js con MySQL", null, "DockerCompose", true, "Compose JavaScript + MySQL", "compose", null, null, 1 },
                    { 17, "JavaScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      NODE_ENV: development\n      PORT: 3000\n      DATABASE_URL: sqlite:///app/data/app.sqlite\n    volumes:\n      - sqlite-data:/app/data\n\nvolumes:\n  sqlite-data:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SQLite", "Docker Compose para Node.js usando SQLite", null, "DockerCompose", true, "Compose JavaScript + SQLite", "compose", null, null, 1 },
                    { 18, "JavaScript", "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n        - name: {{APP_NAME}}\n          image: {{APP_NAME}}:latest\n          ports:\n            - containerPort: 3000\n          env:\n            - name: NODE_ENV\n              value: production\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n    - port: 80\n      targetPort: 3000\n  type: LoadBalancer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kubernetes Deployment y Service para Node.js", null, "Kubernetes", true, "K8s JavaScript Deployment", "k8s-deployment", null, null, 1 },
                    { 19, "JavaScript", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\n\njobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - name: Setup Node\n        uses: actions/setup-node@v4\n        with:\n          node-version: '22'\n          cache: npm\n      - name: Install dependencies\n        run: npm ci\n      - name: Build\n        run: npm run build --if-present\n      - name: Test\n        run: npm test --if-present", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI/CD para Node.js, NestJS y Next.js", null, null, true, "CI JavaScript GitHub Actions", "ci", null, null, 1 },
                    { 20, "JavaScript", "/node_modules/\n/dist/\n/.next/\n/.turbo/\n/coverage/\n/.env\n/.env.*\n/npm-debug.log*\n/yarn-debug.log*\n/pnpm-debug.log*", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gitignore base para proyectos Node.js modernos", null, null, true, "JavaScript .gitignore", "gitignore", null, null, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[] { 6, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura de microservicios para aplicaciones Node.js.", null, null, "Microservices", "Microservices", null, null });
        }
    }
}
