using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.History.Completeness.DependencyInjection;

/// <summary>
/// Registers local glucose history completeness services.
/// </summary>
public static class HistoryCompletenessServiceCollectionExtensions
{
    /// <summary>
    /// Adds local glucose history completeness services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddHistoryCompletenessServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<
            IGlucoseHistoryCompletenessScoringService,
            GlucoseHistoryCompletenessScoringService>();

        return services;
    }
}
