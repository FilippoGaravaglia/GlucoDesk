namespace GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

/// <summary>
/// Coordinates the runtime desktop presence privacy mode state.
/// </summary>
public interface IDesktopPresencePrivacyModeService
{
    /// <summary>
    /// Occurs when the privacy mode state changes.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Gets a value indicating whether privacy mode is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Reloads privacy mode state from persistence.
    /// </summary>
    void Reload();

    /// <summary>
    /// Sets privacy mode state.
    /// </summary>
    /// <param name="isEnabled">Whether privacy mode should be enabled.</param>
    void SetEnabled(bool isEnabled);

    /// <summary>
    /// Toggles privacy mode state.
    /// </summary>
    void Toggle();
}
