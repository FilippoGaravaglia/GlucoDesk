namespace GlucoDesk.Application.Cgm.Diary.Reviews.Enums;

/// <summary>
/// Defines the user-facing severity of a glycemic diary review signal.
/// </summary>
public enum GlycemicDiaryReviewSignalSeverity
{
    /// <summary>
    /// Unknown severity.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Informational change.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Change that should be interpreted carefully.
    /// </summary>
    Caution = 2,

    /// <summary>
    /// Change that deserves attention.
    /// </summary>
    Important = 3
}
