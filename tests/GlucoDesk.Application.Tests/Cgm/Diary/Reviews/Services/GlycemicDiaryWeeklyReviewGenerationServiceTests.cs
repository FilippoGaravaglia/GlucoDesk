using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Reviews.Services;

public sealed class GlycemicDiaryWeeklyReviewGenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldCreateReviewUsingCurrentAndDerivedPreviousPeriod()
    {
        // Arrange
        var currentStartsAt = new DateTimeOffset(2026, 6, 8, 0, 0, 0, TimeSpan.Zero);
        var currentEndsAt = new DateTimeOffset(2026, 6, 14, 23, 59, 59, TimeSpan.Zero);

        var expectedPreviousEndsAt = currentStartsAt.AddTicks(-1);
        var expectedPreviousStartsAt = expectedPreviousEndsAt - (currentEndsAt - currentStartsAt);

        var currentReport = CreateReport(currentStartsAt, currentEndsAt, 200);
        var previousReport = CreateReport(expectedPreviousStartsAt, expectedPreviousEndsAt, 180);

        var diaryService = new FakeGlycemicDiaryService(
            currentReport,
            previousReport);

        var weeklyReviewService = new CapturingWeeklyReviewService();

        var service = new GlycemicDiaryWeeklyReviewGenerationService(
            diaryService,
            weeklyReviewService);

        // Act
        var result = await service.GenerateAsync(
            new GlycemicDiaryWeeklyReviewRequest(
                currentStartsAt,
                currentEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, diaryService.Requests.Count);

        Assert.Equal(currentStartsAt, diaryService.Requests[0].PeriodStartsAt);
        Assert.Equal(currentEndsAt, diaryService.Requests[0].PeriodEndsAt);
        Assert.Equal(expectedPreviousStartsAt, diaryService.Requests[1].PeriodStartsAt);
        Assert.Equal(expectedPreviousEndsAt, diaryService.Requests[1].PeriodEndsAt);

        Assert.Same(currentReport, weeklyReviewService.CapturedCurrentReport);
        Assert.Same(previousReport, weeklyReviewService.CapturedPreviousReport);
    }

    [Fact]
    public async Task GenerateAsync_ShouldUseExplicitPreviousPeriod_WhenProvided()
    {
        // Arrange
        var currentStartsAt = new DateTimeOffset(2026, 6, 8, 0, 0, 0, TimeSpan.Zero);
        var currentEndsAt = new DateTimeOffset(2026, 6, 14, 23, 59, 59, TimeSpan.Zero);
        var previousStartsAt = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var previousEndsAt = new DateTimeOffset(2026, 6, 7, 23, 59, 59, TimeSpan.Zero);

        var currentReport = CreateReport(currentStartsAt, currentEndsAt, 200);
        var previousReport = CreateReport(previousStartsAt, previousEndsAt, 180);

        var diaryService = new FakeGlycemicDiaryService(
            currentReport,
            previousReport);

        var service = new GlycemicDiaryWeeklyReviewGenerationService(
            diaryService,
            new CapturingWeeklyReviewService());

        // Act
        var result = await service.GenerateAsync(
            new GlycemicDiaryWeeklyReviewRequest(
                currentStartsAt,
                currentEndsAt,
                previousStartsAt,
                previousEndsAt),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(previousStartsAt, diaryService.Requests[1].PeriodStartsAt);
        Assert.Equal(previousEndsAt, diaryService.Requests[1].PeriodEndsAt);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_WhenRequestIsNull()
    {
        // Arrange
        var service = new GlycemicDiaryWeeklyReviewGenerationService(
            new FakeGlycemicDiaryService(),
            new CapturingWeeklyReviewService());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GenerateAsync(null!, CancellationToken.None));

        // Assert
        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDiaryServiceIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GlycemicDiaryWeeklyReviewGenerationService(
                null!,
                new CapturingWeeklyReviewService()));

        // Assert
        Assert.Equal("diaryService", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenWeeklyReviewServiceIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GlycemicDiaryWeeklyReviewGenerationService(
                new FakeGlycemicDiaryService(),
                null!));

        // Assert
        Assert.Equal("weeklyReviewService", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a deterministic diary report.
    /// </summary>
    /// <param name="periodStartsAt">The period start.</param>
    /// <param name="periodEndsAt">The period end.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <returns>The diary report.</returns>
    private static GlycemicDiaryReport CreateReport(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        int readingsCount)
    {
        return new GlycemicDiaryReport(
            periodStartsAt,
            periodEndsAt,
            readingsCount,
            120m,
            80m,
            160m,
            95m,
            new GlucoseHistoryContinuityReport(
                periodStartsAt,
                periodEndsAt,
                readingsCount,
                100m,
                []),
            []);
    }

    private sealed class FakeGlycemicDiaryService : IGlycemicDiaryService
    {
        private readonly Queue<GlycemicDiaryReport> _reports;

        public FakeGlycemicDiaryService(
            params GlycemicDiaryReport[] reports)
        {
            _reports = new Queue<GlycemicDiaryReport>(reports);
        }

        public List<GlycemicDiaryRequest> Requests { get; } = [];

        public Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
            GlycemicDiaryRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            var report = _reports.Count > 0
                ? _reports.Dequeue()
                : CreateReport(
                    request.PeriodStartsAt,
                    request.PeriodEndsAt,
                    0);

            return Task.FromResult(Result<GlycemicDiaryReport>.Success(report));
        }
    }

    private sealed class CapturingWeeklyReviewService : IGlycemicDiaryWeeklyReviewService
    {
        public GlycemicDiaryReport? CapturedCurrentReport { get; private set; }

        public GlycemicDiaryReport? CapturedPreviousReport { get; private set; }

        public GlycemicDiaryWeeklyReview CreateReview(
            GlycemicDiaryReport currentReport,
            GlycemicDiaryReport previousReport)
        {
            CapturedCurrentReport = currentReport;
            CapturedPreviousReport = previousReport;

            return new GlycemicDiaryWeeklyReview(
                previousReport.PeriodStartsAt,
                previousReport.PeriodEndsAt,
                currentReport.PeriodStartsAt,
                currentReport.PeriodEndsAt,
                "Weekly review: mostly stable period",
                "The current period looks broadly similar to the previous one.",
                "Current history reliability: Complete · 100%.",
                [],
                []);
        }
    }

    #endregion
}
