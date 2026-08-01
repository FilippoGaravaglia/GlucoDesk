using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for bread and cereals.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetBreadAndCerealFoods()
    {
        return
        [
            new(
                "pane-comune",
                CarbGuideCategoryId.BreadAndCereals,
                10,
                new(
                    "Pane comune",
                    "White bread"),
                new[]
                {
                    new CarbGuidePortion(40, 27),
                    new CarbGuidePortion(60, 40),
                    new CarbGuidePortion(100, 67),
                }),
            new(
                "pane-integrale",
                CarbGuideCategoryId.BreadAndCereals,
                20,
                new(
                    "Pane integrale",
                    "Wholemeal bread"),
                new[]
                {
                    new CarbGuidePortion(30, 14),
                    new CarbGuidePortion(60, 29),
                    new CarbGuidePortion(90, 43),
                }),
            new(
                "panini-al-latte",
                CarbGuideCategoryId.BreadAndCereals,
                30,
                new(
                    "Panini al latte",
                    "Milk rolls"),
                new[]
                {
                    new CarbGuidePortion(30, 14),
                    new CarbGuidePortion(60, 28),
                    new CarbGuidePortion(90, 43),
                }),
            new(
                "fette-biscottate",
                CarbGuideCategoryId.BreadAndCereals,
                40,
                new(
                    "Fette biscottate",
                    "Rusks"),
                new[]
                {
                    new CarbGuidePortion(15, 12),
                    new CarbGuidePortion(30, 25),
                    new CarbGuidePortion(45, 37),
                }),
            new(
                "cornflakes",
                CarbGuideCategoryId.BreadAndCereals,
                50,
                new(
                    "Cornflakes",
                    "Cornflakes"),
                new[]
                {
                    new CarbGuidePortion(15, 13),
                    new CarbGuidePortion(30, 26),
                    new CarbGuidePortion(45, 39),
                }),
            new(
                "muesli",
                CarbGuideCategoryId.BreadAndCereals,
                60,
                new(
                    "Muesli",
                    "Muesli"),
                new[]
                {
                    new CarbGuidePortion(15, 11),
                    new CarbGuidePortion(30, 22),
                    new CarbGuidePortion(45, 32),
                }),
            new(
                "grissini",
                CarbGuideCategoryId.BreadAndCereals,
                70,
                new(
                    "Grissini",
                    "Breadsticks"),
                new[]
                {
                    new CarbGuidePortion(15, 10),
                    new CarbGuidePortion(25, 17),
                    new CarbGuidePortion(40, 27),
                }),
            new(
                "cracker",
                CarbGuideCategoryId.BreadAndCereals,
                80,
                new(
                    "Cracker",
                    "Crackers"),
                new[]
                {
                    new CarbGuidePortion(15, 12),
                    new CarbGuidePortion(30, 24),
                    new CarbGuidePortion(45, 36),
                }),
            new(
                "polenta",
                CarbGuideCategoryId.BreadAndCereals,
                90,
                new(
                    "Polenta",
                    "Polenta"),
                new[]
                {
                    new CarbGuidePortion(150, 30),
                }),
        ];
    }
}
