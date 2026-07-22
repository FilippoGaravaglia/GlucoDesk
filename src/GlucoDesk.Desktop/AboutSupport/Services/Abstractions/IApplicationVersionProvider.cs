namespace GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

/// <summary>
/// Provides the user-facing GlucoDesk application version.
/// </summary>
public interface IApplicationVersionProvider
{
    /// <summary>
    /// Gets the current user-facing application version.
    /// </summary>
    /// <returns>The normalized version text.</returns>
    string GetVersion();
}
