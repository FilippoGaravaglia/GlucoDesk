using GlucoDesk.Application.Cgm.Diary.Stories.DependencyInjection;
using GlucoDesk.Application.Cgm.Diary.Stories.Services;
using GlucoDesk.Application.Cgm.Diary.Stories.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Stories.DependencyInjection;

public sealed class GlycemicDiaryStoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlycemicDiaryStoryServices_ShouldRegisterStoryService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHistoryCompletenessServices();

        // Act
        services.AddGlycemicDiaryStoryServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetRequiredService<IGlycemicDiaryStoryService>();

        Assert.IsType<GlycemicDiaryStoryService>(service);
    }

    [Fact]
    public void AddGlycemicDiaryStoryServices_ShouldThrow_WhenServicesIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => GlycemicDiaryStoryServiceCollectionExtensions.AddGlycemicDiaryStoryServices(null!));

        // Assert
        Assert.Equal("services", exception.ParamName);
    }
}
