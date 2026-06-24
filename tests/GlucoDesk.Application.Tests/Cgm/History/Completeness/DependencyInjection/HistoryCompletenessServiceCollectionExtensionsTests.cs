using GlucoDesk.Application.Cgm.History.Completeness.DependencyInjection;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Tests.Cgm.History.Completeness.DependencyInjection;

public sealed class HistoryCompletenessServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHistoryCompletenessServices_ShouldRegisterScoringService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHistoryCompletenessServices();

        using var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetRequiredService<IGlucoseHistoryCompletenessScoringService>();

        Assert.IsType<GlucoseHistoryCompletenessScoringService>(service);
    }

    [Fact]
    public void AddHistoryCompletenessServices_ShouldThrow_WhenServicesIsNull()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => HistoryCompletenessServiceCollectionExtensions.AddHistoryCompletenessServices(null!));

        // Assert
        Assert.Equal("services", exception.ParamName);
    }
}
