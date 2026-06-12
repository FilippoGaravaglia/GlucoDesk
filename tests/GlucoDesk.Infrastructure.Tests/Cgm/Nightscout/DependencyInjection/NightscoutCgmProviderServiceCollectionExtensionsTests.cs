using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;
using GlucoDesk.Infrastructure.Cgm.Nightscout.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.DependencyInjection;

public sealed class NightscoutCgmProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNightscoutCgmProvider_ShouldRegisterProviderServices()
    {
        var services = new ServiceCollection();

        services.AddNightscoutCgmProvider(
            new NightscoutOptions(new Uri("https://example-nightscout.test")));

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<INightscoutEntryMapper>());
        Assert.NotNull(serviceProvider.GetRequiredService<INightscoutEntriesClient>());
        Assert.Contains(
            serviceProvider.GetServices<ICgmLiveProvider>(),
            provider => provider.GetType().Name == "NightscoutCgmProvider");
        Assert.Contains(
            serviceProvider.GetServices<ICgmHistoricalProvider>(),
            provider => provider.GetType().Name == "NightscoutCgmProvider");
        Assert.Contains(
            serviceProvider.GetServices<ICgmMetadataProvider>(),
            provider => provider.GetType().Name == "NightscoutCgmProvider");
    }

    [Fact]
    public void AddNightscoutCgmProvider_ShouldRejectNullServices()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => NightscoutCgmProviderServiceCollectionExtensions.AddNightscoutCgmProvider(
                null!,
                new NightscoutOptions(new Uri("https://example-nightscout.test"))));

        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddNightscoutCgmProvider_ShouldRejectNullOptions()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddNightscoutCgmProvider(null!));

        Assert.Equal("options", exception.ParamName);
    }
}