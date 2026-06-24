namespace GlucoDesk.Application.Cgm.Diary.Reviews.Enums;

/// <summary>
/// Defines the metric kind used in a glycemic diary review comparison.
/// </summary>
public enum GlycemicDiaryReviewMetricKind
{
    /// <summary>
    /// Unknown metric.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Average glucose metric.
    /// </summary>
    AverageGlucose = 1,

    /// <summary>
    /// Time-in-range metric.
    /// </summary>
    TimeInRange = 2,

    /// <summary>
    /// Local history data coverage metric.
    /// </summary>
    DataCoverage = 3,

    /// <summary>
    /// Local readings count metric.
    /// </summary>
    ReadingCount = 4,

    /// <summary>
    /// Detected local pattern count metric.
    /// </summary>
    PatternCount = 5,

    /// <summary>
    /// Incomplete days count metric.
    /// </summary>
    IncompleteDays = 6,

    /// <summary>
    /// Empty days count metric.
    /// </summary>
    EmptyDays = 7
}
