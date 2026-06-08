using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;

/// <summary>
/// Defines Dexcom EGV API operations.
/// </summary>
public interface IDexcomEgvClient
{
    /// <summary>
    /// Gets Dexcom estimated glucose value records for the requested time range.
    /// </summary>
    /// <param name="request">The Dexcom EGV request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom EGV API response.</returns>
    Task<Result<DexcomEgvResponseDto>> GetEgvsAsync(
        DexcomEgvRequest request,
        CancellationToken cancellationToken);
}