using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Services;

/// <summary>
/// Provides application-level operations for GlucoDesk settings.
/// </summary>
public sealed class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly IApplicationSettingsStore _settingsStore;
    private readonly IApplicationSettingsChangeNotifier? _settingsChangeNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSettingsService"/> class.
    /// </summary>
    /// <param name="settingsStore">The application settings store.</param>
    /// <param name="settingsChangeNotifier">The optional application settings change notifier.</param>
    public ApplicationSettingsService(
        IApplicationSettingsStore settingsStore,
        IApplicationSettingsChangeNotifier? settingsChangeNotifier = null)
    {
        ArgumentNullException.ThrowIfNull(settingsStore);

        _settingsStore = settingsStore;
        _settingsChangeNotifier = settingsChangeNotifier;
    }

    /// <inheritdoc />
    public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
    {
        return _settingsStore.LoadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> SaveSettingsAsync(
        ApplicationSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = await _settingsStore
            .SaveAsync(settings, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _settingsChangeNotifier?.NotifySettingsChanged(settings);
        }

        return result;
    }
}