using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Default coordinator for GlucoDesk update checks and notification state.
/// </summary>
public sealed class UpdateCoordinator : IUpdateCoordinator
{
    private readonly IUpdateService _updateService;
    private readonly IUpdatePreferencesStore _preferencesStore;
    private readonly IApplicationVersionProvider _versionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCoordinator"/> class.
    /// </summary>
    public UpdateCoordinator(
        IUpdateService updateService,
        IUpdatePreferencesStore preferencesStore,
        IApplicationVersionProvider versionProvider)
    {
        _updateService = updateService;
        _preferencesStore = preferencesStore;
        _versionProvider = versionProvider;
    }

    /// <inheritdoc />
    public async Task<UpdateCenterSnapshot> CheckAsync(
        UpdateCheckTrigger trigger,
        CancellationToken cancellationToken)
    {
        var preferences = await _preferencesStore
            .LoadAsync(cancellationToken)
            .ConfigureAwait(false);

        if (trigger == UpdateCheckTrigger.Automatic
            && !preferences.CheckAutomatically)
        {
            return new UpdateCenterSnapshot
            {
                State = UpdateCenterState.Idle,
                CurrentVersion = _versionProvider.CurrentVersion,
                LastSuccessfulCheckAt =
                    preferences.LastSuccessfulCheckAt
            };
        }

        var result = await _updateService
            .CheckForUpdatesAsync(
                preferences.IncludePreviewReleases,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return new UpdateCenterSnapshot
            {
                State = UpdateCenterState.Error,
                CurrentVersion = result.CurrentVersion,
                LastSuccessfulCheckAt =
                    preferences.LastSuccessfulCheckAt,
                ErrorMessage = result.ErrorMessage
            };
        }

        var now = DateTimeOffset.UtcNow;

        preferences.LastSuccessfulCheckAt = now;

        await _preferencesStore
            .SaveAsync(
                preferences,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsUpdateAvailable
            || result.LatestRelease is null)
        {
            return new UpdateCenterSnapshot
            {
                State = UpdateCenterState.UpToDate,
                CurrentVersion = result.CurrentVersion,
                LastSuccessfulCheckAt = now
            };
        }

        var shouldShowDialog =
            trigger == UpdateCheckTrigger.Automatic
            && UpdateNotificationPolicy.ShouldNotify(
                result.LatestRelease,
                preferences,
                now);

        return new UpdateCenterSnapshot
        {
            State = UpdateCenterState.UpdateAvailable,
            CurrentVersion = result.CurrentVersion,
            LatestRelease = result.LatestRelease,
            LastSuccessfulCheckAt = now,
            ShouldShowDialog = shouldShowDialog
        };
    }

    /// <inheritdoc />
    public async Task DismissAsync(
        string version,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        var preferences = await _preferencesStore
            .LoadAsync(cancellationToken)
            .ConfigureAwait(false);

        preferences.LastDismissedVersion = version;
        preferences.LastDismissedAt = DateTimeOffset.UtcNow;

        await _preferencesStore
            .SaveAsync(
                preferences,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<UpdatePreferences> GetPreferencesAsync(
        CancellationToken cancellationToken)
    {
        return _preferencesStore.LoadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SavePreferencesAsync(
        UpdatePreferences preferences,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        return _preferencesStore.SaveAsync(
            preferences,
            cancellationToken);
    }
}
