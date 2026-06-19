using GlucoDesk.Application.Cgm.Diary.Options;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Diary.Services;

/// <summary>
/// Generates glycemic diary reports from local glucose history.
/// </summary>
public sealed class GlycemicDiaryService : IGlycemicDiaryService
{
    private readonly IGlucoseHistoryService _historyService;
    private readonly IGlucoseHistoryContinuityService _continuityService;
    private readonly GlycemicDiaryOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryService"/> class.
    /// </summary>
    /// <param name="historyService">The local glucose history service.</param>
    /// <param name="continuityService">The glucose history continuity service.</param>
    /// <param name="options">The glycemic diary options.</param>
    public GlycemicDiaryService(
        IGlucoseHistoryService historyService,
        IGlucoseHistoryContinuityService continuityService,
        GlycemicDiaryOptions options)
    {
        ArgumentNullException.ThrowIfNull(historyService);
        ArgumentNullException.ThrowIfNull(continuityService);
        ArgumentNullException.ThrowIfNull(options);

        _historyService = historyService;
        _continuityService = continuityService;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
        GlycemicDiaryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var historyResult = await _historyService
            .GetReadingsAsync(
                new GlucoseHistoryRequest(
                    request.PeriodStartsAt,
                    request.PeriodEndsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return Result<GlycemicDiaryReport>.Failure(historyResult.Error);
        }

        var readings = NormalizeReadings(
            historyResult.Value.Readings,
            request.PeriodStartsAt,
            request.PeriodEndsAt);

        var overallContinuityResult = _continuityService.AnalyzeWindow(
            readings,
            request.PeriodStartsAt,
            request.PeriodEndsAt);

        if (overallContinuityResult.IsFailure)
        {
            return Result<GlycemicDiaryReport>.Failure(overallContinuityResult.Error);
        }

        var dailyEntries = CreateDailyEntries(
            readings,
            request.PeriodStartsAt,
            request.PeriodEndsAt);

        var report = new GlycemicDiaryReport(
            request.PeriodStartsAt,
            request.PeriodEndsAt,
            readings.Count,
            CalculateAverage(readings),
            CalculateMinimum(readings),
            CalculateMaximum(readings),
            CalculateTimeInRangePercentage(readings),
            overallContinuityResult.Value,
            dailyEntries);

        return Result<GlycemicDiaryReport>.Success(report);
    }

    #region Helpers

    /// <summary>
    /// Normalizes readings for diary generation.
    /// </summary>
    /// <param name="readings">The readings to normalize.</param>
    /// <param name="periodStartsAt">The diary period start timestamp.</param>
    /// <param name="periodEndsAt">The diary period end timestamp.</param>
    /// <returns>The normalized readings.</returns>
    private static IReadOnlyList<GlucoseReading> NormalizeReadings(
        IReadOnlyCollection<GlucoseReading> readings,
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt)
    {
        return readings
            .Where(reading =>
                reading.Timestamp.ToUniversalTime() >= periodStartsAt.ToUniversalTime() &&
                reading.Timestamp.ToUniversalTime() <= periodEndsAt.ToUniversalTime())
            .GroupBy(reading => reading.Timestamp.ToUniversalTime().Ticks)
            .Select(group => group.First())
            .OrderBy(reading => reading.Timestamp.ToUniversalTime())
            .ToArray();
    }

    /// <summary>
    /// Creates daily diary entries.
    /// </summary>
    /// <param name="readings">The normalized readings.</param>
    /// <param name="periodStartsAt">The diary period start timestamp.</param>
    /// <param name="periodEndsAt">The diary period end timestamp.</param>
    /// <returns>The daily diary entries.</returns>
    private IReadOnlyCollection<GlycemicDiaryDailyEntry> CreateDailyEntries(
        IReadOnlyList<GlucoseReading> readings,
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt)
    {
        var entries = new List<GlycemicDiaryDailyEntry>();

        foreach (var date in EnumerateLocalDates(periodStartsAt, periodEndsAt))
        {
            var dayWindowStartsAt = MaxDateTimeOffset(
                periodStartsAt,
                CreateZonedDateTime(date, TimeOnly.MinValue));

            var dayWindowEndsAt = MinDateTimeOffset(
                periodEndsAt,
                CreateZonedDateTime(date, new TimeOnly(23, 59, 59)));

            if (dayWindowEndsAt <= dayWindowStartsAt)
            {
                continue;
            }

            var dailyReadings = readings
                .Where(reading => GetLocalDate(reading.Timestamp) == date)
                .ToArray();

            var dailyContinuity = CreateDailyContinuityReport(
                dailyReadings,
                dayWindowStartsAt,
                dayWindowEndsAt);

            entries.Add(new GlycemicDiaryDailyEntry(
                date,
                dailyReadings.Length,
                CalculateAverage(dailyReadings),
                CalculateMinimum(dailyReadings),
                CalculateMaximum(dailyReadings),
                CalculateTimeInRangePercentage(dailyReadings),
                dailyContinuity.DataCoveragePercentage,
                dailyContinuity.IsComplete,
                dailyContinuity.Gaps.Count,
                CreateTimeBlockEntries(date, dailyReadings)));
        }

        return entries;
    }

    /// <summary>
    /// Creates a continuity report for a daily window.
    /// </summary>
    /// <param name="readings">The daily readings.</param>
    /// <param name="windowStartsAt">The daily window start timestamp.</param>
    /// <param name="windowEndsAt">The daily window end timestamp.</param>
    /// <returns>The daily continuity report.</returns>
    private GlucoseHistoryContinuityReport CreateDailyContinuityReport(
        IReadOnlyCollection<GlucoseReading> readings,
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt)
    {
        var result = _continuityService.AnalyzeWindow(
            readings,
            windowStartsAt,
            windowEndsAt);

        return result.Value;
    }

    /// <summary>
    /// Creates time block entries for a diary date.
    /// </summary>
    /// <param name="date">The local diary date.</param>
    /// <param name="readings">The daily readings.</param>
    /// <returns>The time block entries.</returns>
    private IReadOnlyCollection<GlycemicDiaryTimeBlockEntry> CreateTimeBlockEntries(
        DateOnly date,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        return _options.TimeBlocks
            .OrderBy(block => block.SortOrder)
            .Select(block => CreateTimeBlockEntry(date, block, readings))
            .ToArray();
    }

    /// <summary>
    /// Creates a single time block entry.
    /// </summary>
    /// <param name="date">The local diary date.</param>
    /// <param name="definition">The time block definition.</param>
    /// <param name="readings">The daily readings.</param>
    /// <returns>The time block entry.</returns>
    private GlycemicDiaryTimeBlockEntry CreateTimeBlockEntry(
        DateOnly date,
        GlycemicDiaryTimeBlockDefinition definition,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        var blockReadings = readings
            .Where(reading =>
            {
                var localTime = GetLocalTime(reading.Timestamp);

                return localTime >= definition.StartsAt &&
                       localTime <= definition.EndsAt;
            })
            .OrderBy(reading => reading.Timestamp.ToUniversalTime())
            .ToArray();

        var representativeReading = GetRepresentativeReading(
            definition,
            blockReadings);

        return new GlycemicDiaryTimeBlockEntry(
            definition.Kind,
            definition.Label,
            definition.StartsAt,
            definition.EndsAt,
            blockReadings.Length,
            representativeReading is null
                ? null
                : GetGlucoseAmountMgDl(representativeReading),
            representativeReading?.Timestamp,
            CalculateAverage(blockReadings),
            CalculateMinimum(blockReadings),
            CalculateMaximum(blockReadings));
    }

    /// <summary>
    /// Gets the representative reading for a time block.
    /// </summary>
    /// <param name="definition">The time block definition.</param>
    /// <param name="readings">The block readings.</param>
    /// <returns>The representative reading.</returns>
    private static GlucoseReading? GetRepresentativeReading(
        GlycemicDiaryTimeBlockDefinition definition,
        IReadOnlyCollection<GlucoseReading> readings)
    {
        if (readings.Count == 0)
        {
            return null;
        }

        var center = definition.StartsAt.ToTimeSpan() +
                     TimeSpan.FromTicks(
                         (definition.EndsAt.ToTimeSpan() - definition.StartsAt.ToTimeSpan()).Ticks / 2);

        return readings
            .OrderBy(reading =>
            {
                var readingTime = TimeOnly
                    .FromDateTime(reading.Timestamp.LocalDateTime)
                    .ToTimeSpan();

                return Math.Abs((readingTime - center).Ticks);
            })
            .First();
    }

    /// <summary>
    /// Calculates the average glucose value in mg/dL.
    /// </summary>
    /// <param name="readings">The readings.</param>
    /// <returns>The average glucose value in mg/dL.</returns>
    private static decimal? CalculateAverage(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        if (readings.Count == 0)
        {
            return null;
        }

        return Math.Round(
            readings.Average(GetGlucoseAmountMgDl),
            0,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calculates the minimum glucose value in mg/dL.
    /// </summary>
    /// <param name="readings">The readings.</param>
    /// <returns>The minimum glucose value in mg/dL.</returns>
    private static decimal? CalculateMinimum(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        return readings.Count == 0
            ? null
            : readings.Min(GetGlucoseAmountMgDl);
    }

    /// <summary>
    /// Calculates the maximum glucose value in mg/dL.
    /// </summary>
    /// <param name="readings">The readings.</param>
    /// <returns>The maximum glucose value in mg/dL.</returns>
    private static decimal? CalculateMaximum(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        return readings.Count == 0
            ? null
            : readings.Max(GetGlucoseAmountMgDl);
    }

    /// <summary>
    /// Calculates the time-in-range percentage.
    /// </summary>
    /// <param name="readings">The readings.</param>
    /// <returns>The time-in-range percentage.</returns>
    private decimal? CalculateTimeInRangePercentage(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        if (readings.Count == 0)
        {
            return null;
        }

        var inRangeCount = readings.Count(reading =>
        {
            var value = GetGlucoseAmountMgDl(reading);

            return value >= _options.LowRangeMgDl &&
                   value <= _options.HighRangeMgDl;
        });

        return Math.Round(
            inRangeCount * 100m / readings.Count,
            2,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Gets the glucose amount in mg/dL.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <returns>The glucose amount in mg/dL.</returns>
    private static decimal GetGlucoseAmountMgDl(GlucoseReading reading)
    {
        return reading.Value.Amount;
    }

    /// <summary>
    /// Enumerates local dates covered by a period.
    /// </summary>
    /// <param name="periodStartsAt">The period start timestamp.</param>
    /// <param name="periodEndsAt">The period end timestamp.</param>
    /// <returns>The local dates.</returns>
    private IEnumerable<DateOnly> EnumerateLocalDates(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt)
    {
        var current = GetLocalDate(periodStartsAt);
        var end = GetLocalDate(periodEndsAt);

        while (current <= end)
        {
            yield return current;

            current = current.AddDays(1);
        }
    }

    /// <summary>
    /// Gets the local date for a timestamp using the diary time zone.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The local date.</returns>
    private DateOnly GetLocalDate(DateTimeOffset timestamp)
    {
        return DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timestamp, _options.TimeZone).DateTime);
    }

    /// <summary>
    /// Gets the local time for a timestamp using the diary time zone.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The local time.</returns>
    private TimeOnly GetLocalTime(DateTimeOffset timestamp)
    {
        return TimeOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timestamp, _options.TimeZone).DateTime);
    }

    /// <summary>
    /// Creates a date-time offset from a local diary date and time.
    /// </summary>
    /// <param name="date">The local date.</param>
    /// <param name="time">The local time.</param>
    /// <returns>The date-time offset.</returns>
    private DateTimeOffset CreateZonedDateTime(
        DateOnly date,
        TimeOnly time)
    {
        var localDateTime = date.ToDateTime(time);
        var offset = _options.TimeZone.GetUtcOffset(localDateTime);

        return new DateTimeOffset(localDateTime, offset);
    }

    /// <summary>
    /// Returns the greater of two date-time offsets.
    /// </summary>
    /// <param name="first">The first timestamp.</param>
    /// <param name="second">The second timestamp.</param>
    /// <returns>The greater timestamp.</returns>
    private static DateTimeOffset MaxDateTimeOffset(
        DateTimeOffset first,
        DateTimeOffset second)
    {
        return first >= second ? first : second;
    }

    /// <summary>
    /// Returns the lower of two date-time offsets.
    /// </summary>
    /// <param name="first">The first timestamp.</param>
    /// <param name="second">The second timestamp.</param>
    /// <returns>The lower timestamp.</returns>
    private static DateTimeOffset MinDateTimeOffset(
        DateTimeOffset first,
        DateTimeOffset second)
    {
        return first <= second ? first : second;
    }

    #endregion
}