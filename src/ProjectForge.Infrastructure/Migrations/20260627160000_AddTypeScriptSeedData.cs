using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1814

namespace ProjectForge.Infrastructure.Migrations
{
    public partial class AddTypeScriptSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Templates",
                columns: ["Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "Version"],
                values: new object[,]
                {
                    { 500, "TypeScript", "FROM node:22-alpine AS build\nWORKDIR /app\nCOPY package*.json .\nRUN npm ci\nCOPY . .\nRUN npm run build\n\nFROM node:22-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/dist ./dist\nCOPY --from=build /app/node_modules ./node_modules\nEXPOSE 3000\nCMD [\"node\", \"dist/main\"]", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), null, "Dockerfile multi-stage para NestJS con TypeScript", null, null, true, "Dockerfile NestJS TypeScript", "dockerfile", 1 },
                    { 501, "TypeScript", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      retries: 5\n    volumes:\n      - pgdata:/var/lib/postgresql/data\nvolumes:\n  pgdata:", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "PostgreSQL", "Docker Compose para NestJS TypeScript + PostgreSQL", null, "DockerCompose", true, "Compose NestTS + PostgreSQL", "compose", 1 },
                    { 502, "TypeScript", "name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-node@v4\n        with:\n          node-version: '22'\n          cache: npm\n      - run: npm ci\n      - run: npm run build\n      - run: npm test", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), null, "Pipeline CI para NestJS/NextJS TypeScript", null, null, true, "CI TypeScript GitHub Actions", "ci", 1 },
                    { 503, "TypeScript", "node_modules/\ndist/\n.next/\nbuild/\n.env\n.env.*\n*.log\ncoverage/\n.DS_Store", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), null, ".gitignore para proyectos TypeScript", null, null, true, "TypeScript .gitignore", "gitignore", 1 },
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: ["Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "LibraryRecommendationId", "Name", "PackageName", "PopularityScore"],
                values: new object[,]
                {
                    { 500, "TypeScript", "ORM",            new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "ORM para TypeScript con decoradores",                              null,      "npm install typeorm @nestjs/typeorm reflect-metadata", null, "TypeORM",            "typeorm",            91 },
                    { 501, "TypeScript", "ORM",            new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "ORM type-safe de última generación para TypeScript",               null,      "npm install prisma @prisma/client",                   null, "Prisma",             "prisma",             97 },
                    { 502, "TypeScript", "Validation",     new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Decoradores de validación para clases TypeScript",                 null,      "npm install class-validator class-transformer",       null, "class-validator",    "class-validator",    94 },
                    { 503, "TypeScript", "Documentation",  new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Integración OpenAPI/Swagger para NestJS",                          "NestTs",  "npm install @nestjs/swagger swagger-ui-express",      null, "@nestjs/swagger",    "@nestjs/swagger",    93 },
                    { 504, "TypeScript", "Autenticación",  new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Módulo JWT para NestJS",                                           "NestTs",  "npm install @nestjs/jwt @nestjs/passport passport passport-jwt", null, "@nestjs/jwt", "@nestjs/jwt",       92 },
                    { 505, "TypeScript", "Validation",     new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Validación de esquemas TypeScript con inferencia de tipos",        null,      "npm install zod",                                    null, "zod",                "zod",                98 },
                    { 506, "TypeScript", "Cache",          new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Cliente Redis robusto para Node.js/TypeScript",                    null,      "npm install ioredis",                                null, "ioredis",            "ioredis",            91 },
                    { 507, "TypeScript", "Testing",        new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Testing para TypeScript con cobertura",                            null,      "npm install --save-dev jest ts-jest @types/jest",     null, "jest + ts-jest",     "jest",               99 },
                    { 508, "TypeScript", "Background Jobs",new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Cola de trabajos en background basada en Redis",                   null,      "npm install bullmq @nestjs/bull",                    null, "BullMQ",             "bullmq",             89 },
                });

            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: ["Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson"],
                values: new object[,]
                {
                    { 500, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Repositorios tipados con TypeORM o Prisma en NestJS.", "Crear interfaces de repositorio en domain/. Implementar con TypeORM @InjectRepository o PrismaService.", null, "Repository Pattern (NestJS/TypeScript)", "Repository", "[\"mkdir -p src/domain/repositories src/infrastructure/repositories src/domain/entities\"]" },
                    { 501, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "CQRS con @nestjs/cqrs: CommandBus, QueryBus, EventBus.",  "Instalar @nestjs/cqrs. Crear commands/, queries/, events/ con handlers.", null, "CQRS (NestJS/TypeScript)", "CQRS", "[\"mkdir -p src/application/commands src/application/queries src/application/events src/application/handlers\"]" },
                    { 502, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Capas: domain, application, infrastructure, presentation.", "Domain puro sin dependencias de NestJS. Inyección de dependencias mediante interfaces.", null, "Clean Architecture (TypeScript)", "CleanArchitecture", "[\"mkdir -p src/domain src/application/use-cases src/infrastructure src/presentation\"]" },
                    { 503, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Ports & Adapters: interfaces TypeScript como ports.", "Ports como interfaces en core/. Adapters en infrastructure/. NestJS inyecta los adapters.", null, "Hexagonal Architecture (TypeScript)", "HexagonalArchitecture", "[\"mkdir -p src/core/ports src/core/domain src/infrastructure/adapters src/presentation\"]" },
                    { 504, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Aggregates, Entities y Value Objects con TypeScript.", "Usar clases inmutables para Value Objects. Domain Events con EventEmitter2 de NestJS.", null, "Domain-Driven Design (TypeScript)", "DomainDrivenDesign", "[\"mkdir -p src/domain/aggregates src/domain/value-objects src/domain/events src/domain/services src/application\"]" },
                    { 505, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Estado derivado de eventos con @nestjs/cqrs y EventBus.", "Usar AggregateRoot de @nestjs/cqrs. Guardar eventos en EventStore.", null, "Event Sourcing (NestJS/TypeScript)", "EventSourcing", "[\"mkdir -p src/domain/events src/infrastructure/event-store src/application/sagas\"]" },
                    { 506, "TypeScript", new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc), "Microservicios NestJS comunicándose por TCP, Redis o NATS.", "Usar @nestjs/microservices. ClientProxy para comunicación. API Gateway como entry point.", null, "Microservices (NestJS/TypeScript)", "Microservices", "[\"mkdir -p services/gateway services/users services/orders\"]" },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Templates",      keyColumn: "Id", keyValues: [500, 501, 502, 503]);
            migrationBuilder.DeleteData(table: "Libraries",      keyColumn: "Id", keyValues: [500, 501, 502, 503, 504, 505, 506, 507, 508]);
            migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValues: [500, 501, 502, 503, 504, 505, 506]);
        }
    }
}
