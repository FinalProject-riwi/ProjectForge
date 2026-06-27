using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Seeders.Python;

public static class PythonSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<LibraryRecommendation>().HasData(
            new LibraryRecommendation { Id = 8,  CreatedAt = SeedDate, Name = "SQLAlchemy", PackageName = "sqlalchemy", Architecture = Core.Enums.ArchitectureType.Python, Category = "ORM", Description = "ORM más popular para Python", PopularityScore = 98, InstallCommand = "pip install sqlalchemy" },
            new LibraryRecommendation { Id = 9,  CreatedAt = SeedDate, Name = "Pydantic",   PackageName = "pydantic",   Architecture = Core.Enums.ArchitectureType.Python, Category = "Validation", Description = "Validación de datos con type hints", PopularityScore = 97, InstallCommand = "pip install pydantic" },
            new LibraryRecommendation { Id = 10, CreatedAt = SeedDate, Name = "Alembic",    PackageName = "alembic",    Architecture = Core.Enums.ArchitectureType.Python, Category = "Migrations", Description = "Migraciones de base de datos para SQLAlchemy", PopularityScore = 90, InstallCommand = "pip install alembic" }
        );

        mb.Entity<DesignPatternEntry>().HasData(
            new DesignPatternEntry { Id = 5, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository, Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.Python, Description = "Patrón de repositorio adaptado para Python/FastAPI." }
        );
    }
}
