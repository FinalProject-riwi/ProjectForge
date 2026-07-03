using System.Text.Json;
using ProjectForge.Application.Services;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using Xunit;

namespace ProjectForge.Application.Tests;

/// <summary>
/// Direct, no-I/O tests of the internal Build*/Get* generator functions — regression coverage
/// for the Java/Python/C#/PHP/JS/TS scaffolding bugs found and fixed across previous review
/// sessions. These call the generators directly (via InternalsVisibleTo) rather than running the
/// full GenerateAsync pipeline, so they're fast and don't need a shell executor double.
/// </summary>
public class PatternGeneratorTests
{
    // ── Python: Django used to hardcode sqlite3 regardless of the selected database ──────────

    [Theory]
    [InlineData(DatabaseType.PostgreSQL, "django.db.backends.postgresql")]
    [InlineData(DatabaseType.MySQL, "django.db.backends.mysql")]
    public void Django_databases_block_matches_selected_database(DatabaseType db, string expectedEngine)
    {
        var block = ProjectGeneratorService.BuildDjangoDatabasesBlock(db);

        Assert.Contains(expectedEngine, block);
    }

    [Fact]
    public void Django_databases_block_falls_back_to_sqlite_for_mongo_and_redis()
    {
        // Django's ORM has no first-party Mongo/Redis backend — 'default' intentionally stays
        // on SQLite for auth/sessions/admin; Mongo/Redis get wired separately (see extras block).
        var mongo = ProjectGeneratorService.BuildDjangoDatabasesBlock(DatabaseType.MongoDB);
        var redis = ProjectGeneratorService.BuildDjangoDatabasesBlock(DatabaseType.Redis);

        Assert.Contains("django.db.backends.sqlite3", mongo);
        Assert.Contains("django.db.backends.sqlite3", redis);
    }

    [Fact]
    public void Django_extras_block_wires_redis_as_a_cache_not_a_database()
    {
        var extras = ProjectGeneratorService.BuildDjangoExtrasBlock(DatabaseType.Redis);

        Assert.Contains("django_redis.cache.RedisCache", extras);
    }

    // ── Python: Flask used to only set a dead SQLALCHEMY_DATABASE_URI, never db.init_app() ───

    [Fact]
    public void Flask_relational_main_initializes_the_sqlalchemy_extension()
    {
        var main = ProjectGeneratorService.BuildFlaskMain(DatabaseType.PostgreSQL, "app_db");

        Assert.Contains("db.init_app(app)", main);
        Assert.Contains("db.create_all()", main);
    }

    [Fact]
    public void Flask_mongo_main_does_not_use_the_relational_sqlalchemy_uri_key()
    {
        var main = ProjectGeneratorService.BuildFlaskMain(DatabaseType.MongoDB, "app_db");

        Assert.DoesNotContain("SQLALCHEMY_DATABASE_URI", main);
    }

    // ── Java: Quarkus/Micronaut used to get Spring Boot's application.properties/pom deps ────

    [Fact]
    public void Quarkus_properties_never_leak_spring_configuration_keys()
    {
        var props = ProjectGeneratorService.BuildQuarkusApplicationProperties(DatabaseType.PostgreSQL, "app_db");

        Assert.DoesNotContain("spring.", props);
        Assert.Contains("quarkus.datasource", props);
    }

    [Fact]
    public void Micronaut_properties_never_leak_spring_configuration_keys()
    {
        var props = ProjectGeneratorService.BuildMicronautApplicationProperties(DatabaseType.MongoDB, "app_db");

        Assert.DoesNotContain("spring.", props);
        Assert.Contains("mongodb.uri", props);
    }

    // ── C#: Repository/Hexagonal used to hardcode EF Core regardless of DB/framework ─────────

    [Fact]
    public void DotNet_repository_pattern_uses_http_client_for_blazor_wasm_regardless_of_database()
    {
        var files = ProjectGeneratorService.BuildDotNetRepositoryPatternFiles(FrameworkType.BlazorWasm, DatabaseType.PostgreSQL);

        Assert.Contains(files, f => f.Content.Contains("HttpClient"));
        Assert.DoesNotContain(files, f => f.Content.Contains("EntityFrameworkCore"));
    }

    [Fact]
    public void DotNet_hexagonal_pattern_uses_mongo_driver_for_mongo_not_ef_core()
    {
        var files = ProjectGeneratorService.BuildDotNetHexagonalPatternFiles(FrameworkType.AspNetCoreWebApi, DatabaseType.MongoDB);

        Assert.Contains(files, f => f.Content.Contains("MongoDB.Driver"));
        Assert.DoesNotContain(files, f => f.Content.Contains("AppDbContext"));
    }

