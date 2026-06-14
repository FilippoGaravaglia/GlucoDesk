using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Services;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Statistics.Services;

public sealed class GlucoseStatisticsServiceTests
{
    [Fact]
    public async Task CalculateAsync_ShouldCalculateStatistics_FromHistoryReadings()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService(
        [
            CreateReading(from.AddMinutes(0), 60, CgmProviderKind.Nightscout, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(5), 100, CgmProviderKind.Nightscout, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(10), 150, CgmProviderKind.Nightscout, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(15), 220, CgmProviderKind.Nightscout, GlucoseUnit.MgDl)
        ]);

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                new GlucoseStatisticsTargetRange(70, 180, GlucoseUnit.MgDl)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasData);
        Assert.Equal(4, result.Value.LoadedReadingsCount);
        Assert.Equal(4, result.Value.AnalyzedReadingsCount);
        Assert.Equal(132.5m, result.Value.AverageGlucose);
        Assert.Equal(60, result.Value.MinimumGlucose);
        Assert.Equal(220, result.Value.MaximumGlucose);
        Assert.Equal(1, result.Value.BelowRangeCount);
        Assert.Equal(2, result.Value.InRangeCount);
        Assert.Equal(1, result.Value.AboveRangeCount);
        Assert.Equal(25, result.Value.BelowRangePercentage);
        Assert.Equal(50, result.Value.InRangePercentage);
        Assert.Equal(25, result.Value.AboveRangePercentage);
    }

    [Fact]
    public async Task CalculateAsync_ShouldExcludeMockReadings_ByDefault()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService(
        [
            CreateReading(from.AddMinutes(0), 100, CgmProviderKind.Mock, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(5), 150, CgmProviderKind.Nightscout, GlucoseUnit.MgDl)
        ]);

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                GlucoseStatisticsTargetRange.DefaultMgDl()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.LoadedReadingsCount);
        Assert.Equal(1, result.Value.AnalyzedReadingsCount);
        Assert.Equal(1, result.Value.IgnoredMockReadingsCount);
        Assert.Equal(150, result.Value.AverageGlucose);
    }

    [Fact]
    public async Task CalculateAsync_ShouldIncludeMockReadings_WhenRequested()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService(
        [
            CreateReading(from.AddMinutes(0), 100, CgmProviderKind.Mock, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(5), 150, CgmProviderKind.Nightscout, GlucoseUnit.MgDl)
        ]);

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                GlucoseStatisticsTargetRange.DefaultMgDl(),
                includeMockData: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.LoadedReadingsCount);
        Assert.Equal(2, result.Value.AnalyzedReadingsCount);
        Assert.Equal(0, result.Value.IgnoredMockReadingsCount);
        Assert.Equal(125, result.Value.AverageGlucose);
    }

    [Fact]
    public async Task CalculateAsync_ShouldIgnoreReadingsWithDifferentUnit()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService(
        [
            CreateReading(from.AddMinutes(0), 100, CgmProviderKind.Nightscout, GlucoseUnit.MgDl),
            CreateReading(from.AddMinutes(5), 6, CgmProviderKind.Nightscout, GlucoseUnit.MmolL)
        ]);

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                GlucoseStatisticsTargetRange.DefaultMgDl()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.LoadedReadingsCount);
        Assert.Equal(1, result.Value.AnalyzedReadingsCount);
        Assert.Equal(1, result.Value.IgnoredDifferentUnitReadingsCount);
        Assert.Equal(100, result.Value.AverageGlucose);
    }

    [Fact]
    public async Task CalculateAsync_ShouldReturnEmptyStatistics_WhenNoReadingsCanBeAnalyzed()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService(
        [
            CreateReading(from.AddMinutes(0), 100, CgmProviderKind.Mock, GlucoseUnit.MgDl)
        ]);

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                GlucoseStatisticsTargetRange.DefaultMgDl()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasData);
        Assert.Equal(1, result.Value.LoadedReadingsCount);
        Assert.Equal(0, result.Value.AnalyzedReadingsCount);
        Assert.Equal(1, result.Value.IgnoredMockReadingsCount);
        Assert.Null(result.Value.AverageGlucose);
    }

    [Fact]
    public async Task CalculateAsync_ShouldPropagateHistoryFailure()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var historyService = new FakeGlucoseHistoryService([])
        {
            HistoryResult = Result<GlucoseHistoryResult>.Failure(
                new Error("History.LoadFailed", "Unable to load glucose history."))
        };

        var service = new GlucoseStatisticsService(historyService);

        var result = await service.CalculateAsync(
            new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                GlucoseStatisticsTargetRange.DefaultMgDl()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("History.LoadFailed", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Fake glucose history service used by statistics service tests.
    /// </summary>
    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        private readonly IReadOnlyCollection<GlucoseReading> _readings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGlucoseHistoryService"/> class.
        /// </summary>
        /// <param name="readings">The readings returned by local history.</param>
        public FakeGlucoseHistoryService(IReadOnlyCollection<GlucoseReading> readings)
        {
            ArgumentNullException.ThrowIfNull(readings);

            _readings = readings;
            HistoryResult = Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult(readings));
        }

        /// <summary>
        /// Gets or sets the history result returned by the fake service.
        /// </summary>
        public Result<GlucoseHistoryResult> HistoryResult { get; set; }

        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

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

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(HistoryResult);
        }
    }

    /// <summary>
    /// Creates a glucose reading for statistics service tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="amount">The glucose amount.</param>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal amount,
        CgmProviderKind providerKind,
        GlucoseUnit unit)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(amount, unit),
            TrendDirection.Flat,
            providerKind,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}