using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixJavaScriptTemplateIdCollision : Migration
    {
        // JS Templates previously inserted with IDs 14-20 collide with PHP Templates 14-19.
        // This migration moves them to IDs 200-206, which are collision-free.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Insert with new IDs (copy data, change PK)
            // We do individual renames via temp approach:
            // SQL Server doesn't support UPDATE on PKs directly in EF style,
            // so we DELETE + INSERT for each row.

            // Get existing data and re-insert with new ID
            migrationBuilder.Sql(@"
                -- Move JS Templates IDs 14-20 → 200-206 to avoid collision with PHP Templates
                SET IDENTITY_INSERT Templates ON;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 200, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 14 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 14;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 201, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 15 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 15;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 202, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 16 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 16;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 203, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 17 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 17;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 204, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 18 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 18;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 205, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 19 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 19;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 206, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 20 AND Architecture = 'JavaScript';
                DELETE FROM Templates WHERE Id = 20;

                SET IDENTITY_INSERT Templates OFF;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Revert: 200-206 → 14-20
                SET IDENTITY_INSERT Templates ON;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 14, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 200;
                DELETE FROM Templates WHERE Id = 200;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 15, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 201;
                DELETE FROM Templates WHERE Id = 201;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 16, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 202;
                DELETE FROM Templates WHERE Id = 202;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 17, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 203;
                DELETE FROM Templates WHERE Id = 203;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 18, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 204;
                DELETE FROM Templates WHERE Id = 204;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 19, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 205;
                DELETE FROM Templates WHERE Id = 205;

                INSERT INTO Templates (Id, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version)
                SELECT 20, Architecture, Content, CreatedAt, [Database], Description, Framework, Infrastructure, IsActive, Name, TemplateType, UpdatedAt, VariablesSchemaJson, Version
                FROM Templates WHERE Id = 206;
                DELETE FROM Templates WHERE Id = 206;

                SET IDENTITY_INSERT Templates OFF;
            ");
        }
    }
}
