using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Seeders;

public static class DesignPatternSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<DesignPatternEntry>().HasData(
            new DesignPatternEntry { Id = 1, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Abstrae el acceso a datos detrás de interfaces, facilitando testing y mantenimiento." },
            new DesignPatternEntry { Id = 2, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CQRS,              Name = "CQRS",               Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Separa las operaciones de lectura (Queries) de las de escritura (Commands)." },
            new DesignPatternEntry { Id = 3, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.CleanArchitecture, Name = "Clean Architecture", Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Arquitectura en capas concéntricas con dependencias hacia el centro." },
            new DesignPatternEntry { Id = 4, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Mediator,          Name = "Mediator",           Architecture = Core.Enums.ArchitectureType.DotNet,      Description = "Reduce el acoplamiento directo entre componentes usando un mediador." },
            new DesignPatternEntry { Id = 5, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Repository,        Name = "Repository Pattern", Architecture = Core.Enums.ArchitectureType.Python,      Description = "Patrón de repositorio adaptado para Python/FastAPI." },
            new DesignPatternEntry { Id = 6, CreatedAt = SeedDate, Pattern = Core.Enums.DesignPattern.Microservices,     Name = "Microservices",      Architecture = Core.Enums.ArchitectureType.JavaScript,   Description = "Arquitectura de microservicios para aplicaciones Node.js." }
        );
    }
}
