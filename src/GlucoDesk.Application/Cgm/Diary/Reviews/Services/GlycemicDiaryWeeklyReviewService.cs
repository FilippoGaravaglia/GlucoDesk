using GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Enums;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.Results;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Services;

/// <summary>
/// Builds comparison-based weekly glycemic diary reviews.
/// </summary>
public sealed class GlycemicDiaryWeeklyReviewService : IGlycemicDiaryWeeklyReviewService
{
    private const decimal MeaningfulGlucoseDeltaMgDl = 5m;
    private const decimal MeaningfulPercentageDelta = 5m;

    private readonly IGlucoseHistoryCompletenessScoringService _completenessScoringService;
    private readonly IGlycemicDiaryPatternAnalysisService _patternAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryWeeklyReviewService"/> class.
    /// </summary>
    /// <param name="completenessScoringService">The history completeness scoring service.</param>
    /// <param name="patternAnalysisService">The glycemic diary pattern analysis service.</param>
    public GlycemicDiaryWeeklyReviewService(
        IGlucoseHistoryCompletenessScoringService completenessScoringService,
        IGlycemicDiaryPatternAnalysisService patternAnalysisService)
    {
        ArgumentNullException.ThrowIfNull(completenessScoringService);
        ArgumentNullException.ThrowIfNull(patternAnalysisService);

        _completenessScoringService = completenessScoringService;
        _patternAnalysisService = patternAnalysisService;
    }

    /// <inheritdoc />
    public GlycemicDiaryWeeklyReview CreateReview(
        GlycemicDiaryReport currentReport,
        GlycemicDiaryReport previousReport)
    {
        ArgumentNullException.ThrowIfNull(currentReport);
        ArgumentNullException.ThrowIfNull(previousReport);

        var currentCompleteness = _completenessScoringService.Calculate(currentReport.OverallContinuity);
        var previousCompleteness = _completenessScoringService.Calculate(previousReport.OverallContinuity);
        var currentPatterns = _patternAnalysisService.Analyze(currentReport);
        var previousPatterns = _patternAnalysisService.Analyze(previousReport);

        var changes = BuildChanges(
            currentReport,
            previousReport,
            currentCompleteness,
            previousCompleteness,
            currentPatterns.Patterns.Count,
            previousPatterns.Patterns.Count);

        return new GlycemicDiaryWeeklyReview(
            previousReport.PeriodStartsAt,
            previousReport.PeriodEndsAt,
            currentReport.PeriodStartsAt,
            currentReport.PeriodEndsAt,
            BuildHeadline(currentReport, currentCompleteness, changes),
            BuildSummary(currentReport, currentCompleteness, changes),
            BuildHistoryReliabilityText(currentCompleteness),
            changes,
            BuildHighlights(changes));
    }

    #region Helpers

    /// <summary>
    /// Builds all metric changes for the review.
    /// </summary>
    /// <param name="currentReport">The current report.</param>
    /// <param name="previousReport">The previous report.</param>
    /// <param name="currentCompleteness">The current completeness score.</param>
    /// <param name="previousCompleteness">The previous completeness score.</param>
    /// <param name="currentPatternCount">The current detected pattern count.</param>
    /// <param name="previousPatternCount">The previous detected pattern count.</param>
    /// <returns>The metric changes.</returns>
    private static IReadOnlyCollection<GlycemicDiaryMetricChange> BuildChanges(
        GlycemicDiaryReport currentReport,
        GlycemicDiaryReport previousReport,
        GlucoseHistoryCompletenessScore currentCompleteness,
        GlucoseHistoryCompletenessScore previousCompleteness,
        int currentPatternCount,
        int previousPatternCount)
    {
        var changes = new List<GlycemicDiaryMetricChange>();

        AddDecimalChange(
            changes,
            GlycemicDiaryReviewMetricKind.AverageGlucose,
            "Average glucose",
            previousReport.AverageMgDl,
            currentReport.AverageMgDl,
            " mg/dL",
            MeaningfulGlucoseDeltaMgDl,
            higherIsBetter: false);

        AddDecimalChange(
            changes,
            GlycemicDiaryReviewMetricKind.TimeInRange,
            "Time in range",
            previousReport.TimeInRangePercentage,
            currentReport.TimeInRangePercentage,
            "%",
            MeaningfulPercentageDelta,
            higherIsBetter: true);

        AddDecimalChange(
            changes,
            GlycemicDiaryReviewMetricKind.DataCoverage,
            "Data coverage",
            previousCompleteness.DataCoveragePercentage,
            currentCompleteness.DataCoveragePercentage,
            "%",
            MeaningfulPercentageDelta,
            higherIsBetter: true);

        AddIntegerChange(
            changes,
            GlycemicDiaryReviewMetricKind.ReadingCount,
            "Readings",
            previousReport.ReadingsCount,
            currentReport.ReadingsCount,
            higherIsBetter: true);

        AddIntegerChange(
            changes,
            GlycemicDiaryReviewMetricKind.PatternCount,
            "Detected patterns",
            previousPatternCount,
            currentPatternCount,
            higherIsBetter: false);

        AddIntegerChange(
            changes,
            GlycemicDiaryReviewMetricKind.IncompleteDays,
            "Incomplete days",
            previousReport.IncompleteDaysCount,
            currentReport.IncompleteDaysCount,
            higherIsBetter: false);

        AddIntegerChange(
            changes,
            GlycemicDiaryReviewMetricKind.EmptyDays,
            "Empty days",
            previousReport.EmptyDaysCount,
            currentReport.EmptyDaysCount,
            higherIsBetter: false);

        return changes;
    }

