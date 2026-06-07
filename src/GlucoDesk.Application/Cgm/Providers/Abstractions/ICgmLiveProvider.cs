using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Providers.Abstractions;

/// <summary>
/// Defines a CGM provider capable of returning live or near real-time glucose readings.
/// </summary>
public interface ICgmLiveProvider
{
    /// <summary>
    /// Gets the latest available glucose reading.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest glucose reading result.</returns>
    Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets recent glucose readings for the requested time range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose readings result.</returns>
    Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken);
}