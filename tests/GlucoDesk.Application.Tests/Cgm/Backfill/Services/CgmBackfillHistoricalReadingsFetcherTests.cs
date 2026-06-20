using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillHistoricalReadingsFetcherTests
{
    [Fact]
    public async Task FetchAsync_ShouldReturnFetchedReadingsCount_WhenHistoricalReadingsAreLoaded()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 9, 0, 0, TimeSpan.Zero);

        var glucoseDataService = new FakeGlucoseDataService
        {
            Readings =
            [
                CreateReading(startsAt.AddMinutes(5)),
                CreateReading(startsAt.AddMinutes(10)),
                CreateReading(startsAt.AddMinutes(15))
            ]
        };

        var fetcher = new CgmBackfillHistoricalReadingsFetcher(glucoseDataService);

        // Act
        var result = await fetcher.FetchAsync(
            CreatePlanGap(startsAt, endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, glucoseDataService.HistoricalReadingsRequestCount);
        Assert.Equal(3, result.Value.ReadingsCount);
        Assert.True(result.Value.HasReadings);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnZeroReadings_WhenProviderReturnsNoHistoricalReadings()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 9, 0, 0, TimeSpan.Zero);

        var fetcher = new CgmBackfillHistoricalReadingsFetcher(
            new FakeGlucoseDataService
            {
                Readings = []
            });

        // Act
        var result = await fetcher.FetchAsync(
            CreatePlanGap(startsAt, endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.ReadingsCount);
        Assert.False(result.Value.HasReadings);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnFailure_WhenHistoricalReadingsCannotBeLoaded()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 9, 0, 0, TimeSpan.Zero);

        var fetcher = new CgmBackfillHistoricalReadingsFetcher(
            new FakeGlucoseDataService
            {
                ShouldFailHistoricalReadings = true
            });

        // Act
        var result = await fetcher.FetchAsync(
            CreatePlanGap(startsAt, endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Readings.HistoricalFailed", result.Error.Code);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnFailure_WhenGapWindowIsInvalid()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt;

        var fetcher = new CgmBackfillHistoricalReadingsFetcher(
            new FakeGlucoseDataService());

        // Act
        var result = await fetcher.FetchAsync(
            CreatePlanGap(startsAt, endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.InvalidFetchGap", result.Error.Code);
    }

    #region Helpers

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

    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        public int HistoricalReadingsRequestCount { get; private set; }

        public bool ShouldFailHistoricalReadings { get; init; }

        public IReadOnlyCollection<GlucoseReading> Readings { get; init; } = [];

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Provider metadata is not used by these tests.");
        }

        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Latest readings are not used by these tests.");
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Recent readings are not used by these tests.");
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            HistoricalReadingsRequestCount++;

            if (ShouldFailHistoricalReadings)
            {
                return Task.FromResult(Result<GlucoseReadingsResult>.Failure(
                    new Error(
                        "Readings.HistoricalFailed",
                        "Unable to load historical readings.")));
            }

            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult(
                    Readings,
                    DateTimeOffset.UtcNow)));
        }

        /// <inheritdoc />
        public Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
            GlucoseDashboardRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Dashboard snapshots are not used by these tests.");
        }
    }

    #endregion
}