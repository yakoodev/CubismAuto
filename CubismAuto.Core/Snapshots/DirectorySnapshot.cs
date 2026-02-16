using System.Security.Cryptography;

namespace CubismAuto.Core.Snapshots;

public sealed record FileEntry(
    string Path,
    long Size,
    DateTimeOffset LastWriteTimeUtc,
    string Sha256
);

public sealed record DirectorySnapshot(
    string RootPath,
    DateTimeOffset TakenAtUtc,
    IReadOnlyList<FileEntry> Files
);

public static class DirectorySnapshotter
{
    public static DirectorySnapshot Take(string rootPath, Func<string, bool>? includeFile = null, Action<string>? warn = null)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException(rootPath);

        includeFile ??= (_)=> true;

        var files = new List<FileEntry>();
        var rootFull = Path.GetFullPath(rootPath);

        // Safe traversal: skip directories/files we can't access (Temp often contains protected folders).
        var stack = new Stack<string>();
        stack.Push(rootFull);

        while (stack.Count > 0)
        {
            var dir = stack.Pop();

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(dir);
            }
            catch (UnauthorizedAccessException)
            {
                warn?.Invoke($"skip dir (unauthorized): {dir}");
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                warn?.Invoke($"skip dir (not found): {dir}");
                continue;
            }
            catch (IOException)
            {
                warn?.Invoke($"skip dir (io): {dir}");
                continue;
            }

            foreach (var sd in subDirs)
                stack.Push(sd);

            IEnumerable<string> dirFiles;
            try
            {
                dirFiles = Directory.EnumerateFiles(dir);
            }
            catch (UnauthorizedAccessException)
            {
                warn?.Invoke($"skip files in dir (unauthorized): {dir}");
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                warn?.Invoke($"skip files in dir (not found): {dir}");
                continue;
            }
            catch (IOException)
            {
                warn?.Invoke($"skip files in dir (io): {dir}");
                continue;
            }

            foreach (var file in dirFiles)
            {
                if (!includeFile(file)) continue;

                try
                {
                    var fi = new FileInfo(file);
                    files.Add(new FileEntry(
                        Path: file,
                        Size: fi.Length,
                        LastWriteTimeUtc: fi.LastWriteTimeUtc,
                        Sha256: ComputeSha256(file)
                    ));
                }
                catch (UnauthorizedAccessException)
                {
                    warn?.Invoke($"skip file (unauthorized): {file}");
                }
                catch (DirectoryNotFoundException)
                {
                    warn?.Invoke($"skip file (not found): {file}");
                }
                catch (IOException)
                {
                    warn?.Invoke($"skip file (io): {file}");
                }
            }
        }

        return new DirectorySnapshot(rootFull, DateTimeOffset.UtcNow, files);
    }

    private static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var hash = sha.ComputeHash(fs);
        return Convert.ToHexString(hash);
    }
}
