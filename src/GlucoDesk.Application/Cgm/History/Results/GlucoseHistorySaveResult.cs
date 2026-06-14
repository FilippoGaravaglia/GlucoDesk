using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.History.Results;

/// <summary>
/// Represents the result of saving glucose readings into local history.
/// </summary>
public sealed record GlucoseHistorySaveResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistorySaveResult"/> class.
    /// </summary>
    /// <param name="providerKind">The provider kind for the saved batch.</param>
    /// <param name="incomingReadingsCount">The number of incoming readings.</param>
    /// <param name="addedReadingsCount">The number of newly added readings.</param>
    /// <param name="duplicateReadingsCount">The number of duplicate readings ignored during merge.</param>
    /// <param name="storedReadingsCount">The total number of stored readings after merge.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the counts is invalid.</exception>
    public GlucoseHistorySaveResult(
        CgmProviderKind providerKind,
        int incomingReadingsCount,
        int addedReadingsCount,
        int duplicateReadingsCount,
        int storedReadingsCount)
    {
        if (incomingReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(incomingReadingsCount),
                incomingReadingsCount,
                "Incoming readings count cannot be negative.");
        }

        if (addedReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(addedReadingsCount),
                addedReadingsCount,
                "Added readings count cannot be negative.");
        }

        if (duplicateReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duplicateReadingsCount),
                duplicateReadingsCount,
                "Duplicate readings count cannot be negative.");
        }

        if (storedReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(storedReadingsCount),
                storedReadingsCount,
                "Stored readings count cannot be negative.");
        }

        ProviderKind = providerKind;
        IncomingReadingsCount = incomingReadingsCount;
        AddedReadingsCount = addedReadingsCount;
        DuplicateReadingsCount = duplicateReadingsCount;
        StoredReadingsCount = storedReadingsCount;
    }

    /// <summary>
    /// Gets the provider kind for the saved batch.
    /// </summary>
    public CgmProviderKind ProviderKind { get; }

    /// <summary>
    /// Gets the number of incoming readings.
    /// </summary>
    public int IncomingReadingsCount { get; }

    /// <summary>
    /// Gets the number of newly added readings.
    /// </summary>
    public int AddedReadingsCount { get; }

    /// <summary>
    /// Gets the number of duplicate readings ignored during merge.
    /// </summary>
    public int DuplicateReadingsCount { get; }

    /// <summary>
    /// Gets the total number of stored readings after merge.
    /// </summary>
    public int StoredReadingsCount { get; }

    /// <summary>
    /// Gets a value indicating whether the save operation added new readings.
    /// </summary>
    public bool HasNewReadings => AddedReadingsCount > 0;
}