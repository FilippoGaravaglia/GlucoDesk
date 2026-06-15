using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;

/// <summary>
/// Defines Dexcom Share HTTP operations.
/// </summary>
public interface IDexcomShareClient
{
    /// <summary>
    /// Authenticates the configured Dexcom Share publisher account.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session identifier.</returns>
    Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the latest glucose values from Dexcom Share.
    /// </summary>
    /// <param name="sessionId">The Dexcom Share session identifier.</param>
    /// <param name="minutes">The lookback window expressed in minutes.</param>
    /// <param name="maxCount">The maximum number of readings to request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose value DTOs.</returns>
    Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
        string sessionId,
        int minutes,
        int maxCount,
        CancellationToken cancellationToken);
}