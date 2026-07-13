using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Summaries;

/// <summary>
/// Builds short, non-medical ambient glucose summaries for the dashboard.
/// </summary>
public static class AmbientGlucoseSummaryService
{
    private const decimal StableDeltaMgDl = 5m;
    private const decimal MeaningfulDeltaMgDl = 15m;

    /// <summary>
    /// Creates a short ambient glucose summary from the visible chart points.
    /// </summary>
    /// <param name="chartPoints">The visible chart points.</param>
    /// <param name="targetLowMgDl">The lower glucose target expressed in mg/dL.</param>
    /// <param name="targetHighMgDl">The upper glucose target expressed in mg/dL.</param>
    /// <returns>The ambient glucose summary.</returns>
    public static string CreateSummary(
        IReadOnlyCollection<GlucoseChartPoint> chartPoints,
        decimal targetLowMgDl,
        decimal targetHighMgDl)
    {
        ArgumentNullException.ThrowIfNull(chartPoints);

        if (chartPoints.Count == 0)
        {
            return T("DashboardNoRecentGlucoseData");
        }

        var orderedPoints = chartPoints
            .OrderBy(point => point.Timestamp)
            .ToArray();

        var latestPoint = orderedPoints[^1];

        if (latestPoint.ValueMgDl < targetLowMgDl)
        {
            return T("DashboardBelowTargetSentence");
        }

        if (latestPoint.ValueMgDl > targetHighMgDl)
        {
            return T("DashboardAboveTargetSentence");
        }

        if (HasRecentlyReturnedInRange(orderedPoints, targetLowMgDl, targetHighMgDl))
        {
            return T("DashboardRecentlyBackInRange");
        }

        if (orderedPoints.Length < 2)
        {
            return T("DashboardInRangeSentence");
        }

        var previousPoint = orderedPoints[^2];
        var deltaMgDl = latestPoint.ValueMgDl - previousPoint.ValueMgDl;

        if (Math.Abs(deltaMgDl) <= StableDeltaMgDl)
        {
            return T("DashboardStableAndInRange");
        }

        if (deltaMgDl >= MeaningfulDeltaMgDl)
        {
            return T("DashboardRisingStillInRange");
        }

        if (deltaMgDl <= -MeaningfulDeltaMgDl)
        {
            return T("DashboardFallingStillInRange");
        }

        return deltaMgDl > 0
            ? T("DashboardRisingSlowlyStillInRange")
            : T("DashboardFallingSlowlyStillInRange");
    }

    /// <summary>
    /// Determines whether the latest point is in range after recent out-of-range values.
    /// </summary>
    /// <param name="orderedPoints">The ordered chart points.</param>
    /// <param name="targetLowMgDl">The lower glucose target expressed in mg/dL.</param>
    /// <param name="targetHighMgDl">The upper glucose target expressed in mg/dL.</param>
    /// <returns>True when the latest point recently returned in range; otherwise, false.</returns>
    private static bool HasRecentlyReturnedInRange(
        IReadOnlyList<GlucoseChartPoint> orderedPoints,
        decimal targetLowMgDl,
        decimal targetHighMgDl)
    {
        if (orderedPoints.Count < 2)
        {
            return false;
        }

        var recentPreviousPoints = orderedPoints
            .Take(orderedPoints.Count - 1)
            .TakeLast(3);

        return recentPreviousPoints.Any(
            point => point.ValueMgDl < targetLowMgDl || point.ValueMgDl > targetHighMgDl);
    }
    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

}
