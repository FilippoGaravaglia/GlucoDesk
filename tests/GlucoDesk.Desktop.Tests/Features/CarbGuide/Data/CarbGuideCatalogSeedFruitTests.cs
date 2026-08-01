using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Tests.Features.CarbGuide.Data;

public sealed class CarbGuideCatalogSeedFruitTests
{
    [Fact]
    public void GetFoods_ShouldReturnAllFruitFoods()
    {
        var fruits =
            CarbGuideCatalogSeed.GetFoods()
                .Where(
                    food =>
                        food.CategoryId ==
                        CarbGuideCategoryId.Fruit)
                .ToArray();

        Assert.Equal(9, fruits.Length);

        var expectedIds = new[]
        {
            "ananas-fresco",
            "pesche",
            "kiwi",
            "melone",
            "banana",
            "mela",
            "fragole",
            "fichi",
            "macedonia",
        };

        Assert.Equal(
            expectedIds,
            fruits.Select(food => food.Id));
    }

    [Fact]
    public void GetFoods_ShouldMapFreshPineapplePortionsCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "ananas-fresco");

        Assert.Equal(
            CarbGuideCategoryId.Fruit,
            item.CategoryId);

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(10m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(150, second.WeightGrams);
                Assert.Equal(15m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(200, third.WeightGrams);
                Assert.Equal(20m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapBananaPortionsCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "banana");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(15m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(130, second.WeightGrams);
                Assert.Equal(20m, second.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapFigPortionsCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(food => food.Id == "fichi");

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(60, first.WeightGrams);
                Assert.Equal(7m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(100, second.WeightGrams);
                Assert.Equal(11m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(150, third.WeightGrams);
                Assert.Equal(17m, third.CarbohydratesGrams);
            });
    }
}
