namespace GlucoDesk.Application.Cgm.History.Continuity.Enums;

/// <summary>
/// Defines the reason that triggered a local CGM history continuity synchronization.
/// </summary>
public enum CgmHistoryContinuitySyncTrigger
{
    /// <summary>
    /// Synchronization was triggered when the application started.
    /// </summary>
    Startup = 1,

    /// <summary>
    /// Synchronization was triggered when the application resumed after being inactive.
    /// </summary>
    Resume = 2,

    /// <summary>
    /// Synchronization was triggered manually by the user or by an explicit workflow.
    /// </summary>
    Manual = 3
}