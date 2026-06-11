using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Mock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;

/// <summary>
/// Provides desktop dependency injection registrations for CGM providers.
/// </summary>
public static class DesktopCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers desktop CGM providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dexcomOptions">The optional Dexcom provider options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddDesktopCgmProviders(
        this IServiceCollection services,
        DesktopDexcomProviderOptions? dexcomOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);

        services.AddMockCgmProvider();

        var effectiveDexcomOptions = dexcomOptions ?? DesktopDexcomProviderOptions.FromEnvironmentVariables();

        if (effectiveDexcomOptions.IsEnabled)
        {
            services.AddDexcomOfficialCgmProvider(
                effectiveDexcomOptions.ToApiOptions(),
                effectiveDexcomOptions.ToProviderOptions());
        }

        return services;
    }
}