using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Tests.Features.CarbGuide.Data;

public sealed class CarbGuideCatalogSeedDessertTests
{
    [Fact]
    public void GetFoods_ShouldReturnAllDesserts()
    {
        var desserts =
            CarbGuideCatalogSeed.GetFoods()
                .Where(
                    food =>
                        food.CategoryId ==
                        CarbGuideCategoryId.Desserts)
                .ToArray();

        Assert.Equal(19, desserts.Length);

        var expectedIds = new[]
        {
            "gelato-al-tartufo",
            "creme-caramel",
            "crema-nocciole-cacao",
            "cioccolato-al-latte",
            "strudel-di-mele",
            "tiramisu",
            "torta-sacher",
            "torta-saint-honore",
            "crostata-marmellata",
            "biscotti-secchi",
            "biscotti-integrali",
            "merendina-farcita",
            "merendina-cioccolato",
            "crostata-crema-cacao",
            "cornetto",
            "cannolo-crema",
            "krapfen-crema",
            "marmellata-senza-zucchero",
            "marmellata",
        };

        Assert.Equal(
            expectedIds,
            desserts.Select(food => food.Id));
    }

    [Fact]
    public void GetFoods_ShouldMapSacherCakeCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "torta-sacher");

        Assert.Equal(
            CarbGuideCategoryId.Desserts,
            item.CategoryId);

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(50, first.WeightGrams);
                Assert.Equal(34m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(100, second.WeightGrams);
                Assert.Equal(68m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(150, third.WeightGrams);
                Assert.Equal(102m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapTiramisuCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "tiramisu");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(50, first.WeightGrams);
                Assert.Equal(26m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(100, second.WeightGrams);
                Assert.Equal(52m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(150, third.WeightGrams);
                Assert.Equal(78m, third.CarbohydratesGrams);
            });
    }
}
