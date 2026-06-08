using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Settings.Selections;

/// <summary>
/// Represents a selectable glucose unit option in the settings screen.
/// </summary>
public sealed record GlucoseUnitSelectionItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseUnitSelectionItem"/> class.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <param name="displayName">The glucose unit display name.</param>
    public GlucoseUnitSelectionItem(
        GlucoseUnit unit,
        string displayName)
    {
        if (!Enum.IsDefined(unit))
        {
            throw new ArgumentException("Glucose unit must be valid.", nameof(unit));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name must be specified.", nameof(displayName));
        }

        Unit = unit;
        DisplayName = displayName.Trim();
    }

    /// <summary>
    /// Gets the glucose unit.
    /// </summary>
    public GlucoseUnit Unit { get; }

    /// <summary>
    /// Gets the glucose unit display name.
    /// </summary>
    public string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return DisplayName;
    }
}