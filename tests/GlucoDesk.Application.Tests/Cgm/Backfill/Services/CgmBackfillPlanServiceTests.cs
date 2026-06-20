using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillPlanServiceTests
{
    [Fact]
    public async Task CreatePlanAsync_ShouldReturnRecoverableGap_WhenGapIsWithinLookback()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                Capability = CreateSupportedCapability()
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now.AddHours(-6),
            now,
            [
                new CgmBackfillDetectedGap(
                    now.AddHours(-4),
                    now.AddHours(-3))
            ]);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanBackfill);
        Assert.Single(result.Value.RecoverableGaps);
        Assert.Equal(TimeSpan.FromHours(1), result.Value.RecoverableGaps.Single().Duration);
        Assert.False(result.Value.RecoverableGaps.Single().WasClampedByMaximumLookback);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldClampGap_WhenGapStartsBeforeMaximumLookback()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                Capability = CreateSupportedCapability()
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now.AddHours(-48),
            now,
            [
                new CgmBackfillDetectedGap(
                    now.AddHours(-30),
                    now.AddHours(-23))
            ]);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanBackfill);

        var gap = result.Value.RecoverableGaps.Single();

        Assert.Equal(now.AddHours(-24), gap.StartsAt);
        Assert.Equal(now.AddHours(-23), gap.EndsAt);
        Assert.True(gap.WasClampedByMaximumLookback);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldIgnoreGap_WhenGapIsShorterThanMinimumDuration()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                Capability = CreateSupportedCapability()
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now.AddHours(-1),
            now,
            [
                new CgmBackfillDetectedGap(
                    now.AddMinutes(-20),
                    now.AddMinutes(-15))
            ]);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanBackfill);
        Assert.Empty(result.Value.RecoverableGaps);
        Assert.Equal(1, result.Value.IgnoredGapsCount);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnUnsupportedPlan_WhenCapabilityIsUnsupported()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                Capability = new CgmBackfillCapability(
                    IsSupported: false,
                    Status: CgmBackfillSupportStatus.ProviderDoesNotSupportHistoricalReadings,
                    MaximumLookback: null,
                    MinimumGapDuration: null,
                    Message: "Provider does not support backfill.")
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now.AddHours(-1),
            now,
            [
                new CgmBackfillDetectedGap(
                    now.AddMinutes(-45),
                    now.AddMinutes(-30))
            ]);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanBackfill);
        Assert.Empty(result.Value.RecoverableGaps);
        Assert.Equal(1, result.Value.IgnoredGapsCount);
        Assert.Equal("Provider does not support backfill.", result.Value.Message);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnFailure_WhenCapabilityFails()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                ShouldFail = true
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now.AddHours(-1),
            now,
            []);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.CapabilityFailed", result.Error.Code);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldReturnFailure_WhenRequestWindowIsInvalid()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var service = new CgmBackfillPlanService(
            new FakeBackfillCapabilityService
            {
                Capability = CreateSupportedCapability()
            },
            new FakeTimeProvider(now));

        var request = new CgmBackfillPlanRequest(
            now,
            now,
            []);

        // Act
        var result = await service.CreatePlanAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Backfill.InvalidWindow", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a supported backfill capability used by the tests.
    /// </summary>
    /// <returns>The supported backfill capability.</returns>
    private static CgmBackfillCapability CreateSupportedCapability()
    {
        return new CgmBackfillCapability(
            IsSupported: true,
            Status: CgmBackfillSupportStatus.Supported,
            MaximumLookback: TimeSpan.FromHours(24),
            MinimumGapDuration: TimeSpan.FromMinutes(10),
            Message: "Backfill is supported.");
    }

    private sealed class FakeBackfillCapabilityService : ICgmBackfillCapabilityService
    {
        public CgmBackfillCapability Capability { get; init; } = CreateSupportedCapability();

        public bool ShouldFail { get; init; }

        /// <inheritdoc />
        public Task<Result<CgmBackfillCapability>> GetCapabilityAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFail)
            {
                return Task.FromResult(Result<CgmBackfillCapability>.Failure(
                    new Error(
                        "Backfill.CapabilityFailed",
                        "Unable to load backfill capability.")));
            }

            return Task.FromResult(Result<CgmBackfillCapability>.Success(Capability));
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FakeTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow()
        {
            return _now;
        }

        /// <inheritdoc />
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    #endregion
}