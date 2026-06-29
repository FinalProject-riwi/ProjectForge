using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.AI;

/// <summary>
/// Servicio de IA con fallback automático: Anthropic → OpenAI → Gemini.
/// Las sugerencias se cachean en DB para ahorrar tokens.
/// El README usa el modelo más económico disponible.
/// </summary>
public class MultiProviderAiSuggestionService : IAiSuggestionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILibraryRepository _libraryRepo;
    private readonly IDesignPatternRepository _patternRepo;
    private readonly IAiSuggestionCacheRepository _cacheRepo;

    public MultiProviderAiSuggestionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILibraryRepository libraryRepo,
        IDesignPatternRepository patternRepo,
        IAiSuggestionCacheRepository cacheRepo)
    {
        _httpClientFactory = httpClientFactory;
        _config = configuration;
        _libraryRepo = libraryRepo;
        _patternRepo = patternRepo;
        _cacheRepo = cacheRepo;
    }

    public async Task<AiSuggestionResult> SuggestAsync(WizardSuggestionRequest request)
    {
        // ── 1. Cache-first: no gastar tokens si ya existe resultado ────────────
        var cacheKey = BuildCacheKey(request);
        AiSuggestionCache? cached = null;
        try
        {
            cached = await _cacheRepo.GetByCacheKeyAsync(cacheKey);
        }
        catch
        {
            // La caché es opcional: si la tabla aún no existe, seguimos sin ella.
        }

        if (cached != null)
        {
            return new AiSuggestionResult(
                JsonSerializer.Deserialize<List<string>>(cached.PatternsJson) ?? [],
                JsonSerializer.Deserialize<List<string>>(cached.LibrariesJson) ?? [],
                cached.Rationale);
        }

        // ── 2. Llamar a IA solo si no hay caché ───────────────────────────────
        var availableLibs     = await _libraryRepo.GetByArchitectureAndFrameworkAsync(request.Architecture, request.Framework);
        var availablePatterns = await _patternRepo.GetByArchitectureAsync(request.Architecture);

        var libCatalog     = availableLibs.Select(l => $"{l.Name} ({l.PackageName}) - {l.Category}: {l.Description}");
        var patternCatalog = availablePatterns.Select(p => $"{p.Name}: {p.Description}");

        var prompt   = BuildSuggestionPrompt(request, libCatalog, patternCatalog);
        var response = await CallWithFallbackAsync(prompt);
        var result   = ParseSuggestionResponse(response);

        // ── 3. Guardar en caché 30 días ───────────────────────────────────────
        try
        {
            await _cacheRepo.AddAsync(new AiSuggestionCache
            {
                CacheKey      = cacheKey,
                PatternsJson  = JsonSerializer.Serialize(result.SuggestedPatterns),
                LibrariesJson = JsonSerializer.Serialize(result.SuggestedLibraries),
                Rationale     = result.Rationale,
                CreatedAt     = DateTime.UtcNow,
                ExpiresAt     = DateTime.UtcNow.AddDays(30)
            });
        }
        catch { /* race condition: ignorar duplicados */ }

        return result;
    }

    public async Task<string> GenerateReadmeAsync(ReadmeGenerationRequest req)
    {
        // README corto para ahorrar tokens
        var prompt =
            "Genera un README.md profesional y conciso (máx 400 palabras) para un proyecto con estas características:\n\n" +
            $"- Nombre: {req.ProjectName}\n" +
            $"- Descripción: {req.Description}\n" +
            $"- Framework: {req.Framework}\n" +
            $"- Base de datos: {req.Database}\n" +
            $"- Infraestructura: {req.Infrastructure}\n" +
            $"- Patrones: {string.Join(", ", req.DesignPatterns)}\n" +
            $"- Librerías: {string.Join(", ", req.Libraries)}\n\n" +
            "Incluye: descripción, instalación, variables de entorno y comandos básicos.\n" +
            "Responde SOLO con el Markdown del README.";

        return await CallWithFallbackAsync(prompt);
    }

    // ─── Fallback Chain ───────────────────────────────────────────────────────

    private async Task<string> CallWithFallbackAsync(string prompt)
    {
        var providers = new List<(string Name, Func<string, Task<string>> Call)>
        {
            ("Anthropic", CallAnthropicAsync),
            ("OpenAI",    CallOpenAiAsync),
            ("Gemini",    CallGeminiAsync),
        };

        Exception? lastException = null;
        foreach (var (name, call) in providers)
        {
            try
            {
                var result = await call(prompt);
                if (!string.IsNullOrWhiteSpace(result)) return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine($"[AI] Proveedor {name} falló: {ex.Message}. Intentando siguiente...");
            }
        }

        throw new InvalidOperationException(
            "Todos los proveedores de IA fallaron. Último error: " + lastException?.Message, lastException);
    }

    // ─── Anthropic (Haiku = más barato) ──────────────────────────────────────

    private async Task<string> CallAnthropicAsync(string userPrompt)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "PLACEHOLDER")
            throw new InvalidOperationException("Anthropic API key no configurada.");

        var http = _httpClientFactory.CreateClient("Anthropic");
        var body = new
        {
            model      = "claude-haiku-4-5-20251001", // haiku: más barato para sugerencias
            max_tokens = 1024,
            messages   = new[] { new { role = "user", content = userPrompt } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;
    }

    // ─── OpenAI ───────────────────────────────────────────────────────────────

    private async Task<string> CallOpenAiAsync(string userPrompt)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "PLACEHOLDER")
            throw new InvalidOperationException("OpenAI API key no configurada.");

        var http = _httpClientFactory.CreateClient("OpenAI");
        var body = new
        {
            model      = "gpt-4o-mini",
            max_tokens = 1024,
            messages   = new[] { new { role = "user", content = userPrompt } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }

    // ─── Gemini ───────────────────────────────────────────────────────────────

    private async Task<string> CallGeminiAsync(string userPrompt)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "PLACEHOLDER")
            throw new InvalidOperationException("Gemini API key no configurada.");

        var http = _httpClientFactory.CreateClient("Gemini");
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = userPrompt } } } },
            generationConfig = new { maxOutputTokens = 1024, temperature = 0.7 }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string BuildCacheKey(WizardSuggestionRequest req)
    {
        var sortedPatterns = string.Join(",", req.AlreadySelectedPatterns.OrderBy(p => p));
        var raw = $"{req.Architecture}|{req.Framework}|{req.Database}|{req.Infrastructure}|{sortedPatterns}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    }

    private static string BuildSuggestionPrompt(
        WizardSuggestionRequest request,
        IEnumerable<string> libCatalog,
        IEnumerable<string> patternCatalog)
    {
        var jsonExample =
            "{\n" +
            "  \"patterns\": [\"Pattern1\", \"Pattern2\"],\n" +
            "  \"libraries\": [\"lib-package-1\", \"lib-package-2\"],\n" +
            "  \"rationale\": \"Explicación breve.\"\n" +
            "}";

        return
            "Eres un arquitecto de software experto. El usuario está creando un proyecto con:\n" +
            $"- Arquitectura: {request.Architecture}\n" +
            $"- Framework: {request.Framework}\n" +
            $"- Base de datos: {request.Database}\n" +
            $"- Infraestructura: {request.Infrastructure}\n" +
            $"- Patrones ya seleccionados: {string.Join(", ", request.AlreadySelectedPatterns)}\n\n" +
            "Catálogo de patrones:\n" + string.Join("\n", patternCatalog) + "\n\n" +
            "Catálogo de librerías:\n" + string.Join("\n", libCatalog) + "\n\n" +
            "Sugiere 3-5 patrones y 5-8 librerías. Responde ESTRICTAMENTE en JSON (sin markdown):\n" +
            jsonExample;
    }

    private static AiSuggestionResult ParseSuggestionResponse(string raw)
    {
        try
        {
            var clean = raw.Trim().TrimStart('`').TrimEnd('`');
            if (clean.StartsWith("json")) clean = clean[4..].Trim();

            using var doc = JsonDocument.Parse(clean);
            var root = doc.RootElement;

            return new AiSuggestionResult(
                root.GetProperty("patterns").EnumerateArray().Select(e => e.GetString() ?? "").ToList(),
                root.GetProperty("libraries").EnumerateArray().Select(e => e.GetString() ?? "").ToList(),
                root.GetProperty("rationale").GetString() ?? "");
        }
        catch
        {
            return new AiSuggestionResult([], [], "No se pudo parsear la sugerencia de IA.");
        }
    }
}
