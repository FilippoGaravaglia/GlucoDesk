using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Providers.Resolution.Services;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.Providers.Resolution.Services;

public sealed class CgmProviderResolverTests
{
    [Fact]
    public async Task ResolveActiveLiveProviderAsync_ShouldResolveConfiguredLiveProvider()
    {
        var mockProvider = new FakeProvider(CgmProviderKind.Mock, "Mock");
        var dexcomProvider = new FakeProvider(CgmProviderKind.DexcomOfficial, "Dexcom");

        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.DexcomOfficial,
                historicalProvider: CgmProviderKind.Mock),
            [mockProvider, dexcomProvider]);

        var result = await resolver.ResolveActiveLiveProviderAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(dexcomProvider, result.Value.LiveProvider);
        Assert.Equal(CgmProviderKind.DexcomOfficial, result.Value.Metadata.ProviderKind);
    }

    [Fact]
    public async Task ResolveActiveHistoricalProviderAsync_ShouldResolveConfiguredHistoricalProvider()
    {
        var mockProvider = new FakeProvider(CgmProviderKind.Mock, "Mock");
        var dexcomProvider = new FakeProvider(CgmProviderKind.DexcomOfficial, "Dexcom");

        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.Mock,
                historicalProvider: CgmProviderKind.DexcomOfficial),
            [mockProvider, dexcomProvider]);

        var result = await resolver.ResolveActiveHistoricalProviderAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(dexcomProvider, result.Value.HistoricalProvider);
        Assert.Equal(CgmProviderKind.DexcomOfficial, result.Value.Metadata.ProviderKind);
    }

    [Fact]
    public async Task ResolveActiveLiveProviderAsync_ShouldFallbackToMock_WhenConfiguredProviderIsMissing()
    {
        var mockProvider = new FakeProvider(CgmProviderKind.Mock, "Mock");

        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.DexcomOfficial,
                historicalProvider: CgmProviderKind.Mock),
            [mockProvider]);

        var result = await resolver.ResolveActiveLiveProviderAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(mockProvider, result.Value.LiveProvider);
        Assert.Equal(CgmProviderKind.Mock, result.Value.Metadata.ProviderKind);
    }

    [Fact]
    public async Task ResolveActiveHistoricalProviderAsync_ShouldFallbackToMock_WhenConfiguredProviderIsMissing()
    {
        var mockProvider = new FakeProvider(CgmProviderKind.Mock, "Mock");

        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.Mock,
                historicalProvider: CgmProviderKind.DexcomOfficial),
            [mockProvider]);

        var result = await resolver.ResolveActiveHistoricalProviderAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(mockProvider, result.Value.HistoricalProvider);
        Assert.Equal(CgmProviderKind.Mock, result.Value.Metadata.ProviderKind);
    }

    [Fact]
    public async Task ResolveActiveLiveProviderAsync_ShouldFallbackToDefaultSettings_WhenSettingsCannotBeLoaded()
    {
        var mockProvider = new FakeProvider(CgmProviderKind.Mock, "Mock");

        var resolver = CreateResolver(
            settingsResult: Result<ApplicationSettings>.Failure(
                new Error("Settings.LoadFailed", "Settings failed.")),
            providers: [mockProvider]);

        var result = await resolver.ResolveActiveLiveProviderAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(mockProvider, result.Value.LiveProvider);
        Assert.Equal(CgmProviderKind.Mock, result.Value.Metadata.ProviderKind);
    }

    [Fact]
    public async Task ResolveActiveLiveProviderAsync_ShouldReturnFailure_WhenNoProviderIsAvailable()
    {
        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.DexcomOfficial,
                historicalProvider: CgmProviderKind.DexcomOfficial),
            []);

        var result = await resolver.ResolveActiveLiveProviderAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Cgm.LiveProviderUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task ResolveActiveHistoricalProviderAsync_ShouldReturnFailure_WhenNoProviderIsAvailable()
    {
        var resolver = CreateResolver(
            new ApplicationSettings(
                activeLiveProvider: CgmProviderKind.DexcomOfficial,
                historicalProvider: CgmProviderKind.DexcomOfficial),
            []);

        var result = await resolver.ResolveActiveHistoricalProviderAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Cgm.HistoricalProviderUnavailable", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a resolver using a successful settings result.
    /// </summary>
    /// <param name="settings">The settings to return.</param>
    /// <param name="providers">The registered providers.</param>
    /// <returns>The provider resolver.</returns>
    private static CgmProviderResolver CreateResolver(
        ApplicationSettings settings,
        IReadOnlyCollection<FakeProvider> providers)
    {
        return CreateResolver(
            Result<ApplicationSettings>.Success(settings),
            providers);
    }

    /// <summary>
    /// Creates a resolver using a custom settings result.
    /// </summary>
    /// <param name="settingsResult">The settings result.</param>
    /// <param name="providers">The registered providers.</param>
    /// <returns>The provider resolver.</returns>
    private static CgmProviderResolver CreateResolver(
        Result<ApplicationSettings> settingsResult,
        IReadOnlyCollection<FakeProvider> providers)
    {
        return new CgmProviderResolver(
            new FakeApplicationSettingsService(settingsResult),
            providers,
            providers,
            providers);
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        private readonly Result<ApplicationSettings> _settingsResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeApplicationSettingsService"/> class.
        /// </summary>
        /// <param name="settingsResult">The settings result.</param>
        public FakeApplicationSettingsService(Result<ApplicationSettings> settingsResult)
        {
            _settingsResult = settingsResult;
        }

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settingsResult);
        }

        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
    {
        private readonly CgmProviderMetadata _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeProvider"/> class.
        /// </summary>
        /// <param name="providerKind">The provider kind.</param>
        /// <param name="displayName">The display name.</param>
        public FakeProvider(CgmProviderKind providerKind, string displayName)
        {
            _metadata = new CgmProviderMetadata(
                providerKind,
                displayName,
                GlucoseDataFreshness.NearRealTime,
                supportsLiveReadings: true,
                supportsHistoricalReadings: true);
        }

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
        public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<CgmProviderMetadata>.Success(_metadata));
        }
    }

    #endregion
}