using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelRepositoryPatternFiles()
    {
        return new[]
        {
            ("app/Repositories/Contracts/ProjectRepositoryInterface.php", """
<?php

namespace App\Repositories\Contracts;

use App\Models\Project;

interface ProjectRepositoryInterface
{
    public function all(): array;

    public function findById(int $id): ?Project;

    public function create(array $data): Project;

    public function update(int $id, array $data): ?Project;

    public function delete(int $id): void;
}
"""),
            ("app/Repositories/EloquentProjectRepository.php", """
<?php

namespace App\Repositories;

use App\Models\Project;
use App\Repositories\Contracts\ProjectRepositoryInterface;

final class EloquentProjectRepository implements ProjectRepositoryInterface
{
    public function all(): array
    {
        return Project::query()->orderByDesc('created_at')->get()->all();
    }

    public function findById(int $id): ?Project
    {
        return Project::query()->find($id);
    }

    public function create(array $data): Project
    {
        return Project::query()->create($data);
    }

    public function update(int $id, array $data): ?Project
    {
        $project = $this->findById($id);
        if ($project === null) {
            return null;
        }

        $project->fill($data);
        $project->save();

        return $project;
    }

    public function delete(int $id): void
    {
        $project = $this->findById($id);

        if ($project !== null) {
            $project->delete();
        }
    }
}
"""),
            ("app/Models/Project.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Project extends Model
{
    protected $fillable = [
        'name',
        'description',
    ];
}
"""),
            ("app/Http/Controllers/ProjectController.php", """
<?php

namespace App\Http\Controllers;

use App\Repositories\Contracts\ProjectRepositoryInterface;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

final class ProjectController extends Controller
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function index(): JsonResponse
    {
        return response()->json($this->projects->all());
    }

    public function show(int $id): JsonResponse
    {
        $project = $this->projects->findById($id);

        return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
    }

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'min:3'],
            'description' => ['nullable', 'string'],
        ]);

        $project = $this->projects->create($data);

        return response()->json($project, 201);
    }

    public function update(int $id, Request $request): JsonResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'min:3'],
            'description' => ['nullable', 'string'],
        ]);

        $project = $this->projects->update($id, $data);

        return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
    }

    public function destroy(int $id): JsonResponse
    {
        $this->projects->delete($id);

        return response()->json(null, 204);
    }
}
"""),
            ("app/Providers/RepositoryServiceProvider.php", """
<?php

namespace App\Providers;

use App\Repositories\Contracts\ProjectRepositoryInterface;
use App\Repositories\EloquentProjectRepository;
use Illuminate\Support\ServiceProvider;

final class RepositoryServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryInterface::class, EloquentProjectRepository::class);
    }
}
"""),
            ("routes/web.php", """
<?php

use App\Http\Controllers\ProjectController;
use Illuminate\Support\Facades\Route;

