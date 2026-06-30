using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public static class WizardCompatibilityValidator
{
    public record CompatibilityIssue(string Code, string Message, bool IsError);

    public static IReadOnlyList<CompatibilityIssue> Validate(
        ArchitectureType arch,
        FrameworkType framework,
        DatabaseType primaryDb,
        InfrastructureType infra,
        IEnumerable<string> patterns)
    {
        var issues = new List<CompatibilityIssue>();
        var patternSet = patterns.Select(Tok).ToHashSet();

        // BlazorWasm runs in the browser — cannot connect to a database directly
        if (framework == FrameworkType.BlazorWasm && primaryDb != DatabaseType.SQLite)
            issues.Add(new("WASM_DIRECT_DB",
                "Blazor WebAssembly runs in the browser and cannot connect to a database directly. " +
                "Add an ASP.NET Core Web API backend to handle data access.",
                false));

        // MVVM is a UI-binding pattern, not applicable to API-only frameworks
        if (IsApiOnlyFramework(framework) && patternSet.Contains("mvvm"))
            issues.Add(new("MVVM_API_ONLY",
                $"MVVM is a UI-binding pattern and cannot be applied to {framework}, which is an API-only framework. " +
                "Use it with Blazor Server, MVC, or a frontend framework instead.",
                false));

        // Redis as the sole store is not suitable for EventSourcing/DDD (no durable event log)
        if (primaryDb == DatabaseType.Redis &&
            (patternSet.Contains("eventsourcing") || patternSet.Contains("domaindrivendesign")))
            issues.Add(new("REDIS_EVENT_STORE",
                "Redis is not suitable as the sole store for Event Sourcing or DDD aggregates — " +
                "it lacks durable event-log semantics. Consider PostgreSQL as primary and Redis as a cache layer.",
                false));

        // MinimalApi + Microservices generates a multi-service structure that doesn't fit Minimal API's design
        if (framework == FrameworkType.MinimalApi && patternSet.Contains("microservices"))
            issues.Add(new("MINIMAL_API_MICROSERVICES",
                "The Microservices scaffold generates a multi-service project structure better suited to ASP.NET Core Web API. " +
                "Minimal API targets lightweight single-endpoint services.",
                false));

        // EventSourcing is a backend persistence pattern — meaningless on frontend-only frameworks
        if (IsFrontendFramework(framework) && patternSet.Contains("eventsourcing"))
            issues.Add(new("EVENTSOURCING_FRONTEND",
                $"Event Sourcing is a backend persistence pattern and cannot be applied to {framework}, " +
                "which is a frontend-only framework.",
                true));

        // Saga without Microservices generates an incomplete scaffold
        if (patternSet.Contains("saga") && !patternSet.Contains("microservices"))
            issues.Add(new("SAGA_WITHOUT_MICROSERVICES",
                "The Saga pattern coordinates distributed transactions across multiple microservices. " +
                "Selecting it without the Microservices pattern may produce an incomplete scaffold.",
                false));

        return issues.AsReadOnly();
    }

    private static bool IsApiOnlyFramework(FrameworkType fw) => fw is
        FrameworkType.AspNetCoreWebApi or FrameworkType.MinimalApi or
        FrameworkType.SpringBoot or FrameworkType.Quarkus or FrameworkType.Micronaut or
        FrameworkType.FastAPI or FrameworkType.NestJs or FrameworkType.NestTs or
        FrameworkType.ExpressJs or FrameworkType.NodeJs;

    private static bool IsFrontendFramework(FrameworkType fw) => fw is
        FrameworkType.BlazorWasm or FrameworkType.NextJs or FrameworkType.NextTs;

    private static string Tok(string s) =>
        new string((s ?? string.Empty).Trim().ToLower().Where(char.IsLetterOrDigit).ToArray());
}