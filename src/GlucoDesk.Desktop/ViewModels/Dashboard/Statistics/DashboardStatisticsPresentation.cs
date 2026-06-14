namespace GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;

/// <summary>
/// Represents user-facing dashboard statistics presentation values.
/// </summary>
/// <param name="StatusText">The statistics status text.</param>
/// <param name="AverageGlucoseText">The average glucose text.</param>
/// <param name="TimeInRangeText">The time in range text.</param>
/// <param name="BelowRangeText">The below range text.</param>
/// <param name="AboveRangeText">The above range text.</param>
/// <param name="ReadingsAnalyzedText">The analyzed readings text.</param>
/// <param name="TargetRangeText">The target range text.</param>
/// <param name="HasStatisticsData">A value indicating whether statistics data is available.</param>
public sealed record DashboardStatisticsPresentation(
    string StatusText,
    string AverageGlucoseText,
    string TimeInRangeText,
    string BelowRangeText,
    string AboveRangeText,
    string ReadingsAnalyzedText,
    string TargetRangeText,
    bool HasStatisticsData);