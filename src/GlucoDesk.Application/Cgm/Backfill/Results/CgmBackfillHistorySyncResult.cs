using GlucoDesk.Application.Cgm.History.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Represents the result of synchronizing historical CGM backfill readings into local history.
/// </summary>
public sealed record CgmBackfillHistorySyncResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillHistorySyncResult"/> class.
    /// </summary>
    /// <param name="backfillExecution">The executed backfill result.</param>
    /// <param name="historySave">The local history save summary, when readings were persisted.</param>
    /// <param name="message">A user-facing or diagnostic message describing the sync result.</param>
    public CgmBackfillHistorySyncResult(
        CgmBackfillExecutionResult backfillExecution,
        GlucoseHistorySaveResult? historySave,
        string message)
    {
        ArgumentNullException.ThrowIfNull(backfillExecution);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        BackfillExecution = backfillExecution;
        HistorySave = historySave;
        Message = message;
    }

    /// <summary>
    /// Gets the executed backfill result.
    /// </summary>
    public CgmBackfillExecutionResult BackfillExecution { get; }

    /// <summary>
    /// Gets the local history save summary, when readings were persisted.
    /// </summary>
    public GlucoseHistorySaveResult? HistorySave { get; }

    /// <summary>
    /// Gets a user-facing or diagnostic message describing the sync result.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the total number of readings fetched by the backfill execution.
    /// </summary>
    public int TotalFetchedReadings => BackfillExecution.TotalFetchedReadings;

    /// <summary>
    /// Gets the number of readings inserted into local history.
    /// </summary>
    public int AddedReadingsCount => HistorySave?.AddedReadingsCount ?? 0;

    /// <summary>
    /// Gets the number of duplicate readings ignored during local history merge.
    /// </summary>
    public int DuplicateReadingsCount => HistorySave?.DuplicateReadingsCount ?? 0;

    /// <summary>
    /// Gets the total number of stored readings after the local history merge.
    /// </summary>
    public int StoredReadingsCount => HistorySave?.StoredReadingsCount ?? 0;

    /// <summary>
    /// Gets a value indicating whether the sync wrote to local history.
    /// </summary>
    public bool WasPersisted => HistorySave is not null;

    /// <summary>
    /// Gets a value indicating whether the sync added at least one new reading.
    /// </summary>
    public bool HasNewReadings => AddedReadingsCount > 0;
}