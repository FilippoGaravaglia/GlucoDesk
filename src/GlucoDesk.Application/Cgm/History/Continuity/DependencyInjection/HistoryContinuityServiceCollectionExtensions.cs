using GlucoDesk.Application.Cgm.History.Continuity.Options;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.History.Continuity.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for glucose history continuity services.
/// </summary>
public static class HistoryContinuityServiceCollectionExtensions
{
    /// <summary>
    /// Registers glucose history continuity services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional history continuity options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlucoseHistoryContinuity(
        this IServiceCollection services,
        HistoryContinuityOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? HistoryContinuityOptions.Default);
        services.TryAddSingleton<IGlucoseHistoryContinuityService, GlucoseHistoryContinuityService>();

        return services;
    }
}