using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;

/// <summary>
/// Defines endpoint resolution for Dexcom Official API environments.
/// </summary>
public interface IDexcomApiEndpointProvider
{
    /// <summary>
    /// Gets the Dexcom API endpoints for the specified environment.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The Dexcom API endpoints.</returns>
    DexcomApiEndpoints GetEndpoints(DexcomApiEnvironment environment);
}