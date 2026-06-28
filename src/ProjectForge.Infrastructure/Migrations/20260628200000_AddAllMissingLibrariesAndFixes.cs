using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectForge.Infrastructure.Migrations;

/// <summary>
/// Adds missing libraries for JavaScript, TypeScript, Python, and Java.
/// </summary>
public partial class AddAllMissingLibrariesAndFixes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // JavaScript: ORM, Redis, JWT, Testing libraries (Ids 37-45)
        var jsLibs = new[]
        {
            (37, "JavaScript", "Prisma", "prisma", "ORM", "ORM type-safe moderno para Node.js", "npm install prisma @prisma/client", 97),
            (38, "JavaScript", "TypeORM", "typeorm", "ORM", "ORM con decoradores para NestJS y TypeScript", "npm install typeorm @nestjs/typeorm reflect-metadata", 91),
            (39, "JavaScript", "Mongoose", "mongoose", "ODM", "ODM popular para MongoDB en Node.js", "npm install mongoose", 95),
            (40, "JavaScript", "ioredis", "ioredis", "Cache", "Cliente Redis robusto para Node.js", "npm install ioredis", 92),
            (41, "JavaScript", "jsonwebtoken", "jsonwebtoken", "Autenticacion", "Generacion y verificacion de JWT", "npm install jsonwebtoken", 96),
            (42, "JavaScript", "bcryptjs", "bcryptjs", "Seguridad", "Hashing de passwords para Node.js", "npm install bcryptjs", 93),
            (43, "JavaScript", "Jest", "jest", "Testing", "Framework de testing para JS", "npm install --save-dev jest @types/jest", 99),
            (44, "JavaScript", "BullMQ", "bullmq", "Background Jobs", "Cola de trabajos con Redis", "npm install bullmq", 89),
            (45, "JavaScript", "nestjs-jwt", "nestjs-jwt", "Autenticacion", "Modulo JWT para NestJS", "npm install @nestjs/jwt @nestjs/passport", 94),
        };

        foreach (var (id, arch, name, pkg, cat, desc, cmd, score) in jsLibs)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = {id})
                BEGIN
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({id},N'{arch}',N'{cat}','2026-01-01 00:00:00',N'{desc}',NULL,N'{cmd}',N'{name}',N'{pkg}',{score});
                    SET IDENTITY_INSERT [Libraries] OFF;
                END
                """);
        }

        // TypeScript: Sequelize, Axios, BullMQ (Ids 510-514)
        var tsLibs = new[]
        {
            (510, "TypeScript", "Mongoose + NestJS", "nestjs-mongoose", "ODM", "Integracion de Mongoose con NestJS", "npm install @nestjs/mongoose mongoose", 93),
            (511, "TypeScript", "@nestjs/config", "@nestjs/config", "Configuration", "Gestion de configuracion por entorno para NestJS", "npm install @nestjs/config", 96),
            (512, "TypeScript", "Axios", "axios", "HTTP Client", "Cliente HTTP para TypeScript y Node.js", "npm install axios", 97),
            (513, "TypeScript", "bcrypt", "bcrypt", "Seguridad", "Hashing de passwords para Node.js", "npm install bcrypt @types/bcrypt", 91),
            (514, "TypeScript", "@nestjs/passport", "@nestjs/passport", "Autenticacion", "Integracion Passport.js para NestJS", "npm install @nestjs/passport passport passport-local passport-jwt", 93),
        };

        foreach (var (id, arch, name, pkg, cat, desc, cmd, score) in tsLibs)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = {id})
                BEGIN
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({id},N'{arch}',N'{cat}','2026-01-01 00:00:00',N'{desc}',NULL,N'{cmd}',N'{name}',N'{pkg}',{score});
                    SET IDENTITY_INSERT [Libraries] OFF;
                END
                """);
        }

        // Java: add Quarkus and Micronaut specific libraries
        var javaLibs = new[]
        {
            (1105, "Java", "Quarkus Hibernate ORM", "quarkus-hibernate-orm-panache", "ORM", "Hibernate ORM con Panache para Quarkus", "<!-- add to pom.xml: io.quarkus:quarkus-hibernate-orm-panache -->", 87),
            (1106, "Java", "Micronaut Data JPA", "micronaut-data-hibernate-jpa", "ORM", "Data JPA para Micronaut", "<!-- add to pom.xml: io.micronaut.data:micronaut-data-hibernate-jpa -->", 83),
            (1107, "Java", "Spring Boot Validation", "spring-boot-starter-validation", "Validation", "Validacion de beans con anotaciones JSR-303", "<!-- add to pom.xml: spring-boot-starter-validation -->", 98),
            (1108, "Java", "Liquibase", "liquibase-core", "Migraciones", "Migraciones de BD alternativa a Flyway", "<!-- add to pom.xml: org.liquibase:liquibase-core -->", 85),
            (1109, "Java", "Spring Data MongoDB", "spring-boot-starter-data-mongodb", "ODM", "Integracion MongoDB para Spring Boot", "<!-- add to pom.xml: spring-boot-starter-data-mongodb -->", 88),
        };

        foreach (var (id, arch, name, pkg, cat, desc, cmd, score) in javaLibs)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = {id})
                BEGIN
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({id},N'{arch}',N'{cat}','2026-01-01 00:00:00',N'{desc}',NULL,N'{cmd}',N'{name}',N'{pkg}',{score});
                    SET IDENTITY_INSERT [Libraries] OFF;
                END
                """);
        }

        // Python: add missing per-framework libraries
        var pyLibs = new[]
        {
            (1206, "Python", "FastAPI Users", "fastapi-users", "Autenticacion", "Auth lista con JWT y OAuth2 para FastAPI", "pip install fastapi-users[sqlalchemy]", 90),
            (1207, "Python", "Beanie", "beanie", "ODM", "ODM async para MongoDB con Pydantic", "pip install beanie motor", 88),
            (1208, "Python", "Gunicorn", "gunicorn", "Servidor", "Servidor WSGI para produccion", "pip install gunicorn", 95),
            (1209, "Python", "Django Environ", "django-environ", "Configuration", "Variables de entorno para Django", "pip install django-environ", 91),
            (1210, "Python", "Flask-JWT-Extended", "flask-jwt-extended", "Autenticacion", "JWT completo para Flask", "pip install flask-jwt-extended", 89),
        };

        foreach (var (id, arch, name, pkg, cat, desc, cmd, score) in pyLibs)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Libraries] WHERE [Id] = {id})
                BEGIN
                    SET IDENTITY_INSERT [Libraries] ON;
                    INSERT INTO [Libraries] ([Id],[Architecture],[Category],[CreatedAt],[Description],[Framework],[InstallCommand],[Name],[PackageName],[PopularityScore])
                    VALUES ({id},N'{arch}',N'{cat}','2026-01-01 00:00:00',N'{desc}',NULL,N'{cmd}',N'{name}',N'{pkg}',{score});
                    SET IDENTITY_INSERT [Libraries] OFF;
                END
                """);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove the inserted libraries
        foreach (var id in new[] { 37, 38, 39, 40, 41, 42, 43, 44, 45, 510, 511, 512, 513, 514, 1105, 1106, 1107, 1108, 1109, 1206, 1207, 1208, 1209, 1210 })
        {
            migrationBuilder.Sql($"DELETE FROM [Libraries] WHERE [Id] = {id}");
        }
    }
}
