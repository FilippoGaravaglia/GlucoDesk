using GlucoDesk.Application.Cgm.History.Completeness.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;

/// <summary>
/// Calculates a user-facing completeness score from local glucose history continuity data.
/// </summary>
public interface IGlucoseHistoryCompletenessScoringService
{
    /// <summary>
    /// Calculates a user-facing completeness score from a continuity report.
    /// </summary>
    /// <param name="continuityReport">The local glucose history continuity report.</param>
    /// <returns>The completeness score.</returns>
    GlucoseHistoryCompletenessScore Calculate(GlucoseHistoryContinuityReport continuityReport);
}
