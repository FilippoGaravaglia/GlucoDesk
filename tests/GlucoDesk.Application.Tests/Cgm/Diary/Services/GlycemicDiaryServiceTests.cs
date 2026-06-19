using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Options;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Options;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Services;

public sealed class GlycemicDiaryServiceTests
{
    [Fact]
    public async Task CreateDiaryAsync_ShouldCreateDailyEntriesAndTimeBlocks()
    {
        // Arrange
        var periodStartsAt = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);
        var periodEndsAt = new DateTimeOffset(2026, 6, 19, 23, 59, 59, TimeSpan.Zero);

        var readings = new[]
        {
            CreateReading(new DateTimeOffset(2026, 6, 19, 8, 0, 0, TimeSpan.Zero), 110m),
            CreateReading(new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero), 150m),
            CreateReading(new DateTimeOffset(2026, 6, 19, 19, 0, 0, TimeSpan.Zero), 170m),
            CreateReading(new DateTimeOffset(2026, 6, 19, 22, 30, 0, TimeSpan.Zero), 130m)
        };

        var service = CreateService(readings);

        // Act
        var result = await service.CreateDiaryAsync(
            new GlycemicDiaryRequest(periodStartsAt, periodEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var day = Assert.Single(result.Value.DailyEntries);

        Assert.Equal(new DateOnly(2026, 6, 19), day.Date);
        Assert.Equal(4, day.ReadingsCount);
        Assert.Equal(140m, day.AverageMgDl);
        Assert.Equal(110m, day.MinimumMgDl);
        Assert.Equal(170m, day.MaximumMgDl);
        Assert.Equal(100m, day.TimeInRangePercentage);
        Assert.Equal(4, day.TimeBlocks.Count);

        Assert.Contains(
            day.TimeBlocks,
            block => block.Kind == GlycemicDiaryTimeBlockKind.Breakfast &&
                     block.RepresentativeValueMgDl == 110m);

        Assert.Contains(
            day.TimeBlocks,
            block => block.Kind == GlycemicDiaryTimeBlockKind.Lunch &&
                     block.RepresentativeValueMgDl == 150m);

        Assert.Contains(
            day.TimeBlocks,
            block => block.Kind == GlycemicDiaryTimeBlockKind.Dinner &&
                     block.RepresentativeValueMgDl == 170m);

        Assert.Contains(
            day.TimeBlocks,
            block => block.Kind == GlycemicDiaryTimeBlockKind.Bedtime &&
                     block.RepresentativeValueMgDl == 130m);
    }

    [Fact]
    public async Task CreateDiaryAsync_ShouldCreateEmptyDay_WhenNoReadingsExist()
    {
        // Arrange
        var periodStartsAt = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);
        var periodEndsAt = new DateTimeOffset(2026, 6, 19, 23, 59, 59, TimeSpan.Zero);

        var service = CreateService([]);

        // Act
        var result = await service.CreateDiaryAsync(
            new GlycemicDiaryRequest(periodStartsAt, periodEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var day = Assert.Single(result.Value.DailyEntries);

        Assert.False(day.HasData);
        Assert.False(day.IsDataComplete);
        Assert.Equal(0m, day.DataCoveragePercentage);
        Assert.All(day.TimeBlocks, block => Assert.False(block.HasData));
    }

    [Fact]
    public async Task CreateDiaryAsync_ShouldPropagateHistoryFailure()
    {
        // Arrange
        var service = new GlycemicDiaryService(
            new FailingGlucoseHistoryService(),
            new GlucoseHistoryContinuityService(HistoryContinuityOptions.Default),
            CreateOptions());

        var periodStartsAt = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);
        var periodEndsAt = new DateTimeOffset(2026, 6, 19, 23, 59, 59, TimeSpan.Zero);

        // Act
        var result = await service.CreateDiaryAsync(
            new GlycemicDiaryRequest(periodStartsAt, periodEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("History.ReadFailed", result.Error.Code);
    }

    [Fact]
    public async Task CreateDiaryAsync_ShouldCreateMultipleDays()
    {
        // Arrange
        var periodStartsAt = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);
        var periodEndsAt = new DateTimeOffset(2026, 6, 20, 23, 59, 59, TimeSpan.Zero);

        var readings = new[]
        {
            CreateReading(new DateTimeOffset(2026, 6, 19, 8, 0, 0, TimeSpan.Zero), 110m),
            CreateReading(new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero), 140m)
        };

        var service = CreateService(readings);

        // Act
        var result = await service.CreateDiaryAsync(
            new GlycemicDiaryRequest(periodStartsAt, periodEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.DailyEntries.Count);
        Assert.Contains(result.Value.DailyEntries, day => day.Date == new DateOnly(2026, 6, 19));
        Assert.Contains(result.Value.DailyEntries, day => day.Date == new DateOnly(2026, 6, 20));
    }

    #region Helpers

    /// <summary>
    /// Creates the glycemic diary service.
    /// </summary>
    /// <param name="readings">The local history readings.</param>
    /// <returns>The glycemic diary service.</returns>
    private static GlycemicDiaryService CreateService(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        return new GlycemicDiaryService(
            new FakeGlucoseHistoryService(readings),
            new GlucoseHistoryContinuityService(HistoryContinuityOptions.Default),
            CreateOptions());
    }

    /// <summary>
    /// Creates deterministic glycemic diary options for tests.
    /// </summary>
    /// <returns>The glycemic diary options.</returns>
    private static GlycemicDiaryOptions CreateOptions()
    {
        return new GlycemicDiaryOptions(
            TimeZoneInfo.Utc,
            70m,
            180m,
            [
                new GlycemicDiaryTimeBlockDefinition(
                    GlycemicDiaryTimeBlockKind.Breakfast,
                    "Breakfast",
                    new TimeOnly(6, 0),
                    new TimeOnly(10, 59, 59),
                    1),
                new GlycemicDiaryTimeBlockDefinition(
                    GlycemicDiaryTimeBlockKind.Lunch,
                    "Lunch",
                    new TimeOnly(11, 0),
                    new TimeOnly(15, 59, 59),
                    2),
                new GlycemicDiaryTimeBlockDefinition(
                    GlycemicDiaryTimeBlockKind.Dinner,
                    "Dinner",
                    new TimeOnly(18, 0),
                    new TimeOnly(21, 59, 59),
                    3),
                new GlycemicDiaryTimeBlockDefinition(
                    GlycemicDiaryTimeBlockKind.Bedtime,
                    "Pre-night",
                    new TimeOnly(22, 0),
                    new TimeOnly(23, 59, 59),
                    4)
            ]);
    }

    /// <summary>
    /// Creates a glucose reading for glycemic diary tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="valueMgDl">The glucose value in mg/dL.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal valueMgDl)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(valueMgDl, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        private readonly IReadOnlyCollection<GlucoseReading> _readings;

        public FakeGlucoseHistoryService(IReadOnlyCollection<GlucoseReading> readings)
        {
            _readings = readings;
        }

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
            return Task.FromResult(Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.Mock,
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
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Result<GlucoseHistoryResult>.Success(
                new GlucoseHistoryResult(_readings)));
        }
    }

    private sealed class FailingGlucoseHistoryService : IGlucoseHistoryService
    {
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
            return Task.FromResult(Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.Mock,
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
            return Task.FromResult(Result<GlucoseHistoryResult>.Failure(
                new Error(
                    "History.ReadFailed",
                    "Unable to read local glucose history.")));
        }
    }

    #endregion
}