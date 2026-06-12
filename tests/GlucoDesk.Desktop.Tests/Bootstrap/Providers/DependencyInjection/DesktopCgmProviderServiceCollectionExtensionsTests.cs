using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers;
using GlucoDesk.Infrastructure.Cgm.Mock.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.DependencyInjection;

public sealed class DesktopCgmProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDesktopCgmProviders_ShouldRegisterMockProvider_WhenDexcomIsDisabled()
    {
        var services = new ServiceCollection();

        services.AddDesktopCgmProviders(
            dexcomOptions: DesktopDexcomProviderOptions.Disabled,
            nightscoutOptions: CreateDisabledNightscoutOptions());

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

        services.AddDesktopCgmProviders(
            dexcomOptions: CreateEnabledDexcomOptions(),
            nightscoutOptions: CreateDisabledNightscoutOptions());

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
    public async Task AddDesktopCgmProviders_ShouldRegisterNightscoutProvider_WhenNightscoutIsEnabled()
    {
        var services = new ServiceCollection();

        services.AddDesktopCgmProviders(
            dexcomOptions: DesktopDexcomProviderOptions.Disabled,
            nightscoutOptions: new DesktopNightscoutProviderOptions(
                true,
                new Uri("https://example-nightscout.test")));

        using var serviceProvider = services.BuildServiceProvider();

        var metadataProviders = serviceProvider
            .GetServices<ICgmMetadataProvider>()
            .ToArray();

        var metadataResults = new List<CgmProviderKind>();

        foreach (var provider in metadataProviders)
        {
            var metadataResult = await provider.GetMetadataAsync(CancellationToken.None);

            if (metadataResult.IsSuccess)
            {
                metadataResults.Add(metadataResult.Value.ProviderKind);
            }
        }

        Assert.Contains(CgmProviderKind.Mock, metadataResults);
        Assert.Contains(CgmProviderKind.Nightscout, metadataResults);
        Assert.DoesNotContain(CgmProviderKind.DexcomSandbox, metadataResults);
        Assert.DoesNotContain(CgmProviderKind.DexcomOfficial, metadataResults);
    }

    [Fact]
    public void AddDesktopCgmProviders_ShouldRejectNullServices()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDesktopCgmProviders(
                dexcomOptions: DesktopDexcomProviderOptions.Disabled,
                nightscoutOptions: CreateDisabledNightscoutOptions()));

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

    /// <summary>
    /// Creates disabled Nightscout desktop provider options for tests.
    /// </summary>
    /// <returns>The disabled Nightscout desktop provider options.</returns>
    private static DesktopNightscoutProviderOptions CreateDisabledNightscoutOptions()
    {
        return new DesktopNightscoutProviderOptions(false, null);
    }

    #endregion
}