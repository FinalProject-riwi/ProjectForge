using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using ProjectForge.Core.Interfaces;

namespace ProjectForge.Infrastructure;

// ─── Shell Executor ───────────────────────────────────────────────────────────

public class ShellExecutor : IShellExecutor
{
    // Guards against a hung CLI tool (mvn/pip/npm/composer) blocking generation forever.
    // The wizard's StartGeneration action never passes a CancellationToken to GenerateAsync,
    // so without this the whole background generation task could hang indefinitely.
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);

    private readonly ILogger<ShellExecutor> _logger;

    public ShellExecutor(ILogger<ShellExecutor> logger) => _logger = logger;

    public async Task<ShellResult> RunAsync(string command, string workingDirectory, CancellationToken ct = default)
    {
        _logger.LogInformation("Shell: {Command} in {Dir}", command, workingDirectory);

        using var proc = new Process();
        proc.StartInfo = BuildStartInfo(command, workingDirectory);

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(DefaultTimeout);

        try
        {
            await proc.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            TryKill(proc);
            return new ShellResult(-1, stdout.ToString(),
                $"El comando superó el tiempo límite de {DefaultTimeout.TotalMinutes:0} minutos y fue cancelado: {command}");
        }

        return new ShellResult(proc.ExitCode, stdout.ToString(), stderr.ToString());
    }

    private static void TryKill(Process proc)
    {
        try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { /* best effort */ }
    }

    public async IAsyncEnumerable<string> StreamAsync(string command, string workingDirectory,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var proc = new Process();
        proc.StartInfo = BuildStartInfo(command, workingDirectory);
        proc.Start();

        string? line;
        while ((line = await proc.StandardOutput.ReadLineAsync(ct)) != null)
            yield return line;

        await proc.WaitForExitAsync(ct);
    }

    private static ProcessStartInfo BuildStartInfo(string command, string workingDirectory)
    {
        var isWindows = OperatingSystem.IsWindows();
        return new ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "/bin/bash",
            Arguments = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }
}

// ─── GitHub Service ───────────────────────────────────────────────────────────

public class GitHubService : IGitHubService
{
    public async Task<string> CreateRepositoryAsync(
        string accessToken, string repoName, string description, bool isPrivate)
    {
        var client = new GitHubClient(new ProductHeaderValue("ProjectForge"))
        {
            Credentials = new Credentials(accessToken)
        };

        var existing = await TryGetRepoAsync(client, repoName);
        if (existing != null)
        {
            await client.Repository.Edit(existing.Id, new RepositoryUpdate
            {
                Name = repoName,
                Description = description,
                Private = isPrivate
            });

            return existing.CloneUrl;
        }

        var repo = await client.Repository.Create(new NewRepository(repoName)
        {
            Description = description,
            Private = isPrivate,
            AutoInit = false
        });

        return repo.CloneUrl;
    }

    public async Task PushToRepositoryAsync(string localPath, string repoUrl, string accessToken)
    {
        // Inject token into URL for authenticated push
        var authenticatedUrl = repoUrl.Replace("https://", $"https://{accessToken}@");

        // Invoke git directly (no shell) instead of hardcoding /bin/bash — the previous
        // implementation crashed outright on Windows (e.g. running "dotnet run" locally
        // per the README's local-dev instructions, without the Linux Docker image).
        // Using ArgumentList also avoids interpolating the token-bearing URL into a shell
        // command string, so a repo URL/token containing shell metacharacters can't break out.
        await RunGitAsync(localPath, "remote", "set-url", "origin", authenticatedUrl);
        await RunGitAsync(localPath, "push", "-u", "origin", "main");
    }

    private static async Task RunGitAsync(string workingDirectory, params string[] arguments)
    {
        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in arguments)
            proc.StartInfo.ArgumentList.Add(arg);

        proc.Start();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            var err = await proc.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Git push falló: {err}");
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("ProjectForge"))
            {
                Credentials = new Credentials(accessToken)
            };
            var user = await client.User.Current();
            return user != null;
        }
        catch { return false; }
    }

    private static async Task<Repository?> TryGetRepoAsync(GitHubClient client, string name)
    {
        try
        {
            var user = await client.User.Current();
            return await client.Repository.Get(user.Login, name);
        }
        catch { return null; }
    }
}

// ─── Encryption Service (AES-256) ────────────────────────────────────────────

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        var secret = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key not configured");
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        _iv = MD5.HashData(Encoding.UTF8.GetBytes(secret));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key; aes.IV = _iv;
        using var enc = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(enc.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key; aes.IV = _iv;
        using var dec = aes.CreateDecryptor();
        var bytes = Convert.FromBase64String(cipherText);
        return Encoding.UTF8.GetString(dec.TransformFinalBlock(bytes, 0, bytes.Length));
    }
}
