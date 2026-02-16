namespace CubismAuto.Api.Models;

public sealed record CubismSessionResult(
    bool Success,
    int? ResolvedPid,
    string? ProcessName,
    string ArtifactsRoot,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    IReadOnlyList<CubismRootDiff> RootDiffs,
    IReadOnlyList<CubismRecentArtifact> RecentArtifacts,
    IReadOnlyList<CubismSessionWarning> Warnings
);
