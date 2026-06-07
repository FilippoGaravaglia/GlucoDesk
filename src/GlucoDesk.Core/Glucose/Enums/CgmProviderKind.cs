namespace GlucoDesk.Core.Glucose.Enums;

/// <summary>
/// Identifies the CGM data provider used to obtain glucose data.
/// </summary>
public enum CgmProviderKind
{
    /// <summary>
    /// The provider is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Locally generated mock data.
    /// </summary>
    Mock = 1,

    /// <summary>
    /// Nightscout data provider.
    /// </summary>
    Nightscout = 2,

    /// <summary>
    /// Dexcom sandbox API provider.
    /// </summary>
    DexcomSandbox = 3,

    /// <summary>
    /// Dexcom official API provider.
    /// </summary>
    DexcomOfficial = 4
}