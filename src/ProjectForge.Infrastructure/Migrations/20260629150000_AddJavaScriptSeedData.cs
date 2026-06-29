using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all JavaScript data: Templates, Libraries y DesignPatterns.
/// Frameworks: Node.js, Express.js, NestJS, Next.js.
/// Databases: PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite.
/// NOTA: Algunos IDs (14–20, 25–26) ya estaban en migraciones anteriores.
/// Esta migración solo inserta con IF NOT EXISTS los que puedan faltar
/// y agrega los IDs de rango 200–209 del JavaScriptSeeder.
/// </summary>
public partial class AddJavaScriptSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns JavaScript (IDs 19–28) ─────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 19)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (19,N'JavaScript','2026-01-01T00:00:00Z',N'Aisla el acceso a datos detrás de un contrato para Node.js.',N'Crear src/domain, src/application, src/ports y src/infrastructure; registrar el repo en el contenedor de la app.',N'Repository Pattern (Node.js)',N'Repository',N'["mkdir -p src/domain src/application src/ports src/infrastructure"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 20)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (20,N'JavaScript','2026-01-01T00:00:00Z',N'Organiza Node.js en capas para mantener la lógica de negocio fuera del framework.',N'Separar src/domain, src/application, src/interfaces y src/infrastructure.',N'Clean Architecture (Node.js)',N'CleanArchitecture',N'["mkdir -p src/domain src/application src/interfaces src/infrastructure"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 21)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (21,N'JavaScript','2026-01-01T00:00:00Z',N'Usa puertos y adaptadores para aislar el dominio en aplicaciones Node.js.',N'Modelar puertos en src/ports y adaptadores en src/adapters.',N'Hexagonal Architecture (Node.js)',N'HexagonalArchitecture',N'["mkdir -p src/domain src/application src/ports src/adapters"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 22)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (22,N'JavaScript','2026-01-01T00:00:00Z',N'Separa comandos y consultas para simplificar la evolución del backend Node.js.',N'Separar commands, queries y handlers en src/application.',N'CQRS (JavaScript)',N'CQRS',N'["mkdir -p src/application/commands src/application/queries src/application/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 23)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (23,N'JavaScript','2026-01-01T00:00:00Z',N'Centraliza la orquestación de mensajes para reducir el acoplamiento entre handlers.',N'Crear un mediador liviano en src/application y separar los mensajes en src/application/messages.',N'Mediator (JavaScript)',N'Mediator',N'["mkdir -p src/application src/application/messages src/application/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 24)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (24,N'JavaScript','2026-01-01T00:00:00Z',N'Divide la solución en servicios Node.js desacoplados.',N'Separar src/services, src/events, src/workers y src/integrations; usar HTTP o colas para desacoplar.',N'Microservices (JavaScript)',N'Microservices',N'["mkdir -p src/services src/events src/workers src/integrations"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 25)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (25,N'JavaScript','2026-01-01T00:00:00Z',N'Implementa CQRS con el paquete oficial de NestJS usando CommandBus, QueryBus y EventBus.',N'Crear módulos, comandos, consultas, eventos de dominio y handlers; registrar CqrsModule en el módulo raíz.',N'CQRS (NestJS)',N'CQRS',N'["npm install @nestjs/cqrs"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 26)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (26,N'JavaScript','2026-01-01T00:00:00Z',N'Estado derivado de una secuencia de eventos inmutables en Node.js.',N'DomainEvent base class, InMemoryEventStore, BaseAggregate con apply/pullEvents.',N'Event Sourcing (JavaScript)',N'EventSourcing',N'["mkdir -p src/domain/aggregates src/domain/events src/infrastructure"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 27)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (27,N'JavaScript','2026-01-01T00:00:00Z',N'Aggregates, Entities y Value Objects para Node.js.',N'BaseAggregate con domain events, ValueObject inmutable, DomainEvent con id y occurredAt.',N'Domain-Driven Design (JavaScript)',N'DomainDrivenDesign',N'["mkdir -p src/domain/aggregates src/domain/events src/domain/value-objects"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 28)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (28,N'JavaScript','2026-01-01T00:00:00Z',N'Orquesta transacciones distribuidas con compensación en Node.js.',N'Clase Saga con pasos async. Cada paso tiene compensación.',N'Saga (JavaScript)',N'Saga',N'["mkdir -p src/application/sagas"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries JavaScript (IDs 26–36) ─────────────────────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 26)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (26,N'JavaScript',N'Configuration','2026-01-01T00:00:00Z',N'Carga variables de entorno desde archivos .env',NULL,N'npm install dotenv',N'dotenv',N'dotenv',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 27)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (27,N'JavaScript',N'HTTP Framework','2026-01-01T00:00:00Z',N'Framework minimalista y flexible para APIs Node.js',N'ExpressJs',N'npm install express',N'Express',N'express',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 28)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (28,N'JavaScript',N'Security','2026-01-01T00:00:00Z',N'Cabeceras HTTP seguras para aplicaciones Node.js',NULL,N'npm install helmet',N'Helmet',N'helmet',94);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 29)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (29,N'JavaScript',N'Logging','2026-01-01T00:00:00Z',N'Logger HTTP simple para Express',N'ExpressJs',N'npm install morgan',N'Morgan',N'morgan',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 30)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (30,N'JavaScript',N'Configuration','2026-01-01T00:00:00Z',N'Gestión de configuración por entorno para NestJS',N'NestJs',N'npm install @nestjs/config',N'@nestjs/config',N'@nestjs/config',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 31)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (31,N'JavaScript',N'Validation','2026-01-01T00:00:00Z',N'Validación declarativa basada en decoradores',N'NestJs',N'npm install class-validator',N'class-validator',N'class-validator',95);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 32)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (32,N'JavaScript',N'Documentation','2026-01-01T00:00:00Z',N'Generación de OpenAPI y Swagger para NestJS',N'NestJs',N'npm install @nestjs/swagger swagger-ui-express',N'@nestjs/swagger',N'@nestjs/swagger',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 33)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (33,N'JavaScript',N'Authentication','2026-01-01T00:00:00Z',N'Autenticación lista para Next.js',N'NextJs',N'npm install next-auth',N'NextAuth',N'next-auth',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 34)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (34,N'JavaScript',N'Validation','2026-01-01T00:00:00Z',N'Esquemas de validación y parseo para Node y Next',NULL,N'npm install zod',N'Zod',N'zod',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 35)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (35,N'JavaScript',N'Data Fetching','2026-01-01T00:00:00Z',N'Cache y sincronización de estado servidor para Next.js',N'NextJs',N'npm install @tanstack/react-query',N'React Query',N'@tanstack/react-query',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 36)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (36,N'JavaScript',N'CQRS','2026-01-01T00:00:00Z',N'CommandBus, QueryBus y EventBus oficiales para NestJS',N'NestJs',N'npm install @nestjs/cqrs',N'@nestjs/cqrs',N'@nestjs/cqrs',97);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates JavaScript — todas las combinaciones de DB ──────────
        // IDs 200–209 vienen del JavaScriptSeeder actualizado
        // IDs 14–20 ya están en la migración histórica AddJavaScriptSeedData (20260626)
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Dockerfile genérico Node.js
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 200)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (200,N'JavaScript',N'FROM node:22-alpine

WORKDIR /app

COPY package*.json ./
RUN if [ -f package-lock.json ]; then npm ci --omit=dev; else npm install --omit=dev; fi

COPY . .

EXPOSE 3000

CMD ["npm", "start"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile genérico para Node.js, Express, NestJS y Next.js',NULL,NULL,1,N'Dockerfile JavaScript',N'dockerfile',1);

            -- PostgreSQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 201)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (201,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
      DATABASE_URL: postgres://postgres:secret@db:5432/{{DB_NAME}}
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Node.js con PostgreSQL',NULL,N'DockerCompose',1,N'Compose JavaScript + PostgreSQL',N'compose',1);

            -- MySQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 202)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (202,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
      DATABASE_URL: mysql://root:secret@db:3306/{{DB_NAME}}
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Node.js con MySQL',NULL,N'DockerCompose',1,N'Compose JavaScript + MySQL',N'compose',1);

            -- SQLite
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 203)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (203,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
      DATABASE_URL: sqlite:///app/data/app.sqlite
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Node.js usando SQLite',NULL,N'DockerCompose',1,N'Compose JavaScript + SQLite',N'compose',1);

            -- SQL Server
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 207)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (207,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Node.js con SQL Server',NULL,N'DockerCompose',1,N'Compose JavaScript + SQL Server',N'compose',1);

            -- MongoDB
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 208)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (208,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
      DATABASE_URL: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Node.js con MongoDB',NULL,N'DockerCompose',1,N'Compose JavaScript + MongoDB',N'compose',1);

            -- Redis
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 209)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (209,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      NODE_ENV: development
      PORT: 3000
      DATABASE_URL: postgres://postgres:secret@db:5432/{{DB_NAME}}
      REDIS_URL: redis://redis:6379
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
      test: ["CMD", "pg_isready", "-U", "postgres"]
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Node.js con PostgreSQL + Redis',NULL,N'DockerCompose',1,N'Compose JavaScript + Redis',N'compose',1);

            -- Kubernetes
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 204)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (204,N'JavaScript',N'apiVersion: apps/v1
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
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para Node.js',NULL,N'Kubernetes',1,N'K8s JavaScript Deployment',N'k8s-deployment',1);

            -- CI
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 205)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (205,N'JavaScript',N'name: CI
on:
  push:
    branches: [main]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: ''22''
          cache: npm
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build --if-present
      - name: Test
        run: npm test --if-present','2026-01-01T00:00:00Z',NULL,N'Pipeline CI/CD para Node.js, NestJS y Next.js',NULL,NULL,1,N'CI JavaScript GitHub Actions',N'ci',1);

            -- .gitignore
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 206)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (206,N'JavaScript',N'/node_modules/
/dist/
/.next/
/.turbo/
/coverage/
/.env
/.env.*
/npm-debug.log*
/yarn-debug.log*
/pnpm-debug.log*','2026-01-01T00:00:00Z',NULL,N'Gitignore base para proyectos Node.js modernos',NULL,NULL,1,N'JavaScript .gitignore',N'gitignore',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (19,20,21,22,23,24,25,26,27,28);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (26,27,28,29,30,31,32,33,34,35,36);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (200,201,202,203,204,205,206,207,208,209);");
    }
}
