namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Decides whether the first-launch language onboarding must be displayed.
/// </summary>
public static class LanguageOnboardingLaunchPolicy
{
    /// <summary>
    /// Determines whether onboarding should be displayed.
    /// </summary>
    /// <param name="hasExplicitLanguagePreference">
    /// Whether the user already persisted an explicit language choice.
    /// </param>
    /// <param name="forceOnboardingValue">
    /// Optional environment override used for development and QA.
    /// </param>
    /// <returns>
    /// True when onboarding must be displayed; otherwise false.
    /// </returns>
    public static bool ShouldShow(
        bool hasExplicitLanguagePreference,
        string? forceOnboardingValue)
    {
        return IsEnabled(forceOnboardingValue)
            || !hasExplicitLanguagePreference;
    }

    private static bool IsEnabled(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim() switch
        {
            "1" => true,
            "true" => true,
            "TRUE" => true,
            "True" => true,
            "yes" => true,
            "YES" => true,
            "Yes" => true,
            _ => false
        };
    }
}
