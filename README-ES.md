<div align="center">
  <img src="src/ProjectForge.Web/wwwroot/img/logo.jpg" alt="TabBuilder Logo" width="200" />

# TabBuilder

> **Plataforma SaaS para generar proyectos de software completos con IA**
> Selecciona tu stack, recibe sugerencias inteligentes de patrones y librerías, genera el código base completo, crea el repositorio en GitHub y despliega en tu VPS — todo desde un wizard en el navegador.

</div>

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://www.docker.com)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-FF6B6B)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

---

## 📋 Tabla de contenidos

- [¿Qué es TabBuilder?](#-qué-es-tabbuilder)
- [Características](#-características)
- [Lenguajes y Frameworks soportados](#-lenguajes-y-frameworks-soportados)
- [Arquitectura del sistema](#-arquitectura-del-sistema)
- [Estructura del proyecto](#-estructura-del-proyecto)
- [Instalación rápida](#-instalación-rápida)
- [Variables de entorno](#-variables-de-entorno)
- [Rutas y endpoints](#-rutas-y-endpoints)
- [API REST](#-api-rest)
- [Integración con IA](#-integración-con-ia-multi-proveedor)
- [Tiempo real con SignalR](#-tiempo-real-con-signalr)
- [Base de datos](#-base-de-datos)
- [Seguridad](#-seguridad)
- [CI/CD y Docker](#-cicd-y-docker)
- [Tests](#-tests)
- [Contribuir](#-contribuir)
- [Licencia](#-licencia)

---

## 🚀 ¿Qué es TabBuilder?

TabBuilder es una plataforma SaaS construida con **ASP.NET Core 10 MVC** que automatiza el scaffolding de proyectos de software. A través de un wizard de 5 pasos, el usuario elige su lenguaje, framework, base de datos, infraestructura y patrones de diseño. La IA (Claude, con fallback a OpenAI y Gemini) sugiere patrones y librerías acordes al stack. El sistema luego ejecuta los comandos de scaffolding nativos (`dotnet new`, `composer create-project`, Spring Initializr, etc.), genera los archivos de infraestructura (Docker Compose, Kubernetes, GitHub Actions), crea el repositorio en GitHub, hace el commit inicial y puede desplegarlo vía SSH en uno o varios servidores VPS.

---

## ✨ Características

| Feature | Descripción |
|---|---|
| 🧙 **Wizard paso a paso** | 5 pasos guiados: arquitectura → framework → base de datos → infraestructura → patrones |
| 🤖 **IA multi-proveedor** | Sugerencias con Claude (Anthropic) → OpenAI → Gemini (fallback automático con caché en DB) |
| ⚡ **Scaffolding nativo** | Ejecuta `dotnet new`, `npm init`, `composer`, Spring Initializr, `django-admin`, etc. |
| 🐳 **Docker & Kubernetes** | Genera `docker-compose.yml`, `Dockerfile` y manifiestos K8s completos |
| 📤 **Push a GitHub** | Autenticación OAuth, creación automática del repo, commit inicial y push |
| 🖥️ **Deploy a VPS** | SSH a uno o múltiples servidores con credenciales cifradas AES-256 |
| 📄 **README con IA** | La IA genera documentación completa adaptada al stack elegido |
| 🔄 **GitHub Actions CI/CD** | Pipeline listo con build, test y push de imagen Docker a GHCR |
| 📡 **Tiempo real** | Logs de generación en vivo via SignalR (`/hubs/generation`) |
| 🔒 **Seguridad** | Cookies HttpOnly, cifrado AES-256, sesiones de 30 días |

---

## 🌐 Lenguajes y Frameworks soportados

| Lenguaje | Frameworks disponibles | Seeder | Patrones de diseño |
|---|---|---|---|
| **C# / .NET** | ASP.NET Core Web API, MVC, Blazor Server, Blazor WASM, Minimal API | `DotNetSeeder` | Repository, CQRS, Clean Architecture, DDD, Hexagonal |
| **Java** | Spring Boot 3.x, Quarkus, Micronaut | `JavaSeeder` | Repository, CQRS, Clean Architecture, DDD, Hexagonal |
| **Python** | FastAPI, Django 5, Flask 3 | `PythonSeeder` | Repository, CQRS, Clean Architecture |
| **PHP** | Laravel 11, Symfony | `PhpSeeder` (4 archivos modulares) | DDD, Clean Arch, Hexagonal, Repository, CQRS, Event Sourcing, Mediator, Saga, Microservices |
| **JavaScript** | Node.js, Express.js, NestJS, Next.js | `JavaScriptSeeder` | Repository, Clean Arch, Hexagonal, CQRS, Event Sourcing, Mediator, Microservices |
| **TypeScript** | NestJS (TS), Next.js (TS) | `JavaScriptSeeder` | CQRS (NestJS), Repository, Clean Architecture |

### Bases de datos soportadas

`PostgreSQL` · `MySQL` · `SQL Server` · `MongoDB` · `Redis` · `SQLite`

### Infraestructura

- **Docker Compose** — `docker-compose.yml` + `Dockerfile` listos para producción
- **Kubernetes** — `k8s/deployment.yaml`, `k8s/service.yaml`, `k8s/configmap.yaml`
- **GitHub Actions** — pipeline con build, test y push de imagen a GHCR

---

## 🏛️ Arquitectura del sistema

TabBuilder sigue una **Clean Architecture** en capas:

![## ARCHITECTURE](architecture.png)

```
┌─────────────────────────────────────────────────────┐
│                  TabBuilder.Web                   │
│  ASP.NET Core MVC · Razor Views · SignalR Hubs      │
│  Controllers: Auth / Wizard / Projects / Dashboard  │
└────────────────────────┬────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────┐
│              TabBuilder.Application               │
│  Use Cases · AI Services · Generator Service        │
│  DTOs · VPS Deployment Service                      │
└────────────────────────┬────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────┐
│              TabBuilder.Infrastructure            │
│  EF Core · Repositories · Seeders · Migrations      │
│  GitHub Service · Shell Executor · AES Encryption   │
└────────────────────────┬────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────┐
│                 TabBuilder.Core                   │
│  Entities · Interfaces · Enums · Domain Exceptions  │
└─────────────────────────────────────────────────────┘
```

---

## 📁 Estructura del proyecto

```
TabBuilder/
├── src/
│   ├── TabBuilder.Core/                    # Núcleo del dominio
│   │   ├── Entities/Entities.cs              # User, Project, WizardConfig, Template…
│   │   ├── Interfaces/IInterfaces.cs         # IProjectRepo, IGitHub, IAI, IEncryption…
│   │   ├── Enums/Enums.cs                    # ArchitectureType, DatabaseType, FrameworkType…
│   │   └── Exceptions/DomainException.cs
│   │
│   ├── TabBuilder.Application/             # Casos de uso y servicios
│   │   ├── AI/
│   │   │   ├── AnthropicAiSuggestionService.cs   # Llamadas directas a Claude
│   │   │   └── MultiProviderAiSuggestionService.cs  # Fallback + caché en DB
│   │   ├── Services/
│   │   │   ├── ProjectGeneratorService.cs          # Orquestador principal
│   │   │   ├── ProjectGeneratorService.Scaffolding.cs
│   │   │   ├── ProjectGeneratorService.BaseFiles.cs
│   │   │   ├── ProjectGeneratorService.Patterns.DotNet.cs
│   │   │   ├── ProjectGeneratorService.Patterns.Java.cs
│   │   │   ├── ProjectGeneratorService.Patterns.Laravel.cs
│   │   │   ├── ProjectGeneratorService.Patterns.Php.cs
│   │   │   ├── ProjectGeneratorService.Patterns.Symfony.cs
│   │   │   ├── ProjectGeneratorService.PostProcess.cs
│   │   │   └── VpsDeploymentService.cs
│   │   ├── UseCases/Projects/CreateProjectUseCase.cs
│   │   └── DTOs/WizardDtos.cs
│   │
│   ├── TabBuilder.Infrastructure/          # Implementaciones de infraestructura
│   │   ├── Data/AppDbContext.cs              # EF Core + registro de seeders
│   │   ├── Repositories/Repositories.cs
│   │   ├── Migrations/                       # Historial de migraciones EF Core
│   │   ├── Seeders/
│   │   │   ├── DotNet/DotNetSeeder.cs
│   │   │   ├── Java/JavaSeeder.cs
│   │   │   ├── JavaScript/JavaScriptSeeder.cs
│   │   │   ├── Php/PhpSeeder.cs + DesignPatternsSeeder.cs + LibrariesSeeder.cs + ProjectTemplatesSeeder.cs
│   │   │   ├── Python/PythonSeeder.cs
│   │   │   └── TypeScript/TypeScriptSeeder.cs
│   │   └── Infrastructure.cs                # ShellExecutor · GitHubService · AesEncryptionService
│   │
│   ├── TabBuilder.Web/                     # Aplicación MVC principal
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs            # OAuth GitHub
│   │   │   ├── WizardController.cs          # Wizard 5 pasos + IA + generación
│   │   │   ├── ProjectsController.cs        # CRUD proyectos
│   │   │   └── HomeController.cs
│   │   ├── Hubs/GenerationHub.cs            # SignalR hub
│   │   ├── Views/                           # Razor views (Auth, Wizard, Dashboard, Projects)
│   │   ├── wwwroot/
│   │   │   ├── css/app.css
│   │   │   ├── js/wizard.js                 # Lógica del wizard + conexión SignalR
│   │   │   └── img/logos/                   # Logos de frameworks, DBs e infra
│   │   └── Program.cs
│   │
│   └── TabBuilder.API/                    # API REST independiente con Swagger
│       ├── Controllers/TabBuilderController.cs
│       └── Program.cs
│
├── tests/
│   └── TabBuilder.Application.Tests/
│       ├── CreateProjectUseCaseTests.cs
│       └── ProjectDomainTests.cs
│
├── docker/
│   ├── Dockerfile                            # Imagen del MVC web
│   └── Dockerfile.API                        # Imagen de la API REST
├── docker-compose.yml                        # Web + API + SQL Server
├── .github/workflows/ci.yml                  # Build → Test → Docker Push → Deploy
├── scripts/setup.sh                          # Setup inicial interactivo
├── .env.example
└── TabBuilder.sln
```

---

## 🚀 Instalación rápida

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)
- Git
- Cuenta de GitHub (para crear la OAuth App)
- API Key de [Anthropic](https://console.anthropic.com/) (recomendado), [OpenAI](https://platform.openai.com/) o [Google Gemini](https://aistudio.google.com/)

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/tabbuilder.git
cd tabbuilder
```

### 2. Ejecutar el script de setup

```bash
chmod +x scripts/setup.sh && ./scripts/setup.sh
```

El script crea el archivo `.env` con valores por defecto, verifica las dependencias y aplica las migraciones si se detecta una base de datos local.

### 3. Crear la GitHub OAuth App

1. Ve a [https://github.com/settings/applications/new](https://github.com/settings/applications/new)
2. **Application name**: `TabBuilder`
3. **Homepage URL**: `http://localhost:5000`
4. **Authorization callback URL**: `http://localhost:5000/auth/github/callback`
5. Copia el **Client ID** y el **Client Secret** generados

### 4. Configurar variables de entorno

```bash
cp .env.example .env
nano .env   # o el editor de tu preferencia
```

Rellena al menos:

```env
GITHUB_CLIENT_ID=tu_client_id
GITHUB_CLIENT_SECRET=tu_client_secret
ANTHROPIC_API_KEY=sk-ant-...
ENCRYPTION_KEY=una_clave_aleatoria_de_minimo_32_caracteres
```

### 5. Arrancar la aplicación

![## DOCKER-ARCHITECTURE](docker-architecture.png)

---

```bash
# Con Docker Compose (recomendado — levanta Web + API + SQL Server)
docker-compose up --build

# O en modo desarrollo local (requiere SQL Server disponible)
cd src/TabBuilder.Web
dotnet run
```

La aplicación quedará disponible en:

| Servicio | URL |
|---|---|
| Web (MVC) | http://localhost:5000 |
| API REST (Swagger) | http://localhost:5001/swagger |
| SQL Server | localhost:1433 |

---

## 🔧 Variables de entorno

| Variable | Descripción | Requerida |
|---|---|---|
| `GITHUB_CLIENT_ID` | Client ID de la OAuth App de GitHub | ✅ |
| `GITHUB_CLIENT_SECRET` | Client Secret de la OAuth App de GitHub | ✅ |
| `ANTHROPIC_API_KEY` | API Key de Anthropic (Claude) | ⚠️ Recomendada |
| `OPENAI_API_KEY` | API Key de OpenAI (fallback #1) | Opcional |
| `GEMINI_API_KEY` | API Key de Google Gemini (fallback #2) | Opcional |
| `ENCRYPTION_KEY` | Clave AES-256 para cifrar contraseñas VPS (mín. 32 chars) | ✅ |
| `DB_PASSWORD` | Contraseña de SQL Server en Docker Compose | ✅ |

---

## 🗺️ Rutas y endpoints

### Web (MVC)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/` | Landing page |
| `GET` | `/auth/login` | Página de inicio de sesión |
| `GET` | `/auth/github` | Inicia flujo OAuth con GitHub |
| `GET` | `/auth/github/callback` | Callback OAuth de GitHub |
| `POST` | `/auth/logout` | Cerrar sesión |
| `GET` | `/dashboard` | Lista de proyectos del usuario autenticado |
| `POST` | `/projects` | Crea un proyecto via `CreateProjectUseCase` |
| `GET` | `/projects/{id}` | Detalle de un proyecto |
| `GET` | `/projects/{id}/logs` | Logs de generación en JSON |
| `POST` | `/projects/{id}/deploy` | Despliega el proyecto en VPS via SSH |
| `DELETE` | `/projects/{id}` | Elimina un proyecto |
| `GET` | `/wizard` | Inicio del wizard (paso 1) |
| `GET/POST` | `/wizard/step1` | Selección de arquitectura |
| `GET/POST` | `/wizard/step2` | Selección de framework |
| `GET/POST` | `/wizard/step3` | Selección de base de datos |
| `GET/POST` | `/wizard/step4` | Selección de infraestructura |
| `GET/POST` | `/wizard/step5` | Selección de patrones de diseño |
| `GET` | `/wizard/generate/{id}` | Vista de generación en tiempo real |
| `POST` | `/wizard/generate/{id}/start` | Inicia la generación (asíncrono) |
| `POST` | `/wizard/api/suggest` | Sugerencias IA (JSON) |
| `GET` | `/wizard/api/frameworks/{arch}` | Opciones de framework por arquitectura |

---

## 🔌 API REST

La API independiente (`TabBuilder.API`) expone los mismos recursos como REST puro con Swagger en `http://localhost:5001/swagger`.

Ejemplo de request para generar un proyecto:

```http
POST /api/v1/projects
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "my-api",
  "description": "REST API con FastAPI y PostgreSQL",
  "architecture": "Python",
  "framework": "FastAPI",
  "database": "PostgreSQL",
  "infrastructure": "DockerCompose",
  "designPatterns": ["Repository", "CQRS"],
  "isPrivate": false
}
```

---

## 🤖 Integración con IA (multi-proveedor)

El servicio `MultiProviderAiSuggestionService` implementa un sistema de **fallback automático**:

```
Anthropic (Claude) → OpenAI (GPT-4o) → Google Gemini
```

Si el proveedor primario falla o no tiene API key configurada, pasa automáticamente al siguiente. Todas las respuestas se **cachean en la base de datos por 30 días** para minimizar el consumo de tokens.

### Casos de uso de IA

**1. Sugerencias del wizard** (`POST /wizard/api/suggest`)

Recibe el stack seleccionado hasta ese punto y devuelve patrones de diseño y librerías recomendadas, tomando el catálogo de la base de datos como contexto.

```json
{
  "architecture": "Java",
  "framework": "SpringBoot",
  "database": "PostgreSQL",
  "infrastructure": "DockerCompose",
  "alreadySelectedPatterns": []
}
```

Respuesta:
```json
{
  "suggestedPatterns": ["Repository", "CQRS", "CleanArchitecture"],
  "suggestedLibraries": ["Spring Data JPA", "MapStruct", "Lombok"],
  "rationale": "Para una API REST con Spring Boot y PostgreSQL, el patrón Repository..."
}
```

**2. Generación de README** (automático al finalizar el proyecto)

La IA genera un `README.md` completo adaptado al stack, incluyendo comandos de instalación, variables de entorno, estructura de directorios y guía de CI/CD.

---

## 📡 Tiempo real con SignalR

La pantalla de generación se conecta al hub `/hubs/generation` y recibe eventos en vivo:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/generation")
    .build();

// Log de cada paso de generación
connection.on("ReceiveLog", (log) => {
    // log = { step, message, isError, timestamp }
    console.log(`[${log.step}] ${log.message}`);
});

// Cambio de estado del proyecto
connection.on("StatusChanged", (status) => {
    // "Generating" | "Published" | "Failed"
});

await connection.start();
await connection.invoke("JoinProject", projectId);
```

El servidor emite eventos a través de `IHubContext<GenerationHub>` desde `ProjectGeneratorService` a medida que ejecuta cada comando de scaffolding.

---

## 🗄️ Base de datos

### Tablas principales

| Tabla | Descripción |
|---|---|
| `Users` | Usuarios autenticados via GitHub OAuth |
| `Projects` | Proyectos generados (nombre, estado, repo URL, path local) |
| `WizardConfigs` | Configuración elegida en el wizard por proyecto |
| `ProjectLogs` | Log line a line de cada paso de generación |
| `ProjectTemplates` | Plantillas: Dockerfile, docker-compose, k8s, ci, .gitignore |
| `LibraryRecommendations` | Catálogo de librerías por lenguaje y framework |
| `DesignPatternEntries` | Catálogo de patrones de diseño por lenguaje |
| `VpsCredentials` | Credenciales SSH por proyecto (contraseña cifrada AES-256) |
| `AiSuggestionCache` | Caché de respuestas de IA (TTL 30 días) |

### Migraciones

```bash
# Aplicar migraciones pendientes
cd src/TabBuilder.Infrastructure
dotnet ef database update --startup-project ../TabBuilder.Web

# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion --startup-project ../TabBuilder.Web
```

### Agregar nuevas plantillas de infraestructura

```csharp
// En un seeder o migración:
new ProjectTemplate
{
    Name = "Compose Python + MongoDB",
    TemplateType = "compose",
    Architecture = ArchitectureType.Python,
    Database = DatabaseType.MongoDB,
    Infrastructure = InfrastructureType.DockerCompose,
    Content = "..." // Contenido con {{PROJECT_NAME}}, {{DB_PASSWORD}}, etc.
}
```

---

## 🔒 Seguridad

- Las **contraseñas de VPS** se almacenan cifradas con **AES-256** (`AesEncryptionService`)
- Los **tokens de GitHub** se almacenan en la sesión de cookie cifrada (Data Protection API de ASP.NET Core)
- Las cookies de sesión son `HttpOnly`, `SameSite=Lax` con expiración de **30 días**
- Las claves de Data Protection se persisten en un volumen Docker dedicado (`dataprotection-keys`)
- En producción, se recomienda almacenar los tokens de GitHub cifrados con la misma clave AES o en un vault externo (Azure Key Vault, HashiCorp Vault)

---

## 🐳 CI/CD y Docker

### Docker Compose

```bash
# Levantar todos los servicios (Web + API + SQL Server)
docker-compose up --build -d

# Ver logs
docker-compose logs -f web

# Parar y eliminar contenedores
docker-compose down
```

### Pipeline GitHub Actions (`.github/workflows/ci.yml`)

El pipeline se ejecuta en cada push a `main` o `develop` y en Pull Requests a `main`:

```
push / PR
    │
    ▼
[Build & Test] ── .NET 10 + SQL Server en servicio
    │               dotnet restore → build → test
    │               Sube resultados .trx como artifact
    │
    ▼ (solo en main)
[Docker Build & Push] ── Buildx + caché GHA
    │                     Push a GHCR (ghcr.io/org/tabbuilder)
    │                     Tags: latest, sha-XXXX, semver
    │
    ▼ (solo en main, environment: production)
[Deploy VPS] ── SSH con appleboy/ssh-action
                docker-compose pull && up -d
```

Secrets necesarios en GitHub para el deploy:

| Secret | Descripción |
|---|---|
| `DEPLOY_HOST` | IP o dominio del servidor de producción |
| `DEPLOY_USER` | Usuario SSH |
| `DEPLOY_SSH_KEY` | Clave privada SSH |

---

## 🧪 Tests

```bash
# Ejecutar todos los tests
dotnet test TabBuilder.sln

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Solo los tests de Application
dotnet test tests/TabBuilder.Application.Tests/
```

Los tests actuales cubren:

- `CreateProjectUseCaseTests` — validación del flujo de creación de proyectos
- `ProjectDomainTests` — reglas de dominio de las entidades `Project` y `WizardConfig`

---

## 🤝 Contribuir

```bash
# 1. Fork y clonar
git checkout -b feature/nueva-arquitectura

# 2. Hacer cambios y commits convencionales
git commit -m "feat: agregar soporte para Go/Gin"

# 3. Push y abrir Pull Request
git push origin feature/nueva-arquitectura
```

### Agregar soporte para un nuevo stack

1. Añadir valor en `ArchitectureType` y/o `FrameworkType` (`Core/Enums/Enums.cs`)
2. Crear `NuevoSeeder.cs` en `Infrastructure/Seeders/NuevoLenguaje/` con librerías y patrones
3. Registrar el seeder en `AppDbContext.OnModelCreating()`
4. Añadir caso en `ProjectGeneratorService.GetScaffoldCommands()`
5. Crear archivo `ProjectGeneratorService.Patterns.NuevoLenguaje.cs` con los templates de patrones
6. Actualizar `WizardController.GetFrameworkOptions()` para el nuevo stack
7. Añadir migración con `dotnet ef migrations add AddNuevoLenguajeSeedData`
8. Agregar el logo del framework en `wwwroot/img/logos/frameworks/`

### Convenciones de commits

```
feat:     nueva funcionalidad
fix:      corrección de bug
docs:     solo documentación
refactor: refactorización sin cambio funcional
test:     agregar o corregir tests
chore:    cambios de build, CI, dependencias
```

---

## 📄 Licencia

MIT — ver [LICENSE](LICENSE)

---

<div align="center">
  Construido con ❤️ sobre ASP.NET Core 10 · Claude AI · SignalR · Entity Framework Core · Docker
</div>