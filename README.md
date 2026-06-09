# ⚙ ProjectForge

> **Plataforma SaaS para generar proyectos de software completos con IA**
> Selecciona tu stack, la IA sugiere patrones y librerías, el sistema genera el código, crea el repositorio en GitHub y lo sube automáticamente.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

---

## ✨ Características

| Feature | Descripción |
|---|---|
| 🏗️ **Wizard paso a paso** | 5 pasos para configurar arquitectura, framework, BD, infraestructura y patrones |
| 🤖 **IA con Claude** | Sugerencias de patrones de diseño y librerías según tu stack |
| ⚡ **Generación automática** | Ejecuta `dotnet new`, `npm init`, `django-admin`, Spring Initializr, etc. |
| 🐳 **Docker & K8s** | Genera `docker-compose.yml`, `Dockerfile` y manifiestos de Kubernetes |
| 📤 **Push a GitHub** | Crea repo, commit inicial y push con GitHub OAuth |
| 🖥️ **Deploy a VPS** | SSH a uno o múltiples servidores con credenciales configurables |
| 📄 **README generado** | La IA redacta documentación completa con endpoints, comandos y CI/CD |
| 🔄 **GitHub Actions** | Pipeline CI/CD listo para usar |

---

## 🏛️ Arquitectura

```
ProjectForge/
├── src/
│   ├── ProjectForge.Core/             # Entidades, interfaces, enums
│   │   ├── Entities/                  # ApplicationUser, Project, WizardConfig, ...
│   │   ├── Interfaces/                # IProjectRepo, IGitHubService, IAiService, ...
│   │   └── Enums/                     # ArchitectureType, DatabaseType, ...
│   │
│   ├── ProjectForge.Application/      # Casos de uso
│   │   ├── AI/                        # AnthropicAiSuggestionService
│   │   ├── Services/                  # ProjectGeneratorService, VpsDeploymentService
│   │   └── DTOs/                      # WizardDtos, ProjectDtos
│   │
│   ├── ProjectForge.Infrastructure/   # Implementaciones
│   │   ├── Data/                      # AppDbContext, Seeds
│   │   ├── Repositories/              # EF Core repositories
│   │   ├── Migrations/                # EF Core migrations
│   │   └── Infrastructure.cs          # ShellExecutor, GitHubService, AES Encryption
│   │
│   └── ProjectForge.Web/              # ASP.NET Core MVC
│       ├── Controllers/               # Auth, Wizard, Dashboard, Projects
│       ├── Views/                     # Razor views + layouts
│       ├── Hubs/                      # SignalR GenerationHub
│       └── wwwroot/                   # CSS, JS
│
├── docker/
│   └── Dockerfile
├── docker-compose.yml
├── .github/workflows/ci.yml
└── scripts/setup.sh
```

---

## 🚀 Instalación rápida

### Prerrequisitos

- .NET 10 SDK
- Docker + Docker Compose
- Git
- Cuenta de GitHub (para crear OAuth App)
- API Key de Anthropic

### 1. Clonar y configurar

```bash
git clone https://github.com/tu-usuario/projectforge.git
cd projectforge
chmod +x scripts/setup.sh && ./scripts/setup.sh
```

### 2. Editar variables de entorno

```bash
nano .env
```

```env
GITHUB_CLIENT_ID=tu_client_id
GITHUB_CLIENT_SECRET=tu_client_secret
ANTHROPIC_API_KEY=sk-ant-...
ENCRYPTION_KEY=clave_aleatoria_de_32_caracteres
```

### 3. Crear GitHub OAuth App

1. Ve a https://github.com/settings/applications/new
2. **Homepage URL**: `http://localhost:5000`
3. **Authorization callback URL**: `http://localhost:5000/auth/github/callback`
4. Copia Client ID y Client Secret al `.env`

### 4. Arrancar

```bash
# Con Docker Compose (recomendado)
docker-compose up --build

# O en modo desarrollo
cd src/ProjectForge.Web
dotnet run
```

La aplicación estará disponible en **http://localhost:5000**

---

