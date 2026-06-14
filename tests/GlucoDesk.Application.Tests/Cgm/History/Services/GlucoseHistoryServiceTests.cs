using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Services;

public sealed class GlucoseHistoryServiceTests
{
    [Fact]
    public async Task SaveReadingsAsync_ShouldDelegateToStore()
    {
        var store = new FakeGlucoseHistoryStore();
        var service = new GlucoseHistoryService(store);
        var reading = CreateReading(new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero));

        var result = await service.SaveReadingsAsync([reading], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(store.SavedReadings);
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldDelegateToStore()
    {
        var readings = new[]
        {
            CreateReading(new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero))
        };

        var expectedResult = new GlucoseHistorySaveResult(
            CgmProviderKind.Mock,
            1,
            1,
            0,
            1);

        var store = new FakeGlucoseHistoryStore
        {
            SaveWithSummaryResult = Result<GlucoseHistorySaveResult>.Success(expectedResult)
        };

        var service = new GlucoseHistoryService(store);

        var result = await service.SaveReadingsWithSummaryAsync(readings, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResult, result.Value);
        Assert.Single(store.SavedReadingsWithSummary);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldDelegateToStore()
    {
        var store = new FakeGlucoseHistoryStore();
        var service = new GlucoseHistoryService(store);
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var request = new GlucoseHistoryRequest(from, from.AddHours(1));

        var result = await service.GetReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(request, store.LastRequest);
    }

    #region Helpers

    /// <summary>
    /// Fake glucose history store used by application service tests.
    /// </summary>
    private sealed class FakeGlucoseHistoryStore : IGlucoseHistoryStore
    {
        /// <summary>
        /// Gets the saved readings.
        /// </summary>
        public IReadOnlyCollection<GlucoseReading> SavedReadings { get; private set; } = [];

        /// <summary>
        /// Gets the readings saved through the detailed summary method.
        /// </summary>
        public IReadOnlyCollection<GlucoseReading> SavedReadingsWithSummary { get; private set; } = [];

        /// <summary>
        /// Gets or sets the detailed save result returned by the fake store.
        /// </summary>
        public Result<GlucoseHistorySaveResult> SaveWithSummaryResult { get; set; } =
            Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.Unknown,
                    0,
                    0,
                    0,
                    0));

        /// <summary>
        /// Gets the last history request.
        /// </summary>
        public GlucoseHistoryRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;

            return Task.FromResult(Result.Success());
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadingsWithSummary = readings;

            return Task.FromResult(SaveWithSummaryResult);
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult([])));
        }
    }

    /// <summary>
    /// Creates a glucose reading for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(110, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}