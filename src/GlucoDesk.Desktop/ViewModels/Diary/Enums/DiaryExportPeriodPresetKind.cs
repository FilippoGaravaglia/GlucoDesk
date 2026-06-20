namespace GlucoDesk.Desktop.ViewModels.Diary.Enums;

/// <summary>
/// Represents the supported glycemic diary export period presets.
/// </summary>
public enum DiaryExportPeriodPresetKind
{
    /// <summary>
    /// The last fourteen days.
    /// </summary>
    LastFourteenDays = 1,

    /// <summary>
    /// The last thirty days.
    /// </summary>
    LastThirtyDays = 2,

    /// <summary>
    /// The current calendar month.
    /// </summary>
    CurrentMonth = 3,

    /// <summary>
    /// The previous calendar month.
    /// </summary>
    PreviousMonth = 4
}