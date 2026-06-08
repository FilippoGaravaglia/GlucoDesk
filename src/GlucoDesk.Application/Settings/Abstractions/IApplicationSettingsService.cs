using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Abstractions;

/// <summary>
/// Defines application-level operations for reading and updating GlucoDesk settings.
/// </summary>
public interface IApplicationSettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current application settings.</returns>
    Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the supplied application settings.
    /// </summary>
    /// <param name="settings">The application settings to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The save operation result.</returns>
    Task<Result> SaveSettingsAsync(ApplicationSettings settings, CancellationToken cancellationToken);
}