namespace CubismAuto.Api.Models;

public sealed record CubismSessionOptions(
    string CubismExePath,
    string? Cmo3Path,
    string ProjectsRoot,
    IReadOnlyList<string> AdditionalRoots,
    string ArtifactsRoot,
    bool WaitForManualAction,
    bool StopCubismOnExit,
    TimeSpan ResolvePidTimeout
)
{
    public static CubismSessionOptions CreateDefault(
        string cubismExePath,
        string projectsRoot,
        string artifactsRoot)
        => new(
            CubismExePath: cubismExePath,
            Cmo3Path: null,
            ProjectsRoot: projectsRoot,
            AdditionalRoots: Array.Empty<string>(),
            ArtifactsRoot: artifactsRoot,
            WaitForManualAction: true,
            StopCubismOnExit: false,
            ResolvePidTimeout: TimeSpan.FromSeconds(15));
}
