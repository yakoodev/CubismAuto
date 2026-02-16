namespace CubismAuto.Api.Models;

public sealed record CubismCapabilities(
    bool CanLaunch,
    bool CanInspectModules,
    bool CanUiAutomate,
    string Notes
);
