namespace GlucoDesk.Desktop.DesktopPresence.Models;

/// <summary>
/// Represents formatted text for the desktop presence indicator.
/// </summary>
/// <param name="Tooltip">The text shown in the tray or menu bar tooltip.</param>
/// <param name="MenuHeader">The text shown as the top menu entry.</param>
public sealed record DesktopPresenceText(
    string Tooltip,
    string MenuHeader);
