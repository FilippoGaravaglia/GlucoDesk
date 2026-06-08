using GlucoDesk.Application.Settings.Events;
using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Abstractions;

/// <summary>
/// Defines an in-process notification mechanism for application settings changes.
/// </summary>
public interface IApplicationSettingsChangeNotifier
{
    /// <summary>
    /// Occurs when application settings have been changed and saved successfully.
    /// </summary>
    event EventHandler<ApplicationSettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Notifies subscribers that application settings have changed.
    /// </summary>
    /// <param name="settings">The changed application settings.</param>
    void NotifySettingsChanged(ApplicationSettings settings);
}