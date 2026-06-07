using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Providers.Abstractions;

/// <summary>
/// Defines a CGM provider capable of exposing provider metadata.
/// </summary>
public interface ICgmMetadataProvider
{
    /// <summary>
    /// Gets metadata describing the provider capabilities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The provider metadata result.</returns>
    Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken);
}