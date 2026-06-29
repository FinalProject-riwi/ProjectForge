using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Agrega 114 Docker Compose templates: una por cada combinación
/// Framework x Database (6 DB x 19 frameworks). IDs 10000-10185.
/// Todos los inserts usan IF NOT EXISTS -> idempotentes.
/// </summary>
public partial class AddComposeTemplatesAllFrameworks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

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

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10050)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10050,N'Java',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Spring Boot 3.x con PostgreSQL',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10051)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10051,N'Java',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Spring Boot 3.x con MySQL',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10052)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10052,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlserver://db:1433;databaseName={{DB_NAME}};encrypt=false
      SPRING_DATASOURCE_USERNAME: sa
      SPRING_DATASOURCE_PASSWORD: YourStrong!Passw0rd
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Spring Boot 3.x con SqlServer',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10053)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10053,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATA_MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Spring Boot 3.x con MongoDB',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10054)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10054,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: postgres
      SPRING_DATASOURCE_PASSWORD: secret
      SPRING_REDIS_HOST: redis
      SPRING_REDIS_PORT: 6379
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Spring Boot 3.x con Redis',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10055)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10055,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/app.db
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Spring Boot 3.x con SQLite',N'SpringBoot',N'DockerCompose',1,N'Compose Spring Boot 3.x + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10060)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10060,N'Java',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Quarkus con PostgreSQL',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10061)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10061,N'Java',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Quarkus con MySQL',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10062)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10062,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlserver://db:1433;databaseName={{DB_NAME}};encrypt=false
      SPRING_DATASOURCE_USERNAME: sa
      SPRING_DATASOURCE_PASSWORD: YourStrong!Passw0rd
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Quarkus con SqlServer',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10063)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10063,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATA_MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Quarkus con MongoDB',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10064)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10064,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: postgres
      SPRING_DATASOURCE_PASSWORD: secret
      SPRING_REDIS_HOST: redis
      SPRING_REDIS_PORT: 6379
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Quarkus con Redis',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10065)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10065,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/app.db
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Quarkus con SQLite',N'Quarkus',N'DockerCompose',1,N'Compose Quarkus + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10070)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10070,N'Java',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Micronaut con PostgreSQL',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10071)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10071,N'Java',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Micronaut con MySQL',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10072)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10072,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlserver://db:1433;databaseName={{DB_NAME}};encrypt=false
      SPRING_DATASOURCE_USERNAME: sa
      SPRING_DATASOURCE_PASSWORD: YourStrong!Passw0rd
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Micronaut con SqlServer',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10073)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10073,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATA_MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Micronaut con MongoDB',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10074)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10074,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:postgresql://db:5432/{{DB_NAME}}
      SPRING_DATASOURCE_USERNAME: postgres
      SPRING_DATASOURCE_PASSWORD: secret
      SPRING_REDIS_HOST: redis
      SPRING_REDIS_PORT: 6379
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Micronaut con Redis',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10075)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10075,N'Java',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/app.db
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Micronaut con SQLite',N'Micronaut',N'DockerCompose',1,N'Compose Micronaut + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10080)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10080,N'Python',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para FastAPI con PostgreSQL',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10081)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10081,N'Python',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para FastAPI con MySQL',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10082)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10082,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: mssql+pyodbc://sa:YourStrong!Passw0rd@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para FastAPI con SqlServer',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10083)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10083,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      MONGODB_URL: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para FastAPI con MongoDB',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10084)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10084,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}
      REDIS_URL: redis://redis:6379/0
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para FastAPI con Redis',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10085)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10085,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para FastAPI con SQLite',N'FastAPI',N'DockerCompose',1,N'Compose FastAPI + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10090)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10090,N'Python',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Django 5 con PostgreSQL',N'Django',N'DockerCompose',1,N'Compose Django 5 + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10091)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10091,N'Python',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Django 5 con MySQL',N'Django',N'DockerCompose',1,N'Compose Django 5 + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10092)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10092,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: mssql+pyodbc://sa:YourStrong!Passw0rd@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Django 5 con SqlServer',N'Django',N'DockerCompose',1,N'Compose Django 5 + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10093)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10093,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      MONGODB_URL: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Django 5 con MongoDB',N'Django',N'DockerCompose',1,N'Compose Django 5 + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10094)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10094,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}
      REDIS_URL: redis://redis:6379/0
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Django 5 con Redis',N'Django',N'DockerCompose',1,N'Compose Django 5 + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10095)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10095,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Django 5 con SQLite',N'Django',N'DockerCompose',1,N'Compose Django 5 + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10100)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10100,N'Python',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Flask 3 con PostgreSQL',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10101)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10101,N'Python',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Flask 3 con MySQL',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10102)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10102,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: mssql+pyodbc://sa:YourStrong!Passw0rd@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Flask 3 con SqlServer',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10103)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10103,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      MONGODB_URL: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo

  mongo:
    image: mongo:7
    ports:
      - "{{DB_PORT}}:27017"
    volumes:
      - mongodb-data:/data/db

