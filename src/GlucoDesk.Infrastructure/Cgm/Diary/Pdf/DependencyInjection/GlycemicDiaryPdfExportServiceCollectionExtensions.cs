using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Options;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Diary.Pdf.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for glycemic diary PDF export services.
/// </summary>
public static class GlycemicDiaryPdfExportServiceCollectionExtensions
{
    /// <summary>
    /// Registers glycemic diary PDF export services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional PDF export options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiaryPdfExport(
        this IServiceCollection services,
        GlycemicDiaryPdfExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? GlycemicDiaryPdfExportOptions.Default);
        services.TryAddScoped<IGlycemicDiaryPdfExportService, QuestPdfGlycemicDiaryPdfExportService>();

        return services;
    }
}