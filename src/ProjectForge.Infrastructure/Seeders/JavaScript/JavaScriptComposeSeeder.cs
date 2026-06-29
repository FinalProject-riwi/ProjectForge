using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.JavaScript;

/// <summary>
/// Docker Compose templates para JavaScript: todas las combinaciones Framework x Database.
/// IDs 10130–10165. Idempotente (IF NOT EXISTS).
/// </summary>
public static partial class JavaScriptComposeSeeder
{
    public static async Task SeedComposeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Node.js
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

            -- Express.js
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

            -- NestJS
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

            -- Next.js
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

            SET IDENTITY_INSERT [Templates] OFF;
""", ct);
    }
}