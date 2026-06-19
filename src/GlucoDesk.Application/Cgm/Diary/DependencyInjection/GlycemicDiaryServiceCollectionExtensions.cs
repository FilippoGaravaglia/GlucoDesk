using GlucoDesk.Application.Cgm.Diary.Options;
using GlucoDesk.Application.Cgm.Diary.Services;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Application.Cgm.Diary.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for glycemic diary services.
/// </summary>
public static class GlycemicDiaryServiceCollectionExtensions
{
    /// <summary>
    /// Registers glycemic diary services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional glycemic diary options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiary(
        this IServiceCollection services,
        GlycemicDiaryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? GlycemicDiaryOptions.Default);
        services.TryAddScoped<IGlycemicDiaryService, GlycemicDiaryService>();

        return services;
    }
}