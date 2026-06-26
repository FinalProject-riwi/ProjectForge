using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPatternsAndLibraries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Patrones Python adicionales (screenshots) ─────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 17, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patrón MVT de Django: Model-View-Template para separar lógica, datos y presentación.", null, null, "Model-View-Template (MVT)", "Repository", null, null },
                    { 18, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura de microservicios con FastAPI: servicios pequeños e independientes comunicados via HTTP/async.", null, null, "Microservices", "Microservices", null, null },
                    { 19, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Crea objetos sin especificar la clase exacta. Muy usado en Django para forms, serializers y conexiones.", null, null, "Factory Method", "Repository", null, null },
                    { 20, "Python", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Añade comportamiento a funciones/clases de forma dinámica usando @decoradores nativos de Python.", null, null, "Decorator Pattern", "CleanArchitecture", null, null },
                });

            // ── Patrones Java adicionales (screenshots) ───────────────────────
            migrationBuilder.InsertData(
                table: "DesignPatterns",
                columns: new[] { "Id", "Architecture", "CreatedAt", "Description", "ImplementationNotes", "LibraryRecommendationId", "Name", "Pattern", "ScaffoldCommandsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { 21, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patrón MVC con Spring MVC: Model-View-Controller para separar lógica de negocio, presentación y control de flujo.", null, null, "MVC (Model-View-Controller)", "Repository", null, null },
                    { 22, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arquitectura en capas concéntricas (Domain, Application, Infrastructure, Presentation) con Spring Boot.", null, null, "Clean Architecture", "CleanArchitecture", null, null },
                    { 23, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Domain-Driven Design con Aggregates, Entities, Value Objects y Domain Events en Spring Boot.", null, null, "Domain-Driven Design (DDD)", "DomainDrivenDesign", null, null },
                    { 24, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Garantiza una única instancia de una clase (ej: conexiones a BD). Implementado con @Bean en Spring.", null, null, "Singleton Pattern", "Repository", null, null },
                    { 25, "Java", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inversión de control (IoC) con el contenedor de Spring: @Autowired, @Inject, @Component.", null, null, "Dependency Injection", "CleanArchitecture", null, null },
                });

            // ── Librerías Python con descripción detallada ────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 28, "Python", "Validation",  new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Validación de datos con tipado. Cuando una API mande un JSON con campos inesperados, lanzará un error decente en lugar de que tu app colapse silenciosamente.", null, "pip install pydantic", "Pydantic", "pydantic", 99, null, null },
                    { 29, "Python", "ORM",         new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ORM por excelencia para implementar el patrón Repository. Separa la lógica de la base de datos de tu código principal.", null, "pip install sqlalchemy", "SQLAlchemy", "sqlalchemy", 97, null, null },
                    { 30, "Python", "Framework",   new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ideal si decidiste hacer Microservicios. Rápido, moderno y se integra perfectamente con Pydantic.", null, "pip install fastapi uvicorn", "FastAPI", "fastapi", 98, null, null },
                });

            // ── Librerías Java con descripción detallada ──────────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 31, "Java", "Framework",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "El estándar absoluto para Java empresarial. Ya trae todo el contenedor de inyección de dependencias y la estructura MVC lista para usar.", "SpringBoot", "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-web", "Spring Boot", "org.springframework.boot:spring-boot-starter-web", 100, null, null },
                    { 32, "Java", "ORM",          new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Facilita enormemente implementar el patrón Repository de forma casi automática con unas pocas interfaces.", "SpringBoot", "mvn dependency:get -Dartifact=org.springframework.boot:spring-boot-starter-data-jpa", "Spring Data JPA", "org.springframework.boot:spring-boot-starter-data-jpa", 99, null, null },
                    { 33, "Java", "Mapping",      new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Si vas a usar Clean Architecture o DDD, mapea datos de tu BD a DTOs automáticamente. Genera el código por ti para no escribir getters/setters a mano.", null, "mvn dependency:get -Dartifact=org.mapstruct:mapstruct:1.5.5.Final", "MapStruct", "org.mapstruct:mapstruct", 91, null, null },
                });

            // ── Librerías TypeScript con descripción detallada ────────────────
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Architecture", "Category", "CreatedAt", "Description", "Framework", "InstallCommand", "Name", "PackageName", "PopularityScore", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 34, "TypeScript", "Validation",  new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tu mejor amigo. Define esquemas y valida que el JSON que recibes tenga exactamente la estructura que esperas. Si no cumple, explota antes de dañar tu interfaz.", null, "npm install zod", "Zod", "zod", 97, null, null },
                    { 35, "TypeScript", "State/Fetch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Maneja el estado de carga (isLoading), los errores y la caché automáticamente al consumir tu API en componentes. Así no tienes excusa si la UI se cuelga.", null, "npm install @tanstack/react-query", "TanStack Query (React Query)", "@tanstack/react-query", 96, null, null },
                    { 36, "TypeScript", "DI",          new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Si quieres aplicar Inyección de Dependencias en TypeScript de forma estricta, como en un entorno corporativo.", null, "npm install inversify reflect-metadata", "InversifyJS", "inversify", 85, null, null },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int i = 17; i <= 25; i++)
                migrationBuilder.DeleteData(table: "DesignPatterns", keyColumn: "Id", keyValue: i);

            for (int i = 28; i <= 36; i++)
                migrationBuilder.DeleteData(table: "Libraries", keyColumn: "Id", keyValue: i);
        }
    }
}
