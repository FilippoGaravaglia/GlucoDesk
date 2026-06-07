using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Services.Abstractions;

/// <summary>
/// Defines application-level operations used to retrieve glucose data for the UI and reporting layers.
/// </summary>
public interface IGlucoseDataService
{
    /// <summary>
    /// Gets the metadata of the active CGM provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The active provider metadata.</returns>
    Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(CancellationToken cancellationToken);

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
    /// <returns>The recent glucose readings result.</returns>
    Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets historical glucose readings for the requested time range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The historical glucose readings result.</returns>
    Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Builds a dashboard snapshot using provider metadata, latest reading and recent readings.
    /// </summary>
    /// <param name="request">The dashboard request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose dashboard snapshot.</returns>
    Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
        GlucoseDashboardRequest request,
        CancellationToken cancellationToken);
}