namespace CubismAuto.Api.Models;

public sealed record CubismDiffItem(
    string Path,
    string Kind,
    string? Before,
    string? After
);
