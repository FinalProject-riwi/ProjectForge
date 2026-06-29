using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all TypeScript data: Templates, Libraries y DesignPatterns.
/// Frameworks: NestJS (TS), Next.js (TS).
/// Databases: PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite.
/// </summary>
public partial class AddTypeScriptSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns TypeScript (IDs 500–509) ────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 500)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (500,N'TypeScript','2026-01-01T00:00:00Z',N'Repositorios tipados con TypeORM o Prisma en NestJS.',N'Crear interfaces de repositorio en domain/. Implementar con TypeORM @InjectRepository o PrismaService.',N'Repository Pattern (NestJS/TypeScript)',N'Repository',N'["mkdir -p src/domain/repositories src/infrastructure/repositories src/domain/entities"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 501)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (501,N'TypeScript','2026-01-01T00:00:00Z',N'CQRS con @nestjs/cqrs: CommandBus, QueryBus, EventBus.',N'Instalar @nestjs/cqrs. Crear commands/, queries/, events/ con handlers.',N'CQRS (NestJS/TypeScript)',N'CQRS',N'["mkdir -p src/application/commands src/application/queries src/application/events src/application/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 502)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (502,N'TypeScript','2026-01-01T00:00:00Z',N'Capas: domain, application, infrastructure, presentation.',N'Domain puro sin dependencias de NestJS. Inyección de dependencias mediante interfaces.',N'Clean Architecture (TypeScript)',N'CleanArchitecture',N'["mkdir -p src/domain src/application/use-cases src/infrastructure src/presentation"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 503)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (503,N'TypeScript','2026-01-01T00:00:00Z',N'Ports & Adapters: interfaces TypeScript como ports.',N'Ports como interfaces en core/. Adapters en infrastructure/. NestJS inyecta los adapters.',N'Hexagonal Architecture (TypeScript)',N'HexagonalArchitecture',N'["mkdir -p src/core/ports src/core/domain src/infrastructure/adapters src/presentation"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 504)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (504,N'TypeScript','2026-01-01T00:00:00Z',N'Aggregates, Entities y Value Objects con TypeScript.',N'Usar clases inmutables para Value Objects. Domain Events con EventEmitter2 de NestJS.',N'Domain-Driven Design (TypeScript)',N'DomainDrivenDesign',N'["mkdir -p src/domain/aggregates src/domain/value-objects src/domain/events src/domain/services src/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 505)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (505,N'TypeScript','2026-01-01T00:00:00Z',N'Estado derivado de eventos con @nestjs/cqrs y EventBus.',N'Usar AggregateRoot de @nestjs/cqrs. Guardar eventos en EventStore.',N'Event Sourcing (NestJS/TypeScript)',N'EventSourcing',N'["mkdir -p src/domain/events src/infrastructure/event-store src/application/sagas"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 506)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (506,N'TypeScript','2026-01-01T00:00:00Z',N'Microservicios NestJS comunicándose por TCP, Redis o NATS.',N'Usar @nestjs/microservices. ClientProxy para comunicación. API Gateway como entry point.',N'Microservices (NestJS/TypeScript)',N'Microservices',N'["mkdir -p services/gateway services/users services/orders"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 507)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (507,N'TypeScript','2026-01-01T00:00:00Z',N'Mediator con @nestjs/cqrs CommandBus y QueryBus.',N'Usar CommandBus y QueryBus de @nestjs/cqrs. Registrar handlers en módulo.',N'Mediator (TypeScript/NestJS)',N'Mediator',N'["mkdir -p src/application/commands src/application/queries src/application/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 508)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (508,N'TypeScript','2026-01-01T00:00:00Z',N'Sagas reactivas con @nestjs/cqrs que orquestan flujos de eventos.',N'Usar @Saga() decorator con RxJS. Escucha eventos y despacha comandos compensatorios.',N'Saga (NestJS/TypeScript)',N'Saga',N'["mkdir -p src/application/sagas"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 509)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (509,N'TypeScript','2026-01-01T00:00:00Z',N'Separación ViewModel-View con hooks o servicios de estado.',N'En React: custom hook como ViewModel. En Angular: servicio observable como ViewModel.',N'MVVM (TypeScript/React o Angular)',N'MVVM',N'["mkdir -p src/presentation/view-models src/presentation/views"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries TypeScript (IDs 500–508) ────────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 500)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (500,N'TypeScript',N'ORM','2026-01-01T00:00:00Z',N'ORM para TypeScript con decoradores',NULL,N'npm install typeorm @nestjs/typeorm reflect-metadata',N'TypeORM',N'typeorm',91);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 501)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (501,N'TypeScript',N'ORM','2026-01-01T00:00:00Z',N'ORM type-safe de última generación para TypeScript',NULL,N'npm install prisma @prisma/client',N'Prisma',N'prisma',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 502)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (502,N'TypeScript',N'Validation','2026-01-01T00:00:00Z',N'Decoradores de validación para clases TypeScript',NULL,N'npm install class-validator class-transformer',N'class-validator',N'class-validator',94);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 503)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (503,N'TypeScript',N'Documentation','2026-01-01T00:00:00Z',N'Integración OpenAPI/Swagger para NestJS',N'NestTs',N'npm install @nestjs/swagger swagger-ui-express',N'@nestjs/swagger',N'@nestjs/swagger',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 504)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (504,N'TypeScript',N'Authentication','2026-01-01T00:00:00Z',N'Módulo JWT para NestJS',N'NestTs',N'npm install @nestjs/jwt @nestjs/passport passport passport-jwt',N'@nestjs/jwt',N'@nestjs/jwt',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 505)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (505,N'TypeScript',N'Validation','2026-01-01T00:00:00Z',N'Validación de esquemas TypeScript con inferencia de tipos',NULL,N'npm install zod',N'zod',N'zod',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 506)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (506,N'TypeScript',N'Cache','2026-01-01T00:00:00Z',N'Cliente Redis robusto para Node.js/TypeScript',NULL,N'npm install ioredis',N'ioredis',N'ioredis',91);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 507)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (507,N'TypeScript',N'Testing','2026-01-01T00:00:00Z',N'Testing para TypeScript con cobertura',NULL,N'npm install --save-dev jest ts-jest @types/jest',N'jest + ts-jest',N'jest',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 508)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (508,N'TypeScript',N'Background Jobs','2026-01-01T00:00:00Z',N'Cola de trabajos en background basada en Redis',NULL,N'npm install bullmq @nestjs/bull',N'BullMQ',N'bullmq',89);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates TypeScript — todas las combinaciones de DB ───────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Dockerfile NestJS TS multi-stage
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 500)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (500,N'TypeScript',N'FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json .
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine AS final
WORKDIR /app
COPY --from=build /app/dist ./dist
COPY --from=build /app/node_modules ./node_modules
EXPOSE 3000
CMD ["node", "dist/main"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile multi-stage para NestJS con TypeScript',NULL,NULL,1,N'Dockerfile NestJS TypeScript',N'dockerfile',1);

            -- PostgreSQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 501)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (501,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
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
    healthcheck:
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      retries: 5
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para NestJS TypeScript + PostgreSQL',NULL,N'DockerCompose',1,N'Compose NestTS + PostgreSQL',N'compose',1);

            -- MySQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 510)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (510,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para NestJS TypeScript + MySQL',NULL,N'DockerCompose',1,N'Compose NestTS + MySQL',N'compose',1);

            -- SQL Server
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 511)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (511,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para NestJS TypeScript + SQL Server',NULL,N'DockerCompose',1,N'Compose NestTS + SQL Server',N'compose',1);

            -- MongoDB
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 512)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (512,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para NestJS TypeScript + MongoDB',NULL,N'DockerCompose',1,N'Compose NestTS + MongoDB',N'compose',1);

            -- Redis
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 513)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (513,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}
      REDIS_HOST: redis
      REDIS_PORT: 6379
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
      retries: 5
  redis:
    image: redis:7-alpine
    ports:
      - "{{DB_PORT}}:6379"
    volumes:
      - redis-data:/data
volumes:
  pgdata:
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para NestJS TypeScript + PostgreSQL + Redis',NULL,N'DockerCompose',1,N'Compose NestTS + Redis',N'compose',1);

            -- SQLite
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 514)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (514,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: file:/app/data/dev.db
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para NestJS TypeScript + SQLite (Prisma)',NULL,N'DockerCompose',1,N'Compose NestTS + SQLite',N'compose',1);

            -- Kubernetes
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 515)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (515,N'TypeScript',N'apiVersion: apps/v1
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
            - containerPort: 3000
          env:
            - name: NODE_ENV
              value: production
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
      targetPort: 3000
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para NestJS/TypeScript',NULL,N'Kubernetes',1,N'K8s TypeScript Deployment',N'k8s-deployment',1);

            -- CI
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 502)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (502,N'TypeScript',N'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: ''22''
          cache: npm
      - run: npm ci
      - run: npm run build
      - run: npm test','2026-01-01T00:00:00Z',NULL,N'Pipeline CI para NestJS/NextJS TypeScript',NULL,NULL,1,N'CI TypeScript GitHub Actions',N'ci',1);

            -- .gitignore
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 503)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (503,N'TypeScript',N'node_modules/
dist/
.next/
build/
.env
.env.*
*.log
coverage/
.DS_Store','2026-01-01T00:00:00Z',NULL,N'Gitignore para proyectos TypeScript',NULL,NULL,1,N'TypeScript .gitignore',N'gitignore',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (500,501,502,503,504,505,506,507,508,509);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (500,501,502,503,504,505,506,507,508);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (500,501,502,503,510,511,512,513,514,515);");
    }
}
