using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Paso 2: Scaffold por arquitectura ────────────────────────────────────

    private async Task ScaffoldProjectAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Scaffold", $"🏗️  Iniciando scaffold ({cfg.Architecture} / {cfg.Framework})...", ct: ct);

        foreach (var cmd in GetScaffoldCommands(cfg, project.Name, path))
        {
            await EmitLogAsync(project, "Scaffold", $"$ {cmd.Command}", ct: ct);
            var result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);

            if (!string.IsNullOrWhiteSpace(result.Stdout))
                await EmitLogAsync(project, "Scaffold", result.Stdout.Trim(), ct: ct);

            if (!result.Success)
            {
                await EmitLogAsync(project, "Scaffold", result.Stderr, isError: true, ct: ct);
                throw new InvalidOperationException($"Scaffold falló: {result.Stderr}");
            }
        }
    }

    private static IEnumerable<(string Command, string? WorkingDir)> GetScaffoldCommands(
        WizardConfig cfg, string projectName, string path)
    {
        var safeName = projectName.Replace(" ", "");

        return cfg.Architecture switch
        {
            ArchitectureType.DotNet => cfg.Framework switch
            {
                FrameworkType.AspNetCoreWebApi => new[]
                {
                    ($"dotnet new webapi -n {safeName} -o {path} --no-https false", (string?)null),
                    ($"dotnet new sln -n {safeName}", path),
                    ($"dotnet sln add {safeName}.csproj", path),
                },
                FrameworkType.AspNetCoreMVC => new[] { ($"dotnet new mvc -n {safeName} -o {path}", (string?)null) },
                FrameworkType.BlazorServer => new[] { ($"dotnet new blazorserver -n {safeName} -o {path}", (string?)null) },
                _ => new[] { ($"dotnet new webapi -n {safeName} -o {path}", (string?)null) }
            },

            ArchitectureType.Python => cfg.Framework switch
            {
                FrameworkType.FastAPI => new[]
                {
                    ($"mkdir -p {path}/app/api/v1 {path}/app/models {path}/app/services {path}/tests", (string?)null),
                    ($"python3 -m venv {path}/venv", null),
                },
                FrameworkType.Django => new[] { ($"django-admin startproject {safeName} {path}", (string?)null) },
                _ => new[] { ($"mkdir -p {path}/src {path}/tests", (string?)null) }
            },

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => cfg.Framework switch
            {
                FrameworkType.NodeJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm pkg set scripts.start=node src/index.js", (string?)path),
                },
                FrameworkType.ExpressJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm install express", (string?)path),
                    ($"npm pkg set scripts.start=node src/server.js", (string?)path),
                },
                FrameworkType.NestJs => new[] { ($"npx @nestjs/cli new {safeName} --language JS --package-manager npm --skip-git --directory .", (string?)path) },
                FrameworkType.NextJs => new[] { ($"npm create next-app@latest . -- --js --app --eslint --src-dir --import-alias \"@/*\"", (string?)path) },
                FrameworkType.NestTs => new[] { ($"npx @nestjs/cli new {safeName} --language TS --package-manager npm --skip-git --directory .", (string?)path) },
                FrameworkType.NextTs => new[] { ($"npm create next-app@latest . -- --ts --app --eslint --src-dir --import-alias \"@/*\"", (string?)path) },
                _ => new[] { ($"npm init -y", (string?)path) }
            },

            ArchitectureType.Java => new[]
            {
                ($"curl -s https://start.spring.io/starter.zip -d type=maven-project -d language=java -d bootVersion={cfg.FrameworkVersion} -d artifactId={safeName.ToLower()} -d packaging=jar -d dependencies=web,actuator -o {path}/project.zip && cd {path} && unzip -q project.zip && rm project.zip", (string?)null),
            },

            ArchitectureType.Php => cfg.Framework switch
            {
                FrameworkType.Symfony => new[] { ($"composer create-project symfony/skeleton {path}", (string?)null) },
                _ => new[] { ($"composer create-project laravel/laravel {path}", (string?)null) }
            },

            _ => Array.Empty<(string, string?)>()
        };
    }

    // ─── Paso 2b: Base files de JS/TS ─────────────────────────────────────────

    private async Task ScaffoldJavaScriptBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture is not (ArchitectureType.JavaScript or ArchitectureType.TypeScript))
            return;

        var src = Path.Combine(path, "src");
        Directory.CreateDirectory(src);

        switch (cfg.Framework)
        {
            case FrameworkType.NodeJs:
                await File.WriteAllTextAsync(Path.Combine(src, "index.js"), "console.log('ProjectForge');\n", ct);
                break;
            case FrameworkType.ExpressJs:
                await File.WriteAllTextAsync(Path.Combine(src, "server.js"), """
const express = require('express');
const app = express();
app.get('/', (_, res) => res.json({ ok: true }));
app.listen(process.env.PORT || 3000);
""", ct);
                break;
            case FrameworkType.NestJs:
            case FrameworkType.NestTs:
                await File.WriteAllTextAsync(Path.Combine(src, "main.ts"), """
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  await app.listen(process.env.PORT || 3000);
}
bootstrap();
""", ct);
                break;
            case FrameworkType.NextJs:
            case FrameworkType.NextTs:
                Directory.CreateDirectory(Path.Combine(src, "app"));
                await File.WriteAllTextAsync(Path.Combine(src, "app", "page.js"), "export default function Page(){ return null; }\n", ct);
                break;
        }
    }

    private async Task ScaffoldPhpBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Php || cfg.Framework != FrameworkType.Symfony)
            return;

        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Any(p => NormalizePatternToken(p) == "microservices"))
            return;

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando base Symfony...", ct: ct);

        foreach (var (relativePath, content) in BuildPhpDddPatternFiles(FrameworkType.Symfony))
        {
            var filePath = Path.Combine(path, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, content, ct);
        }

        await EmitLogAsync(project, "Scaffold", "✅ Base Symfony generada", ct: ct);
    }

    // ─── Paso 3: Patrones de diseño ──────────────────────────────────────────

    private async Task ScaffoldDesignPatternsAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Count == 0)
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  Sin patrones seleccionados", ct: ct);
            return;
        }

        foreach (var pattern in selectedPatterns)
        {
            var files = cfg.Architecture switch
            {
                ArchitectureType.Php => BuildPhpPatternFiles(cfg, pattern),
                ArchitectureType.JavaScript or ArchitectureType.TypeScript => BuildJavaScriptPatternFiles(cfg.Framework, pattern),
                _ => Array.Empty<(string RelativePath, string Content)>()
            };

            if (files.Count == 0)
            {
                await EmitLogAsync(project, "Patterns", $"ℹ️  Patrón no soportado: {pattern}", ct: ct);
                continue;
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                NormalizePatternToken(pattern) == "microservices")
            {
                if (cfg.Framework == FrameworkType.Laravel)
                {
                    await ScaffoldLaravelMicroservicesWorkspaceAsync(path, ct);
                }
                else if (cfg.Framework == FrameworkType.Symfony)
                {
                    await ScaffoldSymfonyMicroservicesWorkspaceAsync(path, ct);
                }
            }

            foreach (var (relativePath, content) in files)
            {
                var filePath = Path.Combine(path, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllTextAsync(filePath, content, ct);
            }

            await EmitLogAsync(project, "Patterns", $"✅ Patrón generado: {pattern}", ct: ct);

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "hexagonalarchitecture")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\HexagonalServiceProvider::class", ct);
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "repository")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\RepositoryServiceProvider::class", ct);
            }
        }
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpPatternFiles(
        WizardConfig cfg,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "domaindrivendesign" => BuildPhpDddPatternFiles(cfg.Framework),
            "cleanarchitecture" => BuildPhpCleanArchitecturePatternFiles(cfg.Framework),
            "hexagonalarchitecture" => BuildPhpHexagonalPatternFiles(cfg.Framework),
            "repository" => BuildPhpRepositoryPatternFiles(cfg.Framework),
            "cqrs" => BuildPhpCqrsPatternFiles(cfg.Framework),
            "eventsourcing" => BuildPhpEventSourcingPatternFiles(cfg.Framework),
            "mediator" => BuildPhpMediatorPatternFiles(cfg.Framework),
            "saga" => BuildPhpSagaPatternFiles(cfg.Framework),
            "microservices" => BuildPhpMicroservicesPatternFiles(cfg.Framework, cfg.Database),
            _ => Array.Empty<(string, string)>()
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptPatternFiles(
        FrameworkType framework,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "cqrs" => BuildJavaScriptCqrsPatternFiles(framework),
            "eventsourcing" => BuildJavaScriptEventSourcingPatternFiles(framework),
            "mediator" => BuildJavaScriptMediatorPatternFiles(framework),
            _ => Array.Empty<(string, string)>()
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpDddPatternFiles(FrameworkType framework)
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

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpCleanArchitecturePatternFiles(FrameworkType framework)
        => framework == FrameworkType.Symfony
            ? BuildSymfonyCleanArchitecturePatternFiles()
            : BuildPhpDddPatternFiles(framework);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpHexagonalPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelHexagonalPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyHexagonalPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpRepositoryPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelRepositoryPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyRepositoryPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

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

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpCqrsPatternFiles(FrameworkType framework)
        => BuildPhpDddPatternFiles(framework);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpEventSourcingPatternFiles(FrameworkType framework)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelEventSourcingPatternFiles(),
            FrameworkType.Symfony => BuildSymfonyEventSourcingPatternFiles(),
            _ => BuildPhpDddPatternFiles(framework)
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpMediatorPatternFiles(FrameworkType framework)
        => BuildPhpDddPatternFiles(framework);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpSagaPatternFiles(FrameworkType framework)
        => BuildPhpDddPatternFiles(framework);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpMicroservicesPatternFiles(
        FrameworkType framework,
        DatabaseType database)
        => framework switch
        {
            FrameworkType.Laravel => BuildLaravelMicroservicesPatternFiles(database),
            FrameworkType.Symfony => BuildSymfonyMicroservicesPatternFiles(database),
            _ => BuildPhpDddPatternFiles(framework)
        };

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyCleanArchitecturePatternFiles()
        => BuildPhpDddPatternFiles(FrameworkType.Symfony);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyRepositoryPatternFiles()
        => BuildPhpDddPatternFiles(FrameworkType.Symfony);

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyHexagonalPatternFiles()
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

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyEventSourcingPatternFiles()
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

    private static IReadOnlyList<(string RelativePath, string Content)> BuildSymfonyMicroservicesPatternFiles(DatabaseType database)
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

    private static string GetSymfonyMicroservicesDbEnvironment(DatabaseType database, string serviceName)
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

    private static string BuildSymfonyMicroservicesCompose(DatabaseType database) =>
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

    private static async Task EnsureLaravelProviderRegistrationAsync(string path, string providerEntry, CancellationToken ct)
    {
        var providersFile = Path.Combine(path, "bootstrap", "providers.php");
        if (!File.Exists(providersFile))
            return;

        var content = await File.ReadAllTextAsync(providersFile, ct);
        if (content.Contains(providerEntry, StringComparison.Ordinal))
            return;

        var insertMarker = "return [";
        var idx = content.IndexOf(insertMarker, StringComparison.Ordinal);
        if (idx < 0)
            return;

        var insertAt = content.IndexOf('\n', idx);
        if (insertAt < 0)
            return;

        var updated = content.Insert(insertAt + 1, $"    {providerEntry},\n");
        await File.WriteAllTextAsync(providersFile, updated, ct);
    }

    private static async Task ScaffoldLaravelMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios: {result.Stderr}");
        }
    }

    private static async Task ScaffoldSymfonyMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios de Symfony: {result.Stderr}");
        }
    }

    private static async Task<ShellResult> RunComposerCommandAsync(IEnumerable<string> arguments, string workingDirectory, CancellationToken ct)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "composer",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        return new ShellResult(process.ExitCode, stdout, stderr);
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptCqrsPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptEventSourcingPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptMediatorPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static string NormalizePatternToken(string value)
        => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
