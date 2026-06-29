using ProjectForge.Infrastructure.Data;
using ProjectForge.Infrastructure.Seeders.DotNet;
using ProjectForge.Infrastructure.Seeders.Java;
using ProjectForge.Infrastructure.Seeders.Python;
using ProjectForge.Infrastructure.Seeders.Php;
using ProjectForge.Infrastructure.Seeders.JavaScript;
using ProjectForge.Infrastructure.Seeders.TypeScript;

namespace ProjectForge.Infrastructure.Seeders;

/// <summary>
/// Orquestador principal de seeders.
/// Orden:
///   1. Datos base por lenguaje (DesignPatterns, Libraries, Templates básicos)
///   2. Docker Compose templates completos: Framework x Database (114 combinaciones)
/// Todos los seeders son idempotentes (IF NOT EXISTS / AnyAsync check).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAllAsync(AppDbContext db, CancellationToken ct = default)
    {
        // ── Paso 1: Datos base por lenguaje ──────────────────────────────
        await DotNetSeeder.SeedAsync(db, ct);
        await JavaSeeder.SeedAsync(db, ct);
        await PythonSeeder.SeedAsync(db, ct);
        await PhpSeeder.SeedAsync(db, ct);
        await JavaScriptSeeder.SeedAsync(db, ct);
        await TypeScriptSeeder.SeedAsync(db, ct);

        // ── Paso 2: 114 Docker Compose templates (Framework × Database) ──
        // DotNet: 5 frameworks × 6 DB = 30  (IDs 10000–10045)
        await DotNetComposeSeeder.SeedComposeAsync(db, ct);

        // Java: 3 frameworks × 6 DB = 18  (IDs 10050–10075)
        await JavaComposeSeeder.SeedComposeAsync(db, ct);

        // Python: 3 frameworks × 6 DB = 18  (IDs 10080–10105)
        await PythonComposeSeeder.SeedComposeAsync(db, ct);

        // PHP: 2 frameworks × 6 DB = 12  (IDs 10110–10125)
        await PhpComposeSeeder.SeedComposeAsync(db, ct);

        // JavaScript: 4 frameworks × 6 DB = 24  (IDs 10130–10165)
        await JavaScriptComposeSeeder.SeedComposeAsync(db, ct);

        // TypeScript: 2 frameworks × 6 DB = 12  (IDs 10170–10185)
        await TypeScriptComposeSeeder.SeedComposeAsync(db, ct);
    }
}
