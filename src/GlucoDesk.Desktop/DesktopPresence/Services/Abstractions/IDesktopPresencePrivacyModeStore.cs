namespace GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

/// <summary>
/// Persists the desktop presence privacy mode state.
/// </summary>
public interface IDesktopPresencePrivacyModeStore
{
    /// <summary>
    /// Loads the persisted privacy mode state.
    /// </summary>
    /// <returns>True when privacy mode should be enabled; otherwise, false.</returns>
    bool Load();

    /// <summary>
    /// Saves the privacy mode state.
    /// </summary>
    /// <param name="isEnabled">Whether privacy mode is enabled.</param>
    void Save(bool isEnabled);
}
