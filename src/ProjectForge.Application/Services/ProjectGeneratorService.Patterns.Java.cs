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
    <java.version>21</java.version>
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

        // Framework
        switch (framework)
        {
            case FrameworkType.FastAPI:
                packages.Add("fastapi[standard]>=0.111.0");
                packages.Add("uvicorn[standard]>=0.29.0");
                break;
            case FrameworkType.Django:
                packages.Add("django>=5.0.0");
                packages.Add("djangorestframework>=3.15.0");
                break;
            case FrameworkType.Flask:
                packages.Add("flask>=3.0.0");
                packages.Add("flask-restful>=0.3.10");
                break;
            default:
                packages.Add("fastapi[standard]>=0.111.0");
                packages.Add("uvicorn[standard]>=0.29.0");
                break;
        }

        // DB
        switch (db)
        {
            case DatabaseType.PostgreSQL:
                packages.Add("sqlalchemy>=2.0.0");
                packages.Add("asyncpg>=0.29.0");
                packages.Add("psycopg2-binary>=2.9.9");
                break;
            case DatabaseType.MySQL:
                packages.Add("sqlalchemy>=2.0.0");
                packages.Add("pymysql>=1.1.0");
                packages.Add("aiomysql>=0.2.0");
                break;
            case DatabaseType.SqlServer:
                packages.Add("sqlalchemy>=2.0.0");
                packages.Add("pyodbc>=5.0.0");
                break;
            case DatabaseType.SQLite:
                packages.Add("sqlalchemy>=2.0.0");
                packages.Add("aiosqlite>=0.20.0");
                break;
            case DatabaseType.MongoDB:
                packages.Add("motor>=3.4.0");
                packages.Add("pymongo>=4.7.0");
                packages.Add("beanie>=1.26.0");
                break;
            case DatabaseType.Redis:
                packages.Add("redis[asyncio]>=5.0.0");
                packages.Add("aioredis>=2.0.1");
                break;
        }

        // Common
        packages.Add("python-dotenv>=1.0.0");
        packages.Add("pydantic>=2.7.0");
        packages.Add("pydantic-settings>=2.3.0");
        packages.Add("pytest>=8.2.0");
        packages.Add("pytest-asyncio>=0.23.0");
        packages.Add("httpx>=0.27.0");

        // Extra user libs (pip install commands stripped)
        foreach (var lib in libs)
        {
            var clean = lib.Replace("pip install ", "").Trim();
            if (!string.IsNullOrWhiteSpace(clean) && !packages.Any(p => p.StartsWith(clean.Split(' ')[0], StringComparison.OrdinalIgnoreCase)))
                packages.Add(clean);
        }

        return string.Join("\n", packages) + "\n";
    }

    private static string BuildPythonDatabaseConfig(DatabaseType db, string dbName) => db switch
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

    private static string BuildPythonEnvFile(DatabaseType db, string dbName) => db switch
    {
        DatabaseType.PostgreSQL  => $"DATABASE_URL=postgresql+asyncpg://postgres:secret@db:5432/{dbName}\nDEBUG=true\n",
        DatabaseType.MySQL       => $"DATABASE_URL=mysql+aiomysql://root:secret@db:3306/{dbName}\nDEBUG=true\n",
        DatabaseType.SqlServer   => $"DATABASE_URL=mssql+pyodbc://sa:YourStrong!Passw0rd@sqlserver:1433/{dbName}?driver=ODBC+Driver+18+for+SQL+Server&TrustServerCertificate=yes\nDEBUG=true\n",
        DatabaseType.SQLite      => $"DATABASE_URL=sqlite+aiosqlite:///./{dbName}.db\nDEBUG=true\n",
        DatabaseType.MongoDB     => $"MONGODB_URL=mongodb://mongo:27017/{dbName}\nDB_NAME={dbName}\nDEBUG=true\n",
        DatabaseType.Redis       => $"REDIS_URL=redis://redis:6379/0\nDEBUG=true\n",
        _                        => $"DATABASE_URL=postgresql+asyncpg://postgres:secret@db:5432/{dbName}\nDEBUG=true\n"
    };
}