    [Fact]
    public void DotNet_hexagonal_pattern_uses_ef_core_for_relational_databases()
    {
        var files = ProjectGeneratorService.BuildDotNetHexagonalPatternFiles(FrameworkType.AspNetCoreWebApi, DatabaseType.PostgreSQL);

        Assert.Contains(files, f => f.Content.Contains("AppDbContext"));
    }

    // ── PHP: CQRS/Mediator/Saga used to silently alias to the unrelated DDD scaffold ─────────

    [Fact]
    public void Php_cqrs_pattern_is_framework_appropriate_not_ddd()
    {
        var laravel = ProjectGeneratorService.BuildPhpCqrsPatternFiles(FrameworkType.Laravel);
        var symfony = ProjectGeneratorService.BuildPhpCqrsPatternFiles(FrameworkType.Symfony);

        Assert.Contains(laravel, f => f.RelativePath.Contains("Commands") && f.Content.Contains("namespace App\\Application\\Commands"));
        Assert.Contains(symfony, f => f.RelativePath.Contains("Command") && f.Content.Contains("namespace App\\Application\\Command"));
        Assert.DoesNotContain(laravel, f => f.Content.Contains("AggregateRoot"));
    }

    [Fact]
    public void Php_mediator_and_saga_patterns_are_distinct_from_each_other_and_from_ddd()
    {
        var mediator = ProjectGeneratorService.BuildPhpMediatorPatternFiles(FrameworkType.Laravel);
        var saga = ProjectGeneratorService.BuildPhpSagaPatternFiles(FrameworkType.Laravel);

        Assert.Contains(mediator, f => f.Content.Contains("class Mediator"));
        Assert.Contains(saga, f => f.Content.Contains("class OrderSaga"));
        Assert.DoesNotContain(mediator, f => f.Content.Contains("class OrderSaga"));
    }

    // ── JS/TS: no database wiring existed at all for any framework/database combination ──────

    [Theory]
    [InlineData(DatabaseType.PostgreSQL, "pg")]
    [InlineData(DatabaseType.MongoDB, "mongodb")]
    [InlineData(DatabaseType.Redis, "ioredis")]
    public void JavaScript_db_config_requires_the_matching_driver_package(DatabaseType db, string expectedPackage)
    {
        var config = ProjectGeneratorService.BuildJavaScriptDbConfig(db, "app_db", isTypeScript: false);

        Assert.Contains($"require('{expectedPackage}')", config);
    }

    [Fact]
    public void TypeScript_db_config_uses_import_syntax()
    {
        var config = ProjectGeneratorService.BuildJavaScriptDbConfig(DatabaseType.PostgreSQL, "app_db", isTypeScript: true);

        Assert.Contains("import { Pool } from 'pg';", config);
    }

    // ── C#: only CQRS used to trigger the implicit MediatR package, not Mediator/Saga ────────

    [Theory]
    [InlineData("Mediator")]
    [InlineData("Saga")]
    [InlineData("CQRS")]
    public void GetImplicitLibraries_adds_mediatr_for_every_pattern_that_generates_mediatr_code(string pattern)
    {
        var cfg = new WizardConfig
        {
            Architecture = ArchitectureType.DotNet,
            Framework = FrameworkType.AspNetCoreWebApi,
            Database = DatabaseType.SQLite,
            DesignPatternsJson = JsonSerializer.Serialize(new[] { pattern }),
            LibrariesJson = "[]"
        };

        var libraries = ProjectGeneratorService.GetImplicitLibraries(cfg).ToList();

        Assert.Contains("MediatR", libraries);
    }

    [Theory]
    [InlineData(DatabaseType.PostgreSQL, "pg")]
    [InlineData(DatabaseType.MongoDB, "mongodb")]
    public void GetImplicitLibraries_adds_the_js_driver_package_for_backend_frameworks(DatabaseType db, string expectedPackage)
    {
        var cfg = new WizardConfig
        {
            Architecture = ArchitectureType.JavaScript,
            Framework = FrameworkType.ExpressJs,
            Database = db,
            DesignPatternsJson = "[]",
            LibrariesJson = "[]"
        };

        var libraries = ProjectGeneratorService.GetImplicitLibraries(cfg).ToList();

        Assert.Contains(expectedPackage, libraries);
    }
}
