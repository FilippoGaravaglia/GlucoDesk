using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillRunServiceTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnPlanned_WhenPlanHasRecoverableGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillRunService(
            new FakeBackfillPlanQueryService
            {
                Plan = CreatePlan(
                    startsAt,
                    endsAt,
                    [
                        new CgmBackfillPlanGap(
                            OriginalStartsAt: startsAt.AddHours(1),
                            OriginalEndsAt: startsAt.AddHours(2),
                            StartsAt: startsAt.AddHours(1),
                            EndsAt: startsAt.AddHours(2),
                            WasClampedByMaximumLookback: false)
                    ])
            });

        // Act
        var result = await service.RunAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmBackfillRunStatus.Planned, result.Value.Status);
        Assert.True(result.Value.HasRecoverableGaps);
        Assert.Equal(1, result.Value.RecoverableGapsCount);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnSkipped_WhenPlanHasNoRecoverableGaps()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillRunService(
            new FakeBackfillPlanQueryService
            {
                Plan = CreatePlan(
                    startsAt,
                    endsAt,
                    [])
            });

        // Act
        var result = await service.RunAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmBackfillRunStatus.SkippedNoRecoverableGaps, result.Value.Status);
        Assert.False(result.Value.HasRecoverableGaps);
        Assert.Equal(0, result.Value.RecoverableGapsCount);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnFailure_WhenPlanQueryFails()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        var service = new CgmBackfillRunService(
            new FakeBackfillPlanQueryService
            {
                ShouldFail = true
            });

        // Act
        var result = await service.RunAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.PlanQueryFailed", result.Error.Code);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnFailure_WhenRequestWindowIsInvalid()
    {
        // Arrange
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt;

        var service = new CgmBackfillRunService(
            new FakeBackfillPlanQueryService());

        // Act
        var result = await service.RunAsync(
            new CgmBackfillRunRequest(
                startsAt,
                endsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.InvalidRunWindow", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a backfill plan used by the tests.
    /// </summary>
    /// <param name="startsAt">The requested start timestamp.</param>
    /// <param name="endsAt">The requested end timestamp.</param>
    /// <param name="gaps">The recoverable gaps.</param>
    /// <returns>The backfill plan.</returns>
    private static CgmBackfillPlan CreatePlan(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        IReadOnlyCollection<CgmBackfillPlanGap> gaps)
    {
        return new CgmBackfillPlan(
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
    }

    private sealed class FakeBackfillPlanQueryService : ICgmBackfillPlanQueryService
    {
        public bool ShouldFail { get; init; }

        public CgmBackfillPlan Plan { get; init; } =
            CreatePlan(
                new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero),
                []);

        /// <inheritdoc />
        public Task<Result<CgmBackfillPlan>> CreatePlanAsync(
            CgmBackfillPlanFromHistoryRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFail)
            {
                return Task.FromResult(Result<CgmBackfillPlan>.Failure(
                    new Error(
                        "Backfill.PlanQueryFailed",
                        "Unable to create backfill plan.")));
            }

            return Task.FromResult(Result<CgmBackfillPlan>.Success(Plan));
        }
    }

    #endregion
}