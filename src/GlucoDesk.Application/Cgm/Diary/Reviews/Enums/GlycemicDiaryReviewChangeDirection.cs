namespace GlucoDesk.Application.Cgm.Diary.Reviews.Enums;

/// <summary>
/// Defines how a metric changed between two diary periods.
/// </summary>
public enum GlycemicDiaryReviewChangeDirection
{
    /// <summary>
    /// Unknown change direction.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The metric is broadly unchanged.
    /// </summary>
    Unchanged = 1,

    /// <summary>
    /// The metric increased.
    /// </summary>
    Increased = 2,

    /// <summary>
    /// The metric decreased.
    /// </summary>
    Decreased = 3,

    /// <summary>
    /// The metric is available only in the current period.
    /// </summary>
    NewlyAvailable = 4,

    /// <summary>
    /// The metric is no longer available in the current period.
    /// </summary>
    NoLongerAvailable = 5
}
