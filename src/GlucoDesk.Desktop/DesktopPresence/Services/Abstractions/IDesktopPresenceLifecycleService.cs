using Avalonia.Controls.ApplicationLifetimes;

namespace GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

/// <summary>
/// Manages the lifecycle of the desktop presence indicator.
/// </summary>
public interface IDesktopPresenceLifecycleService
{
    /// <summary>
    /// Starts the desktop presence indicator.
    /// </summary>
    /// <param name="desktopLifetime">The classic desktop application lifetime.</param>
    void Start(IClassicDesktopStyleApplicationLifetime desktopLifetime);

    /// <summary>
    /// Stops the desktop presence indicator and releases UI resources.
    /// </summary>
    void Stop();
}
