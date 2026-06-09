using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PopularityScore = table.Column<int>(type: "int", nullable: false),
                    InstallCommand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Database = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Infrastructure = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TemplateType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VariablesSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GitHubId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WizardConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrameworkVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Infrastructure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeploymentTarget = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DesignPatternsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibrariesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WizardConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DesignPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Architecture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImplementationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScaffoldCommandsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LibraryRecommendationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignPatterns_Libraries_LibraryRecommendationId",
                        column: x => x.LibraryRecommendationId,
                        principalTable: "Libraries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WizardConfigId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GeneratedReadme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_WizardConfigs_WizardConfigId",
                        column: x => x.WizardConfigId,
                        principalTable: "WizardConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VpsCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WizardConfigId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PrivateKeyPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpsCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VpsCredentials_WizardConfigs_WizardConfigId",
                        column: x => x.WizardConfigId,
                        principalTable: "WizardConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Step = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    CommandExecuted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitCode = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Abstrae el acceso a datos detrás de interfaces, facilitando testing y mantenimiento.", null, null, "Repository Pattern", "Repository", null, null },
                    { 2, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Separa las operaciones de lectura (Queries) de las de escritura (Commands).", null, null, "CQRS", "CQRS", null, null },
                    { 3, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura en capas concéntricas con dependencias hacia el centro.", null, null, "Clean Architecture", "CleanArchitecture", null, null },
                    { 4, "DotNet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reduce el acoplamiento directo entre componentes usando un mediador.", null, null, "Mediator", "Mediator", null, null },
                    { 5, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patrón de repositorio adaptado para Python/FastAPI.", null, null, "Repository Pattern", "Repository", null, null },
                    { 6, "JavaScript", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura de microservicios para aplicaciones Node.js.", null, null, "Microservices", "Microservices", null, null }
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 1, "DotNet", "CQRS/Mediator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Implementación del patrón Mediator para CQRS", "AspNetCoreWebApi", "dotnet add package MediatR", "MediatR", "MediatR", 95, null, null },
                    { 2, "DotNet", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM oficial de Microsoft para .NET", "AspNetCoreWebApi", "dotnet add package Microsoft.EntityFrameworkCore", "Entity Framework Core", "Microsoft.EntityFrameworkCore", 99, null, null },
                    { 3, "DotNet", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación fluida y expresiva", null, "dotnet add package FluentValidation.AspNetCore", "FluentValidation", "FluentValidation.AspNetCore", 92, null, null },
                    { 4, "DotNet", "Logging", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logging estructurado para .NET", null, "dotnet add package Serilog.AspNetCore", "Serilog", "Serilog.AspNetCore", 97, null, null },
                    { 5, "DotNet", "Mapping", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mapeo automático entre objetos", null, "dotnet add package AutoMapper", "AutoMapper", "AutoMapper", 94, null, null },
                    { 6, "DotNet", "Documentation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generación automática de documentación OpenAPI", "AspNetCoreWebApi", "dotnet add package Swashbuckle.AspNetCore", "Swashbuckle (Swagger)", "Swashbuckle.AspNetCore", 98, null, null },
                    { 7, "DotNet", "Testing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Framework de testing unitario para .NET", null, "dotnet add package xunit", "xUnit", "xunit", 96, null, null },
                    { 8, "Python", "ORM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM más popular para Python", null, "pip install sqlalchemy", "SQLAlchemy", "sqlalchemy", 98, null, null },
                    { 9, "Python", "Validation", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación de datos con type hints", null, "pip install pydantic", "Pydantic", "pydantic", 97, null, null },
                    { 10, "Python", "Migrations", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Migraciones de base de datos para SQLAlchemy", null, "pip install alembic", "Alembic", "alembic", 90, null, null }
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Architecture", "Content", "CreatedAt", "Database", "Description", "Framework", "Infrastructure", "IsActive", "Name", "TemplateType", "UpdatedAt", "VariablesSchemaJson", "Version" },
                values: new object[,]
                {
                    { 1, "DotNet", "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base\nWORKDIR /app\nEXPOSE 8080\n\nFROM mcr.microsoft.com/dotnet/sdk:10.0 AS build\nWORKDIR /src\nCOPY . .\nRUN dotnet restore\nRUN dotnet publish -c Release -o /app/publish\n\nFROM base AS final\nWORKDIR /app\nCOPY --from=build /app/publish .\nENTRYPOINT [\"dotnet\", \"{{APP_NAME}}.dll\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Multi-stage Dockerfile para ASP.NET Core", null, null, true, "Dockerfile .NET", "dockerfile", null, null, 1 },
                    { 2, "DotNet", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Host=db;Database={{DB_NAME}};Username=postgres;Password=secret\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: postgres:16-alpine\n    environment:\n      POSTGRES_DB: {{DB_NAME}}\n      POSTGRES_PASSWORD: secret\n    ports:\n      - \"{{DB_PORT}}:5432\"\n    volumes:\n      - pgdata:/var/lib/postgresql/data\n    healthcheck:\n      test: [\"CMD\",\"pg_isready\",\"-U\",\"postgres\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  pgdata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL", "Docker Compose para .NET + PostgreSQL", null, "DockerCompose", true, "Compose .NET + PostgreSQL", "compose", null, null, 1 },
                    { 3, "DotNet", "version: '3.9'\nservices:\n  app:\n    build: .\n    ports:\n      - \"{{APP_PORT}}:8080\"\n    environment:\n      - ConnectionStrings__Default=Server=db;Database={{DB_NAME}};User=root;Password=secret;\n    depends_on:\n      db:\n        condition: service_healthy\n\n  db:\n    image: mysql:8.0\n    environment:\n      MYSQL_ROOT_PASSWORD: secret\n      MYSQL_DATABASE: {{DB_NAME}}\n    ports:\n      - \"{{DB_PORT}}:3306\"\n    volumes:\n      - mysqldata:/var/lib/mysql\n    healthcheck:\n      test: [\"CMD\",\"mysqladmin\",\"ping\",\"-h\",\"localhost\"]\n      interval: 10s\n      timeout: 5s\n      retries: 5\n\nvolumes:\n  mysqldata:", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL", "Docker Compose para .NET + MySQL", null, "DockerCompose", true, "Compose .NET + MySQL", "compose", null, null, 1 },
                    { 4, "DotNet", "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: {{APP_NAME}}\nspec:\n  replicas: 2\n  selector:\n    matchLabels:\n      app: {{APP_NAME}}\n  template:\n    metadata:\n      labels:\n        app: {{APP_NAME}}\n    spec:\n      containers:\n      - name: {{APP_NAME}}\n        image: {{APP_NAME}}:latest\n        ports:\n        - containerPort: 8080\n---\napiVersion: v1\nkind: Service\nmetadata:\n  name: {{APP_NAME}}-svc\nspec:\n  selector:\n    app: {{APP_NAME}}\n  ports:\n  - port: 80\n    targetPort: 8080\n  type: LoadBalancer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kubernetes Deployment para la aplicación", null, "Kubernetes", true, "K8s App Deployment", "k8s-deployment", null, null, 1 },
                    { 5, "DotNet", "[Dd]ebug/\n[Rr]elease/\n[Bb]in/\n[Oo]bj/\n*.user\n*.suo\n*.vs/\n.vscode/\n.env\n.env.*\n*.pfx\npackages/", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gitignore para proyectos .NET", null, null, true, ".gitignore .NET", "gitignore", null, null, 1 },
                    { 6, "DotNet", "name: CI/CD\non:\n  push:\n    branches: [main]\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v4\n    - uses: actions/setup-dotnet@v4\n      with:\n        dotnet-version: '10.0.x'\n    - run: dotnet restore\n    - run: dotnet build --no-restore\n    - run: dotnet test --no-build", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pipeline CI/CD para .NET con GitHub Actions", null, null, true, "CI .NET GitHub Actions", "ci", null, null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignPatterns_LibraryRecommendationId",
                table: "DesignPatterns",
                column: "LibraryRecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLogs_ProjectId",
                table: "ProjectLogs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WizardConfigId",
                table: "Projects",
                column: "WizardConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Architecture_TemplateType_Database_Infrastructure",
                table: "Templates",
                columns: new[] { "Architecture", "TemplateType", "Database", "Infrastructure" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GitHubId",
                table: "Users",
                column: "GitHubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VpsCredentials_WizardConfigId",
                table: "VpsCredentials",
                column: "WizardConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignPatterns");

            migrationBuilder.DropTable(
                name: "ProjectLogs");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "VpsCredentials");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WizardConfigs");
        }
    }
}
