using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Represents the result of fetching historical readings for a planned backfill gap.
/// </summary>
public sealed record CgmBackfillFetchedGapResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillFetchedGapResult"/> class.
    /// </summary>
    /// <param name="gap">The planned backfill gap.</param>
    /// <param name="readings">The readings fetched for the gap.</param>
    public CgmBackfillFetchedGapResult(
        CgmBackfillPlanGap gap,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        ArgumentNullException.ThrowIfNull(gap);
        ArgumentNullException.ThrowIfNull(readings);

        Gap = gap;
        Readings = readings;
        ReadingsCount = readings.Count;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillFetchedGapResult"/> class.
    /// </summary>
    /// <param name="gap">The planned backfill gap.</param>
    /// <param name="readingsCount">The number of readings fetched for the gap.</param>
    public CgmBackfillFetchedGapResult(
        CgmBackfillPlanGap gap,
        int readingsCount)
    {
        ArgumentNullException.ThrowIfNull(gap);

        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Fetched readings count cannot be negative.");
        }

        Gap = gap;
        Readings = [];
        ReadingsCount = readingsCount;
    }

    /// <summary>
    /// Gets the planned backfill gap.
    /// </summary>
    public CgmBackfillPlanGap Gap { get; }

    /// <summary>
    /// Gets the readings fetched for the gap.
    /// </summary>
    public IReadOnlyCollection<GlucoseReading> Readings { get; }

    /// <summary>
    /// Gets the number of readings fetched for the gap.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets a value indicating whether at least one reading was fetched.
    /// </summary>
    public bool HasReadings => ReadingsCount > 0;
}