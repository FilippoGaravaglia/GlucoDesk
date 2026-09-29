namespace GlucoDesk.Application.Updates;

/// <summary>
/// Provides the version of the currently running GlucoDesk application.
/// </summary>
public interface IApplicationVersionProvider
{
    /// <summary>
    /// Gets the current application version.
    /// </summary>
    string CurrentVersion { get; }
}
