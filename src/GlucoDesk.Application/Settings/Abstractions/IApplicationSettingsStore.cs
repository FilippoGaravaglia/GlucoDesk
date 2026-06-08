using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Abstractions;

/// <summary>
/// Defines persistence operations for GlucoDesk application settings.
/// </summary>
public interface IApplicationSettingsStore
{
    /// <summary>
    /// Loads application settings from the configured storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded application settings.</returns>
    Task<Result<ApplicationSettings>> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves application settings to the configured storage.
    /// </summary>
    /// <param name="settings">The application settings to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The save operation result.</returns>
    Task<Result> SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken);
}