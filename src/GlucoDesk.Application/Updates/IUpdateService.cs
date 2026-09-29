using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Coordinates update discovery and download for GlucoDesk.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks the official release source for a newer compatible application
    /// version.
    /// </summary>
    /// <param name="includePreviewReleases">
    /// Whether preview releases may be considered.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>The update-check result.</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync(
        bool includePreviewReleases,
        CancellationToken cancellationToken);

    /// <summary>
    /// Starts downloading the release asset that matches the current operating
    /// system and architecture.
    /// </summary>
    /// <param name="release">Release to download.</param>
    void Download(UpdateRelease release);
}
