using GlucoDesk.Application.Cgm.Services;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Common.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for the application layer.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers GlucoDesk application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddGlucoDeskApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<IApplicationSettingsChangeNotifier, ApplicationSettingsChangeNotifier>();

        services.AddScoped<IGlucoseDataService, GlucoseDataService>();
        services.AddScoped<IApplicationSettingsService, ApplicationSettingsService>();

        return services;
    }
}