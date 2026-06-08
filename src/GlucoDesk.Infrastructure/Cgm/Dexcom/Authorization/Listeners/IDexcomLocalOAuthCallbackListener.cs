using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

/// <summary>
/// Defines a local listener for Dexcom OAuth callback redirects.
/// </summary>
public interface IDexcomLocalOAuthCallbackListener
{
    /// <summary>
    /// Waits for a Dexcom OAuth callback on the configured local redirect URI.
    /// </summary>
    /// <param name="request">The local OAuth callback listen request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The received and validated OAuth callback result.</returns>
    Task<Result<DexcomLocalOAuthCallbackListenResult>> ListenForCallbackAsync(
        DexcomLocalOAuthCallbackListenRequest request,
        CancellationToken cancellationToken);
}