using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Seeders.Php;

public static partial class PhpSeeder
{
    private static IEnumerable<LibraryRecommendation> GetLibraries() =>
        new[]
        {
            new LibraryRecommendation
            {
                Id = 11,
                CreatedAt = SeedDate,
                Name = "Laravel Sanctum",
                PackageName = "laravel/sanctum",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Laravel,
                Category = "Authentication",
                Description = "Autenticacion ligera para APIs y SPAs en Laravel",
                PopularityScore = 96,
                InstallCommand = "composer require laravel/sanctum"
            },
            new LibraryRecommendation
            {
                Id = 12,
                CreatedAt = SeedDate,
                Name = "Spatie Permission",
                PackageName = "spatie/laravel-permission",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Laravel,
                Category = "Authorization",
                Description = "Roles y permisos robustos para Laravel",
                PopularityScore = 95,
                InstallCommand = "composer require spatie/laravel-permission"
            },
            new LibraryRecommendation
            {
                Id = 13,
                CreatedAt = SeedDate,
                Name = "Spatie Activitylog",
                PackageName = "spatie/laravel-activitylog",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Laravel,
                Category = "Auditing",
                Description = "Auditoria y trazabilidad de cambios en modelos",
                PopularityScore = 92,
                InstallCommand = "composer require spatie/laravel-activitylog"
            },
            new LibraryRecommendation
            {
                Id = 14,
                CreatedAt = SeedDate,
                Name = "Laravel Horizon",
                PackageName = "laravel/horizon",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Laravel,
                Category = "Queues",
                Description = "Panel y supervision de colas Redis en Laravel",
                PopularityScore = 91,
                InstallCommand = "composer require laravel/horizon"
            },
            new LibraryRecommendation
            {
                Id = 15,
                CreatedAt = SeedDate,
                Name = "Guzzle HTTP",
                PackageName = "guzzlehttp/guzzle",
                Architecture = ArchitectureType.Php,
                Category = "HTTP Client",
                Description = "Cliente HTTP estandar para integraciones y consumo de APIs",
                PopularityScore = 98,
                InstallCommand = "composer require guzzlehttp/guzzle"
            },
            new LibraryRecommendation
            {
                Id = 21,
                CreatedAt = SeedDate,
                Name = "Symfony ORM Pack",
                PackageName = "symfony/orm-pack",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Symfony,
                Category = "ORM",
                Description = "Stack de Doctrine ORM listo para Symfony",
                PopularityScore = 97,
                InstallCommand = "composer require symfony/orm-pack"
            },
            new LibraryRecommendation
            {
                Id = 22,
                CreatedAt = SeedDate,
                Name = "Symfony Validator",
                PackageName = "symfony/validator",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Symfony,
                Category = "Validation",
                Description = "Validacion de objetos y DTOs en Symfony",
                PopularityScore = 95,
                InstallCommand = "composer require symfony/validator"
            },
            new LibraryRecommendation
            {
                Id = 23,
                CreatedAt = SeedDate,
                Name = "Symfony Security Bundle",
                PackageName = "symfony/security-bundle",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Symfony,
                Category = "Security",
                Description = "Autenticacion y autorizacion para Symfony",
                PopularityScore = 96,
                InstallCommand = "composer require symfony/security-bundle"
            },
            new LibraryRecommendation
            {
                Id = 24,
                CreatedAt = SeedDate,
                Name = "Symfony Messenger",
                PackageName = "symfony/messenger",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Symfony,
                Category = "Queues",
                Description = "Mensajeria y colas para tareas asincronas en Symfony",
                PopularityScore = 93,
                InstallCommand = "composer require symfony/messenger"
            },
            new LibraryRecommendation
            {
                Id = 25,
                CreatedAt = SeedDate,
                Name = "Symfony Serializer",
                PackageName = "symfony/serializer-pack",
                Architecture = ArchitectureType.Php,
                Framework = FrameworkType.Symfony,
                Category = "Serialization",
                Description = "Serializacion y normalizacion de datos en Symfony",
                PopularityScore = 94,
                InstallCommand = "composer require symfony/serializer-pack"
            }
        };
}
