using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.DesktopPresence.Formatters;

/// <summary>
/// Formats dashboard presentation state into desktop presence text.
/// </summary>
public interface IDesktopPresenceDashboardTextFormatter
{
    /// <summary>
    /// Formats the specified dashboard state.
    /// </summary>
    /// <param name="state">The dashboard presentation state.</param>
    /// <returns>The formatted desktop presence text.</returns>
    DesktopPresenceText Format(DesktopPresenceDashboardState state);
}
