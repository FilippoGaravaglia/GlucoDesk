using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Providers.DependencyInjection;

public sealed class DexcomOfficialCgmProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDexcomOfficialCgmProvider_ShouldRegisterProviderServices()
    {
        var services = new ServiceCollection();

        services.AddDexcomOfficialCgmProvider(
            CreateApiOptions(),
            new DexcomCgmProviderOptions("client-secret"));

        using var serviceProvider = services.BuildServiceProvider();

        var provider = serviceProvider.GetRequiredService<DexcomOfficialCgmProvider>();
        var liveProvider = serviceProvider.GetRequiredService<ICgmLiveProvider>();
        var historicalProvider = serviceProvider.GetRequiredService<ICgmHistoricalProvider>();
        var metadataProvider = serviceProvider.GetRequiredService<ICgmMetadataProvider>();

        Assert.NotNull(provider);
        Assert.Same(provider, liveProvider);
        Assert.Same(provider, historicalProvider);
        Assert.Same(provider, metadataProvider);
    }

    [Fact]
    public void AddDexcomOfficialCgmProvider_ShouldRejectNullServices()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDexcomOfficialCgmProvider(
                CreateApiOptions(),
                new DexcomCgmProviderOptions("client-secret")));

        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddDexcomOfficialCgmProvider_ShouldRejectNullApiOptions()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDexcomOfficialCgmProvider(
                null!,
                new DexcomCgmProviderOptions("client-secret")));

        Assert.Equal("apiOptions", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates Dexcom API options for dependency injection tests.
    /// </summary>
    /// <returns>The Dexcom API options.</returns>
    private static DexcomApiOptions CreateApiOptions()
    {
        return new DexcomApiOptions(
            DexcomApiEnvironment.Sandbox,
            "client-id",
            new Uri("http://127.0.0.1:51234/callback"),
            ["egv", "offline_access"]);
    }

    #endregion
}