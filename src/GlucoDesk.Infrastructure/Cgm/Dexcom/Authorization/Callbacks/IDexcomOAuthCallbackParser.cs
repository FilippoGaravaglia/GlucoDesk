using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

/// <summary>
/// Defines Dexcom OAuth callback parsing and validation.
/// </summary>
public interface IDexcomOAuthCallbackParser
{
    /// <summary>
    /// Parses and validates a Dexcom OAuth callback URI.
    /// </summary>
    /// <param name="callbackUri">The callback URI received after OAuth authorization.</param>
    /// <param name="expectedState">The expected OAuth state generated before opening the authorization URL.</param>
    /// <returns>The parsed OAuth callback result.</returns>
    Result<DexcomOAuthCallbackResult> ParseCallback(
        Uri callbackUri,
        string expectedState);
}