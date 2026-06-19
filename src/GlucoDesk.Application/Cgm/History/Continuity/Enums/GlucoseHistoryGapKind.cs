namespace GlucoDesk.Application.Cgm.History.Continuity.Enums;

/// <summary>
/// Represents the type of a detected glucose history gap.
/// </summary>
public enum GlucoseHistoryGapKind
{
    /// <summary>
    /// The gap type is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The gap is at the beginning of the analyzed window.
    /// </summary>
    Leading = 1,

    /// <summary>
    /// The gap is between two recorded glucose readings.
    /// </summary>
    BetweenReadings = 2,

    /// <summary>
    /// The gap is at the end of the analyzed window.
    /// </summary>
    Trailing = 3,

    /// <summary>
    /// The analyzed window contains no glucose readings.
    /// </summary>
    EmptyWindow = 4
}