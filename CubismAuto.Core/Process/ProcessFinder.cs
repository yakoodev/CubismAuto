using System.Diagnostics;

namespace CubismAuto.Core.Process;

public static class ProcessFinder
{
    /// <summary>
    /// CubismEditor5.exe может быть "лаунчером": быстро завершиться и поднять реальный процесс (часто javaw.exe).
    /// Эта функция пытается найти "настоящий" процесс редактора, стартовавший рядом по времени.
    /// </summary>
    public static int ResolveCubismPid(int initialPid, DateTimeOffset launchTimeUtc, string expectedExePath, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        // Сначала попробуем жить с initialPid
        if (IsRunning(initialPid))
            return initialPid;

        // Потом ищем "свежие" процессы, которые похожи на Cubism
        while (DateTimeOffset.UtcNow < deadline)
        {
            var candidate = FindCandidate(launchTimeUtc, expectedExePath);
            if (candidate != null)
                return candidate.Value;

            Thread.Sleep(250);
        }

        // Фолбэк: если initialPid вдруг появился (маловероятно)
        return initialPid;
    }

    private static int? FindCandidate(DateTimeOffset launchTimeUtc, string expectedExePath)
    {
        var min = launchTimeUtc.AddSeconds(-2);
        var max = DateTimeOffset.UtcNow.AddSeconds(2);

        System.Diagnostics.Process[] procs;
        try { procs = System.Diagnostics.Process.GetProcesses(); }
        catch { return null; }

        int? bestPid = null;
        DateTimeOffset bestStart = DateTimeOffset.MinValue;

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

                // Пытаемся убедиться, что это реально Cubism
                if (!LooksLikeCubismByWindowTitle(p) && !LooksLikeCubismByMainModule(p, expectedExePath))
                    continue;

                if (stUtc > bestStart)
                {
                    bestStart = stUtc;
                    bestPid = p.Id;
                }
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

        return bestPid;
    }

    private static bool LooksLikeCubismProcessName(string processName)
    {
        // CubismEditor5 / javaw / java
        return processName.Equals("CubismEditor5", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("CubismEditor5_d3d", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("javaw", StringComparison.OrdinalIgnoreCase)
            || processName.Equals("java", StringComparison.OrdinalIgnoreCase);
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

    private static bool LooksLikeCubismByMainModule(System.Diagnostics.Process p, string expectedExePath)
    {
        try
        {
            var mm = p.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(mm)) return false;

            // Реальный процесс может быть javaw.exe, но иногда main module всё-таки CubismEditor5.exe.
            if (Path.GetFullPath(mm).Equals(Path.GetFullPath(expectedExePath), StringComparison.OrdinalIgnoreCase))
                return true;

            // javaw/java тоже ок, если он из папки Cubism (часто так)
            var expectedDir = Path.GetDirectoryName(Path.GetFullPath(expectedExePath))!;
            var mmDir = Path.GetDirectoryName(Path.GetFullPath(mm))!;
            return mmDir.StartsWith(expectedDir, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
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
