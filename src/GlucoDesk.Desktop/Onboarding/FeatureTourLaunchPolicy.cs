namespace GlucoDesk.Desktop.Onboarding;

/// <summary>
/// Decides whether the first-run feature tour must be displayed.
/// </summary>
public static class FeatureTourLaunchPolicy
{
    /// <summary>
    /// Returns whether the feature tour should be shown.
    /// </summary>
    /// <param name="hasCompletedCurrentTour">
    /// Whether the persisted current tour is complete.
    /// </param>
    /// <param name="forceTourValue">
    /// Optional environment variable value used for development testing.
    /// </param>
    public static bool ShouldShow(
        bool hasCompletedCurrentTour,
        string? forceTourValue)
    {
        if (IsEnabled(forceTourValue))
        {
            return true;
        }

        return !hasCompletedCurrentTour;
    }

    private static bool IsEnabled(string? value)
    {
        return string.Equals(
                   value,
                   "1",
                   StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                value,
                "true",
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                value,
                "yes",
                StringComparison.OrdinalIgnoreCase);
    }
}
