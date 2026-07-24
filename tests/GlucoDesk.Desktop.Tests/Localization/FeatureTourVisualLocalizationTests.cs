using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

/// <summary>
/// Verifies that every user-facing label embedded in the feature-tour
/// illustrations is available in both supported languages.
/// </summary>
[Collection(LocalizationStateCollection.Name)]
public sealed class FeatureTourVisualLocalizationTests :
    EnglishLocalizationTestBase,
    IDisposable
{
    private static readonly string[] VisualTranslationKeys =
    [
        "FeatureTourVisualSteady",
        "FeatureTourVisualRange",
        "FeatureTourVisualTrend",
        "FeatureTourVisualDashboardSummary",
        "FeatureTourVisualLive",
        "FeatureTourVisualLocalHistory",
        "FeatureTourVisualSynced",
        "FeatureTourVisualLastTwentyFourHours",
        "FeatureTourVisualWeeklyContinuity",
        "FeatureTourVisualSavedLocally",
        "FeatureTourVisualHistoryDescription",
        "FeatureTourVisualGlycemicDiary",
        "FeatureTourVisualVisitReady",
        "FeatureTourVisualDataFriendly",
        "FeatureTourVisualDailySummary",
        "FeatureTourVisualMorning",
        "FeatureTourVisualMeals",
        "FeatureTourVisualNight",
        "FeatureTourVisualAccount",
        "FeatureTourVisualSignIn",
        "FeatureTourVisualSecure",
        "FeatureTourVisualAccountDescription",
        "FeatureTourVisualTray",
        "FeatureTourVisualDesktopAlert",
        "FeatureTourVisualNativeNotifications",
        "FeatureTourVisualReady",
        "FeatureTourVisualDashboardFirst",
        "FeatureTourVisualAccountForDexcom",
        "FeatureTourVisualSettingsAnytime",
        "FeatureTourVisualReadyDescription"
    ];

    [Theory]
    [MemberData(nameof(GetVisualTranslationKeys))]
    public void VisualTranslation_ShouldResolveInEnglish(
        string key)
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");

        var translatedText =
            LocalizationManager.GetString(key);

        Assert.False(
            string.IsNullOrWhiteSpace(translatedText));

        Assert.NotEqual(
            key,
            translatedText);
    }

    [Theory]
    [MemberData(nameof(GetVisualTranslationKeys))]
    public void VisualTranslation_ShouldResolveInItalian(
        string key)
    {
        LocalizationManager.SetLanguageForCurrentProcess("it");

        var translatedText =
            LocalizationManager.GetString(key);

        Assert.False(
            string.IsNullOrWhiteSpace(translatedText));

        Assert.NotEqual(
            key,
            translatedText);
    }

    [Fact]
    public void VisualTranslation_ShouldUseExpectedLanguage()
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");

        Assert.Equal(
            "Local history",
            LocalizationManager.GetString(
                "FeatureTourVisualLocalHistory"));

        LocalizationManager.SetLanguageForCurrentProcess("it");

        Assert.Equal(
            "Storico locale",
            LocalizationManager.GetString(
                "FeatureTourVisualLocalHistory"));

        Assert.Equal(
            "GlucoDesk è pronto",
            LocalizationManager.GetString(
                "FeatureTourVisualReady"));

        Assert.Equal(
            "Barra menu",
            LocalizationManager.GetString(
                "FeatureTourVisualTray"));

        Assert.Equal(
            "per la visita",
            LocalizationManager.GetString(
                "FeatureTourVisualVisitReady"));

        Assert.Equal(
            "Impostazioni disponibili",
            LocalizationManager.GetString(
                "FeatureTourVisualSettingsAnytime"));
    }

    public static TheoryData<string> GetVisualTranslationKeys()
    {
        var data = new TheoryData<string>();

        foreach (var key in VisualTranslationKeys)
        {
            data.Add(key);
        }

        return data;
    }

    /// <summary>
    /// Restores deterministic English localization for following tests.
    /// </summary>
    public void Dispose()
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");
    }
}
