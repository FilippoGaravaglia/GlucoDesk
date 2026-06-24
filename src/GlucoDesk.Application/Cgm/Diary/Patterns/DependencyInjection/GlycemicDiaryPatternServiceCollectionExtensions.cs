using GlucoDesk.Application.Cgm.Diary.Patterns.Services;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.Diary.Patterns.DependencyInjection;

/// <summary>
/// Registers glycemic diary pattern analysis services.
/// </summary>
public static class GlycemicDiaryPatternServiceCollectionExtensions
{
    /// <summary>
    /// Adds glycemic diary pattern analysis services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiaryPatternServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IGlycemicDiaryPatternAnalysisService, GlycemicDiaryPatternAnalysisService>();

        return services;
    }
}
