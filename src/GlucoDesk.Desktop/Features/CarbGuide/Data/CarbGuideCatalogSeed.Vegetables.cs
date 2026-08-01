using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for vegetables.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetVegetableFoods()
    {
        return
        [
            new(
                "insalata",
                CarbGuideCategoryId.Vegetables,
                10,
                new(
                    "Insalata",
                    "Salad"),
                new[]
                {
                    new CarbGuidePortion(30, 0),
                    new CarbGuidePortion(50, 1),
                    new CarbGuidePortion(70, 2),
                }),
            new(
                "pomodori",
                CarbGuideCategoryId.Vegetables,
                20,
                new(
                    "Pomodori",
                    "Tomatoes"),
                new[]
                {
                    new CarbGuidePortion(100, 3),
                    new CarbGuidePortion(200, 7),
                    new CarbGuidePortion(300, 10),
                }),
            new(
                "carote",
                CarbGuideCategoryId.Vegetables,
                30,
                new(
                    "Carote",
                    "Carrots"),
                new[]
                {
                    new CarbGuidePortion(50, 4),
                    new CarbGuidePortion(100, 8),
                    new CarbGuidePortion(150, 11),
                }),
            new(
                "patate-lesse",
                CarbGuideCategoryId.Vegetables,
                40,
                new(
                    "Patate lesse",
                    "Boiled potatoes"),
                new[]
                {
                    new CarbGuidePortion(100, 18),
                    new CarbGuidePortion(150, 27),
                    new CarbGuidePortion(200, 36),
                }),
            new(
                "patate-fritte",
                CarbGuideCategoryId.Vegetables,
                50,
                new(
                    "Patate fritte",
                    "French fries"),
                new[]
                {
                    new CarbGuidePortion(50, 30),
                    new CarbGuidePortion(100, 30),
                }),
        ];
    }
}
