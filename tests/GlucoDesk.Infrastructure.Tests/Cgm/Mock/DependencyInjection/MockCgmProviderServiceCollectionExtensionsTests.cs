using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Mock.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Mock.Options;
using GlucoDesk.Infrastructure.Cgm.Mock.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Mock.DependencyInjection;

public sealed class MockCgmProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMockCgmProvider_ShouldRegisterMockProviderServices()
    {
        var services = new ServiceCollection();

        services.AddMockCgmProvider(new MockCgmProviderOptions(deviceName: "DI Mock CGM"));

        using var serviceProvider = services.BuildServiceProvider();

        var concreteProvider = serviceProvider.GetRequiredService<MockCgmProvider>();
        var liveProvider = serviceProvider.GetRequiredService<ICgmLiveProvider>();
        var historicalProvider = serviceProvider.GetRequiredService<ICgmHistoricalProvider>();
        var metadataProvider = serviceProvider.GetRequiredService<ICgmMetadataProvider>();

        Assert.Same(concreteProvider, liveProvider);
        Assert.Same(concreteProvider, historicalProvider);
        Assert.Same(concreteProvider, metadataProvider);
    }

    [Fact]
    public void AddMockCgmProvider_ShouldRejectNullServiceCollection()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddMockCgmProvider());

        Assert.Equal("services", exception.ParamName);
    }
}