    /// <summary>
    /// Adds a decimal metric change.
    /// </summary>
    /// <param name="changes">The change collection.</param>
    /// <param name="kind">The metric kind.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="suffix">The value suffix.</param>
    /// <param name="meaningfulDelta">The meaningful delta threshold.</param>
    /// <param name="higherIsBetter">Whether higher values are generally better.</param>
    private static void AddDecimalChange(
        ICollection<GlycemicDiaryMetricChange> changes,
        GlycemicDiaryReviewMetricKind kind,
        string displayName,
        decimal? previousValue,
        decimal? currentValue,
        string suffix,
        decimal meaningfulDelta,
        bool higherIsBetter)
    {
        var direction = CalculateDirection(previousValue, currentValue, meaningfulDelta);
        var deltaText = FormatDecimalDelta(previousValue, currentValue, suffix);

        changes.Add(new GlycemicDiaryMetricChange(
            kind,
            displayName,
            FormatDecimalValue(previousValue, suffix),
            FormatDecimalValue(currentValue, suffix),
            deltaText,
            direction,
            CalculateSeverity(direction, higherIsBetter),
            BuildDescription(displayName, previousValue, currentValue, direction, deltaText, suffix)));
    }

    /// <summary>
    /// Adds an integer metric change.
    /// </summary>
    /// <param name="changes">The change collection.</param>
    /// <param name="kind">The metric kind.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="higherIsBetter">Whether higher values are generally better.</param>
    private static void AddIntegerChange(
        ICollection<GlycemicDiaryMetricChange> changes,
        GlycemicDiaryReviewMetricKind kind,
        string displayName,
        int previousValue,
        int currentValue,
        bool higherIsBetter)
    {
        var previousDecimal = (decimal)previousValue;
        var currentDecimal = (decimal)currentValue;
        var direction = CalculateDirection(previousDecimal, currentDecimal, 1m);
        var deltaText = FormatIntegerDelta(previousValue, currentValue);

        changes.Add(new GlycemicDiaryMetricChange(
            kind,
            displayName,
            previousValue.ToString(),
            currentValue.ToString(),
            deltaText,
            direction,
            CalculateSeverity(direction, higherIsBetter),
            BuildDescription(displayName, previousDecimal, currentDecimal, direction, deltaText, string.Empty)));
    }

    /// <summary>
    /// Calculates the change direction.
    /// </summary>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="meaningfulDelta">The meaningful delta threshold.</param>
    /// <returns>The change direction.</returns>
    private static GlycemicDiaryReviewChangeDirection CalculateDirection(
        decimal? previousValue,
        decimal? currentValue,
        decimal meaningfulDelta)
    {
        if (!previousValue.HasValue && !currentValue.HasValue)
        {
            return GlycemicDiaryReviewChangeDirection.Unchanged;
        }

        if (!previousValue.HasValue && currentValue.HasValue)
        {
            return GlycemicDiaryReviewChangeDirection.NewlyAvailable;
        }

        if (previousValue.HasValue && !currentValue.HasValue)
        {
            return GlycemicDiaryReviewChangeDirection.NoLongerAvailable;
        }

        var delta = currentValue!.Value - previousValue!.Value;

        if (Math.Abs(delta) < meaningfulDelta)
        {
            return GlycemicDiaryReviewChangeDirection.Unchanged;
        }

        return delta > 0
            ? GlycemicDiaryReviewChangeDirection.Increased
            : GlycemicDiaryReviewChangeDirection.Decreased;
    }

