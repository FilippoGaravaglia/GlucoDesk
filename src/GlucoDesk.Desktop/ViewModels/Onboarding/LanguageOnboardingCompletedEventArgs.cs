namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Describes a completed first-launch language selection.
/// </summary>
public sealed class LanguageOnboardingCompletedEventArgs : EventArgs
{
    public LanguageOnboardingCompletedEventArgs(
        string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException(
                "Language code cannot be empty.",
                nameof(languageCode));
        }

        LanguageCode = languageCode;
    }

    public string LanguageCode { get; }
}
