using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Mock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;
using GlucoDesk.Infrastructure.Cgm.Nightscout.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Services;

namespace GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;

/// <summary>
/// Provides desktop dependency injection registrations for CGM providers.
/// </summary>
public static class DesktopCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds desktop CGM providers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dexcomOptions">The optional Dexcom provider options.</param>
    /// <param name="nightscoutOptions">The optional Nightscout provider options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDesktopCgmProviders(
        this IServiceCollection services,
        DesktopDexcomProviderOptions? dexcomOptions = null,
        DesktopNightscoutProviderOptions? nightscoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);

        services.AddMockCgmProvider();

        var effectiveDexcomOptions = dexcomOptions ?? DesktopDexcomProviderOptions.FromEnvironmentVariables();

        if (effectiveDexcomOptions.IsEnabled)
        {
            services.TryAddSingleton(effectiveDexcomOptions);

            services.AddDexcomOfficialCgmProvider(
                effectiveDexcomOptions.ToApiOptions(),
                effectiveDexcomOptions.ToProviderOptions());

            services.AddDexcomConnectionStatus();

            services.TryAddScoped<IDexcomDesktopConnectionService, DexcomDesktopConnectionService>();
        }

        var effectiveNightscoutOptions = nightscoutOptions ?? DesktopNightscoutProviderOptions.FromEnvironmentVariables();

        if (effectiveNightscoutOptions.IsEnabled)
        {
            services.TryAddSingleton(effectiveNightscoutOptions);
        
            services.AddNightscoutCgmProvider(
                effectiveNightscoutOptions.ToNightscoutOptions());
        
            services.AddHttpClient<INightscoutDesktopConnectionService, NightscoutDesktopConnectionService>();
        }

        return services;
    }
}