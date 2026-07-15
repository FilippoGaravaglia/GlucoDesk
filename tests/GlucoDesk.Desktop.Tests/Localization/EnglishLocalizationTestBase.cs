using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

/// <summary>
/// Ensures tests that verify legacy English UI contracts do not depend
/// on the language preference stored on the developer machine.
/// </summary>
public abstract class EnglishLocalizationTestBase
{
    /// <summary>
    /// Initializes the test using the deterministic English language.
    /// The preference is changed only for the current test process and
    /// is never written to the user's local settings.
    /// </summary>
    protected EnglishLocalizationTestBase()
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");
    }
}
