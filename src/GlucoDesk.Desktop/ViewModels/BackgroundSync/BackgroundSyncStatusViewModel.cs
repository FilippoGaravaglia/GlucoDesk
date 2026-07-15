using CommunityToolkit.Mvvm.ComponentModel;
using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Application.Cgm.BackgroundSync.State;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;
using System.Globalization;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.BackgroundSync;

/// <summary>
/// Exposes user-facing background sync runtime state for desktop UI binding.
/// </summary>
public sealed partial class BackgroundSyncStatusViewModel : ObservableObject, IDisposable
{
    private readonly IBackgroundSyncStateService _stateService;
    private readonly IBackgroundSyncUiDispatcher _uiDispatcher;
    private BackgroundSyncStateSnapshot _lastSnapshot;

    private bool _isDisposed;

    [ObservableProperty]
    private string title = T("BackgroundSyncAutomaticSync");

    [ObservableProperty]
    private string statusText = T("BackgroundSyncWaitingToStart");

    [ObservableProperty]
    private string summaryText = T("BackgroundSyncLocalHistoryAutoUpdate");

    [ObservableProperty]
    private string supportingText = T("BackgroundSyncRecentReadingsUpToDate");

    [ObservableProperty]
    private string lastUpdateText = T("BackgroundSyncNoUpdatesYet");

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

        _lastSnapshot = _stateService.CurrentSnapshot;

        ApplySnapshot(_lastSnapshot);

        _stateService.SnapshotChanged += OnSnapshotChanged;
        LocalizationManager.LanguageChanged += OnLanguageChanged;
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
        LocalizationManager.LanguageChanged -= OnLanguageChanged;

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
    /// Rebuilds user-facing synchronization text when the language changes.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        _uiDispatcher.Post(() =>
        {
            if (!_isDisposed)
            {
                ApplySnapshot(_lastSnapshot);
            }
        });
    }

    /// <summary>
    /// Applies a background sync state snapshot to user-facing bindable properties.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    private void ApplySnapshot(BackgroundSyncStateSnapshot snapshot)
    {
        _lastSnapshot = snapshot;

        Title = T("BackgroundSyncAutomaticSync");
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
            return T("BackgroundSyncNeedsAttention");
        }

        if (snapshot.IsRunning && snapshot.LastSucceededAt is not null)
        {
            return T("BackgroundSyncActive");
        }

        if (snapshot.IsRunning)
        {
            return T("BackgroundSyncPreparing");
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return T("BackgroundSyncPaused");
        }

        return T("BackgroundSyncWaitingToStart");
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
            return T("BackgroundSyncLastUpdateFailed");
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                T("BackgroundSyncUpdatedAt"),
                FormatTime(snapshot.LastSucceededAt.Value));
        }

        if (snapshot.IsRunning)
        {
            return T("BackgroundSyncLookingForReadings");
        }

        return T("BackgroundSyncLocalHistoryAutoUpdate");
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
            return T("BackgroundSyncRetryAutomatically");
        }

        if (snapshot.IsRunning)
        {
            return T("BackgroundSyncKeepsHistoryUpdated");
        }

        if (snapshot.LastSucceededAt is not null)
        {
            return T("BackgroundSyncResumeWhenActive");
        }

        return T("BackgroundSyncRecentReadingsUpToDate");
    }

    /// <summary>
    /// Builds the last update user-facing text.
    /// </summary>
    /// <param name="snapshot">The background sync state snapshot.</param>
    /// <returns>The last update text.</returns>
    private static string BuildLastUpdateText(BackgroundSyncStateSnapshot snapshot)
    {
        return snapshot.LastSucceededAt is null
            ? T("BackgroundSyncNoUpdatesYet")
            : string.Format(
                    CultureInfo.CurrentCulture,
                    T("BackgroundSyncLastSuccessfulUpdate"),
                    FormatTime(snapshot.LastSucceededAt.Value));
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

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    #endregion
}