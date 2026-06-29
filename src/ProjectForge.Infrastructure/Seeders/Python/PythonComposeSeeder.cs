using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Python;

/// <summary>
/// Docker Compose templates para Python: todas las combinaciones Framework x Database.
/// IDs 10080–10105. Idempotente (IF NOT EXISTS).
/// </summary>
public static partial class PythonComposeSeeder
{
    public static async Task SeedComposeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Templates] ON;

            -- FastAPI
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

            -- Django 5
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

            -- Flask 3
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

            SET IDENTITY_INSERT [Templates] OFF;
""", ct);
    }
}