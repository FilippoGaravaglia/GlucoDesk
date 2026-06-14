using GlucoDesk.Application.Cgm.History.Analytics.Requests;
using GlucoDesk.Application.Cgm.History.Analytics.Services;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Analytics.Services;

public sealed class GlucoseHistoryAnalyticsServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ShouldCalculateSummary_WhenHistoryContainsReadings()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var historyService = new FakeGlucoseHistoryService(
            [
                CreateReading(from.AddMinutes(5), 60),
                CreateReading(from.AddMinutes(10), 100),
                CreateReading(from.AddMinutes(15), 150),
                CreateReading(from.AddMinutes(20), 190)
            ]);

        var service = new GlucoseHistoryAnalyticsService(historyService);

        var result = await service.GetSummaryAsync(
            new GlucoseHistorySummaryRequest(from, from.AddHours(1), CreateTargetRange()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.ReadingsCount);
        Assert.Equal(125m, result.Value.AverageMgDl);
        Assert.Equal(60m, result.Value.MinimumMgDl);
        Assert.Equal(190m, result.Value.MaximumMgDl);
        Assert.Equal(2, result.Value.InRangeCount);
        Assert.Equal(1, result.Value.BelowRangeCount);
        Assert.Equal(1, result.Value.AboveRangeCount);
        Assert.Equal(50m, result.Value.InRangePercentage);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnEmptySummary_WhenHistoryIsEmpty()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var service = new GlucoseHistoryAnalyticsService(new FakeGlucoseHistoryService([]));

        var result = await service.GetSummaryAsync(
            new GlucoseHistorySummaryRequest(from, from.AddHours(1), CreateTargetRange()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasReadings);
        Assert.Equal(0, result.Value.ReadingsCount);
        Assert.Null(result.Value.AverageMgDl);
        Assert.Equal(0m, result.Value.InRangePercentage);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnFailure_WhenHistoryServiceFails()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var expectedError = new Error("History.LoadFailed", "Unable to load history.");

        var service = new GlucoseHistoryAnalyticsService(
            new FakeGlucoseHistoryService(Result<GlucoseHistoryResult>.Failure(expectedError)));

        var result = await service.GetSummaryAsync(
            new GlucoseHistorySummaryRequest(from, from.AddHours(1), CreateTargetRange()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    #region Helpers

    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        private readonly Result<GlucoseHistoryResult> _result;

        /// <inheritdoc />
        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<GlucoseHistorySaveResult>.Success(
                    new GlucoseHistorySaveResult(
                        CgmProviderKind.Unknown,
                        readings.Count,
                        readings.Count,
                        0,
                        readings.Count)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGlucoseHistoryService"/> class.
        /// </summary>
        /// <param name="readings">The fake history readings.</param>
        public FakeGlucoseHistoryService(IReadOnlyCollection<GlucoseReading> readings)
            : this(Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult(readings)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGlucoseHistoryService"/> class.
        /// </summary>
        /// <param name="result">The fake history result.</param>
        public FakeGlucoseHistoryService(Result<GlucoseHistoryResult> result)
        {
            _result = result;
        }

        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    /// <summary>
    /// Creates a glucose reading.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="amount">The glucose amount in mg/dL.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal amount)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(amount, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    /// <summary>
    /// Creates the test target range.
    /// </summary>
    /// <returns>The target glucose range.</returns>
    private static GlucoseRange CreateTargetRange()
    {
        return new GlucoseRange(
            new GlucoseValue(70, GlucoseUnit.MgDl),
            new GlucoseValue(180, GlucoseUnit.MgDl));
    }

    #endregion
}