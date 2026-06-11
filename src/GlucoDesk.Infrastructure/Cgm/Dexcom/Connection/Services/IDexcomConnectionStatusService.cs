using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;

/// <summary>
/// Defines operations for inspecting the current Dexcom connection status.
/// </summary>
public interface IDexcomConnectionStatusService
{
    /// <summary>
    /// Gets the current Dexcom connection status without exposing OAuth secrets.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current Dexcom connection status.</returns>
    Task<Result<DexcomConnectionStatus>> GetConnectionStatusAsync(CancellationToken cancellationToken);
}