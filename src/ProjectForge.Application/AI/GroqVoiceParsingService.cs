using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ProjectForge.Application.AI;

public record VoiceParseResult(
    string Architecture,
    string Framework,
    string Database,
    string Infrastructure,
    string? ProjectName
);

public interface IVoiceParsingService
{
    Task<VoiceParseResult?> ParseAsync(string transcript);
}

/// <summary>
/// Parses a natural-language voice transcript into a structured WizardConfig
/// using Groq (Llama 3.3-70B) — free tier, sub-second latency.
/// Falls back to keyword matching when the API key is not configured.
/// </summary>
public class GroqVoiceParsingService : IVoiceParsingService
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public GroqVoiceParsingService(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private const string SystemPrompt = """
        You are a software project configuration extractor for ProjectForge.
        Given a natural language description (possibly in Spanish or English), extract the project configuration.

        Available values — use EXACT spelling:
          architecture : DotNet | Java | Python | Php | JavaScript | TypeScript
          framework (DotNet)      : AspNetCoreWebApi | AspNetCoreMVC | BlazorServer | BlazorWasm | MinimalApi
          framework (Java)        : SpringBoot | Quarkus | Micronaut
          framework (Python)      : FastAPI | Django | Flask
          framework (Php)         : Laravel | Symfony
          framework (JavaScript)  : NodeJs | ExpressJs | NestJs | NextJs
          framework (TypeScript)  : NestTs | NextTs
          database     : PostgreSQL | MySQL | SqlServer | MongoDB | Redis | SQLite
          infrastructure : None | DockerCompose | Kubernetes

        Defaults when not mentioned: architecture=DotNet, framework=AspNetCoreWebApi, database=PostgreSQL, infrastructure=None.
        For projectName: extract if clearly mentioned (e.g. "llamado mi-api"), otherwise return null.

        Respond ONLY with a single-line JSON object — no markdown, no explanation:
        {"architecture":"...","framework":"...","database":"...","infrastructure":"...","projectName":null}
        """;

    public async Task<VoiceParseResult?> ParseAsync(string transcript)
    {
        var apiKey = _config["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "PLACEHOLDER")
            return FallbackParse(transcript);

        try
        {
            var http = _http.CreateClient("Groq");
            var body = new
            {
                model       = "llama-3.3-70b-versatile",
                max_tokens  = 128,
                temperature = 0.1,
                messages    = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = transcript   }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resp = await http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var json    = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            return ParseJson(content) ?? FallbackParse(transcript);
        }
        catch
        {
            return FallbackParse(transcript);
        }
    }

    private static VoiceParseResult? ParseJson(string raw)
    {
        var clean = raw.Trim();
        if (clean.StartsWith("```"))
        {
            var s = clean.IndexOf('{');
            var e = clean.LastIndexOf('}');
            if (s >= 0 && e > s) clean = clean[s..(e + 1)];
        }
        try
        {
            using var doc = JsonDocument.Parse(clean);
            var r = doc.RootElement;
            return new VoiceParseResult(
                Str(r, "architecture", "DotNet"),
                Str(r, "framework",    "AspNetCoreWebApi"),
                Str(r, "database",     "PostgreSQL"),
                Str(r, "infrastructure", "None"),
                NullableStr(r, "projectName")
            );
        }
        catch { return null; }
    }

    // Keyword-based fallback — no API key required
    private static VoiceParseResult FallbackParse(string text)
    {
        var t = text.ToLowerInvariant();

        var arch = t.Contains("python") ? "Python"
            : System.Text.RegularExpressions.Regex.IsMatch(t, @"\bjava\b") && !t.Contains("javascript") ? "Java"
            : t.Contains("php") || t.Contains("laravel") || t.Contains("symfony") ? "Php"
            : t.Contains("typescript") ? "TypeScript"
            : t.Contains("javascript") || t.Contains("node") ? "JavaScript"
            : "DotNet";

        var fw = arch switch
        {
            "Python"     => t.Contains("django") ? "Django" : t.Contains("flask") ? "Flask" : "FastAPI",
            "Java"       => t.Contains("quarkus") ? "Quarkus" : t.Contains("micronaut") ? "Micronaut" : "SpringBoot",
            "Php"        => t.Contains("symfony") ? "Symfony" : "Laravel",
            "JavaScript" => t.Contains("express") ? "ExpressJs" : t.Contains("next") ? "NextJs" : t.Contains("nest") ? "NestJs" : "NodeJs",
            "TypeScript" => t.Contains("next") ? "NextTs" : "NestTs",
            _            => t.Contains("blazor") ? "BlazorServer" : t.Contains("minimal") ? "MinimalApi" : t.Contains("mvc") ? "AspNetCoreMVC" : "AspNetCoreWebApi",
        };

        var db = t.Contains("mysql")   ? "MySQL"
            : t.Contains("mongo")      ? "MongoDB"
            : t.Contains("redis")      ? "Redis"
            : t.Contains("sqlite")     ? "SQLite"
            : t.Contains("sql server") || t.Contains("sqlserver") || t.Contains("mssql") ? "SqlServer"
            : "PostgreSQL";

        var infra = t.Contains("kubernetes") || t.Contains("k8s") ? "Kubernetes"
            : t.Contains("docker") ? "DockerCompose"
            : "None";

        return new VoiceParseResult(arch, fw, db, infra, null);
    }

    private static string Str(JsonElement r, string key, string fallback)
        => r.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() ?? fallback : fallback;

    private static string? NullableStr(JsonElement r, string key)
        => r.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() : null;
}
