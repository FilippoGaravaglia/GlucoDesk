using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

/// <summary>
/// Provides Dexcom Share options from the current runtime configuration.
/// </summary>
public interface IDexcomShareOptionsProvider
{
    /// <summary>
    /// Gets the current Dexcom Share options.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current Dexcom Share options.</returns>
    Task<Result<DexcomShareOptions>> GetOptionsAsync(CancellationToken cancellationToken);
}