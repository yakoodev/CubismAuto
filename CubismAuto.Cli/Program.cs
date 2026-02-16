using CubismAuto.Core.Process;
using CubismAuto.Core.Reporting;
using CubismAuto.Core.Snapshots;

static string Arg(string[] args, string key, string? @default = null)
{
    // формат: --key value  или --key=value
    for (int i = 0; i < args.Length; i++)
    {
        var a = args[i];
        if (a.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 < args.Length) return args[i + 1];
            return @default ?? "";
        }

        if (a.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
            return a[(key.Length + 1)..];
    }
    return @default ?? "";
}

static bool Has(string[] args, string key)
    => args.Any(a => a.Equals(key, StringComparison.OrdinalIgnoreCase));

static List<string> ArgsMany(string[] args, string key)
{
    // позволяет: --path A --path B  или --path=A  или --paths A;B;C
    var res = new List<string>();
    for (int i = 0; i < args.Length; i++)
    {
        var a = args[i];
        if (a.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 < args.Length) res.Add(args[i + 1]);
            continue;
        }

        if (a.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
        {
            res.Add(a[(key.Length + 1)..]);
        }
    }
    return res;
}

static void PrintHelp()
{
    Console.WriteLine("CubismAuto CLI (net10.0)");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project CubismAuto.Cli -- [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine(@"  --exe <path>         Path to CubismEditor5.exe (default: C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe)");
    Console.WriteLine("  --projects <path>    Root folder with test projects (you create them manually)");
    Console.WriteLine("  --path <path>        Additional snapshot root (repeatable)");
    Console.WriteLine(@"  --cmo3 <file>        Path to a .cmo3 project file (default: C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3)");
    Console.WriteLine("  --paths <a;b;c>      Additional snapshot roots (semicolon-separated)");
    Console.WriteLine("  --artifacts <path>   Where to store artifacts (default: ./artifacts/<timestamp>)");
    Console.WriteLine("  --wait               Wait for Enter between snapshots (default: true)");
    Console.WriteLine("  --no-wait            Don't wait (takes after snapshot immediately)");
    Console.WriteLine("  --stop               Try to close Cubism at the end");
    Console.WriteLine("  --help               Show this help");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine(@"  dotnet run --project .\CubismAuto.Cli -- --exe ""C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe"" --projects ""C:\Users\Yakoo\source\repos\CubismAuto\_projects"" --stop");
}

// In top-level programs .NET already provides an implicit `args` parameter.
// Use a different variable name to avoid CS0136.
var cliArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
if (Has(cliArgs, "--help") || Has(cliArgs, "-h"))
{
    PrintHelp();
    return;
}

// defaults as requested
var defaultExe = @"C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe";
var defaultProjects = @"./_projects";
var defaultCmo3 = @"C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3";

var cubismExe = Arg(cliArgs, "--exe", defaultExe);
var projectsRoot = Arg(cliArgs, "--projects", defaultProjects);

// Extra snapshot roots
var extraRoots = new List<string>();
extraRoots.AddRange(ArgsMany(cliArgs, "--path"));

var pathsInline = Arg(cliArgs, "--paths", "");
if (!string.IsNullOrWhiteSpace(pathsInline))
{
    extraRoots.AddRange(pathsInline.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
var cmo3File = Arg(cliArgs, "--cmo3", defaultCmo3);
string? cmo3Dir = null;
if (!string.IsNullOrWhiteSpace(cmo3File))
{
    try
    {
        var full = Path.GetFullPath(cmo3File);
        if (File.Exists(full))
            cmo3Dir = Path.GetDirectoryName(full);
        else
            Console.WriteLine($"[warn] --cmo3 указан, но файл не найден: {full}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[warn] не смог обработать --cmo3: {ex.Message}");
    }
}


var artifactsRoot = Arg(cliArgs, "--artifacts", "");
if (string.IsNullOrWhiteSpace(artifactsRoot))
{
    artifactsRoot = Path.Combine(Environment.CurrentDirectory, "artifacts", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
}
Directory.CreateDirectory(artifactsRoot);
Directory.CreateDirectory(projectsRoot);

bool wait = true;
if (Has(cliArgs, "--no-wait")) wait = false;
if (Has(cliArgs, "--wait")) wait = true;

Console.WriteLine("=== CubismAuto CLI ===");
Console.WriteLine($"Exe:        {cubismExe}");
Console.WriteLine($"Projects:   {projectsRoot}");
Console.WriteLine($"Cmo3:       {cmo3File}");
Console.WriteLine($"Artifacts:  {artifactsRoot}");
if (extraRoots.Count > 0)
    Console.WriteLine($"ExtraRoots: {string.Join(" | ", extraRoots)}");
Console.WriteLine();

// include filter: avoid hashing common junk
bool IncludeFile(string path)
{
    var p = path.Replace('/', '\\').ToLowerInvariant();
    if (p.Contains(@"\.git\")) return false;
    if (p.Contains(@"\library\")) return false;
    if (p.Contains(@"\cache\")) return false;
    if (p.Contains(@"\logs\old\")) return false;
    return true;
}

bool IsSamePath(string left, string right)
{
    var l = Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var r = Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    return string.Equals(l, r, StringComparison.OrdinalIgnoreCase);
}

bool IsCubismTempPath(string path)
    => path.Contains("live2d", StringComparison.OrdinalIgnoreCase)
       || path.Contains("cubism", StringComparison.OrdinalIgnoreCase);

// snapshot roots: projectsRoot + some sensible defaults (AppData) + user extras
var roots = new List<string>();
roots.Add(projectsRoot);
if (!string.IsNullOrWhiteSpace(cmo3Dir) && Directory.Exists(cmo3Dir)) roots.Add(cmo3Dir);

var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var temp = Path.GetTempPath();

roots.Add(Path.Combine(appData, "Live2D"));
roots.Add(Path.Combine(localAppData, "Live2D"));

// temp can be huge; we snapshot it, but only keep Live2D/Cubism-related files via filter
roots.Add(temp);

foreach (var r in extraRoots)
    roots.Add(r);

// normalize + dedupe + keep existing only
roots = roots
    .Select(p => Path.GetFullPath(p))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .Where(Directory.Exists)
    .ToList();

Console.WriteLine("Taking BEFORE snapshots...");
var beforeSnapshots = new List<DirectorySnapshot>();
for (int i = 0; i < roots.Count; i++)
{
    var root = roots[i];
    Console.WriteLine($"  [{i+1}/{roots.Count}] {root}");

    Func<string, bool> filter = IncludeFile;
    if (IsSamePath(root, temp))
    {
        filter = p => IncludeFile(p) && IsCubismTempPath(p);
    }

    var snap = DirectorySnapshotter.Take(root, filter);
    beforeSnapshots.Add(snap);
    SnapshotWriter.WriteSnapshot(Path.Combine(artifactsRoot, $"snapshot_before_{i+1}.json"), snap);
}

var sessionStartUtc = DateTimeOffset.UtcNow;
Console.WriteLine("Launching Cubism Editor...");
LaunchedProcess launched;
try
{
    launched = ProcessLauncher.Start(cubismExe);
}
catch (Exception ex)
{
    Console.WriteLine("Не смог запустить Cubism Editor. Проверь путь --exe.");
    Console.WriteLine(ex.ToString());
    Environment.ExitCode = 2;
    return;
}

Console.WriteLine($"Launched PID={launched.Pid}");

// CubismEditor5.exe может отстрелиться и поднять реальный процесс (часто javaw.exe)
var resolvedPid = CubismAuto.Core.Process.ProcessFinder.ResolveCubismPid(
    initialPid: launched.Pid,
    launchTimeUtc: launched.StartTimeUtc,
    expectedExePath: cubismExe,
    timeout: TimeSpan.FromSeconds(15));

if (resolvedPid != launched.Pid)
    Console.WriteLine($"Resolved PID={resolvedPid} (launcher exited)");


Console.WriteLine("Inspecting process modules...");
var pinfo = ProcessInspector.Inspect(resolvedPid);
SnapshotWriter.WriteJson(Path.Combine(artifactsRoot, "process_modules.json"), pinfo);

if (wait)
{
    Console.WriteLine();
    Console.WriteLine("Сделай руками нужное действие в Cubism (например Export moc3), потом вернись сюда.");
    Console.WriteLine("Нажми Enter, когда готов.");
    Console.ReadLine();
}

Console.WriteLine("Taking AFTER snapshot...");
Console.WriteLine("Taking AFTER snapshots...");
var afterSnapshots = new List<DirectorySnapshot>();
for (int i = 0; i < roots.Count; i++)
{
    var root = roots[i];
    Console.WriteLine($"  [{i+1}/{roots.Count}] {root}");

    Func<string, bool> filter = IncludeFile;
    if (IsSamePath(root, temp))
    {
        filter = p => IncludeFile(p) && IsCubismTempPath(p);
    }

    var snap = DirectorySnapshotter.Take(root, filter);
    afterSnapshots.Add(snap);
    SnapshotWriter.WriteSnapshot(Path.Combine(artifactsRoot, $"snapshot_after_{i+1}.json"), snap);
}

Console.WriteLine("Diffing...");
var diffs = new List<SnapshotDiff>();
for (int i = 0; i < roots.Count; i++)
{
    var d = SnapshotDiffer.Diff(beforeSnapshots[i], afterSnapshots[i]);
    diffs.Add(d);
    SnapshotWriter.WriteDiff(Path.Combine(artifactsRoot, $"snapshot_diff_{i+1}.json"), d);
}

// Best-effort scan for recently modified artifacts (helps when project folder doesn't change)
IReadOnlyList<RecentFileHit>? recentHits = null;
try
{
    var recentAfterUtc = sessionStartUtc.AddSeconds(-30);
    var recentBeforeUtc = DateTimeOffset.UtcNow;
    var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    var scanRoots = new List<string>();
    if (!string.IsNullOrWhiteSpace(cmo3Dir)) scanRoots.Add(cmo3Dir);
    scanRoots.Add(Path.Combine(appData, "Live2D"));
    scanRoots.Add(Path.Combine(localAppData, "Live2D"));
    if (Directory.Exists(downloads)) scanRoots.Add(downloads);

    scanRoots = scanRoots
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Where(Directory.Exists)
        .ToList();

    var patterns = new[]
    {
        "*.moc3",
        "*.cmo3",
        "*model3.json",
        "*physics3.json",
        "*motion3.json",
        "*exp3.json",
        "*pose3.json",
        "*userdata3.json",
        "*.png",
        "*.atlas"
    };

    var merged = new Dictionary<string, RecentFileHit>(StringComparer.OrdinalIgnoreCase);
    foreach (var root in scanRoots)
    {
        var hits = RecentFileScanner.Find(
            rootPath: root,
            modifiedAfterUtc: recentAfterUtc,
            modifiedBeforeUtc: recentBeforeUtc,
            patterns: patterns,
            maxHits: 200,
            maxDepth: 12,
            shouldSkipDir: null);

        foreach (var hit in hits)
        {
            merged[hit.Path] = hit;
        }
    }

    recentHits = merged.Values
        .OrderByDescending(h => h.LastWriteTimeUtc)
        .ThenBy(h => h.Path, StringComparer.OrdinalIgnoreCase)
        .Take(200)
        .ToList();
}
catch
{
    recentHits = null;
}

Console.WriteLine("Writing report.md...");
var md = MarkdownReport.Build(pinfo, diffs, recentHits: recentHits, extraNotes: "- Tip: если в `projectsRoot` пусто — ты импортируешь/сохраняешь проект где-то в другом месте. Тогда либо сохрани проект внутрь `_projects`, либо добавь путь: `--path C:\\путь\\к\\проекту` (или `--paths a;b;c`).\n- Ищи `*.moc3` и рядом `model3.json`, `physics3.json`, `motion3.json` и текстуры.\n- Если изменений слишком много — сузь roots до одной маленькой папки с тестовым проектом.");
File.WriteAllText(Path.Combine(artifactsRoot, "report.md"), md);

Console.WriteLine();
Console.WriteLine("Done ✅");
Console.WriteLine($"Artifacts saved to: {artifactsRoot}");

if (Has(cliArgs, "--stop"))
{
    Console.WriteLine("Stopping Cubism...");
    var ok = ProcessLauncher.TryStop(resolvedPid, TimeSpan.FromSeconds(10));
    Console.WriteLine(ok ? "Closed." : "Failed to close (maybe already exited).");
}
