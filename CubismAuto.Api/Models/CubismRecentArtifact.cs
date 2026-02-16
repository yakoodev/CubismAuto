namespace CubismAuto.Api.Models;

public sealed record CubismRecentArtifact(
    string Path,
    long Size,
    DateTimeOffset LastWriteTimeUtc
);
