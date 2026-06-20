using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.Common.Dispatching.Abstractions;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.ViewModels;

/// <summary>
/// ViewModel exposing desktop-visible history continuity synchronization status.
/// </summary>
public sealed class DesktopHistoryContinuitySyncStatusViewModel : ObservableObject, IDisposable
{
    private readonly IDesktopHistoryContinuitySyncStatusStore _statusStore;
    private readonly IDesktopUiDispatcher _uiDispatcher;

    private bool _isDisposed;
    private DesktopHistoryContinuitySyncRunState _state;
    private string _stateText = string.Empty;
    private string _triggerText = string.Empty;
    private string _message = string.Empty;
    private string _startedAtText = string.Empty;
    private string _completedAtText = string.Empty;
    private string _lastSuccessfulSyncAtText = string.Empty;
    private string _readingSummaryText = string.Empty;
    private string _errorText = string.Empty;
    private bool _isRunning;
    private bool _hasError;
    private bool _hasReadingSummary;
    private bool _hasLastSuccessfulSync;
    private bool _hasNewReadings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopHistoryContinuitySyncStatusViewModel"/> class.
    /// </summary>
    /// <param name="statusStore">The history continuity synchronization status store.</param>
    /// <param name="uiDispatcher">The desktop UI dispatcher.</param>
    /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
    public DesktopHistoryContinuitySyncStatusViewModel(
        IDesktopHistoryContinuitySyncStatusStore statusStore,
        IDesktopUiDispatcher uiDispatcher)
    {
        ArgumentNullException.ThrowIfNull(statusStore);
        ArgumentNullException.ThrowIfNull(uiDispatcher);

        _statusStore = statusStore;
        _uiDispatcher = uiDispatcher;

        ApplySnapshot(_statusStore.Current);

        _statusStore.StatusChanged += OnStatusChanged;
    }

    /// <summary>
    /// Gets the current synchronization state.
    /// </summary>
    public DesktopHistoryContinuitySyncRunState State
    {
        get => _state;
        private set => SetProperty(ref _state, value);
    }

    /// <summary>
    /// Gets the formatted synchronization state text.
    /// </summary>
    public string StateText
    {
        get => _stateText;
        private set => SetProperty(ref _stateText, value);
    }

    /// <summary>
    /// Gets the formatted synchronization trigger text.
    /// </summary>
    public string TriggerText
    {
        get => _triggerText;
        private set => SetProperty(ref _triggerText, value);
    }

    /// <summary>
    /// Gets the user-facing synchronization message.
    /// </summary>
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    /// <summary>
    /// Gets the formatted synchronization start timestamp.
    /// </summary>
    public string StartedAtText
    {
        get => _startedAtText;
        private set => SetProperty(ref _startedAtText, value);
    }

    /// <summary>
    /// Gets the formatted synchronization completion timestamp.
    /// </summary>
    public string CompletedAtText
    {
        get => _completedAtText;
        private set => SetProperty(ref _completedAtText, value);
    }

    /// <summary>
    /// Gets the formatted latest successful synchronization timestamp.
    /// </summary>
    public string LastSuccessfulSyncAtText
    {
        get => _lastSuccessfulSyncAtText;
        private set => SetProperty(ref _lastSuccessfulSyncAtText, value);
    }

    /// <summary>
    /// Gets the latest reading synchronization summary.
    /// </summary>
    public string ReadingSummaryText
    {
        get => _readingSummaryText;
        private set => SetProperty(ref _readingSummaryText, value);
    }

    /// <summary>
    /// Gets the latest error text.
    /// </summary>
    public string ErrorText
    {
        get => _errorText;
        private set => SetProperty(ref _errorText, value);
    }

