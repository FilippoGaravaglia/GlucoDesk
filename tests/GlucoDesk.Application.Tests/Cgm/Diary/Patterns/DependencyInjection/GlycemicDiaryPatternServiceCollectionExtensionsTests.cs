using GlucoDesk.Application.Cgm.Diary.Patterns.DependencyInjection;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Patterns.DependencyInjection;

public sealed class GlycemicDiaryPatternServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlycemicDiaryPatternServices_ShouldRegisterPatternAnalysisService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHistoryCompletenessServices();

        // Act
        services.AddGlycemicDiaryPatternServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetRequiredService<IGlycemicDiaryPatternAnalysisService>();

        Assert.IsType<GlycemicDiaryPatternAnalysisService>(service);
    }

    [Fact]
    public void AddGlycemicDiaryPatternServices_ShouldThrow_WhenServicesIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => GlycemicDiaryPatternServiceCollectionExtensions.AddGlycemicDiaryPatternServices(null!));

        // Assert
        Assert.Equal("services", exception.ParamName);
    }
}
