using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Services;

/// <summary>
/// Provides application-level operations for local glucose history.
/// </summary>
public sealed class GlucoseHistoryService : IGlucoseHistoryService
{
    private readonly IGlucoseHistoryStore _historyStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryService"/> class.
    /// </summary>
    /// <param name="historyStore">The glucose history store.</param>
    public GlucoseHistoryService(IGlucoseHistoryStore historyStore)
    {
        ArgumentNullException.ThrowIfNull(historyStore);

        _historyStore = historyStore;
    }

    /// <inheritdoc />
    public Task<Result> SaveReadingsAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(readings);

        return _historyStore.SaveReadingsAsync(readings, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(readings);

        return _historyStore.SaveReadingsWithSummaryAsync(readings, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
        GlucoseHistoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _historyStore.GetReadingsAsync(request, cancellationToken);
    }
}