using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;

/// <summary>
/// Provides Nightscout entries API operations.
/// </summary>
public interface INightscoutEntriesClient
{
    /// <summary>
    /// Gets glucose entries from Nightscout.
    /// </summary>
    /// <param name="request">The entries request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Nightscout glucose entries.</returns>
    Task<Result<IReadOnlyList<NightscoutEntryDto>>> GetEntriesAsync(
        NightscoutEntriesRequest request,
        CancellationToken cancellationToken);
}