using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetAppetizerFoods()
    {
        return
        [
            new(
                "prosciutto-melone",
                CarbGuideCategoryId.Appetizers,
                10,
                new(
                    "Prosciutto e melone",
                    "Prosciutto and melon"),
                new[]
                {
                    new CarbGuidePortion(
                        240,
                        15),
                }),
            new(
                "affettato-misto",
                CarbGuideCategoryId.Appetizers,
                20,
                new(
                    "Affettato misto",
                    "Mixed cured meats"),
                new[]
                {
                    new CarbGuidePortion(
                        50,
                        0),
                }),
            new(
                "crostini",
                CarbGuideCategoryId.Appetizers,
                30,
                new(
                    "Crostini",
                    "Crostini"),
                new[]
                {
                    new CarbGuidePortion(
                        100,
                        14),
                }),
        ];
    }
}
