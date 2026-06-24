using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Patterns.DependencyInjection;
using GlucoDesk.Application.Cgm.Diary.Reviews.DependencyInjection;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Reviews.DependencyInjection;

public sealed class GlycemicDiaryReviewServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlycemicDiaryReviewServices_ShouldRegisterWeeklyReviewService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHistoryCompletenessServices();
        services.AddGlycemicDiaryPatternServices();
        services.AddScoped<IGlycemicDiaryService, FakeGlycemicDiaryService>();

        // Act
        services.AddGlycemicDiaryReviewServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetRequiredService<IGlycemicDiaryWeeklyReviewService>();
        var generationService = provider.GetRequiredService<IGlycemicDiaryWeeklyReviewGenerationService>();

        Assert.IsType<GlycemicDiaryWeeklyReviewService>(service);
        Assert.IsType<GlycemicDiaryWeeklyReviewGenerationService>(generationService);
    }

    [Fact]
    public void AddGlycemicDiaryReviewServices_ShouldThrow_WhenServicesIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => GlycemicDiaryReviewServiceCollectionExtensions.AddGlycemicDiaryReviewServices(null!));

        // Assert
        Assert.Equal("services", exception.ParamName);
    }
    #region Helpers

    private sealed class FakeGlycemicDiaryService : IGlycemicDiaryService
    {
        public Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
            GlycemicDiaryRequest request,
            CancellationToken cancellationToken)
        {
            var report = new GlycemicDiaryReport(
                request.PeriodStartsAt,
                request.PeriodEndsAt,
                0,
                null,
                null,
                null,
                null,
                new GlucoseHistoryContinuityReport(
                    request.PeriodStartsAt,
                    request.PeriodEndsAt,
                    0,
                    0m,
                    []),
                []);

            return Task.FromResult(Result<GlycemicDiaryReport>.Success(report));
        }
    }

    #endregion
}
