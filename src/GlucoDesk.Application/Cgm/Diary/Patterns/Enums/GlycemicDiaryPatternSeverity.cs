namespace GlucoDesk.Application.Cgm.Diary.Patterns.Enums;

/// <summary>
/// Defines the user-facing severity of a detected glycemic diary pattern.
/// </summary>
public enum GlycemicDiaryPatternSeverity
{
    /// <summary>
    /// No severity could be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Informational pattern.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Pattern that should be interpreted carefully.
    /// </summary>
    Caution = 2,

    /// <summary>
    /// Pattern that deserves user attention.
    /// </summary>
    Important = 3
}
