using GlucoDesk.Application.Cgm.Diary.Patterns.Results;
using GlucoDesk.Application.Cgm.Diary.Results;

namespace GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;

/// <summary>
/// Analyzes glycemic diary reports to detect local recurring glucose patterns.
/// </summary>
public interface IGlycemicDiaryPatternAnalysisService
{
    /// <summary>
    /// Analyzes a glycemic diary report and detects local recurring patterns.
    /// </summary>
    /// <param name="report">The glycemic diary report.</param>
    /// <returns>The detected pattern analysis.</returns>
    GlycemicDiaryPatternAnalysis Analyze(GlycemicDiaryReport report);
}
