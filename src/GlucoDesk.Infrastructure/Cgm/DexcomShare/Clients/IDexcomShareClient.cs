using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;

/// <summary>
/// Defines Dexcom Share HTTP operations.
/// </summary>
public interface IDexcomShareClient
{
    /// <summary>
    /// Authenticates Dexcom Share using the currently configured credentials.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session identifier.</returns>
    Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Authenticates Dexcom Share using explicit credentials without persisting them.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session identifier.</returns>
    Task<Result<string>> AuthenticateAsync(
        DexcomShareOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the latest Dexcom Share glucose values.
    /// </summary>
    /// <param name="sessionId">The Dexcom Share session identifier.</param>
    /// <param name="minutes">The lookback window in minutes.</param>
    /// <param name="maxCount">The maximum number of readings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest Dexcom Share glucose values.</returns>
    Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
        string sessionId,
        int minutes,
        int maxCount,
        CancellationToken cancellationToken);
}