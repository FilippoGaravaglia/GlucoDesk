using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Determines when an available-update dialog should be shown.
/// </summary>
public static class UpdateNotificationPolicy
{
    private static readonly TimeSpan ReminderInterval =
        TimeSpan.FromDays(1);

    /// <summary>
    /// Determines whether the update dialog should be displayed.
    /// </summary>
    public static bool ShouldNotify(
        UpdateRelease release,
        UpdatePreferences preferences,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(preferences);

        if (!string.Equals(
                release.Version,
                preferences.LastDismissedVersion,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (preferences.LastDismissedAt is null)
        {
            return true;
        }

        return now - preferences.LastDismissedAt.Value
               >= ReminderInterval;
    }
}
