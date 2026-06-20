using GlucoDesk.Desktop.ViewModels.Diary.Enums;

namespace GlucoDesk.Desktop.ViewModels.Diary.Options;

/// <summary>
/// Represents a user-facing diary export format option.
/// </summary>
public sealed record DiaryExportFormatOption(
    DiaryExportFormatKind Kind,
    string Label,
    string Description)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return Label;
    }
}