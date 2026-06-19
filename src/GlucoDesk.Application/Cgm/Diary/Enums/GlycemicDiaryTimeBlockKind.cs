namespace GlucoDesk.Application.Cgm.Diary.Enums;

/// <summary>
/// Represents a standard time block used in the glycemic diary.
/// </summary>
public enum GlycemicDiaryTimeBlockKind
{
    /// <summary>
    /// The time block is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The breakfast time block.
    /// </summary>
    Breakfast = 1,

    /// <summary>
    /// The lunch time block.
    /// </summary>
    Lunch = 2,

    /// <summary>
    /// The dinner time block.
    /// </summary>
    Dinner = 3,

    /// <summary>
    /// The pre-night or bedtime time block.
    /// </summary>
    Bedtime = 4
}