using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Seeders;

/// <summary>
/// Helper to upsert seed data rows with explicit IDs into IDENTITY columns.
/// Uses raw SQL MERGE so it works even after EF migrations have run.
/// </summary>
public static class SeedHelper
{
    /// <summary>
    /// Runs a raw SQL block wrapped in IDENTITY_INSERT ON/OFF for the given table.
    /// The sqlBody should be one or more IF NOT EXISTS ... INSERT statements.
    /// </summary>
    public static async Task ExecuteWithIdentityInsertAsync(
        AppDbContext db, string table, string sqlBody, CancellationToken ct = default)
    {
        var sql = $"""
            SET IDENTITY_INSERT [{table}] ON;
            {sqlBody}
            SET IDENTITY_INSERT [{table}] OFF;
            """;

        await db.Database.ExecuteSqlRawAsync(sql, ct);
    }

    /// <summary>
    /// Inserts a row only if the given Id does not yet exist in the table.
    /// columns: comma-separated column names (must include Id first)
    /// values:  comma-separated parameter placeholders (@p0, @p1, ...)
    /// </summary>
    public static string IfNotExists(string table, int id, string columns, string values)
        => $"""
            IF NOT EXISTS (SELECT 1 FROM [{table}] WHERE [Id] = {id})
                INSERT INTO [{table}] ({columns}) VALUES ({values});
            """;
}
