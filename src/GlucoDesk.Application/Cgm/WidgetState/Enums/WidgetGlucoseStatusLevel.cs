namespace GlucoDesk.Application.Cgm.WidgetState.Enums;

/// <summary>
/// Represents the glucose status level exposed to external widget surfaces.
/// </summary>
public enum WidgetGlucoseStatusLevel
{
    /// <summary>
    /// The status is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// No glucose reading is currently available.
    /// </summary>
    Unavailable = 1,

    /// <summary>
    /// The glucose reading is in the configured display range.
    /// </summary>
    InRange = 2,

    /// <summary>
    /// The glucose reading is below the configured display range.
    /// </summary>
    Low = 3,

    /// <summary>
    /// The glucose reading is above the configured display range.
    /// </summary>
    High = 4,

    /// <summary>
    /// The glucose reading exists but is too old for a live widget display.
    /// </summary>
    Stale = 5
}