using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Events;
using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Services;

/// <summary>
/// Provides in-process application settings change notifications.
/// </summary>
public sealed class ApplicationSettingsChangeNotifier : IApplicationSettingsChangeNotifier
{
    /// <inheritdoc />
    public event EventHandler<ApplicationSettingsChangedEventArgs>? SettingsChanged;

    /// <inheritdoc />
    public void NotifySettingsChanged(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        SettingsChanged?.Invoke(this, new ApplicationSettingsChangedEventArgs(settings));
    }
}