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
    public static DirectorySnapshot Take(string rootPath, Func<string, bool>? includeFile = null)
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
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }
            catch (IOException)
            {
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
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }
            catch (IOException)
            {
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
                    // Skip unreadable files.
                }
                catch (DirectoryNotFoundException)
                {
                    // File disappeared during traversal.
                }
                catch (IOException)
                {
                    // Skip locked/temporary files.
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
