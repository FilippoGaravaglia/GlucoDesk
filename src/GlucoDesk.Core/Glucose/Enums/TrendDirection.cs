namespace GlucoDesk.Core.Glucose.Enums;

/// <summary>
/// Represents the glucose trend direction reported by a CGM provider.
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// The trend is unknown or not available.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Glucose is rising very quickly.
    /// </summary>
    DoubleUp = 1,

    /// <summary>
    /// Glucose is rising quickly.
    /// </summary>
    SingleUp = 2,

    /// <summary>
    /// Glucose is rising moderately.
    /// </summary>
    FortyFiveUp = 3,

    /// <summary>
    /// Glucose is stable.
    /// </summary>
    Flat = 4,

    /// <summary>
    /// Glucose is falling moderately.
    /// </summary>
    FortyFiveDown = 5,

    /// <summary>
    /// Glucose is falling quickly.
    /// </summary>
    SingleDown = 6,

    /// <summary>
    /// Glucose is falling very quickly.
    /// </summary>
    DoubleDown = 7,

    /// <summary>
    /// The trend cannot be computed.
    /// </summary>
    NotComputable = 8,

    /// <summary>
    /// The trend rate is outside the expected range.
    /// </summary>
    RateOutOfRange = 9
}