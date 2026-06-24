using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.DependencyInjection;

/// <summary>
/// Registers glycemic diary review services.
/// </summary>
public static class GlycemicDiaryReviewServiceCollectionExtensions
{
    /// <summary>
    /// Adds glycemic diary review services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiaryReviewServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IGlycemicDiaryWeeklyReviewService, GlycemicDiaryWeeklyReviewService>();

        return services;
    }
}
