using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Defines operations for analyzing local glucose history continuity.
/// </summary>
public interface IGlucoseHistoryContinuityService
{
    /// <summary>
    /// Analyzes the continuity of a local glucose history window.
    /// </summary>
    /// <param name="readings">The readings to analyze.</param>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    /// <returns>The glucose history continuity report.</returns>
    Result<GlucoseHistoryContinuityReport> AnalyzeWindow(
        IReadOnlyCollection<GlucoseReading> readings,
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt);
}