volumes:
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Flask 3 con MongoDB',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10104)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10104,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: postgresql://postgres:secret@db:5432/{{DB_NAME}}
      REDIS_URL: redis://redis:6379/0
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Flask 3 con Redis',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10105)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10105,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.db
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Flask 3 con SQLite',N'Flask',N'DockerCompose',1,N'Compose Flask 3 + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10110)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10110,N'Php',N'version: ''3.9''
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Laravel 11 con PostgreSQL',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10111)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10111,N'Php',N'version: ''3.9''
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Laravel 11 con MySQL',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10112)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10112,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: db
      DB_PORT: 1433
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Laravel 11 con SqlServer',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10113)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10113,N'Php',N'version: ''3.9''
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Laravel 11 con MongoDB',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10114)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10114,N'Php',N'version: ''3.9''
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Laravel 11 con Redis',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10115)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10115,N'Php',N'version: ''3.9''
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
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Laravel 11 con SQLite',N'Laravel',N'DockerCompose',1,N'Compose Laravel 11 + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10120)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10120,N'Php',N'version: ''3.9''
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Symfony 7 con PostgreSQL',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10121)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10121,N'Php',N'version: ''3.9''
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
      test: ["CMD","mysqladmin","ping","-h","localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Symfony 7 con MySQL',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10122)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10122,N'Php',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: db
      DB_PORT: 1433
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Symfony 7 con SqlServer',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10123)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10123,N'Php',N'version: ''3.9''
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Symfony 7 con MongoDB',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10124)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10124,N'Php',N'version: ''3.9''
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Symfony 7 con Redis',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10125)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10125,N'Php',N'version: ''3.9''
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
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Symfony 7 con SQLite',N'Symfony',N'DockerCompose',1,N'Compose Symfony 7 + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10130)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10130,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Node.js con PostgreSQL',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10131)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10131,N'JavaScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Node.js con MySQL',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10132)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10132,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Node.js con SqlServer',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10133)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10133,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Node.js con MongoDB',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10134)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10134,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Node.js con Redis',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10135)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10135,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.sqlite
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Node.js con SQLite',N'NodeJs',N'DockerCompose',1,N'Compose Node.js + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10140)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10140,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Express.js con PostgreSQL',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10141)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10141,N'JavaScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Express.js con MySQL',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10142)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10142,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Express.js con SqlServer',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10143)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10143,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Express.js con MongoDB',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10144)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10144,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Express.js con Redis',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10145)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10145,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.sqlite
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Express.js con SQLite',N'ExpressJs',N'DockerCompose',1,N'Compose Express.js + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10150)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10150,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para NestJS con PostgreSQL',N'NestJs',N'DockerCompose',1,N'Compose NestJS + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10151)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10151,N'JavaScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para NestJS con MySQL',N'NestJs',N'DockerCompose',1,N'Compose NestJS + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10152)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10152,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para NestJS con SqlServer',N'NestJs',N'DockerCompose',1,N'Compose NestJS + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10153)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10153,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para NestJS con MongoDB',N'NestJs',N'DockerCompose',1,N'Compose NestJS + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10154)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10154,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para NestJS con Redis',N'NestJs',N'DockerCompose',1,N'Compose NestJS + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10155)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10155,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.sqlite
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para NestJS con SQLite',N'NestJs',N'DockerCompose',1,N'Compose NestJS + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10160)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10160,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Next.js con PostgreSQL',N'NextJs',N'DockerCompose',1,N'Compose Next.js + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10161)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10161,N'JavaScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Next.js con MySQL',N'NextJs',N'DockerCompose',1,N'Compose Next.js + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10162)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10162,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Next.js con SqlServer',N'NextJs',N'DockerCompose',1,N'Compose Next.js + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10163)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10163,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Next.js con MongoDB',N'NextJs',N'DockerCompose',1,N'Compose Next.js + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10164)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10164,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Next.js con Redis',N'NextJs',N'DockerCompose',1,N'Compose Next.js + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10165)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10165,N'JavaScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlite:////app/data/app.sqlite
    volumes:
      - sqlite-data:/app/data

volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Next.js con SQLite',N'NextJs',N'DockerCompose',1,N'Compose Next.js + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10170)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10170,N'TypeScript',N'version: ''3.9''
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
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para NestJS (TypeScript) con PostgreSQL',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10171)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10171,N'TypeScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para NestJS (TypeScript) con MySQL',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10172)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10172,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para NestJS (TypeScript) con SqlServer',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10173)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10173,N'TypeScript',N'version: ''3.9''
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para NestJS (TypeScript) con MongoDB',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10174)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10174,N'TypeScript',N'version: ''3.9''
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para NestJS (TypeScript) con Redis',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10175)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10175,N'TypeScript',N'version: ''3.9''
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
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para NestJS (TypeScript) con SQLite',N'NestTs',N'DockerCompose',1,N'Compose NestJS (TypeScript) + SQLite',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10180)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10180,N'TypeScript',N'version: ''3.9''
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
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD","pg_isready","-U","postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para Next.js (TypeScript) con PostgreSQL',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + PostgreSQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10181)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10181,N'TypeScript',N'version: ''3.9''
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Next.js (TypeScript) con MySQL',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + MySQL',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10182)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10182,N'TypeScript',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:3000"
    environment:
      DATABASE_URL: sqlserver://db:1433;database={{DB_NAME}};user=sa;password=YourStrong!Passw0rd;encrypt=false
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Next.js (TypeScript) con SqlServer',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + SqlServer',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10183)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10183,N'TypeScript',N'version: ''3.9''
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Next.js (TypeScript) con MongoDB',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + MongoDB',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10184)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10184,N'TypeScript',N'version: ''3.9''
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Next.js (TypeScript) con Redis',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + Redis',N'compose',1);

            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10185)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (10185,N'TypeScript',N'version: ''3.9''
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
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Next.js (TypeScript) con SQLite',N'NextTs',N'DockerCompose',1,N'Compose Next.js (TypeScript) + SQLite',N'compose',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (10000,10001,10002,10003,10004,10005,10010,10011,10012,10013,10014,10015,10020,10021,10022,10023,10024,10025,10030,10031,10032,10033,10034,10035,10040,10041,10042,10043,10044,10045,10050,10051,10052,10053,10054,10055,10060,10061,10062,10063,10064,10065,10070,10071,10072,10073,10074,10075,10080,10081,10082,10083,10084,10085,10090,10091,10092,10093,10094,10095,10100,10101,10102,10103,10104,10105,10110,10111,10112,10113,10114,10115,10120,10121,10122,10123,10124,10125,10130,10131,10132,10133,10134,10135,10140,10141,10142,10143,10144,10145,10150,10151,10152,10153,10154,10155,10160,10161,10162,10163,10164,10165,10170,10171,10172,10173,10174,10175,10180,10181,10182,10183,10184,10185);");
    }
}