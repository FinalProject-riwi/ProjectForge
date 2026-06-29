using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Php;

/// <summary>
/// Docker Compose templates para Php: todas las combinaciones Framework x Database.
/// IDs 10110–10125. Idempotente (IF NOT EXISTS).
/// </summary>
public static partial class PhpComposeSeeder
{
    public static async Task SeedComposeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Laravel 11
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

            -- Symfony 7
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

            SET IDENTITY_INSERT [Templates] OFF;
""", ct);
    }
}