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

        // Act
        services.AddGlycemicDiaryReviewServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetRequiredService<IGlycemicDiaryWeeklyReviewService>();

        Assert.IsType<GlycemicDiaryWeeklyReviewService>(service);
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
}
