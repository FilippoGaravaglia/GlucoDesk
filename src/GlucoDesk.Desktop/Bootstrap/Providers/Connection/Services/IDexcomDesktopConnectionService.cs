using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Models;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;

/// <summary>
/// Defines desktop Dexcom connection actions.
/// </summary>
public interface IDexcomDesktopConnectionService
{
    /// <summary>
    /// Starts the Dexcom OAuth connection flow from the desktop application.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop Dexcom connection result.</returns>
    Task<Result<DexcomDesktopConnectionResult>> ConnectAsync(CancellationToken cancellationToken);
}