using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for main courses.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetMainCourseFoods()
    {
        return
        [
            new(
                "arrosto-bovino-magro",
                CarbGuideCategoryId.MainCourses,
                10,
                new(
                    "Arrosto di bovino adulto magro",
                    "Lean roast beef"),
                new[]
                {
                    new CarbGuidePortion(100, 0),
                    new CarbGuidePortion(150, 0),
                    new CarbGuidePortion(200, 0),
                }),
            new(
                "roast-beef",
                CarbGuideCategoryId.MainCourses,
                20,
                new(
                    "Roast beef",
                    "Roast beef"),
                new[]
                {
                    new CarbGuidePortion(100, 0),
                    new CarbGuidePortion(150, 0),
                    new CarbGuidePortion(200, 0),
                }),
            new(
                "hamburger",
                CarbGuideCategoryId.MainCourses,
                30,
                new(
                    "Hamburger",
                    "Hamburger"),
                new[]
                {
                    new CarbGuidePortion(115, 29),
                }),
            new(
                "prosciutto-crudo",
                CarbGuideCategoryId.MainCourses,
                40,
                new(
                    "Prosciutto crudo",
                    "Prosciutto crudo"),
                new[]
                {
                    new CarbGuidePortion(40, 0),
                    new CarbGuidePortion(60, 0),
                    new CarbGuidePortion(80, 0),
                }),
            new(
                "orata",
                CarbGuideCategoryId.MainCourses,
                50,
                new(
                    "Orata",
                    "Sea bream"),
                new[]
                {
                    new CarbGuidePortion(350, 4),
                }),
            new(
                "bastoncini-pesce",
                CarbGuideCategoryId.MainCourses,
                60,
                new(
                    "Bastoncini di pesce",
                    "Fish sticks"),
                new[]
                {
                    new CarbGuidePortion(50, 8),
                    new CarbGuidePortion(100, 15),
                    new CarbGuidePortion(150, 23),
                }),
            new(
                "tonno-sottolio-sgocciolato",
                CarbGuideCategoryId.MainCourses,
                70,
                new(
                    "Tonno sott’olio (sgocciolato)",
                    "Tuna in oil (drained)"),
                new[]
                {
                    new CarbGuidePortion(80, 0),
                }),
            new(
                "emmenthal",
                CarbGuideCategoryId.MainCourses,
                80,
                new(
                    "Emmenthal",
                    "Emmental"),
                new[]
                {
                    new CarbGuidePortion(50, 2),
                    new CarbGuidePortion(80, 3),
                    new CarbGuidePortion(120, 4),
                }),
        ];
    }
}
