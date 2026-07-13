namespace GlucoDesk.Desktop.Localization;

/// <summary>
/// Represents a user-selectable application language.
/// </summary>
/// <param name="Code">The BCP-47 language code used by GlucoDesk.</param>
/// <param name="DisplayName">The English display name.</param>
/// <param name="NativeName">The native display name shown in the UI.</param>
public sealed record AppLanguageOption(
    string Code,
    string DisplayName,
    string NativeName)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return NativeName;
    }
}
