using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillExecutionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldFetchReadings_ForEachRecoverableGap()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var firstGap = CreatePlanGap(
            startsAt.AddHours(1),
            startsAt.AddHours(2));

        var secondGap = CreatePlanGap(
            startsAt.AddHours(3),
            startsAt.AddHours(4));

        var fetcher = new FakeHistoricalReadingsFetcher
        {
            ReadingsCountByGapStart =
            {
                [firstGap.StartsAt] = 5,
                [secondGap.StartsAt] = 3
            }
        };

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService
            {
                Run = CreateRun(
                    startsAt,
                    endsAt,
                    [firstGap, secondGap])
            },
            fetcher);

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmBackfillExecutionStatus.Completed, result.Value.Status);
        Assert.Equal(2, fetcher.FetchCount);
        Assert.Equal(2, result.Value.FetchedGaps.Count);
        Assert.Equal(8, result.Value.TotalFetchedReadings);
        Assert.True(result.Value.HasFetchedReadings);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipFetcher_WhenRunHasNoRecoverableGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var fetcher = new FakeHistoricalReadingsFetcher();

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService
            {
                Run = CreateRun(
                    startsAt,
                    endsAt,
                    [])
            },
            fetcher);

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmBackfillExecutionStatus.SkippedNoRecoverableGaps, result.Value.Status);
        Assert.Equal(0, fetcher.FetchCount);
        Assert.Empty(result.Value.FetchedGaps);
        Assert.Equal(0, result.Value.TotalFetchedReadings);
        Assert.False(result.Value.HasFetchedReadings);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenRunFails()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService
            {
                ShouldFail = true
            },
            new FakeHistoricalReadingsFetcher());

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.RunFailed", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenFetcherFails()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService
            {
                Run = CreateRun(
                    startsAt,
                    endsAt,
                    [
                        CreatePlanGap(
                            startsAt.AddHours(1),
                            startsAt.AddHours(2))
                    ])
            },
            new FakeHistoricalReadingsFetcher
            {
                ShouldFail = true
            });

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.FetchFailed", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenRequestWindowIsInvalid()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt;

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService(),
            new FakeHistoricalReadingsFetcher());

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.InvalidExecutionWindow", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExposeFetchedReadingsAcrossFetchedGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var firstGap = CreatePlanGap(
            startsAt.AddHours(1),
            startsAt.AddHours(2));

        var secondGap = CreatePlanGap(
            startsAt.AddHours(3),
            startsAt.AddHours(4));

        var firstReading = CreateReading(firstGap.StartsAt.AddMinutes(5));
        var secondReading = CreateReading(secondGap.StartsAt.AddMinutes(5));

        var fetcher = new FakeHistoricalReadingsFetcher
        {
            ReadingsByGapStart =
            {
                [firstGap.StartsAt] = [firstReading],
                [secondGap.StartsAt] = [secondReading]
            }
        };

        var service = new CgmBackfillExecutionService(
            new FakeBackfillRunService
            {
                Run = CreateRun(
                    startsAt,
                    endsAt,
                    [firstGap, secondGap])
            },
            fetcher);

        // Act
        var result = await service.ExecuteAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalFetchedReadings);
        Assert.Equal(2, result.Value.FetchedReadings.Count);
        Assert.Contains(firstReading, result.Value.FetchedReadings);
        Assert.Contains(secondReading, result.Value.FetchedReadings);
    }

    #region Helpers

    /// <summary>
    /// Creates a backfill run result used by the tests.
    /// </summary>
    /// <param name="startsAt">The requested start timestamp.</param>
    /// <param name="endsAt">The requested end timestamp.</param>
    /// <param name="gaps">The recoverable gaps.</param>
    /// <returns>The backfill run result.</returns>
    private static CgmBackfillRunResult CreateRun(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        IReadOnlyCollection<CgmBackfillPlanGap> gaps)
    {
        var plan = new CgmBackfillPlan(
            CanBackfill: gaps.Count > 0,
            RequestedStartsAt: startsAt,
            RequestedEndsAt: endsAt,
            RecoverableFrom: startsAt,
            RecoverableTo: endsAt,
            RecoverableGaps: gaps,
            IgnoredGapsCount: 0,
            Message: gaps.Count > 0
                ? "Recoverable gaps found."
                : "No recoverable gaps found.");

        return new CgmBackfillRunResult(
            gaps.Count > 0
                ? CgmBackfillRunStatus.Planned
                : CgmBackfillRunStatus.SkippedNoRecoverableGaps,
            plan,
            RecoverableGapsCount: gaps.Count,
            Message: gaps.Count > 0
                ? "Backfill run can be attempted."
                : "No recoverable gaps were found.");
    }

    /// <summary>
    /// Creates a planned backfill gap used by the tests.
    /// </summary>
    /// <param name="startsAt">The planned gap start timestamp.</param>
    /// <param name="endsAt">The planned gap end timestamp.</param>
    /// <returns>The planned backfill gap.</returns>
    private static CgmBackfillPlanGap CreatePlanGap(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return new CgmBackfillPlanGap(
            OriginalStartsAt: startsAt,
            OriginalEndsAt: endsAt,
            StartsAt: startsAt,
            EndsAt: endsAt,
            WasClampedByMaximumLookback: false);
    }

    private sealed class FakeBackfillRunService : ICgmBackfillRunService
    {
        public bool ShouldFail { get; init; }

        public CgmBackfillRunResult Run { get; init; } =
            CreateRun(
                new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero),
                []);

        /// <inheritdoc />
        public Task<Result<CgmBackfillRunResult>> RunAsync(
            CgmBackfillRunRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFail)
            {
                return Task.FromResult(Result<CgmBackfillRunResult>.Failure(
                    new Error(
                        "Backfill.RunFailed",
                        "Unable to run backfill orchestration.")));
            }

            return Task.FromResult(Result<CgmBackfillRunResult>.Success(Run));
        }
    }

    private sealed class FakeHistoricalReadingsFetcher : ICgmBackfillHistoricalReadingsFetcher
    {
        public int FetchCount { get; private set; }

        public bool ShouldFail { get; init; }

        public Dictionary<DateTimeOffset, int> ReadingsCountByGapStart { get; } = [];

        public Dictionary<DateTimeOffset, IReadOnlyCollection<GlucoseReading>> ReadingsByGapStart { get; } = [];

        /// <inheritdoc />
        public Task<Result<CgmBackfillFetchedGapResult>> FetchAsync(
            CgmBackfillPlanGap gap,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gap);
            cancellationToken.ThrowIfCancellationRequested();

            FetchCount++;

            if (ShouldFail)
            {
                return Task.FromResult(Result<CgmBackfillFetchedGapResult>.Failure(
                    new Error(
                        "Backfill.FetchFailed",
                        "Unable to fetch historical readings.")));
            }

            if (ReadingsByGapStart.TryGetValue(gap.StartsAt, out var readings))
            {
                return Task.FromResult(Result<CgmBackfillFetchedGapResult>.Success(
                    new CgmBackfillFetchedGapResult(
                        gap,
                        readings)));
            }
            
            var readingsCount = ReadingsCountByGapStart.GetValueOrDefault(gap.StartsAt);
            
            return Task.FromResult(Result<CgmBackfillFetchedGapResult>.Success(
                new CgmBackfillFetchedGapResult(
                    gap,
                    readingsCount)));
        }
    }

    /// <summary>
    /// Creates a glucose reading used by the tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Historical);
    }

    #endregion
}