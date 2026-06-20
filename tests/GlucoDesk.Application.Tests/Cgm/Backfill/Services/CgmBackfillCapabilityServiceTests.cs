using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Options;
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

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillCapabilityServiceTests
{
    [Fact]
    public async Task GetCapabilityAsync_ShouldReturnSupported_WhenProviderSupportsHistoricalReadings()
    {
        // Arrange
        var service = new CgmBackfillCapabilityService(
            new FakeGlucoseDataService
            {
                SupportsHistoricalReadings = true
            },
            CgmBackfillCapabilityOptions.Default);

        // Act
        var result = await service.GetCapabilityAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsSupported);
        Assert.Equal(CgmBackfillSupportStatus.Supported, result.Value.Status);
        Assert.Equal(TimeSpan.FromHours(24), result.Value.MaximumLookback);
        Assert.Equal(TimeSpan.FromMinutes(10), result.Value.MinimumGapDuration);
    }

    [Fact]
    public async Task GetCapabilityAsync_ShouldReturnUnsupported_WhenProviderDoesNotSupportHistoricalReadings()
    {
        // Arrange
        var service = new CgmBackfillCapabilityService(
            new FakeGlucoseDataService
            {
                SupportsHistoricalReadings = false
            },
            CgmBackfillCapabilityOptions.Default);

        // Act
        var result = await service.GetCapabilityAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsSupported);
        Assert.Equal(
            CgmBackfillSupportStatus.ProviderDoesNotSupportHistoricalReadings,
            result.Value.Status);
        Assert.Null(result.Value.MaximumLookback);
        Assert.Null(result.Value.MinimumGapDuration);
    }

    [Fact]
    public async Task GetCapabilityAsync_ShouldReturnDisabled_WhenBackfillIsDisabled()
    {
        // Arrange
        var service = new CgmBackfillCapabilityService(
            new FakeGlucoseDataService
            {
                SupportsHistoricalReadings = true
            },
            CgmBackfillCapabilityOptions.Default with
            {
                IsEnabled = false
            });

        // Act
        var result = await service.GetCapabilityAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsSupported);
        Assert.Equal(CgmBackfillSupportStatus.Disabled, result.Value.Status);
    }

    [Fact]
    public async Task GetCapabilityAsync_ShouldReturnFailure_WhenMetadataCannotBeLoaded()
    {
        // Arrange
        var service = new CgmBackfillCapabilityService(
            new FakeGlucoseDataService
            {
                ShouldFailMetadata = true
            },
            CgmBackfillCapabilityOptions.Default);

        // Act
        var result = await service.GetCapabilityAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Provider.MetadataFailed", result.Error.Code);
    }

    #region Helpers

    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        public bool SupportsHistoricalReadings { get; init; }

        public bool ShouldFailMetadata { get; init; }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFailMetadata)
            {
                return Task.FromResult(Result<CgmProviderMetadata>.Failure(
                    new Error(
                        "Provider.MetadataFailed",
                        "Unable to load provider metadata.")));
            }

            return Task.FromResult(Result<CgmProviderMetadata>.Success(
                new CgmProviderMetadata(
                    CgmProviderKind.Mock,
                    "Mock CGM Provider",
                    GlucoseDataFreshness.NearRealTime,
                    supportsLiveReadings: true,
                    supportsHistoricalReadings: SupportsHistoricalReadings)));
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
            throw new NotSupportedException("Historical readings are not used by these tests.");
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