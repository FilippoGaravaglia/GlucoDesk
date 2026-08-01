using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Features.CarbGuide.Data;

/// <summary>
/// Provides the multilingual carbohydrate-guide seed catalog.
/// </summary>
public static partial class CarbGuideCatalogSeed
{
    private static readonly LocalizedText RawAndUnseasonedReferenceNote =
        new(
            "Valori riferiti all’alimento crudo e non condito.",
            "Values refer to the uncooked and unseasoned food.");

    /// <summary>
    /// Gets all supported categories.
    /// </summary>
    public static IReadOnlyList<CarbGuideCategory> GetCategories()
    {
        return
        [
            new(
                CarbGuideCategoryId.Appetizers,
                0,
                new("Antipasti", "Appetizers")),
            new(
                CarbGuideCategoryId.FirstCourses,
                1,
                new("Primi piatti", "First courses")),
            new(
                CarbGuideCategoryId.MainCourses,
                2,
                new("Secondi piatti", "Main courses")),
            new(
                CarbGuideCategoryId.Legumes,
                3,
                new("Legumi", "Legumes")),
            new(
                CarbGuideCategoryId.Vegetables,
                4,
                new("Verdure", "Vegetables")),
            new(
                CarbGuideCategoryId.BreadAndCereals,
                5,
                new("Pane e cereali", "Bread and cereals")),
            new(
                CarbGuideCategoryId.Snacks,
                6,
                new("Snack", "Snacks")),
            new(
                CarbGuideCategoryId.Fruit,
                7,
                new("Frutta", "Fruit")),
            new(
                CarbGuideCategoryId.Desserts,
                8,
                new("Dolci", "Desserts")),
            new(
                CarbGuideCategoryId.Beverages,
                9,
                new("Bevande", "Beverages")),
            new(
                CarbGuideCategoryId.Various,
                10,
                new("Varie", "Other")),
        ];
    }

    /// <summary>
    /// Gets all currently seeded foods across all categories.
    /// </summary>
    public static IReadOnlyList<CarbGuideFoodItem> GetFoods()
    {
        var foods = new List<CarbGuideFoodItem>();

        foods.AddRange(GetAppetizerFoods());
        foods.AddRange(GetFirstCourseFoods());
        foods.AddRange(GetMainCourseFoods());
        foods.AddRange(GetLegumeFoods());
        foods.AddRange(GetVegetableFoods());
        foods.AddRange(GetBreadAndCerealFoods());
        foods.AddRange(GetSnackFoods());
        foods.AddRange(GetFruitFoods());
        foods.AddRange(GetDessertFoods());
        foods.AddRange(GetBeverageFoods());
        foods.AddRange(GetVariousFoods());

        var categoryOrder = GetCategories()
            .ToDictionary(
                category => category.Id,
                category => category.DisplayOrder);

        return foods
            .OrderBy(food => categoryOrder[food.CategoryId])
            .ThenBy(food => food.DisplayOrder)
            .ThenBy(food => food.Name.It)
            .ToArray();
    }

    private static partial IEnumerable<CarbGuideFoodItem> GetAppetizerFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetFirstCourseFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetMainCourseFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetLegumeFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetVegetableFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetBreadAndCerealFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetSnackFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetFruitFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetDessertFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetBeverageFoods();

    private static partial IEnumerable<CarbGuideFoodItem> GetVariousFoods();
}
