namespace GlucoDesk.Core.Updates;

/// <summary>
/// Represents the current state of the GlucoDesk update center.
/// </summary>
public enum UpdateCenterState
{
    /// <summary>
    /// No update check has been performed yet.
    /// </summary>
    Idle,

    /// <summary>
    /// An update check is currently running.
    /// </summary>
    Checking,

    /// <summary>
    /// The current GlucoDesk installation is up to date.
    /// </summary>
    UpToDate,

    /// <summary>
    /// A newer compatible release is available.
    /// </summary>
    UpdateAvailable,

    /// <summary>
    /// The latest release could not be checked.
    /// </summary>
    Error
}
