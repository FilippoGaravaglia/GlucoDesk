namespace GlucoDesk.Core.Updates;

/// <summary>
/// Represents the result of checking whether a newer GlucoDesk release exists.
/// </summary>
public sealed record UpdateCheckResult
{
    /// <summary>
    /// Gets the currently installed application version.
    /// </summary>
    public required string CurrentVersion { get; init; }

    /// <summary>
    /// Gets the latest compatible release when one was discovered.
    /// </summary>
    public UpdateRelease? LatestRelease { get; init; }

    /// <summary>
    /// Gets a value indicating whether a newer compatible release is available.
    /// </summary>
    public bool IsUpdateAvailable { get; init; }

    /// <summary>
    /// Gets a human-readable failure message when the check could not complete.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets a value indicating whether the check completed successfully.
    /// </summary>
    public bool IsSuccessful => ErrorMessage is null;
}
