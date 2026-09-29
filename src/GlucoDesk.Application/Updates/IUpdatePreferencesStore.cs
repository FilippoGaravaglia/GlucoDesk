using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Persists update-related preferences and local notification state.
/// </summary>
public interface IUpdatePreferencesStore
{
    /// <summary>
    /// Loads update preferences.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>The persisted preferences or their defaults.</returns>
    Task<UpdatePreferences> LoadAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists update preferences.
    /// </summary>
    /// <param name="preferences">
    /// Preferences to persist.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task SaveAsync(
        UpdatePreferences preferences,
        CancellationToken cancellationToken);
}
