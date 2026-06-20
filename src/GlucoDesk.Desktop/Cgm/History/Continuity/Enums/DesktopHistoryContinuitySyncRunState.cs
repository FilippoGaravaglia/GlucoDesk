namespace GlucoDesk.Desktop.Cgm.History.Continuity.Enums;

/// <summary>
/// Represents the current desktop history continuity synchronization state.
/// </summary>
public enum DesktopHistoryContinuitySyncRunState
{
    Idle = 0,
    Running = 1,
    Succeeded = 2,
    Skipped = 3,
    Failed = 4,
    Canceled = 5
}