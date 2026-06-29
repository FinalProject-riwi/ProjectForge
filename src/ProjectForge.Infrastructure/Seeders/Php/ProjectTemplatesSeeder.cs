using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders.Php;

public static partial class PhpSeeder
{
    private static IEnumerable<ProjectTemplate> GetTemplates() =>
        new[]
        {
            new ProjectTemplate
            {
                Id = 7,
                CreatedAt = SeedDate,
                Name = "Dockerfile PHP",
                TemplateType = "dockerfile",
                Architecture = ArchitectureType.Php,
                Description = "Dockerfile generico para apps PHP modernas como Laravel y Symfony",
                IsActive = true,
                Version = 1,
                Content = """
FROM php:8.3-cli

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

RUN printf '%s\n' \
    '<?php' \
    'if (php_sapi_name() === '"'"'\'"'"''"'"'cli-server'"'"'\'"'"''"'"') {' \
    '    $path = parse_url($_SERVER['"'"'\'"'"''"'"'REQUEST_URI'"'"'\'"'"''"'"'], PHP_URL_PATH);' \
    '    $file = __DIR__ . '"'"'\'"'"''"'"'/public'"'"'\'"'"''"'"' . $path;' \
    '    if ($path !== '"'"'\'"'"''"'"'/'\'"'"''"'"' && is_file($file)) {' \
    '        return false;' \
    '    }' \
    '}' \
    'require __DIR__ . '"'"'\'"'"''"'"'/public/index.php'"'"'\'"'"''"'"';' \
    > /usr/local/bin/router.php

EXPOSE 8080

CMD ["php", "-S", "0.0.0.0:8080", "-t", "public", "/usr/local/bin/router.php"]
"""
            },
            new ProjectTemplate
            {
                Id = 8,
                CreatedAt = SeedDate,
                Name = "Compose PHP + MySQL",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.MySQL,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony con MySQL",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
      DB_CONNECTION: mysql
      DB_HOST: db
      DB_PORT: 3306
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: root
      DB_PASSWORD: secret
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
                Id = 9,
                CreatedAt = SeedDate,
                Name = "Compose PHP + PostgreSQL",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.PostgreSQL,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony con PostgreSQL",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
      DB_CONNECTION: pgsql
      DB_HOST: db
      DB_PORT: 5432
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: postgres
      DB_PASSWORD: secret
      DATABASE_URL: pgsql://postgres:secret@db:5432/{{DB_NAME}}
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
                Id = 10,
                CreatedAt = SeedDate,
                Name = "Compose PHP + SQLite",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.SQLite,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony usando SQLite",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
      DB_CONNECTION: sqlite
      DB_DATABASE: /var/www/html/database/database.sqlite
      DATABASE_URL: sqlite:///var/www/html/database/database.sqlite
    volumes:
      - sqlite-data:/var/www/html/database

volumes:
  sqlite-data:
"""
            },
            new ProjectTemplate
            {
                Id = 14,
                CreatedAt = SeedDate,
                Name = "Dockerfile PHP + MongoDB",
                TemplateType = "dockerfile",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.MongoDB,
                Description = "Dockerfile para PHP/Laravel o Symfony con soporte MongoDB",
                IsActive = true,
                Version = 1,
                Content = """
FROM php:8.3-cli

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

RUN printf '%s\n' \
    '<?php' \
    'if (php_sapi_name() === '"'"'"'"'"'"'cli-server'"'"'"'"'"'"') {' \
    '    $path = parse_url($_SERVER['"'"'"'"'"'"'REQUEST_URI'"'"'"'"'"'"'], PHP_URL_PATH);' \
    '    $file = __DIR__ . '"'"'"'"'"'"'/public'"'"'"'"'"'"' . $path;' \
    '    if ($path !== '"'"'"'"'"'"'/'"'"'"'"'"'"' && is_file($file)) {' \
    '        return false;' \
    '    }' \
    '}' \
    'require __DIR__ . '"'"'"'"'"'"'/public/index.php'"'"'"'"'"'"';' \
    > /usr/local/bin/router.php

EXPOSE 8080

CMD ["php", "-S", "0.0.0.0:8080", "-t", "public", "/usr/local/bin/router.php"]
"""
            },
            new ProjectTemplate
            {
                Id = 15,
                CreatedAt = SeedDate,
                Name = "Compose PHP + MongoDB",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.MongoDB,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony con MongoDB",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
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
  mongodb-data:
"""
            },
            new ProjectTemplate
            {
                Id = 16,
                CreatedAt = SeedDate,
                Name = "Dockerfile PHP + Redis",
                TemplateType = "dockerfile",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.Redis,
                Description = "Dockerfile para PHP/Laravel o Symfony con soporte Redis",
                IsActive = true,
                Version = 1,
                Content = """
FROM php:8.3-cli

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

RUN printf '%s\n' \
    '<?php' \
    'if (php_sapi_name() === '"'"'"'"'"'"'cli-server'"'"'"'"'"'"') {' \
    '    $path = parse_url($_SERVER['"'"'"'"'"'"'REQUEST_URI'"'"'"'"'"'"'], PHP_URL_PATH);' \
    '    $file = __DIR__ . '"'"'"'"'"'"'/public'"'"'"'"'"'"' . $path;' \
    '    if ($path !== '"'"'"'"'"'"'/'"'"'"'"'"'"' && is_file($file)) {' \
    '        return false;' \
    '    }' \
    '}' \
    'require __DIR__ . '"'"'"'"'"'"'/public/index.php'"'"'"'"'"';' \
    > /usr/local/bin/router.php

EXPOSE 8080

CMD ["php", "-S", "0.0.0.0:8080", "-t", "public", "/usr/local/bin/router.php"]
"""
            },
            new ProjectTemplate
            {
                Id = 17,
                CreatedAt = SeedDate,
                Name = "Compose PHP + Redis",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.Redis,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony con Redis",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
      DB_CONNECTION: redis
      DB_HOST: redis
      DB_PORT: 6379
      DB_DATABASE: {{DB_NAME}}
      CACHE_STORE: redis
      CACHE_DRIVER: redis
      QUEUE_CONNECTION: redis
      SESSION_DRIVER: redis
      REDIS_CLIENT: phpredis
      REDIS_HOST: redis
      REDIS_PORT: 6379
      REDIS_PASSWORD: null
    depends_on:
      - redis

  redis:
    image: redis:7-alpine
    ports:
      - "{{DB_PORT}}:6379"
    volumes:
      - redis-data:/data

volumes:
  redis-data:
"""
            },
            new ProjectTemplate
            {
                Id = 18,
                CreatedAt = SeedDate,
                Name = "Dockerfile PHP + SQL Server",
                TemplateType = "dockerfile",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.SqlServer,
                Description = "Dockerfile para PHP/Laravel o Symfony con soporte SQL Server",
                IsActive = true,
                Version = 1,
                Content = """
FROM php:8.3-cli-bookworm

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl gnupg unixodbc-dev libgssapi-krb5-2 libicu-dev libzip-dev libpng-dev libonig-dev libxml2-dev libssl-dev pkg-config $PHPIZE_DEPS \
    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /usr/share/keyrings/microsoft.gpg >/dev/null \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/microsoft-prod.list \
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

RUN printf '%s\n' \
    '<?php' \
    'if (php_sapi_name() === '"'"'"'"'"'"'cli-server'"'"'"'"'"'"') {' \
    '    $path = parse_url($_SERVER['"'"'"'"'"'"'REQUEST_URI'"'"'"'"'"'"'], PHP_URL_PATH);' \
    '    $file = __DIR__ . '"'"'"'"'"'"'/public'"'"'"'"'"'"' . $path;' \
    '    if ($path !== '"'"'"'"'"'"'/'"'"'"'"'"'"' && is_file($file)) {' \
    '        return false;' \
    '    }' \
    '}' \
    'require __DIR__ . '"'"'"'"'"'"'/public/index.php'"'"'"'"'"';' \
    > /usr/local/bin/router.php

EXPOSE 8080

CMD ["php", "-S", "0.0.0.0:8080", "-t", "public", "/usr/local/bin/router.php"]
"""
            },
            new ProjectTemplate
            {
                Id = 19,
                CreatedAt = SeedDate,
                Name = "Compose PHP + SQL Server",
                TemplateType = "compose",
                Architecture = ArchitectureType.Php,
                Database = DatabaseType.SqlServer,
                Infrastructure = InfrastructureType.DockerCompose,
                Description = "Docker Compose para PHP/Laravel o Symfony con SQL Server",
                IsActive = true,
                Version = 1,
                Content = """
version: '3.9'
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8080"
    environment:
      APP_ENV: local
      APP_DEBUG: "true"
      APP_URL: http://localhost:{{APP_PORT}}
      DB_CONNECTION: sqlsrv
      DB_HOST: sqlserver
      DB_PORT: 1433
      DB_DATABASE: {{DB_NAME}}
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
      DB_ENCRYPT: "false"
      DB_TRUST_SERVER_CERTIFICATE: "true"
      DATABASE_URL: sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/{{DB_NAME}}
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
      - "{{DB_PORT}}:1433"
    volumes:
      - mssql-data:/var/opt/mssql

volumes:
  mssql-data:
"""
            },
            new ProjectTemplate
            {
                Id = 11,
                CreatedAt = SeedDate,
                Name = "PHP .gitignore",
                TemplateType = "gitignore",
                Architecture = ArchitectureType.Php,
                Description = "Gitignore base para proyectos PHP modernos",
                IsActive = true,
                Version = 1,
                Content = """
/vendor/
/node_modules/
/.env
/.env.*
/.phpunit.result.cache
/public/build/
/bootstrap/cache/*.php
/storage/app/*.sqlite
/storage/framework/cache/*
/storage/framework/sessions/*
/storage/framework/testing/*
/storage/framework/views/*
/storage/logs/*
/var/
"""
            },
            new ProjectTemplate
            {
                Id = 12,
                CreatedAt = SeedDate,
                Name = "CI PHP GitHub Actions",
                TemplateType = "ci",
                Architecture = ArchitectureType.Php,
                Description = "Pipeline CI/CD para PHP con Composer y GitHub Actions",
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
      - name: Setup PHP
        uses: shivammathur/setup-php@v2
        with:
          php-version: '8.3'
          extensions: mbstring, xml, curl, zip, intl, pdo_sqlite
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress
      - name: Run tests
        run: |
          if [ -f artisan ]; then
            php artisan test
          elif [ -f bin/console ]; then
            if [ -f vendor/bin/phpunit ]; then
              vendor/bin/phpunit
            elif [ -f bin/phpunit ]; then
              php bin/phpunit
            fi
          elif [ -f vendor/bin/phpunit ]; then
            vendor/bin/phpunit
          fi
"""
            },
            new ProjectTemplate
            {
                Id = 13,
                CreatedAt = SeedDate,
                Name = "K8s PHP Deployment",
                TemplateType = "k8s-deployment",
                Architecture = ArchitectureType.Php,
                Infrastructure = InfrastructureType.Kubernetes,
                Description = "Kubernetes Deployment y Service para apps PHP",
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
            - containerPort: 8080
          env:
            - name: APP_ENV
              value: production
            - name: APP_DEBUG
              value: "false"
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
  type: LoadBalancer
"""
            }
        };
}
