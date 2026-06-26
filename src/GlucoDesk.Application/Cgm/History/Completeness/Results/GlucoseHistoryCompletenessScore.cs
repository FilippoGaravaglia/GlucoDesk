using System.Globalization;
using GlucoDesk.Application.Cgm.History.Completeness.Enums;

namespace GlucoDesk.Application.Cgm.History.Completeness.Results;

/// <summary>
/// Represents a user-facing completeness score for local glucose history.
/// </summary>
public sealed record GlucoseHistoryCompletenessScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryCompletenessScore"/> class.
    /// </summary>
    /// <param name="availableReadingsCount">The number of available local readings.</param>
    /// <param name="estimatedExpectedReadingsCount">The estimated number of expected readings.</param>
    /// <param name="dataCoveragePercentage">The local history data coverage percentage.</param>
    /// <param name="detectedGapCount">The number of detected history gaps.</param>
    /// <param name="level">The completeness level.</param>
    /// <param name="statusText">The short user-facing status text.</param>
    /// <param name="detailText">The detailed user-facing explanation.</param>
    public GlucoseHistoryCompletenessScore(
        int availableReadingsCount,
        int estimatedExpectedReadingsCount,
        decimal dataCoveragePercentage,
        int detectedGapCount,
        GlucoseHistoryCompletenessLevel level,
        string statusText,
        string detailText)
    {
        if (availableReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(availableReadingsCount),
                availableReadingsCount,
                "Available readings count must be greater than or equal to zero.");
        }

        if (estimatedExpectedReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(estimatedExpectedReadingsCount),
                estimatedExpectedReadingsCount,
                "Estimated expected readings count must be greater than or equal to zero.");
        }

        if (dataCoveragePercentage is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dataCoveragePercentage),
                dataCoveragePercentage,
                "Data coverage percentage must be between 0 and 100.");
        }

        if (detectedGapCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(detectedGapCount),
                detectedGapCount,
                "Detected gap count must be greater than or equal to zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(statusText);
        ArgumentException.ThrowIfNullOrWhiteSpace(detailText);

        AvailableReadingsCount = availableReadingsCount;
        EstimatedExpectedReadingsCount = estimatedExpectedReadingsCount;
        DataCoveragePercentage = dataCoveragePercentage;
        DetectedGapCount = detectedGapCount;
        Level = level;
        StatusText = statusText;
        DetailText = detailText;
    }

    /// <summary>
    /// Gets the number of available local readings.
    /// </summary>
    public int AvailableReadingsCount { get; }

    /// <summary>
    /// Gets the estimated number of expected readings.
    /// </summary>
    public int EstimatedExpectedReadingsCount { get; }

    /// <summary>
    /// Gets the local history data coverage percentage.
    /// </summary>
    public decimal DataCoveragePercentage { get; }

    /// <summary>
    /// Gets the number of detected history gaps.
    /// </summary>
    public int DetectedGapCount { get; }

    /// <summary>
    /// Gets the completeness level.
    /// </summary>
    public GlucoseHistoryCompletenessLevel Level { get; }

    /// <summary>
    /// Gets the short user-facing status text.
    /// </summary>
    public string StatusText { get; }

    /// <summary>
    /// Gets the detailed user-facing explanation.
    /// </summary>
    public string DetailText { get; }

    /// <summary>
    /// Gets whether the selected period appears complete.
    /// </summary>
    public bool IsComplete => Level is GlucoseHistoryCompletenessLevel.Complete;

    /// <summary>
    /// Gets whether summaries should be interpreted carefully.
    /// </summary>
    public bool RequiresCaution => Level is
        GlucoseHistoryCompletenessLevel.Empty or
        GlucoseHistoryCompletenessLevel.Poor or
        GlucoseHistoryCompletenessLevel.Partial;

    /// <summary>
    /// Gets the display-ready coverage percentage text.
    /// </summary>
    public string CoverageText => $"{DataCoveragePercentage.ToString("0.##", CultureInfo.InvariantCulture)}%";
}
