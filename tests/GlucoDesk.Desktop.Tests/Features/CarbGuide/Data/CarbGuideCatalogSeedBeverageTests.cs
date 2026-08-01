using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Tests.Features.CarbGuide.Data;

public sealed class CarbGuideCatalogSeedBeverageTests
{
    [Fact]
    public void GetFoods_ShouldReturnAllBeverages()
    {
        var beverages =
            CarbGuideCatalogSeed.GetFoods()
                .Where(
                    food =>
                        food.CategoryId ==
                        CarbGuideCategoryId.Beverages)
                .ToArray();

        Assert.Equal(9, beverages.Length);

        var expectedIds = new[]
        {
            "latte-parzialmente-scremato",
            "succo-frutta-zuccherato",
            "succo-frutta-non-zuccherato",
            "spremuta-arancia",
            "te-non-zuccherato",
            "caffe-non-zuccherato",
            "cappuccino-non-zuccherato",
            "vino",
            "birra",
        };

        Assert.Equal(
            expectedIds,
            beverages.Select(food => food.Id));
    }

    [Fact]
    public void GetFoods_ShouldMapSemiSkimmedMilkCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(
                    food =>
                        food.Id ==
                        "latte-parzialmente-scremato");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(5m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(200, second.WeightGrams);
                Assert.Equal(10m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(300, third.WeightGrams);
                Assert.Equal(15m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapSweetenedFruitJuiceCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(
                    food =>
                        food.Id ==
                        "succo-frutta-zuccherato");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(14m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(150, second.WeightGrams);
                Assert.Equal(22m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(200, third.WeightGrams);
                Assert.Equal(29m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapZeroCarbohydrateBeveragesCorrectly()
    {
        var foods =
            CarbGuideCatalogSeed.GetFoods();

        var tea =
            foods.Single(
                food => food.Id == "te-non-zuccherato");

        var coffee =
            foods.Single(
                food => food.Id == "caffe-non-zuccherato");

        var wine =
            foods.Single(
                food => food.Id == "vino");

        Assert.All(
            tea.Portions,
            portion =>
                Assert.Equal(
                    0m,
                    portion.CarbohydratesGrams));

        Assert.All(
            coffee.Portions,
            portion =>
                Assert.Equal(
                    0m,
                    portion.CarbohydratesGrams));

        Assert.All(
            wine.Portions,
            portion =>
                Assert.Equal(
                    0m,
                    portion.CarbohydratesGrams));
    }

    [Fact]
    public void GetFoods_ShouldMapBeerCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "birra");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(200, first.WeightGrams);
                Assert.Equal(7m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(330, second.WeightGrams);
                Assert.Equal(11m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(400, third.WeightGrams);
                Assert.Equal(14m, third.CarbohydratesGrams);
            });
    }
}
