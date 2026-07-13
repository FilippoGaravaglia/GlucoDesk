using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

public sealed class TranslationCatalogTests
{
    [Fact]
    public void GetTranslations_ShouldExposeSameKeysForEnglishAndItalian()
    {
        // Arrange
        var englishKeys = TranslationCatalog.GetTranslations("en")
            .Keys
            .OrderBy(key => key)
            .ToArray();

        var italianKeys = TranslationCatalog.GetTranslations("it")
            .Keys
            .OrderBy(key => key)
            .ToArray();

        // Assert
        Assert.Equal(englishKeys, italianKeys);
    }

    [Theory]
    [InlineData(null, "en")]
    [InlineData("", "en")]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("it", "it")]
    [InlineData("it-IT", "it")]
    [InlineData("fr", "en")]
    public void NormalizeLanguageCode_ShouldReturnSupportedLanguageCode(
        string? languageCode,
        string expectedLanguageCode)
    {
        // Act
        var normalizedLanguageCode = TranslationCatalog.NormalizeLanguageCode(languageCode);

        // Assert
        Assert.Equal(expectedLanguageCode, normalizedLanguageCode);
    }

    [Fact]
    public void Translate_WhenItalianSelected_ShouldReturnItalianText()
    {
        // Act
        var text = TranslationCatalog.Translate("it", "SettingsTitle");

        // Assert
        Assert.Equal("Impostazioni", text);
    }

    [Fact]
    public void Translate_WhenKeyIsMissing_ShouldReturnKey()
    {
        // Act
        var text = TranslationCatalog.Translate("it", "Missing.Key");

        // Assert
        Assert.Equal("Missing.Key", text);
    }
}
