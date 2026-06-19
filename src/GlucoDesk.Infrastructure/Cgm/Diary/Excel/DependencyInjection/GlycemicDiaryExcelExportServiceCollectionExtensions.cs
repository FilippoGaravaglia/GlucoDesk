using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.Options;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Diary.Excel.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for glycemic diary Excel export services.
/// </summary>
public static class GlycemicDiaryExcelExportServiceCollectionExtensions
{
    /// <summary>
    /// Registers glycemic diary Excel export services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional Excel export options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGlycemicDiaryExcelExport(
        this IServiceCollection services,
        GlycemicDiaryExcelExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? GlycemicDiaryExcelExportOptions.Default);
        services.TryAddScoped<IGlycemicDiaryExcelExportService, ClosedXmlGlycemicDiaryExcelExportService>();

        return services;
    }
}