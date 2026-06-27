using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <summary>
    /// Fix: La migración AddExtendedSeedData tenía colisión de PKs con Templates 7-19
    /// porque AddPhpSeedData ya los había insertado en DesignPatterns/Libraries con los mismos IDs.
    /// Esta migración:
    /// 1. Crea AiSuggestionCaches si no existe (fix para fresh installs)
    /// 2. Inserta todos los Templates PHP faltantes con IF NOT EXISTS
    /// 3. Inserta Templates de Java/Python faltantes
    /// </summary>
    public partial class FixPkCollisionAndAddAiCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── 1. Crear AiSuggestionCaches si no existe ─────────────────────
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AiSuggestionCaches]') AND type = 'U')
                BEGIN
                    CREATE TABLE [AiSuggestionCaches] (
                        [Id]          INT IDENTITY(1,1) NOT NULL,
                        [CacheKey]    NVARCHAR(450)     NOT NULL,
                        [PatternsJson]NVARCHAR(MAX)     NOT NULL DEFAULT '',
                        [LibrariesJson]NVARCHAR(MAX)    NOT NULL DEFAULT '',
                        [Rationale]   NVARCHAR(MAX)    NOT NULL DEFAULT '',
                        [ExpiresAt]   DATETIME2         NOT NULL,
                        [CreatedAt]   DATETIME2         NOT NULL,
                        [UpdatedAt]   DATETIME2         NULL,
                        CONSTRAINT [PK_AiSuggestionCaches] PRIMARY KEY ([Id])
                    );
                    CREATE UNIQUE INDEX [IX_AiSuggestionCaches_CacheKey] ON [AiSuggestionCaches]([CacheKey]);
                END
                """);

            // ─── 2. PHP Templates (7-19) con IF NOT EXISTS ───────────────────
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Templates] ON;

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 7)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (7, 'Php', 'FROM php:8.3-cli

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev \
    && docker-php-ext-install pdo pdo_mysql pdo_pgsql pdo_sqlite mbstring zip intl bcmath \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer

COPY . .

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \
    && mkdir -p storage bootstrap/cache database \
    && touch database/database.sqlite \
    && chmod -R 775 storage bootstrap/cache database

EXPOSE 8080

CMD [""php"", ""-S"", ""0.0.0.0:8080"", ""-t"", ""public""]',
                    '2026-01-01T00:00:00Z', NULL, 'Dockerfile generico para apps PHP modernas como Laravel y Symfony', NULL, NULL, 1, 'Dockerfile PHP', 'dockerfile', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 8)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (8, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      APP_ENV: local
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
      - ""{{DB_PORT}}:3306""
    volumes:
      - mysqldata:/var/lib/mysql
    healthcheck:
      test: [""CMD"", ""mysqladmin"", ""ping"", ""-h"", ""localhost""]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  mysqldata:',
                    '2026-01-01T00:00:00Z', 'MySQL', 'Docker Compose para PHP/Laravel o Symfony con MySQL', NULL, 'DockerCompose', 1, 'Compose PHP + MySQL', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 9)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (9, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      APP_ENV: local
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
      - ""{{DB_PORT}}:5432""
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: [""CMD"", ""pg_isready"", ""-U"", ""postgres""]
      interval: 10s
      timeout: 5s
      retries: 5
volumes:
  pgdata:',
                    '2026-01-01T00:00:00Z', 'PostgreSQL', 'Docker Compose para PHP/Laravel o Symfony con PostgreSQL', NULL, 'DockerCompose', 1, 'Compose PHP + PostgreSQL', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 10)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (10, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      APP_ENV: local
      DB_CONNECTION: sqlite
      DB_DATABASE: /var/www/html/database/database.sqlite
    volumes:
      - sqlite-data:/var/www/html/database
volumes:
  sqlite-data:',
                    '2026-01-01T00:00:00Z', 'SQLite', 'Docker Compose para PHP/Laravel o Symfony usando SQLite', NULL, 'DockerCompose', 1, 'Compose PHP + SQLite', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 11)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (11, 'Php', '/vendor/
/node_modules/
/.env
/.env.*
/.phpunit.result.cache
/public/build/
/bootstrap/cache/*.php
/storage/app/*.sqlite
/storage/framework/cache/*
/storage/framework/sessions/*
/storage/logs/*
/var/',
                    '2026-01-01T00:00:00Z', NULL, 'Gitignore base para proyectos PHP modernos', NULL, NULL, 1, 'PHP .gitignore', 'gitignore', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 12)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (12, 'Php', 'name: CI
on:
  push:
    branches: [main]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup PHP
        uses: shivammathur/setup-php@v2
        with:
          php-version: ''8.3''
          extensions: mbstring, xml, curl, zip, intl, pdo_sqlite
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress
      - name: Run tests
        run: |
          if [ -f artisan ]; then
            php artisan test
          elif [ -f vendor/bin/phpunit ]; then
            vendor/bin/phpunit
          fi',
                    '2026-01-01T00:00:00Z', NULL, 'Pipeline CI/CD para PHP con Composer y GitHub Actions', NULL, NULL, 1, 'CI PHP GitHub Actions', 'ci', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 13)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (13, 'Php', 'apiVersion: apps/v1
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
            - containerPort: 8080
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
      targetPort: 8080
  type: LoadBalancer',
                    '2026-01-01T00:00:00Z', NULL, 'Kubernetes Deployment y Service para apps PHP', NULL, 'Kubernetes', 1, 'K8s PHP Deployment', 'k8s-deployment', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 14)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (14, 'Php', 'FROM php:8.3-cli

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev libssl-dev pkg-config \
    && pecl install mongodb \
    && docker-php-ext-enable mongodb \
    && docker-php-ext-install pdo mbstring zip intl bcmath \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \
    && mkdir -p storage bootstrap/cache \
    && chmod -R 775 storage bootstrap/cache

EXPOSE 8080
CMD [""php"", ""-S"", ""0.0.0.0:8080"", ""-t"", ""public""]',
                    '2026-01-01T00:00:00Z', 'MongoDB', 'Dockerfile para PHP/Laravel o Symfony con soporte MongoDB', NULL, NULL, 1, 'Dockerfile PHP + MongoDB', 'dockerfile', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 15)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (15, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      APP_ENV: local
      DB_CONNECTION: mongodb
      MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - ""{{DB_PORT}}:27017""
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:',
                    '2026-01-01T00:00:00Z', 'MongoDB', 'Docker Compose para PHP/Laravel o Symfony con MongoDB', NULL, 'DockerCompose', 1, 'Compose PHP + MongoDB', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 16)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (16, 'Php', 'FROM php:8.3-cli

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev libssl-dev pkg-config \
    && pecl install redis \
    && docker-php-ext-enable redis \
    && docker-php-ext-install pdo mbstring zip intl bcmath \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \
    && mkdir -p storage bootstrap/cache \
    && chmod -R 775 storage bootstrap/cache

EXPOSE 8080
CMD [""php"", ""-S"", ""0.0.0.0:8080"", ""-t"", ""public""]',
                    '2026-01-01T00:00:00Z', 'Redis', 'Dockerfile para PHP/Laravel o Symfony con soporte Redis', NULL, NULL, 1, 'Dockerfile PHP + Redis', 'dockerfile', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 17)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (17, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      CACHE_STORE: redis
      REDIS_HOST: redis
      REDIS_PORT: 6379
    depends_on:
      - redis
  redis:
    image: redis:7-alpine
    ports:
      - ""{{DB_PORT}}:6379""
    volumes:
      - redis-data:/data
volumes:
  redis-data:',
                    '2026-01-01T00:00:00Z', 'Redis', 'Docker Compose para PHP/Laravel o Symfony con Redis', NULL, 'DockerCompose', 1, 'Compose PHP + Redis', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 18)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (18, 'Php', 'FROM php:8.3-cli-bookworm

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl gnupg unixodbc-dev libicu-dev libzip-dev libpng-dev libonig-dev libxml2-dev libssl-dev pkg-config \
    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /usr/share/keyrings/microsoft.gpg >/dev/null \
    && echo ""deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main"" > /etc/apt/sources.list.d/microsoft-prod.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 \
    && pecl install sqlsrv pdo_sqlsrv \
    && docker-php-ext-enable sqlsrv pdo_sqlsrv \
    && docker-php-ext-install pdo mbstring zip intl bcmath \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi \
    && mkdir -p storage bootstrap/cache \
    && chmod -R 775 storage bootstrap/cache

EXPOSE 8080
CMD [""php"", ""-S"", ""0.0.0.0:8080"", ""-t"", ""public""]',
                    '2026-01-01T00:00:00Z', 'SqlServer', 'Dockerfile para PHP/Laravel o Symfony con soporte SQL Server', NULL, NULL, 1, 'Dockerfile PHP + SQL Server', 'dockerfile', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 19)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (19, 'Php', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: sqlserver
      DB_PORT: 1433
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
    depends_on:
      sqlserver:
        condition: service_started
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      MSSQL_PID: Developer
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd
    ports:
      - ""{{DB_PORT}}:1433""
    volumes:
      - mssql-data:/var/opt/mssql
volumes:
  mssql-data:',
                    '2026-01-01T00:00:00Z', 'SqlServer', 'Docker Compose para PHP/Laravel o Symfony con SQL Server', NULL, 'DockerCompose', 1, 'Compose PHP + SQL Server', 'compose', NULL, NULL, 1);

                -- Java CI template
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1100)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (1100, 'Java', 'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-java@v4
        with:
          java-version: ''21''
          distribution: temurin
          cache: maven
      - run: mvn -B package --no-transfer-progress',
                    '2026-01-01T00:00:00Z', NULL, 'Pipeline CI para Maven/Spring Boot', NULL, NULL, 1, 'CI Java GitHub Actions', 'ci', NULL, NULL, 1);

                -- Python CI template
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1200)
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES (1200, 'Python', 'name: CI
on:
  push:
    branches: [main]
  pull_request:
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: ''3.12''
      - run: pip install -r requirements.txt
      - run: pytest --tb=short',
                    '2026-01-01T00:00:00Z', NULL, 'Pipeline CI para Python con pytest', NULL, NULL, 1, 'CI Python GitHub Actions', 'ci', NULL, NULL, 1);

                -- Java gitignore
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Java' AND [TemplateType] = 'gitignore')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Java', 'target/
*.class
*.jar
*.war
*.ear
.idea/
*.iml
.classpath
.project
.settings/
*.log
.mvn/
mvnw
mvnw.cmd
.DS_Store',
                    '2026-01-01T00:00:00Z', NULL, '.gitignore para proyectos Java/Maven/Spring Boot', NULL, NULL, 1, 'Java .gitignore', 'gitignore', NULL, NULL, 1);

                -- Python gitignore
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Python' AND [TemplateType] = 'gitignore')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Python', '__pycache__/
*.py[cod]
*.pyo
*.pyd
.Python
env/
venv/
.venv/
.env
.env.*
*.egg-info/
dist/
build/
.pytest_cache/
.coverage
htmlcov/
*.log
.DS_Store',
                    '2026-01-01T00:00:00Z', NULL, '.gitignore para proyectos Python', NULL, NULL, 1, 'Python .gitignore', 'gitignore', NULL, NULL, 1);

                -- DotNet gitignore update (ensure exists)
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'DotNet' AND [TemplateType] = 'gitignore')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('DotNet', '[Dd]ebug/
[Rr]elease/
[Bb]in/
[Oo]bj/
*.user
*.suo
.vs/
.vscode/
.env
.env.*
*.pfx
packages/',
                    '2026-01-01T00:00:00Z', NULL, 'Gitignore para proyectos .NET', NULL, NULL, 1, '.gitignore .NET', 'gitignore', NULL, NULL, 1);

                -- Java additional DB compose templates
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Java' AND [TemplateType] = 'compose' AND [Database] = 'SqlServer')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Java', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlserver://sqlserver:1433;databaseName={{DB_NAME}};encrypt=false;trustServerCertificate=true
      SPRING_DATASOURCE_USERNAME: sa
      SPRING_DATASOURCE_PASSWORD: YourStrong!Passw0rd
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: com.microsoft.sqlserver.jdbc.SQLServerDriver
    depends_on:
      sqlserver:
        condition: service_started
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      MSSQL_PID: Developer
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd
    ports:
      - ""{{DB_PORT}}:1433""
    volumes:
      - mssql-data:/var/opt/mssql
volumes:
  mssql-data:',
                    '2026-01-01T00:00:00Z', 'SqlServer', 'Docker Compose para Spring Boot + SQL Server', NULL, 'DockerCompose', 1, 'Compose Java + SQL Server', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Java' AND [TemplateType] = 'compose' AND [Database] = 'MongoDB')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Java', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      SPRING_DATA_MONGODB_URI: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - ""{{DB_PORT}}:27017""
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:',
                    '2026-01-01T00:00:00Z', 'MongoDB', 'Docker Compose para Spring Boot + MongoDB', NULL, 'DockerCompose', 1, 'Compose Java + MongoDB', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Java' AND [TemplateType] = 'compose' AND [Database] = 'Redis')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Java', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      SPRING_DATA_REDIS_HOST: redis
      SPRING_DATA_REDIS_PORT: 6379
    depends_on:
      - redis
  redis:
    image: redis:7-alpine
    ports:
      - ""{{DB_PORT}}:6379""
    volumes:
      - redis-data:/data
volumes:
  redis-data:',
                    '2026-01-01T00:00:00Z', 'Redis', 'Docker Compose para Spring Boot + Redis', NULL, 'DockerCompose', 1, 'Compose Java + Redis', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Java' AND [TemplateType] = 'compose' AND [Database] = 'SQLite')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Java', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      SPRING_DATASOURCE_URL: jdbc:sqlite:/app/data/{{DB_NAME}}.db
      SPRING_DATASOURCE_DRIVER_CLASS_NAME: org.sqlite.JDBC
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:',
                    '2026-01-01T00:00:00Z', 'SQLite', 'Docker Compose para Spring Boot + SQLite', NULL, 'DockerCompose', 1, 'Compose Java + SQLite', 'compose', NULL, NULL, 1);

                -- Python additional compose templates
                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Python' AND [TemplateType] = 'compose' AND [Database] = 'SqlServer')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Python', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8000""
    environment:
      DATABASE_URL: mssql+pyodbc://sa:YourStrong!Passw0rd@sqlserver:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes
    depends_on:
      sqlserver:
        condition: service_started
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      MSSQL_PID: Developer
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd
    ports:
      - ""{{DB_PORT}}:1433""
    volumes:
      - mssql-data:/var/opt/mssql
volumes:
  mssql-data:',
                    '2026-01-01T00:00:00Z', 'SqlServer', 'Docker Compose para Python + SQL Server', NULL, 'DockerCompose', 1, 'Compose Python + SQL Server', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Python' AND [TemplateType] = 'compose' AND [Database] = 'MongoDB')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Python', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8000""
    environment:
      MONGODB_URL: mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - ""{{DB_PORT}}:27017""
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:',
                    '2026-01-01T00:00:00Z', 'MongoDB', 'Docker Compose para Python + MongoDB', NULL, 'DockerCompose', 1, 'Compose Python + MongoDB', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Python' AND [TemplateType] = 'compose' AND [Database] = 'Redis')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Python', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8000""
    environment:
      REDIS_URL: redis://redis:6379/0
    depends_on:
      - redis
  redis:
    image: redis:7-alpine
    ports:
      - ""{{DB_PORT}}:6379""
    volumes:
      - redis-data:/data
volumes:
  redis-data:',
                    '2026-01-01T00:00:00Z', 'Redis', 'Docker Compose para Python + Redis', NULL, 'DockerCompose', 1, 'Compose Python + Redis', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'Python' AND [TemplateType] = 'compose' AND [Database] = 'SQLite')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('Python', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8000""
    environment:
      DATABASE_URL: sqlite:///./{{DB_NAME}}.db
    volumes:
      - sqlite-data:/app
volumes:
  sqlite-data:',
                    '2026-01-01T00:00:00Z', 'SQLite', 'Docker Compose para Python + SQLite', NULL, 'DockerCompose', 1, 'Compose Python + SQLite', 'compose', NULL, NULL, 1);

                SET IDENTITY_INSERT [Templates] OFF;
                ");

            // ─── 3. Librerías faltantes de DotNet ────────────────────────────
            migrationBuilder.Sql("""
                SET IDENTITY_INSERT [Libraries] ON;

                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 8)
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[LibraryRecommendationId],[Name],[PackageName],[PopularityScore],[UpdatedAt],[Version])
                    VALUES (8, 'Python', 'ORM', '2026-01-01T00:00:00Z', 'ORM más popular para Python, soporta sync y async', NULL, 'pip install sqlalchemy', NULL, 'SQLAlchemy', 'sqlalchemy', 98, NULL, NULL);

                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 9)
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[LibraryRecommendationId],[Name],[PackageName],[PopularityScore],[UpdatedAt],[Version])
                    VALUES (9, 'Python', 'Validation', '2026-01-01T00:00:00Z', 'Validación de datos con type hints', NULL, 'pip install pydantic', NULL, 'Pydantic', 'pydantic', 97, NULL, NULL);

                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 10)
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[LibraryRecommendationId],[Name],[PackageName],[PopularityScore],[UpdatedAt],[Version])
                    VALUES (10, 'Python', 'Migrations', '2026-01-01T00:00:00Z', 'Migraciones de base de datos para SQLAlchemy', NULL, 'pip install alembic', NULL, 'Alembic', 'alembic', 90, NULL, NULL);

                SET IDENTITY_INSERT [Libraries] OFF;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AiSuggestionCaches]') AND type = 'U')
                    DROP TABLE [AiSuggestionCaches];
                """);

            // Remove templates inserted by this fix migration that didn't exist before
            migrationBuilder.Sql("""
                DELETE FROM [Templates] WHERE [Id] IN (7,8,9,10,11,12,13,14,15,16,17,18,19,1100,1200);
                DELETE FROM [Templates] WHERE [Architecture] IN ('Java','Python') AND [TemplateType] IN ('gitignore','compose')
                    AND [Id] NOT IN (SELECT [Id] FROM [Templates] WHERE [Id] IN (800,801,802,900,901,902));
                """);

            // ─── 4. DotNet missing compose templates ──────────────────────────
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Templates] ON;

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'DotNet' AND [TemplateType] = 'compose' AND [Database] = 'MongoDB')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('DotNet', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      - ConnectionStrings__Default=mongodb://mongo:27017/{{DB_NAME}}
    depends_on:
      - mongo
  mongo:
    image: mongo:7
    ports:
      - ""{{DB_PORT}}:27017""
    volumes:
      - mongodb-data:/data/db
volumes:
  mongodb-data:',
                    '2026-01-01T00:00:00Z', 'MongoDB', 'Docker Compose para .NET + MongoDB', NULL, 'DockerCompose', 1, 'Compose .NET + MongoDB', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'DotNet' AND [TemplateType] = 'compose' AND [Database] = 'Redis')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('DotNet', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      - ConnectionStrings__Default=redis:6379
    depends_on:
      - redis
  redis:
    image: redis:7-alpine
    ports:
      - ""{{DB_PORT}}:6379""
    volumes:
      - redis-data:/data
volumes:
  redis-data:',
                    '2026-01-01T00:00:00Z', 'Redis', 'Docker Compose para .NET + Redis', NULL, 'DockerCompose', 1, 'Compose .NET + Redis', 'compose', NULL, NULL, 1);

                IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Architecture] = 'DotNet' AND [TemplateType] = 'compose' AND [Database] = 'SQLite')
                    INSERT INTO [Templates] ([Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[UpdatedAt],[VariablesSchemaJson],[Version])
                    VALUES ('DotNet', 'version: ''3.9''
services:
  app:
    build: .
    ports:
      - ""{{APP_PORT}}:8080""
    environment:
      - ConnectionStrings__Default=Data Source=/app/data/{{DB_NAME}}.db
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:',
                    '2026-01-01T00:00:00Z', 'SQLite', 'Docker Compose para .NET + SQLite', NULL, 'DockerCompose', 1, 'Compose .NET + SQLite', 'compose', NULL, NULL, 1);

                SET IDENTITY_INSERT [Templates] OFF;
                ");
        }
    }
}