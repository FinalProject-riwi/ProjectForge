using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Java: scaffold por DB ────────────────────────────────────────────────

    private static string BuildJavaApplicationProperties(DatabaseType db, string dbName)
    {
        var dsn = db switch
        {
            DatabaseType.PostgreSQL  => $"jdbc:postgresql://db:5432/{dbName}",
            DatabaseType.MySQL       => $"jdbc:mysql://db:3306/{dbName}?useSSL=false&allowPublicKeyRetrieval=true",
            DatabaseType.SqlServer   => $"jdbc:sqlserver://sqlserver:1433;databaseName={dbName};encrypt=false;trustServerCertificate=true",
            DatabaseType.SQLite      => $"jdbc:sqlite:/app/data/{dbName}.db",
            _                        => $"jdbc:postgresql://db:5432/{dbName}"
        };

        return db switch
        {
            DatabaseType.MongoDB => $"""
spring.application.name={dbName}
spring.data.mongodb.uri=mongodb://mongo:27017/{dbName}
server.port=8080
management.endpoints.web.exposure.include=health,info
""",
            DatabaseType.Redis => $"""
spring.application.name={dbName}
spring.data.redis.host=redis
spring.data.redis.port=6379
server.port=8080
management.endpoints.web.exposure.include=health,info
""",
            DatabaseType.SQLite => $"""
spring.application.name={dbName}
spring.datasource.url={dsn}
spring.datasource.driver-class-name=org.sqlite.JDBC
spring.jpa.database-platform=org.hibernate.community.dialect.SQLiteDialect
spring.jpa.hibernate.ddl-auto=update
server.port=8080
""",
            _ => $"""
spring.application.name={dbName}
spring.datasource.url={dsn}
spring.datasource.username={GetJavaDbUser(db)}
spring.datasource.password={GetJavaDbPassword(db)}
spring.datasource.driver-class-name={GetJavaDriver(db)}
spring.jpa.hibernate.ddl-auto=update
spring.jpa.show-sql=false
server.port=8080
management.endpoints.web.exposure.include=health,info
"""
        };
    }

    private static string GetJavaDbUser(DatabaseType db) => db switch
    {
        DatabaseType.MySQL     => "root",
        DatabaseType.SqlServer => "sa",
        _                      => "postgres"
    };

    private static string GetJavaDbPassword(DatabaseType db) => db switch
    {
        DatabaseType.SqlServer => "YourStrong!Passw0rd",
        _                      => "secret"
    };

    private static string GetJavaDriver(DatabaseType db) => db switch
    {
        DatabaseType.MySQL     => "com.mysql.cj.jdbc.Driver",
        DatabaseType.SqlServer => "com.microsoft.sqlserver.jdbc.SQLServerDriver",
        _                      => "org.postgresql.Driver"
    };

    // ─── Quarkus: scaffold por DB ─────────────────────────────────────────────

    private static string BuildQuarkusApplicationProperties(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => $"""
quarkus.application.name={dbName}
quarkus.mongodb.connection-string=mongodb://mongo:27017
quarkus.mongodb.database={dbName}
""",
        DatabaseType.Redis => $"""
quarkus.application.name={dbName}
quarkus.redis.hosts=redis://redis:6379
""",
        DatabaseType.SQLite => $"""
quarkus.application.name={dbName}
# SQLite has no first-party Quarkus datasource extension. Consider the community
# io.quarkiverse.sqlite:quarkus-sqlite extension, or switch to PostgreSQL/MySQL.
""",
        _ => $"""
quarkus.application.name={dbName}
quarkus.datasource.db-kind={GetQuarkusDbKind(db)}
quarkus.datasource.username={GetJavaDbUser(db)}
quarkus.datasource.password={GetJavaDbPassword(db)}
quarkus.datasource.jdbc.url={GetJdbcUrl(db, dbName)}
quarkus.hibernate-orm.database.generation=update
"""
    };

    private static string GetQuarkusDbKind(DatabaseType db) => db switch
    {
        DatabaseType.MySQL     => "mysql",
        DatabaseType.SqlServer => "mssql",
        _                      => "postgresql"
    };

    private static string GetJdbcUrl(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MySQL     => $"jdbc:mysql://db:3306/{dbName}",
        DatabaseType.SqlServer => $"jdbc:sqlserver://sqlserver:1433;databaseName={dbName};encrypt=false;trustServerCertificate=true",
        _                      => $"jdbc:postgresql://db:5432/{dbName}"
    };

    private static string GetQuarkusJdbcExtension(DatabaseType db) => db switch
    {
        DatabaseType.MySQL     => "quarkus-jdbc-mysql",
        DatabaseType.SqlServer => "quarkus-jdbc-mssql",
        _                      => "quarkus-jdbc-postgresql"
    };

    private static async Task InjectQuarkusDbDependencyAsync(string pomPath, DatabaseType db, CancellationToken ct)
    {
        var content = await File.ReadAllTextAsync(pomPath, ct);
        var marker  = "</dependencies>";
        if (!content.Contains(marker)) return;

        string? snippet = db switch
        {
            DatabaseType.MongoDB when !content.Contains("quarkus-mongodb") => """
        <dependency>
            <groupId>io.quarkus</groupId>
            <artifactId>quarkus-mongodb-panache</artifactId>
        </dependency>
""",
            DatabaseType.Redis when !content.Contains("quarkus-redis-client") => """
        <dependency>
            <groupId>io.quarkus</groupId>
            <artifactId>quarkus-redis-client</artifactId>
        </dependency>
""",
            DatabaseType.SQLite => null, // no official extension — see application.properties comment
            DatabaseType.MongoDB or DatabaseType.Redis => null, // already present
            _ when !content.Contains(GetQuarkusJdbcExtension(db)) => $"""
        <dependency>
            <groupId>io.quarkus</groupId>
            <artifactId>quarkus-hibernate-orm</artifactId>
        </dependency>
        <dependency>
            <groupId>io.quarkus</groupId>
            <artifactId>{GetQuarkusJdbcExtension(db)}</artifactId>
        </dependency>
""",
            _ => null
        };

        if (snippet != null)
            await File.WriteAllTextAsync(pomPath, content.Replace(marker, snippet + marker), ct);
    }

    private static string BuildQuarkusEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => $"QUARKUS_MONGODB_CONNECTION_STRING=mongodb://mongo:27017\nQUARKUS_MONGODB_DATABASE={dbName}\n",
        DatabaseType.Redis   => "QUARKUS_REDIS_HOSTS=redis://redis:6379\n",
        DatabaseType.SQLite  => "",
        _ => $"QUARKUS_DATASOURCE_JDBC_URL={GetJdbcUrl(db, dbName)}\nQUARKUS_DATASOURCE_USERNAME={GetJavaDbUser(db)}\nQUARKUS_DATASOURCE_PASSWORD={GetJavaDbPassword(db)}\n"
    };

    // ─── Micronaut: scaffold por DB ───────────────────────────────────────────

    private static string BuildMicronautApplicationProperties(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => $"""
micronaut.application.name={dbName}
mongodb.uri=mongodb://mongo:27017/{dbName}
""",
        DatabaseType.Redis => $"""
micronaut.application.name={dbName}
redis.uri=redis://redis:6379
""",
        DatabaseType.SQLite => $"""
micronaut.application.name={dbName}
# Micronaut Data JDBC has no official SQLite dialect. Consider PostgreSQL/MySQL, or wire
# SQLite manually via a plain javax.sql.DataSource + the org.xerial:sqlite-jdbc driver.
""",
        _ => $"""
micronaut.application.name={dbName}
datasources.default.url={GetJdbcUrl(db, dbName)}
datasources.default.username={GetJavaDbUser(db)}
datasources.default.password={GetJavaDbPassword(db)}
datasources.default.driver-class-name={GetJavaDriver(db)}
"""
    };

    private static (string GroupId, string ArtifactId) GetJavaJdbcDriverCoords(DatabaseType db) => db switch
    {
        DatabaseType.MySQL     => ("com.mysql", "mysql-connector-j"),
        DatabaseType.SqlServer => ("com.microsoft.sqlserver", "mssql-jdbc"),
        _                      => ("org.postgresql", "postgresql")
    };

    private static async Task InjectMicronautDbDependencyAsync(string pomPath, DatabaseType db, CancellationToken ct)
    {
        var content = await File.ReadAllTextAsync(pomPath, ct);
        var marker  = "</dependencies>";
        if (!content.Contains(marker)) return;

        string? snippet = db switch
        {
            DatabaseType.MongoDB when !content.Contains("micronaut-mongo-reactive") => """
        <dependency>
            <groupId>io.micronaut.mongodb</groupId>
            <artifactId>micronaut-mongo-reactive</artifactId>
        </dependency>
""",
            DatabaseType.Redis when !content.Contains("micronaut-redis-lettuce") => """
        <dependency>
            <groupId>io.micronaut.redis</groupId>
            <artifactId>micronaut-redis-lettuce</artifactId>
        </dependency>
""",
            DatabaseType.SQLite => null,
            DatabaseType.MongoDB or DatabaseType.Redis => null, // already present
            _ when !content.Contains("micronaut-jdbc-hikari") => $"""
        <dependency>
            <groupId>io.micronaut.sql</groupId>
            <artifactId>micronaut-jdbc-hikari</artifactId>
        </dependency>
        <dependency>
            <groupId>{GetJavaJdbcDriverCoords(db).GroupId}</groupId>
            <artifactId>{GetJavaJdbcDriverCoords(db).ArtifactId}</artifactId>
        </dependency>
""",
            _ => null
        };

        if (snippet != null)
            await File.WriteAllTextAsync(pomPath, content.Replace(marker, snippet + marker), ct);
    }

    private static string BuildMicronautEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => $"MONGODB_URI=mongodb://mongo:27017/{dbName}\n",
        DatabaseType.Redis   => "REDIS_URI=redis://redis:6379\n",
        DatabaseType.SQLite  => "",
        _ => $"DATASOURCES_DEFAULT_URL={GetJdbcUrl(db, dbName)}\nDATASOURCES_DEFAULT_USERNAME={GetJavaDbUser(db)}\nDATASOURCES_DEFAULT_PASSWORD={GetJavaDbPassword(db)}\n"
    };

    private static string BuildJavaPomXml(string projectName, DatabaseType db, IList<string> libs)
    {
        var dbDependencies = db switch
        {
            DatabaseType.PostgreSQL  => """
        <dependency>
            <groupId>org.postgresql</groupId>
            <artifactId>postgresql</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.MySQL       => """
        <dependency>
            <groupId>com.mysql</groupId>
            <artifactId>mysql-connector-j</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.SqlServer   => """
        <dependency>
            <groupId>com.microsoft.sqlserver</groupId>
            <artifactId>mssql-jdbc</artifactId>
            <scope>runtime</scope>
        </dependency>
""",
            DatabaseType.MongoDB     => """
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-mongodb</artifactId>
        </dependency>
""",
            DatabaseType.Redis       => """
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-redis</artifactId>
        </dependency>
""",
            DatabaseType.SQLite      => """
        <dependency>
            <groupId>org.xerial</groupId>
            <artifactId>sqlite-jdbc</artifactId>
            <version>3.45.2.0</version>
        </dependency>
        <dependency>
            <groupId>org.hibernate.orm</groupId>
            <artifactId>hibernate-community-dialects</artifactId>
        </dependency>
""",
            _ => ""
        };

        var jpaStarter = db is DatabaseType.MongoDB or DatabaseType.Redis ? "" : """
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-data-jpa</artifactId>
        </dependency>
""";

        return $"""
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>
    <parent>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-parent</artifactId>
        <version>3.3.0</version>
        <relativePath/>
    </parent>
    <groupId>com.example</groupId>
    <artifactId>{projectName.ToLower().Replace(" ", "-")}</artifactId>
    <version>0.0.1-SNAPSHOT</version>
    <name>{projectName}</name>
    <description>Generated by ProjectForge</description>
    <properties>
        <java.version>21</java.version>
    </properties>
    <dependencies>
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-web</artifactId>
        </dependency>
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-actuator</artifactId>
        </dependency>
{jpaStarter}
{dbDependencies}
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-test</artifactId>
            <scope>test</scope>
        </dependency>
    </dependencies>
    <build>
        <plugins>
            <plugin>
                <groupId>org.springframework.boot</groupId>
                <artifactId>spring-boot-maven-plugin</artifactId>
            </plugin>
        </plugins>
    </build>
</project>
""";
    }

    // ─── Python: scaffold por DB ──────────────────────────────────────────────

    private static string BuildPythonRequirementsTxt(FrameworkType framework, DatabaseType db, IList<string> libs)
    {
        var packages = new List<string>();

        // Framework + framework-appropriate DB driver. Django's ORM is synchronous and has no
        // first-party async/Mongo/Redis support, and standard (WSGI) Flask can't use asyncio
        // drivers either — reusing the FastAPI async SQLAlchemy stack for those two used to
        // install packages the generated app cannot actually use (asyncpg/aiomysql with a sync
        // framework, or beanie/motor with no code that imports them).
        switch (framework)
        {
            case FrameworkType.FastAPI:
                packages.Add("fastapi[standard]>=0.111.0");
                packages.Add("uvicorn[standard]>=0.29.0");
                packages.AddRange(GetFastApiDbPackages(db));
                packages.Add("pydantic>=2.7.0");
                packages.Add("pydantic-settings>=2.3.0");
                packages.Add("pytest-asyncio>=0.23.0");
                packages.Add("httpx>=0.27.0");
                break;
            case FrameworkType.Django:
                packages.Add("django>=5.0.0");
                packages.Add("djangorestframework>=3.15.0");
                packages.AddRange(GetDjangoDbPackages(db));
                break;
            case FrameworkType.Flask:
                packages.Add("flask>=3.0.0");
                packages.Add("flask-restful>=0.3.10");
                packages.AddRange(GetFlaskDbPackages(db));
                break;
            default:
                packages.Add("fastapi[standard]>=0.111.0");
                packages.Add("uvicorn[standard]>=0.29.0");
                packages.AddRange(GetFastApiDbPackages(db));
                break;
        }

        // Common
        packages.Add("python-dotenv>=1.0.0");
        packages.Add("pytest>=8.2.0");

        // Extra user libs (pip install commands stripped)
        foreach (var lib in libs)
        {
            var clean = lib.Replace("pip install ", "").Trim();
            if (!string.IsNullOrWhiteSpace(clean) && !packages.Any(p => p.StartsWith(clean.Split(' ')[0], StringComparison.OrdinalIgnoreCase)))
                packages.Add(clean);
        }

        return string.Join("\n", packages) + "\n";
    }

    private static IReadOnlyList<string> GetFastApiDbPackages(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => new[] { "sqlalchemy>=2.0.0", "asyncpg>=0.29.0" },
        DatabaseType.MySQL      => new[] { "sqlalchemy>=2.0.0", "aiomysql>=0.2.0" },
        DatabaseType.SqlServer  => new[] { "sqlalchemy>=2.0.0", "pyodbc>=5.0.0" },
        DatabaseType.SQLite     => new[] { "sqlalchemy>=2.0.0", "aiosqlite>=0.20.0" },
        DatabaseType.MongoDB    => new[] { "motor>=3.4.0", "pymongo>=4.7.0", "beanie>=1.26.0" },
        DatabaseType.Redis      => new[] { "redis[asyncio]>=5.0.0" },
        _                       => Array.Empty<string>()
    };

    // Flask (WSGI) is synchronous — it needs the sync SQLAlchemy driver + the Flask-SQLAlchemy
    // extension, not the asyncio drivers FastAPI uses.
    private static IReadOnlyList<string> GetFlaskDbPackages(DatabaseType db) => db switch
    {
        DatabaseType.PostgreSQL => new[] { "sqlalchemy>=2.0.0", "flask-sqlalchemy>=3.1.0", "psycopg2-binary>=2.9.9" },
        DatabaseType.MySQL      => new[] { "sqlalchemy>=2.0.0", "flask-sqlalchemy>=3.1.0", "pymysql>=1.1.0" },
        DatabaseType.SqlServer  => new[] { "sqlalchemy>=2.0.0", "flask-sqlalchemy>=3.1.0", "pyodbc>=5.0.0" },
        DatabaseType.SQLite     => new[] { "sqlalchemy>=2.0.0", "flask-sqlalchemy>=3.1.0" },
        DatabaseType.MongoDB    => new[] { "pymongo>=4.7.0" },
        DatabaseType.Redis      => new[] { "redis>=5.0.0" },
        _                       => Array.Empty<string>()
    };

    private static string BuildPythonDatabaseConfig(FrameworkType fw, DatabaseType db, string dbName) =>
        fw == FrameworkType.Flask
            ? BuildFlaskDatabaseConfig(db, dbName)
            : BuildAsyncPythonDatabaseConfig(db, dbName);

    // Flask (WSGI) is synchronous — it can't use the asyncio-only motor/aioredis/async-SQLAlchemy
    // clients built below for FastAPI.
    private static string BuildFlaskDatabaseConfig(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.MongoDB => $$"""
from pymongo import MongoClient
import os

MONGODB_URL = os.getenv("MONGODB_URL", "mongodb://mongo:27017/{{dbName}}")
DB_NAME = os.getenv("DB_NAME", "{{dbName}}")

client = MongoClient(MONGODB_URL)
db = client[DB_NAME]
""",
        DatabaseType.Redis => """
import redis
import os

REDIS_URL = os.getenv("REDIS_URL", "redis://redis:6379/0")
redis_client = redis.Redis.from_url(REDIS_URL, decode_responses=True)
""",
        _ => """
from flask_sqlalchemy import SQLAlchemy

# Bound to the app in app/main.py via db.init_app(app)
db = SQLAlchemy()
"""
    };

    private static string BuildAsyncPythonDatabaseConfig(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL => $"""
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import DeclarativeBase, sessionmaker
import os

DATABASE_URL = os.getenv("DATABASE_URL", "postgresql+asyncpg://postgres:secret@db:5432/{dbName}")

engine = create_async_engine(DATABASE_URL, echo=False)
AsyncSessionLocal = sessionmaker(engine, class_=AsyncSession, expire_on_commit=False)

class Base(DeclarativeBase):
    pass

async def get_db():
    async with AsyncSessionLocal() as session:
        yield session
""",
        DatabaseType.MySQL => $"""
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import DeclarativeBase, sessionmaker
import os

DATABASE_URL = os.getenv("DATABASE_URL", "mysql+aiomysql://root:secret@db:3306/{dbName}")

engine = create_async_engine(DATABASE_URL, echo=False)
AsyncSessionLocal = sessionmaker(engine, class_=AsyncSession, expire_on_commit=False)

class Base(DeclarativeBase):
    pass

async def get_db():
    async with AsyncSessionLocal() as session:
        yield session
""",
        DatabaseType.SqlServer => $"""
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import DeclarativeBase, sessionmaker
import os

DATABASE_URL = os.getenv("DATABASE_URL",
    "mssql+pyodbc://sa:YourStrong!Passw0rd@sqlserver:1433/{dbName}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes")

engine = create_async_engine(DATABASE_URL, echo=False)
AsyncSessionLocal = sessionmaker(engine, class_=AsyncSession, expire_on_commit=False)

class Base(DeclarativeBase):
    pass

async def get_db():
    async with AsyncSessionLocal() as session:
        yield session
""",
        DatabaseType.SQLite => 
            "from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession\n" +
            "from sqlalchemy.orm import DeclarativeBase, sessionmaker\n" +
            "import os\n\n" +
            $"DATABASE_URL = os.getenv(\"DATABASE_URL\", \"sqlite+aiosqlite:///./{dbName}.db\")\n\n" +
            "engine = create_async_engine(DATABASE_URL, echo=False, connect_args={\"check_same_thread\": False})\n" +
            "AsyncSessionLocal = sessionmaker(engine, class_=AsyncSession, expire_on_commit=False)\n\n" +
            "class Base(DeclarativeBase):\n" +
            "    pass\n\n" +
            "async def get_db():\n" +
            "    async with AsyncSessionLocal() as session:\n" +
            "        yield session\n",
        DatabaseType.MongoDB => $"""
from motor.motor_asyncio import AsyncIOMotorClient
import os

MONGODB_URL = os.getenv("MONGODB_URL", "mongodb://mongo:27017/{dbName}")
DB_NAME = os.getenv("DB_NAME", "{dbName}")

client: AsyncIOMotorClient = None
db = None

async def connect_db():
    global client, db
    client = AsyncIOMotorClient(MONGODB_URL)
    db = client[DB_NAME]

async def close_db():
    if client:
        client.close()

async def get_db():
    return db
""",
        DatabaseType.Redis => $"""
import redis.asyncio as redis
import os

REDIS_URL = os.getenv("REDIS_URL", "redis://redis:6379/0")

redis_client: redis.Redis = None

async def connect_redis():
    global redis_client
    redis_client = redis.from_url(REDIS_URL, encoding="utf-8", decode_responses=True)

async def close_redis():
    if redis_client:
        await redis_client.close()

async def get_redis():
    return redis_client
""",
        _ => $"""
import os
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql+asyncpg://postgres:secret@db:5432/{dbName}")
"""
    };

    // Includes both the SQLAlchemy-style DATABASE_URL (read by FastAPI/Flask's app/database.py)
    // and the plain DB_HOST/DB_PORT/... vars (read by Django's settings.py) — this file is
    // written once and shared by whichever framework ends up needing it, so it must satisfy
    // both readers regardless of which one is active.
    private static string BuildPythonEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL  => $"DATABASE_URL=postgresql+asyncpg://postgres:secret@db:5432/{dbName}\nDB_HOST=db\nDB_PORT=5432\nDB_NAME={dbName}\nDB_USER=postgres\nDB_PASSWORD=secret\nDEBUG=true\n",
        DatabaseType.MySQL       => $"DATABASE_URL=mysql+aiomysql://root:secret@db:3306/{dbName}\nDB_HOST=db\nDB_PORT=3306\nDB_NAME={dbName}\nDB_USER=root\nDB_PASSWORD=secret\nDEBUG=true\n",
        DatabaseType.SqlServer   => $"DATABASE_URL=mssql+pyodbc://sa:YourStrong!Passw0rd@sqlserver:1433/{dbName}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes\nDB_HOST=sqlserver\nDB_PORT=1433\nDB_NAME={dbName}\nDB_USER=sa\nDB_PASSWORD=YourStrong!Passw0rd\nDEBUG=true\n",
        DatabaseType.SQLite      => $"DATABASE_URL=sqlite+aiosqlite:///./{dbName}.db\nDB_NAME={dbName}\nDEBUG=true\n",
        DatabaseType.MongoDB     => $"MONGODB_URL=mongodb://mongo:27017/{dbName}\nDB_NAME={dbName}\nDEBUG=true\n",
        DatabaseType.Redis       => $"REDIS_URL=redis://redis:6379/0\nDEBUG=true\n",
        _                        => $"DATABASE_URL=postgresql+asyncpg://postgres:secret@db:5432/{dbName}\nDB_HOST=db\nDB_PORT=5432\nDB_NAME={dbName}\nDB_USER=postgres\nDB_PASSWORD=secret\nDEBUG=true\n"
    };
}
