using GlucoDesk.Application.Cgm.History.Analytics.Requests;
using GlucoDesk.Application.Cgm.History.Analytics.Results;
using GlucoDesk.Application.Cgm.History.Analytics.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Analytics.Services;

/// <summary>
/// Provides analytics operations over local glucose history.
/// </summary>
public sealed class GlucoseHistoryAnalyticsService : IGlucoseHistoryAnalyticsService
{
    private readonly IGlucoseHistoryService _historyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryAnalyticsService"/> class.
    /// </summary>
    /// <param name="historyService">The glucose history service.</param>
    public GlucoseHistoryAnalyticsService(IGlucoseHistoryService historyService)
    {
        ArgumentNullException.ThrowIfNull(historyService);

        _historyService = historyService;
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseHistorySummaryResult>> GetSummaryAsync(
        GlucoseHistorySummaryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var historyRequest = new GlucoseHistoryRequest(request.From, request.To);

        var historyResult = await _historyService
            .GetReadingsAsync(historyRequest, cancellationToken)
            .ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return Result<GlucoseHistorySummaryResult>.Failure(historyResult.Error);
        }

        var summary = BuildSummary(request, historyResult.Value.Readings);

        return Result<GlucoseHistorySummaryResult>.Success(summary);
    }

    #region Helpers

    /// <summary>
    /// Builds a summary result from glucose readings.
    /// </summary>
    /// <param name="request">The summary request.</param>
    /// <param name="readings">The glucose readings.</param>
    /// <returns>The glucose history summary result.</returns>
    private static GlucoseHistorySummaryResult BuildSummary(
        GlucoseHistorySummaryRequest request,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        if (readings.Count == 0)
        {
            return new GlucoseHistorySummaryResult(
                request.From,
                request.To,
                readingsCount: 0,
                averageMgDl: null,
                minimumMgDl: null,
                maximumMgDl: null,
                inRangeCount: 0,
                belowRangeCount: 0,
                aboveRangeCount: 0);
        }

        var normalizedReadings = readings
            .Select(NormalizeToMgDl)
            .ToArray();

        var belowRangeCount = normalizedReadings.Count(reading => reading.GetStatus(request.TargetRange) == GlucoseStatus.Low);
        var inRangeCount = normalizedReadings.Count(reading => reading.GetStatus(request.TargetRange) == GlucoseStatus.InRange);
        var aboveRangeCount = normalizedReadings.Count(reading => reading.GetStatus(request.TargetRange) == GlucoseStatus.High);

        return new GlucoseHistorySummaryResult(
            request.From,
            request.To,
            normalizedReadings.Length,
            averageMgDl: Math.Round(normalizedReadings.Average(reading => reading.Value.Amount), 2, MidpointRounding.AwayFromZero),
            minimumMgDl: normalizedReadings.Min(reading => reading.Value.Amount),
            maximumMgDl: normalizedReadings.Max(reading => reading.Value.Amount),
            inRangeCount,
            belowRangeCount,
            aboveRangeCount);
    }

    /// <summary>
    /// Normalizes a glucose reading to mg/dL.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <returns>The normalized glucose reading.</returns>
    private static GlucoseReading NormalizeToMgDl(GlucoseReading reading)
    {
        if (reading.Value.Unit == GlucoseUnit.MgDl)
        {
            return reading;
        }

        return new GlucoseReading(
            reading.Timestamp,
            reading.Value.ConvertTo(GlucoseUnit.MgDl),
            reading.Trend,
            reading.Provider,
            reading.Freshness);
    }

    #endregion
}