    /// <summary>
    /// Calculates the signal severity for a change direction.
    /// </summary>
    /// <param name="direction">The change direction.</param>
    /// <param name="higherIsBetter">Whether higher values are generally better.</param>
    /// <returns>The severity.</returns>
    private static GlycemicDiaryReviewSignalSeverity CalculateSeverity(
        GlycemicDiaryReviewChangeDirection direction,
        bool higherIsBetter)
    {
        return direction switch
        {
            GlycemicDiaryReviewChangeDirection.NoLongerAvailable => GlycemicDiaryReviewSignalSeverity.Caution,
            GlycemicDiaryReviewChangeDirection.Increased => higherIsBetter
                ? GlycemicDiaryReviewSignalSeverity.Info
                : GlycemicDiaryReviewSignalSeverity.Caution,
            GlycemicDiaryReviewChangeDirection.Decreased => higherIsBetter
                ? GlycemicDiaryReviewSignalSeverity.Caution
                : GlycemicDiaryReviewSignalSeverity.Info,
            _ => GlycemicDiaryReviewSignalSeverity.Info
        };
    }

    /// <summary>
    /// Builds the review headline.
    /// </summary>
    /// <param name="currentReport">The current report.</param>
    /// <param name="currentCompleteness">The current completeness score.</param>
    /// <param name="changes">The metric changes.</param>
    /// <returns>The headline.</returns>
    private static string BuildHeadline(
        GlycemicDiaryReport currentReport,
        GlucoseHistoryCompletenessScore currentCompleteness,
        IReadOnlyCollection<GlycemicDiaryMetricChange> changes)
    {
        if (currentReport.ReadingsCount == 0)
        {
            return "Weekly review: no local readings available";
        }

        if (currentCompleteness.RequiresCaution)
        {
            return "Weekly review: data quality needs attention";
        }

        var tirChange = FindChange(changes, GlycemicDiaryReviewMetricKind.TimeInRange);

        if (tirChange.Direction == GlycemicDiaryReviewChangeDirection.Increased)
        {
            return "Weekly review: time in range improved";
        }

        if (tirChange.Direction == GlycemicDiaryReviewChangeDirection.Decreased)
        {
            return "Weekly review: time in range decreased";
        }

        var patternChange = FindChange(changes, GlycemicDiaryReviewMetricKind.PatternCount);

        if (patternChange.Direction == GlycemicDiaryReviewChangeDirection.Increased)
        {
            return "Weekly review: new local patterns detected";
        }

        return "Weekly review: mostly stable period";
    }

    /// <summary>
    /// Builds the review summary.
    /// </summary>
    /// <param name="currentReport">The current report.</param>
    /// <param name="currentCompleteness">The current completeness score.</param>
    /// <param name="changes">The metric changes.</param>
    /// <returns>The summary.</returns>
    private static string BuildSummary(
        GlycemicDiaryReport currentReport,
        GlucoseHistoryCompletenessScore currentCompleteness,
        IReadOnlyCollection<GlycemicDiaryMetricChange> changes)
    {
        if (currentReport.ReadingsCount == 0)
        {
            return "The current period has no local readings, so no meaningful comparison can be generated.";
        }

        var meaningfulChanges = changes
            .Where(change => change.HasMeaningfulChange)
            .Take(2)
            .Select(change => change.Description)
            .ToArray();

        var summary = meaningfulChanges.Length == 0
            ? "The current period looks broadly similar to the previous one."
            : string.Join(" ", meaningfulChanges);

        if (currentCompleteness.RequiresCaution)
        {
            summary += $" Current local history reliability is {currentCompleteness.StatusText} · {currentCompleteness.CoverageText}, so comparisons should be interpreted carefully.";
        }

        return summary;
    }

    /// <summary>
    /// Builds the current history reliability text.
    /// </summary>
    /// <param name="currentCompleteness">The current completeness score.</param>
    /// <returns>The reliability text.</returns>
    private static string BuildHistoryReliabilityText(
        GlucoseHistoryCompletenessScore currentCompleteness)
    {
        return $"Current history reliability: {currentCompleteness.StatusText} · {currentCompleteness.CoverageText}. {currentCompleteness.DetailText}";
    }

