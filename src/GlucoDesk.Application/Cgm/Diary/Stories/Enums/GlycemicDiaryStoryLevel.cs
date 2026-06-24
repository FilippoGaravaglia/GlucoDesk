namespace GlucoDesk.Application.Cgm.Diary.Stories.Enums;

/// <summary>
/// Defines the user-facing interpretation level of a glycemic diary story.
/// </summary>
public enum GlycemicDiaryStoryLevel
{
    /// <summary>
    /// No story level could be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// No local glucose data is available.
    /// </summary>
    NoData = 1,

    /// <summary>
    /// The story should be interpreted carefully because local history is incomplete.
    /// </summary>
    Caution = 2,

    /// <summary>
    /// The glucose period appears variable.
    /// </summary>
    Variable = 3,

    /// <summary>
    /// The glucose period appears mostly stable.
    /// </summary>
    Stable = 4,

    /// <summary>
    /// The glucose period appears stable with strong time-in-range.
    /// </summary>
    Excellent = 5
}
