using GlucoDesk.Application.Cgm.BackgroundSync.Options;
using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Services;

/// <summary>
/// Runs the in-app CGM background sync loop with controlled start and stop lifecycle.
/// </summary>
public sealed class BackgroundSyncLoopService : IBackgroundSyncLoopService, IAsyncDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BackgroundSyncOptions _options;
    private readonly IBackgroundSyncStateService? _stateService;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private CancellationTokenSource? _loopCancellationTokenSource;
    private Task? _loopTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundSyncLoopService"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="options">The background sync options.</param>
    /// <param name="stateService">The optional background sync state service.</param>
    /// <param name="timeProvider">The optional time provider.</param>
    public BackgroundSyncLoopService(
        IServiceScopeFactory serviceScopeFactory,
        BackgroundSyncOptions options,
        IBackgroundSyncStateService? stateService = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(options);

        _serviceScopeFactory = serviceScopeFactory;
        _options = options;
        _stateService = stateService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public bool IsRunning => _loopTask is { IsCompleted: false };

    /// <inheritdoc />
    public async Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (IsRunning)
            {
                return Result.Success();
            }

            _loopCancellationTokenSource?.Dispose();
            _loopCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

            _loopTask = RunLoopAsync(_loopCancellationTokenSource.Token);

            _stateService?.MarkStarted(_timeProvider.GetUtcNow());

            return Result.Success();
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result> StopAsync(CancellationToken cancellationToken)
    {
        Task? loopTaskToAwait;

        await _lifecycleLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (_loopTask is null)
            {
                _stateService?.MarkStopped(_timeProvider.GetUtcNow());
                return Result.Success();
            }

            await _loopCancellationTokenSource
                ?.CancelAsync()!;

            loopTaskToAwait = _loopTask;
        }
        finally
        {
            _lifecycleLock.Release();
        }

        try
        {
            await loopTaskToAwait
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Stopping the loop is expected to cancel the background task.
        }
        finally
        {
            await _lifecycleLock
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                _loopTask = null;
                _loopCancellationTokenSource?.Dispose();
                _loopCancellationTokenSource = null;
                _stateService?.MarkStopped(_timeProvider.GetUtcNow());
            }
            finally
            {
                _lifecycleLock.Release();
            }
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        finally
        {
            _lifecycleLock.Dispose();
            _loopCancellationTokenSource?.Dispose();
        }
    }

    #region Helpers

    /// <summary>
    /// Runs the background sync loop until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await RunIterationSafelyAsync(cancellationToken)
                .ConfigureAwait(false);

            await DelaySafelyAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

   /// <summary>
    /// Runs a single background sync iteration without allowing failures to stop the loop.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RunIterationSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
    
            var backgroundSyncService = scope
                .ServiceProvider
                .GetRequiredService<ICgmBackgroundSyncService>();
    
            var result = await backgroundSyncService
                .RunOnceAsync(cancellationToken)
                .ConfigureAwait(false);
    
            if (result.IsSuccess)
            {
                _stateService?.RecordIteration(result.Value);
                return;
            }
    
            _stateService?.RecordFailure(
                _timeProvider.GetUtcNow(),
                result.Error.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            _stateService?.RecordFailure(
                _timeProvider.GetUtcNow(),
                "Background sync loop iteration failed unexpectedly.");
    
            // The background sync loop must remain alive even when one iteration fails.
        }
    }

    /// <summary>
    /// Waits for the configured sync interval.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task DelaySafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task
                .Delay(_options.SyncInterval, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    #endregion
}