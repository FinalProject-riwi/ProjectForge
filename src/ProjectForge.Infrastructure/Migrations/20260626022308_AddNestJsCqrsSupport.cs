using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNestJsCqrsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[] { 25, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Implementa CQRS con el paquete oficial de NestJS usando CommandBus, QueryBus y EventBus.", "Crear módulos, comandos, consultas, eventos de dominio y handlers; registrar CqrsModule en el modulo raiz.", null, "CQRS (NestJS)", "CQRS", "[\"npm install @nestjs/cqrs\"]", null });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[] { 36, "JavaScript", "CQRS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CommandBus, QueryBus y EventBus oficiales para NestJS", "NestJs", "npm install @nestjs/cqrs", "@nestjs/cqrs", "@nestjs/cqrs", 97, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DesignPatterns",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 36);
        }
    }
}
