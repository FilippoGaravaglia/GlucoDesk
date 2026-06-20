using GlucoDesk.Application.Cgm.Backfill.Enums;

namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Represents the result of executing a historical CGM backfill run.
/// </summary>
public sealed record CgmBackfillExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillExecutionResult"/> class.
    /// </summary>
    /// <param name="status">The execution status.</param>
    /// <param name="run">The planned backfill run.</param>
    /// <param name="fetchedGaps">The fetched gap results.</param>
    /// <param name="message">A user-facing or diagnostic message describing the execution.</param>
    public CgmBackfillExecutionResult(
        CgmBackfillExecutionStatus status,
        CgmBackfillRunResult run,
        IReadOnlyCollection<CgmBackfillFetchedGapResult> fetchedGaps,
        string message)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(fetchedGaps);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Status = status;
        Run = run;
        FetchedGaps = fetchedGaps;
        Message = message;
    }

    /// <summary>
    /// Gets the execution status.
    /// </summary>
    public CgmBackfillExecutionStatus Status { get; }

    /// <summary>
    /// Gets the planned backfill run.
    /// </summary>
    public CgmBackfillRunResult Run { get; }

    /// <summary>
    /// Gets the fetched gap results.
    /// </summary>
    public IReadOnlyCollection<CgmBackfillFetchedGapResult> FetchedGaps { get; }

    /// <summary>
    /// Gets a user-facing or diagnostic message describing the execution.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the total number of fetched readings.
    /// </summary>
    public int TotalFetchedReadings => FetchedGaps.Sum(gap => gap.ReadingsCount);

    /// <summary>
    /// Gets a value indicating whether at least one reading was fetched.
    /// </summary>
    public bool HasFetchedReadings => TotalFetchedReadings > 0;
}