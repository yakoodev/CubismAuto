using System.Diagnostics;

namespace CubismAuto.Core.Process;

public sealed record ProcessModuleInfo(string FileName, string? ModuleName);

public sealed record ProcessInfo(
    int Pid,
    string ProcessName,
    string? MainModuleFileName,
    DateTimeOffset? StartTimeUtc,
    IReadOnlyList<ProcessModuleInfo> Modules
);

public static class ProcessInspector
{
    public static ProcessInfo Inspect(int pid, int maxModules = 20000)
    {
        using var p = System.Diagnostics.Process.GetProcessById(pid);

        string? main = null;
        DateTimeOffset? start = null;

        try
        {
            main = p.MainModule?.FileName;
            start = p.StartTime.ToUniversalTime();
        }
        catch
        {
            // Доступ к MainModule/StartTime иногда требует прав, не падаем.
        }

        var modules = new List<ProcessModuleInfo>();
        try
        {
            foreach (ProcessModule m in p.Modules)
            {
                if (modules.Count >= maxModules) break;
                modules.Add(new ProcessModuleInfo(m.FileName, m.ModuleName));
            }
        }
        catch
        {
            // Может не дать прочитать.
        }

        return new ProcessInfo(
            Pid: pid,
            ProcessName: p.ProcessName,
            MainModuleFileName: main,
            StartTimeUtc: start,
            Modules: modules
        );
    }
}
