using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeScriptSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Patrones TypeScript ───────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 12, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aísla el dominio de la infraestructura con puertos y adaptadores. Ideal para NestJS.", null, null, "Hexagonal Architecture (Ports & Adapters)", "HexagonalArchitecture", null, null },
                    { 13, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Organización basada en componentes reutilizables, clave en Next.js y NestJS.", null, null, "Component-Based Architecture", "Repository", null, null },
                    { 14, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comunicación reactiva entre componentes desacoplados usando observadores.", null, null, "Observer Pattern", "MVVM", null, null },
                    { 15, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Centraliza la comunicación entre módulos TypeScript evitando dependencias directas.", null, null, "Mediator Pattern", "Mediator", null, null },
                    { 16, "TypeScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aplicación de los 5 principios SOLID al diseño de módulos TypeScript/NestJS.", null, null, "SOLID Principles Integration", "CleanArchitecture", null, null }
                });

            // ── Librerías TypeScript ──────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 21, "TypeScript", "Validation",  new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación de esquemas con inferencia de tipos TypeScript",     null, "npm install zod",                                                  "Zod",              "zod",              97, null, null },
                    { 22, "TypeScript", "ORM",         new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM moderno con tipado automático para TypeScript/Node.js",     null, "npm install prisma @prisma/client",                                 "Prisma",           "prisma",           96, null, null },
                    { 23, "TypeScript", "ORM",         new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM basado en decoradores, nativo para TypeScript y NestJS",    null, "npm install typeorm reflect-metadata",                              "TypeORM",          "typeorm",          91, null, null },
                    { 24, "TypeScript", "Testing",     new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing rápido con soporte nativo para TypeScript", null, "npm install --save-dev jest ts-jest @types/jest",                  "Jest",             "jest",             98, null, null },
                    { 25, "TypeScript", "Reactive",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Librería para programación reactiva y manejo de streams",       null, "npm install rxjs",                                                  "RxJS",             "rxjs",             94, null, null },
                    { 26, "TypeScript", "Validation",  new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decoradores de validación para clases TypeScript (ideal NestJS)",null, "npm install class-validator class-transformer",                    "class-validator",  "class-validator",  93, null, null },
                    { 27, "TypeScript", "Config",      new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carga variables de entorno desde .env en Node.js/TypeScript",   null, "npm install dotenv",                                                "dotenv",           "dotenv",           99, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int i = 12; i <= 16; i++)
                migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValue: i);

            for (int i = 21; i <= 27; i++)
                migrationBuilder.DeleteData(table: "Libraries", keyColumn: "Id", keyValue: i);
        }
    }
}
