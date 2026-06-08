using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for Dexcom Official API infrastructure.
/// </summary>
public static class DexcomOfficialApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dexcom Official API infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Dexcom API options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or options is null.</exception>
    public static IServiceCollection AddDexcomOfficialApi(
        this IServiceCollection services,
        DexcomApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(options);
        services.TryAddSingleton(DexcomOAuthStateOptions.Default);
        services.TryAddSingleton(DexcomLocalOAuthCallbackOptions.Default);
        services.TryAddSingleton(DexcomOAuthTokenRefreshOptions.Default);
        services.TryAddSingleton<IDexcomApiEndpointProvider, DexcomApiEndpointProvider>();
        services.TryAddSingleton<IDexcomAuthorizationUrlBuilder, DexcomAuthorizationUrlBuilder>();
        services.TryAddSingleton<IDexcomOAuthStateGenerator, DexcomOAuthStateGenerator>();
        services.TryAddSingleton<IDexcomOAuthCallbackParser, DexcomOAuthCallbackParser>();
        services.TryAddSingleton<IDexcomLocalOAuthCallbackListener, DexcomLocalOAuthCallbackListener>();
        services.TryAddSingleton<IDexcomSystemBrowser, DexcomSystemBrowser>();
        services.TryAddSingleton<IDexcomOAuthTokenStore, InMemoryDexcomOAuthTokenStore>();
        services.TryAddSingleton<IDexcomOAuthTokenService, DexcomOAuthTokenService>();
        services.TryAddSingleton<IDexcomOAuthAuthorizationSessionService, DexcomOAuthAuthorizationSessionService>();

        services.AddHttpClient<IDexcomTokenClient, DexcomTokenClient>();

        return services;
    }
}