using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;
using GlucoDesk.Infrastructure.Cgm.Dexcom.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;
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
        var systemBrowser = serviceProvider.GetRequiredService<IDexcomSystemBrowser>();
        var authorizationSessionService = serviceProvider.GetRequiredService<IDexcomOAuthAuthorizationSessionService>();
        var tokenStore = serviceProvider.GetRequiredService<IDexcomOAuthTokenStore>();
        var tokenRefreshOptions = serviceProvider.GetRequiredService<DexcomOAuthTokenRefreshOptions>();
        var tokenService = serviceProvider.GetRequiredService<IDexcomOAuthTokenService>();
        var egvClient = serviceProvider.GetRequiredService<IDexcomEgvClient>();
        var egvMapper = serviceProvider.GetRequiredService<IDexcomEgvMapper>();

        Assert.NotNull(options);
        Assert.NotNull(endpointProvider);
        Assert.NotNull(authorizationUrlBuilder);
        Assert.NotNull(tokenClient);
        Assert.NotNull(stateOptions);
        Assert.NotNull(stateGenerator);
        Assert.NotNull(callbackParser);
        Assert.NotNull(callbackListenerOptions);
        Assert.NotNull(callbackListener);
        Assert.NotNull(systemBrowser);
        Assert.NotNull(authorizationSessionService);
        Assert.NotNull(tokenStore);
        Assert.NotNull(tokenRefreshOptions);
        Assert.NotNull(tokenService);
        Assert.NotNull(egvClient);
        Assert.NotNull(egvMapper);
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