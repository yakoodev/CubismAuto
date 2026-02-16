namespace CubismAuto.Api.Models;

public sealed record CubismSessionWarning(
    string Code,
    string Message,
    string? Path
);
