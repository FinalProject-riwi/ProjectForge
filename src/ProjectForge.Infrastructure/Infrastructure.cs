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
        await proc.WaitForExitAsync(ct);

        return new ShellResult(proc.ExitCode, stdout.ToString(), stderr.ToString());
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
        if (existing != null) return existing.CloneUrl;

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

        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"git remote set-url origin {authenticatedUrl} && git push -u origin main\"",
            WorkingDirectory = localPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
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
