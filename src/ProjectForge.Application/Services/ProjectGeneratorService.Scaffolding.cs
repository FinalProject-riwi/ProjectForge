using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    // ─── Paso 2: Scaffold por arquitectura ────────────────────────────────────

    private async Task ScaffoldProjectAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        await EmitLogAsync(project, "Scaffold", $"🏗️  Iniciando scaffold ({cfg.Architecture} / {cfg.Framework})...", ct: ct);

        foreach (var cmd in GetScaffoldCommands(cfg, project.Name, path))
        {
            await EmitLogAsync(project, "Scaffold", $"$ {cmd.Command}", ct: ct);
            var result = await _shell.RunAsync(cmd.Command, cmd.WorkingDir ?? path, ct);

            if (!string.IsNullOrWhiteSpace(result.Stdout))
                await EmitLogAsync(project, "Scaffold", result.Stdout.Trim(), ct: ct);

            if (!result.Success)
            {
                await EmitLogAsync(project, "Scaffold", result.Stderr, isError: true, ct: ct);
                throw new InvalidOperationException($"Scaffold falló: {result.Stderr}");
            }
        }
    }

    private static IEnumerable<(string Command, string? WorkingDir)> GetScaffoldCommands(
        WizardConfig cfg, string projectName, string path)
    {
        var safeName = projectName.Replace(" ", "");

        return cfg.Architecture switch
        {
            ArchitectureType.DotNet => cfg.Framework switch
            {
                FrameworkType.AspNetCoreWebApi => new[]
                {
                    ($"dotnet new webapi -n {safeName} -o {path} --no-https false", (string?)null),
                    ($"dotnet new sln -n {safeName}", path),
                    ($"dotnet sln add {safeName}.csproj", path),
                },
                FrameworkType.AspNetCoreMVC => new[] { ($"dotnet new mvc -n {safeName} -o {path}", (string?)null) },
                FrameworkType.BlazorServer => new[] { ($"dotnet new blazorserver -n {safeName} -o {path}", (string?)null) },
                _ => new[] { ($"dotnet new webapi -n {safeName} -o {path}", (string?)null) }
            },

            // Python: file-based scaffolding handled by ScaffoldPythonFilesInternalAsync
            ArchitectureType.Python => Array.Empty<(string, string?)>(),

            ArchitectureType.JavaScript or ArchitectureType.TypeScript => cfg.Framework switch
            {
                FrameworkType.NodeJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm pkg set 'scripts.start=node src/index.js'", (string?)path),
                },
                FrameworkType.ExpressJs => new[]
                {
                    ($"npm init -y", (string?)path),
                    ($"npm install express", (string?)path),
                    ($"npm pkg set 'scripts.start=node src/server.js'", (string?)path),
                },
                FrameworkType.NestJs => new[] { ($"nest new {safeName} --language JS --package-manager npm --skip-git --directory . --skip-install", (string?)path) },
                FrameworkType.NextJs => new[] { ($"npx create-next-app@latest . --js --app --eslint --src-dir --import-alias=@/* --no-git", (string?)path) },
                FrameworkType.NestTs => new[] { ($"nest new {safeName} --language TS --package-manager npm --skip-git --directory . --skip-install", (string?)path) },
                FrameworkType.NextTs => new[] { ($"npx create-next-app@latest . --ts --app --eslint --src-dir --import-alias=@/* --no-git", (string?)path) },
                _ => new[] { ($"npm init -y", (string?)path) }
            },

            // Java: file-based scaffolding handled by ScaffoldJavaFilesInternalAsync
            ArchitectureType.Java => Array.Empty<(string, string?)>(),

            // PHP: file-based scaffolding handled by ScaffoldPhpBaseFilesAsync
            ArchitectureType.Php => Array.Empty<(string, string?)>(),

            _ => Array.Empty<(string, string?)>()
        };
    }

    // ─── Paso 2b: Base files de JS/TS ─────────────────────────────────────────

    private async Task ScaffoldJavaScriptBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture is not (ArchitectureType.JavaScript or ArchitectureType.TypeScript))
            return;

        var src = Path.Combine(path, "src");
        Directory.CreateDirectory(src);

        switch (cfg.Framework)
        {
            case FrameworkType.NodeJs:
                await File.WriteAllTextAsync(Path.Combine(src, "index.js"), "console.log('ProjectForge');\n", ct);
                break;
            case FrameworkType.ExpressJs:
                await File.WriteAllTextAsync(Path.Combine(src, "server.js"), """
const express = require('express');
const app = express();
app.get('/', (_, res) => res.json({ ok: true }));
app.listen(process.env.PORT || 3000);
""", ct);
                break;
            case FrameworkType.NestJs:
            case FrameworkType.NestTs:
                await File.WriteAllTextAsync(Path.Combine(src, "main.ts"), """
import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  await app.listen(process.env.PORT || 3000);
}
bootstrap();
""", ct);
                break;
            case FrameworkType.NextJs:
            case FrameworkType.NextTs:
                Directory.CreateDirectory(Path.Combine(src, "app"));
                await File.WriteAllTextAsync(Path.Combine(src, "app", "page.js"), "export default function Page(){ return null; }\n", ct);
                break;
        }
    }

    private async Task ScaffoldPhpBaseFilesAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        if (cfg.Architecture != ArchitectureType.Php || cfg.Framework != FrameworkType.Symfony)
            return;

        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Any(p => NormalizePatternToken(p) == "microservices"))
            return;

        await EmitLogAsync(project, "Scaffold", "🏗️  Generando base Symfony...", ct: ct);

        foreach (var (relativePath, content) in BuildPhpDddPatternFiles(FrameworkType.Symfony))
        {
            var filePath = Path.Combine(path, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, content, ct);
        }

        await EmitLogAsync(project, "Scaffold", "✅ Base Symfony generada", ct: ct);
    }

    // ─── Paso 3: Patrones de diseño ──────────────────────────────────────────

    private async Task ScaffoldDesignPatternsAsync(Project project, WizardConfig cfg, string path, CancellationToken ct)
    {
        var selectedPatterns = JsonSerializer.Deserialize<List<string>>(cfg.DesignPatternsJson) ?? [];
        if (selectedPatterns.Count == 0)
        {
            await EmitLogAsync(project, "Patterns", "ℹ️  Sin patrones seleccionados", ct: ct);
            return;
        }

        foreach (var pattern in selectedPatterns)
        {
            var files = cfg.Architecture switch
            {
                ArchitectureType.Php => BuildPhpPatternFiles(cfg, pattern),
                ArchitectureType.JavaScript or ArchitectureType.TypeScript => BuildJavaScriptPatternFiles(cfg.Framework, pattern),
                ArchitectureType.DotNet => BuildDotNetPatternFiles(cfg.Framework, pattern),
                ArchitectureType.Java => BuildJavaPatternFiles(cfg.Framework, pattern),
                ArchitectureType.Python => BuildPythonPatternFiles(cfg.Framework, pattern),
                _ => Array.Empty<(string RelativePath, string Content)>()
            };

            if (files.Count == 0)
            {
                await EmitLogAsync(project, "Patterns", $"ℹ️  Patrón no soportado: {pattern}", ct: ct);
                continue;
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                NormalizePatternToken(pattern) == "microservices")
            {
                if (cfg.Framework == FrameworkType.Laravel)
                {
                    await ScaffoldLaravelMicroservicesWorkspaceAsync(path, ct);
                }
                else if (cfg.Framework == FrameworkType.Symfony)
                {
                    await ScaffoldSymfonyMicroservicesWorkspaceAsync(path, ct);
                }
            }

            foreach (var (relativePath, content) in files)
            {
                var filePath = Path.Combine(path, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllTextAsync(filePath, content, ct);
            }

            await EmitLogAsync(project, "Patterns", $"✅ Patrón generado: {pattern}", ct: ct);

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "hexagonalarchitecture")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\HexagonalServiceProvider::class", ct);
            }

            if (cfg.Architecture == ArchitectureType.Php &&
                cfg.Framework == FrameworkType.Laravel &&
                NormalizePatternToken(pattern) == "repository")
            {
                await EnsureLaravelProviderRegistrationAsync(path, "App\\Providers\\RepositoryServiceProvider::class", ct);
            }
        }
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPhpPatternFiles(
        WizardConfig cfg,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "domaindrivendesign" => BuildPhpDddPatternFiles(cfg.Framework),
            "cleanarchitecture" => BuildPhpCleanArchitecturePatternFiles(cfg.Framework),
            "hexagonalarchitecture" => BuildPhpHexagonalPatternFiles(cfg.Framework),
            "repository" => BuildPhpRepositoryPatternFiles(cfg.Framework),
            "cqrs" => BuildPhpCqrsPatternFiles(cfg.Framework),
            "eventsourcing" => BuildPhpEventSourcingPatternFiles(cfg.Framework),
            "mediator" => BuildPhpMediatorPatternFiles(cfg.Framework),
            "saga" => BuildPhpSagaPatternFiles(cfg.Framework),
            "microservices" => BuildPhpMicroservicesPatternFiles(cfg.Framework, cfg.Database),
            _ => Array.Empty<(string, string)>()
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptPatternFiles(
        FrameworkType framework,
        string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "cqrs" => BuildJavaScriptCqrsPatternFiles(framework),
            "eventsourcing" => BuildJavaScriptEventSourcingPatternFiles(framework),
            "mediator" => BuildJavaScriptMediatorPatternFiles(framework),
            _ => Array.Empty<(string, string)>()
        };
    }

    private static async Task EnsureLaravelProviderRegistrationAsync(string path, string providerEntry, CancellationToken ct)
    {
        var providersFile = Path.Combine(path, "bootstrap", "providers.php");
        if (!File.Exists(providersFile))
            return;

        var content = await File.ReadAllTextAsync(providersFile, ct);
        if (content.Contains(providerEntry, StringComparison.Ordinal))
            return;

        var insertMarker = "return [";
        var idx = content.IndexOf(insertMarker, StringComparison.Ordinal);
        if (idx < 0)
            return;

        var insertAt = content.IndexOf('\n', idx);
        if (insertAt < 0)
            return;

        var updated = content.Insert(insertAt + 1, $"    {providerEntry},\n");
        await File.WriteAllTextAsync(providersFile, updated, ct);
    }

    private static async Task ScaffoldLaravelMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "laravel/laravel", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios: {result.Stderr}");
        }
    }

    private static async Task ScaffoldSymfonyMicroservicesWorkspaceAsync(string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(path, "services"));

        var commands = new[]
        {
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "projects"), "--no-interaction" },
            new[] { "create-project", "symfony/skeleton", Path.Combine(path, "services", "notifications"), "--no-interaction" }
        };

        foreach (var command in commands)
        {
            var result = await RunComposerCommandAsync(command, path, ct);
            if (!result.Success)
                throw new InvalidOperationException($"No se pudo crear el workspace de microservicios de Symfony: {result.Stderr}");
        }
    }

    private static async Task<ShellResult> RunComposerCommandAsync(IEnumerable<string> arguments, string workingDirectory, CancellationToken ct)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "composer",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        return new ShellResult(process.ExitCode, stdout, stderr);
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptCqrsPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptEventSourcingPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaScriptMediatorPatternFiles(FrameworkType framework)
        => Array.Empty<(string, string)>();

    private static string NormalizePatternToken(string value)
        => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
