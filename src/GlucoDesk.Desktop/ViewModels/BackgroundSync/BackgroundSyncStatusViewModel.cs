using CommunityToolkit.Mvvm.ComponentModel;
using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Application.Cgm.BackgroundSync.State;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;

namespace GlucoDesk.Desktop.ViewModels.BackgroundSync;

/// <summary>
/// Exposes user-facing background sync runtime state for desktop UI binding.
/// </summary>
public sealed partial class BackgroundSyncStatusViewModel : ObservableObject, IDisposable
{
    private readonly IBackgroundSyncStateService _stateService;
    private readonly IBackgroundSyncUiDispatcher _uiDispatcher;

    private bool _isDisposed;

    [ObservableProperty]
    private string title = "Automatic sync";

    [ObservableProperty]
    private string statusText = "Waiting to start";

    [ObservableProperty]
    private string summaryText = "Your local history will update automatically while GlucoDesk is open.";

    [ObservableProperty]
    private string supportingText = "GlucoDesk will keep your recent readings up to date in the background.";

    [ObservableProperty]
    private string lastUpdateText = "No updates yet";

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private bool needsAttention;

    [ObservableProperty]
    private bool hasSuccessfulSync;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundSyncStatusViewModel"/> class.
    /// </summary>
    /// <param name="stateService">The background sync state service.</param>
    /// <param name="uiDispatcher">The UI dispatcher.</param>
    public BackgroundSyncStatusViewModel(
        IBackgroundSyncStateService stateService,
        IBackgroundSyncUiDispatcher uiDispatcher)
    {
        ArgumentNullException.ThrowIfNull(stateService);
        ArgumentNullException.ThrowIfNull(uiDispatcher);

        _stateService = stateService;
        _uiDispatcher = uiDispatcher;

        ApplySnapshot(_stateService.CurrentSnapshot);
        _stateService.SnapshotChanged += OnSnapshotChanged;
    }

    /// <summary>
    /// Releases event subscriptions used by the view model.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _stateService.SnapshotChanged -= OnSnapshotChanged;
        _isDisposed = true;
    }

    #region Helpers

    /// <summary>
    /// Handles background sync state changes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="snapshot">The updated background sync state snapshot.</param>
    private void OnSnapshotChanged(object? sender, BackgroundSyncStateSnapshot snapshot)
    {
        if (_isDisposed)
        {
            return;
        }

        _uiDispatcher.Post(() =>
        {
            if (_isDisposed)
            {
                return;
            }

            ApplySnapshot(snapshot);
        });
    }

    /// <summary>
    /// Applies a background sync state snapshot to user-facing bindable properties.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    private void ApplySnapshot(BackgroundSyncStateSnapshot snapshot)
    {
        IsRunning = snapshot.IsRunning;
        NeedsAttention = snapshot.LastStatus is BackgroundSyncStatus.ProviderFailed or BackgroundSyncStatus.Failed;
        HasSuccessfulSync = snapshot.LastSucceededAt is not null;

        StatusText = BuildStatusText(snapshot);
        SummaryText = BuildSummaryText(snapshot);
        SupportingText = BuildSupportingText(snapshot);
        LastUpdateText = BuildLastUpdateText(snapshot);
    }

    /// <summary>
    /// Builds the primary user-facing status text.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    /// <returns>The status text.</returns>
    private static string BuildStatusText(BackgroundSyncStateSnapshot snapshot)
    {
        if (snapshot.LastStatus is BackgroundSyncStatus.ProviderFailed or BackgroundSyncStatus.Failed)
        {
            return "Needs attention";
        }

        if (snapshot.IsRunning && snapshot.LastSucceededAt is not null)
        {
            return "Sync active";
        }

        if (snapshot.IsRunning)
        {
            return "Preparing sync";
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return "Sync paused";
        }

        return "Waiting to start";
    }

    /// <summary>
    /// Builds the short user-facing summary text.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    /// <returns>The summary text.</returns>
    private static string BuildSummaryText(BackgroundSyncStateSnapshot snapshot)
    {
        if (snapshot.LastStatus is BackgroundSyncStatus.ProviderFailed or BackgroundSyncStatus.Failed)
        {
            return "Last update failed";
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return $"Updated at {FormatTime(snapshot.LastSucceededAt.Value)}";
        }

        if (snapshot.IsRunning)
        {
            return "Looking for recent readings";
        }

        return "Your local history will update automatically while GlucoDesk is open.";
    }

    /// <summary>
    /// Builds the supporting user-facing explanation text.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    /// <returns>The supporting text.</returns>
    private static string BuildSupportingText(BackgroundSyncStateSnapshot snapshot)
    {
        if (snapshot.LastStatus is BackgroundSyncStatus.ProviderFailed or BackgroundSyncStatus.Failed)
        {
            return "GlucoDesk will try again automatically. If the issue continues, check your account or connection.";
        }

        if (snapshot.IsRunning)
        {
            return "GlucoDesk keeps your local history updated while the app is open.";
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return "Automatic updates will resume when background sync is active.";
        }

        return "GlucoDesk will keep your recent readings up to date in the background.";
    }

    /// <summary>
    /// Builds the last update user-facing text.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    /// <returns>The last update text.</returns>
    private static string BuildLastUpdateText(BackgroundSyncStateSnapshot snapshot)
    {
        return snapshot.LastSucceededAt is null
            ? "No updates yet"
            : $"Last successful update: {FormatTime(snapshot.LastSucceededAt.Value)}";
    }

    /// <summary>
    /// Formats a timestamp for user-facing display.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The formatted time.</returns>
    private static string FormatTime(DateTimeOffset timestamp)
    {
        return timestamp
            .ToLocalTime()
            .ToString("HH:mm");
    }

    #endregion
}