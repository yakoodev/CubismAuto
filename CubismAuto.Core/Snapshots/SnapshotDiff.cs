namespace CubismAuto.Core.Snapshots;

public sealed record DiffItem(string Path, string Kind, string? Before, string? After);

public sealed record SnapshotDiff(
    string RootPath,
    DateTimeOffset BeforeAtUtc,
    DateTimeOffset AfterAtUtc,
    IReadOnlyList<DiffItem> Items
);

public static class SnapshotDiffer
{
    public static SnapshotDiff Diff(DirectorySnapshot before, DirectorySnapshot after)
    {
        var b = before.Files.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);
        var a = after.Files.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);

        var items = new List<DiffItem>();

        foreach (var (path, af) in a)
        {
            if (!b.TryGetValue(path, out var bf))
            {
                items.Add(new DiffItem(path, "Added", null, $"{af.Size} bytes {af.Sha256}"));
                continue;
            }

            if (!string.Equals(bf.Sha256, af.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new DiffItem(path, "Modified", $"{bf.Size} bytes {bf.Sha256}", $"{af.Size} bytes {af.Sha256}"));
            }
        }

        foreach (var (path, bf) in b)
        {
            if (!a.ContainsKey(path))
                items.Add(new DiffItem(path, "Removed", $"{bf.Size} bytes {bf.Sha256}", null));
        }

        return new SnapshotDiff(before.RootPath, before.TakenAtUtc, after.TakenAtUtc, items);
    }
}
