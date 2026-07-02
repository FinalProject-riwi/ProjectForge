using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ProjectForge.Application.AI;

public record VoiceParseResult(
    string Architecture,
    string Framework,
    string Database,
    string Infrastructure,
    string? ProjectName,
    bool InScope = true,
    string? OutOfScopeReply = null
);

public interface IVoiceParsingService
{
    Task<VoiceParseResult?> ParseAsync(string transcript);
}

/// <summary>
/// Parses natural-language voice transcripts into structured WizardConfig using Groq (Llama 3.3-70B).
/// Detects out-of-scope requests and returns a warm Spanish reply.
/// Falls back to keyword matching when no API key is configured.
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
        Eres la asistente de voz de ProjectForge, una app que genera proyectos de software.
        Tu única función es ayudar a crear proyectos. NO puedes enseñar, responder preguntas generales, ni hacer nada fuera de crear proyectos.

        Analiza el mensaje del usuario (puede ser en español coloquial, informal o poco estructurado) y determina:
        1. ¿Está pidiendo crear un proyecto de software? → inScope: true
        2. ¿Está hablando de otra cosa (preguntas, saludos, conceptos, otra app, etc.)? → inScope: false

        Si inScope: false, responde con un mensaje cálido y breve en español que explique que solo puedes ayudar a crear proyectos con ProjectForge, y pregunta si quiere crear uno. Máximo 2 oraciones.

        Si inScope: true, extrae la configuración del proyecto de lo que dijo el usuario, siendo MUY inteligente con el lenguaje natural:
        - "una tienda" → Django o ASP.NET MVC con PostgreSQL
        - "una API" o "backend" → AspNetCoreWebApi o FastAPI con PostgreSQL
        - "un blog" → Django o Laravel con MySQL
        - "algo con node para un chat" → NodeJs o NestJs con MongoDB
        - "microservicios" → SpringBoot o NestJs con Kubernetes
        - Si menciona el nombre del proyecto ("se llamará X", "llamado X", "el nombre es X") → extráelo
        - Usa el contexto para inferir lo que no dijo explícitamente
        - Si no hay suficiente información, usa defaults razonables (no lo más difícil)

        Valores disponibles (usa EXACTAMENTE estos):
          architecture : DotNet | Java | Python | Php | JavaScript | TypeScript
          framework (DotNet)     : AspNetCoreWebApi | AspNetCoreMVC | BlazorServer | BlazorWasm | MinimalApi
          framework (Java)       : SpringBoot | Quarkus | Micronaut
          framework (Python)     : FastAPI | Django | Flask
          framework (Php)        : Laravel | Symfony
          framework (JavaScript) : NodeJs | ExpressJs | NestJs | NextJs
          framework (TypeScript) : NestTs | NextTs
          database     : PostgreSQL | MySQL | SqlServer | MongoDB | Redis | SQLite
          infrastructure : None | DockerCompose | Kubernetes

        Defaults si no se menciona: architecture=DotNet, framework=AspNetCoreWebApi, database=PostgreSQL, infrastructure=None

        Responde ÚNICAMENTE con JSON válido en una sola línea, sin markdown ni explicaciones:

        Si inScope true:
        {"inScope":true,"architecture":"...","framework":"...","database":"...","infrastructure":"...","projectName":null,"outOfScopeReply":null}

        Si inScope false:
        {"inScope":false,"architecture":"DotNet","framework":"AspNetCoreWebApi","database":"PostgreSQL","infrastructure":"None","projectName":null,"outOfScopeReply":"Solo puedo ayudarte a crear proyectos de software con ProjectForge. ¿Quieres que creemos uno juntos?"}
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
                max_tokens  = 200,
                temperature = 0.2,
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
        // Strip markdown code fences if Groq wraps response
        if (clean.StartsWith("```"))
        {
            var s = clean.IndexOf('{');
            var e = clean.LastIndexOf('}');
            if (s >= 0 && e > s) clean = clean[s..(e + 1)];
        }
        // Find JSON object bounds
        var start = clean.IndexOf('{');
        var end   = clean.LastIndexOf('}');
        if (start >= 0 && end > start) clean = clean[start..(end + 1)];

        try
        {
            using var doc = JsonDocument.Parse(clean);
            var r = doc.RootElement;

            var inScope = r.TryGetProperty("inScope", out var p) && p.ValueKind == JsonValueKind.True;
            if (!inScope)
            {
                var reply = NullableStr(r, "outOfScopeReply")
                    ?? "Solo estoy equipada para crear proyectos con ProjectForge. ¿Creamos uno?";
                return new VoiceParseResult("DotNet", "AspNetCoreWebApi", "PostgreSQL", "None", null,
                    InScope: false, OutOfScopeReply: reply);
            }

            return new VoiceParseResult(
                Str(r, "architecture",   "DotNet"),
                Str(r, "framework",      "AspNetCoreWebApi"),
                Str(r, "database",       "PostgreSQL"),
                Str(r, "infrastructure", "None"),
                NullableStr(r, "projectName"),
                InScope: true
            );
        }
        catch { return null; }
    }

    // Keyword fallback — no Groq key required
    private static VoiceParseResult FallbackParse(string text)
    {
        var t = text.ToLowerInvariant();

        // Out-of-scope heuristic: very short or common non-project phrases
        var nonProjectPhrases = new[] { "hola", "qué tal", "cómo estás", "gracias", "adiós", "bye", "help" };
        if (t.Split(' ').Length <= 2 && nonProjectPhrases.Any(p => t.Contains(p)))
        {
            return new VoiceParseResult("DotNet", "AspNetCoreWebApi", "PostgreSQL", "None", null,
                InScope: false, OutOfScopeReply: "Solo puedo ayudarte a crear proyectos de software con ProjectForge. ¿Creamos uno juntos?");
        }

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
            : t.Contains("sql server") || t.Contains("mssql") ? "SqlServer"
            : "PostgreSQL";

        var infra = t.Contains("kubernetes") || t.Contains("k8s") ? "Kubernetes"
            : t.Contains("docker") ? "DockerCompose"
            : "None";

        return new VoiceParseResult(arch, fw, db, infra, null, InScope: true);
    }

    private static string Str(JsonElement r, string key, string fallback)
        => r.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() ?? fallback : fallback;

    private static string? NullableStr(JsonElement r, string key)
        => r.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() : null;
}