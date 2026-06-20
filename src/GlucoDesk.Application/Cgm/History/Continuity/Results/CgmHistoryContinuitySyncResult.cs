using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;

namespace GlucoDesk.Application.Cgm.History.Continuity.Results;

/// <summary>
/// Represents the result of a local CGM history continuity synchronization.
/// </summary>
public sealed record CgmHistoryContinuitySyncResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmHistoryContinuitySyncResult"/> class.
    /// </summary>
    /// <param name="request">The original continuity synchronization request.</param>
    /// <param name="startsAt">The resolved synchronization window start.</param>
    /// <param name="endsAt">The resolved synchronization window end.</param>
    /// <param name="backfillSync">The backfill-to-history synchronization result.</param>
    /// <param name="message">A user-facing or diagnostic synchronization message.</param>
    public CgmHistoryContinuitySyncResult(
        CgmHistoryContinuitySyncRequest request,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CgmBackfillHistorySyncResult backfillSync,
        string message)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(backfillSync);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (endsAt <= startsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endsAt),
                endsAt,
                "Continuity synchronization end timestamp must be greater than start timestamp.");
        }

        Request = request;
        StartsAt = startsAt;
        EndsAt = endsAt;
        BackfillSync = backfillSync;
        Message = message;
    }

    /// <summary>
    /// Gets the original continuity synchronization request.
    /// </summary>
    public CgmHistoryContinuitySyncRequest Request { get; }

    /// <summary>
    /// Gets the resolved synchronization window start.
    /// </summary>
    public DateTimeOffset StartsAt { get; }

    /// <summary>
    /// Gets the resolved synchronization window end.
    /// </summary>
    public DateTimeOffset EndsAt { get; }

    /// <summary>
    /// Gets the backfill-to-history synchronization result.
    /// </summary>
    public CgmBackfillHistorySyncResult BackfillSync { get; }

    /// <summary>
    /// Gets a user-facing or diagnostic synchronization message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the total number of readings fetched by backfill.
    /// </summary>
    public int TotalFetchedReadings => BackfillSync.TotalFetchedReadings;

    /// <summary>
    /// Gets the number of readings added to local history.
    /// </summary>
    public int AddedReadingsCount => BackfillSync.AddedReadingsCount;

    /// <summary>
    /// Gets the number of duplicate readings ignored during local history merge.
    /// </summary>
    public int DuplicateReadingsCount => BackfillSync.DuplicateReadingsCount;

    /// <summary>
    /// Gets the number of stored readings after synchronization.
    /// </summary>
    public int StoredReadingsCount => BackfillSync.StoredReadingsCount;

    /// <summary>
    /// Gets a value indicating whether the synchronization added at least one new reading.
    /// </summary>
    public bool HasNewReadings => BackfillSync.HasNewReadings;
}