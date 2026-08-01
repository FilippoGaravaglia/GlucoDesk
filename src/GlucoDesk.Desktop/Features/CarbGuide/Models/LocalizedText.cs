
namespace GlucoDesk.Desktop.Features.CarbGuide.Models;

/// <summary>
/// Represents a simple bilingual text used by the carbohydrate guide.
/// </summary>
public sealed record LocalizedText(
    string It,
    string En)
{
    /// <summary>
    /// Returns the best text for the specified language code.
    /// Defaults to English when the language is not Italian.
    /// </summary>
    public string Get(
        string? languageCode)
    {
        return string.Equals(
            languageCode,
            "it",
            StringComparison.OrdinalIgnoreCase)
            ? It
            : En;
    }
}
