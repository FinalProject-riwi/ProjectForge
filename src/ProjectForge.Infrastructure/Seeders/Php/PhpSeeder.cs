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
                var notes = (pattern.ImplementationNotes??"").Replace("'","''");
                var cmds = (pattern.ScaffoldCommandsJson??"[]").Replace("'","''");
                var sqlPat = $"""
                    SET IDENTITY_INSERT [DesignPatterns] ON;
                    INSERT INTO [DesignPatterns] ([Id],[Architecture],[CreatedAt],[Description],[ImplementationNotes],[Name],[Pattern],[ScaffoldCommandsJson])
                    VALUES ({pattern.Id},N'{pattern.Architecture}','{pattern.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(pattern.Description??"").Replace("'","''")}',N'{notes}',N'{pattern.Name.Replace("'","''")}',N'{pattern.Pattern}',N'{cmds}');
                    SET IDENTITY_INSERT [DesignPatterns] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sqlPat, ct);
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
                var fwLib = library.Framework.HasValue ? $"N'{library.Framework}'" : "NULL";
                var sqlLib = $"""
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({library.Id},N'{library.Architecture}',N'{(library.Category??"").Replace("'","''")}','{library.CreatedAt:yyyy-MM-dd HH:mm:ss}',N'{(library.Description??"").Replace("'","''")}',{fwLib},N'{(library.InstallCommand??"").Replace("'","''")}',N'{library.Name.Replace("'","''")}',N'{library.PackageName.Replace("'","''")}',{library.PopularityScore});
                    SET IDENTITY_INSERT [Libraries] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sqlLib, ct);
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
                var dbVal = template.Database.HasValue ? $"N'{template.Database}'" : "NULL";
                var infra = template.Infrastructure.HasValue ? $"N'{template.Infrastructure}'" : "NULL";
                var fw = template.Framework.HasValue ? $"N'{template.Framework}'" : "NULL";
                var sqlT = $"""
                    SET IDENTITY_INSERT [Templates] ON;
                    INSERT INTO [Templates] ([Id],[Architecture],[Content],[CreatedAt],[Database],[Description],[Framework],[Infrastructure],[IsActive],[Name],[TemplateType],[Version])
                    VALUES ({template.Id},N'{template.Architecture}',N'{template.Content.Replace("'","''")}','{template.CreatedAt:yyyy-MM-dd HH:mm:ss}',{dbVal},N'{(template.Description??"").Replace("'","''")}',{fw},{infra},1,N'{template.Name.Replace("'","''")}',N'{template.TemplateType}',{template.Version});
                    SET IDENTITY_INSERT [Templates] OFF;
                    """;
                await db.Database.ExecuteSqlRawAsync(sqlT, ct);
                inserted = true;
            }
        }

        if (inserted)
            await db.SaveChangesAsync(ct);
    }
}
