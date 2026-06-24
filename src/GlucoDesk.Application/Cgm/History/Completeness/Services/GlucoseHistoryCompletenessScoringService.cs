using GlucoDesk.Application.Cgm.History.Completeness.Enums;
using GlucoDesk.Application.Cgm.History.Completeness.Results;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Cgm.History.Completeness.Services;

/// <summary>
/// Calculates user-facing local glucose history completeness scores.
/// </summary>
public sealed class GlucoseHistoryCompletenessScoringService : IGlucoseHistoryCompletenessScoringService
{
    private const decimal CompleteCoverageThreshold = 99.5m;
    private const decimal ReliableCoverageThreshold = 90m;
    private const decimal PartialCoverageThreshold = 50m;

    /// <inheritdoc />
    public GlucoseHistoryCompletenessScore Calculate(GlucoseHistoryContinuityReport continuityReport)
    {
        ArgumentNullException.ThrowIfNull(continuityReport);

        var detectedGapCount = continuityReport.Gaps.Count;
        var normalizedCoverage = NormalizeCoverage(continuityReport.DataCoveragePercentage);
        var level = CalculateLevel(
            continuityReport.ReadingsCount,
            normalizedCoverage,
            detectedGapCount);

        return new GlucoseHistoryCompletenessScore(
            continuityReport.ReadingsCount,
            EstimateExpectedReadingsCount(
                continuityReport.ReadingsCount,
                normalizedCoverage),
            normalizedCoverage,
            detectedGapCount,
            level,
            BuildStatusText(level),
            BuildDetailText(level));
    }

    #region Helpers

    /// <summary>
    /// Calculates the completeness level.
    /// </summary>
    /// <param name="readingsCount">The available readings count.</param>
    /// <param name="dataCoveragePercentage">The data coverage percentage.</param>
    /// <param name="detectedGapCount">The detected gap count.</param>
    /// <returns>The completeness level.</returns>
    private static GlucoseHistoryCompletenessLevel CalculateLevel(
        int readingsCount,
        decimal dataCoveragePercentage,
        int detectedGapCount)
    {
        if (readingsCount == 0 || dataCoveragePercentage <= 0m)
        {
            return GlucoseHistoryCompletenessLevel.Empty;
        }

        if (dataCoveragePercentage >= CompleteCoverageThreshold && detectedGapCount == 0)
        {
            return GlucoseHistoryCompletenessLevel.Complete;
        }

        if (dataCoveragePercentage >= ReliableCoverageThreshold)
        {
            return GlucoseHistoryCompletenessLevel.Reliable;
        }

        if (dataCoveragePercentage >= PartialCoverageThreshold)
        {
            return GlucoseHistoryCompletenessLevel.Partial;
        }

        return GlucoseHistoryCompletenessLevel.Poor;
    }

    /// <summary>
    /// Estimates the expected number of readings from available readings and coverage.
    /// </summary>
    /// <param name="availableReadingsCount">The available readings count.</param>
    /// <param name="dataCoveragePercentage">The data coverage percentage.</param>
    /// <returns>The estimated expected readings count.</returns>
    private static int EstimateExpectedReadingsCount(
        int availableReadingsCount,
        decimal dataCoveragePercentage)
    {
        if (availableReadingsCount <= 0 || dataCoveragePercentage <= 0m)
        {
            return 0;
        }

        var estimated = availableReadingsCount * 100m / dataCoveragePercentage;

        return (int)Math.Ceiling(estimated);
    }

    /// <summary>
    /// Normalizes a coverage percentage to a safe display and scoring range.
    /// </summary>
    /// <param name="dataCoveragePercentage">The raw coverage percentage.</param>
    /// <returns>The normalized coverage percentage.</returns>
    private static decimal NormalizeCoverage(decimal dataCoveragePercentage)
    {
        return Math.Round(
            Math.Clamp(dataCoveragePercentage, 0m, 100m),
            2,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Builds the short user-facing status text.
    /// </summary>
    /// <param name="level">The completeness level.</param>
    /// <returns>The status text.</returns>
    private static string BuildStatusText(GlucoseHistoryCompletenessLevel level)
    {
        return level switch
        {
            GlucoseHistoryCompletenessLevel.Complete => "Complete",
            GlucoseHistoryCompletenessLevel.Reliable => "Reliable",
            GlucoseHistoryCompletenessLevel.Partial => "Partial",
            GlucoseHistoryCompletenessLevel.Poor => "Poor",
            GlucoseHistoryCompletenessLevel.Empty => "No local history",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Builds the detailed user-facing explanation.
    /// </summary>
    /// <param name="level">The completeness level.</param>
    /// <returns>The detail text.</returns>
    private static string BuildDetailText(GlucoseHistoryCompletenessLevel level)
    {
        return level switch
        {
            GlucoseHistoryCompletenessLevel.Complete =>
                "Local history appears complete for the selected period.",
            GlucoseHistoryCompletenessLevel.Reliable =>
                "Local history is mostly complete, but minor gaps or missing readings may exist.",
            GlucoseHistoryCompletenessLevel.Partial =>
                "Local history is partially complete. Interpret summaries with caution.",
            GlucoseHistoryCompletenessLevel.Poor =>
                "Local history has limited coverage. Summary quality is low.",
            GlucoseHistoryCompletenessLevel.Empty =>
                "No local glucose history is available for the selected period.",
            _ =>
                "Local history completeness could not be determined."
        };
    }

    #endregion
}
