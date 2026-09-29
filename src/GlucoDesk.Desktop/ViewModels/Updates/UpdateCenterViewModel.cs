using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;
using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.ViewModels.Common;

namespace GlucoDesk.Desktop.ViewModels.Updates;

/// <summary>
/// Represents the GlucoDesk update center.
/// </summary>
public sealed partial class UpdateCenterViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    private readonly IUpdatePreferencesStore _preferencesStore;
    private readonly TimeProvider _timeProvider;

    private UpdateRelease? _latestRelease;

    [ObservableProperty]
    private UpdateCenterState _state = UpdateCenterState.Idle;

    [ObservableProperty]
    private string _currentVersion;

    [ObservableProperty]
    private string? _latestVersion;

    [ObservableProperty]
    private string? _releaseNotes;

    [ObservableProperty]
    private string? _assetName;

    [ObservableProperty]
    private DateTimeOffset? _lastSuccessfulCheckAt;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UpdateCenterViewModel"/> class.
    /// </summary>
    /// <param name="updateService">Update application service.</param>
    /// <param name="versionProvider">
    /// Current application version provider.
    /// </param>
    /// <param name="preferencesStore">
    /// Update preferences persistence store.
    /// </param>
    /// <param name="timeProvider">Time provider.</param>
    public UpdateCenterViewModel(
        IUpdateService updateService,
        IApplicationVersionProvider versionProvider,
        IUpdatePreferencesStore preferencesStore,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(updateService);
        ArgumentNullException.ThrowIfNull(versionProvider);
        ArgumentNullException.ThrowIfNull(preferencesStore);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _updateService = updateService;
        _preferencesStore = preferencesStore;
        _timeProvider = timeProvider;

        CurrentVersion = versionProvider.CurrentVersion;

        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Gets a value indicating whether an update is available.
    /// </summary>
    public bool HasUpdate =>
        State == UpdateCenterState.UpdateAvailable;

    /// <summary>
    /// Gets a value indicating whether an update check is running.
    /// </summary>
    public bool IsChecking =>
        State == UpdateCenterState.Checking;

    /// <summary>
    /// Gets a value indicating whether GlucoDesk is up to date.
    /// </summary>
    public bool IsUpToDate =>
        State == UpdateCenterState.UpToDate;

    /// <summary>
    /// Gets a value indicating whether the latest update check failed.
    /// </summary>
    public bool HasError =>
        State == UpdateCenterState.Error;

    /// <summary>
    /// Gets the localized last-check description.
    /// </summary>
    public string LastCheckedText =>
        LastSuccessfulCheckAt is null
            ? LocalizationManager.GetString(
                "UpdatesNeverChecked")
            : string.Format(
                LocalizationManager.GetString(
                    "UpdatesLastCheckedFormat"),
                LastSuccessfulCheckAt.Value.ToLocalTime());

    /// <summary>
    /// Performs an update check.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    [RelayCommand]
    public async Task CheckNowAsync(
        CancellationToken cancellationToken)
    {
        State = UpdateCenterState.Checking;
        ErrorMessage = null;

        OnStateDependentPropertiesChanged();

        var preferences = await _preferencesStore
            .LoadAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await _updateService
            .CheckForUpdatesAsync(
                preferences.IncludePreviewReleases,
                cancellationToken)
            .ConfigureAwait(false);

        CurrentVersion = result.CurrentVersion;

        if (!result.IsSuccessful)
        {
            State = UpdateCenterState.Error;
            ErrorMessage = result.ErrorMessage;

            LastSuccessfulCheckAt =
                preferences.LastSuccessfulCheckAt;

            OnStateDependentPropertiesChanged();
            return;
        }

        var now = _timeProvider.GetUtcNow();

        preferences.LastSuccessfulCheckAt = now;

        await _preferencesStore
            .SaveAsync(preferences, cancellationToken)
            .ConfigureAwait(false);

        LastSuccessfulCheckAt = now;
        _latestRelease = result.LatestRelease;

        LatestVersion = _latestRelease?.Version;
        ReleaseNotes = _latestRelease?.ReleaseNotes;
        AssetName = _latestRelease?.AssetName;

        State = result.IsUpdateAvailable
            ? UpdateCenterState.UpdateAvailable
            : UpdateCenterState.UpToDate;

        OnStateDependentPropertiesChanged();
    }

    /// <summary>
    /// Starts downloading the latest compatible release.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDownloadUpdate))]
    private void DownloadUpdate()
    {
        if (_latestRelease is null)
        {
            return;
        }

        _updateService.Download(_latestRelease);
    }

    #region Helpers

    /// <summary>
    /// Determines whether the latest update can be downloaded.
    /// </summary>
    private bool CanDownloadUpdate()
    {
        return _latestRelease is not null
               && State == UpdateCenterState.UpdateAvailable;
    }

    /// <summary>
    /// Refreshes derived properties after the state changes.
    /// </summary>
    private void OnStateDependentPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasUpdate));
        OnPropertyChanged(nameof(IsChecking));
        OnPropertyChanged(nameof(IsUpToDate));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(LastCheckedText));

        DownloadUpdateCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Refreshes localized update-center values after a language change.
    /// </summary>
    private void OnLanguageChanged(
        object? sender,
        EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        OnPropertyChanged(nameof(LastCheckedText));
    }

    #endregion
}
