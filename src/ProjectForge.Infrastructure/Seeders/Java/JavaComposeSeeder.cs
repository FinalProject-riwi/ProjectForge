using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Java;

/// <summary>
/// Docker Compose templates para Java: todas las combinaciones Framework x Database.
/// IDs 10050–10075. Idempotente (IF NOT EXISTS).
/// </summary>
public static partial class JavaComposeSeeder
{
    public static async Task SeedComposeAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Spring Boot 3.x
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

            -- Quarkus
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

            -- Micronaut
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

            SET IDENTITY_INSERT [Templates] OFF;
""", ct);
    }
}