using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;

namespace ProjectForge.Infrastructure.Seeders.JavaScript;

public static class JavaScriptSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<DesignPatternEntry>().HasData(
            new DesignPatternEntry
            {
                Id = 6,
                CreatedAt = SeedDate,
                Pattern = Core.Enums.DesignPattern.Microservices,
                Name = "Microservices",
                Architecture = Core.Enums.ArchitectureType.JavaScript,
                Description = "Arquitectura de microservicios para aplicaciones Node.js."
            }
        );
    }
}
