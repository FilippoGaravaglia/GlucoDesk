using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;

namespace GlucoDesk.Application.Cgm.Providers.Resolution.Models;

/// <summary>
/// Represents the resolved active live CGM provider and its metadata.
/// </summary>
public sealed record CgmLiveProviderResolution
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmLiveProviderResolution"/> class.
    /// </summary>
    /// <param name="metadata">The provider metadata.</param>
    /// <param name="liveProvider">The resolved live provider.</param>
    /// <param name="metadataProvider">The resolved metadata provider.</param>
    public CgmLiveProviderResolution(
        CgmProviderMetadata metadata,
        ICgmLiveProvider liveProvider,
        ICgmMetadataProvider metadataProvider)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(liveProvider);
        ArgumentNullException.ThrowIfNull(metadataProvider);

        Metadata = metadata;
        LiveProvider = liveProvider;
        MetadataProvider = metadataProvider;
    }

    /// <summary>
    /// Gets the provider metadata.
    /// </summary>
    public CgmProviderMetadata Metadata { get; }

    /// <summary>
    /// Gets the resolved live provider.
    /// </summary>
    public ICgmLiveProvider LiveProvider { get; }

    /// <summary>
    /// Gets the resolved metadata provider.
    /// </summary>
    public ICgmMetadataProvider MetadataProvider { get; }
}