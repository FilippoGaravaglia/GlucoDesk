using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for legumes.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetLegumeFoods()
    {
        return
        [
            new(
                "fagioli-freschi",
                CarbGuideCategoryId.Legumes,
                10,
                new(
                    "Fagioli freschi",
                    "Fresh beans"),
                new[]
                {
                    new CarbGuidePortion(100, 23),
                    new CarbGuidePortion(150, 34),
                    new CarbGuidePortion(200, 45),
                }),
            new(
                "ceci-secchi",
                CarbGuideCategoryId.Legumes,
                20,
                new(
                    "Ceci secchi",
                    "Dried chickpeas"),
                new[]
                {
                    new CarbGuidePortion(25, 12),
                    new CarbGuidePortion(50, 23),
                    new CarbGuidePortion(75, 35),
                }),
            new(
                "piselli-surgelati",
                CarbGuideCategoryId.Legumes,
                30,
                new(
                    "Piselli surgelati",
                    "Frozen peas"),
                new[]
                {
                    new CarbGuidePortion(100, 13),
                    new CarbGuidePortion(150, 19),
                    new CarbGuidePortion(200, 26),
                }),
        ];
    }
}
