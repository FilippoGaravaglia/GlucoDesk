using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

public sealed class DesktopPresenceNotificationTranslationTests
{
    [Theory]
    [InlineData(
        "en",
        "DesktopPresenceRefreshNow",
        "Refresh now")]
    [InlineData(
        "it",
        "DesktopPresenceRefreshNow",
        "Aggiorna ora")]
    [InlineData(
        "en",
        "DesktopPresencePrivacyOn",
        "Privacy mode: On")]
    [InlineData(
        "it",
        "DesktopPresencePrivacyOn",
        "Modalità privacy: attiva")]
    [InlineData(
        "en",
        "DesktopPresenceOpen",
        "Open GlucoDesk")]
    [InlineData(
        "it",
        "DesktopPresenceOpen",
        "Apri GlucoDesk")]
    [InlineData(
        "en",
        "GlucoseAlertLowTitle",
        "Glucose below target")]
    [InlineData(
        "it",
        "GlucoseAlertLowTitle",
        "Glicemia sotto target")]
    [InlineData(
        "en",
        "GlucoseAlertTestTitle",
        "GlucoDesk notification test")]
    [InlineData(
        "it",
        "GlucoseAlertTestTitle",
        "Test notifiche GlucoDesk")]
    public void Translate_ShouldReturnExpectedDesktopText(
        string languageCode,
        string key,
        string expected)
    {
        var result = TranslationCatalog.Translate(
            languageCode,
            key);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TranslationCatalog_ShouldContainSameKeysForAllLanguages()
    {
        var englishKeys = TranslationCatalog
            .GetTranslations("en")
            .Keys
            .OrderBy(key => key)
            .ToArray();

        var italianKeys = TranslationCatalog
            .GetTranslations("it")
            .Keys
            .OrderBy(key => key)
            .ToArray();

        Assert.Equal(englishKeys, italianKeys);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("it")]
    public void NotificationTranslations_ShouldNotBeEmpty(
        string languageCode)
    {
        var requiredKeys = new[]
        {
            "GlucoseAlertLowTitle",
            "GlucoseAlertHighTitle",
            "GlucoseAlertLowPrivacyMessage",
            "GlucoseAlertHighPrivacyMessage",
            "GlucoseAlertDetailedLowMessage",
            "GlucoseAlertDetailedHighMessage",
            "GlucoseAlertSafetyAction",
            "GlucoseAlertNativeSubtitle",
            "GlucoseAlertTestTitle",
            "GlucoseAlertTestMessage"
        };

        foreach (var key in requiredKeys)
        {
            var result = TranslationCatalog.Translate(
                languageCode,
                key);

            Assert.False(
                string.IsNullOrWhiteSpace(result),
                $"Translation '{key}' is empty for '{languageCode}'.");

            Assert.NotEqual(key, result);
        }
    }
}
