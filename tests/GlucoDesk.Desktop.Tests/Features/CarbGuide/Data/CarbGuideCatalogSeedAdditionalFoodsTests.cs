using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Tests.Features.CarbGuide.Data;

/// <summary>
/// Regression tests for newly added carbohydrate-guide foods.
/// </summary>
public sealed class CarbGuideCatalogSeedAdditionalFoodsTests
{
    [Fact]
    public void GetCategories_ShouldContainLegumesInExpectedDisplayPosition()
    {
        var categories = CarbGuideCatalogSeed.GetCategories();

        var legumes = categories.Single(x => x.Id == CarbGuideCategoryId.Legumes);

        Assert.Equal(3, legumes.DisplayOrder);
        Assert.Equal("Legumi", legumes.Name.It);
        Assert.Equal("Legumes", legumes.Name.En);
    }

    [Fact]
    public void GetFoods_ShouldContainTortelliniInBroth_WithExpectedPortions()
    {
        var item = CarbGuideCatalogSeed.GetFoods()
            .Single(x => x.Id == "tortellini-brodo");

        Assert.Equal(CarbGuideCategoryId.FirstCourses, item.CategoryId);
        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(30, first.WeightGrams);
                Assert.Equal(15m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(60, second.WeightGrams);
                Assert.Equal(30m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(100, third.WeightGrams);
                Assert.Equal(50m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldContainRiceSalad_WithExpectedPortions()
    {
        var item = CarbGuideCatalogSeed.GetFoods()
            .Single(x => x.Id == "insalata-riso");

        Assert.Equal(CarbGuideCategoryId.FirstCourses, item.CategoryId);
        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(90, first.WeightGrams);
                Assert.Equal(18m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(180, second.WeightGrams);
                Assert.Equal(36m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(360, third.WeightGrams);
                Assert.Equal(75m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldContainFishSticks_WithExpectedPortions()
    {
        var item = CarbGuideCatalogSeed.GetFoods()
            .Single(x => x.Id == "bastoncini-pesce");

        Assert.Equal(CarbGuideCategoryId.MainCourses, item.CategoryId);
        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(50, first.WeightGrams);
                Assert.Equal(8m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(100, second.WeightGrams);
                Assert.Equal(15m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(150, third.WeightGrams);
                Assert.Equal(23m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldContainLegumes_WithExpectedValues()
    {
        var freshBeans = CarbGuideCatalogSeed.GetFoods()
            .Single(x => x.Id == "fagioli-freschi");

        var driedChickpeas = CarbGuideCatalogSeed.GetFoods()
            .Single(x => x.Id == "ceci-secchi");

        Assert.Equal(CarbGuideCategoryId.Legumes, freshBeans.CategoryId);
        Assert.Equal(CarbGuideCategoryId.Legumes, driedChickpeas.CategoryId);

        Assert.Collection(
            freshBeans.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(23m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(150, second.WeightGrams);
                Assert.Equal(34m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(200, third.WeightGrams);
                Assert.Equal(45m, third.CarbohydratesGrams);
            });

        Assert.Collection(
            driedChickpeas.Portions,
            first =>
            {
                Assert.Equal(25, first.WeightGrams);
                Assert.Equal(12m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(50, second.WeightGrams);
                Assert.Equal(23m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(75, third.WeightGrams);
                Assert.Equal(35m, third.CarbohydratesGrams);
            });
    }
}
