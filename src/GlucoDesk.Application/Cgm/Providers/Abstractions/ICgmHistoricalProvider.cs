using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Providers.Abstractions;

/// <summary>
/// Defines a CGM provider capable of returning historical glucose readings.
/// </summary>
public interface ICgmHistoricalProvider
{
    /// <summary>
    /// Gets historical glucose readings for the requested time range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The historical glucose readings result.</returns>
    Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken);
}