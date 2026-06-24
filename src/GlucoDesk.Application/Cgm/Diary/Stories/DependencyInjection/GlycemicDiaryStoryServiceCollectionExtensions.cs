using GlucoDesk.Application.Cgm.Diary.Stories.Services;
using GlucoDesk.Application.Cgm.Diary.Stories.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.Diary.Stories.DependencyInjection;

/// <summary>
/// Registers glycemic diary story services.
/// </summary>
public static class GlycemicDiaryStoryServiceCollectionExtensions
{
    /// <summary>
    /// Adds glycemic diary story services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiaryStoryServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IGlycemicDiaryStoryService, GlycemicDiaryStoryService>();

        return services;
    }
}
