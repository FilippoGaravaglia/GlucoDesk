using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for beverages.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetBeverageFoods()
    {
        return
        [
            new(
                "latte-parzialmente-scremato",
                CarbGuideCategoryId.Beverages,
                10,
                new(
                    "Latte parzialmente scremato",
                    "Semi-skimmed milk"),
                new[]
                {
                    new CarbGuidePortion(100, 5),
                    new CarbGuidePortion(200, 10),
                    new CarbGuidePortion(300, 15),
                }),
            new(
                "succo-frutta-zuccherato",
                CarbGuideCategoryId.Beverages,
                20,
                new(
                    "Succo di frutta zuccherato",
                    "Sweetened fruit juice"),
                new[]
                {
                    new CarbGuidePortion(100, 14),
                    new CarbGuidePortion(150, 22),
                    new CarbGuidePortion(200, 29),
                }),
            new(
                "succo-frutta-non-zuccherato",
                CarbGuideCategoryId.Beverages,
                30,
                new(
                    "Succo di frutta non zuccherato",
                    "Unsweetened fruit juice"),
                new[]
                {
                    new CarbGuidePortion(100, 10),
                    new CarbGuidePortion(150, 15),
                    new CarbGuidePortion(200, 20),
                }),
            new(
                "spremuta-arancia",
                CarbGuideCategoryId.Beverages,
                40,
                new(
                    "Spremuta di arancia",
                    "Fresh orange juice"),
                new[]
                {
                    new CarbGuidePortion(100, 8),
                    new CarbGuidePortion(150, 12),
                    new CarbGuidePortion(200, 16),
                }),
            new(
                "te-non-zuccherato",
                CarbGuideCategoryId.Beverages,
                50,
                new(
                    "Tè non zuccherato",
                    "Unsweetened tea"),
                new[]
                {
                    new CarbGuidePortion(200, 0),
                }),
            new(
                "caffe-non-zuccherato",
                CarbGuideCategoryId.Beverages,
                60,
                new(
                    "Caffè non zuccherato",
                    "Unsweetened coffee"),
                new[]
                {
                    new CarbGuidePortion(200, 0),
                }),
            new(
                "cappuccino-non-zuccherato",
                CarbGuideCategoryId.Beverages,
                70,
                new(
                    "Cappuccino non zuccherato",
                    "Unsweetened cappuccino"),
                new[]
                {
                    new CarbGuidePortion(150, 5),
                }),
            new(
                "vino",
                CarbGuideCategoryId.Beverages,
                80,
                new(
                    "Vino",
                    "Wine"),
                new[]
                {
                    new CarbGuidePortion(200, 0),
                    new CarbGuidePortion(400, 0),
                }),
            new(
                "birra",
                CarbGuideCategoryId.Beverages,
                90,
                new(
                    "Birra",
                    "Beer"),
                new[]
                {
                    new CarbGuidePortion(200, 7),
                    new CarbGuidePortion(330, 11),
                    new CarbGuidePortion(400, 14),
                }),
        ];
    }
}
