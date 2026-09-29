namespace GlucoDesk.Core.Updates;

/// <summary>
/// Identifies what initiated an update check.
/// </summary>
public enum UpdateCheckTrigger
{
    /// <summary>
    /// The application initiated the check automatically.
    /// </summary>
    Automatic,

    /// <summary>
    /// The user explicitly requested the check.
    /// </summary>
    Manual
}
