using GlucoDesk.Desktop.DesktopPresence.Enums;

namespace GlucoDesk.Desktop.DesktopPresence.Models;

/// <summary>
/// Represents the minimal glucose state required by the desktop presence indicator.
/// </summary>
/// <param name="DataState">The current data state.</param>
/// <param name="DisplayValue">The display glucose value in the selected unit.</param>
/// <param name="UnitSymbol">The selected glucose unit symbol.</param>
/// <param name="TrendText">The optional glucose trend text.</param>
/// <param name="ReadingTimestamp">The timestamp of the displayed glucose reading.</param>
/// <param name="Now">The current timestamp used to calculate reading age.</param>
/// <param name="IsPrivacyModeEnabled">Whether the exact glucose value should be hidden.</param>
public sealed record DesktopPresenceSnapshot(
    DesktopPresenceDataState DataState,
    decimal? DisplayValue,
    string UnitSymbol,
    string? TrendText,
    DateTimeOffset? ReadingTimestamp,
    DateTimeOffset Now,
    bool IsPrivacyModeEnabled);
