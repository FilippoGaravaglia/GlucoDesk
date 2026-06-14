using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Statistics.Services;

/// <summary>
/// Provides application-level glucose statistics calculations from local history.
/// </summary>
public sealed class GlucoseStatisticsService : IGlucoseStatisticsService
{
    private readonly IGlucoseHistoryService _historyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseStatisticsService"/> class.
    /// </summary>
    /// <param name="historyService">The glucose history service.</param>
    public GlucoseStatisticsService(IGlucoseHistoryService historyService)
    {
        ArgumentNullException.ThrowIfNull(historyService);

        _historyService = historyService;
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseStatisticsResult>> CalculateAsync(
        GlucoseStatisticsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var historyRequest = new GlucoseHistoryRequest(request.From, request.To);

        var historyResult = await _historyService
            .GetReadingsAsync(historyRequest, cancellationToken)
            .ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return Result<GlucoseStatisticsResult>.Failure(historyResult.Error);
        }

        var readings = historyResult.Value.Readings;

        var filteredReadings = FilterReadings(
            readings,
            request,
            out var ignoredMockReadingsCount,
            out var ignoredDifferentUnitReadingsCount);

        var statistics = BuildStatisticsResult(
            request,
            readings.Count,
            ignoredMockReadingsCount,
            ignoredDifferentUnitReadingsCount,
            filteredReadings);

        return Result<GlucoseStatisticsResult>.Success(statistics);
    }

    #region Helpers

    /// <summary>
    /// Filters readings according to the statistics request.
    /// </summary>
    /// <param name="readings">The source readings.</param>
    /// <param name="request">The statistics request.</param>
    /// <param name="ignoredMockReadingsCount">The number of ignored Mock readings.</param>
    /// <param name="ignoredDifferentUnitReadingsCount">The number of ignored readings with a different unit.</param>
    /// <returns>The filtered readings.</returns>
    private static IReadOnlyCollection<GlucoseReading> FilterReadings(
        IReadOnlyCollection<GlucoseReading> readings,
        GlucoseStatisticsRequest request,
        out int ignoredMockReadingsCount,
        out int ignoredDifferentUnitReadingsCount)
    {
        ignoredMockReadingsCount = 0;
        ignoredDifferentUnitReadingsCount = 0;

        var filteredReadings = new List<GlucoseReading>();

        foreach (var reading in readings)
        {
            if (!request.IncludeMockData && reading.Provider is CgmProviderKind.Mock)
            {
                ignoredMockReadingsCount++;
                continue;
            }

            if (reading.Value.Unit != request.TargetRange.Unit)
            {
                ignoredDifferentUnitReadingsCount++;
                continue;
            }

            filteredReadings.Add(reading);
        }

        return filteredReadings
            .OrderBy(reading => reading.Timestamp)
            .ToArray();
    }

    /// <summary>
    /// Builds the statistics result from filtered readings.
    /// </summary>
    /// <param name="request">The statistics request.</param>
    /// <param name="loadedReadingsCount">The number of loaded readings.</param>
    /// <param name="ignoredMockReadingsCount">The number of ignored Mock readings.</param>
    /// <param name="ignoredDifferentUnitReadingsCount">The number of ignored readings with a different unit.</param>
    /// <param name="readings">The filtered readings.</param>
    /// <returns>The statistics result.</returns>
    private static GlucoseStatisticsResult BuildStatisticsResult(
        GlucoseStatisticsRequest request,
        int loadedReadingsCount,
        int ignoredMockReadingsCount,
        int ignoredDifferentUnitReadingsCount,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        if (readings.Count == 0)
        {
            return GlucoseStatisticsResult.Empty(
                request.From,
                request.To,
                request.TargetRange.Unit,
                request.IncludeMockData,
                loadedReadingsCount,
                ignoredMockReadingsCount,
                ignoredDifferentUnitReadingsCount);
        }

        var orderedReadings = readings
            .OrderBy(reading => reading.Timestamp)
            .ToArray();

        var values = orderedReadings
            .Select(reading => reading.Value.Amount)
            .ToArray();

        var belowRangeCount = values.Count(value => value < request.TargetRange.Low);
        var inRangeCount = values.Count(value =>
            value >= request.TargetRange.Low && value <= request.TargetRange.High);
        var aboveRangeCount = values.Count(value => value > request.TargetRange.High);

        return new GlucoseStatisticsResult(
            request.From,
            request.To,
            request.TargetRange.Unit,
            request.IncludeMockData,
            loadedReadingsCount,
            orderedReadings.Length,
            ignoredMockReadingsCount,
            ignoredDifferentUnitReadingsCount,
            Math.Round(values.Average(), 1, MidpointRounding.AwayFromZero),
            values.Min(),
            values.Max(),
            belowRangeCount,
            inRangeCount,
            aboveRangeCount,
            orderedReadings.First().Timestamp,
            orderedReadings.Last().Timestamp);
    }

    #endregion
}