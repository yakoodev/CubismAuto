using System.Diagnostics;

namespace CubismAuto.Core.Process;

public sealed record LaunchedProcess(
    int Pid,
    string ExePath,
    string Arguments,
    DateTimeOffset StartTimeUtc
);

public static class ProcessLauncher
{
    public static LaunchedProcess Start(string exePath, string? arguments = null, string? workingDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(exePath))
            throw new ArgumentException("exePath is empty.", nameof(exePath));
        if (!File.Exists(exePath))
            throw new FileNotFoundException("Exe not found.", exePath);

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments ?? "",
            UseShellExecute = false,
            CreateNoWindow = false,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? "" : workingDirectory!
        };

        var p = System.Diagnostics.Process.Start(psi)
                ?? throw new InvalidOperationException($"Не удалось запустить процесс: {exePath}");

        // Дадим процессу чуть-чуть подышать
        Thread.Sleep(800);

        return new LaunchedProcess(
            Pid: p.Id,
            ExePath: exePath,
            Arguments: psi.Arguments,
            StartTimeUtc: DateTimeOffset.UtcNow
        );
    }

    public static bool TryStop(int pid, TimeSpan timeout, bool killTreeIfNeeded = true)
    {
        try
        {
            using var p = System.Diagnostics.Process.GetProcessById(pid);
            if (p.HasExited) return true;

            try { p.CloseMainWindow(); } catch { /* ignore */ }

            if (p.WaitForExit((int)timeout.TotalMilliseconds))
                return true;

            if (!killTreeIfNeeded)
                return false;

            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return p.WaitForExit((int)timeout.TotalMilliseconds);
        }
        catch
        {
            return false;
        }
    }
}
