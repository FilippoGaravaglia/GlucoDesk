using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.Updates.Presentation;

namespace GlucoDesk.Desktop.ViewModels.Updates;

/// <summary>
/// Exposes GlucoDesk update-center operations and state to the desktop UI.
/// </summary>
public sealed partial class UpdateCenterViewModel :
    ViewModelBase,
    IDisposable
{
    private readonly IUpdateCoordinator _updateCoordinator;
    private readonly IUpdateService _updateService;
    private readonly UpdateCenterStore _store;

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UpdateCenterViewModel"/> class.
    /// </summary>
    public UpdateCenterViewModel(
        IUpdateCoordinator updateCoordinator,
        IUpdateService updateService,
        UpdateCenterStore store)
    {
        _updateCoordinator = updateCoordinator;
        _updateService = updateService;
        _store = store;

        _store.PropertyChanged += OnStorePropertyChanged;
        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Gets the current update-center snapshot.
    /// </summary>
    public UpdateCenterSnapshot Snapshot =>
        _store.Snapshot;

    /// <summary>
    /// Gets a value indicating whether an update is available.
    /// </summary>
    public bool HasUpdate =>
        _store.HasUpdate;

    /// <summary>
    /// Gets a value indicating whether an update check is running.
    /// </summary>
    public bool IsChecking =>
        _store.IsChecking;

    /// <summary>
    /// Gets a value indicating whether GlucoDesk is currently up to date.
    /// </summary>
    public bool IsUpToDate =>
        _store.IsUpToDate;

    /// <summary>
    /// Gets a value indicating whether the latest update check failed.
    /// </summary>
    public bool HasError =>
        _store.HasError;

    /// <summary>
    /// Gets the badge text displayed by the application navigation.
    /// </summary>
    public string? BadgeText =>
        _store.BadgeText;

    /// <summary>
    /// Gets the installed GlucoDesk version.
    /// </summary>
    public string CurrentVersion =>
        Snapshot.CurrentVersion;

    /// <summary>
    /// Gets the latest available GlucoDesk version.
    /// </summary>
    public string? LatestVersion =>
        Snapshot.LatestRelease?.Version;

    /// <summary>
    /// Gets the localized user-facing release notes.
    /// </summary>
    public string ReleaseNotes =>
        ReleaseNotesPresenter.Format(
            Snapshot.LatestRelease?.ReleaseNotes,
            LocalizationManager.GetString(
                "CarbGuideLanguageCode"));

    /// <summary>
    /// Gets the latest compatible asset name.
    /// </summary>
    public string? AssetName =>
        Snapshot.LatestRelease?.AssetName;

    /// <summary>
    /// Gets the last successful update-check timestamp.
    /// </summary>
    public DateTimeOffset? LastSuccessfulCheckAt =>
        Snapshot.LastSuccessfulCheckAt;

    /// <summary>
    /// Gets the error returned by the latest failed update check.
    /// </summary>
    public string? ErrorMessage =>
        Snapshot.ErrorMessage;

    /// <summary>
    /// Checks manually for a newer GlucoDesk version.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCheckNow))]
    private async Task CheckNowAsync(
        CancellationToken cancellationToken)
    {
        await ExecuteCheckAsync(
            UpdateCheckTrigger.Manual,
            cancellationToken);
    }

    /// <summary>
    /// Downloads the compatible release asset in the user's browser.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDownloadUpdate))]
    private void DownloadUpdate()
    {
        ThrowIfDisposed();

        var release = Snapshot.LatestRelease;

        if (release is null)
        {
            return;
        }

        _updateService.Download(release);
    }

    /// <summary>
    /// Dismisses the currently available update notification.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDismissUpdate))]
    private async Task DismissAsync(
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        var release = Snapshot.LatestRelease;

        if (release is null)
        {
            return;
        }

        await _updateCoordinator.DismissAsync(
            release.Version,
            cancellationToken);

        _store.Snapshot = Snapshot with
        {
            ShouldShowDialog = false
        };
    }

    /// <summary>
    /// Performs the startup update check without blocking application startup.
    /// </summary>
    public Task CheckOnStartupAsync(
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        return ExecuteCheckAsync(
            UpdateCheckTrigger.Automatic,
            cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _store.PropertyChanged -= OnStorePropertyChanged;
        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        _isDisposed = true;
    }

    #region Helpers

    /// <summary>
    /// Executes an update check and publishes its result to the shared UI store.
    /// </summary>
    private async Task ExecuteCheckAsync(
        UpdateCheckTrigger trigger,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (IsChecking)
        {
            return;
        }

        _store.Snapshot = Snapshot with
        {
            State = UpdateCenterState.Checking,
            ErrorMessage = null,
            ShouldShowDialog = false
        };

        try
        {
            var snapshot = await _updateCoordinator.CheckAsync(
                trigger,
                cancellationToken);

            _store.Snapshot = snapshot;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    /// <summary>
    /// Determines whether a manual update check can be started.
    /// </summary>
    private bool CanCheckNow()
    {
        return !_isDisposed && !IsChecking;
    }

    /// <summary>
    /// Determines whether the current release can be downloaded.
    /// </summary>
    private bool CanDownloadUpdate()
    {
        return !_isDisposed
               && Snapshot.State == UpdateCenterState.UpdateAvailable
               && Snapshot.LatestRelease is not null;
    }

    /// <summary>
    /// Determines whether the current release notification can be dismissed.
    /// </summary>
    private bool CanDismissUpdate()
    {
        return !_isDisposed
               && Snapshot.State == UpdateCenterState.UpdateAvailable
               && Snapshot.LatestRelease is not null;
    }

    /// <summary>
    /// Refreshes properties derived from the current update snapshot.
    /// </summary>
    private void NotifySnapshotProperties()
    {
        OnPropertyChanged(nameof(Snapshot));
        OnPropertyChanged(nameof(HasUpdate));
        OnPropertyChanged(nameof(IsChecking));
        OnPropertyChanged(nameof(IsUpToDate));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(BadgeText));
        OnPropertyChanged(nameof(CurrentVersion));
        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(ReleaseNotes));
        OnPropertyChanged(nameof(AssetName));
        OnPropertyChanged(nameof(LastSuccessfulCheckAt));
        OnPropertyChanged(nameof(ErrorMessage));

        CheckNowCommand.NotifyCanExecuteChanged();
        DownloadUpdateCommand.NotifyCanExecuteChanged();
        DismissCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Refreshes UI properties whenever the shared update-center state changes.
    /// </summary>
    private void OnStorePropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        NotifySnapshotProperties();
    }

    /// <summary>
    /// Refreshes localized release notes when the application language changes.
    /// </summary>
    private void OnLanguageChanged(
        object? sender,
        EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        OnPropertyChanged(nameof(ReleaseNotes));
    }

    /// <summary>
    /// Throws when the view model has already been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _isDisposed,
            this);
    }

    #endregion
}
