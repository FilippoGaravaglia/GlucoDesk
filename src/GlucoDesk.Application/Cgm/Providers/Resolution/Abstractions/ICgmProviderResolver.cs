using GlucoDesk.Application.Cgm.Providers.Resolution.Models;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;

/// <summary>
/// Resolves the active CGM providers based on the current application settings.
/// </summary>
public interface ICgmProviderResolver
{
    /// <summary>
    /// Resolves the active live CGM provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active live provider resolution.</returns>
    Task<Result<CgmLiveProviderResolution>> ResolveActiveLiveProviderAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the active historical CGM provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active historical provider resolution.</returns>
    Task<Result<CgmHistoricalProviderResolution>> ResolveActiveHistoricalProviderAsync(
        CancellationToken cancellationToken);
}