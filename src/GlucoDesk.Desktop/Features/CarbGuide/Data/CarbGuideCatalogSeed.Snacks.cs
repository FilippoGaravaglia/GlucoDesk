using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for snacks.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetSnackFoods()
    {
        return
        [
            new(
                "toast",
                CarbGuideCategoryId.Snacks,
                10,
                new(
                    "Toast",
                    "Ham and cheese toast"),
                new[]
                {
                    new CarbGuidePortion(80, 19),
                }),
            new(
                "panino-al-prosciutto",
                CarbGuideCategoryId.Snacks,
                20,
                new(
                    "Panino al prosciutto",
                    "Ham sandwich"),
                new[]
                {
                    new CarbGuidePortion(110, 40),
                    new CarbGuidePortion(130, 54),
                }),
            new(
                "pizzetta-pomodoro-mozzarella",
                CarbGuideCategoryId.Snacks,
                30,
                new(
                    "Pizzetta pomodoro e mozzarella",
                    "Tomato and mozzarella mini pizza"),
                new[]
                {
                    new CarbGuidePortion(100, 53),
                }),
            new(
                "tramezzino",
                CarbGuideCategoryId.Snacks,
                40,
                new(
                    "Tramezzino",
                    "Triangular sandwich"),
                new[]
                {
                    new CarbGuidePortion(60, 14),
                }),
            new(
                "pop-corn",
                CarbGuideCategoryId.Snacks,
                50,
                new(
                    "Pop corn",
                    "Popcorn"),
                new[]
                {
                    new CarbGuidePortion(10, 7),
                }),
            new(
                "yogurt-frutta-zuccherato",
                CarbGuideCategoryId.Snacks,
                60,
                new(
                    "Yogurt alla frutta zuccherato",
                    "Sweetened fruit yogurt"),
                new[]
                {
                    new CarbGuidePortion(125, 20),
                    new CarbGuidePortion(250, 40),
                    new CarbGuidePortion(375, 60),
                }),
            new(
                "yogurt-frutta-non-zuccherato",
                CarbGuideCategoryId.Snacks,
                70,
                new(
                    "Yogurt alla frutta non zuccherato",
                    "Unsweetened fruit yogurt"),
                new[]
                {
                    new CarbGuidePortion(125, 7),
                    new CarbGuidePortion(250, 14),
                    new CarbGuidePortion(375, 21),
                }),
            new(
                "yogurt-naturale-magro",
                CarbGuideCategoryId.Snacks,
                80,
                new(
                    "Yogurt naturale magro",
                    "Low-fat plain yogurt"),
                new[]
                {
                    new CarbGuidePortion(125, 5),
                    new CarbGuidePortion(250, 10),
                    new CarbGuidePortion(375, 15),
                }),
        ];
    }
}
