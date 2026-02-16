using CubismAuto.Api.Abstractions;
using CubismAuto.Api.Models;

namespace CubismAuto.Api.Implementations;

/// <summary>
/// Temporary planning implementation for the API project.
/// Real execution adapter to CubismAuto.Core is planned in T10.
/// </summary>
public sealed class PlanningCubismAutomationApi : ICubismAutomationApi
{
    public Task<CubismCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var capabilities = new CubismCapabilities(
            CanLaunch: true,
            CanInspectModules: true,
            CanUiAutomate: false,
            Notes: "Planning implementation. Runtime adapter is not wired yet.");

        return Task.FromResult(capabilities);
    }

    public Task<CubismSessionResult> RunSessionAsync(
        CubismSessionOptions options,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var finishedAt = startedAt;

        var result = new CubismSessionResult(
            Success: false,
            ResolvedPid: null,
            ProcessName: null,
            ArtifactsRoot: options.ArtifactsRoot,
            StartedAtUtc: startedAt,
            FinishedAtUtc: finishedAt,
            RootDiffs: Array.Empty<CubismRootDiff>(),
            RecentArtifacts: Array.Empty<CubismRecentArtifact>(),
            Warnings: new[]
            {
                new CubismSessionWarning(
                    Code: "not_implemented",
                    Message: "Planning API implementation does not execute sessions yet.",
                    Path: null)
            });

        return Task.FromResult(result);
    }
}
