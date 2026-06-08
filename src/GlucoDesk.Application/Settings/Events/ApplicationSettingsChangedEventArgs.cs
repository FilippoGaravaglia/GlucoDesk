using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Settings.Events;

/// <summary>
/// Provides data for application settings change notifications.
/// </summary>
public sealed class ApplicationSettingsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSettingsChangedEventArgs"/> class.
    /// </summary>
    /// <param name="settings">The changed application settings.</param>
    public ApplicationSettingsChangedEventArgs(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }

    /// <summary>
    /// Gets the changed application settings.
    /// </summary>
    public ApplicationSettings Settings { get; }
}