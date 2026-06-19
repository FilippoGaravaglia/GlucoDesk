using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services;

/// <summary>
/// Analyzes continuity of persisted local glucose history.
/// </summary>
public sealed class GlucoseHistoryContinuityQueryService : IGlucoseHistoryContinuityQueryService
{
    private readonly IGlucoseHistoryService _historyService;
    private readonly IGlucoseHistoryContinuityService _continuityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryContinuityQueryService"/> class.
    /// </summary>
    /// <param name="historyService">The local glucose history service.</param>
    /// <param name="continuityService">The glucose history continuity service.</param>
    public GlucoseHistoryContinuityQueryService(
        IGlucoseHistoryService historyService,
        IGlucoseHistoryContinuityService continuityService)
    {
        ArgumentNullException.ThrowIfNull(historyService);
        ArgumentNullException.ThrowIfNull(continuityService);

        _historyService = historyService;
        _continuityService = continuityService;
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseHistoryContinuityReport>> AnalyzeLocalHistoryAsync(
        GlucoseHistoryContinuityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var historyResult = await _historyService
            .GetReadingsAsync(
                CreateHistoryRequest(request),
                cancellationToken)
            .ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return Result<GlucoseHistoryContinuityReport>.Failure(historyResult.Error);
        }

        return _continuityService.AnalyzeWindow(
            historyResult.Value.Readings,
            request.WindowStartsAt,
            request.WindowEndsAt);
    }

    #region Helpers

    /// <summary>
    /// Creates a local history request from a continuity request.
    /// </summary>
    /// <param name="request">The continuity request.</param>
    /// <returns>The glucose history request.</returns>
    private static GlucoseHistoryRequest CreateHistoryRequest(
        GlucoseHistoryContinuityRequest request)
    {
        return new GlucoseHistoryRequest(
            request.WindowStartsAt,
            request.WindowEndsAt);
    }

    #endregion
}