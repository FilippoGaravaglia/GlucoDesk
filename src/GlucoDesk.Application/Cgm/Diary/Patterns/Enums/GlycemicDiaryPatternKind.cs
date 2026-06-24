namespace GlucoDesk.Application.Cgm.Diary.Patterns.Enums;

/// <summary>
/// Defines the type of glycemic diary pattern detected from local history.
/// </summary>
public enum GlycemicDiaryPatternKind
{
    /// <summary>
    /// No pattern kind could be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The selected period contains limited local history coverage.
    /// </summary>
    LimitedDataCoverage = 1,

    /// <summary>
    /// A recurring low glucose tendency was detected.
    /// </summary>
    RecurringLow = 2,

    /// <summary>
    /// A recurring high glucose tendency was detected.
    /// </summary>
    RecurringHigh = 3,

    /// <summary>
    /// A recurring variable glucose tendency was detected.
    /// </summary>
    RecurringVariability = 4,

    /// <summary>
    /// A stable time block was detected.
    /// </summary>
    StableTimeBlock = 5
}
