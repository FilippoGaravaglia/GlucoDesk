using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Desktop.ViewModels.Account;

/// <summary>
/// Represents a Dexcom Share region selection option.
/// </summary>
public sealed record DexcomShareRegionSelectionItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareRegionSelectionItem"/> class.
    /// </summary>
    /// <param name="region">The Dexcom Share region.</param>
    /// <param name="displayName">The display name.</param>
    public DexcomShareRegionSelectionItem(
        DexcomShareRegion region,
        string displayName)
    {
        Region = region;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the Dexcom Share region.
    /// </summary>
    public DexcomShareRegion Region { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return DisplayName;
    }
}