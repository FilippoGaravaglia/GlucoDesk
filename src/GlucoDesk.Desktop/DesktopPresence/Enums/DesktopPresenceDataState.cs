namespace GlucoDesk.Desktop.DesktopPresence.Enums;

/// <summary>
/// Represents the data state shown by the desktop presence indicator.
/// </summary>
public enum DesktopPresenceDataState
{
    /// <summary>
    /// The provider is not configured.
    /// </summary>
    ProviderNotConfigured = 0,

    /// <summary>
    /// No glucose reading is currently available.
    /// </summary>
    NoData = 1,

    /// <summary>
    /// A fresh glucose reading is available.
    /// </summary>
    Fresh = 2,

    /// <summary>
    /// A glucose reading is available, but it is no longer fresh.
    /// </summary>
    Stale = 3
}
