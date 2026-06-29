using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.TypeScript;

/// <summary>TypeScript ecosystem seeder (NestTS, NextTS, Angular)</summary>
public static class TypeScriptSeeder
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
        await UpsertTemplatesAsync(db, ct);
        await UpsertLibrariesAsync(db, ct);
        await UpsertPatternsAsync(db, ct);
    }

    private static async Task UpsertTemplatesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var t in GetTemplates())
        {
            if (await db.Templates.AnyAsync(x => x.Id == t.Id || (x.Architecture == t.Architecture && x.TemplateType == t.TemplateType && x.Name == t.Name), ct))
                continue;
            var sql = $"""
                SET IDENTITY_INSERT [Templates] ON;
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Description],[IsActive],[Name],[TemplateType],[Version])
                VALUES ({t.Id},N'{t.Architecture}',N'{t.Content.Replace("'","''").Replace("{","{{").Replace("}","}}")}','{t.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(t.Description ?? "").Replace("'","''")}',1,N'{t.Name.Replace("'","''")}',N'{t.TemplateType}',{t.Version});
                SET IDENTITY_INSERT [Templates] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
        }
    }

    private static async Task UpsertLibrariesAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var l in GetLibraries())
        {
            if (await db.Libraries.AnyAsync(x => x.Id == l.Id || (x.Architecture == l.Architecture && x.PackageName == l.PackageName), ct))
                continue;
            var fw = l.Framework.HasValue ? $"N'{l.Framework}'" : "NULL";
            var sql = $"""
                SET IDENTITY_INSERT [Libraries] ON;
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES ({l.Id},N'{l.Architecture}',N'{(l.Category ?? "").Replace("'","''")}','{l.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(l.Description ?? "").Replace("'","''")}',{fw},N'{(l.InstallCommand ?? "").Replace("'","''")}',N'{l.Name.Replace("'","''")}',N'{l.PackageName.Replace("'","''")}',{l.PopularityScore});
                SET IDENTITY_INSERT [Libraries] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
        }
    }

    private static async Task UpsertPatternsAsync(AppDbContext db, CancellationToken ct)
    {
        foreach (var p in GetDesignPatterns())
        {
            if (await db.DesignPatterns.AnyAsync(x => x.Id == p.Id || (x.Architecture == p.Architecture && x.Pattern == p.Pattern && x.Name == p.Name), ct))
                continue;
            var notes = (p.ImplementationNotes ?? "").Replace("'","''");
            var cmds = (p.ScaffoldCommandsJson ?? "[]").Replace("'","''");
            var sql = $"""
                SET IDENTITY_INSERT [DesignPatterns] ON;
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES ({p.Id},N'{p.Architecture}','{p.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(p.Description ?? "").Replace("'","''")}',N'{notes}',N'{p.Name.Replace("'","''")}',N'{p.Pattern}',N'{cmds}');
                SET IDENTITY_INSERT [DesignPatterns] OFF;
                """;
            await db.Database.ExecuteSqlRawAsync(sql, Array.Empty<object>());
        }
    }

    public static IEnumerable<ProjectTemplate> GetTemplates() => new[]
    {
        new ProjectTemplate { Id=500, CreatedAt=SeedDate, Name="Dockerfile NestJS TypeScript", TemplateType="dockerfile", Architecture=ArchitectureType.TypeScript, Description="Dockerfile multi-stage para NestJS con TypeScript", IsActive=true, Version=1,
            Content="FROM node:22-alpine AS build\nWORKDIR /app\nCOPY package*.json .\nRUN npm ci\nCOPY . .\nRUN npm run build\n\nFROM node:22-alpine AS final\nWORKDIR /app\nCOPY --from=build /app/dist ./dist\nCOPY --from=build /app/node_modules ./node_modules\nEXPOSE 3000\nCMD [\"node\", \"dist/main\"]" },
        new ProjectTemplate { Id=501, CreatedAt=SeedDate, Name="Compose NestTS + PostgreSQL", TemplateType="compose", Architecture=ArchitectureType.TypeScript, Database=DatabaseType.PostgreSQL, Infrastructure=InfrastructureType.DockerCompose, Description="Docker Compose para NestJS TypeScript + PostgreSQL", IsActive=true, Version=1,
            Content="version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:3000\"\n    environment:\n      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}\n    depends_on:\n      db:\n        condition: service_healthy\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      retries: 5\n    volumes:\n      - pgdata:/var/lib/postgresql/data\nvolumes:\n  pgdata:" },
        new ProjectTemplate { Id=502, CreatedAt=SeedDate, Name="CI TypeScript GitHub Actions", TemplateType="ci", Architecture=ArchitectureType.TypeScript, Description="Pipeline CI para NestJS/NextJS TypeScript", IsActive=true, Version=1,
            Content="name: CI\non:\n  push:\n    branches: [main]\n  pull_request:\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-node@v4\n        with:\n          node-version: '22'\n          cache: npm\n      - run: npm ci\n      - run: npm run build\n      - run: npm test" },
        new ProjectTemplate { Id=503, CreatedAt=SeedDate, Name="TypeScript .gitignore", TemplateType="gitignore", Architecture=ArchitectureType.TypeScript, Description=".gitignore para proyectos TypeScript", IsActive=true, Version=1,
            Content="node_modules/\ndist/\n.next/\nbuild/\n.env\n.env.*\n*.log\ncoverage/\n.DS_Store" },
    };

    public static IEnumerable<LibraryRecommendation> GetLibraries() => new[]
    {
        new LibraryRecommendation { Id=500, CreatedAt=SeedDate, Name="TypeORM", PackageName="typeorm", Architecture=ArchitectureType.TypeScript, Category="ORM", Description="ORM para TypeScript con decoradores", PopularityScore=91, InstallCommand="npm install typeorm @nestjs/typeorm reflect-metadata" },
        new LibraryRecommendation { Id=501, CreatedAt=SeedDate, Name="Prisma", PackageName="prisma", Architecture=ArchitectureType.TypeScript, Category="ORM", Description="ORM type-safe de última generación para TypeScript", PopularityScore=97, InstallCommand="npm install prisma @prisma/client" },
        new LibraryRecommendation { Id=502, CreatedAt=SeedDate, Name="class-validator", PackageName="class-validator", Architecture=ArchitectureType.TypeScript, Category="Validation", Description="Decoradores de validación para clases TypeScript", PopularityScore=94, InstallCommand="npm install class-validator class-transformer" },
        new LibraryRecommendation { Id=503, CreatedAt=SeedDate, Name="@nestjs/swagger", PackageName="@nestjs/swagger", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="Documentation", Description="Integración OpenAPI/Swagger para NestJS", PopularityScore=93, InstallCommand="npm install @nestjs/swagger swagger-ui-express" },
        new LibraryRecommendation { Id=504, CreatedAt=SeedDate, Name="@nestjs/jwt", PackageName="@nestjs/jwt", Architecture=ArchitectureType.TypeScript, Framework=FrameworkType.NestTs, Category="Autenticación", Description="Módulo JWT para NestJS", PopularityScore=92, InstallCommand="npm install @nestjs/jwt @nestjs/passport passport passport-jwt" },
        new LibraryRecommendation { Id=505, CreatedAt=SeedDate, Name="zod", PackageName="zod", Architecture=ArchitectureType.TypeScript, Category="Validation", Description="Validación de esquemas TypeScript con inferencia de tipos", PopularityScore=98, InstallCommand="npm install zod" },
        new LibraryRecommendation { Id=506, CreatedAt=SeedDate, Name="ioredis", PackageName="ioredis", Architecture=ArchitectureType.TypeScript, Category="Cache", Description="Cliente Redis robusto para Node.js/TypeScript", PopularityScore=91, InstallCommand="npm install ioredis" },
        new LibraryRecommendation { Id=507, CreatedAt=SeedDate, Name="jest + ts-jest", PackageName="jest", Architecture=ArchitectureType.TypeScript, Category="Testing", Description="Testing para TypeScript con cobertura", PopularityScore=99, InstallCommand="npm install --save-dev jest ts-jest @types/jest" },
        new LibraryRecommendation { Id=508, CreatedAt=SeedDate, Name="BullMQ", PackageName="bullmq", Architecture=ArchitectureType.TypeScript, Category="Background Jobs", Description="Cola de trabajos en background basada en Redis", PopularityScore=89, InstallCommand="npm install bullmq @nestjs/bull" },
    };

    public static IEnumerable<DesignPatternEntry> GetDesignPatterns() => new[]
    {
        new DesignPatternEntry { Id=500, CreatedAt=SeedDate, Pattern=DesignPattern.Repository, Name="Repository Pattern (NestJS/TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Repositorios tipados con TypeORM o Prisma en NestJS.", ImplementationNotes="Crear interfaces de repositorio en domain/. Implementar con TypeORM @InjectRepository o PrismaService.", ScaffoldCommandsJson="[\"mkdir -p src/domain/repositories src/infrastructure/repositories src/domain/entities\"]" },
        new DesignPatternEntry { Id=501, CreatedAt=SeedDate, Pattern=DesignPattern.CQRS, Name="CQRS (NestJS/TypeScript)", Architecture=ArchitectureType.TypeScript, Description="CQRS con @nestjs/cqrs: CommandBus, QueryBus, EventBus.", ImplementationNotes="Instalar @nestjs/cqrs. Crear commands/, queries/, events/ con handlers.", ScaffoldCommandsJson="[\"mkdir -p src/application/commands src/application/queries src/application/events src/application/handlers\"]" },
        new DesignPatternEntry { Id=502, CreatedAt=SeedDate, Pattern=DesignPattern.CleanArchitecture, Name="Clean Architecture (TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Capas: domain, application, infrastructure, presentation.", ImplementationNotes="Domain puro sin dependencias de NestJS. Inyección de dependencias mediante interfaces.", ScaffoldCommandsJson="[\"mkdir -p src/domain src/application/use-cases src/infrastructure src/presentation\"]" },
        new DesignPatternEntry { Id=503, CreatedAt=SeedDate, Pattern=DesignPattern.HexagonalArchitecture, Name="Hexagonal Architecture (TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Ports & Adapters: interfaces TypeScript como ports.", ImplementationNotes="Ports como interfaces en core/. Adapters en infrastructure/. NestJS inyecta los adapters.", ScaffoldCommandsJson="[\"mkdir -p src/core/ports src/core/domain src/infrastructure/adapters src/presentation\"]" },
        new DesignPatternEntry { Id=504, CreatedAt=SeedDate, Pattern=DesignPattern.DomainDrivenDesign, Name="Domain-Driven Design (TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Aggregates, Entities y Value Objects con TypeScript.", ImplementationNotes="Usar clases inmutables para Value Objects. Domain Events con EventEmitter2 de NestJS.", ScaffoldCommandsJson="[\"mkdir -p src/domain/aggregates src/domain/value-objects src/domain/events src/domain/services src/application\"]" },
        new DesignPatternEntry { Id=505, CreatedAt=SeedDate, Pattern=DesignPattern.EventSourcing, Name="Event Sourcing (NestJS/TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Estado derivado de eventos con @nestjs/cqrs y EventBus.", ImplementationNotes="Usar AggregateRoot de @nestjs/cqrs. Guardar eventos en EventStore.", ScaffoldCommandsJson="[\"mkdir -p src/domain/events src/infrastructure/event-store src/application/sagas\"]" },
        new DesignPatternEntry { Id=506, CreatedAt=SeedDate, Pattern=DesignPattern.Microservices, Name="Microservices (NestJS/TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Microservicios NestJS comunicándose por TCP, Redis o NATS.", ImplementationNotes="Usar @nestjs/microservices. ClientProxy para comunicación. API Gateway como entry point.", ScaffoldCommandsJson="[\"mkdir -p services/gateway services/users services/orders\"]" },
        new DesignPatternEntry { Id=507, CreatedAt=SeedDate, Pattern=DesignPattern.Mediator, Name="Mediator (TypeScript/NestJS)", Architecture=ArchitectureType.TypeScript, Description="Mediator con @nestjs/cqrs CommandBus y QueryBus.", ImplementationNotes="Usar CommandBus y QueryBus de @nestjs/cqrs. Registrar handlers en modulo.", ScaffoldCommandsJson="[\"mkdir -p src/application/commands src/application/queries src/application/handlers\"]" },
        new DesignPatternEntry { Id=508, CreatedAt=SeedDate, Pattern=DesignPattern.Saga, Name="Saga (NestJS/TypeScript)", Architecture=ArchitectureType.TypeScript, Description="Sagas reactivas con @nestjs/cqrs que orquestan flujos de eventos.", ImplementationNotes="Usar @Saga() decorator con RxJS. Escucha eventos y despacha comandos compensatorios.", ScaffoldCommandsJson="[\"mkdir -p src/application/sagas\"]" },
        new DesignPatternEntry { Id=509, CreatedAt=SeedDate, Pattern=DesignPattern.MVVM, Name="MVVM (TypeScript/React o Angular)", Architecture=ArchitectureType.TypeScript, Description="Separacion ViewModel-View con hooks o servicios de estado.", ImplementationNotes="En React: custom hook como ViewModel. En Angular: servicio observable como ViewModel.", ScaffoldCommandsJson="[\"mkdir -p src/presentation/view-models src/presentation/views\"]" },
    };
}
