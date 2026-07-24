using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

/// <summary>
/// Serializes tests that intentionally mutate the process-wide localization
/// state and restores deterministic English localization after every test.
/// </summary>
[Collection(LocalizationStateCollection.Name)]
public sealed class LocalizationManagerPersistenceTests :
    EnglishLocalizationTestBase,
    IDisposable
{
    [Fact]
    public void ApplyAndPersistLanguage_ShouldPersist_WhenLanguageIsAlreadyActive()
    {
        // Arrange
        LocalizationManager.SetLanguageForCurrentProcess("it");

        var persistedLanguages = new List<string>();

        // Act
        LocalizationManager.ApplyAndPersistLanguage(
            "it",
            persistedLanguages.Add);

        // Assert
        Assert.Equal(
            ["it"],
            persistedLanguages);

        Assert.Equal(
            "it",
            LocalizationManager.CurrentLanguageCode);
    }

    [Fact]
    public void ApplyAndPersistLanguage_ShouldPersistNormalizedLanguage()
    {
        // Arrange
        LocalizationManager.SetLanguageForCurrentProcess("en");

        var persistedLanguages = new List<string>();

        // Act
        LocalizationManager.ApplyAndPersistLanguage(
            "it-IT",
            persistedLanguages.Add);

        // Assert
        Assert.Equal(
            ["it"],
            persistedLanguages);

        Assert.Equal(
            "it",
            LocalizationManager.CurrentLanguageCode);
    }

    /// <summary>
    /// Restores the deterministic language expected by the legacy UI tests.
    /// </summary>
    public void Dispose()
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");
    }
}