    /// <summary>
    /// Gets a value indicating whether synchronization is currently running.
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        private set => SetProperty(ref _isRunning, value);
    }

    /// <summary>
    /// Gets a value indicating whether the latest synchronization failed.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    /// <summary>
    /// Gets a value indicating whether the latest synchronization has a reading summary.
    /// </summary>
    public bool HasReadingSummary
    {
        get => _hasReadingSummary;
        private set => SetProperty(ref _hasReadingSummary, value);
    }

    /// <summary>
    /// Gets a value indicating whether at least one successful synchronization timestamp is available.
    /// </summary>
    public bool HasLastSuccessfulSync
    {
        get => _hasLastSuccessfulSync;
        private set => SetProperty(ref _hasLastSuccessfulSync, value);
    }

    /// <summary>
    /// Gets a value indicating whether the latest synchronization added new readings.
    /// </summary>
    public bool HasNewReadings
    {
        get => _hasNewReadings;
        private set => SetProperty(ref _hasNewReadings, value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _statusStore.StatusChanged -= OnStatusChanged;
        _isDisposed = true;
    }

    #region Helpers

    /// <summary>
    /// Handles status snapshot changes from the underlying store.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="snapshot">The new status snapshot.</param>
    private void OnStatusChanged(
        object? sender,
        DesktopHistoryContinuitySyncStatusSnapshot snapshot)
    {
        _uiDispatcher.Post(() => ApplySnapshot(snapshot));
    }

    /// <summary>
    /// Applies a status snapshot to the ViewModel properties.
    /// </summary>
    /// <param name="snapshot">The status snapshot.</param>
    private void ApplySnapshot(DesktopHistoryContinuitySyncStatusSnapshot snapshot)
    {
        State = snapshot.State;
        StateText = FormatState(snapshot.State);
        TriggerText = FormatTrigger(snapshot.Trigger);
        Message = snapshot.Message;
        StartedAtText = FormatTimestamp(snapshot.StartedAtUtc);
        CompletedAtText = FormatTimestamp(snapshot.CompletedAtUtc);
        LastSuccessfulSyncAtText = FormatTimestamp(snapshot.LastSuccessfulSyncAtUtc);
        ReadingSummaryText = FormatReadingSummary(snapshot);
        ErrorText = FormatError(snapshot);
        IsRunning = snapshot.State == DesktopHistoryContinuitySyncRunState.Running;
        HasError = snapshot.State == DesktopHistoryContinuitySyncRunState.Failed;
        HasReadingSummary = snapshot.TotalFetchedReadings > 0 ||
                            snapshot.AddedReadingsCount > 0 ||
                            snapshot.DuplicateReadingsCount > 0 ||
                            snapshot.StoredReadingsCount > 0;
        HasLastSuccessfulSync = snapshot.LastSuccessfulSyncAtUtc is not null;
        HasNewReadings = snapshot.HasNewReadings;
    }

    /// <summary>
    /// Formats a synchronization state.
    /// </summary>
    /// <param name="state">The synchronization state.</param>
    /// <returns>The formatted synchronization state.</returns>
    private static string FormatState(DesktopHistoryContinuitySyncRunState state)
    {
        return state switch
        {
            DesktopHistoryContinuitySyncRunState.Idle => "Idle",
            DesktopHistoryContinuitySyncRunState.Running => "Syncing history",
            DesktopHistoryContinuitySyncRunState.Succeeded => "History synced",
            DesktopHistoryContinuitySyncRunState.Skipped => "Skipped",
            DesktopHistoryContinuitySyncRunState.Failed => "Sync failed",
            DesktopHistoryContinuitySyncRunState.Canceled => "Canceled",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Formats a synchronization trigger.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <returns>The formatted trigger.</returns>
    private static string FormatTrigger(CgmHistoryContinuitySyncTrigger? trigger)
    {
        return trigger switch
        {
            CgmHistoryContinuitySyncTrigger.Startup => "Startup",
            CgmHistoryContinuitySyncTrigger.Resume => "Resume",
            CgmHistoryContinuitySyncTrigger.Manual => "Manual",
            _ => "Not available"
        };
    }

    /// <summary>
    /// Formats a UTC timestamp for desktop display.
    /// </summary>
    /// <param name="timestamp">The UTC timestamp.</param>
    /// <returns>The formatted local timestamp.</returns>
    private static string FormatTimestamp(DateTimeOffset? timestamp)
    {
        return timestamp is null
            ? "Not available"
            : timestamp.Value
                .ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats the latest reading synchronization summary.
    /// </summary>
    /// <param name="snapshot">The status snapshot.</param>
    /// <returns>The formatted reading summary.</returns>
    private static string FormatReadingSummary(DesktopHistoryContinuitySyncStatusSnapshot snapshot)
    {
        if (snapshot.TotalFetchedReadings == 0 &&
            snapshot.AddedReadingsCount == 0 &&
            snapshot.DuplicateReadingsCount == 0 &&
            snapshot.StoredReadingsCount == 0)
        {
            return "No readings synchronized yet.";
        }

        return string.Create(
            CultureInfo.CurrentCulture,
            $"Fetched: {snapshot.TotalFetchedReadings}, added: {snapshot.AddedReadingsCount}, duplicates: {snapshot.DuplicateReadingsCount}, stored: {snapshot.StoredReadingsCount}");
    }

    /// <summary>
    /// Formats the latest error information.
    /// </summary>
    /// <param name="snapshot">The status snapshot.</param>
    /// <returns>The formatted error text.</returns>
    private static string FormatError(DesktopHistoryContinuitySyncStatusSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.ErrorCode) &&
            string.IsNullOrWhiteSpace(snapshot.ErrorDescription))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(snapshot.ErrorCode))
        {
            return snapshot.ErrorDescription ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(snapshot.ErrorDescription))
        {
            return snapshot.ErrorCode;
        }

        return $"{snapshot.ErrorCode}: {snapshot.ErrorDescription}";
    }

    #endregion
}