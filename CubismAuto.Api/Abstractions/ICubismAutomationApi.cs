using CubismAuto.Api.Models;

namespace CubismAuto.Api.Abstractions;

/// <summary>
/// Primary DLL API contract for Cubism automation sessions.
/// </summary>
public interface ICubismAutomationApi
{
    /// <summary>
    /// Returns runtime capabilities of the current host machine.
    /// </summary>
    Task<CubismCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs one observability/automation session and returns structured results.
    /// </summary>
    Task<CubismSessionResult> RunSessionAsync(
        CubismSessionOptions options,
        CancellationToken cancellationToken = default);
}
