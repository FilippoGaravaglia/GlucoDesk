using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;

namespace GlucoDesk.Application.Cgm.Providers.Resolution.Models;

/// <summary>
/// Represents the resolved active historical CGM provider and its metadata.
/// </summary>
public sealed record CgmHistoricalProviderResolution
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmHistoricalProviderResolution"/> class.
    /// </summary>
    /// <param name="metadata">The provider metadata.</param>
    /// <param name="historicalProvider">The resolved historical provider.</param>
    /// <param name="metadataProvider">The resolved metadata provider.</param>
    public CgmHistoricalProviderResolution(
        CgmProviderMetadata metadata,
        ICgmHistoricalProvider historicalProvider,
        ICgmMetadataProvider metadataProvider)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(historicalProvider);
        ArgumentNullException.ThrowIfNull(metadataProvider);

        Metadata = metadata;
        HistoricalProvider = historicalProvider;
        MetadataProvider = metadataProvider;
    }

    /// <summary>
    /// Gets the provider metadata.
    /// </summary>
    public CgmProviderMetadata Metadata { get; }

    /// <summary>
    /// Gets the resolved historical provider.
    /// </summary>
    public ICgmHistoricalProvider HistoricalProvider { get; }

    /// <summary>
    /// Gets the resolved metadata provider.
    /// </summary>
    public ICgmMetadataProvider MetadataProvider { get; }
}