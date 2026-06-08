using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Settings.Selections;

/// <summary>
/// Represents a selectable CGM provider option in the settings screen.
/// </summary>
public sealed record ProviderSelectionItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderSelectionItem"/> class.
    /// </summary>
    /// <param name="kind">The CGM provider kind.</param>
    /// <param name="displayName">The provider display name.</param>
    public ProviderSelectionItem(
        CgmProviderKind kind,
        string displayName)
    {
        if (kind == CgmProviderKind.Unknown)
        {
            throw new ArgumentException("Provider kind must be specified.", nameof(kind));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name must be specified.", nameof(displayName));
        }

        Kind = kind;
        DisplayName = displayName.Trim();
    }

    /// <summary>
    /// Gets the CGM provider kind.
    /// </summary>
    public CgmProviderKind Kind { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return DisplayName;
    }
}