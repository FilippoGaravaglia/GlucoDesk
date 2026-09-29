using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Represents the current update-center state exposed to the presentation layer.
/// </summary>
public sealed record UpdateCenterSnapshot
{
    /// <summary>
    /// Gets the current update-center state.
    /// </summary>
    public required UpdateCenterState State { get; init; }

    /// <summary>
    /// Gets the currently installed application version.
    /// </summary>
    public required string CurrentVersion { get; init; }

    /// <summary>
    /// Gets the latest compatible release when available.
    /// </summary>
    public UpdateRelease? LatestRelease { get; init; }

    /// <summary>
    /// Gets the timestamp of the last successful update check.
    /// </summary>
    public DateTimeOffset? LastSuccessfulCheckAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the startup update dialog should be shown.
    /// </summary>
    public bool ShouldShowDialog { get; init; }

    /// <summary>
    /// Gets a user-displayable error message when the update check failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
