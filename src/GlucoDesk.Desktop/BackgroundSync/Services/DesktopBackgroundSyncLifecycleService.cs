using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.BackgroundSync.Options;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;

namespace GlucoDesk.Desktop.BackgroundSync.Services;

/// <summary>
/// Coordinates the desktop lifecycle of the in-app background sync loop.
/// </summary>
public sealed class DesktopBackgroundSyncLifecycleService : IDesktopBackgroundSyncLifecycleService
{
    private readonly IBackgroundSyncLoopService _backgroundSyncLoopService;
    private readonly DesktopBackgroundSyncLifecycleOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopBackgroundSyncLifecycleService"/> class.
    /// </summary>
    /// <param name="backgroundSyncLoopService">The background sync loop service.</param>
    /// <param name="options">The desktop background sync lifecycle options.</param>
    public DesktopBackgroundSyncLifecycleService(
        IBackgroundSyncLoopService backgroundSyncLoopService,
        DesktopBackgroundSyncLifecycleOptions options)
    {
        ArgumentNullException.ThrowIfNull(backgroundSyncLoopService);
        ArgumentNullException.ThrowIfNull(options);

        _backgroundSyncLoopService = backgroundSyncLoopService;
        _options = options;
    }

    /// <inheritdoc />
    public bool IsRunning => _backgroundSyncLoopService.IsRunning;

    /// <inheritdoc />
    public async Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.StartOnApplicationStartup)
        {
            return Result.Success();
        }

        try
        {
            return await _backgroundSyncLoopService
                .StartAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(
                new Error(
                    "BackgroundSync.StartCancelled",
                    "Background sync startup was cancelled."));
        }
        catch
        {
            return Result.Failure(
                new Error(
                    "BackgroundSync.StartFailed",
                    "Unable to start background sync."));
        }
    }

    /// <inheritdoc />
    public async Task<Result> StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            timeoutCancellationTokenSource.CancelAfter(_options.StopTimeout);

            return await _backgroundSyncLoopService
                .StopAsync(timeoutCancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(
                new Error(
                    "BackgroundSync.StopCancelled",
                    "Background sync shutdown was cancelled."));
        }
        catch
        {
            return Result.Failure(
                new Error(
                    "BackgroundSync.StopFailed",
                    "Unable to stop background sync."));
        }
    }
}