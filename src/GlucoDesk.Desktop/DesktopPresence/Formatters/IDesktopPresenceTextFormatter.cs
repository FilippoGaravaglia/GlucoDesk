using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.DesktopPresence.Formatters;

/// <summary>
/// Formats desktop presence snapshots into user-facing text.
/// </summary>
public interface IDesktopPresenceTextFormatter
{
    /// <summary>
    /// Formats the specified snapshot.
    /// </summary>
    /// <param name="snapshot">The desktop presence snapshot.</param>
    /// <returns>The formatted desktop presence text.</returns>
    DesktopPresenceText Format(DesktopPresenceSnapshot snapshot);
}
