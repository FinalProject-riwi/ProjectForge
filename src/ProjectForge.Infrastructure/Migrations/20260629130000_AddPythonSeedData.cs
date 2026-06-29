using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Seeds all Python data: Templates, Libraries y DesignPatterns.
/// Frameworks: FastAPI, Django 5, Flask 3.
/// Databases: PostgreSQL, MySQL, SQL Server, MongoDB, Redis, SQLite.
/// </summary>
public partial class AddPythonSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DesignPatterns Python (IDs 5, 800–801, 1200–1205) ────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [DesignPatterns] ON;

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 5)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (5,N'Python','2026-01-01T00:00:00Z',N'Aisla acceso a datos detrás de repositorios abstractos.',N'Clase base AbstractRepository. Implementar con SQLAlchemy en infrastructure.',N'Repository Pattern (Python/FastAPI)',N'Repository',N'["mkdir -p app/repositories app/domain app/services"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 800)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (800,N'Python','2026-01-01T00:00:00Z',N'Capas: domain, application, infrastructure y adapters.',N'Domain no importa FastAPI. DI de FastAPI conecta las capas.',N'Clean Architecture (Python/FastAPI)',N'CleanArchitecture',N'["mkdir -p app/domain app/application/use_cases app/infrastructure app/adapters/api"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 801)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (801,N'Python','2026-01-01T00:00:00Z',N'Separa comandos y consultas con handlers.',N'Dataclasses para Command/Query. Handlers en application/.',N'CQRS (Python/FastAPI)',N'CQRS',N'["mkdir -p app/commands app/queries app/handlers"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1200)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1200,N'Python','2026-01-01T00:00:00Z',N'Ports como ABCs de Python, adapters como implementaciones concretas.',N'Usar ABC para ports. Inyectar adapters en los servicios de aplicación.',N'Hexagonal Architecture (Python)',N'HexagonalArchitecture',N'["mkdir -p app/core/ports app/core/domain app/infrastructure/adapters app/api"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1201)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1201,N'Python','2026-01-01T00:00:00Z',N'Aggregates, Entities y Value Objects con dataclasses.',N'Usar @dataclass para Value Objects. Domain events con publish/subscribe.',N'Domain-Driven Design (Python)',N'DomainDrivenDesign',N'["mkdir -p app/domain/aggregates app/domain/events app/domain/value_objects app/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1202)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1202,N'Python','2026-01-01T00:00:00Z',N'Estado reconstruido desde eventos inmutables.',N'Usar EventStoreDB o PostgreSQL como event store. Proyecciones para read models.',N'Event Sourcing (Python)',N'EventSourcing',N'["mkdir -p app/domain/events app/infrastructure/event_store app/application/projections"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1203)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1203,N'Python','2026-01-01T00:00:00Z',N'Servicios independientes con FastAPI comunicándose por HTTP.',N'Cada servicio tiene su propio main.py y requirements.txt. Usar httpx para comunicación entre servicios.',N'Microservices (Python/FastAPI)',N'Microservices',N'["mkdir -p services/items services/notifications services/gateway"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1204)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1204,N'Python','2026-01-01T00:00:00Z',N'Mediador que desacopla handlers de mensajes en Python.',N'Clase Mediator que registra handlers por tipo. Compatible con FastAPI DI.',N'Mediator (Python)',N'Mediator',N'["mkdir -p app/application"]');

            IF NOT EXISTS (SELECT 1 FROM [DesignPatterns] WHERE [Id] = 1205)
                INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                VALUES (1205,N'Python','2026-01-01T00:00:00Z',N'Orquesta transacciones distribuidas con compensación.',N'Clase OrderSaga con pasos async. Cada paso tiene compensación en caso de fallo.',N'Saga (Python/FastAPI)',N'Saga',N'["mkdir -p app/application/sagas"]');

            SET IDENTITY_INSERT [DesignPatterns] OFF;
            """);

        // ── Libraries Python (IDs 8–10, 800–802, 1200–1205) ─────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Libraries] ON;

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 8)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (8,N'Python',N'ORM','2026-01-01T00:00:00Z',N'ORM más popular para Python, soporta sync y async',NULL,N'pip install sqlalchemy',N'SQLAlchemy',N'sqlalchemy',98);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 9)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (9,N'Python',N'Validation','2026-01-01T00:00:00Z',N'Validación de datos con type hints',NULL,N'pip install pydantic',N'Pydantic',N'pydantic',97);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 10)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (10,N'Python',N'Migrations','2026-01-01T00:00:00Z',N'Migraciones de base de datos para SQLAlchemy',NULL,N'pip install alembic',N'Alembic',N'alembic',90);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 800)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (800,N'Python',N'HTTP Client','2026-01-01T00:00:00Z',N'Cliente HTTP moderno con soporte async',NULL,N'pip install httpx',N'httpx',N'httpx',89);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 801)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (801,N'Python',N'Background Tasks','2026-01-01T00:00:00Z',N'Cola de tareas distribuidas, ideal con Redis',NULL,N'pip install celery',N'Celery',N'celery',93);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 802)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (802,N'Python',N'Testing','2026-01-01T00:00:00Z',N'Framework de testing más popular de Python',NULL,N'pip install pytest pytest-asyncio',N'pytest',N'pytest',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1200)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1200,N'Python',N'Framework','2026-01-01T00:00:00Z',N'Framework web moderno y rápido para APIs',N'FastAPI',N'pip install fastapi uvicorn[standard]',N'FastAPI',N'fastapi',99);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1201)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1201,N'Python',N'API','2026-01-01T00:00:00Z',N'Extensión de Django para crear APIs REST',N'Django',N'pip install djangorestframework',N'Django REST Framework',N'djangorestframework',96);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1202)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1202,N'Python',N'ORM','2026-01-01T00:00:00Z',N'Integración de SQLAlchemy con Flask',N'Flask',N'pip install flask-sqlalchemy',N'Flask-SQLAlchemy',N'flask-sqlalchemy',88);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1203)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1203,N'Python',N'Cache','2026-01-01T00:00:00Z',N'Cliente Redis async para Python',NULL,N'pip install redis[asyncio]',N'aioredis',N'redis',87);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1204)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1204,N'Python',N'Authentication','2026-01-01T00:00:00Z',N'Manejo de tokens JWT en Python',NULL,N'pip install pyjwt',N'PyJWT',N'pyjwt',92);

            IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = 1205)
                INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                VALUES (1205,N'Python',N'Config','2026-01-01T00:00:00Z',N'Carga variables de entorno desde .env',NULL,N'pip install python-dotenv',N'Python-dotenv',N'python-dotenv',96);

            SET IDENTITY_INSERT [Libraries] OFF;
            """);

        // ── Templates Python — todas las combinaciones de DB ─────────────
        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Templates] ON;

            -- Dockerfile genérico (sirve FastAPI/Django/Flask)
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 800)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (800,N'Python',N'FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
EXPOSE 8000
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile para FastAPI con uvicorn',NULL,NULL,1,N'Dockerfile Python / FastAPI',N'dockerfile',1);

            -- Django Dockerfile
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 803)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (803,N'Python',N'FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
EXPOSE 8000
CMD ["python", "manage.py", "runserver", "0.0.0.0:8000"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile para Django 5',N'Django',NULL,1,N'Dockerfile Python / Django',N'dockerfile',1);

            -- Flask Dockerfile
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 804)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (804,N'Python',N'FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
EXPOSE 5000
CMD ["flask", "run", "--host=0.0.0.0"]','2026-01-01T00:00:00Z',NULL,N'Dockerfile para Flask 3',N'Flask',NULL,1,N'Dockerfile Python / Flask',N'dockerfile',1);

            -- PostgreSQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 801)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (801,N'Python',N'version: ''3.9''
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
  pgdata:','2026-01-01T00:00:00Z',N'PostgreSQL',N'Docker Compose para FastAPI/Django + PostgreSQL',NULL,N'DockerCompose',1,N'Compose Python + PostgreSQL',N'compose',1);

            -- MySQL
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 802)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (802,N'Python',N'version: ''3.9''
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
      MYSQL_DATABASE: {{DB_NAME}}
      MYSQL_ROOT_PASSWORD: secret
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
  mysqldata:','2026-01-01T00:00:00Z',N'MySQL',N'Docker Compose para Python + MySQL',NULL,N'DockerCompose',1,N'Compose Python + MySQL',N'compose',1);

            -- SQL Server
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1206)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1206,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: mssql+pyodbc://sa:YourStrong!Passw0rd@db:1433/{{DB_NAME}}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes
    depends_on:
      - db
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
  mssql-data:','2026-01-01T00:00:00Z',N'SqlServer',N'Docker Compose para Python + SQL Server',NULL,N'DockerCompose',1,N'Compose Python + SQL Server',N'compose',1);

            -- MongoDB
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1207)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1207,N'Python',N'version: ''3.9''
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
  mongodb-data:','2026-01-01T00:00:00Z',N'MongoDB',N'Docker Compose para Python + MongoDB',NULL,N'DockerCompose',1,N'Compose Python + MongoDB',N'compose',1);

            -- Redis
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1208)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1208,N'Python',N'version: ''3.9''
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
  redis-data:','2026-01-01T00:00:00Z',N'Redis',N'Docker Compose para Python + PostgreSQL + Redis',NULL,N'DockerCompose',1,N'Compose Python + Redis',N'compose',1);

            -- SQLite
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1209)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1209,N'Python',N'version: ''3.9''
services:
  app:
    build: .
    ports:
      - "{{APP_PORT}}:8000"
    environment:
      DATABASE_URL: sqlite:///app/data/app.db
    volumes:
      - sqlite-data:/app/data
