using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

public sealed class LocalizationManagerPersistenceTests
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
}
