using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers;
using GlucoDesk.Infrastructure.Cgm.Mock.Providers;
using Microsoft.Extensions.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.DependencyInjection;

public sealed class DesktopCgmProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDesktopCgmProviders_ShouldRegisterMockProvider_WhenDexcomIsDisabled()
    {
        var services = new ServiceCollection();

        services.AddDesktopCgmProviders(DesktopDexcomProviderOptions.Disabled);

        using var serviceProvider = services.BuildServiceProvider();

        var liveProviders = serviceProvider
            .GetServices<ICgmLiveProvider>()
            .ToArray();

        var historicalProviders = serviceProvider
            .GetServices<ICgmHistoricalProvider>()
            .ToArray();

        var metadataProviders = serviceProvider
            .GetServices<ICgmMetadataProvider>()
            .ToArray();

        var desktopConnectionServices = serviceProvider
            .GetServices<IDexcomDesktopConnectionService>()
            .ToArray();

        Assert.Single(liveProviders);
        Assert.Single(historicalProviders);
        Assert.Single(metadataProviders);

        Assert.IsType<MockCgmProvider>(liveProviders[0]);
        Assert.IsType<MockCgmProvider>(historicalProviders[0]);
        Assert.IsType<MockCgmProvider>(metadataProviders[0]);

        Assert.Empty(desktopConnectionServices);
    }

    [Fact]
    public void AddDesktopCgmProviders_ShouldRegisterMockAndDexcomProviders_WhenDexcomIsEnabled()
    {
        var services = new ServiceCollection();

        services.AddDesktopCgmProviders(CreateEnabledDexcomOptions());

        using var serviceProvider = services.BuildServiceProvider();

        var liveProviders = serviceProvider
            .GetServices<ICgmLiveProvider>()
            .ToArray();

        var historicalProviders = serviceProvider
            .GetServices<ICgmHistoricalProvider>()
            .ToArray();

        var metadataProviders = serviceProvider
            .GetServices<ICgmMetadataProvider>()
            .ToArray();

        var desktopConnectionServices = serviceProvider
            .GetServices<IDexcomDesktopConnectionService>()
            .ToArray();

        Assert.Equal(2, liveProviders.Length);
        Assert.Equal(2, historicalProviders.Length);
        Assert.Equal(2, metadataProviders.Length);

        Assert.Contains(liveProviders, provider => provider is MockCgmProvider);
        Assert.Contains(liveProviders, provider => provider is DexcomOfficialCgmProvider);

        Assert.Contains(historicalProviders, provider => provider is MockCgmProvider);
        Assert.Contains(historicalProviders, provider => provider is DexcomOfficialCgmProvider);

        Assert.Contains(metadataProviders, provider => provider is MockCgmProvider);
        Assert.Contains(metadataProviders, provider => provider is DexcomOfficialCgmProvider);

        Assert.Single(desktopConnectionServices);
        Assert.IsType<DexcomDesktopConnectionService>(desktopConnectionServices[0]);
    }

    [Fact]
    public void AddDesktopCgmProviders_ShouldRejectNullServices()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDesktopCgmProviders(DesktopDexcomProviderOptions.Disabled));

        Assert.Equal("services", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates enabled Dexcom desktop provider options for tests.
    /// </summary>
    /// <returns>The enabled Dexcom desktop provider options.</returns>
    private static DesktopDexcomProviderOptions CreateEnabledDexcomOptions()
    {
        return new DesktopDexcomProviderOptions(
            isEnabled: true,
            environment: DexcomApiEnvironment.Sandbox,
            clientId: "client-id",
            clientSecret: "client-secret",
            redirectUri: new Uri("http://127.0.0.1:51234/callback"),
            scopes: ["egv", "offline_access"]);
    }

    #endregion
}