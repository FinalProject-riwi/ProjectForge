namespace ProjectForge.Core.Enums;

public enum ArchitectureType
{
    DotNet,
    Java,
    Python,
    Php,
    JavaScript,
    TypeScript
}

public enum DatabaseType
{
    MySQL,
    PostgreSQL,
    SqlServer,
    MongoDB,
    Redis,
    SQLite
}

public enum InfrastructureType
{
    None,
    DockerCompose,
    Kubernetes
}

public enum ProjectStatus
{
    Draft,
    Generating,
    Generated,
    Pushing,
    Published,
    Failed
}

public enum DeploymentTarget
{
    Local,
    SingleVPS,
    MultiVPS,
    Cloud
}

public enum FrameworkType
{
    // .NET
    AspNetCoreMVC,
    AspNetCoreWebApi,
    BlazorServer,
    BlazorWasm,
    MinimalApi,
    // Java
    SpringBoot,
    Quarkus,
    Micronaut,
    // Python
    FastAPI,
    Django,
    Flask,
    // PHP
    Laravel,
    Symfony,
    // JavaScript
    ExpressJs,
    NestJs,
    NextJs,
    // TypeScript
    NestTs,
    NextTs
}

public enum DesignPattern
{
    Repository,
    CQRS,
    Mediator,
    DomainDrivenDesign,
    CleanArchitecture,
    HexagonalArchitecture,
    Microservices,
    EventSourcing,
    Saga,
    MVVM
}
