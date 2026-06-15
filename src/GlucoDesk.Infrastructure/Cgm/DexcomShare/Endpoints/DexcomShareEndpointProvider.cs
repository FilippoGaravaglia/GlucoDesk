using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Endpoints;

/// <summary>
/// Resolves Dexcom Share endpoint URLs for a configured region.
/// </summary>
public sealed class DexcomShareEndpointProvider
{
    /// <summary>
    /// Gets the base endpoint for the supplied Dexcom Share region.
    /// </summary>
    /// <param name="region">The Dexcom Share region.</param>
    /// <returns>The Dexcom Share base endpoint.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the region is unsupported.</exception>
    public Uri GetBaseUri(DexcomShareRegion region)
    {
        return region switch
        {
            DexcomShareRegion.Us => new Uri("https://share2.dexcom.com"),
            DexcomShareRegion.OutsideUs => new Uri("https://shareous1.dexcom.com"),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unsupported Dexcom Share region.")
        };
    }
}