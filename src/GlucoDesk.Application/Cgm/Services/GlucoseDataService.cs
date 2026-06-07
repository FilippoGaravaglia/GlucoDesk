using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Services;

/// <summary>
/// Provides application-level operations for retrieving glucose data from configured CGM providers.
/// </summary>
public sealed class GlucoseDataService : IGlucoseDataService
{
    private readonly ICgmLiveProvider _liveProvider;
    private readonly ICgmHistoricalProvider _historicalProvider;
    private readonly ICgmMetadataProvider _metadataProvider;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseDataService"/> class.
    /// </summary>
    /// <param name="liveProvider">The configured live CGM provider.</param>
    /// <param name="historicalProvider">The configured historical CGM provider.</param>
    /// <param name="metadataProvider">The configured metadata CGM provider.</param>
    /// <param name="timeProvider">The time provider.</param>
    public GlucoseDataService(
        ICgmLiveProvider liveProvider,
        ICgmHistoricalProvider historicalProvider,
        ICgmMetadataProvider metadataProvider,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(liveProvider);
        ArgumentNullException.ThrowIfNull(historicalProvider);
        ArgumentNullException.ThrowIfNull(metadataProvider);

        _liveProvider = liveProvider;
        _historicalProvider = historicalProvider;
        _metadataProvider = metadataProvider;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(CancellationToken cancellationToken)
    {
        return _metadataProvider.GetMetadataAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
    {
        return _liveProvider.GetLatestReadingAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _liveProvider.GetRecentReadingsAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _historicalProvider.GetReadingsAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
        GlucoseDashboardRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var snapshotCreatedAt = _timeProvider.GetUtcNow();

        var metadataResult = await _metadataProvider
            .GetMetadataAsync(cancellationToken)
            .ConfigureAwait(false);

        if (metadataResult.IsFailure)
        {
            return Result<GlucoseDashboardSnapshot>.Failure(metadataResult.Error);
        }

        var latestReadingResult = await _liveProvider
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

        var recentReadingsResult = await _liveProvider
            .GetRecentReadingsAsync(recentReadingsRequest, cancellationToken)
            .ConfigureAwait(false);

        if (recentReadingsResult.IsFailure)
        {
            return Result<GlucoseDashboardSnapshot>.Failure(recentReadingsResult.Error);
        }

        var snapshot = BuildDashboardSnapshot(
            request,
            metadataResult.Value,
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
    /// <param name="metadata">The provider metadata.</param>
    /// <param name="latestReadingResult">The latest reading result.</param>
    /// <param name="recentReadingsResult">The recent readings result.</param>
    /// <param name="snapshotCreatedAt">The snapshot creation timestamp.</param>
    /// <returns>The glucose dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot BuildDashboardSnapshot(
        GlucoseDashboardRequest request,
        CgmProviderMetadata metadata,
        LatestGlucoseReadingResult latestReadingResult,
        GlucoseReadingsResult recentReadingsResult,
        DateTimeOffset snapshotCreatedAt)
    {
        return new GlucoseDashboardSnapshot(
            metadata,
            latestReadingResult.Reading,
            recentReadingsResult.Readings,
            latestReadingResult.RetrievedAt,
            recentReadingsResult.RetrievedAt,
            snapshotCreatedAt,
            request.StaleThreshold);
    }

    #endregion
}