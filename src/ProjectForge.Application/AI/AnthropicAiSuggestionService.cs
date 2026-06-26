using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.AI;

/// <summary>
/// Usa la API de Anthropic Claude para sugerir patrones y librerías.
/// Las sugerencias se cachean en DB para ahorrar tokens.
/// </summary>
public class AnthropicAiSuggestionService : IAiSuggestionService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILibraryRepository _libraryRepo;
    private readonly IDesignPatternRepository _patternRepo;
    private readonly IAiSuggestionCacheRepository _cacheRepo;

    public AnthropicAiSuggestionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILibraryRepository libraryRepo,
        IDesignPatternRepository patternRepo,
        IAiSuggestionCacheRepository cacheRepo)
    {
        _http = httpClientFactory.CreateClient("Anthropic");
        _apiKey = configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic:ApiKey not configured");
        _libraryRepo = libraryRepo;
        _patternRepo = patternRepo;
        _cacheRepo = cacheRepo;
    }

    public async Task<AiSuggestionResult> SuggestAsync(WizardSuggestionRequest request)
    {
        // ── 1. Intentar leer desde caché ──────────────────────────────────────
        var cacheKey = BuildCacheKey(request);
        AiSuggestionCache? cached = null;
        try
        {
            cached = await _cacheRepo.GetByCacheKeyAsync(cacheKey);
        }
        catch
        {
            // La tabla de caché puede no existir aún en bases antiguas.
        }

        if (cached != null)
        {
            var patterns = JsonSerializer.Deserialize<List<string>>(cached.PatternsJson) ?? [];
            var libraries = JsonSerializer.Deserialize<List<string>>(cached.LibrariesJson) ?? [];
            return new AiSuggestionResult(patterns, libraries, cached.Rationale);
        }

        // ── 2. Llamar a la IA solo si no hay caché ────────────────────────────
        var availableLibs = await _libraryRepo.GetByArchitectureAndFrameworkAsync(request.Architecture, request.Framework);
        var availablePatterns = await _patternRepo.GetByArchitectureAsync(request.Architecture);

        var libCatalog = availableLibs.Select(l => $"{l.Name} ({l.PackageName}) - {l.Category}: {l.Description}");
        var patternCatalog = availablePatterns.Select(p => $"{p.Name}: {p.Description}");

        var prompt = BuildSuggestionPrompt(request, libCatalog, patternCatalog);
        var response = await CallAnthropicAsync(prompt);
        var result = ParseSuggestionResponse(response);

        // ── 3. Guardar en caché (30 días) ─────────────────────────────────────
        try
        {
            await _cacheRepo.AddAsync(new AiSuggestionCache
            {
                CacheKey    = cacheKey,
                PatternsJson  = JsonSerializer.Serialize(result.SuggestedPatterns),
                LibrariesJson = JsonSerializer.Serialize(result.SuggestedLibraries),
                Rationale   = result.Rationale,
                CreatedAt   = DateTime.UtcNow,
                ExpiresAt   = DateTime.UtcNow.AddDays(30)
            });
        }
        catch { /* ignorar error de caché duplicado (race condition) */ }

        return result;
    }

    public async Task<string> GenerateReadmeAsync(ReadmeGenerationRequest req)
    {
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
            "Responde SOLO con el Markdown del README, sin explicaciones adicionales.";

        return await CallAnthropicAsync(prompt);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Genera un hash SHA256 estable para la combinación de parámetros.</summary>
    private static string BuildCacheKey(WizardSuggestionRequest req)
    {
        var sortedPatterns = string.Join(",", req.AlreadySelectedPatterns.OrderBy(p => p));
        var raw = $"{req.Architecture}|{req.Framework}|{req.Database}|{req.Infrastructure}|{sortedPatterns}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string BuildSuggestionPrompt(
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
            "Catálogo de patrones disponibles:\n" +
            string.Join("\n", patternCatalog) + "\n\n" +
            "Catálogo de librerías disponibles:\n" +
            string.Join("\n", libCatalog) + "\n\n" +
            "Sugiere 3-5 patrones y 5-8 librerías. Responde ESTRICTAMENTE en JSON (sin markdown):\n" +
            jsonExample;
    }

    private async Task<string> CallAnthropicAsync(string userPrompt)
    {
        var body = new
        {
            model = "claude-haiku-4-5-20251001", // haiku: más barato para sugerencias
            max_tokens = 1024,
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    private static AiSuggestionResult ParseSuggestionResponse(string raw)
    {
        try
        {
            var clean = raw.Trim().TrimStart('`').TrimEnd('`');
            if (clean.StartsWith("json")) clean = clean[4..].Trim();

            using var doc = JsonDocument.Parse(clean);
            var root = doc.RootElement;

            var patterns = root.GetProperty("patterns").EnumerateArray()
                .Select(e => e.GetString() ?? "").ToList();
            var libraries = root.GetProperty("libraries").EnumerateArray()
                .Select(e => e.GetString() ?? "").ToList();
            var rationale = root.GetProperty("rationale").GetString() ?? "";

            return new AiSuggestionResult(patterns, libraries, rationale);
        }
        catch
        {
            return new AiSuggestionResult([], [], "No se pudo parsear la sugerencia de IA.");
        }
    }
}
