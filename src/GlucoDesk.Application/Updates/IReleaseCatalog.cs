using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Provides GlucoDesk releases from an external release source.
/// </summary>
public interface IReleaseCatalog
{
    /// <summary>
    /// Retrieves the latest compatible GlucoDesk release.
    /// </summary>
    /// <param name="includePreviewReleases">
    /// Whether preview releases may be returned.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The latest compatible release, or <see langword="null"/> when none can be
    /// selected for the current platform.
    /// </returns>
    Task<UpdateRelease?> GetLatestAsync(
        bool includePreviewReleases,
        CancellationToken cancellationToken);
}
