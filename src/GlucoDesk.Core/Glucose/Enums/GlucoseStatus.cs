namespace GlucoDesk.Core.Glucose.Enums;

/// <summary>
/// Represents the status of a glucose value compared to a configured target range.
/// </summary>
public enum GlucoseStatus
{
    /// <summary>
    /// The status could not be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The glucose value is below the configured target range.
    /// </summary>
    Low = 1,

    /// <summary>
    /// The glucose value is inside the configured target range.
    /// </summary>
    InRange = 2,

    /// <summary>
    /// The glucose value is above the configured target range.
    /// </summary>
    High = 3
}