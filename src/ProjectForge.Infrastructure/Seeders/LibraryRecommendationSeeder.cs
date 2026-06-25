using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Seeders;

public static class LibraryRecommendationSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<LibraryRecommendation>().HasData(
            new LibraryRecommendation { Id = 1,  CreatedAt = SeedDate, Name = "MediatR",              PackageName = "MediatR",                       Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "CQRS/Mediator",  Description = "Implementación del patrón Mediator para CQRS",          PopularityScore = 95, InstallCommand = "dotnet add package MediatR" },
            new LibraryRecommendation { Id = 2,  CreatedAt = SeedDate, Name = "Entity Framework Core", PackageName = "Microsoft.EntityFrameworkCore",  Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "ORM",           Description = "ORM oficial de Microsoft para .NET",                   PopularityScore = 99, InstallCommand = "dotnet add package Microsoft.EntityFrameworkCore" },
            new LibraryRecommendation { Id = 3,  CreatedAt = SeedDate, Name = "FluentValidation",      PackageName = "FluentValidation.AspNetCore",    Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Validation",    Description = "Validación fluida y expresiva",                        PopularityScore = 92, InstallCommand = "dotnet add package FluentValidation.AspNetCore" },
            new LibraryRecommendation { Id = 4,  CreatedAt = SeedDate, Name = "Serilog",               PackageName = "Serilog.AspNetCore",             Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Logging",       Description = "Logging estructurado para .NET",                       PopularityScore = 97, InstallCommand = "dotnet add package Serilog.AspNetCore" },
            new LibraryRecommendation { Id = 5,  CreatedAt = SeedDate, Name = "AutoMapper",            PackageName = "AutoMapper",                    Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Mapping",       Description = "Mapeo automático entre objetos",                       PopularityScore = 94, InstallCommand = "dotnet add package AutoMapper" },
            new LibraryRecommendation { Id = 6,  CreatedAt = SeedDate, Name = "Swashbuckle (Swagger)", PackageName = "Swashbuckle.AspNetCore",         Architecture = Core.Enums.ArchitectureType.DotNet,   Framework = Core.Enums.FrameworkType.AspNetCoreWebApi, Category = "Documentation", Description = "Generación automática de documentación OpenAPI",        PopularityScore = 98, InstallCommand = "dotnet add package Swashbuckle.AspNetCore" },
            new LibraryRecommendation { Id = 7,  CreatedAt = SeedDate, Name = "xUnit",                 PackageName = "xunit",                         Architecture = Core.Enums.ArchitectureType.DotNet,   Category = "Testing",       Description = "Framework de testing unitario para .NET",              PopularityScore = 96, InstallCommand = "dotnet add package xunit" },
            new LibraryRecommendation { Id = 8,  CreatedAt = SeedDate, Name = "SQLAlchemy",            PackageName = "sqlalchemy",                    Architecture = Core.Enums.ArchitectureType.Python,   Category = "ORM",           Description = "ORM más popular para Python",                          PopularityScore = 98, InstallCommand = "pip install sqlalchemy" },
            new LibraryRecommendation { Id = 9,  CreatedAt = SeedDate, Name = "Pydantic",              PackageName = "pydantic",                      Architecture = Core.Enums.ArchitectureType.Python,   Category = "Validation",    Description = "Validación de datos con type hints",                   PopularityScore = 97, InstallCommand = "pip install pydantic" },
            new LibraryRecommendation { Id = 10, CreatedAt = SeedDate, Name = "Alembic",               PackageName = "alembic",                       Architecture = Core.Enums.ArchitectureType.Python,   Category = "Migrations",    Description = "Migraciones de base de datos para SQLAlchemy",         PopularityScore = 90, InstallCommand = "pip install alembic" }
        );
    }
}
