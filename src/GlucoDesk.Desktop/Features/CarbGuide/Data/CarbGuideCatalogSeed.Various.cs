using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for miscellaneous foods and condiments.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetVariousFoods()
    {
        return
        [
            new(
                "panna",
                CarbGuideCategoryId.Various,
                10,
                new(
                    "Panna",
                    "Cream"),
                new[]
                {
                    new CarbGuidePortion(20, 0.5m),
                }),
            new(
                "ketchup",
                CarbGuideCategoryId.Various,
                20,
                new(
                    "Ketchup",
                    "Ketchup"),
                new[]
                {
                    new CarbGuidePortion(30, 7),
                }),
            new(
                "burro",
                CarbGuideCategoryId.Various,
                30,
                new(
                    "Burro",
                    "Butter"),
                new[]
                {
                    new CarbGuidePortion(10, 0.5m),
                }),
        ];
    }
}
