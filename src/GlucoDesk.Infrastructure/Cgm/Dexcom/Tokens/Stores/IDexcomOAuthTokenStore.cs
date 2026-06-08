using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

/// <summary>
/// Defines storage operations for Dexcom OAuth tokens.
/// </summary>
public interface IDexcomOAuthTokenStore
{
    /// <summary>
    /// Saves the current Dexcom OAuth token set.
    /// </summary>
    /// <param name="tokenSet">The token set to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<Result> SaveTokenSetAsync(
        DexcomOAuthTokenSet tokenSet,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the currently stored Dexcom OAuth token set.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored token set.</returns>
    Task<Result<DexcomOAuthTokenSet>> GetTokenSetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a Dexcom OAuth token set is currently stored.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when a token set is stored; otherwise, false.</returns>
    Task<Result<bool>> HasTokenSetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Clears the currently stored Dexcom OAuth token set.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<Result> ClearTokenSetAsync(CancellationToken cancellationToken);
}