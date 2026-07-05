using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyCleanArchitecturePatternFiles()
        => BuildPhpDddPatternFiles(FrameworkType.Symfony);

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyRepositoryPatternFiles()
        => BuildPhpDddPatternFiles(FrameworkType.Symfony);

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyHexagonalPatternFiles()
    {
        return new[]
        {
            ("src/Application/Ports/In/CreateProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Application\DTOs\CreateProjectCommand;
use App\Domain\Entities\Project;

interface CreateProjectUseCaseInterface
{
    public function execute(CreateProjectCommand $command): Project;
}
"""),
            ("src/Application/Ports/In/UpdateProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Application\DTOs\UpdateProjectCommand;
use App\Domain\Entities\Project;

interface UpdateProjectUseCaseInterface
{
    public function execute(UpdateProjectCommand $command): ?Project;
}
"""),
            ("src/Application/Ports/In/ListProjectsUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

interface ListProjectsUseCaseInterface
{
    public function execute(): array;
}
"""),
            ("src/Application/Ports/In/ShowProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

use App\Domain\Entities\Project;

interface ShowProjectUseCaseInterface
{
    public function execute(int $id): ?Project;
}
"""),
            ("src/Application/Ports/In/DeleteProjectUseCaseInterface.php", """
<?php

namespace App\Application\Ports\In;

interface DeleteProjectUseCaseInterface
{
    public function execute(int $id): void;
}
"""),
            ("src/Application/Ports/Out/ProjectRepositoryPort.php", """
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
            ("src/Application/UseCases/CreateProjectUseCase.php", """
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
            ("src/Application/UseCases/UpdateProjectUseCase.php", """
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
            ("src/Application/UseCases/ListProjectsUseCase.php", """
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
            ("src/Application/UseCases/ShowProjectUseCase.php", """
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
            ("src/Application/UseCases/DeleteProjectUseCase.php", """
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
            ("src/Infrastructure/Persistence/DoctrineProjectRepository.php", """
<?php

namespace App\Infrastructure\Persistence;

use App\Application\Ports\Out\ProjectRepositoryPort;
use App\Domain\Entities\Project;
use App\Entity\Project as ProjectEntity;
use Doctrine\ORM\EntityManagerInterface;

final class DoctrineProjectRepository implements ProjectRepositoryPort
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    public function save(Project $project): Project
    {
        $entity = $project->id ? $this->entityManager->find(ProjectEntity::class, $project->id) : new ProjectEntity();
        $entity ??= new ProjectEntity();
        $entity->setName($project->name);
        $entity->setDescription($project->description);

        $this->entityManager->persist($entity);
        $this->entityManager->flush();

        return new Project($entity->getId(), $entity->getName(), $entity->getDescription());
    }

    public function findById(int $id): ?Project
    {
        $entity = $this->entityManager->find(ProjectEntity::class, $id);

        return $entity ? new Project($entity->getId(), $entity->getName(), $entity->getDescription()) : null;
    }

    public function findAll(): array
    {
        $entities = $this->entityManager->getRepository(ProjectEntity::class)->findBy([], ['id' => 'DESC']);

        return array_map(
            static fn (ProjectEntity $entity) => new Project($entity->getId(), $entity->getName(), $entity->getDescription()),
            $entities
        );
    }

    public function delete(int $id): void
    {
        $entity = $this->entityManager->find(ProjectEntity::class, $id);
        if ($entity !== null) {
            $this->entityManager->remove($entity);
            $this->entityManager->flush();
        }
    }
}
"""),
            ("src/Controller/ProjectController.php", """
<?php

namespace App\Controller;

use App\Application\DTOs\CreateProjectCommand;
use App\Application\DTOs\UpdateProjectCommand;
use App\Application\Ports\In\CreateProjectUseCaseInterface;
use App\Application\Ports\In\DeleteProjectUseCaseInterface;
use App\Application\Ports\In\ListProjectsUseCaseInterface;
use App\Application\Ports\In\ShowProjectUseCaseInterface;
use App\Application\Ports\In\UpdateProjectUseCaseInterface;
use DomainException;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;

final class ProjectController extends AbstractController
{
    public function __construct(
        private readonly CreateProjectUseCaseInterface $createProject,
        private readonly UpdateProjectUseCaseInterface $updateProject,
        private readonly ListProjectsUseCaseInterface $listProjects,
        private readonly ShowProjectUseCaseInterface $showProject,
        private readonly DeleteProjectUseCaseInterface $deleteProject
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

        return $project ? $this->json($project) : $this->json(['message' => 'Project not found.'], 404);
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

            return $project ? $this->json($project) : $this->json(['message' => 'Project not found.'], 404);
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

    App\Application\Ports\Out\ProjectRepositoryPort: '@App\Infrastructure\Persistence\DoctrineProjectRepository'
    App\Application\Ports\In\CreateProjectUseCaseInterface: '@App\Application\UseCases\CreateProjectUseCase'
    App\Application\Ports\In\UpdateProjectUseCaseInterface: '@App\Application\UseCases\UpdateProjectUseCase'
    App\Application\Ports\In\ListProjectsUseCaseInterface: '@App\Application\UseCases\ListProjectsUseCase'
    App\Application\Ports\In\ShowProjectUseCaseInterface: '@App\Application\UseCases\ShowProjectUseCase'
    App\Application\Ports\In\DeleteProjectUseCaseInterface: '@App\Application\UseCases\DeleteProjectUseCase'
"""),
            ("config/routes.yaml", """
controllers:
    resource: ../src/Controller/
    type: attribute
"""),
            ("migrations/Version20260626010000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260626010000 extends AbstractMigration
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

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyEventSourcingPatternFiles()
    {
        return new[]
        {
            ("src/Domain/Aggregates/AggregateRoot.php", """
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
            ("src/Domain/Events/ProjectCreated.php", """
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
            ("src/Domain/Events/ProjectRenamed.php", """
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
            ("src/Domain/Events/ProjectDeleted.php", """
<?php

namespace App\Domain\Events;

final class ProjectDeleted
{
    public function __construct(public readonly string $aggregateId) {}
}
"""),
            ("src/Domain/Events/ProjectDescriptionChanged.php", """
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
            ("src/Domain/Entities/Project.php", """
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
            ("src/Domain/Repositories/EventStoreInterface.php", """
<?php

namespace App\Domain\Repositories;

interface EventStoreInterface
{
    public function append(string $aggregateId, array $events): void;

    public function loadForAggregate(string $aggregateId): array;
}
"""),
            ("src/Application/UseCases/CreateProjectUseCase.php", """
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
            ("src/Application/UseCases/UpdateProjectUseCase.php", """
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
            ("src/Application/UseCases/ListProjectsUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Entity\ProjectProjection;
use Doctrine\ORM\EntityManagerInterface;

final class ListProjectsUseCase
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    public function execute(): array
    {
        return $this->entityManager->getRepository(ProjectProjection::class)->findBy([], ['aggregateId' => 'DESC']);
    }
}
"""),
            ("src/Application/UseCases/ShowProjectUseCase.php", """
<?php

namespace App\Application\UseCases;

use App\Entity\ProjectProjection;
use Doctrine\ORM\EntityManagerInterface;

final class ShowProjectUseCase
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    public function execute(string $id): ?ProjectProjection
    {
        return $this->entityManager->find(ProjectProjection::class, $id);
    }
}
"""),
            ("src/Application/UseCases/DeleteProjectUseCase.php", """
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
            ("src/Infrastructure/EventStore/DoctrineEventStore.php", """
<?php

namespace App\Infrastructure\EventStore;

use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectDeleted;
use App\Domain\Events\ProjectDescriptionChanged;
use App\Domain\Events\ProjectRenamed;
use App\Domain\Repositories\EventStoreInterface;
use App\Entity\StoredEvent;
use DateTimeImmutable;
use Doctrine\ORM\EntityManagerInterface;
use RuntimeException;

final class DoctrineEventStore implements EventStoreInterface
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    public function append(string $aggregateId, array $events): void
    {
        foreach ($events as $event) {
            $this->entityManager->persist(new StoredEvent($aggregateId, $event::class, get_object_vars($event), new DateTimeImmutable()));
        }

        $this->entityManager->flush();
    }

    public function loadForAggregate(string $aggregateId): array
    {
        $records = $this->entityManager->getRepository(StoredEvent::class)->findBy(['aggregateId' => $aggregateId], ['eventId' => 'ASC']);

        return array_map(static function (StoredEvent $record): object {
            $payload = $record->getPayload();

            return match ($record->getEventType()) {
                ProjectCreated::class => new ProjectCreated($record->getAggregateId(), $payload['name'] ?? '', $payload['description'] ?? null),
                ProjectRenamed::class => new ProjectRenamed($record->getAggregateId(), $payload['previousName'] ?? '', $payload['newName'] ?? ''),
                ProjectDescriptionChanged::class => new ProjectDescriptionChanged($record->getAggregateId(), $payload['previousDescription'] ?? null, $payload['newDescription'] ?? null),
                ProjectDeleted::class => new ProjectDeleted($record->getAggregateId()),
                default => throw new RuntimeException("Unsupported event type: {$record->getEventType()}"),
            };
        }, $records);
    }
}
"""),
            ("src/Infrastructure/Projectors/ProjectProjector.php", """
<?php

namespace App\Infrastructure\Projectors;

use App\Domain\Events\ProjectCreated;
use App\Domain\Events\ProjectDeleted;
use App\Domain\Events\ProjectDescriptionChanged;
use App\Domain\Events\ProjectRenamed;
use App\Entity\ProjectProjection;
use Doctrine\ORM\EntityManagerInterface;

final class ProjectProjector
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

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

        $this->entityManager->flush();
    }

    private function applyProjectCreated(ProjectCreated $event): void
    {
        $projection = $this->entityManager->find(ProjectProjection::class, $event->aggregateId) ?? new ProjectProjection($event->aggregateId);
        $projection->setName($event->name);
        $projection->setDescription($event->description);
        $this->entityManager->persist($projection);
    }

    private function applyProjectRenamed(ProjectRenamed $event): void
    {
        $projection = $this->entityManager->find(ProjectProjection::class, $event->aggregateId);
        if ($projection !== null) {
            $projection->setName($event->newName);
        }
    }

    private function applyProjectDescriptionChanged(ProjectDescriptionChanged $event): void
    {
        $projection = $this->entityManager->find(ProjectProjection::class, $event->aggregateId);
        if ($projection !== null) {
            $projection->setDescription($event->newDescription);
        }
    }

    private function applyProjectDeleted(ProjectDeleted $event): void
    {
        $projection = $this->entityManager->find(ProjectProjection::class, $event->aggregateId);
        if ($projection !== null) {
            $this->entityManager->remove($projection);
        }
    }
}
"""),
            ("src/Entity/StoredEvent.php", """
<?php

namespace App\Entity;

use DateTimeImmutable;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'stored_events')]
class StoredEvent
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column(name: 'event_id', type: 'integer')]
    private ?int $eventId = null;

    #[ORM\Column(name: 'aggregate_id', type: 'string', length: 36)]
    private string $aggregateId;

    #[ORM\Column(name: 'event_type', type: 'string', length: 255)]
    private string $eventType;

    #[ORM\Column(name: 'payload', type: 'json')]
    private array $payload = [];

    #[ORM\Column(name: 'created_at', type: 'datetime_immutable')]
    private DateTimeImmutable $createdAt;

    public function __construct(string $aggregateId, string $eventType, array $payload, DateTimeImmutable $createdAt)
    {
        $this->aggregateId = $aggregateId;
        $this->eventType = $eventType;
        $this->payload = $payload;
        $this->createdAt = $createdAt;
    }

    public function getEventId(): ?int { return $this->eventId; }
    public function getAggregateId(): string { return $this->aggregateId; }
    public function getEventType(): string { return $this->eventType; }
    public function getPayload(): array { return $this->payload; }
    public function getCreatedAt(): DateTimeImmutable { return $this->createdAt; }
}
"""),
            ("src/Entity/ProjectProjection.php", """
<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'project_projections')]
class ProjectProjection
{
    #[ORM\Id]
    #[ORM\Column(name: 'aggregate_id', type: 'string', length: 36)]
    public string $aggregateId;

    #[ORM\Column(name: 'name', type: 'string', length: 255)]
    public string $name = '';

    #[ORM\Column(name: 'description', type: 'text', nullable: true)]
    public ?string $description = null;

    public function __construct(string $aggregateId)
    {
        $this->aggregateId = $aggregateId;
    }

    public function getAggregateId(): string { return $this->aggregateId; }
    public function getName(): string { return $this->name; }
    public function setName(string $name): void { $this->name = $name; }
    public function getDescription(): ?string { return $this->description; }
    public function setDescription(?string $description): void { $this->description = $description; }
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
    public function show(string $id): JsonResponse
    {
        $project = $this->showProject->execute($id);

        return $project ? $this->json($project) : $this->json(['message' => 'Project not found.'], 404);
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
    public function update(string $id, Request $request): JsonResponse
    {
        try {
            $data = $request->toArray();
            $project = $this->updateProject->execute(new UpdateProjectCommand($id, $data['name'] ?? '', $data['description'] ?? null));

            return $project ? $this->json($project) : $this->json(['message' => 'Project not found.'], 404);
        } catch (DomainException|JsonException $exception) {
            return $this->json(['message' => $exception->getMessage()], 422);
        }
    }

    #[Route('/projects/{id}', name: 'project_delete', methods: ['DELETE'])]
    public function destroy(string $id): JsonResponse
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

    App\Domain\Repositories\EventStoreInterface: '@App\Infrastructure\EventStore\DoctrineEventStore'
"""),
            ("config/routes.yaml", """
controllers:
    resource: ../src/Controller/
    type: attribute
"""),
            ("migrations/Version20260626020000.php", """
<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

final class Version20260626020000 extends AbstractMigration
{
    public function getDescription(): string
    {
        return 'Create stored events and projections tables.';
    }

    public function up(Schema $schema): void
    {
        $storedEvents = $schema->createTable('stored_events');
        $storedEvents->addColumn('event_id', 'integer', ['autoincrement' => true]);
        $storedEvents->addColumn('aggregate_id', 'string', ['length' => 36]);
        $storedEvents->addColumn('event_type', 'string', ['length' => 255]);
        $storedEvents->addColumn('payload', 'json');
        $storedEvents->addColumn('created_at', 'datetime_immutable');
        $storedEvents->setPrimaryKey(['event_id']);
        $storedEvents->addIndex(['aggregate_id']);

        $projection = $schema->createTable('project_projections');
        $projection->addColumn('aggregate_id', 'string', ['length' => 36]);
        $projection->addColumn('name', 'string', ['length' => 255]);
        $projection->addColumn('description', 'text', ['notnull' => false]);
        $projection->setPrimaryKey(['aggregate_id']);
    }

    public function down(Schema $schema): void
    {
        $schema->dropTable('project_projections');
        $schema->dropTable('stored_events');
    }
}
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyMicroservicesPatternFiles(DatabaseType database)
    {
        var projectsDbEnv = GetSymfonyMicroservicesDbEnvironment(database, "projects");
        var notificationsDbEnv = GetSymfonyMicroservicesDbEnvironment(database, "notifications");

        return new[]
        {
            ("src/Controller/GatewayController.php", """
<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpClient\HttpClient;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;

final class GatewayController extends AbstractController
{
    #[Route('/health', name: 'gateway_health', methods: ['GET'])]
    public function health(): JsonResponse
    {
        return $this->json(['service' => 'gateway', 'status' => 'ok']);
    }

    #[Route('/projects/{path}', name: 'gateway_projects', methods: ['GET', 'POST', 'PUT', 'DELETE'], requirements: ['path' => '.*'], defaults: ['path' => ''])]
    public function projects(Request $request, string $path = ''): JsonResponse
    {
        return $this->forward($request, $_ENV['PROJECTS_SERVICE_URL'] ?? 'http://projects-service:8080', $path);
    }

    #[Route('/notifications/{path}', name: 'gateway_notifications', methods: ['GET', 'POST', 'PUT', 'DELETE'], requirements: ['path' => '.*'], defaults: ['path' => ''])]
    public function notifications(Request $request, string $path = ''): JsonResponse
    {
        return $this->forward($request, $_ENV['NOTIFICATIONS_SERVICE_URL'] ?? 'http://notifications-service:8080', $path);
    }

    private function forward(Request $request, string $baseUrl, string $path): JsonResponse
    {
        $client = HttpClient::create();
        $payload = $request->getContent() !== '' ? $request->toArray() : [];
        $response = $client->request($request->getMethod(), rtrim($baseUrl, '/') . '/' . ltrim($path, '/'), [
            'query' => $request->query->all(),
            'json' => $payload,
        ]);

        return $this->json($response->toArray(false), $response->getStatusCode());
    }
}
"""),
            ("config/routes.yaml", """
controllers:
    resource: ../src/Controller/
    type: attribute
"""),
            (".env.example", """
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:8080
PROJECTS_SERVICE_URL=http://projects-service:8080
NOTIFICATIONS_SERVICE_URL=http://notifications-service:8080
REDIS_URL=redis://redis:6379
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
"""),
            ("services/projects/.env.example", projectsDbEnv),
            ("services/notifications/.env.example", notificationsDbEnv),
            ("services/projects/src/Controller/ProjectController.php", """
<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;

final class ProjectController extends AbstractController
{
    #[Route('/health', name: 'projects_health', methods: ['GET'])]
    public function health(): JsonResponse
    {
        return $this->json(['service' => 'projects', 'status' => 'ok']);
    }

    #[Route('/projects', name: 'projects_index', methods: ['GET'])]
    public function index(): JsonResponse
    {
        return $this->json([
            ['id' => 1, 'name' => 'Project Alpha'],
            ['id' => 2, 'name' => 'Project Beta'],
        ]);
    }

    #[Route('/projects', name: 'projects_store', methods: ['POST'])]
    public function store(Request $request): JsonResponse
    {
        return $this->json(['message' => 'Project created by projects service.', 'data' => $request->toArray()], 201);
    }
}
"""),
            ("services/projects/config/routes.yaml", """
controllers:
    resource: ../../src/Controller/
    type: attribute
"""),
            ("services/notifications/src/Controller/NotificationController.php", """
<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

final class NotificationController extends AbstractController
{
    #[Route('/health', name: 'notifications_health', methods: ['GET'])]
    public function health(): JsonResponse
    {
        return $this->json(['service' => 'notifications', 'status' => 'ok']);
    }

    #[Route('/notifications', name: 'notifications_index', methods: ['GET'])]
    public function index(): JsonResponse
    {
        return $this->json([['id' => 1, 'message' => 'Notification ready']]);
    }
}
"""),
            ("services/notifications/config/routes.yaml", """
controllers:
    resource: ../../src/Controller/
    type: attribute
"""),
            ("services/projects/Dockerfile", BuildLaravelMicroservicesDockerfile(database)),
            ("services/notifications/Dockerfile", BuildLaravelMicroservicesDockerfile(database)),
            ("docker-compose.yml", BuildSymfonyMicroservicesCompose(database)),
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
          extensions: mbstring, xml, curl, zip, intl
          coverage: none
      - name: Install dependencies
        run: composer install --no-interaction --prefer-dist --no-progress
      - name: Validate routes
        run: php bin/console debug:router
""")
        };
    }

    internal static string GetSymfonyMicroservicesDbEnvironment(DatabaseType database, string serviceName)
    {
        var databaseName = $"{serviceName}_db";
        var port = serviceName == "projects" ? 8081 : 8082;

        return database switch
        {
            DatabaseType.MySQL => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="mysql://root:secret@{serviceName}-db:3306/{databaseName}?serverVersion=8.0&charset=utf8mb4"
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
""",
            DatabaseType.PostgreSQL => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="postgresql://postgres:secret@{serviceName}-db:5432/{databaseName}?serverVersion=16&charset=utf8"
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
""",
            DatabaseType.SqlServer => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/{databaseName}?encrypt=false&trustServerCertificate=true"
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
""",
            DatabaseType.MongoDB => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="mongodb://mongo:27017/{databaseName}"
MONGODB_URL="mongodb://mongo:27017/{databaseName}"
MONGODB_DB={databaseName}
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
""",
            DatabaseType.Redis => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="sqlite:///%kernel.project_dir%/var/{serviceName}.sqlite"
REDIS_URL=redis://redis:6379
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
""",
            _ => $"""
APP_ENV=dev
APP_DEBUG=1
APP_URL=http://localhost:{port}
DATABASE_URL="sqlite:///%kernel.project_dir%/var/{serviceName}.sqlite"
CACHE_DSN=redis://redis:6379
MESSENGER_TRANSPORT_DSN=redis://redis:6379/messages
"""
        };
    }

    internal static string BuildSymfonyMicroservicesCompose(DatabaseType database) =>
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
      - redis
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: mysql://root:secret@projects-db:3306/projects_db?serverVersion=8.0&charset=utf8mb4
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - projects-db
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: mysql://root:secret@notifications-db:3306/notifications_db?serverVersion=8.0&charset=utf8mb4
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - notifications-db
      - redis
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

  redis:
    image: redis:7-alpine

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
      - redis
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: postgresql://postgres:secret@projects-db:5432/projects_db?serverVersion=16&charset=utf8
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - projects-db
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: postgresql://postgres:secret@notifications-db:5432/notifications_db?serverVersion=16&charset=utf8
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - notifications-db
      - redis
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

  redis:
    image: redis:7-alpine

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
""",
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
      - redis
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/projects_db?encrypt=false&trustServerCertificate=true
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - sqlserver
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: sqlsrv://sa:YourStrong!Passw0rd@sqlserver:1433/notifications_db?encrypt=false&trustServerCertificate=true
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - sqlserver
      - redis
      - rabbitmq

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: YourStrong!Passw0rd

  redis:
    image: redis:7-alpine

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
      - mongo
      - redis
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: mongodb://mongo:27017/projects_db
      MONGODB_URL: mongodb://mongo:27017/projects_db
      MONGODB_DB: projects_db
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - mongo
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: mongodb://mongo:27017/notifications_db
      MONGODB_URL: mongodb://mongo:27017/notifications_db
      MONGODB_DB: notifications_db
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    depends_on:
      - mongo
      - redis
      - rabbitmq

  mongo:
    image: mongo:7

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
      - redis
      - rabbitmq

  projects-service:
    build: ./services/projects
    ports:
      - "8081:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: sqlite:////var/www/html/var/projects.sqlite
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    volumes:
      - projects-db:/var/www/html/var
    depends_on:
      - redis
      - rabbitmq

  notifications-service:
    build: ./services/notifications
    ports:
      - "8082:8080"
    environment:
      APP_ENV: dev
      DATABASE_URL: sqlite:////var/www/html/var/notifications.sqlite
      CACHE_DSN: redis://redis:6379
      MESSENGER_TRANSPORT_DSN: redis://redis:6379/messages
    volumes:
      - notifications-db:/var/www/html/var
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

volumes:
  projects-db:
  notifications-db:
"""
        };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyCqrsPatternFiles()
    {
        return new[]
        {
            ("src/Application/Command/CreateItemCommand.php", """
<?php

namespace App\Application\Command;

final class CreateItemCommand
{
    public function __construct(
        public readonly string $name,
        public readonly ?string $description = null
    ) {}
}
"""),
            ("src/Application/Command/CreateItemHandler.php", """
<?php

namespace App\Application\Command;

use App\Entity\Item;
use Doctrine\ORM\EntityManagerInterface;

final class CreateItemHandler
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    public function __invoke(CreateItemCommand $command): Item
    {
        $item = new Item($command->name, $command->description);
        $this->entityManager->persist($item);
        $this->entityManager->flush();

        return $item;
    }
}
"""),
            ("src/Application/Query/GetAllItemsQuery.php", """
<?php

namespace App\Application\Query;

final class GetAllItemsQuery
{
}
"""),
            ("src/Application/Query/GetAllItemsHandler.php", """
<?php

namespace App\Application\Query;

use App\Entity\Item;
use Doctrine\ORM\EntityManagerInterface;

final class GetAllItemsHandler
{
    public function __construct(private readonly EntityManagerInterface $entityManager) {}

    /** @return Item[] */
    public function __invoke(GetAllItemsQuery $query): array
    {
        return $this->entityManager->getRepository(Item::class)->findAll();
    }
}
"""),
            ("src/Entity/Item.php", """
<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'items')]
class Item
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column(type: 'integer')]
    private ?int $id = null;

    #[ORM\Column(type: 'string', length: 255)]
    private string $name;

    #[ORM\Column(type: 'text', nullable: true)]
    private ?string $description;

    public function __construct(string $name, ?string $description = null)
    {
        $this->name = $name;
        $this->description = $description;
    }

    public function getId(): ?int { return $this->id; }
    public function getName(): string { return $this->name; }
    public function getDescription(): ?string { return $this->description; }
}
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyMediatorPatternFiles()
    {
        return new[]
        {
            ("src/Application/Mediator/Mediator.php", """
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
            throw new \InvalidArgumentException('No handler registered for ' . $request::class);
        }

        return $handler($request);
    }
}
"""),
            ("src/Application/Mediator/Messages/CreateItemRequest.php", """
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
            ("src/Application/Mediator/MediatorFactory.php", """
<?php

namespace App\Application\Mediator;

use App\Application\Mediator\Messages\CreateItemRequest;
use App\Entity\Item;
use Doctrine\ORM\EntityManagerInterface;

final class MediatorFactory
{
    public static function create(EntityManagerInterface $entityManager): Mediator
    {
        $mediator = new Mediator();
        $mediator->register(CreateItemRequest::class, function (CreateItemRequest $request) use ($entityManager) {
            $item = new Item($request->name, $request->description);
            $entityManager->persist($item);
            $entityManager->flush();

            return $item;
        });

        return $mediator;
    }
}
"""),
            ("config/services.yaml", """
services:
    App\Application\Mediator\Mediator:
        factory: ['App\Application\Mediator\MediatorFactory', 'create']
        arguments: ['@doctrine.orm.entity_manager']
"""),
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonySagaPatternFiles()
    {
        return new[]
        {
            ("src/Application/Saga/OrderSaga.php", """
<?php

namespace App\Application\Saga;

/**
 * Coordinates the order creation workflow across multiple steps, compensating
 * (rolling back) already-completed steps in reverse order if a later step fails.
 */
final class OrderSaga
{
    public function execute(string $customerId, array $items, array $paymentInfo): bool
    {
        $orderId = $this->generateId();
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

    // No extra composer package required (symfony/uid isn't part of symfony/skeleton by default).
    private function generateId(): string
    {
        return bin2hex(random_bytes(16));
    }

    private function createOrder(string $orderId, string $customerId, array $items): void {}
    private function reserveInventory(string $orderId, array $items): string { return $this->generateId(); }
    private function processPayment(string $orderId, array $paymentInfo): void {}
    private function confirmOrder(string $orderId): void {}
    private function cancelOrder(string $orderId): void {}
    private function releaseInventory(string $reservationId): void {}
}
"""),
        };
    }
}
