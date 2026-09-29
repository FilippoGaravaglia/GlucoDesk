namespace GlucoDesk.Core.Updates;

/// <summary>
/// Stores local preferences and state related to GlucoDesk update notifications.
/// </summary>
public sealed class UpdatePreferences
{
    /// <summary>
    /// Gets or sets whether updates are checked automatically.
    /// </summary>
    public bool CheckAutomatically { get; set; } = true;

    /// <summary>
    /// Gets or sets whether preview releases may be offered.
    /// </summary>
    public bool IncludePreviewReleases { get; set; } = true;

    /// <summary>
    /// Gets or sets the last release dismissed by the user.
    /// </summary>
    public string? LastDismissedVersion { get; set; }

    /// <summary>
    /// Gets or sets when the last release notification was dismissed.
    /// </summary>
    public DateTimeOffset? LastDismissedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last successful update check.
    /// </summary>
    public DateTimeOffset? LastSuccessfulCheckAt { get; set; }
}
