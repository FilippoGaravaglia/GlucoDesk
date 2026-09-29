using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Coordinates update discovery, persisted preferences and notification policy.
/// </summary>
public interface IUpdateCoordinator
{
    /// <summary>
    /// Checks for an application update and returns presentation-ready state.
    /// </summary>
    /// <param name="trigger">
    /// Indicates whether the check was automatic or manually requested.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>The resulting update-center snapshot.</returns>
    Task<UpdateCenterSnapshot> CheckAsync(
        UpdateCheckTrigger trigger,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists that the supplied release notification was dismissed.
    /// </summary>
    /// <param name="version">Dismissed release version.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task DismissAsync(
        string version,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads the persisted update preferences.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>The persisted update preferences.</returns>
    Task<UpdatePreferences> GetPreferencesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Saves update preferences.
    /// </summary>
    /// <param name="preferences">Preferences to persist.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task SavePreferencesAsync(
        UpdatePreferences preferences,
        CancellationToken cancellationToken);
}
