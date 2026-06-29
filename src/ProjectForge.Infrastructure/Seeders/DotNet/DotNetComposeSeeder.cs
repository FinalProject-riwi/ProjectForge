using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.DotNet;

/// <summary>
/// Docker Compose templates para DotNet: todas las combinaciones Framework x Database.
/// IDs 10000–10045. Idempotente (IF NOT EXISTS).
/// </summary>
public static partial class DotNetComposeSeeder
{
    public static async Task SeedComposeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Templates] ON;

            -- ASP.NET Core MVC
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10000)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10000,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para ASP.NET Core MVC con PostgreSQL',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10001)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10001,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para ASP.NET Core MVC con MySQL',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10002)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10002,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_started

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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para ASP.NET Core MVC con SqlServer',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10003)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10003,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para ASP.NET Core MVC con MongoDB',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10004)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10004,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
    ports:
      - "{{DB_PORT}}:5432"
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para ASP.NET Core MVC con Redis',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10005)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10005,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para ASP.NET Core MVC con SQLite',N'AspNetCoreMVC',N'DockerCompose',1,N'Compose ASP.NET Core MVC + SQLite',N'compose',1);

            -- ASP.NET Core Web API
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10010)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10010,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para ASP.NET Core Web API con PostgreSQL',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10011)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10011,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para ASP.NET Core Web API con MySQL',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10012)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10012,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_started

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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para ASP.NET Core Web API con SqlServer',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10013)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10013,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para ASP.NET Core Web API con MongoDB',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10014)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10014,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
    ports:
      - "{{DB_PORT}}:5432"
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para ASP.NET Core Web API con Redis',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10015)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10015,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para ASP.NET Core Web API con SQLite',N'AspNetCoreWebApi',N'DockerCompose',1,N'Compose ASP.NET Core Web API + SQLite',N'compose',1);

            -- Blazor Server
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10020)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10020,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Blazor Server con PostgreSQL',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10021)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10021,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Blazor Server con MySQL',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10022)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10022,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_started

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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Blazor Server con SqlServer',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10023)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10023,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Blazor Server con MongoDB',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10024)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10024,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
    ports:
      - "{{DB_PORT}}:5432"
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Blazor Server con Redis',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10025)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10025,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Blazor Server con SQLite',N'BlazorServer',N'DockerCompose',1,N'Compose Blazor Server + SQLite',N'compose',1);

            -- Blazor WASM
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10030)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10030,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Blazor WASM con PostgreSQL',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10031)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10031,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Blazor WASM con MySQL',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10032)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10032,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_started

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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Blazor WASM con SqlServer',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10033)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10033,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Blazor WASM con MongoDB',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10034)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10034,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
    ports:
      - "{{DB_PORT}}:5432"
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Blazor WASM con Redis',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10035)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10035,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Blazor WASM con SQLite',N'BlazorWasm',N'DockerCompose',1,N'Compose Blazor WASM + SQLite',N'compose',1);

            -- Minimal API
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10040)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10040,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Minimal API con PostgreSQL',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10041)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10041,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};Uid=root;Pwd=secret;
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Minimal API con MySQL',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10042)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10042,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Server=db,1433;Database={{DB_NAME}};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_started

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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Minimal API con SqlServer',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10043)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10043,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Mongo=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Minimal API con MongoDB',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10044)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10044,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret
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
    ports:
      - "{{DB_PORT}}:5432"
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Minimal API con Redis',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10045)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10045,N'DotNet',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Minimal API con SQLite',N'MinimalApi',N'DockerCompose',1,N'Compose Minimal API + SQLite',N'compose',1);

            SET IDENTITY_INSERT [Templates] OFF;
""", ct);
    }
}