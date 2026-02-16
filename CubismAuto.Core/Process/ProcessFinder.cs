using System.Diagnostics;

namespace CubismAuto.Core.Process;

public static class ProcessFinder
{
    private sealed record Candidate(int Pid, int Score, DateTimeOffset StartTimeUtc);

    /// <summary>
    /// CubismEditor5.exe может быть "лаунчером": быстро завершиться и поднять реальный процесс (часто javaw.exe).
    /// Эта функция пытается найти "настоящий" процесс редактора, стартовавший рядом по времени.
    /// </summary>
    public static int ResolveCubismPid(int initialPid, DateTimeOffset launchTimeUtc, string expectedExePath, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        Candidate? bestSeen = null;

        // Ждем до timeout и пытаемся найти лучший кандидат.
        while (DateTimeOffset.UtcNow < deadline)
        {
            var candidate = FindCandidate(launchTimeUtc, expectedExePath);
            if (candidate != null)
            {
                if (bestSeen is null || IsBetter(candidate, bestSeen))
                    bestSeen = candidate;

                // Достаточно уверенный матч - можно вернуть раньше timeout.
                if (candidate.Score >= 70)
                    return candidate.Pid;
            }

            Thread.Sleep(250);
        }

        if (bestSeen != null)
            return bestSeen.Pid;

        // Фолбэк: если initialPid все еще жив.
        if (IsRunning(initialPid))
            return initialPid;

        // Последний фолбэк: отдаем исходный pid, чтобы не ломать сценарий.
        return initialPid;
    }

    private static Candidate? FindCandidate(DateTimeOffset launchTimeUtc, string expectedExePath)
    {
        var min = launchTimeUtc;
        var max = DateTimeOffset.UtcNow.AddSeconds(2);

        System.Diagnostics.Process[] procs;
        try { procs = System.Diagnostics.Process.GetProcesses(); }
        catch { return null; }

        Candidate? best = null;

        foreach (var p in procs)
        {
            try
            {
                var name = p.ProcessName; // без .exe
                if (!LooksLikeCubismProcessName(name))
                    continue;

                // StartTime может бросать
                var st = p.StartTime.ToUniversalTime();
                var stUtc = new DateTimeOffset(st, TimeSpan.Zero);
                if (stUtc < min || stUtc > max)
                    continue;

                var score = ScoreCandidate(p, name, expectedExePath);
                var candidate = new Candidate(p.Id, score, stUtc);
                if (score > 0 && (best is null || IsBetter(candidate, best)))
                    best = candidate;
            }
            catch
            {
                // игнор
            }
            finally
            {
                try { p.Dispose(); } catch { }
            }
        }

        return best;
    }

    private static bool IsBetter(Candidate left, Candidate right)
    {
        if (left.Score != right.Score)
            return left.Score > right.Score;

        return left.StartTimeUtc > right.StartTimeUtc;
    }

    private static bool LooksLikeCubismProcessName(string processName)
    {
        // CubismEditor5 / javaw / java
        return processName.Equals("CubismEditor5", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("CubismEditor5_d3d", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("javaw", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("java", StringComparison.OrdinalIgnoreCase);
    }

    private static int ScoreCandidate(System.Diagnostics.Process p, string processName, string expectedExePath)
    {
        var score = 0;

        if (processName.Equals("CubismEditor5", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("CubismEditor5_d3d", StringComparison.OrdinalIgnoreCase))
            score += 25;
        else if (processName.Equals("javaw", StringComparison.OrdinalIgnoreCase)
                 || processName.Equals("java", StringComparison.OrdinalIgnoreCase))
            score += 10;

        if (LooksLikeCubismByWindowTitle(p))
            score += 30;

        score += ScoreMainModulePath(p, expectedExePath);

        return score;
    }

    private static bool LooksLikeCubismByWindowTitle(System.Diagnostics.Process p)
    {
        try
        {
            var title = p.MainWindowTitle;
            if (string.IsNullOrWhiteSpace(title)) return false;
            return title.Contains("Cubism", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Live2D", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static int ScoreMainModulePath(System.Diagnostics.Process p, string expectedExePath)
    {
        try
        {
            var mm = p.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(mm)) return 0;

            // Реальный процесс может быть javaw.exe, но иногда main module всё-таки CubismEditor5.exe.
            if (Path.GetFullPath(mm).Equals(Path.GetFullPath(expectedExePath), StringComparison.OrdinalIgnoreCase))
                return 50;

            // javaw/java тоже ок, если он из папки Cubism (часто так)
            var expectedDir = Path.GetDirectoryName(Path.GetFullPath(expectedExePath))!;
            var mmDir = Path.GetDirectoryName(Path.GetFullPath(mm))!;
            if (mmDir.StartsWith(expectedDir, StringComparison.OrdinalIgnoreCase))
                return 35;

            if (mm.Contains("live2d", StringComparison.OrdinalIgnoreCase)
                || mm.Contains("cubism", StringComparison.OrdinalIgnoreCase))
                return 20;

            return 0;
        }
        catch
        {
            // Нет прав на MainModule/32-64-bit mismatch и т.п.
            return 0;
        }
    }

    private static bool IsRunning(int pid)
    {
        try
        {
            using var p = System.Diagnostics.Process.GetProcessById(pid);
            return !p.HasExited;
        }
        catch
        {
            return false;
        }
    }
}
