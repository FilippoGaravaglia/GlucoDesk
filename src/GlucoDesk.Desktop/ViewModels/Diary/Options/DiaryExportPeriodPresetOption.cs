using GlucoDesk.Desktop.ViewModels.Diary.Enums;

namespace GlucoDesk.Desktop.ViewModels.Diary.Options;

/// <summary>
/// Represents a user-facing diary export period preset.
/// </summary>
public sealed record DiaryExportPeriodPresetOption(
    DiaryExportPeriodPresetKind Kind,
    string Label,
    string Description)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return Label;
    }
}