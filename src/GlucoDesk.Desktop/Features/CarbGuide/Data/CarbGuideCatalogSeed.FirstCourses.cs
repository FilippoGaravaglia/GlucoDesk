using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Seed foods for first courses.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static partial IEnumerable<CarbGuideFoodItem> GetFirstCourseFoods()
    {
        return
        [
            new(
                "riso-parboiled",
                CarbGuideCategoryId.FirstCourses,
                10,
                new(
                    "Riso parboiled",
                    "Parboiled rice"),
                new[]
                {
                    new CarbGuidePortion(60, 49),
                    new CarbGuidePortion(80, 65),
                    new CarbGuidePortion(100, 81),
                },
                RawAndUnseasonedReferenceNote),
            new(
                "spaghetti",
                CarbGuideCategoryId.FirstCourses,
                20,
                new(
                    "Spaghetti",
                    "Spaghetti"),
                new[]
                {
                    new CarbGuidePortion(60, 47),
                    new CarbGuidePortion(80, 63),
                    new CarbGuidePortion(120, 95),
                },
                RawAndUnseasonedReferenceNote),
            new(
                "tortelloni-ricotta-spinaci",
                CarbGuideCategoryId.FirstCourses,
                30,
                new(
                    "Tortelloni ricotta e spinaci",
                    "Ricotta and spinach tortelloni"),
                new[]
                {
                    new CarbGuidePortion(100, 36),
                    new CarbGuidePortion(130, 47),
                    new CarbGuidePortion(200, 72),
                },
                RawAndUnseasonedReferenceNote),
            new(
                "gnocchi-pomodoro",
                CarbGuideCategoryId.FirstCourses,
                40,
                new(
                    "Gnocchi al pomodoro",
                    "Tomato gnocchi"),
                new[]
                {
                    new CarbGuidePortion(100, 30),
                    new CarbGuidePortion(150, 45),
                    new CarbGuidePortion(250, 75),
                },
                RawAndUnseasonedReferenceNote),
            new(
                "tortellini-brodo",
                CarbGuideCategoryId.FirstCourses,
                50,
                new(
                    "Tortellini in brodo",
                    "Tortellini in broth"),
                new[]
                {
                    new CarbGuidePortion(30, 15),
                    new CarbGuidePortion(60, 30),
                    new CarbGuidePortion(100, 50),
                },
                RawAndUnseasonedReferenceNote),
            new(
                "insalata-riso",
                CarbGuideCategoryId.FirstCourses,
                60,
                new(
                    "Insalata di riso",
                    "Rice salad"),
                new[]
                {
                    new CarbGuidePortion(90, 18),
                    new CarbGuidePortion(180, 36),
                    new CarbGuidePortion(360, 75),
                },
                RawAndUnseasonedReferenceNote),
        ];
    }
}
