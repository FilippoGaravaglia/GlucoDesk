using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillPlanQueryServiceTests
{
    [Fact]
    public async Task CreatePlanAsync_ShouldCreateBackfillPlanFromContinuityGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var continuityService = new FakeGlucoseHistoryContinuityQueryService
        {
            Report = CreateContinuityReport(
                startsAt,
                endsAt,
                [
                    CreateGap(
                        startsAt.AddHours(1),
                        startsAt.AddHours(2))
                ])
        };

        var planService = new FakeBackfillPlanService();

        var service = new CgmBackfillPlanQueryService(
            continuityService,
            planService);

        // Act
        var result = await service.CreatePlanAsync(
            new CgmBackfillPlanFromHistoryRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, continuityService.QueryCount);
        Assert.Equal(1, planService.PlanCount);
        Assert.NotNull(planService.LastRequest);
        Assert.Single(planService.LastRequest.DetectedGaps);
        Assert.True(result.Value.CanBackfill);

        var detectedGap = planService.LastRequest.DetectedGaps.Single();

        Assert.Equal(startsAt.AddHours(1), detectedGap.StartsAt);
        Assert.Equal(startsAt.AddHours(2), detectedGap.EndsAt);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldCreateEmptyPlan_WhenContinuityHasNoGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var continuityService = new FakeGlucoseHistoryContinuityQueryService
        {
            Report = CreateContinuityReport(
                startsAt,
                endsAt,
                [])
        };

        var planService = new FakeBackfillPlanService();

        var service = new CgmBackfillPlanQueryService(
            continuityService,
            planService);

        // Act
        var result = await service.CreatePlanAsync(
            new CgmBackfillPlanFromHistoryRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, continuityService.QueryCount);
        Assert.Equal(1, planService.PlanCount);
        Assert.NotNull(planService.LastRequest);
        Assert.Empty(planService.LastRequest.DetectedGaps);
        Assert.False(result.Value.CanBackfill);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnFailure_WhenContinuityQueryFails()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillPlanQueryService(
            new FakeGlucoseHistoryContinuityQueryService
            {
                ShouldFail = true
            },
            new FakeBackfillPlanService());

        // Act
        var result = await service.CreatePlanAsync(
            new CgmBackfillPlanFromHistoryRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("History.ContinuityFailed", result.Error.Code);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnFailure_WhenRequestWindowIsInvalid()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt;

        var service = new CgmBackfillPlanQueryService(
            new FakeGlucoseHistoryContinuityQueryService(),
            new FakeBackfillPlanService());

        // Act
        var result = await service.CreatePlanAsync(
            new CgmBackfillPlanFromHistoryRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.InvalidHistoryWindow", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a continuity report used by the tests.
    /// </summary>
    /// <param name="startsAt">The report start timestamp.</param>
    /// <param name="endsAt">The report end timestamp.</param>
    /// <param name="gaps">The continuity gaps.</param>
    /// <returns>The continuity report.</returns>
    private static GlucoseHistoryContinuityReport CreateContinuityReport(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        IReadOnlyCollection<GlucoseHistoryGap> gaps)
    {
        return new GlucoseHistoryContinuityReport(
            startsAt,
            endsAt,
            readingsCount: 10,
            dataCoveragePercentage: gaps.Count == 0 ? 100m : 80m,
            gaps);
    }

    /// <summary>
    /// Creates a glucose history gap used by the tests.
    /// </summary>
    /// <param name="startsAt">The gap start timestamp.</param>
    /// <param name="endsAt">The gap end timestamp.</param>
    /// <returns>The glucose history gap.</returns>
    private static GlucoseHistoryGap CreateGap(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return new GlucoseHistoryGap(
            GlucoseHistoryGapKind.BetweenReadings,
            startsAt,
            endsAt,
            estimatedMissingReadings: 12);
    }

    private sealed class FakeGlucoseHistoryContinuityQueryService : IGlucoseHistoryContinuityQueryService
    {
        public int QueryCount { get; private set; }

        public bool ShouldFail { get; init; }

        public GlucoseHistoryContinuityReport Report { get; init; } =
            CreateContinuityReport(
                new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero),
                []);

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryContinuityReport>> AnalyzeLocalHistoryAsync(
            GlucoseHistoryContinuityRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            QueryCount++;

            if (ShouldFail)
            {
                return Task.FromResult(Result<GlucoseHistoryContinuityReport>.Failure(
                    new Error(
                        "History.ContinuityFailed",
                        "Unable to analyze local history continuity.")));
            }

            return Task.FromResult(Result<GlucoseHistoryContinuityReport>.Success(Report));
        }
    }

    private sealed class FakeBackfillPlanService : ICgmBackfillPlanService
    {
        public int PlanCount { get; private set; }

        public CgmBackfillPlanRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<CgmBackfillPlan>> CreatePlanAsync(
            CgmBackfillPlanRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            PlanCount++;
            LastRequest = request;

            return Task.FromResult(Result<CgmBackfillPlan>.Success(
                new CgmBackfillPlan(
                    CanBackfill: request.DetectedGaps.Count > 0,
                    RequestedStartsAt: request.StartsAt,
                    RequestedEndsAt: request.EndsAt,
                    RecoverableFrom: request.StartsAt,
                    RecoverableTo: request.EndsAt,
                    RecoverableGaps: request.DetectedGaps
                        .Select(gap => new CgmBackfillPlanGap(
                            OriginalStartsAt: gap.StartsAt,
                            OriginalEndsAt: gap.EndsAt,
                            StartsAt: gap.StartsAt,
                            EndsAt: gap.EndsAt,
                            WasClampedByMaximumLookback: false))
                        .ToArray(),
                    IgnoredGapsCount: 0,
                    Message: "Test backfill plan.")));
        }
    }

    #endregion
}