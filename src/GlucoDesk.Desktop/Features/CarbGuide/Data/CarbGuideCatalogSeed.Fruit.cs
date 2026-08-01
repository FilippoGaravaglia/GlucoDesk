using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for fruit.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetFruitFoods()
    {
        return
        [
            new(
                "ananas-fresco",
                CarbGuideCategoryId.Fruit,
                10,
                new(
                    "Ananas fresco",
                    "Fresh pineapple"),
                new[]
                {
                    new CarbGuidePortion(100, 10),
                    new CarbGuidePortion(150, 15),
                    new CarbGuidePortion(200, 20),
                }),
            new(
                "pesche",
                CarbGuideCategoryId.Fruit,
                20,
                new(
                    "Pesche",
                    "Peaches"),
                new[]
                {
                    new CarbGuidePortion(100, 6),
                    new CarbGuidePortion(150, 9),
                    new CarbGuidePortion(250, 15),
                }),
            new(
                "kiwi",
                CarbGuideCategoryId.Fruit,
                30,
                new(
                    "Kiwi",
                    "Kiwi"),
                new[]
                {
                    new CarbGuidePortion(100, 9),
                    new CarbGuidePortion(150, 14),
                    new CarbGuidePortion(200, 18),
                }),
            new(
                "melone",
                CarbGuideCategoryId.Fruit,
                40,
                new(
                    "Melone",
                    "Melon"),
                new[]
                {
                    new CarbGuidePortion(100, 7),
                    new CarbGuidePortion(200, 15),
                    new CarbGuidePortion(300, 22),
                }),
            new(
                "banana",
                CarbGuideCategoryId.Fruit,
                50,
                new(
                    "Banana",
                    "Banana"),
                new[]
                {
                    new CarbGuidePortion(100, 15),
                    new CarbGuidePortion(130, 20),
                }),
            new(
                "mela",
                CarbGuideCategoryId.Fruit,
                60,
                new(
                    "Mela",
                    "Apple"),
                new[]
                {
                    new CarbGuidePortion(100, 10),
                    new CarbGuidePortion(150, 15),
                    new CarbGuidePortion(200, 20),
                }),
            new(
                "fragole",
                CarbGuideCategoryId.Fruit,
                70,
                new(
                    "Fragole",
                    "Strawberries"),
                new[]
                {
                    new CarbGuidePortion(100, 5),
                    new CarbGuidePortion(150, 8),
                    new CarbGuidePortion(200, 10),
                }),
            new(
                "fichi",
                CarbGuideCategoryId.Fruit,
                80,
                new(
                    "Fichi",
                    "Figs"),
                new[]
                {
                    new CarbGuidePortion(60, 7),
                    new CarbGuidePortion(100, 11),
                    new CarbGuidePortion(150, 17),
                }),
            new(
                "macedonia",
                CarbGuideCategoryId.Fruit,
                90,
                new(
                    "Macedonia",
                    "Fruit salad"),
                new[]
                {
                    new CarbGuidePortion(100, 17),
                    new CarbGuidePortion(150, 25),
                    new CarbGuidePortion(200, 34),
                }),
        ];
    }
}
