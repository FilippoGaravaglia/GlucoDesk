namespace GlucoDesk.Application.Cgm.History.Completeness.Enums;

/// <summary>
/// Defines the user-facing quality level of local glucose history completeness.
/// </summary>
public enum GlucoseHistoryCompletenessLevel
{
    /// <summary>
    /// No completeness level could be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// No local glucose history is available for the selected period.
    /// </summary>
    Empty = 1,

    /// <summary>
    /// Local glucose history coverage is very limited.
    /// </summary>
    Poor = 2,

    /// <summary>
    /// Local glucose history is partially available and should be interpreted carefully.
    /// </summary>
    Partial = 3,

    /// <summary>
    /// Local glucose history is mostly complete and generally usable for summaries.
    /// </summary>
    Reliable = 4,

    /// <summary>
    /// Local glucose history appears complete for the selected period.
    /// </summary>
    Complete = 5
}
