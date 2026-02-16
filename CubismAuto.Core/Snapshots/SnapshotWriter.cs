using System.Text.Json;

namespace CubismAuto.Core.Snapshots;

public static class SnapshotWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void WriteSnapshot(string filePath, DirectorySnapshot snapshot)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    public static void WriteDiff(string filePath, SnapshotDiff diff)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(diff, JsonOptions));
    }

    public static void WriteJson<T>(string filePath, T data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(data, JsonOptions));
    }
}