    /// <summary>
    /// Builds the review highlights.
    /// </summary>
    /// <param name="changes">The metric changes.</param>
    /// <returns>The highlights.</returns>
    private static IReadOnlyCollection<string> BuildHighlights(
        IReadOnlyCollection<GlycemicDiaryMetricChange> changes)
    {
        var average = FindChange(changes, GlycemicDiaryReviewMetricKind.AverageGlucose);
        var tir = FindChange(changes, GlycemicDiaryReviewMetricKind.TimeInRange);
        var coverage = FindChange(changes, GlycemicDiaryReviewMetricKind.DataCoverage);
        var patterns = FindChange(changes, GlycemicDiaryReviewMetricKind.PatternCount);

        return
        [
            $"Average glucose: {average.PreviousValueText} → {average.CurrentValueText}.",
            $"Time in range: {tir.PreviousValueText} → {tir.CurrentValueText}.",
            $"Data coverage: {coverage.PreviousValueText} → {coverage.CurrentValueText}.",
            $"Detected patterns: {patterns.PreviousValueText} → {patterns.CurrentValueText}."
        ];
    }

    /// <summary>
    /// Finds a metric change by kind.
    /// </summary>
    /// <param name="changes">The metric changes.</param>
    /// <param name="kind">The metric kind.</param>
    /// <returns>The matching metric change.</returns>
    private static GlycemicDiaryMetricChange FindChange(
        IReadOnlyCollection<GlycemicDiaryMetricChange> changes,
        GlycemicDiaryReviewMetricKind kind)
    {
        return changes.Single(change => change.Kind == kind);
    }

    /// <summary>
    /// Builds a metric description.
    /// </summary>
    /// <param name="displayName">The metric display name.</param>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="direction">The change direction.</param>
    /// <param name="deltaText">The formatted delta.</param>
    /// <param name="suffix">The value suffix.</param>
    /// <returns>The metric description.</returns>
    private static string BuildDescription(
        string displayName,
        decimal? previousValue,
        decimal? currentValue,
        GlycemicDiaryReviewChangeDirection direction,
        string deltaText,
        string suffix)
    {
        if (direction == GlycemicDiaryReviewChangeDirection.NewlyAvailable)
        {
            return $"{displayName} is now available at {FormatDecimalValue(currentValue, suffix)}.";
        }

        if (direction == GlycemicDiaryReviewChangeDirection.NoLongerAvailable)
        {
            return $"{displayName} is no longer available in the current period.";
        }

        if (direction == GlycemicDiaryReviewChangeDirection.Unchanged)
        {
            return $"{displayName} remained broadly stable ({FormatDecimalValue(previousValue, suffix)} → {FormatDecimalValue(currentValue, suffix)}).";
        }

        return $"{displayName} {FormatDirection(direction)} from {FormatDecimalValue(previousValue, suffix)} to {FormatDecimalValue(currentValue, suffix)} ({deltaText}).";
    }

    /// <summary>
    /// Formats the change direction.
    /// </summary>
    /// <param name="direction">The change direction.</param>
    /// <returns>The formatted direction.</returns>
    private static string FormatDirection(GlycemicDiaryReviewChangeDirection direction)
    {
        return direction switch
        {
            GlycemicDiaryReviewChangeDirection.Increased => "increased",
            GlycemicDiaryReviewChangeDirection.Decreased => "decreased",
            _ => "changed"
        };
    }

    /// <summary>
    /// Formats a decimal value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>The formatted value.</returns>
    private static string FormatDecimalValue(decimal? value, string suffix)
    {
        if (!value.HasValue)
        {
            return "—";
        }

        if (suffix == " mg/dL")
        {
            return $"{Math.Round(value.Value, 0, MidpointRounding.AwayFromZero):0}{suffix}";
        }

        return $"{Math.Round(value.Value, 2, MidpointRounding.AwayFromZero):0.##}{suffix}";
    }

    /// <summary>
    /// Formats a decimal delta.
    /// </summary>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>The formatted delta.</returns>
    private static string FormatDecimalDelta(
        decimal? previousValue,
        decimal? currentValue,
        string suffix)
    {
        if (!previousValue.HasValue || !currentValue.HasValue)
        {
            return "—";
        }

        var delta = currentValue.Value - previousValue.Value;
        var sign = delta > 0 ? "+" : string.Empty;

        if (suffix == " mg/dL")
        {
            return $"{sign}{Math.Round(delta, 0, MidpointRounding.AwayFromZero):0}{suffix}";
        }

        return $"{sign}{Math.Round(delta, 2, MidpointRounding.AwayFromZero):0.##}{suffix}";
    }

    /// <summary>
    /// Formats an integer delta.
    /// </summary>
    /// <param name="previousValue">The previous value.</param>
    /// <param name="currentValue">The current value.</param>
    /// <returns>The formatted delta.</returns>
    private static string FormatIntegerDelta(int previousValue, int currentValue)
    {
        var delta = currentValue - previousValue;
        var sign = delta > 0 ? "+" : string.Empty;

        return $"{sign}{delta}";
    }

    #endregion
}
