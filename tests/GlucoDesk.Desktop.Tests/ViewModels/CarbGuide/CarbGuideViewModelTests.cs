using GlucoDesk.Desktop.Features.CarbGuide.Models;
using GlucoDesk.Desktop.Tests.Localization;
using GlucoDesk.Desktop.ViewModels.CarbGuide;

namespace GlucoDesk.Desktop.Tests.ViewModels.CarbGuide;

[Collection(LocalizationStateCollection.Name)]
public sealed class CarbGuideViewModelTests :
    EnglishLocalizationTestBase
{
    [Fact]
    public void Constructor_ShouldExposeAllSeededFoods()
    {
        using var viewModel = new CarbGuideViewModel();

        Assert.Equal(82, viewModel.Foods.Count);
        Assert.True(viewModel.HasVisibleFoods);
        Assert.Equal(12, viewModel.Categories.Count);
    }

    [Fact]
    public void Constructor_ShouldExposePackagedFoodImages()
    {
        using var viewModel = new CarbGuideViewModel();

        Assert.All(
            viewModel.Foods,
            food =>
            {
                Assert.StartsWith(
                    "avares://GlucoDesk.Desktop/Assets/CarbGuide/",
                    food.ImageAssetPath,
                    StringComparison.Ordinal);

                Assert.EndsWith(
                    $"{food.Id}.png",
                    food.ImageAssetPath,
                    StringComparison.Ordinal);
            });
    }

    [Fact]
    public void Constructor_ShouldExposeNewFoodImages()
    {
        using var viewModel = new CarbGuideViewModel();

        var expectedIds = new[]
        {
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
            var food =
                viewModel.Foods.Single(
                    x => x.Id == expectedId);

            Assert.Equal(
                $"avares://GlucoDesk.Desktop/Assets/CarbGuide/{expectedId}.png",
                food.ImageAssetPath);
        }
    }

    [Fact]
    public void SearchText_ShouldFilterUsingEnglishName()
    {
        using var viewModel = new CarbGuideViewModel();

        viewModel.SearchText = "spaghetti";

        var food = Assert.Single(viewModel.Foods);

        Assert.Equal("spaghetti", food.Id);
    }

    [Fact]
    public void SearchText_ShouldAlsoFilterUsingItalianName()
    {
        using var viewModel = new CarbGuideViewModel();

        viewModel.SearchText = "melone";

        Assert.Equal(2, viewModel.Foods.Count);

        Assert.Contains(
            viewModel.Foods,
            food => food.Id == "prosciutto-melone");

        Assert.Contains(
            viewModel.Foods,
            food => food.Id == "melone");
    }

    [Fact]
    public void SearchText_ShouldFilterNewFoodUsingEnglishName()
    {
        using var viewModel = new CarbGuideViewModel();

        viewModel.SearchText = "frozen peas";

        var food = Assert.Single(viewModel.Foods);

        Assert.Equal("piselli-surgelati", food.Id);
    }

    [Fact]
    public void SearchText_ShouldFilterNewFoodUsingItalianName()
    {
        using var viewModel = new CarbGuideViewModel();

        viewModel.SearchText = "integrale";

        Assert.Equal(2, viewModel.Foods.Count);

        Assert.Contains(
            viewModel.Foods,
            food => food.Id == "pane-integrale");

        Assert.Contains(
            viewModel.Foods,
            food => food.Id == "biscotti-integrali");
    }

    [Fact]
    public void SelectCategory_ShouldFilterByCategory()
    {
        using var viewModel = new CarbGuideViewModel();

        var category = viewModel.Categories.Single(
            x => x.CategoryId ==
                 CarbGuideCategoryId.Appetizers);

        viewModel.SelectCategoryCommand.Execute(category);

        Assert.Equal(3, viewModel.Foods.Count);

        Assert.All(
            viewModel.Foods,
            x => Assert.Equal(
                CarbGuideCategoryId.Appetizers,
                x.CategoryId));
    }

    [Fact]
    public void SelectVegetablesCategory_ShouldExposeOnlyVegetables()
    {
        using var viewModel = new CarbGuideViewModel();

        var category = viewModel.Categories.Single(
            x => x.CategoryId ==
                 CarbGuideCategoryId.Vegetables);

        viewModel.SelectCategoryCommand.Execute(category);

        Assert.Equal(5, viewModel.Foods.Count);

        Assert.All(
            viewModel.Foods,
            x => Assert.Equal(
                CarbGuideCategoryId.Vegetables,
                x.CategoryId));
    }

    [Fact]
    public void SelectBreadAndCerealsCategory_ShouldExposeOnlyBreadAndCereals()
    {
        using var viewModel = new CarbGuideViewModel();

        var category = viewModel.Categories.Single(
            x => x.CategoryId ==
                 CarbGuideCategoryId.BreadAndCereals);

        viewModel.SelectCategoryCommand.Execute(category);

        Assert.Equal(9, viewModel.Foods.Count);

        Assert.All(
            viewModel.Foods,
            x => Assert.Equal(
                CarbGuideCategoryId.BreadAndCereals,
                x.CategoryId));
    }

    [Fact]
    public void SearchText_ShouldExposeEmptyState()
    {
        using var viewModel = new CarbGuideViewModel();

        viewModel.SearchText = "food-that-does-not-exist";

        Assert.Empty(viewModel.Foods);
        Assert.False(viewModel.HasVisibleFoods);
    }
}
