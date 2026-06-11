using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Models;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Cgm.Services;

/// <summary>
/// Provides application-level glucose data operations.
/// </summary>
public sealed class GlucoseDataService : IGlucoseDataService
{
    private readonly ICgmProviderResolver _providerResolver;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseDataService"/> class.
    /// </summary>
    /// <param name="providerResolver">The CGM provider resolver.</param>
    /// <param name="timeProvider">The time provider.</param>
    [ActivatorUtilitiesConstructor]
    public GlucoseDataService(
        ICgmProviderResolver providerResolver,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(providerResolver);

        _providerResolver = providerResolver;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseDataService"/> class.
    /// </summary>
    /// <param name="liveProvider">The live CGM provider.</param>
    /// <param name="historicalProvider">The historical CGM provider.</param>
    /// <param name="metadataProvider">The provider metadata provider.</param>
    /// <param name="timeProvider">The time provider.</param>
    public GlucoseDataService(
        ICgmLiveProvider liveProvider,
        ICgmHistoricalProvider historicalProvider,
        ICgmMetadataProvider metadataProvider,
        TimeProvider? timeProvider = null)
        : this(
            new StaticCgmProviderResolver(
                liveProvider,
                historicalProvider,
                metadataProvider),
            timeProvider)
    {
    }

    /// <inheritdoc />
    public async Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(
        CancellationToken cancellationToken)
    {
        var providerResolutionResult = await _providerResolver
            .ResolveActiveLiveProviderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providerResolutionResult.IsFailure)
        {
            return Result<CgmProviderMetadata>.Failure(providerResolutionResult.Error);
        }

        return Result<CgmProviderMetadata>.Success(providerResolutionResult.Value.Metadata);
    }

    /// <inheritdoc />
    public async Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
        CancellationToken cancellationToken)
    {
        var providerResolutionResult = await _providerResolver
            .ResolveActiveLiveProviderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providerResolutionResult.IsFailure)
        {
            return Result<LatestGlucoseReadingResult>.Failure(providerResolutionResult.Error);
        }

        return await providerResolutionResult.Value.LiveProvider
            .GetLatestReadingAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var providerResolutionResult = await _providerResolver
            .ResolveActiveLiveProviderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providerResolutionResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(providerResolutionResult.Error);
        }

        return await providerResolutionResult.Value.LiveProvider
            .GetRecentReadingsAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var providerResolutionResult = await _providerResolver
            .ResolveActiveHistoricalProviderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providerResolutionResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(providerResolutionResult.Error);
        }

        return await providerResolutionResult.Value.HistoricalProvider
            .GetReadingsAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
        GlucoseDashboardRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var snapshotCreatedAt = _timeProvider.GetUtcNow();

        var providerResolutionResult = await _providerResolver
            .ResolveActiveLiveProviderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providerResolutionResult.IsFailure)
        {
            return Result<GlucoseDashboardSnapshot>.Failure(providerResolutionResult.Error);
        }

        var latestReadingResult = await providerResolutionResult.Value.LiveProvider
            .GetLatestReadingAsync(cancellationToken)
            .ConfigureAwait(false);

        if (latestReadingResult.IsFailure)
        {
            return Result<GlucoseDashboardSnapshot>.Failure(latestReadingResult.Error);
        }

        var recentReadingsRequest = GlucoseReadingsRequest.ForLast(
            request.HistoryDuration,
            snapshotCreatedAt,
            request.MaxReadings);

        var recentReadingsResult = await providerResolutionResult.Value.LiveProvider
            .GetRecentReadingsAsync(recentReadingsRequest, cancellationToken)
            .ConfigureAwait(false);

        if (recentReadingsResult.IsFailure)
        {
            return Result<GlucoseDashboardSnapshot>.Failure(recentReadingsResult.Error);
        }

        var snapshot = BuildDashboardSnapshot(
            request,
            providerResolutionResult.Value,
            latestReadingResult.Value,
            recentReadingsResult.Value,
            snapshotCreatedAt);

        return Result<GlucoseDashboardSnapshot>.Success(snapshot);
    }

    #region Helpers

    /// <summary>
    /// Builds the dashboard snapshot from provider results.
    /// </summary>
    /// <param name="request">The dashboard request.</param>
    /// <param name="providerResolution">The resolved live provider.</param>
    /// <param name="latestReadingResult">The latest reading result.</param>
    /// <param name="recentReadingsResult">The recent readings result.</param>
    /// <param name="snapshotCreatedAt">The snapshot creation timestamp.</param>
    /// <returns>The glucose dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot BuildDashboardSnapshot(
        GlucoseDashboardRequest request,
        CgmLiveProviderResolution providerResolution,
        LatestGlucoseReadingResult latestReadingResult,
        GlucoseReadingsResult recentReadingsResult,
        DateTimeOffset snapshotCreatedAt)
    {
        return new GlucoseDashboardSnapshot(
            providerResolution.Metadata,
            latestReadingResult.Reading,
            recentReadingsResult.Readings,
            latestReadingResult.RetrievedAt,
            recentReadingsResult.RetrievedAt,
            snapshotCreatedAt,
            request.StaleThreshold);
    }

    private sealed class StaticCgmProviderResolver : ICgmProviderResolver
    {
        private readonly ICgmLiveProvider _liveProvider;
        private readonly ICgmHistoricalProvider _historicalProvider;
        private readonly ICgmMetadataProvider _metadataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticCgmProviderResolver"/> class.
        /// </summary>
        /// <param name="liveProvider">The live provider.</param>
        /// <param name="historicalProvider">The historical provider.</param>
        /// <param name="metadataProvider">The metadata provider.</param>
        public StaticCgmProviderResolver(
            ICgmLiveProvider liveProvider,
            ICgmHistoricalProvider historicalProvider,
            ICgmMetadataProvider metadataProvider)
        {
            ArgumentNullException.ThrowIfNull(liveProvider);
            ArgumentNullException.ThrowIfNull(historicalProvider);
            ArgumentNullException.ThrowIfNull(metadataProvider);

            _liveProvider = liveProvider;
            _historicalProvider = historicalProvider;
            _metadataProvider = metadataProvider;
        }

        /// <inheritdoc />
        public async Task<Result<CgmLiveProviderResolution>> ResolveActiveLiveProviderAsync(
            CancellationToken cancellationToken)
        {
            var metadataResult = await _metadataProvider
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            if (metadataResult.IsFailure)
            {
                return Result<CgmLiveProviderResolution>.Failure(metadataResult.Error);
            }

            return Result<CgmLiveProviderResolution>.Success(
                new CgmLiveProviderResolution(
                    metadataResult.Value,
                    _liveProvider,
                    _metadataProvider));
        }

        /// <inheritdoc />
        public async Task<Result<CgmHistoricalProviderResolution>> ResolveActiveHistoricalProviderAsync(
            CancellationToken cancellationToken)
        {
            var metadataResult = await _metadataProvider
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            if (metadataResult.IsFailure)
            {
                return Result<CgmHistoricalProviderResolution>.Failure(metadataResult.Error);
            }

            return Result<CgmHistoricalProviderResolution>.Success(
                new CgmHistoricalProviderResolution(
                    metadataResult.Value,
                    _historicalProvider,
                    _metadataProvider));
        }
    }

    #endregion
}