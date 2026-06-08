using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

/// <summary>
/// Defines Dexcom OAuth authorization session orchestration.
/// </summary>
public interface IDexcomOAuthAuthorizationSessionService
{
    /// <summary>
    /// Starts a Dexcom OAuth authorization session.
    /// </summary>
    /// <param name="request">The authorization session request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The completed authorization session result.</returns>
    Task<Result<DexcomOAuthAuthorizationSessionResult>> StartAuthorizationSessionAsync(
        DexcomOAuthAuthorizationSessionRequest request,
        CancellationToken cancellationToken);
}