using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpDddPatternFiles(FrameworkType framework)
    {
        if (framework == FrameworkType.Symfony)
        {
            return new[]
            {
                ("src/Domain/Aggregates/AggregateRoot.php", """
<?php

namespace App\Domain\Aggregates;

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
}
"""),
                ("src/Domain/ValueObjects/ProjectName.php", """
<?php

namespace App\Domain\ValueObjects;

use DomainException;
use JsonException;

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
                ("src/Domain/Events/ProjectCreated.php", """
<?php

namespace App\Domain\Events;

final class ProjectCreated
{
    public function __construct(
        public readonly ?int $projectId,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
                ("src/Domain/Events/ProjectRenamed.php", """
<?php

namespace App\Domain\Events;

final class ProjectRenamed
{
    public function __construct(
        public readonly ?int $projectId,
        public readonly string $previousName,
        public readonly string $newName
    ) {}
}
"""),
                ("src/Domain/Entities/Project.php", """
<?php

namespace App\Domain\Entities;

use App\Domain\Aggregates\AggregateRoot;
use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectRenamed;
use App\Domain\ValueObjects\ProjectName;
use DomainException;

final class Project extends AggregateRoot
{
    public function __construct(
        public readonly ?int $id = null,
        public string $name = '',
        public ?string $description = null
    ) {}

    public static function create(ProjectName $name, ?string $description = null): self
    {
        $project = new self(name: $name->value, description: self::normalizeDescription($description));
        $project->recordThat(new ProjectCreated($project->id, $project->name, $project->description));

        return $project;
    }

    public static function reconstitute(?int $id, string $name, ?string $description = null): self
    {
        return new self($id, $name, self::normalizeDescription($description));
    }

    public function rename(ProjectName $name): void
    {
        if ($this->name === $name->value) {
            throw new DomainException('The new project name must be different.');
        }

        $previousName = $this->name;
        $this->name = $name->value;
        $this->recordThat(new ProjectRenamed($this->id, $previousName, $this->name));
    }

    public function changeDescription(?string $description): void
    {
        $this->description = self::normalizeDescription($description);
    }

    private static function normalizeDescription(?string $description): ?string
    {
        $description = trim((string) $description);

        return $description === '' ? null : $description;
    }
}
"""),
                ("src/Domain/Repositories/ProjectRepositoryInterface.php", """
<?php

namespace App\Domain\Repositories;

use App\Domain\Entities\Project;

interface ProjectRepositoryInterface
{
    public function save(Project $project): Project;

    public function findById(int $id): ?Project;

    public function findAll(): array;

    public function delete(int $id): void;
}
"""),
                ("src/Domain/Services/ProjectDomainService.php", """
<?php

namespace App\Domain\Services;

use App\Domain\Entities\Project;
use App\Domain\ValueObjects\ProjectName;
use DomainException;

final class ProjectDomainService
{
    public function assertCanCreate(ProjectName $name): void
    {
        if (mb_strlen($name->value) < 3) {
            throw new DomainException('Project name must contain at least 3 characters.');
        }
    }

    public function assertCanRename(Project $project, ProjectName $newName): void
    {
        if ($project->name === $newName->value) {
            throw new DomainException('The new project name must be different.');
        }
    }
}
"""),
                ("src/Application/DTOs/CreateProjectCommand.php", """
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
                ("src/Application/DTOs/UpdateProjectCommand.php", """
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
                ("src/Application/UseCases/CreateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\CreateProjectCommand;
use App\Domain\Entities\Project;
use App\Domain\Repositories\ProjectRepositoryInterface;
use App\Domain\Services\ProjectDomainService;
use App\Domain\ValueObjects\ProjectName;

final class CreateProjectUseCase
{
    public function __construct(
        private readonly ProjectRepositoryInterface $projects,
        private readonly ProjectDomainService $domainService
    ) {}

    public function execute(CreateProjectCommand $command): Project
    {
        $name = ProjectName::from($command->name);
        $this->domainService->assertCanCreate($name);

        return $this->projects->save(Project::create($name, $command->description));
    }
}
"""),
                ("src/Application/UseCases/UpdateProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Application\DTOs\UpdateProjectCommand;
use App\Domain\Entities\Project;
use App\Domain\Repositories\ProjectRepositoryInterface;
use App\Domain\Services\ProjectDomainService;
use App\Domain\ValueObjects\ProjectName;

final class UpdateProjectUseCase
{
    public function __construct(
        private readonly ProjectRepositoryInterface $projects,
        private readonly ProjectDomainService $domainService
    ) {}

    public function execute(UpdateProjectCommand $command): ?Project
    {
        $project = $this->projects->findById($command->id);
        if (!$project) {
            return null;
        }

        $name = ProjectName::from($command->name);
        $this->domainService->assertCanRename($project, $name);
        $project->rename($name);
        $project->changeDescription($command->description);

        return $this->projects->save($project);
    }
}
"""),
                ("src/Application/UseCases/ListProjectsUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Domain\Repositories\ProjectRepositoryInterface;

final class ListProjectsUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(): array
    {
        return $this->projects->findAll();
    }
}
"""),
                ("src/Application/UseCases/ShowProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Domain\Entities\Project;
use App\Domain\Repositories\ProjectRepositoryInterface;

final class ShowProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(int $id): ?Project
    {
        return $this->projects->findById($id);
    }
}
"""),
                ("src/Application/UseCases/DeleteProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Domain\Repositories\ProjectRepositoryInterface;

final class DeleteProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(int $id): void
    {
        $this->projects->delete($id);
    }
}
"""),
                ("src/Infrastructure/Persistence/DoctrineProjectRepository.php", """
<?php

namespace App\Infrastructure\Persistence;

use App\Domain\Entities\Project;
use App\Domain\Repositories\ProjectRepositoryInterface;

final class DoctrineProjectRepository implements ProjectRepositoryInterface
{
    public function save(Project $project): Project
    {
        return $project;
    }

    public function findById(int $id): ?Project
    {
        return null;
    }

    public function findAll(): array
    {
        return [];
    }

    public function delete(int $id): void
    {
    }
}
"""),
                ("src/Controller/ProjectController.php", """
<?php

namespace App\Controller;

use App\Application\DTOs\CreateProjectCommand;
use App\Application\DTOs\UpdateProjectCommand;
use App\Application\UseCases\CreateProjectUseCase;
use App\Application\UseCases\DeleteProjectUseCase;
use App\Application\UseCases\ListProjectsUseCase;
use App\Application\UseCases\ShowProjectUseCase;
use App\Application\UseCases\UpdateProjectUseCase;
use DomainException;
use JsonException;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;

final class ProjectController extends AbstractController
{
    public function __construct(
        private readonly CreateProjectUseCase $createProject,
        private readonly UpdateProjectUseCase $updateProject,
        private readonly ListProjectsUseCase $listProjects,
        private readonly ShowProjectUseCase $showProject,
        private readonly DeleteProjectUseCase $deleteProject
    ) {}

    #[Route('/projects', name: 'project_index', methods: ['GET'])]
    public function index(): JsonResponse
    {
        return $this->json($this->listProjects->execute());
    }

    #[Route('/projects/{id}', name: 'project_show', methods: ['GET'])]
    public function show(int $id): JsonResponse
    {
        $project = $this->showProject->execute($id);
        if ($project === null) {
            return $this->json(['message' => 'Project not found.'], 404);
        }

        return $this->json($project);
    }

    #[Route('/projects', name: 'project_store', methods: ['POST'])]
    public function store(Request $request): JsonResponse
    {
        try {
            $data = $request->toArray();
            $project = $this->createProject->execute(new CreateProjectCommand($data['name'] ?? '', $data['description'] ?? null));

            return $this->json($project, 201);
        } catch (DomainException|JsonException $exception) {
            return $this->json(['message' => $exception->getMessage()], 422);
        }
    }

    #[Route('/projects/{id}', name: 'project_update', methods: ['PUT'])]
    public function update(int $id, Request $request): JsonResponse
    {
        try {
            $data = $request->toArray();
            $project = $this->updateProject->execute(new UpdateProjectCommand($id, $data['name'] ?? '', $data['description'] ?? null));

            if ($project === null) {
                return $this->json(['message' => 'Project not found.'], 404);
            }

            return $this->json($project);
        } catch (DomainException|JsonException $exception) {
            return $this->json(['message' => $exception->getMessage()], 422);
        }
    }

    #[Route('/projects/{id}', name: 'project_delete', methods: ['DELETE'])]
    public function destroy(int $id): JsonResponse
    {
        $this->deleteProject->execute($id);

        return $this->json(null, 204);
    }
}
"""),
                ("config/services.yaml", """
parameters:
    locale: 'en'

services:
    _defaults:
        autowire: true
        autoconfigure: true

    App\:
        resource: '../src/'

    App\Domain\Repositories\ProjectRepositoryInterface: '@App\Infrastructure\Persistence\DoctrineProjectRepository'
"""),
                ("config/routes.yaml", """
controllers:
    resource: ../src/Controller/
    type: attribute
"""),
                ("migrations/Version20260626000000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260626000000 extends AbstractMigration
{
    public function getDescription(): string
    {
        return 'Create projects table.';
    }

    public function up(Schema $schema): void
    {
        $table = $schema->createTable('projects');
        $table->addColumn('id', 'integer', ['autoincrement' => true]);
        $table->addColumn('name', 'string', ['length' => 255]);
        $table->addColumn('description', 'text', ['notnull' => false]);
        $table->addColumn('created_at', 'datetime_immutable', ['notnull' => false]);
        $table->addColumn('updated_at', 'datetime_immutable', ['notnull' => false]);
        $table->setPrimaryKey(['id']);
    }

    public function down(Schema $schema): void
    {
        $schema->dropTable('projects');
    }
}
"""),
            };
        }

        var root = framework == FrameworkType.Symfony ? "src" : "app";
        var ns = "App";

        return new[]
        {
            ($"{root}/Domain/Aggregates/AggregateRoot.php", $$"""
<?php

namespace {{ns}}\Domain\Aggregates;

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
}
"""),
            ($"{root}/Domain/ValueObjects/ProjectName.php", $$"""
<?php

namespace {{ns}}\Domain\ValueObjects;

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
            ($"{root}/Domain/Events/ProjectCreated.php", $$"""
<?php

namespace {{ns}}\Domain\Events;

final class ProjectCreated
{
    public function __construct(
        public readonly ?int $projectId,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ($"{root}/Domain/Events/ProjectRenamed.php", $$"""
<?php

namespace {{ns}}\Domain\Events;

final class ProjectRenamed
{
    public function __construct(
        public readonly ?int $projectId,
        public readonly string $previousName,
        public readonly string $newName
    ) {}
}
"""),
            ($"{root}/Domain/Entities/Project.php", $$"""
<?php

namespace {{ns}}\Domain\Entities;

use {{ns}}\Domain\Aggregates\AggregateRoot;
use {{ns}}\Domain\Events\ProjectCreated;
use {{ns}}\Domain\Events\ProjectRenamed;
use {{ns}}\Domain\ValueObjects\ProjectName;
use DomainException;

final class Project extends AggregateRoot
{
    public function __construct(
        public readonly ?int $id = null,
        public string $name = '',
        public ?string $description = null
    ) {}

    public static function create(ProjectName $name, ?string $description = null): self
    {
        $project = new self(name: $name->value, description: self::normalizeDescription($description));
        $project->recordThat(new ProjectCreated($project->id, $project->name, $project->description));
        return $project;
    }

    public static function reconstitute(?int $id, string $name, ?string $description = null): self
    {
        return new self($id, $name, self::normalizeDescription($description));
    }

    public function rename(ProjectName $name): void
    {
        if ($this->name === $name->value) {
            throw new DomainException('The new project name must be different.');
        }

        $previousName = $this->name;
        $this->name = $name->value;
        $this->recordThat(new ProjectRenamed($this->id, $previousName, $this->name));
    }

    public function changeDescription(?string $description): void
    {
        $this->description = self::normalizeDescription($description);
    }

    private static function normalizeDescription(?string $description): ?string
    {
        $description = trim((string) $description);
        return $description === '' ? null : $description;
    }
}
"""),
            ($"{root}/Domain/Repositories/ProjectRepositoryInterface.php", $$"""
<?php

namespace {{ns}}\Domain\Repositories;

use {{ns}}\Domain\Entities\Project;

interface ProjectRepositoryInterface
{
    public function save(Project $project): Project;
    public function findById(int $id): ?Project;
    public function findAll(): array;
    public function delete(int $id): void;
}
"""),
            ($"{root}/Domain/Services/ProjectDomainService.php", $$"""
<?php

namespace {{ns}}\Domain\Services;

use {{ns}}\Domain\Entities\Project;
use {{ns}}\Domain\ValueObjects\ProjectName;
use DomainException;

final class ProjectDomainService
{
    public function assertCanCreate(ProjectName $name): void
    {
        if (mb_strlen($name->value) < 3) {
            throw new DomainException('Project name must contain at least 3 characters.');
        }
    }

    public function assertCanRename(Project $project, ProjectName $newName): void
    {
        if ($project->name === $newName->value) {
            throw new DomainException('The new project name must be different.');
        }
    }
}
"""),
            ($"{root}/Application/DTOs/CreateProjectCommand.php", $$"""
<?php

namespace {{ns}}\Application\DTOs;

final class CreateProjectCommand
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ($"{root}/Application/DTOs/UpdateProjectCommand.php", $$"""
<?php

namespace {{ns}}\Application\DTOs;

final class UpdateProjectCommand
{
    public function __construct(
        public readonly int $id,
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ($"{root}/Application/UseCases/CreateProjectUseCase.php", $$"""
<?php

namespace {{ns}}\Application\UseCases;

use {{ns}}\Application\DTOs\CreateProjectCommand;
use {{ns}}\Domain\Entities\Project;
use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;
use {{ns}}\Domain\Services\ProjectDomainService;
use {{ns}}\Domain\ValueObjects\ProjectName;

final class CreateProjectUseCase
{
    public function __construct(
        private readonly ProjectRepositoryInterface $projects,
        private readonly ProjectDomainService $domainService
    ) {}

    public function execute(CreateProjectCommand $command): Project
    {
        $name = ProjectName::from($command->name);
        $this->domainService->assertCanCreate($name);
        return $this->projects->save(Project::create($name, $command->description));
    }
}
"""),
            ($"{root}/Application/UseCases/UpdateProjectUseCase.php", $$"""
<?php

namespace {{ns}}\Application\UseCases;

use {{ns}}\Application\DTOs\UpdateProjectCommand;
use {{ns}}\Domain\Entities\Project;
use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;
use {{ns}}\Domain\Services\ProjectDomainService;
use {{ns}}\Domain\ValueObjects\ProjectName;

final class UpdateProjectUseCase
{
    public function __construct(
        private readonly ProjectRepositoryInterface $projects,
        private readonly ProjectDomainService $domainService
    ) {}

    public function execute(UpdateProjectCommand $command): ?Project
    {
        $project = $this->projects->findById($command->id);
        if (!$project) {
            return null;
        }

        $name = ProjectName::from($command->name);
        $this->domainService->assertCanRename($project, $name);
        $project->rename($name);
        $project->changeDescription($command->description);
        return $this->projects->save($project);
    }
}
"""),
            ($"{root}/Application/UseCases/ListProjectsUseCase.php", $$"""
<?php

namespace {{ns}}\Application\UseCases;

use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;

final class ListProjectsUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(): array
    {
        return $this->projects->findAll();
    }
}
"""),
            ($"{root}/Application/UseCases/ShowProjectUseCase.php", $$"""
<?php

namespace {{ns}}\Application\UseCases;

use {{ns}}\Domain\Entities\Project;
use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;

final class ShowProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(int $id): ?Project
    {
        return $this->projects->findById($id);
    }
}
"""),
            ($"{root}/Application/UseCases/DeleteProjectUseCase.php", $$"""
<?php

namespace {{ns}}\Application\UseCases;

use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;

final class DeleteProjectUseCase
{
    public function __construct(private readonly ProjectRepositoryInterface $projects) {}

    public function execute(int $id): void
    {
        $this->projects->delete($id);
    }
}
"""),
            ($"{root}/Infrastructure/Persistence/{(framework == FrameworkType.Symfony ? "Doctrine" : "Eloquent")}ProjectRepository.php", $$"""
<?php

namespace {{ns}}\Infrastructure\Persistence;

use {{ns}}\Domain\Entities\Project;
use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;

final class {{(framework == FrameworkType.Symfony ? "Doctrine" : "Eloquent")}}ProjectRepository implements ProjectRepositoryInterface
{
    public function save(Project $project): Project { return $project; }
    public function findById(int $id): ?Project { return null; }
    public function findAll(): array { return []; }
    public function delete(int $id): void {}
}
"""),
            ($"{root}/Http/Controllers/ProjectController.php", $$"""
<?php

namespace {{ns}}\Http\Controllers;

use {{ns}}\Application\DTOs\CreateProjectCommand;
use {{ns}}\Application\DTOs\UpdateProjectCommand;
use {{ns}}\Application\UseCases\CreateProjectUseCase;
use {{ns}}\Application\UseCases\DeleteProjectUseCase;
use {{ns}}\Application\UseCases\ListProjectsUseCase;
use {{ns}}\Application\UseCases\ShowProjectUseCase;
use {{ns}}\Application\UseCases\UpdateProjectUseCase;
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

    public function index(): JsonResponse { return response()->json($this->listProjects->execute()); }
    public function show(int $id): JsonResponse { return response()->json($this->showProject->execute($id)); }
    public function store(Request $request): JsonResponse
    {
        $data = $request->validate(['name' => ['required', 'string'], 'description' => ['nullable', 'string']]);
        $project = $this->createProject->execute(new CreateProjectCommand($data['name'], $data['description'] ?? null));
        return response()->json($project, 201);
    }
    public function update(int $id, Request $request): JsonResponse
    {
        $data = $request->validate(['name' => ['required', 'string'], 'description' => ['nullable', 'string']]);
        return response()->json($this->updateProject->execute(new UpdateProjectCommand($id, $data['name'], $data['description'] ?? null)));
    }
    public function destroy(int $id): JsonResponse { $this->deleteProject->execute($id); return response()->json(null, 204); }
}
"""),
            ($"{root}/Providers/DomainDrivenDesignServiceProvider.php", $$"""
<?php

namespace {{ns}}\Providers;

use {{ns}}\Domain\Repositories\ProjectRepositoryInterface;
use {{ns}}\Infrastructure\Persistence\{{(framework == FrameworkType.Symfony ? "Doctrine" : "Eloquent")}}ProjectRepository;
use Illuminate\Support\ServiceProvider;

final class DomainDrivenDesignServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        $this->app->bind(ProjectRepositoryInterface::class, {{(framework == FrameworkType.Symfony ? "Doctrine" : "Eloquent")}}ProjectRepository::class);
    }
}
"""),
            ("routes/web.php", $$"""
<?php

use {{ns}}\Http\Controllers\ProjectController;
use Illuminate\Support\Facades\Route;

Route::get('/projects', [ProjectController::class, 'index']);
Route::post('/projects', [ProjectController::class, 'store']);
Route::get('/projects/{id}', [ProjectController::class, 'show']);
Route::put('/projects/{id}', [ProjectController::class, 'update']);
Route::delete('/projects/{id}', [ProjectController::class, 'destroy']);
"""),
            ($"database/migrations/2026_01_01_000000_create_projects_table.php", """
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

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpCleanArchitecturePatternFiles(FrameworkType framework)
        => framework == FrameworkType.Symfony
            ? BuildSymfonyCleanArchitecturePatternFiles()
            : BuildPhpDddPatternFiles(framework);

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpHexagonalPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelHexagonalPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyHexagonalPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpRepositoryPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelRepositoryPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyRepositoryPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    // These three used to just alias to BuildPhpDddPatternFiles(framework) — selecting "CQRS",
    // "Mediator" or "Saga" for PHP silently produced the DDD Aggregate/Entity/UseCase scaffold
    // instead, with no actual command/query split, mediator class, or saga/compensation code
    // (every other language here — DotNet/Java/Python/JS/TS — has a real implementation).
    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpCqrsPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelCqrsPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyCqrsPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpEventSourcingPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelEventSourcingPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyEventSourcingPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpMediatorPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelMediatorPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyMediatorPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpSagaPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelSagaPatternFiles(),
            FrameworkType.Symfony => BuildSymfonySagaPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPhpMicroservicesPatternFiles(
        FrameworkType framework,
        DatabaseType database)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelMicroservicesPatternFiles(database),
            FrameworkType.Symfony => BuildSymfonyMicroservicesPatternFiles(database),
            _ => BuildPhpDddPatternFiles(framework)
        };
}
