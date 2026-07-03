using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Guards against a hung CLI tool blocking a generation request forever — same rationale and
// value as ShellExecutor.DefaultTimeout in ProjectForge.Infrastructure, which this worker
// replaces for whichever language toolchain its Dockerfile installs.
var defaultTimeout = TimeSpan.FromMinutes(10);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/execute", async (ExecuteRequest request, CancellationToken ct) =>
{
    using var proc = new Process();
    proc.StartInfo = BuildStartInfo(request.Command, request.WorkingDirectory);

    var stdout = new StringBuilder();
    var stderr = new StringBuilder();
    proc.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
    proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

    proc.Start();
    proc.BeginOutputReadLine();
    proc.BeginErrorReadLine();

    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    timeoutCts.CancelAfter(defaultTimeout);

    try
    {
        await proc.WaitForExitAsync(timeoutCts.Token);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        TryKill(proc);
        return Results.Ok(new ExecuteResponse(-1, stdout.ToString(),
            $"El comando superó el tiempo límite de {defaultTimeout.TotalMinutes:0} minutos y fue cancelado: {request.Command}"));
    }

    return Results.Ok(new ExecuteResponse(proc.ExitCode, stdout.ToString(), stderr.ToString()));
});

app.Run();

static void TryKill(Process proc)
{
    try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { /* best effort */ }
}

// Every worker container runs Linux (see docker/Dockerfile.Worker.*), so there's no cmd.exe
// branch here the way ProjectForge.Infrastructure's ShellExecutor has for local Windows dev.
static ProcessStartInfo BuildStartInfo(string command, string workingDirectory) => new()
{
    FileName = "/bin/bash",
    Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
    WorkingDirectory = workingDirectory,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};

record ExecuteRequest(string Command, string WorkingDirectory);
record ExecuteResponse(int ExitCode, string Stdout, string Stderr);
