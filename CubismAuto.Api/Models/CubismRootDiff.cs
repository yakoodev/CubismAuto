namespace CubismAuto.Api.Models;

public sealed record CubismRootDiff(
    string RootPath,
    DateTimeOffset BeforeAtUtc,
    DateTimeOffset AfterAtUtc,
    IReadOnlyList<CubismDiffItem> Items
);
