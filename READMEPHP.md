# README PHP - Cambios respecto al proyecto original

Este documento resume los cambios e implementaciones que se agregaron al proyecto
original de ProjectForge, con foco en el soporte PHP y en la evolucion general del
generador.

## Punto de partida

El proyecto original ya tenia la base de:

- wizard de configuracion por pasos
- autenticacion con GitHub
- persistencia con EF Core
- generacion de proyectos por stack
- integracion con IA para sugerencias
- despliegue y scaffolding de infraestructura

Sobre esa base, el trabajo actual amplio el alcance real del generador, corrigio
varios flujos rotos y elevo el soporte PHP para que deje de ser un caso minimo.

## Cambios principales

### 1. PHP paso de scaffold basico a scaffold utilizable

Se reforzo la generacion de proyectos PHP para que no dependa solo de carpetas
vacías o archivos placeholder.

Ahora el generador puede crear estructuras concretas para:

- Laravel
- Symfony

Y para patrones de arquitectura como:

- Repository
- Clean Architecture
- Hexagonal Architecture
- Domain-Driven Design
- Event Sourcing
- Microservices

### 2. Laravel quedo con implementaciones reales

Los patrones PHP de Laravel dejaron de ser simples marcadores y ahora incluyen
archivos funcionales como:

- contratos o puertos
- modelos Eloquent
- repositorios concretos
- casos de uso
- providers para registrar dependencias
- migraciones iniciales

Ejemplos de lo agregado:

- `app/Contracts/ProjectRepositoryInterface.php`
- `app/Repositories/ProjectRepository.php`
- `app/Application/UseCases/CreateProjectUseCase.php`
- `app/Providers/ProjectRepositoryServiceProvider.php`
- `database/migrations/2026_01_01_000000_create_projects_table.php`

### 3. Symfony quedo al mismo nivel que Laravel

Este fue uno de los cambios mas importantes.

Antes Symfony generaba solo estructuras reducidas. Ahora el scaffold Symfony
tambien incluye:

- entidad Doctrine
- contrato o puerto
- caso de uso
- repositorio o adaptador de persistencia
- migracion Doctrine
- binding de servicios en `config/services.yaml`

#### Repository Pattern (Symfony)

Genera:

- `src/Entity/Project.php`
- `src/Contract/ProjectRepositoryInterface.php`
- `src/Repository/ProjectRepository.php`
- `src/Application/UseCase/CreateProjectUseCase.php`
- `migrations/Version20260101000000.php`

#### Clean Architecture (Symfony)

Genera:

- `src/Domain/Entity/Project.php`
- `src/Contract/ProjectRepositoryInterface.php`
- `src/Application/UseCase/CreateProjectUseCase.php`
- `src/Entity/Project.php`
- `src/Infrastructure/Persistence/DoctrineProjectRepository.php`
- `migrations/Version20260101000000.php`

#### Hexagonal Architecture (Symfony)

Genera:

- `src/Domain/Entity/Project.php`
- `src/Port/ProjectRepositoryPort.php`
- `src/Application/UseCase/CreateProjectUseCase.php`
- `src/Entity/Project.php`
- `src/Adapters/Persistence/DoctrineProjectRepository.php`
- `migrations/Version20260101000000.php`

Ademas, el generador inserta automaticamente el binding correspondiente en
`config/services.yaml` para que Symfony resuelva la implementacion correcta.

### 4. La IA ya no depende solo de texto hardcodeado

El paso de sugerencias del wizard paso a apoyarse en el catalogo real almacenado
en base de datos:

- `DesignPatterns`
- `Libraries`

Tambien se ajusto la logica para trabajar con multiples proveedores de IA, con
fallback entre servicios cuando uno falla.

Esto reduce la dependencia de respuestas fijas y hace que las sugerencias se
acoplen al stack elegido.

### 5. El wizard quedo mas estricto con las opciones validas

Se ajusto el flujo de creacion para mostrar solo opciones que existen en la base
de datos, en lugar de mezclar opciones duras con opciones configurables.

Eso aplica especialmente a:

- patrones de diseño
- librerias recomendadas
- frameworks disponibles por arquitectura

### 6. GitHub OAuth quedo mas robusto

Se corrigieron varios problemas del flujo de autenticacion:

- el callback ahora maneja cancelacion o denegacion de acceso
- el logout cierra la cookie de sesion correctamente
- se evita intentar hacer `SignOutAsync` con el handler de GitHub
- la revocacion del grant de GitHub se hace como paso separado

### 7. El proceso de generacion paso a ser mas completo

Se reforzo el generador para que, durante la creacion de un proyecto, tambien
pueda producir:

- estructura de carpetas
- archivos base del framework
- archivos de patrones de arquitectura
- templates de Docker y Docker Compose
- manifiestos de Kubernetes
- pipelines de GitHub Actions
- README generado por IA
- logs en tiempo real durante la generacion

### 8. Se mejoro la integracion en tiempo real

La pantalla de generacion sigue el avance del proceso por SignalR:

- eventos de logs
- cambio de estado
- error o exito de pasos intermedios

Eso permite ver el scaffold mientras se construye y no solo al final.

## Cambios tecnicos por area

### Backend

- se ampliaron los casos de uso del generador
- se agrego resolucion mas precisa de patrones PHP por framework
- se incorporo binding automatico para Laravel y Symfony
- se refino el manejo de sugerencias de IA con cache y fallback

### Persistencia

- se actualizaron seeds para patrones, librerias y templates
- se extendio el catalogo de patrones PHP en base de datos
- se agregaron migraciones de soporte para datos semilla y para proyectos generados

### Frontend

- se ajusto el wizard para consumir solo datos validos
- se corrigieron validaciones del paso final
- se mejoro la experiencia del flujo de autenticacion y generacion

### Infraestructura

- se conservaron los templates de Docker y Kubernetes
- se integraron comandos de scaffolding por stack
- se mantuvo la generacion automatica de repositorios GitHub

## Resultado actual

En comparacion con el proyecto original, ahora ProjectForge no solo configura un
wizard y genera proyectos base. Tambien:

- genera estructuras PHP mas cercanas a produccion
- respeta el estilo arquitectonico elegido
- registra dependencias en el contenedor del framework
- usa el catalogo de base de datos como fuente de verdad para sugerencias
- maneja mejor el flujo de GitHub OAuth
- mantiene el proceso de generacion observable en tiempo real

## Verificacion

Los cambios principales fueron validados con compilacion de los proyectos de la
solucion y con publicacion del frontend ASP.NET Core para asegurar que el
generador sigue construyendo correctamente.

