using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Services;

/// <summary>
/// Runs best-effort in-app CGM background sync iterations.
/// </summary>
public sealed class CgmBackgroundSyncService : ICgmBackgroundSyncService
{
    private readonly IGlucoseDataService _glucoseDataService;
    private readonly IGlucoseHistoryService? _glucoseHistoryService;
    private readonly IWidgetStatePublisher? _widgetStatePublisher;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackgroundSyncService"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="glucoseHistoryService">The optional glucose history service.</param>
    /// <param name="widgetStatePublisher">The optional widget state publisher.</param>
    public CgmBackgroundSyncService(
        IGlucoseDataService glucoseDataService,
        TimeProvider timeProvider,
        IGlucoseHistoryService? glucoseHistoryService = null,
        IWidgetStatePublisher? widgetStatePublisher = null)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _glucoseDataService = glucoseDataService;
        _timeProvider = timeProvider;
        _glucoseHistoryService = glucoseHistoryService;
        _widgetStatePublisher = widgetStatePublisher;
    }

    /// <inheritdoc />
    public async Task<Result<BackgroundSyncIterationResult>> RunOnceAsync(
        CancellationToken cancellationToken)
    {
        if (!await _syncLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return Result<BackgroundSyncIterationResult>.Success(
                BackgroundSyncIterationResult.SkippedAlreadyRunning(
                    _timeProvider.GetUtcNow()));
        }

        try
        {
            var snapshotResult = await _glucoseDataService
                .GetDashboardSnapshotAsync(
                    CgmBackgroundSyncRequestFactory.CreateDefaultDashboardRequest(),
                    cancellationToken)
                .ConfigureAwait(false);

            if (snapshotResult.IsFailure)
            {
                await PublishUnavailableWidgetStateSafelyAsync(
                        CgmProviderKind.Unknown,
                        snapshotResult.Error.Message,
                        cancellationToken)
                    .ConfigureAwait(false);

                return Result<BackgroundSyncIterationResult>.Success(
                    BackgroundSyncIterationResult.ProviderFailed(
                        _timeProvider.GetUtcNow(),
                        snapshotResult.Error.Message));
            }

            var snapshot = snapshotResult.Value;
            var readings = NormalizeReadings(snapshot.RecentReadings, snapshot.LatestReading);

            if (readings.Count == 0)
            {
                await PublishUnavailableWidgetStateSafelyAsync(
                        snapshot.Metadata.ProviderKind,
                        "No glucose readings available during background sync.",
                        cancellationToken)
                    .ConfigureAwait(false);

                return Result<BackgroundSyncIterationResult>.Success(
                    BackgroundSyncIterationResult.NoData(
                        snapshot.Metadata.ProviderKind,
                        _timeProvider.GetUtcNow()));
            }

            await PersistReadingsSafelyAsync(readings, cancellationToken)
                .ConfigureAwait(false);

            await PublishWidgetStateSafelyAsync(
                    readings,
                    snapshot.Metadata.ProviderKind,
                    cancellationToken)
                .ConfigureAwait(false);

            return Result<BackgroundSyncIterationResult>.Success(
                BackgroundSyncIterationResult.Succeeded(
                    snapshot.Metadata.ProviderKind,
                    readings.Count,
                    _timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            await PublishUnavailableWidgetStateSafelyAsync(
                    CgmProviderKind.Unknown,
                    "Background sync failed unexpectedly.",
                    cancellationToken)
                .ConfigureAwait(false);

            return Result<BackgroundSyncIterationResult>.Success(
                BackgroundSyncIterationResult.Failed(_timeProvider.GetUtcNow()));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    #region Helpers

    /// <summary>
    /// Persists glucose readings as a best-effort background sync side effect.
    /// </summary>
    /// <param name="readings">The glucose readings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PersistReadingsSafelyAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken)
    {
        if (_glucoseHistoryService is null || readings.Count == 0)
        {
            return;
        }

        try
        {
            _ = await _glucoseHistoryService
                .SaveReadingsWithSummaryAsync(readings, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Background history persistence must not break the app.
        }
    }

    /// <summary>
    /// Publishes widget state as a best-effort background sync side effect.
    /// </summary>
    /// <param name="readings">The glucose readings.</param>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PublishWidgetStateSafelyAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CgmProviderKind providerKind,
        CancellationToken cancellationToken)
    {
        if (_widgetStatePublisher is null)
        {
            return;
        }

        try
        {
            _ = await _widgetStatePublisher
                .PublishLatestReadingAsync(readings, providerKind, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Widget state publishing must not break background sync.
        }
    }

    /// <summary>
    /// Publishes unavailable widget state as a best-effort background sync side effect.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="statusMessage">The status message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PublishUnavailableWidgetStateSafelyAsync(
        CgmProviderKind providerKind,
        string statusMessage,
        CancellationToken cancellationToken)
    {
        if (_widgetStatePublisher is null)
        {
            return;
        }

        try
        {
            _ = await _widgetStatePublisher
                .PublishUnavailableAsync(providerKind, statusMessage, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Widget state publishing must not break background sync.
        }
    }

    /// <summary>
    /// Normalizes readings by combining latest and recent readings.
    /// </summary>
    /// <param name="recentReadings">The recent readings.</param>
    /// <param name="latestReading">The latest reading.</param>
    /// <returns>The normalized readings.</returns>
    private static IReadOnlyCollection<GlucoseReading> NormalizeReadings(
        IReadOnlyCollection<GlucoseReading> recentReadings,
        GlucoseReading? latestReading)
    {
        var readingsByKey = new Dictionary<string, GlucoseReading>();

        foreach (var reading in recentReadings)
        {
            readingsByKey[BuildReadingKey(reading)] = reading;
        }

        if (latestReading is not null)
        {
            readingsByKey[BuildReadingKey(latestReading)] = latestReading;
        }

        return readingsByKey.Values
            .OrderBy(reading => reading.Timestamp)
            .ToArray();
    }

    /// <summary>
    /// Builds a stable deduplication key for a glucose reading.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <returns>The reading key.</returns>
    private static string BuildReadingKey(GlucoseReading reading)
    {
        return string.Create(
            null,
            $"{reading.Provider}|{reading.Timestamp.ToUniversalTime().Ticks}");
    }

    #endregion
}