Route::get('/projects', [ProjectController::class, 'index']);
Route::get('/projects/{id}', [ProjectController::class, 'show']);
Route::post('/projects', [ProjectController::class, 'store']);
Route::put('/projects/{id}', [ProjectController::class, 'update']);
Route::delete('/projects/{id}', [ProjectController::class, 'destroy']);
"""),
            ("database/migrations/2026_01_01_000000_create_projects_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('projects', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('projects');
    }
};
"""),
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelMicroservicesPatternFiles(DatabaseType database)
    {
        var dbConnection = GetLaravelDatabaseConnection(database);
        var projectsDbEnv = GetLaravelMicroservicesDbEnvironment(database, "projects");
        var notificationsDbEnv = GetLaravelMicroservicesDbEnvironment(database, "notifications");

        return new[]
        {
            ("app/Http/Controllers/GatewayController.php", """
<?php

namespace App\Http\Controllers;

use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

final class GatewayController extends Controller
{
    public function health(): JsonResponse
    {
        return response()->json([
            'service' => 'gateway',
            'status' => 'ok',
        ]);
    }

    public function projects(Request $request, string $path = ''): JsonResponse
    {
        return $this->forward($request, env('PROJECTS_SERVICE_URL', 'http://projects-service:8080'), $path);
    }

    public function notifications(Request $request, string $path = ''): JsonResponse
    {
        return $this->forward($request, env('NOTIFICATIONS_SERVICE_URL', 'http://notifications-service:8080'), $path);
    }

    private function forward(Request $request, string $baseUrl, string $path): JsonResponse
    {
        $url = rtrim($baseUrl, '/') . '/' . ltrim($path, '/');
        $response = Http::acceptJson()->send($request->method(), $url, [
            'query' => $request->query(),
            'json' => $request->all(),
        ]);

        return response()->json($response->json(), $response->status());
    }
}
"""),
            ("routes/api.php", """
<?php

use App\Http\Controllers\GatewayController;
use Illuminate\Support\Facades\Route;

Route::get('/health', [GatewayController::class, 'health']);
Route::prefix('projects')->group(function () {
    Route::any('{path?}', [GatewayController::class, 'projects'])->where('path', '.*');
});
Route::prefix('notifications')->group(function () {
    Route::any('{path?}', [GatewayController::class, 'notifications'])->where('path', '.*');
});
"""),
            (".env.example", """
APP_NAME=Laravel
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:8080
PROJECTS_SERVICE_URL=http://projects-service:8080
NOTIFICATIONS_SERVICE_URL=http://notifications-service:8080
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
"""),
            ("services/projects/.env.example", projectsDbEnv),
            ("services/notifications/.env.example", notificationsDbEnv),
            ("services/projects/routes/api.php", """
<?php

use Illuminate\Support\Facades\Route;

Route::get('/health', fn () => response()->json(['service' => 'projects', 'status' => 'ok']));
Route::get('/projects', fn () => response()->json([
    ['id' => 1, 'name' => 'Project Alpha'],
    ['id' => 2, 'name' => 'Project Beta'],
]));
"""),
            ("services/notifications/routes/api.php", """
<?php

use Illuminate\Support\Facades\Route;

Route::get('/health', fn () => response()->json(['service' => 'notifications', 'status' => 'ok']));
Route::get('/notifications', fn () => response()->json([
    ['id' => 1, 'message' => 'Notification ready'],
]));
"""),
            ("services/projects/app/Http/Controllers/ProjectController.php", """
<?php

namespace App\Http\Controllers;

use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

final class ProjectController extends Controller
{
    public function index(): JsonResponse
    {
        return response()->json([
            ['id' => 1, 'name' => 'Project Alpha'],
            ['id' => 2, 'name' => 'Project Beta'],
        ]);
    }

    public function store(Request $request): JsonResponse
    {
        return response()->json([
            'message' => 'Project created by projects service.',
            'data' => $request->all(),
        ], 201);
    }
}
"""),
            ("services/notifications/app/Http/Controllers/NotificationController.php", """
<?php

namespace App\Http\Controllers;

use Illuminate\Http\JsonResponse;

final class NotificationController extends Controller
{
    public function index(): JsonResponse
    {
        return response()->json([
            ['id' => 1, 'message' => 'Notification ready'],
        ]);
    }
}
"""),
            ("services/projects/Dockerfile", BuildLaravelMicroservicesDockerfile(database)),
            ("services/notifications/Dockerfile", BuildLaravelMicroservicesDockerfile(database)),
            ("docker-compose.yml", BuildLaravelMicroservicesCompose(database)),
            (".github/workflows/ci.yml", """
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
      - name: Validate gateway
        run: php artisan route:list
""")
        };
    }

    private static string BuildLaravelMicroservicesDockerfile(DatabaseType database) =>
        database switch
        {
            DatabaseType.SqlServer => """
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

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi
EXPOSE 8080
CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]
""",
            DatabaseType.MongoDB => """
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

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi
EXPOSE 8080
CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]
""",
            DatabaseType.Redis => """
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

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi
EXPOSE 8080
CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]
""",
            _ => """
FROM php:8.3-cli

WORKDIR /var/www/html

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl unzip libzip-dev libpng-dev libicu-dev libonig-dev libxml2-dev libssl-dev pkg-config \
    && docker-php-ext-install pdo pdo_mysql pdo_pgsql pdo_sqlite mbstring zip intl bcmath \
    && rm -rf /var/lib/apt/lists/*

COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .

RUN if [ -f composer.json ]; then composer install --no-interaction --prefer-dist --optimize-autoloader; fi
EXPOSE 8080
CMD ["php", "-S", "0.0.0.0:8080", "-t", "public"]
"""
        };

    private static string BuildLaravelMicroservicesCompose(DatabaseType database) =>
        database switch
        {
            DatabaseType.MySQL => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: mysql
      DB_HOST: projects-db
      DB_PORT: 3306
      DB_DATABASE: projects_db
      DB_USERNAME: root
      DB_PASSWORD: secret
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - projects-db
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: mysql
      DB_HOST: notifications-db
      DB_PORT: 3306
      DB_DATABASE: notifications_db
      DB_USERNAME: root
      DB_PASSWORD: secret
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - notifications-db
      - rabbitmq

  projects-db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: secret
      MYSQL_DATABASE: projects_db

  notifications-db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: secret
      MYSQL_DATABASE: notifications_db

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
            DatabaseType.PostgreSQL => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: pgsql
      DB_HOST: projects-db
      DB_PORT: 5432
      DB_DATABASE: projects_db
      DB_USERNAME: postgres
      DB_PASSWORD: secret
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - projects-db
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: pgsql
      DB_HOST: notifications-db
      DB_PORT: 5432
      DB_DATABASE: notifications_db
      DB_USERNAME: postgres
      DB_PASSWORD: secret
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - notifications-db
      - rabbitmq

  projects-db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: projects_db
      POSTGRES_PASSWORD: secret

  notifications-db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: notifications_db
      POSTGRES_PASSWORD: secret

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
            // Previously fell into the SQLite default below — no sqlserver/mongo/redis container
            // was ever started, even though .env.example and the Dockerfile (which already
            // installs the matching PECL extension) both expected one.
            DatabaseType.SqlServer => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: sqlserver
      DB_PORT: 1433
      DB_DATABASE: projects_db
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - sqlserver
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: sqlsrv
      DB_HOST: sqlserver
      DB_PORT: 1433
      DB_DATABASE: notifications_db
      DB_USERNAME: sa
      DB_PASSWORD: YourStrong!Passw0rd
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - sqlserver
      - rabbitmq

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourStrong!Passw0rd

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
            DatabaseType.MongoDB => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: mongodb
      DB_HOST: mongo
      DB_PORT: 27017
      DB_DATABASE: projects_db
      MONGODB_URI: mongodb://mongo:27017/projects_db
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - mongo
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: mongodb
      DB_HOST: mongo
      DB_PORT: 27017
      DB_DATABASE: notifications_db
      MONGODB_URI: mongodb://mongo:27017/notifications_db
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - mongo
      - rabbitmq

  mongo:
    image: mongo:7

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
            DatabaseType.Redis => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: redis
      REDIS_HOST: redis
      REDIS_PORT: 6379
      CACHE_STORE: redis
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: redis
      REDIS_HOST: redis
      REDIS_PORT: 6379
      CACHE_STORE: redis
      QUEUE_CONNECTION: rabbitmq
    depends_on:
      - redis
      - rabbitmq

  redis:
    image: redis:7-alpine

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
            _ => """
version: '3.9'
services:
  gateway:
    build: .
    ports:
      - "8080:8080"
    environment:
      PROJECTS_SERVICE_URL: http://projects-service:8080
      NOTIFICATIONS_SERVICE_URL: http://notifications-service:8080
    depends_on:
      - projects-service
      - notifications-service
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      DB_CONNECTION: sqlite
      QUEUE_CONNECTION: rabbitmq
    volumes:
      - projects-db:/var/www/html/database
    depends_on:
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      DB_CONNECTION: sqlite
      QUEUE_CONNECTION: rabbitmq
    volumes:
      - notifications-db:/var/www/html/database
    depends_on:
      - rabbitmq

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

volumes:
  projects-db:
  notifications-db:
"""
        };

    private static string GetLaravelDatabaseConnection(DatabaseType database) => database switch
    {
        DatabaseType.MySQL => "mysql",
        DatabaseType.PostgreSQL => "pgsql",
        DatabaseType.SqlServer => "sqlsrv",
        DatabaseType.MongoDB => "mongodb",
        DatabaseType.Redis => "redis",
        _ => "sqlite"
    };

    private static string GetLaravelMicroservicesDbEnvironment(DatabaseType database, string serviceName)
    {
        var connection = GetLaravelDatabaseConnection(database);
        var databaseName = $"{serviceName}_db";

        return database switch
        {
            DatabaseType.MySQL => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_HOST={serviceName}-db
DB_PORT=3306
DB_DATABASE={databaseName}
DB_USERNAME=root
DB_PASSWORD=secret
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
""",
            DatabaseType.PostgreSQL => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_HOST={serviceName}-db
DB_PORT=5432
DB_DATABASE={databaseName}
DB_USERNAME=postgres
DB_PASSWORD=secret
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
""",
            // Previously fell into the SQLite default below with DB_CONNECTION overridden to
            // "sqlsrv"/"mongodb"/"redis" but DB_DATABASE still pointing at a .sqlite file path —
            // an internally inconsistent env file that couldn't actually reach the selected
            // database. BuildLaravelMicroservicesDockerfile already installs the sqlsrv/mongodb
            // PECL extensions for these DBs; this now matches it with real connection details.
            DatabaseType.SqlServer => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_HOST=sqlserver
DB_PORT=1433
DB_DATABASE={databaseName}
DB_USERNAME=sa
DB_PASSWORD=YourStrong!Passw0rd
DB_ENCRYPT=false
DB_TRUST_SERVER_CERTIFICATE=true
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
""",
            DatabaseType.MongoDB => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_HOST=mongo
DB_PORT=27017
DB_DATABASE={databaseName}
MONGODB_URI=mongodb://mongo:27017/{databaseName}
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
""",
            DatabaseType.Redis => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_HOST=redis
DB_PORT=6379
CACHE_STORE=redis
CACHE_DRIVER=redis
SESSION_DRIVER=redis
REDIS_CLIENT=phpredis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_DB=0
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
""",
            _ => $"""
APP_NAME={serviceName}
APP_ENV=local
APP_KEY=
APP_DEBUG=true
APP_URL=http://localhost:{(serviceName == "projects" ? 8081 : 8082)}
DB_CONNECTION={connection}
DB_DATABASE=/var/www/html/database/{serviceName}.sqlite
QUEUE_CONNECTION=rabbitmq
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
"""
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelEventSourcingPatternFiles()
    {
        return new[]
        {
            ("app/Domain/Aggregates/AggregateRoot.php", """
<?php

namespace App\Domain\Aggregates;

use ReflectionClass;

abstract class AggregateRoot
{
    /** @var array<int, object> */
    private array $recordedEvents = [];

    protected function recordThat(object $event): void
    {
        $this->recordedEvents[] = $event;
    }

    public function releaseEvents(): array
    {
        $events = $this->recordedEvents;
        $this->recordedEvents = [];

        return $events;
    }

    protected function replay(array $events): void
    {
        foreach ($events as $event) {
            $this->apply($event);
        }
    }

    protected function apply(object $event): void
    {
        $method = 'apply' . (new ReflectionClass($event))->getShortName();
        if (method_exists($this, $method)) {
            $this->{$method}($event);
        }
    }
}
"""),
            ("app/Domain/ValueObjects/ProjectName.php", """
<?php

namespace App\Domain\ValueObjects;

use DomainException;

final class ProjectName
{
    public function __construct(public readonly string $value)
    {
        if (mb_strlen(trim($this->value)) < 3) {
            throw new DomainException('Project name must contain at least 3 characters.');
        }
    }

    public static function from(string $value): self
    {
        return new self(trim($value));
    }
}
"""),
            ("app/Domain/Events/ProjectCreated.php", """
<?php

namespace App\Domain\Events;

final class ProjectCreated
{
    public function __construct(
        public readonly string $aggregateId,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Domain/Events/ProjectRenamed.php", """
<?php

namespace App\Domain\Events;

final class ProjectRenamed
{
    public function __construct(
        public readonly string $aggregateId,
        public readonly string $previousName,
        public readonly string $newName
    ) {}
}
"""),
            ("app/Domain/Events/ProjectDeleted.php", """
<?php

namespace App\Domain\Events;

final class ProjectDeleted
{
    public function __construct(public readonly string $aggregateId) {}
}
"""),
            ("app/Domain/Events/ProjectDescriptionChanged.php", """
<?php

namespace App\Domain\Events;

final class ProjectDescriptionChanged
{
    public function __construct(
        public readonly string $aggregateId,
        public readonly ?string $previousDescription,
        public readonly ?string $newDescription
    ) {}
}
"""),
            ("app/Domain/Events/StoredEvent.php", """
<?php

namespace App\Domain\Events;

use DateTimeImmutable;

final class StoredEvent
{
    public function __construct(
        public readonly ?int $eventId = null,
        public readonly string $aggregateId = '',
        public readonly string $eventType = '',
        public readonly array $payload = [],
        public readonly ?DateTimeImmutable $createdAt = null
    ) {}
}
"""),
            ("app/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

use App\Domain\Aggregates\AggregateRoot;
use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectDeleted;
use App\Domain\Events\ProjectDescriptionChanged;
use App\Domain\Events\ProjectRenamed;
use App\Domain\ValueObjects\ProjectName;
use DomainException;
use Illuminate\Support\Str;

final class Project extends AggregateRoot
{
    public function __construct(
        public string $id = '',
        public string $name = '',
        public ?string $description = null,
        public bool $deleted = false
    ) {}

    public static function create(ProjectName $name, ?string $description = null): self
    {
        $project = new self(Str::uuid()->toString());
        $project->applyProjectCreated(new ProjectCreated($project->id, $name->value, self::normalizeDescription($description)));
        $project->recordThat(new ProjectCreated($project->id, $project->name, $project->description));

        return $project;
    }

    public static function reconstituteFromEvents(array $events): self
    {
        $project = new self();
        $project->replay($events);

        return $project;
    }

    public static function fromHistory(array $events): self
    {
        return self::reconstituteFromEvents($events);
    }

    public function rename(ProjectName $name): void
    {
        if ($this->deleted) {
            throw new DomainException('Cannot rename a deleted project.');
        }

        if ($this->name === $name->value) {
            throw new DomainException('The new project name must be different.');
        }

        $previousName = $this->name;
        $this->applyProjectRenamed(new ProjectRenamed($this->id, $previousName, $name->value));
        $this->recordThat(new ProjectRenamed($this->id, $previousName, $this->name));
    }

    public function delete(): void
    {
        if ($this->deleted) {
            return;
        }

        $this->deleted = true;
        $this->recordThat(new ProjectDeleted($this->id));
    }

    public function changeDescription(?string $description): void
    {
        if ($this->deleted) {
            throw new DomainException('Cannot update a deleted project.');
        }

        $normalized = self::normalizeDescription($description);
        if ($this->description === $normalized) {
            return;
        }

        $previousDescription = $this->description;
        $this->applyProjectDescriptionChanged(new ProjectDescriptionChanged($this->id, $previousDescription, $normalized));
        $this->recordThat(new ProjectDescriptionChanged($this->id, $previousDescription, $this->description));
    }

    public function applyProjectCreated(ProjectCreated $event): void
    {
        $this->id = $event->aggregateId;
        $this->name = $event->name;
        $this->description = self::normalizeDescription($event->description);
    }

    public function applyProjectRenamed(ProjectRenamed $event): void
    {
        $this->name = $event->newName;
    }

    public function applyProjectDeleted(ProjectDeleted $event): void
    {
        $this->deleted = true;
    }

    public function applyProjectDescriptionChanged(ProjectDescriptionChanged $event): void
    {
        $this->description = $event->newDescription;
    }

    private static function normalizeDescription(?string $description): ?string
    {
        $description = trim((string) $description);

        return $description === '' ? null : $description;
    }
}
"""),
            ("app/Domain/Repositories/EventStoreInterface.php", """
<?php

namespace App\Domain\Repositories;

use App\Domain\Events\StoredEvent;

interface EventStoreInterface
{
    public function append(string $aggregateId, array $events): void;

    public function loadForAggregate(string $aggregateId): array;
}
"""),
            ("app/Application/DTOs/CreateProjectCommand.php", """
<?php

namespace App\Application\DTOs;

final class CreateProjectCommand
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Application/DTOs/UpdateProjectCommand.php", """
<?php

namespace App\Application\DTOs;

final class UpdateProjectCommand
{
    public function __construct(
        public readonly string $id,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Application/UseCases/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\CreateProjectCommand;
use App\Domain\Entities\Project;
use App\Domain\Repositories\EventStoreInterface;
use App\Domain\ValueObjects\ProjectName;
use App\Infrastructure\Projectors\ProjectProjector;

final class CreateProjectUseCase
{
    public function __construct(
        private readonly EventStoreInterface $eventStore,
        private readonly ProjectProjector $projector
    ) {}

    public function execute(CreateProjectCommand $command): Project
    {
        $project = Project::create(ProjectName::from($command->name), $command->description);
        $events = $project->releaseEvents();

        $this->eventStore->append($project->id, $events);
        $this->projector->project($events);

        return $project;
    }
}
"""),
            ("app/Application/UseCases/UpdateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\UpdateProjectCommand;
use App\Domain\Entities\Project;
use App\Domain\Repositories\EventStoreInterface;
use App\Domain\ValueObjects\ProjectName;
use App\Infrastructure\Projectors\ProjectProjector;

final class UpdateProjectUseCase
{
    public function __construct(
        private readonly EventStoreInterface $eventStore,
        private readonly ProjectProjector $projector
    ) {}

    public function execute(UpdateProjectCommand $command): ?Project
    {
        $events = $this->eventStore->loadForAggregate($command->id);
        if ($events === []) {
            return null;
        }

        $project = Project::reconstituteFromEvents($events);
        $project->rename(ProjectName::from($command->name));
        $project->changeDescription($command->description);
        $newEvents = $project->releaseEvents();

        $this->eventStore->append($project->id, $newEvents);
        $this->projector->project($newEvents);

        return $project;
    }
}
"""),
            ("app/Application/UseCases/ListProjectsUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Models\ProjectProjection;

final class ListProjectsUseCase
{
    public function execute(): array
    {
        return ProjectProjection::query()->orderBy('updated_at', 'desc')->get()->all();
    }
}
"""),
            ("app/Application/UseCases/ShowProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Models\ProjectProjection;

final class ShowProjectUseCase
{
    public function execute(string $id): ?ProjectProjection
    {
        return ProjectProjection::query()->where('aggregate_id', $id)->first();
    }
}
"""),
            ("app/Application/UseCases/DeleteProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Domain\Events\ProjectDeleted;
use App\Domain\Repositories\EventStoreInterface;
use App\Infrastructure\Projectors\ProjectProjector;

final class DeleteProjectUseCase
{
    public function __construct(
        private readonly EventStoreInterface $eventStore,
        private readonly ProjectProjector $projector
    ) {}

    public function execute(string $id): void
    {
        $event = new ProjectDeleted($id);
        $this->eventStore->append($id, [$event]);
        $this->projector->project([$event]);
    }
}
"""),
            ("app/Infrastructure/EventStore/EloquentEventStore.php", """
<?php

namespace App\Infrastructure\EventStore;

use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectDeleted;
use App\Domain\Events\ProjectDescriptionChanged;
use App\Domain\Events\ProjectRenamed;
use App\Domain\Events\StoredEvent;
use App\Domain\Repositories\EventStoreInterface;
use App\Models\StoredEvent as StoredEventModel;
use DateTimeImmutable;
use RuntimeException;

final class EloquentEventStore implements EventStoreInterface
{
    public function append(string $aggregateId, array $events): void
    {
        foreach ($events as $event) {
            StoredEventModel::query()->create([
                'aggregate_id' => $aggregateId,
                'event_type' => $event::class,
                'payload' => get_object_vars($event),
                'created_at' => new DateTimeImmutable(),
            ]);
        }
    }

    public function loadForAggregate(string $aggregateId): array
    {
        return StoredEventModel::query()
            ->where('aggregate_id', $aggregateId)
            ->orderBy('event_id')
            ->get()
            ->map(static function (StoredEventModel $record): object {
                $payload = is_array($record->payload) ? $record->payload : [];

                return match ($record->event_type) {
                    ProjectCreated::class => new ProjectCreated(
                        aggregateId: $record->aggregate_id,
                        name: $payload['name'] ?? '',
                        description: $payload['description'] ?? null
                    ),
                    ProjectRenamed::class => new ProjectRenamed(
                        aggregateId: $record->aggregate_id,
                        previousName: $payload['previousName'] ?? '',
                        newName: $payload['newName'] ?? ''
                    ),
                    ProjectDescriptionChanged::class => new ProjectDescriptionChanged(
                        aggregateId: $record->aggregate_id,
                        previousDescription: $payload['previousDescription'] ?? null,
                        newDescription: $payload['newDescription'] ?? null
                    ),
                    ProjectDeleted::class => new ProjectDeleted($record->aggregate_id),
                    default => throw new RuntimeException("Unsupported event type: {$record->event_type}"),
                };
            })
            ->all();
    }
}
"""),
            ("app/Infrastructure/Projectors/ProjectProjector.php", """
<?php

namespace App\Infrastructure\Projectors;

use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectDeleted;
use App\Domain\Events\ProjectDescriptionChanged;
use App\Domain\Events\ProjectRenamed;
use App\Models\ProjectProjection;

final class ProjectProjector
{
    public function project(array $events): void
    {
        foreach ($events as $event) {
            if ($event instanceof ProjectCreated) {
                $this->applyProjectCreated($event);
            }

            if ($event instanceof ProjectRenamed) {
                $this->applyProjectRenamed($event);
            }

            if ($event instanceof ProjectDescriptionChanged) {
                $this->applyProjectDescriptionChanged($event);
            }

            if ($event instanceof ProjectDeleted) {
                $this->applyProjectDeleted($event);
            }
        }
    }

    public function applyProjectCreated(ProjectCreated $event): void
    {
        ProjectProjection::query()->updateOrCreate(
            ['aggregate_id' => $event->aggregateId],
            ['name' => $event->name, 'description' => $event->description]
        );
    }

    public function applyProjectRenamed(ProjectRenamed $event): void
    {
        ProjectProjection::query()
            ->where('aggregate_id', $event->aggregateId)
            ->update(['name' => $event->newName]);
    }

    public function applyProjectDescriptionChanged(ProjectDescriptionChanged $event): void
    {
        ProjectProjection::query()
            ->where('aggregate_id', $event->aggregateId)
            ->update(['description' => $event->newDescription]);
    }

    public function applyProjectDeleted(ProjectDeleted $event): void
    {
        ProjectProjection::query()
            ->where('aggregate_id', $event->aggregateId)
            ->delete();
    }
}
"""),
            ("app/Models/StoredEvent.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class StoredEvent extends Model
{
    protected $table = 'stored_events';
    protected $primaryKey = 'event_id';
    public $timestamps = false;

    protected $fillable = [
        'aggregate_id',
        'event_type',
        'payload',
        'created_at',
    ];

    protected $casts = [
        'payload' => 'array',
        'created_at' => 'datetime',
    ];
}
"""),
            ("app/Models/ProjectProjection.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class ProjectProjection extends Model
{
    protected $table = 'project_projections';

    protected $fillable = [
        'aggregate_id',
        'name',
        'description',
    ];
}
"""),
            ("app/Http/Controllers/ProjectController.php", """
<?php

namespace App\Http\Controllers;

use App\Application\DTOs\CreateProjectCommand;
use App\Application\DTOs\UpdateProjectCommand;
use App\Application\UseCases\CreateProjectUseCase;
use App\Application\UseCases\DeleteProjectUseCase;
use App\Application\UseCases\ListProjectsUseCase;
use App\Application\UseCases\ShowProjectUseCase;
use App\Application\UseCases\UpdateProjectUseCase;
use DomainException;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

final class ProjectController extends Controller
{
    public function __construct(
        private readonly CreateProjectUseCase $createProject,
        private readonly UpdateProjectUseCase $updateProject,
        private readonly ListProjectsUseCase $listProjects,
        private readonly ShowProjectUseCase $showProject,
        private readonly DeleteProjectUseCase $deleteProject
    ) {}

    public function index(): JsonResponse
    {
        return response()->json($this->listProjects->execute());
    }

    public function show(string $id): JsonResponse
    {
        $project = $this->showProject->execute($id);

        return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
    }

    public function store(Request $request): JsonResponse
    {
        try {
            $data = $request->validate([
                'name' => ['required', 'string'],
                'description' => ['nullable', 'string'],
            ]);

            $project = $this->createProject->execute(new CreateProjectCommand($data['name'], $data['description'] ?? null));

            return response()->json($project, 201);
        } catch (DomainException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }
    }

    public function update(string $id, Request $request): JsonResponse
    {
        try {
            $data = $request->validate([
                'name' => ['required', 'string'],
                'description' => ['nullable', 'string'],
            ]);

            $project = $this->updateProject->execute(new UpdateProjectCommand($id, $data['name'], $data['description'] ?? null));

            return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
        } catch (DomainException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }
    }

    public function destroy(string $id): JsonResponse
    {
        $this->deleteProject->execute($id);

        return response()->json(null, 204);
    }
}
"""),
            ("routes/web.php", """
<?php

use App\Http\Controllers\ProjectController;
use Illuminate\Support\Facades\Route;

Route::get('/projects', [ProjectController::class, 'index']);
Route::post('/projects', [ProjectController::class, 'store']);
Route::get('/projects/{id}', [ProjectController::class, 'show']);
Route::put('/projects/{id}', [ProjectController::class, 'update']);
Route::delete('/projects/{id}', [ProjectController::class, 'destroy']);
"""),
            (".github/workflows/ci.yml", """
name: CI
on:
  push:
    branches: [main]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      mongo:
        image: mongo:7
        ports:
          - 27017:27017
        options: >-
          --health-cmd "mongosh --eval 'db.runCommand({ ping: 1 })'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    steps:
      - uses: actions/checkout@v4
      - name: Setup PHP
        uses: shivammathur/setup-php@v2
        with:
          php-version: '8.3'
          extensions: mbstring, xml, curl, zip, intl, pdo_sqlite, mongodb
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress
      - name: Prepare env
        run: |
          cp .env.example .env
          php artisan key:generate
          sed -i "s/^DB_CONNECTION=.*/DB_CONNECTION=mongodb/" .env
          sed -i "s/^DB_HOST=.*/DB_HOST=127.0.0.1/" .env
          sed -i "s/^DB_PORT=.*/DB_PORT=27017/" .env
      - name: Run tests
        run: php artisan test
"""),
            (".github/workflows/cd.yml", """
name: CD
on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup PHP
        uses: shivammathur/setup-php@v2
        with:
          php-version: '8.3'
          extensions: mbstring, xml, curl, zip, intl, mongodb
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress --no-dev
      - name: Build artifact
        run: tar -czf project.tar.gz . --exclude=.git --exclude=vendor --exclude=node_modules
"""),
            ("database/migrations/2026_01_01_000000_create_stored_events_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('stored_events', function (Blueprint $table) {
            $table->bigIncrements('event_id');
            $table->uuid('aggregate_id')->index();
            $table->string('event_type');
            $table->json('payload');
            $table->timestamp('created_at')->useCurrent();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('stored_events');
    }
};
"""),
            ("database/migrations/2026_01_01_000001_create_project_projections_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('project_projections', function (Blueprint $table) {
            $table->uuid('aggregate_id')->primary();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('project_projections');
    }
};
"""),
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelHexagonalPatternFiles()
    {
        return new[]
        {
            ("app/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

use DomainException;

final class Project
{
    public function __construct(
        public readonly ?int $id = null,
        public string $name = '',
        public ?string $description = null
    ) {}

    public static function create(string $name, ?string $description = null): self
    {
        return new self(null, self::normalizeName($name), self::normalizeDescription($description));
    }

    public function rename(string $name): void
    {
        $name = self::normalizeName($name);
        if ($name === $this->name) {
            throw new DomainException('The new project name must be different.');
        }

        $this->name = $name;
    }

    public function changeDescription(?string $description): void
    {
        $this->description = self::normalizeDescription($description);
    }

    private static function normalizeName(string $name): string
    {
        $name = trim($name);
        if (mb_strlen($name) < 3) {
            throw new DomainException('Project name must contain at least 3 characters.');
        }

        return $name;
    }

    private static function normalizeDescription(?string $description): ?string
    {
        $description = trim((string) $description);

        return $description === '' ? null : $description;
    }
}
"""),
            ("app/Application/DTOs/CreateProjectCommand.php", """
<?php

namespace App\Application\DTOs;

final class CreateProjectCommand
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Application/DTOs/UpdateProjectCommand.php", """
<?php

namespace App\Application\DTOs;

final class UpdateProjectCommand
{
    public function __construct(
        public readonly int $id,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Application/Ports/In/CreateProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Application\DTOs\CreateProjectCommand;
use App\Domain\Entities\Project;

interface CreateProjectUseCaseInterface
{
    public function execute(CreateProjectCommand $command): Project;
}
"""),
            ("app/Application/Ports/In/UpdateProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Application\DTOs\UpdateProjectCommand;
use App\Domain\Entities\Project;

interface UpdateProjectUseCaseInterface
{
    public function execute(UpdateProjectCommand $command): ?Project;
}
"""),
            ("app/Application/Ports/In/ListProjectsUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

interface ListProjectsUseCaseInterface
{
    public function execute(): array;
}
"""),
            ("app/Application/Ports/In/ShowProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Domain\Entities\Project;

interface ShowProjectUseCaseInterface
{
    public function execute(int $id): ?Project;
}
"""),
            ("app/Application/Ports/In/DeleteProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

interface DeleteProjectUseCaseInterface
{
    public function execute(int $id): void;
}
"""),
            ("app/Application/Ports/Out/ProjectRepositoryPort.php", """
<?php

namespace App\Application\Ports\Out;

use App\Domain\Entities\Project;

interface ProjectRepositoryPort
{
    public function save(Project $project): Project;

    public function findById(int $id): ?Project;

    public function findAll(): array;

    public function delete(int $id): void;
}
"""),
            ("app/Application/UseCases/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\CreateProjectCommand;
use App\Application\Ports\In\CreateProjectUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Domain\Entities\Project;

final class CreateProjectUseCase implements CreateProjectUseCaseInterface
{
    public function __construct(private readonly ProjectRepositoryPort $projects) {}

    public function execute(CreateProjectCommand $command): Project
    {
        return $this->projects->save(Project::create($command->name, $command->description));
    }
}
"""),
            ("app/Application/UseCases/UpdateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\UpdateProjectCommand;
use App\Application\Ports\In\UpdateProjectUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Domain\Entities\Project;

final class UpdateProjectUseCase implements UpdateProjectUseCaseInterface
{
    public function __construct(private readonly ProjectRepositoryPort $projects) {}

    public function execute(UpdateProjectCommand $command): ?Project
    {
        $project = $this->projects->findById($command->id);
        if (!$project) {
            return null;
        }

        $project->rename($command->name);
        $project->changeDescription($command->description);

        return $this->projects->save($project);
    }
}
"""),
            ("app/Application/UseCases/ListProjectsUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\Ports\In\ListProjectsUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;

final class ListProjectsUseCase implements ListProjectsUseCaseInterface
{
    public function __construct(private readonly ProjectRepositoryPort $projects) {}

    public function execute(): array
    {
        return $this->projects->findAll();
    }
}
"""),
            ("app/Application/UseCases/ShowProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\Ports\In\ShowProjectUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Domain\Entities\Project;

final class ShowProjectUseCase implements ShowProjectUseCaseInterface
{
    public function __construct(private readonly ProjectRepositoryPort $projects) {}

    public function execute(int $id): ?Project
    {
        return $this->projects->findById($id);
    }
}
"""),
            ("app/Application/UseCases/DeleteProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\Ports\In\DeleteProjectUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;

final class DeleteProjectUseCase implements DeleteProjectUseCaseInterface
{
    public function __construct(private readonly ProjectRepositoryPort $projects) {}

    public function execute(int $id): void
    {
        $this->projects->delete($id);
    }
}
"""),
            ("app/Infrastructure/Persistence/EloquentProjectRepository.php", """
<?php

namespace App\Infrastructure\Persistence;

use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Domain\Entities\Project;
use App\Models\Project as ProjectModel;

final class EloquentProjectRepository implements ProjectRepositoryPort
{
    public function save(Project $project): Project
    {
        $model = $project->id ? ProjectModel::query()->find($project->id) : new ProjectModel();
        $model ??= new ProjectModel();
        $model->name = $project->name;
        $model->description = $project->description;
        $model->save();

        return new Project($model->id, $model->name, $model->description);
    }

    public function findById(int $id): ?Project
    {
        $model = ProjectModel::query()->find($id);

        return $model ? new Project($model->id, $model->name, $model->description) : null;
    }

    public function findAll(): array
    {
        return ProjectModel::query()
            ->orderBy('id', 'desc')
            ->get()
            ->map(static fn (ProjectModel $model) => new Project($model->id, $model->name, $model->description))
            ->all();
    }

    public function delete(int $id): void
    {
        ProjectModel::query()->whereKey($id)->delete();
    }
}
"""),
            ("app/Models/Project.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Project extends Model
{
    protected $table = 'projects';

    protected $fillable = [
        'name',
        'description',
    ];
}
"""),
            ("app/Http/Controllers/ProjectController.php", """
<?php

namespace App\Http\Controllers;

use App\Application\DTOs\CreateProjectCommand;
use App\Application\DTOs\UpdateProjectCommand;
use App\Application\Ports\In\CreateProjectUseCaseInterface;
use App\Application\Ports\In\DeleteProjectUseCaseInterface;
use App\Application\Ports\In\ListProjectsUseCaseInterface;
use App\Application\Ports\In\ShowProjectUseCaseInterface;
use App\Application\Ports\In\UpdateProjectUseCaseInterface;
use DomainException;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

final class ProjectController extends Controller
{
    public function __construct(
        private readonly CreateProjectUseCaseInterface $createProject,
        private readonly UpdateProjectUseCaseInterface $updateProject,
        private readonly ListProjectsUseCaseInterface $listProjects,
        private readonly ShowProjectUseCaseInterface $showProject,
        private readonly DeleteProjectUseCaseInterface $deleteProject
    ) {}

    public function index(): JsonResponse
    {
        return response()->json($this->listProjects->execute());
    }

    public function show(int $id): JsonResponse
    {
        $project = $this->showProject->execute($id);

        return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
    }

    public function store(Request $request): JsonResponse
    {
        try {
            $data = $request->validate([
                'name' => ['required', 'string'],
                'description' => ['nullable', 'string'],
            ]);

            $project = $this->createProject->execute(new CreateProjectCommand($data['name'], $data['description'] ?? null));

            return response()->json($project, 201);
        } catch (DomainException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }
    }

    public function update(int $id, Request $request): JsonResponse
    {
        try {
            $data = $request->validate([
                'name' => ['required', 'string'],
                'description' => ['nullable', 'string'],
            ]);

            $project = $this->updateProject->execute(new UpdateProjectCommand($id, $data['name'], $data['description'] ?? null));

            return $project ? response()->json($project) : response()->json(['message' => 'Project not found.'], 404);
        } catch (DomainException $exception) {
            return response()->json(['message' => $exception->getMessage()], 422);
        }
    }

    public function destroy(int $id): JsonResponse
    {
        $this->deleteProject->execute($id);

        return response()->json(null, 204);
    }
}
"""),
            ("app/Providers/HexagonalServiceProvider.php", """
<?php

namespace App\Providers;

use App\Application\Ports\In\CreateProjectUseCaseInterface;
use App\Application\Ports\In\DeleteProjectUseCaseInterface;
use App\Application\Ports\In\ListProjectsUseCaseInterface;
use App\Application\Ports\In\ShowProjectUseCaseInterface;
use App\Application\Ports\In\UpdateProjectUseCaseInterface;
use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Application\UseCases\CreateProjectUseCase;
use App\Application\UseCases\DeleteProjectUseCase;
use App\Application\UseCases\ListProjectsUseCase;
use App\Application\UseCases\ShowProjectUseCase;
use App\Application\UseCases\UpdateProjectUseCase;
use App\Infrastructure\Persistence\EloquentProjectRepository;
use Illuminate\Support\ServiceProvider;

final class HexagonalServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryPort::class, EloquentProjectRepository::class);
        $this->app->bind(CreateProjectUseCaseInterface::class, CreateProjectUseCase::class);
        $this->app->bind(UpdateProjectUseCaseInterface::class, UpdateProjectUseCase::class);
        $this->app->bind(ListProjectsUseCaseInterface::class, ListProjectsUseCase::class);
        $this->app->bind(ShowProjectUseCaseInterface::class, ShowProjectUseCase::class);
        $this->app->bind(DeleteProjectUseCaseInterface::class, DeleteProjectUseCase::class);
    }
}
"""),
            ("routes/web.php", """
<?php

use App\Http\Controllers\ProjectController;
use Illuminate\Support\Facades\Route;

Route::get('/projects', [ProjectController::class, 'index']);
Route::post('/projects', [ProjectController::class, 'store']);
Route::get('/projects/{id}', [ProjectController::class, 'show']);
Route::put('/projects/{id}', [ProjectController::class, 'update']);
Route::delete('/projects/{id}', [ProjectController::class, 'destroy']);
"""),
            ("database/migrations/2026_01_01_000000_create_projects_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('projects', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('projects');
    }
};
"""),
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelCqrsPatternFiles()
    {
        return new[]
        {
            ("app/Application/Commands/CreateItemCommand.php", """
<?php

namespace App\Application\Commands;

final class CreateItemCommand
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Application/Commands/CreateItemHandler.php", """
<?php

namespace App\Application\Commands;

use App\Models\Item;

final class CreateItemHandler
{
    public function handle(CreateItemCommand $command): Item
    {
        return Item::query()->create([
            'name' => $command->name,
            'description' => $command->description,
        ]);
    }
}
"""),
            ("app/Application/Queries/GetAllItemsQuery.php", """
<?php

namespace App\Application\Queries;

use App\Models\Item;
use Illuminate\Support\Collection;

final class GetAllItemsQuery
{
    public function handle(): Collection
    {
        return Item::query()->orderByDesc('created_at')->get();
    }
}
"""),
            ("app/Models/Item.php", """
<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Item extends Model
{
    protected $fillable = ['name', 'description'];
}
"""),
            ("database/migrations/2026_01_01_000001_create_items_table.php", """
<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('items', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->text('description')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('items');
    }
};
"""),
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelMediatorPatternFiles()
    {
        return new[]
        {
            ("app/Application/Mediator/Mediator.php", """
<?php

namespace App\Application\Mediator;

final class Mediator
{
    /** @var array<class-string, callable> */
    private array $handlers = [];

    public function register(string $requestClass, callable $handler): void
    {
        $this->handlers[$requestClass] = $handler;
    }

    public function send(object $request): mixed
    {
        $handler = $this->handlers[$request::class] ?? null;
        if ($handler === null) {
            throw new \InvalidArgumentException("No handler registered for " . $request::class);
        }

        return $handler($request);
    }
}
"""),
            ("app/Application/Mediator/Messages/CreateItemRequest.php", """
<?php

namespace App\Application\Mediator\Messages;

final class CreateItemRequest
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("app/Providers/MediatorServiceProvider.php", """
<?php

namespace App\Providers;

use App\Application\Mediator\Mediator;
use App\Application\Mediator\Messages\CreateItemRequest;
use App\Models\Item;
use Illuminate\Support\ServiceProvider;

final class MediatorServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->singleton(Mediator::class, function () {
            $mediator = new Mediator();
            $mediator->register(CreateItemRequest::class, function (CreateItemRequest $request) {
                return Item::query()->create([
                    'name' => $request->name,
                    'description' => $request->description,
                ]);
            });

            return $mediator;
        });
    }
}
"""),
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildLaravelSagaPatternFiles()
    {
        return new[]
        {
            ("app/Application/Sagas/OrderSaga.php", """
<?php

namespace App\Application\Sagas;

/**
 * Coordinates the order creation workflow across multiple steps, compensating
 * (rolling back) already-completed steps in reverse order if a later step fails.
 */
final class OrderSaga
{
    public function execute(string $customerId, array $items, array $paymentInfo): bool
    {
        $orderId = (string) \Illuminate\Support\Str::uuid();
        $reservationId = null;

        try {
            $this->createOrder($orderId, $customerId, $items);
            $reservationId = $this->reserveInventory($orderId, $items);
            $this->processPayment($orderId, $paymentInfo);
            $this->confirmOrder($orderId);

            return true;
        } catch (\Throwable $e) {
            if ($reservationId !== null) {
                $this->releaseInventory($reservationId);
            }
            $this->cancelOrder($orderId);

            return false;
        }
    }

    private function createOrder(string $orderId, string $customerId, array $items): void {}
    private function reserveInventory(string $orderId, array $items): string { return (string) \Illuminate\Support\Str::uuid(); }
    private function processPayment(string $orderId, array $paymentInfo): void {}
    private function confirmOrder(string $orderId): void {}
    private function cancelOrder(string $orderId): void {}
    private function releaseInventory(string $reservationId): void {}
}
"""),
        };
    }
}
