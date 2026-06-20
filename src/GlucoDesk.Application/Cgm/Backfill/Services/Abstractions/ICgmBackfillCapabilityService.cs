using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Provides the historical backfill capability for the active CGM provider.
/// </summary>
public interface ICgmBackfillCapabilityService
{
    /// <summary>
    /// Gets the historical backfill capability for the active CGM provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill capability result.</returns>
    Task<Result<CgmBackfillCapability>> GetCapabilityAsync(CancellationToken cancellationToken);
}