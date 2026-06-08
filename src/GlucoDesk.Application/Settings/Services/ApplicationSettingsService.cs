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

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSettingsService"/> class.
    /// </summary>
    /// <param name="settingsStore">The application settings store.</param>
    public ApplicationSettingsService(IApplicationSettingsStore settingsStore)
    {
        ArgumentNullException.ThrowIfNull(settingsStore);

        _settingsStore = settingsStore;
    }

    /// <inheritdoc />
    public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
    {
        return _settingsStore.LoadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> SaveSettingsAsync(
        ApplicationSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return _settingsStore.SaveAsync(settings, cancellationToken);
    }
}