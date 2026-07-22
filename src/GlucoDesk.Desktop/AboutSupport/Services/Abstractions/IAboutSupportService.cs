using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.AboutSupport.Enums;
using GlucoDesk.Desktop.AboutSupport.Models;

namespace GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

/// <summary>
/// Provides GlucoDesk product information and safe support navigation.
/// </summary>
public interface IAboutSupportService
{
    /// <summary>
    /// Gets the current product and support information.
    /// </summary>
    /// <returns>The current public information.</returns>
    AboutSupportInformation GetInformation();

    /// <summary>
    /// Opens one supported GlucoDesk destination.
    /// </summary>
    /// <param name="linkKind">The destination to open.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result when the destination was opened.</returns>
    Task<Result> OpenAsync(
        AboutSupportLinkKind linkKind,
        CancellationToken cancellationToken);
}
