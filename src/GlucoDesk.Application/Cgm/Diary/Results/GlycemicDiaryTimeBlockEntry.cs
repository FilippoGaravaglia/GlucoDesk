using GlucoDesk.Application.Cgm.Diary.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Results;

/// <summary>
/// Represents a glycemic diary entry for a standard time block.
/// </summary>
public sealed record GlycemicDiaryTimeBlockEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryTimeBlockEntry"/> class.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The user-facing label.</param>
    /// <param name="startsAt">The local block start time.</param>
    /// <param name="endsAt">The local block end time.</param>
    /// <param name="readingsCount">The number of readings in the block.</param>
    /// <param name="representativeValueMgDl">The representative glucose value in mg/dL.</param>
    /// <param name="representativeTimestamp">The representative reading timestamp.</param>
    /// <param name="averageMgDl">The average glucose value in mg/dL.</param>
    /// <param name="minimumMgDl">The minimum glucose value in mg/dL.</param>
    /// <param name="maximumMgDl">The maximum glucose value in mg/dL.</param>
    public GlycemicDiaryTimeBlockEntry(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        TimeOnly startsAt,
        TimeOnly endsAt,
        int readingsCount,
        decimal? representativeValueMgDl,
        DateTimeOffset? representativeTimestamp,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl)
    {
        if (kind == GlycemicDiaryTimeBlockKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "Time block kind must be specified.");
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException(
                "Time block label cannot be empty.",
                nameof(label));
        }

        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Readings count cannot be negative.");
        }

        Kind = kind;
        Label = label;
        StartsAt = startsAt;
        EndsAt = endsAt;
        ReadingsCount = readingsCount;
        RepresentativeValueMgDl = representativeValueMgDl;
        RepresentativeTimestamp = representativeTimestamp;
        AverageMgDl = averageMgDl;
        MinimumMgDl = minimumMgDl;
        MaximumMgDl = maximumMgDl;
    }

    /// <summary>
    /// Gets the time block kind.
    /// </summary>
    public GlycemicDiaryTimeBlockKind Kind { get; }

    /// <summary>
    /// Gets the user-facing label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the local block start time.
    /// </summary>
    public TimeOnly StartsAt { get; }

    /// <summary>
    /// Gets the local block end time.
    /// </summary>
    public TimeOnly EndsAt { get; }

    /// <summary>
    /// Gets the number of readings in the block.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the representative glucose value in mg/dL.
    /// </summary>
    public decimal? RepresentativeValueMgDl { get; }

    /// <summary>
    /// Gets the representative reading timestamp.
    /// </summary>
    public DateTimeOffset? RepresentativeTimestamp { get; }

    /// <summary>
    /// Gets the average glucose value in mg/dL.
    /// </summary>
    public decimal? AverageMgDl { get; }

    /// <summary>
    /// Gets the minimum glucose value in mg/dL.
    /// </summary>
    public decimal? MinimumMgDl { get; }

    /// <summary>
    /// Gets the maximum glucose value in mg/dL.
    /// </summary>
    public decimal? MaximumMgDl { get; }

    /// <summary>
    /// Gets a value indicating whether the time block contains at least one reading.
    /// </summary>
    public bool HasData => ReadingsCount > 0;
}