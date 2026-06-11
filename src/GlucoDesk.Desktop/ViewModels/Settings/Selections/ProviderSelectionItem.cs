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
    /// <param name="isAvailable">Whether the provider is currently available in the desktop runtime.</param>
    /// <param name="availabilityMessage">The provider availability message.</param>
    public ProviderSelectionItem(
        CgmProviderKind kind,
        string displayName,
        bool isAvailable = true,
        string? availabilityMessage = null)
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
        IsAvailable = isAvailable;
        AvailabilityMessage = string.IsNullOrWhiteSpace(availabilityMessage)
            ? BuildDefaultAvailabilityMessage(isAvailable)
            : availabilityMessage.Trim();
    }

    /// <summary>
    /// Gets the CGM provider kind.
    /// </summary>
    public CgmProviderKind Kind { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets a value indicating whether the provider is currently available in the desktop runtime.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Gets the provider availability message.
    /// </summary>
    public string AvailabilityMessage { get; }

    /// <summary>
    /// Gets the provider display label used by selection controls.
    /// </summary>
    public string DisplayLabel => IsAvailable
        ? DisplayName
        : $"{DisplayName} (not configured)";

    /// <inheritdoc />
    public override string ToString()
    {
        return DisplayLabel;
    }

    #region Helpers

    /// <summary>
    /// Builds a default provider availability message.
    /// </summary>
    /// <param name="isAvailable">Whether the provider is available.</param>
    /// <returns>The default availability message.</returns>
    private static string BuildDefaultAvailabilityMessage(bool isAvailable)
    {
        return isAvailable
            ? "Provider is available."
            : "Provider is not configured in the current desktop runtime.";
    }

    #endregion
}