using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.JavaScript;

public static class JavaScriptSeeder
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
        var inserted = false;

        foreach (var template in GetTemplates())
        {
            var exists = await db.Templates.AnyAsync(t =>
                t.Id == template.Id ||
                (t.Architecture == template.Architecture &&
                 t.TemplateType == template.TemplateType &&
                 t.Name == template.Name), ct);

            if (!exists)
            {
                var dbVal = template.Database.HasValue ? $"N'{template.Database}'" : "NULL";
                var infra = template.Infrastructure.HasValue ? $"N'{template.Infrastructure}'" : "NULL";
                var fw    = template.Framework.HasValue ? $"N'{template.Framework}'" : "NULL";
                var sql = $"""
                    SET IDENTITY_INSERT [Templates] ON;
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                    VALUES ({template.Id},N'{template.Architecture}',N'{template.Content.Replace("'","''")}','{template.CreatedAt:yyyy-MM-dd HH:mm:ss}',{dbVal},N'{(template.Description??"").Replace("'","''")}',{fw},{infra},1,N'{template.Name.Replace("'","''")}',N'{template.TemplateType}',{template.Version});
                    SET IDENTITY_INSERT [Templates] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sql, ct);
                inserted = true;
            }
        }

        foreach (var library in GetLibraries())
        {
            var exists = await db.Libraries.AnyAsync(l =>
                l.Id == library.Id ||
                (l.Architecture == library.Architecture &&
                 l.PackageName == library.PackageName), ct);

            if (!exists)
            {
                var fwLib = library.Framework.HasValue ? $"N'{library.Framework}'" : "NULL";
                var sqlLib = $"""
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({library.Id},N'{library.Architecture}',N'{(library.Category??"").Replace("'","''")}','{library.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(library.Description??"").Replace("'","''")}',{fwLib},N'{(library.InstallCommand??"").Replace("'","''")}',N'{library.Name.Replace("'","''")}',N'{library.PackageName.Replace("'","''")}',{library.PopularityScore});
                    SET IDENTITY_INSERT [Libraries] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sqlLib, ct);
                inserted = true;
            }
        }

        foreach (var pattern in GetDesignPatterns())
        {
            var exists = await db.DesignPatterns.AnyAsync(p =>
                p.Id == pattern.Id ||
                (p.Architecture == pattern.Architecture &&
                 p.Pattern == pattern.Pattern &&
                 p.Name == pattern.Name), ct);

            if (!exists)
            {
                var notes = (pattern.ImplementationNotes??"").Replace("'","''");
                var cmds = (pattern.ScaffoldCommandsJson??"[]").Replace("'","''");
                var sqlPat = $"""
                    SET IDENTITY_INSERT [DesignPatterns] ON;
                    INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                    VALUES ({pattern.Id},N'{pattern.Architecture}','{pattern.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(pattern.Description??"").Replace("'","''")}',N'{notes}',N'{pattern.Name.Replace("'","''")}',N'{pattern.Pattern}',N'{cmds}');
                    SET IDENTITY_INSERT [DesignPatterns] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sqlPat, ct);
                inserted = true;
            }
        }

        if (inserted)
            await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<DesignPatternEntry> GetDesignPatterns() =>
        new[]
        {
            new DesignPatternEntry
            {
                Id = 19,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.Repository,
                Name = "Repository Pattern (Node.js)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Aisla el acceso a datos detras de un contrato y un repositorio concreto para Node.js.",
                ImplementationNotes = "Crear src/domain, src/application, src/ports y src/infrastructure; registrar el repo en el contenedor de la app.",
                ScaffoldCommandsJson = "[\"mkdir -p src/domain src/application src/ports src/infrastructure\"]"
            },
            new DesignPatternEntry
            {
                Id = 20,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.CleanArchitecture,
                Name = "Clean Architecture (Node.js)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Organiza Node.js en capas para mantener la logica de negocio fuera del framework.",
                ImplementationNotes = "Separar src/domain, src/application, src/interfaces y src/infrastructure; exponer el punto de entrada en src/server.js.",
                ScaffoldCommandsJson = "[\"mkdir -p src/domain src/application src/interfaces src/infrastructure\"]"
            },
            new DesignPatternEntry
            {
                Id = 21,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.HexagonalArchitecture,
                Name = "Hexagonal Architecture (Node.js)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Usa puertos y adaptadores para aislar el dominio en aplicaciones Node.js.",
                ImplementationNotes = "Modelar puertos en src/ports y adaptadores en src/adapters; dejar el dominio libre de dependencias externas.",
                ScaffoldCommandsJson = "[\"mkdir -p src/domain src/application src/ports src/adapters\"]"
            },
            new DesignPatternEntry
            {
                Id = 22,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.CQRS,
                Name = "CQRS (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Separa comandos y consultas para simplificar la evolucion del backend Node.js.",
                ImplementationNotes = "Separar commands, queries y handlers en src/application; usar un bus ligero o funciones puras para coordinarlos.",
                ScaffoldCommandsJson = "[\"mkdir -p src/application/commands src/application/queries src/application/handlers\"]"
            },
            new DesignPatternEntry
            {
                Id = 23,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.Mediator,
                Name = "Mediator (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Centraliza la orquestacion de mensajes para reducir el acoplamiento entre handlers.",
                ImplementationNotes = "Crear un mediador liviano en src/application y separar los mensajes en src/application/messages.",
                ScaffoldCommandsJson = "[\"mkdir -p src/application src/application/messages src/application/handlers\"]"
            },
            new DesignPatternEntry
            {
                Id = 24,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.Microservices,
                Name = "Microservices (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Divide la solucion en servicios Node.js desacoplados cuando existan limites claros de dominio.",
                ImplementationNotes = "Separar src/services, src/events, src/workers y src/integrations; usar HTTP o colas para desacoplar.",
                ScaffoldCommandsJson = "[\"mkdir -p src/services src/events src/workers src/integrations\"]"
            },
            new DesignPatternEntry
            {
                Id = 25,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.CQRS,
                Name = "CQRS (NestJS)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Implementa CQRS con el paquete oficial de NestJS usando CommandBus, QueryBus y EventBus.",
                ImplementationNotes = "Crear módulos, comandos, consultas, eventos de dominio y handlers; registrar CqrsModule en el modulo raiz.",
                ScaffoldCommandsJson = "[\"npm install @nestjs/cqrs\"]"
            },
            new DesignPatternEntry
            {
                Id = 26,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.EventSourcing,
                Name = "Event Sourcing (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Estado derivado de una secuencia de eventos inmutables en Node.js.",
                ImplementationNotes = "DomainEvent base class, InMemoryEventStore, BaseAggregate con apply/pullEvents.",
                ScaffoldCommandsJson = "[\"mkdir -p src/domain/aggregates src/domain/events src/infrastructure\"]"
            },
            new DesignPatternEntry
            {
                Id = 27,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.DomainDrivenDesign,
                Name = "Domain-Driven Design (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Aggregates, Entities y Value Objects para Node.js.",
                ImplementationNotes = "BaseAggregate con domain events, ValueObject inmutable, DomainEvent con id y occurredAt.",
                ScaffoldCommandsJson = "[\"mkdir -p src/domain/aggregates src/domain/events src/domain/value-objects\"]"
            },
            new DesignPatternEntry
            {
                Id = 28,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.Saga,
                Name = "Saga (JavaScript)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Orquesta flujos de trabajo con compensacion en Node.js.",
                ImplementationNotes = "OrderSaga con pasos async y compensacion. EventBus simple para subscripcion a eventos.",
                ScaffoldCommandsJson = "[\"mkdir -p src/application/sagas\"]"
            },
            new DesignPatternEntry
            {
                Id = 29,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.MVVM,
                Name = "MVVM (JavaScript/React)",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Separacion de UI y logica con custom hooks como ViewModel.",
                ImplementationNotes = "Custom hook como ViewModel que expone estado y acciones. Componente View solo renderiza.",
                ScaffoldCommandsJson = "[\"mkdir -p src/presentation/view-models src/presentation/views\"]"
            }
        };

    private static IEnumerable<LibraryRecommendation> GetLibraries() =>
        new[]
        {
            new LibraryRecommendation
            {
                Id = 26,
                CreatedAt = SeedDate,
                Name = "dotenv",
                PackageName = "dotenv",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Category = "Configuration",
                Description = "Carga variables de entorno desde archivos .env",
                PopularityScore = 99,
                InstallCommand = "npm install dotenv"
            },
            new LibraryRecommendation
            {
                Id = 27,
                CreatedAt = SeedDate,
                Name = "Express",
                PackageName = "express",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.ExpressJs,
                Category = "HTTP Framework",
                Description = "Framework minimalista y flexible para APIs Node.js",
                PopularityScore = 98,
                InstallCommand = "npm install express"
            },
            new LibraryRecommendation
            {
                Id = 28,
                CreatedAt = SeedDate,
                Name = "Helmet",
                PackageName = "helmet",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Category = "Security",
                Description = "Cabeceras HTTP seguras para aplicaciones Node.js",
                PopularityScore = 94,
                InstallCommand = "npm install helmet"
            },
            new LibraryRecommendation
            {
                Id = 29,
                CreatedAt = SeedDate,
                Name = "Morgan",
                PackageName = "morgan",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.ExpressJs,
                Category = "Logging",
                Description = "Logger HTTP simple para Express",
                PopularityScore = 90,
                InstallCommand = "npm install morgan"
            },
            new LibraryRecommendation
            {
                Id = 30,
                CreatedAt = SeedDate,
                Name = "@nestjs/config",
                PackageName = "@nestjs/config",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NestJs,
                Category = "Configuration",
                Description = "Gestion de configuracion por entorno para NestJS",
                PopularityScore = 96,
                InstallCommand = "npm install @nestjs/config"
            },
            new LibraryRecommendation
            {
                Id = 31,
                CreatedAt = SeedDate,
                Name = "class-validator",
                PackageName = "class-validator",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NestJs,
                Category = "Validation",
                Description = "Validacion declarativa basada en decoradores",
                PopularityScore = 95,
                InstallCommand = "npm install class-validator"
            },
            new LibraryRecommendation
            {
                Id = 32,
                CreatedAt = SeedDate,
                Name = "@nestjs/swagger",
                PackageName = "@nestjs/swagger",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NestJs,
                Category = "Documentation",
                Description = "Generacion de OpenAPI y Swagger para NestJS",
                PopularityScore = 93,
                InstallCommand = "npm install @nestjs/swagger swagger-ui-express"
            },
            new LibraryRecommendation
            {
                Id = 33,
                CreatedAt = SeedDate,
                Name = "NextAuth",
                PackageName = "next-auth",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NextJs,
                Category = "Authentication",
                Description = "Autenticacion lista para Next.js",
                PopularityScore = 97,
                InstallCommand = "npm install next-auth"
            },
            new LibraryRecommendation
            {
                Id = 34,
                CreatedAt = SeedDate,
                Name = "Zod",
                PackageName = "zod",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Category = "Validation",
                Description = "Esquemas de validacion y parseo para Node y Next",
                PopularityScore = 98,
                InstallCommand = "npm install zod"
            },
            new LibraryRecommendation
            {
                Id = 35,
                CreatedAt = SeedDate,
                Name = "React Query",
                PackageName = "@tanstack/react-query",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NextJs,
                Category = "Data Fetching",
                Description = "Cache y sincronizacion de estado servidor para Next.js",
                PopularityScore = 92,
                InstallCommand = "npm install @tanstack/react-query"
            },
            new LibraryRecommendation
            {
                Id = 36,
                CreatedAt = SeedDate,
                Name = "@nestjs/cqrs",
                PackageName = "@nestjs/cqrs",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Framework = Core.Enums.FrameworkType.NestJs,
                Category = "CQRS",
                Description = "CommandBus, QueryBus y EventBus oficiales para NestJS",
                PopularityScore = 97,
                InstallCommand = "npm install @nestjs/cqrs"
            }
        };

    private static IEnumerable<ProjectTemplate> GetTemplates() =>
        new[]
        {
            new ProjectTemplate
            {
                Id = 200,
                CreatedAt = SeedDate,
                Name = "Dockerfile JavaScript",
                TemplateType = "dockerfile",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Dockerfile generico para Node.js, Express, NestJS y Next.js",
                IsActive = true,
                Version = 1,
                Content = """
FROM node:22-alpine

WORKDIR /app

COPY package*.json ./
RUN if [ -f package-lock.json ]; then npm ci --omit=dev; else npm install --omit=dev; fi

COPY . .

EXPOSE 3000

CMD ["npm", "start"]
"""
            },
            new ProjectTemplate
            {
                Id = 201,
                CreatedAt = SeedDate,
                Name = "Compose JavaScript + PostgreSQL",
                TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Database = Core.Enums.DatabaseType.PostgreSQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para Node.js con PostgreSQL",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
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
  pgdata:
"""
            },
            new ProjectTemplate
            {
                Id = 202,
                CreatedAt = SeedDate,
                Name = "Compose JavaScript + MySQL",
                TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Database = Core.Enums.DatabaseType.MySQL,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para Node.js con MySQL",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
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
  mysqldata:
"""
            },
            new ProjectTemplate
            {
                Id = 203,
                CreatedAt = SeedDate,
                Name = "Compose JavaScript + SQLite",
                TemplateType = "compose",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Database = Core.Enums.DatabaseType.SQLite,
                Infrastructure = Core.Enums.InfrastructureType.DockerCompose,
                Description = "Docker Compose para Node.js usando SQLite",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
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
  sqlite-data:
"""
            },
            new ProjectTemplate
            {
                Id = 204,
                CreatedAt = SeedDate,
                Name = "K8s JavaScript Deployment",
                TemplateType = "k8s-deployment",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Infrastructure = Core.Enums.InfrastructureType.Kubernetes,
                Description = "Kubernetes Deployment y Service para Node.js",
                IsActive = true,
                Version = 1,
                Content = """
apiVersion: apps/v1
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
  type: LoadBalancer
"""
            },
            new ProjectTemplate
            {
                Id = 205,
                CreatedAt = SeedDate,
                Name = "CI JavaScript GitHub Actions",
                TemplateType = "ci",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Pipeline CI/CD para Node.js, NestJS y Next.js",
                IsActive = true,
                Version = 1,
                Content = """
name: CI
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
          node-version: '22'
          cache: npm
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build --if-present
      - name: Test
        run: npm test --if-present
"""
            },
            new ProjectTemplate
            {
                Id = 206,
                CreatedAt = SeedDate,
                Name = "JavaScript .gitignore",
                TemplateType = "gitignore",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Gitignore base para proyectos Node.js modernos",
                IsActive = true,
                Version = 1,
                Content = """
/node_modules/
/dist/
/.next/
/.turbo/
/coverage/
/.env
/.env.*
/npm-debug.log*
/yarn-debug.log*
/pnpm-debug.log*
"""
            }
        };
}
