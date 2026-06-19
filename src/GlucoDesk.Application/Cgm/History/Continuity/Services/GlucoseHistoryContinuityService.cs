using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Options;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services;

/// <summary>
/// Analyzes continuity of local glucose history windows.
/// </summary>
public sealed class GlucoseHistoryContinuityService : IGlucoseHistoryContinuityService
{
    private readonly HistoryContinuityOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryContinuityService"/> class.
    /// </summary>
    /// <param name="options">The history continuity options.</param>
    public GlucoseHistoryContinuityService(HistoryContinuityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public Result<GlucoseHistoryContinuityReport> AnalyzeWindow(
        IReadOnlyCollection<GlucoseReading> readings,
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt)
    {
        ArgumentNullException.ThrowIfNull(readings);

        var normalizedWindowStart = windowStartsAt.ToUniversalTime();
        var normalizedWindowEnd = windowEndsAt.ToUniversalTime();

        if (normalizedWindowEnd <= normalizedWindowStart)
        {
            return Result<GlucoseHistoryContinuityReport>.Failure(
                new Error(
                    "HistoryContinuity.InvalidWindow",
                    "History continuity window end must be greater than window start."));
        }

        var normalizedReadings = NormalizeReadings(
            readings,
            normalizedWindowStart,
            normalizedWindowEnd);

        var gaps = DetectGaps(
            normalizedReadings,
            normalizedWindowStart,
            normalizedWindowEnd);

        var report = new GlucoseHistoryContinuityReport(
            normalizedWindowStart,
            normalizedWindowEnd,
            normalizedReadings.Count,
            CalculateCoveragePercentage(
                normalizedWindowStart,
                normalizedWindowEnd,
                gaps),
            gaps);

        return Result<GlucoseHistoryContinuityReport>.Success(report);
    }

    #region Helpers

    /// <summary>
    /// Normalizes readings for continuity analysis.
    /// </summary>
    /// <param name="readings">The readings to normalize.</param>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    /// <returns>The normalized readings.</returns>
    private static IReadOnlyList<GlucoseReading> NormalizeReadings(
        IReadOnlyCollection<GlucoseReading> readings,
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt)
    {
        return readings
            .Where(reading =>
                reading.Timestamp.ToUniversalTime() >= windowStartsAt &&
                reading.Timestamp.ToUniversalTime() <= windowEndsAt)
            .GroupBy(reading => reading.Timestamp.ToUniversalTime().Ticks)
            .Select(group => group.First())
            .OrderBy(reading => reading.Timestamp.ToUniversalTime())
            .ToArray();
    }

    /// <summary>
    /// Detects continuity gaps in a normalized glucose reading window.
    /// </summary>
    /// <param name="readings">The normalized readings.</param>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    /// <returns>The detected gaps.</returns>
    private IReadOnlyCollection<GlucoseHistoryGap> DetectGaps(
        IReadOnlyList<GlucoseReading> readings,
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt)
    {
        if (readings.Count == 0)
        {
            return
            [
                CreateGap(
                    GlucoseHistoryGapKind.EmptyWindow,
                    windowStartsAt,
                    windowEndsAt)
            ];
        }

        var gaps = new List<GlucoseHistoryGap>();

        var firstReadingTimestamp = readings[0].Timestamp.ToUniversalTime();

        AddGapIfNeeded(
            gaps,
            GlucoseHistoryGapKind.Leading,
            windowStartsAt,
            firstReadingTimestamp);

        for (var index = 1; index < readings.Count; index++)
        {
            var previousTimestamp = readings[index - 1].Timestamp.ToUniversalTime();
            var currentTimestamp = readings[index].Timestamp.ToUniversalTime();

            AddGapIfNeeded(
                gaps,
                GlucoseHistoryGapKind.BetweenReadings,
                previousTimestamp,
                currentTimestamp);
        }

        var lastReadingTimestamp = readings[^1].Timestamp.ToUniversalTime();

        AddGapIfNeeded(
            gaps,
            GlucoseHistoryGapKind.Trailing,
            lastReadingTimestamp,
            windowEndsAt);

        return gaps;
    }

    /// <summary>
    /// Adds a gap when the duration exceeds the configured maximum allowed gap.
    /// </summary>
    /// <param name="gaps">The gap collection.</param>
    /// <param name="kind">The gap kind.</param>
    /// <param name="startsAt">The gap start timestamp.</param>
    /// <param name="endsAt">The gap end timestamp.</param>
    private void AddGapIfNeeded(
        ICollection<GlucoseHistoryGap> gaps,
        GlucoseHistoryGapKind kind,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (endsAt - startsAt <= _options.MaximumAllowedGap)
        {
            return;
        }

        gaps.Add(CreateGap(kind, startsAt, endsAt));
    }

    /// <summary>
    /// Creates a glucose history gap.
    /// </summary>
    /// <param name="kind">The gap kind.</param>
    /// <param name="startsAt">The gap start timestamp.</param>
    /// <param name="endsAt">The gap end timestamp.</param>
    /// <returns>The glucose history gap.</returns>
    private GlucoseHistoryGap CreateGap(
        GlucoseHistoryGapKind kind,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return new GlucoseHistoryGap(
            kind,
            startsAt,
            endsAt,
            EstimateMissingReadings(kind, endsAt - startsAt));
    }

    /// <summary>
    /// Estimates the number of missing readings for a gap.
    /// </summary>
    /// <param name="kind">The gap kind.</param>
    /// <param name="gapDuration">The gap duration.</param>
    /// <returns>The estimated number of missing readings.</returns>
    private int EstimateMissingReadings(
        GlucoseHistoryGapKind kind,
        TimeSpan gapDuration)
    {
        if (gapDuration <= TimeSpan.Zero)
        {
            return 0;
        }

        var expectedSlots = (int)Math.Floor(
            gapDuration.TotalSeconds / _options.ExpectedReadingInterval.TotalSeconds);

        return kind == GlucoseHistoryGapKind.BetweenReadings
            ? Math.Max(0, expectedSlots - 1)
            : Math.Max(1, expectedSlots);
    }

    /// <summary>
    /// Calculates the estimated data coverage percentage.
    /// </summary>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    /// <param name="gaps">The detected gaps.</param>
    /// <returns>The estimated data coverage percentage.</returns>
    private static decimal CalculateCoveragePercentage(
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt,
        IReadOnlyCollection<GlucoseHistoryGap> gaps)
    {
        var windowTicks = windowEndsAt.Ticks - windowStartsAt.Ticks;

        if (windowTicks <= 0)
        {
            return 0m;
        }

        var gapTicks = gaps.Sum(gap => gap.Duration.Ticks);
        var coveredTicks = Math.Max(0, windowTicks - gapTicks);

        return Math.Round(
            coveredTicks * 100m / windowTicks,
            2,
            MidpointRounding.AwayFromZero);
    }

    #endregion
}