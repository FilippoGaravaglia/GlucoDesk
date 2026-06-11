using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Providers.Resolution.Models;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.Providers.Resolution.Models;

public sealed class CgmProviderResolutionTests
{
    [Fact]
    public void CgmLiveProviderResolution_ShouldCreateResolution_WhenValuesAreValid()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var resolution = new CgmLiveProviderResolution(
            metadata,
            provider,
            provider);

        Assert.Same(metadata, resolution.Metadata);
        Assert.Same(provider, resolution.LiveProvider);
        Assert.Same(provider, resolution.MetadataProvider);
    }

    [Fact]
    public void CgmHistoricalProviderResolution_ShouldCreateResolution_WhenValuesAreValid()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var resolution = new CgmHistoricalProviderResolution(
            metadata,
            provider,
            provider);

        Assert.Same(metadata, resolution.Metadata);
        Assert.Same(provider, resolution.HistoricalProvider);
        Assert.Same(provider, resolution.MetadataProvider);
    }

    [Fact]
    public void CgmLiveProviderResolution_ShouldRejectNullMetadata()
    {
        var provider = new FakeProvider();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmLiveProviderResolution(null!, provider, provider));

        Assert.Equal("metadata", exception.ParamName);
    }

    [Fact]
    public void CgmLiveProviderResolution_ShouldRejectNullLiveProvider()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmLiveProviderResolution(metadata, null!, provider));

        Assert.Equal("liveProvider", exception.ParamName);
    }

    [Fact]
    public void CgmLiveProviderResolution_ShouldRejectNullMetadataProvider()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmLiveProviderResolution(metadata, provider, null!));

        Assert.Equal("metadataProvider", exception.ParamName);
    }

    [Fact]
    public void CgmHistoricalProviderResolution_ShouldRejectNullMetadata()
    {
        var provider = new FakeProvider();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmHistoricalProviderResolution(null!, provider, provider));

        Assert.Equal("metadata", exception.ParamName);
    }

    [Fact]
    public void CgmHistoricalProviderResolution_ShouldRejectNullHistoricalProvider()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmHistoricalProviderResolution(metadata, null!, provider));

        Assert.Equal("historicalProvider", exception.ParamName);
    }

    [Fact]
    public void CgmHistoricalProviderResolution_ShouldRejectNullMetadataProvider()
    {
        var provider = new FakeProvider();
        var metadata = CreateMetadata();

        var exception = Assert.Throws<ArgumentNullException>(
            () => new CgmHistoricalProviderResolution(metadata, provider, null!));

        Assert.Equal("metadataProvider", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates provider metadata for tests.
    /// </summary>
    /// <returns>The provider metadata.</returns>
    private static CgmProviderMetadata CreateMetadata()
    {
        return new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "Mock Provider",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);
    }

    private sealed class FakeProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
    {
        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetMetadataAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    #endregion
}