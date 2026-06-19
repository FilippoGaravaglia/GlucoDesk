using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Options;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Continuity.Services;

public sealed class GlucoseHistoryContinuityQueryServiceTests
{
    [Fact]
    public async Task AnalyzeLocalHistoryAsync_ShouldReturnCompleteReport_WhenLocalHistoryIsContinuous()
    {
        // Arrange
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(20);

        var readings = new[]
        {
            CreateReading(windowStartsAt),
            CreateReading(windowStartsAt.AddMinutes(5)),
            CreateReading(windowStartsAt.AddMinutes(10)),
            CreateReading(windowStartsAt.AddMinutes(15)),
            CreateReading(windowEndsAt)
        };

        var service = CreateService(readings);

        // Act
        var result = await service.AnalyzeLocalHistoryAsync(
            new GlucoseHistoryContinuityRequest(windowStartsAt, windowEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsComplete);
        Assert.Equal(100m, result.Value.DataCoveragePercentage);
        Assert.Empty(result.Value.Gaps);
    }

    [Fact]
    public async Task AnalyzeLocalHistoryAsync_ShouldReturnGaps_WhenLocalHistoryHasMissingReadings()
    {
        // Arrange
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(30);

        var readings = new[]
        {
            CreateReading(windowStartsAt),
            CreateReading(windowStartsAt.AddMinutes(5)),
            CreateReading(windowStartsAt.AddMinutes(25)),
            CreateReading(windowEndsAt)
        };

        var service = CreateService(readings);

        // Act
        var result = await service.AnalyzeLocalHistoryAsync(
            new GlucoseHistoryContinuityRequest(windowStartsAt, windowEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsComplete);

        var gap = Assert.Single(result.Value.Gaps);

        Assert.Equal(GlucoseHistoryGapKind.BetweenReadings, gap.Kind);
        Assert.True(result.Value.DataCoveragePercentage < 100m);
    }

    [Fact]
    public async Task AnalyzeLocalHistoryAsync_ShouldPropagateFailure_WhenLocalHistoryReadFails()
    {
        // Arrange
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(30);

        var service = new GlucoseHistoryContinuityQueryService(
            new FailingGlucoseHistoryService(),
            new GlucoseHistoryContinuityService(HistoryContinuityOptions.Default));

        // Act
        var result = await service.AnalyzeLocalHistoryAsync(
            new GlucoseHistoryContinuityRequest(windowStartsAt, windowEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("History.ReadFailed", result.Error.Code);
    }

    [Fact]
    public async Task AnalyzeLocalHistoryAsync_ShouldPassRequestedWindowToHistoryService()
    {
        // Arrange
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(30);

        var historyService = new CapturingGlucoseHistoryService([]);

        var service = new GlucoseHistoryContinuityQueryService(
            historyService,
            new GlucoseHistoryContinuityService(HistoryContinuityOptions.Default));

        // Act
        _ = await service.AnalyzeLocalHistoryAsync(
            new GlucoseHistoryContinuityRequest(windowStartsAt, windowEndsAt),
            CancellationToken.None);

        // Assert
        Assert.NotNull(historyService.CapturedRequest);
        Assert.Equal(windowStartsAt, historyService.CapturedRequest.From);
        Assert.Equal(windowEndsAt, historyService.CapturedRequest.To);
    }

    #region Helpers

    /// <summary>
    /// Creates the glucose history continuity query service.
    /// </summary>
    /// <param name="readings">The readings returned by local history.</param>
    /// <returns>The glucose history continuity query service.</returns>
    private static GlucoseHistoryContinuityQueryService CreateService(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        return new GlucoseHistoryContinuityQueryService(
            new CapturingGlucoseHistoryService(readings),
            new GlucoseHistoryContinuityService(HistoryContinuityOptions.Default));
    }

    /// <summary>
    /// Creates a glucose reading for history continuity query tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120m, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    private sealed class CapturingGlucoseHistoryService : IGlucoseHistoryService
    {
        private readonly IReadOnlyCollection<GlucoseReading> _readings;

        public CapturingGlucoseHistoryService(IReadOnlyCollection<GlucoseReading> readings)
        {
            _readings = readings;
        }

        public GlucoseHistoryRequest? CapturedRequest { get; private set; }

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

            CapturedRequest = request;

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