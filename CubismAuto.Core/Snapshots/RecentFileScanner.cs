using System.Collections.Concurrent;

namespace CubismAuto.Core.Snapshots;

public sealed record RecentFileHit(
    string Path,
    long Size,
    DateTimeOffset LastWriteTimeUtc
);

public static class RecentFileScanner
{
    /// <summary>
    /// Ищет файлы по маскам, изменённые после указанного времени, под указанным root.
    /// Сканирование "best effort": пропускает недоступные папки/файлы.
    /// </summary>
    public static IReadOnlyList<RecentFileHit> Find(
        string rootPath,
        DateTimeOffset modifiedAfterUtc,
        string[] patterns,
        int maxHits = 200,
        int maxDepth = 12,
        Func<string, bool>? shouldSkipDir = null)
    {
        var hits = new ConcurrentBag<RecentFileHit>();

        shouldSkipDir ??= _ => false;

        var root = Path.GetFullPath(rootPath);

        void Walk(string dir, int depth)
        {
            if (hits.Count >= maxHits) return;
            if (depth > maxDepth) return;

            try
            {
                // Сначала директории
                foreach (var sub in Directory.EnumerateDirectories(dir))
                {
                    if (hits.Count >= maxHits) break;

                    try
                    {
                        if (shouldSkipDir(sub)) continue;
                        Walk(sub, depth + 1);
                    }
                    catch
                    {
                        // best effort
                    }
                }

                // Потом файлы
                foreach (var pat in patterns)
                {
                    if (hits.Count >= maxHits) break;

                    IEnumerable<string> files;
                    try
                    {
                        files = Directory.EnumerateFiles(dir, pat, SearchOption.TopDirectoryOnly);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var f in files)
                    {
                        if (hits.Count >= maxHits) break;

                        try
                        {
                            var fi = new FileInfo(f);
                            var lw = DateTimeOffset.FromFileTime(fi.LastWriteTimeUtc.ToFileTimeUtc());
                            if (fi.LastWriteTimeUtc <= modifiedAfterUtc.UtcDateTime) continue;

                            hits.Add(new RecentFileHit(fi.FullName, fi.Length, fi.LastWriteTimeUtc));
                        }
                        catch
                        {
                            // best effort
                        }
                    }
                }
            }
            catch
            {
                // best effort
            }
        }

        Walk(root, 0);

        return hits
            .OrderByDescending(h => h.LastWriteTimeUtc)
            .ThenBy(h => h.Path, StringComparer.OrdinalIgnoreCase)
            .Take(maxHits)
            .ToList();
    }
}
