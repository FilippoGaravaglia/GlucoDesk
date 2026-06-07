namespace GlucoDesk.Core.Glucose.Enums;

/// <summary>
/// Describes how fresh a glucose reading is.
/// </summary>
public enum GlucoseDataFreshness
{
    /// <summary>
    /// The freshness is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The reading is considered live.
    /// </summary>
    Live = 1,

    /// <summary>
    /// The reading is near real-time.
    /// </summary>
    NearRealTime = 2,

    /// <summary>
    /// The reading is delayed by the upstream provider.
    /// </summary>
    Delayed = 3,

    /// <summary>
    /// The reading is historical and should not be treated as current.
    /// </summary>
    Historical = 4
}