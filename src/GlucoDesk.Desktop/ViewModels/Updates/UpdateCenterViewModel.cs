using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Desktop.ViewModels.Updates;

/// <summary>
/// Exposes GlucoDesk update-center operations and state to the desktop UI.
/// </summary>
public sealed partial class UpdateCenterViewModel : ObservableObject
{
    private readonly IUpdateCoordinator _updateCoordinator;
    private readonly IUpdateService _updateService;
    private readonly UpdateCenterStore _store;

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
    /// Gets the latest release notes.
    /// </summary>
    public string? ReleaseNotes =>
        Snapshot.LatestRelease?.ReleaseNotes;

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
    [RelayCommand]
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
        var release = Snapshot.LatestRelease;

        if (release is null)
        {
            return;
        }

        await _updateCoordinator.DismissAsync(
            release.Version,
            cancellationToken);

        var refreshed = Snapshot with
        {
            ShouldShowDialog = false
        };

        _store.Snapshot = refreshed;

        NotifySnapshotProperties();
    }

    /// <summary>
    /// Performs the startup update check without blocking application startup.
    /// </summary>
    public Task CheckOnStartupAsync(
        CancellationToken cancellationToken)
    {
        return ExecuteCheckAsync(
            UpdateCheckTrigger.Automatic,
            cancellationToken);
    }

    #region Helpers

    /// <summary>
    /// Executes an update check and publishes its result to the shared UI store.
    /// </summary>
    private async Task ExecuteCheckAsync(
        UpdateCheckTrigger trigger,
        CancellationToken cancellationToken)
    {
        _store.Snapshot = Snapshot with
        {
            State = UpdateCenterState.Checking,
            ErrorMessage = null,
            ShouldShowDialog = false
        };

        NotifySnapshotProperties();

        var snapshot = await _updateCoordinator.CheckAsync(
            trigger,
            cancellationToken);

        _store.Snapshot = snapshot;

        NotifySnapshotProperties();
    }

    /// <summary>
    /// Determines whether the current release can be downloaded.
    /// </summary>
    private bool CanDownloadUpdate()
    {
        return Snapshot.State == UpdateCenterState.UpdateAvailable
               && Snapshot.LatestRelease is not null;
    }

    /// <summary>
    /// Determines whether the current release notification can be dismissed.
    /// </summary>
    private bool CanDismissUpdate()
    {
        return Snapshot.State == UpdateCenterState.UpdateAvailable
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
        OnPropertyChanged(nameof(CurrentVersion));
        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(ReleaseNotes));
        OnPropertyChanged(nameof(AssetName));
        OnPropertyChanged(nameof(LastSuccessfulCheckAt));
        OnPropertyChanged(nameof(ErrorMessage));

        DownloadUpdateCommand.NotifyCanExecuteChanged();
        DismissCommand.NotifyCanExecuteChanged();
        CheckNowCommand.NotifyCanExecuteChanged();
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

    #endregion
}