volumes:
  sqlite-data:','2026-01-01T00:00:00Z',N'SQLite',N'Docker Compose para Python + SQLite',NULL,N'DockerCompose',1,N'Compose Python + SQLite',N'compose',1);

            -- Kubernetes
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1210)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1210,N'Python',N'apiVersion: apps/v1
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
            - containerPort: 8000
          env:
            - name: APP_ENV
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
      targetPort: 8000
  type: LoadBalancer','2026-01-01T00:00:00Z',NULL,N'Kubernetes Deployment y Service para Python',NULL,N'Kubernetes',1,N'K8s Python Deployment',N'k8s-deployment',1);

            -- CI
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1200)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1200,N'Python',N'name: CI
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
      - run: pytest --tb=short','2026-01-01T00:00:00Z',NULL,N'Pipeline CI para Python con pytest',NULL,NULL,1,N'CI Python GitHub Actions',N'ci',1);

            -- .gitignore
            IF NOT EXISTS (SELECT 1 FROM [Templates] WHERE [Id] = 1211)
                INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                VALUES (1211,N'Python',N'__pycache__/
*.py[cod]
*.pyo
*.pyd
.Python
.env
.env.*
.venv/
venv/
env/
*.egg-info/
dist/
build/
.pytest_cache/
.mypy_cache/
.ruff_cache/','2026-01-01T00:00:00Z',NULL,N'Gitignore para proyectos Python',NULL,NULL,1,N'.gitignore Python',N'gitignore',1);

            SET IDENTITY_INSERT [Templates] OFF;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [DesignPatterns] WHERE [Id] IN (5,800,801,1200,1201,1202,1203,1204,1205);");
        migrationBuilder.Sql("DELETE FROM [Libraries] WHERE [Id] IN (8,9,10,800,801,802,1200,1201,1202,1203,1204,1205);");
        migrationBuilder.Sql("DELETE FROM [Templates] WHERE [Id] IN (800,801,802,803,804,1200,1206,1207,1208,1209,1210,1211);");
    }
}
