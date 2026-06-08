using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;

/// <summary>
/// Defines OAuth authorization URL generation for Dexcom Official API.
/// </summary>
public interface IDexcomAuthorizationUrlBuilder
{
    /// <summary>
    /// Builds a Dexcom OAuth authorization URI.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="request">The authorization request.</param>
    /// <returns>The Dexcom OAuth authorization URI.</returns>
    Uri BuildAuthorizationUri(
        DexcomApiEnvironment environment,
        DexcomAuthorizationRequest request);
}