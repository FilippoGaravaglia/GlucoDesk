using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;
using GlucoDesk.Infrastructure.Cgm.Dexcom.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.DependencyInjection;

public sealed class DexcomOfficialApiServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDexcomOfficialApi_ShouldRegisterDexcomOfficialApiServices()
    {
        var services = new ServiceCollection();

        services.AddDexcomOfficialApi(CreateOptions());

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<DexcomApiOptions>();
        var endpointProvider = serviceProvider.GetRequiredService<IDexcomApiEndpointProvider>();
        var authorizationUrlBuilder = serviceProvider.GetRequiredService<IDexcomAuthorizationUrlBuilder>();
        var tokenClient = serviceProvider.GetRequiredService<IDexcomTokenClient>();
        var stateOptions = serviceProvider.GetRequiredService<DexcomOAuthStateOptions>();
        var stateGenerator = serviceProvider.GetRequiredService<IDexcomOAuthStateGenerator>();
        var callbackParser = serviceProvider.GetRequiredService<IDexcomOAuthCallbackParser>();
        var callbackListenerOptions = serviceProvider.GetRequiredService<DexcomLocalOAuthCallbackOptions>();
        var callbackListener = serviceProvider.GetRequiredService<IDexcomLocalOAuthCallbackListener>();

        Assert.NotNull(options);
        Assert.NotNull(endpointProvider);
        Assert.NotNull(authorizationUrlBuilder);
        Assert.NotNull(tokenClient);
        Assert.NotNull(stateOptions);
        Assert.NotNull(stateGenerator);
        Assert.NotNull(callbackParser);
        Assert.NotNull(callbackListenerOptions);
        Assert.NotNull(callbackListener);
    }

    [Fact]
    public void AddDexcomOfficialApi_ShouldRejectNullServiceCollection()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDexcomOfficialApi(CreateOptions()));

        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddDexcomOfficialApi_ShouldRejectNullOptions()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddDexcomOfficialApi(null!));

        Assert.Equal("options", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates Dexcom API options for dependency injection tests.
    /// </summary>
    /// <returns>The Dexcom API options.</returns>
    private static DexcomApiOptions CreateOptions()
    {
        return new DexcomApiOptions(
            DexcomApiEnvironment.Sandbox,
            "client-id",
            new Uri("http://127.0.0.1:51234/callback"));
    }

    #endregion
}