using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders.Php;

public static partial class PhpSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder mb)
    {
        mb.Entity<DesignPatternEntry>().HasData(GetDesignPatterns().ToArray());
        mb.Entity<LibraryRecommendation>().HasData(GetLibraries().ToArray());
        mb.Entity<ProjectTemplate>().HasData(GetTemplates().ToArray());
    }

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var inserted = false;

        foreach (var pattern in GetDesignPatterns())
        {
            var exists = await db.DesignPatterns.AnyAsync(p =>
                p.Id == pattern.Id ||
                (p.Architecture == pattern.Architecture &&
                 p.Pattern == pattern.Pattern &&
                 p.Name == pattern.Name), ct);

            if (!exists)
            {
                await db.DesignPatterns.AddAsync(pattern, ct);
                inserted = true;
            }
        }

        foreach (var library in GetLibraries())
        {
            var exists = await db.Libraries.AnyAsync(l =>
                l.Id == library.Id ||
                (l.Architecture == library.Architecture &&
                 l.PackageName == library.PackageName), ct);

            if (!exists)
            {
                await db.Libraries.AddAsync(library, ct);
                inserted = true;
            }
        }

        foreach (var template in GetTemplates())
        {
            var exists = await db.Templates.AnyAsync(t =>
                t.Id == template.Id ||
                (t.Architecture == template.Architecture &&
                 t.TemplateType == template.TemplateType &&
                 t.Name == template.Name), ct);

            if (!exists)
            {
                await db.Templates.AddAsync(template, ct);
                inserted = true;
            }
        }

        if (inserted)
            await db.SaveChangesAsync(ct);
    }
}