## 🗺️ Rutas principales

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/` | Landing page |
| `GET` | `/auth/login` | Página de login |
| `GET` | `/auth/github` | Inicia flujo OAuth con GitHub |
| `GET` | `/auth/github/callback` | Callback OAuth |
| `POST` | `/auth/logout` | Cerrar sesión |
| `GET` | `/dashboard` | Lista de proyectos del usuario |
| `GET` | `/wizard` | Inicio del wizard |
| `GET/POST` | `/wizard/step1..5` | Pasos del wizard |
| `GET` | `/wizard/generate/{id}` | Vista de generación en tiempo real |
| `POST` | `/wizard/generate/{id}/start` | Inicia generación (async) |
| `POST` | `/wizard/api/suggest` | API de sugerencias IA |
| `GET` | `/wizard/api/frameworks/{arch}` | Opciones de framework por arquitectura |
| `GET` | `/projects/{id}` | Detalle del proyecto |
| `GET` | `/projects/{id}/logs` | Logs de generación (JSON) |
| `POST` | `/projects/{id}/deploy` | Desplegar en VPS via SSH |
| `DELETE` | `/projects/{id}` | Eliminar proyecto |

---

## 🧩 Stacks soportados

### Arquitecturas
| Stack | Scaffold Command | Frameworks |
|---|---|---|
| **.NET** | `dotnet new webapi/mvc/blazorserver` | ASP.NET Core, Blazor, Minimal API |
| **Java** | Spring Initializr API (`curl`) | Spring Boot 3.x, Quarkus, Micronaut |
| **Python** | `mkdir` + estructura manual | FastAPI, Django 5, Flask 3 |
| **Node.js** | `npm init` + NestJS CLI | Express, NestJS, Next.js |
| **Laravel** | `composer create-project` | Laravel 11, Symfony |

### Bases de datos
PostgreSQL · MySQL · SQL Server · MongoDB · Redis · SQLite

### Infraestructura
- **Docker Compose**: genera `docker-compose.yml` + `Dockerfile` listos
- **Kubernetes**: genera `k8s/deployment.yaml`, `k8s/service.yaml`, etc.
- **GitHub Actions**: pipeline CI/CD con build, test y push de imagen Docker

---

## 🤖 Integración con IA (Claude)

El servicio `AnthropicAiSuggestionService` llama a la API de Claude para:

1. **Sugerir patrones** (`/wizard/api/suggest`): analiza el stack y recomienda Repository, CQRS, Clean Architecture, etc.
2. **Generar README** (paso final): redacta documentación completa con endpoints, variables de entorno, comandos y guía CI/CD.

El modelo recibe como contexto el catálogo de patrones y librerías almacenado en la base de datos, lo que permite personalizar las sugerencias.

---

## 🔌 Tiempo real con SignalR

La pantalla de generación (`/wizard/generate/{id}`) se conecta al hub `/hubs/generation` y recibe eventos:

```javascript
connection.on("ReceiveLog", (log) => {
    // { step, message, isError, timestamp }
});
connection.on("StatusChanged", (status) => {
    // "Generating" | "Published" | "Failed"
});
```

---

## 🔒 Seguridad

- Las contraseñas de VPS se almacenan **cifradas con AES-256** (`AesEncryptionService`)
- Los tokens de GitHub se almacenan en la sesión de cookie cifrada
- En producción, considera almacenar tokens en un vault o cifrarlos con la misma clave AES
- Las cookies de sesión son `HttpOnly`, `SameSite=Lax` y con expiración de 30 días

---

## 🗄️ Base de datos

```
Tablas principales:
  Users           — Usuarios autenticados con GitHub
  Projects        — Proyectos generados
  WizardConfigs   — Configuración elegida en el wizard
  ProjectLogs     — Logs de cada paso de generación
  Templates       — Plantillas: Dockerfile, docker-compose, k8s, ci, gitignore
  Libraries       — Catálogo de librerías recomendadas
  DesignPatterns  — Catálogo de patrones de diseño
  VpsCredentials  — Credenciales SSH por proyecto (contraseña AES-256)
```

Agregar nuevas plantillas:
```csharp
// En AppDbContext.SeedTemplates() o via migración:
new ProjectTemplate {
    Name = "Compose Python + MongoDB",
    TemplateType = "compose",
    Architecture = ArchitectureType.Python,
    Database = DatabaseType.MongoDB,
    Infrastructure = InfrastructureType.DockerCompose,
    Content = "..." // Contenido con {{VARIABLES}}
}
```

---

## 🤝 Contribuir

```bash
# Fork del repositorio
git checkout -b feature/nueva-arquitectura
# ... cambios ...
git commit -m "feat: agregar soporte para Go/Gin"
git push origin feature/nueva-arquitectura
# Abrir Pull Request
```

### Para agregar un nuevo stack:
1. Añadir valor en `ArchitectureType` y/o `FrameworkType` (Core/Enums)
2. Añadir caso en `ProjectGeneratorService.GetScaffoldCommands()`
3. Añadir plantillas en `AppDbContext.SeedTemplates()`
4. Añadir librerías en `AppDbContext.SeedLibraries()`
5. Actualizar `WizardController.GetFrameworkOptions()`

---

## 📄 Licencia

MIT — ver [LICENSE](LICENSE)
