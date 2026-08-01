using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.Tests.Features.CarbGuide.Data;

public sealed class CarbGuideCatalogSeedTests
{
    [Fact]
    public void GetCategories_ShouldReturnAllSupportedCategories()
    {
        var categories =
            CarbGuideCatalogSeed.GetCategories();

        Assert.Equal(11, categories.Count);

        Assert.Contains(
            categories,
            x => x.Id == CarbGuideCategoryId.Appetizers &&
                 x.Name.It == "Antipasti" &&
                 x.Name.En == "Appetizers");

        Assert.Contains(
            categories,
            x => x.Id == CarbGuideCategoryId.FirstCourses &&
                 x.Name.It == "Primi piatti" &&
                 x.Name.En == "First courses");

        Assert.Contains(
            categories,
            x => x.Id == CarbGuideCategoryId.Legumes &&
                 x.Name.It == "Legumi" &&
                 x.Name.En == "Legumes");

        Assert.Contains(
            categories,
            x => x.Id == CarbGuideCategoryId.Vegetables &&
                 x.Name.It == "Verdure" &&
                 x.Name.En == "Vegetables");

        Assert.Contains(
            categories,
            x => x.Id == CarbGuideCategoryId.BreadAndCereals &&
                 x.Name.It == "Pane e cereali" &&
                 x.Name.En == "Bread and cereals");
    }

    [Fact]
    public void GetFoods_ShouldReturnCurrentBatchFoods()
    {
        var foods =
            CarbGuideCatalogSeed.GetFoods();

        Assert.Equal(82, foods.Count);

        var expectedIds = new[]
        {
            "prosciutto-melone",
            "affettato-misto",
            "crostini",
            "riso-parboiled",
            "spaghetti",
            "tortelloni-ricotta-spinaci",
            "gnocchi-pomodoro",
            "piselli-surgelati",
            "insalata",
            "pomodori",
            "carote",
            "patate-lesse",
            "patate-fritte",
            "pane-comune",
            "pane-integrale",
            "panini-al-latte",
            "fette-biscottate",
            "cornflakes",
            "muesli",
            "grissini",
            "cracker",
            "polenta",
            "toast",
        };

        foreach (var expectedId in expectedIds)
        {
            Assert.Contains(
                foods,
                food => food.Id == expectedId);
        }

        Assert.Equal(
            foods.Count,
            foods.Select(food => food.Id).Distinct().Count());
    }

    [Fact]
    public void GetFoods_ShouldMapProsciuttoAndMelonCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(x => x.Id == "prosciutto-melone");

        Assert.Equal(
            CarbGuideCategoryId.Appetizers,
            item.CategoryId);

        Assert.Equal(
            "Prosciutto e melone",
            item.Name.It);

        Assert.Equal(
            "Prosciutto and melon",
            item.Name.En);

        var portion = Assert.Single(item.Portions);

        Assert.Equal(240, portion.WeightGrams);
        Assert.Equal(15m, portion.CarbohydratesGrams);
        Assert.Null(item.PortionReferenceNote);
    }

    [Fact]
    public void GetFoods_ShouldMapFirstCourseReferenceNote()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(x => x.Id == "spaghetti");

        Assert.Equal(
            CarbGuideCategoryId.FirstCourses,
            item.CategoryId);

        Assert.NotNull(item.PortionReferenceNote);

        Assert.Equal(
            "Valori riferiti all’alimento crudo e non condito.",
            item.PortionReferenceNote!.It);

        Assert.Equal(
            "Values refer to the uncooked and unseasoned food.",
            item.PortionReferenceNote.En);
    }

    [Fact]
    public void GetFoods_ShouldMapGnocchiPortionsCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(x => x.Id == "gnocchi-pomodoro");

        Assert.Equal(3, item.Portions.Count);

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(30m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(150, second.WeightGrams);
                Assert.Equal(45m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(250, third.WeightGrams);
                Assert.Equal(75m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapFrozenPeasCorrectly()
    {
        var item =
            CarbGuideCatalogSeed.GetFoods()
                .Single(x => x.Id == "piselli-surgelati");

        Assert.Equal(
            CarbGuideCategoryId.Legumes,
            item.CategoryId);

        Assert.Equal("Piselli surgelati", item.Name.It);
        Assert.Equal("Frozen peas", item.Name.En);

        Assert.Collection(
            item.Portions,
            first =>
            {
                Assert.Equal(100, first.WeightGrams);
                Assert.Equal(13m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(150, second.WeightGrams);
                Assert.Equal(19m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(200, third.WeightGrams);
                Assert.Equal(26m, third.CarbohydratesGrams);
            });
    }

    [Fact]
    public void GetFoods_ShouldMapVegetableFoodsCorrectly()
    {
        var foods =
            CarbGuideCatalogSeed.GetFoods()
                .Where(x =>
                    x.CategoryId ==
                    CarbGuideCategoryId.Vegetables)
                .ToArray();

        Assert.Equal(5, foods.Length);

        Assert.Contains(foods, x => x.Id == "insalata");
        Assert.Contains(foods, x => x.Id == "pomodori");
        Assert.Contains(foods, x => x.Id == "carote");
        Assert.Contains(foods, x => x.Id == "patate-lesse");
        Assert.Contains(foods, x => x.Id == "patate-fritte");
    }

    [Fact]
    public void GetFoods_ShouldMapBreadAndCerealFoodsCorrectly()
    {
        var foods =
            CarbGuideCatalogSeed.GetFoods()
                .Where(x =>
                    x.CategoryId ==
                    CarbGuideCategoryId.BreadAndCereals)
                .ToArray();

        Assert.Equal(9, foods.Length);

        var commonBread =
            foods.Single(x => x.Id == "pane-comune");

        Assert.Collection(
            commonBread.Portions,
            first =>
            {
                Assert.Equal(40, first.WeightGrams);
                Assert.Equal(27m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(60, second.WeightGrams);
                Assert.Equal(40m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(100, third.WeightGrams);
                Assert.Equal(67m, third.CarbohydratesGrams);
            });

        var wholemealBread =
            foods.Single(x => x.Id == "pane-integrale");

        Assert.Collection(
            wholemealBread.Portions,
            first =>
            {
                Assert.Equal(30, first.WeightGrams);
                Assert.Equal(14m, first.CarbohydratesGrams);
            },
            second =>
            {
                Assert.Equal(60, second.WeightGrams);
                Assert.Equal(29m, second.CarbohydratesGrams);
            },
            third =>
            {
                Assert.Equal(90, third.WeightGrams);
                Assert.Equal(43m, third.CarbohydratesGrams);
            });
    }
}
