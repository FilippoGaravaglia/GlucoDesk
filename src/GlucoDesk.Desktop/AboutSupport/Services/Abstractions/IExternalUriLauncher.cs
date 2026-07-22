using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

/// <summary>
/// Opens trusted external URI destinations through the operating system.
/// </summary>
public interface IExternalUriLauncher
{
    /// <summary>
    /// Opens the supplied absolute HTTPS URI.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result when the operating system accepted the request.</returns>
    Task<Result> OpenAsync(
        Uri uri,
        CancellationToken cancellationToken);
}
