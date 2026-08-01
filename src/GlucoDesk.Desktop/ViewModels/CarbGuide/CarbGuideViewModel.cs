using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.Converters;
using GlucoDesk.Desktop.Features.CarbGuide.Data;
using GlucoDesk.Desktop.Features.CarbGuide.Models;
using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.ViewModels.Common;

namespace GlucoDesk.Desktop.ViewModels.CarbGuide;

/// <summary>
/// Provides the presentation state for the multilingual carbohydrate guide.
/// </summary>
public sealed partial class CarbGuideViewModel :
    ViewModelBase,
    IDisposable
{
    private const string AssetBaseUri =
        "avares://GlucoDesk.Desktop/Assets/CarbGuide";

    private readonly IReadOnlyList<CarbGuideFoodItem> _sourceFoods;
    private readonly IReadOnlyList<CarbGuideCategory> _sourceCategories;
    private readonly CancellationTokenSource
        _imagePreloadCancellationTokenSource = new();

    private bool _isDisposed;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private CarbGuideCategoryOptionViewModel? _selectedCategory;

    [ObservableProperty]
    private bool _hasVisibleFoods;

    /// <summary>
    /// Initializes a new carbohydrate guide view model.
    /// </summary>
    public CarbGuideViewModel()
    {
        _sourceFoods = CarbGuideCatalogSeed.GetFoods();
        _sourceCategories = CarbGuideCatalogSeed.GetCategories();

        Categories = new ObservableCollection<
            CarbGuideCategoryOptionViewModel>();

        Foods = new ObservableCollection<
            CarbGuideFoodCardViewModel>();

        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RebuildLocalizedContent();

        _ = PreloadImagesAfterStartupAsync(
            _imagePreloadCancellationTokenSource.Token);
    }

    /// <summary>
    /// Gets the category filters.
    /// </summary>
    public ObservableCollection<CarbGuideCategoryOptionViewModel>
        Categories { get; }

    /// <summary>
    /// Gets the currently visible foods.
    /// </summary>
    public ObservableCollection<CarbGuideFoodCardViewModel>
        Foods { get; }

    /// <summary>
    /// Gets the number of currently visible foods.
    /// </summary>
    public int VisibleFoodCount => Foods.Count;

    /// <summary>
    /// Gets the localized result counter.
    /// </summary>
    public string ResultCountText =>
        string.Format(
            T("CarbGuideResultCountFormat"),
            VisibleFoodCount);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        _imagePreloadCancellationTokenSource.Cancel();
        _imagePreloadCancellationTokenSource.Dispose();

        _isDisposed = true;
    }

    /// <summary>
    /// Selects a category filter.
    /// </summary>
    [RelayCommand]
    private void SelectCategory(
        CarbGuideCategoryOptionViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        SelectedCategory = category;

        foreach (var option in Categories)
        {
            option.IsSelected = ReferenceEquals(option, category);
        }

        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = value;
        ApplyFilters();
    }

    private void RebuildLocalizedContent()
    {
        var selectedCategoryId =
            SelectedCategory?.CategoryId;

        Categories.Clear();

        Categories.Add(
            new CarbGuideCategoryOptionViewModel(
                null,
                T("CarbGuideAllCategories")));

        foreach (var category in _sourceCategories)
        {
            Categories.Add(
                new CarbGuideCategoryOptionViewModel(
                    category.Id,
                    category.Name.Get(LanguageCode)));
        }

        SelectedCategory =
            Categories.FirstOrDefault(
                x => x.CategoryId == selectedCategoryId)
            ?? Categories[0];

        foreach (var option in Categories)
        {
            option.IsSelected =
                ReferenceEquals(option, SelectedCategory);
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        Foods.Clear();

        var normalizedSearch =
            SearchText.Trim().ToUpperInvariant();

        var categoryNames = _sourceCategories.ToDictionary(
            x => x.Id,
            x => x.Name.Get(LanguageCode));

        foreach (var source in _sourceFoods)
        {
            if (SelectedCategory?.CategoryId is { } categoryId &&
                source.CategoryId != categoryId)
            {
                continue;
            }

            var card = new CarbGuideFoodCardViewModel(
                source,
                LanguageCode,
                categoryNames[source.CategoryId],
                T("CarbGuidePortion"),
                T("CarbGuideCarbohydratesShort"));

            if (normalizedSearch.Length > 0 &&
                !card.SearchText.Contains(
                    normalizedSearch,
                    StringComparison.Ordinal))
            {
                continue;
            }

            Foods.Add(card);
        }

        HasVisibleFoods = Foods.Count > 0;

        OnPropertyChanged(nameof(VisibleFoodCount));
        OnPropertyChanged(nameof(ResultCountText));
    }

    private async Task PreloadImagesAfterStartupAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            // Let the main window and dashboard finish their initial render
            // before using background resources for the optional guide.
            await Task.Delay(
                    TimeSpan.FromMilliseconds(1500),
                    cancellationToken)
                .ConfigureAwait(false);

            var assetPaths = _sourceFoods.Select(
                food =>
                    $"{AssetBaseUri}/{food.Id}.png");

            await AvaloniaAssetUriToBitmapConverter
                .PreloadAsync(
                    assetPaths,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // The application or owning view model is shutting down.
        }
        catch
        {
            // Cache warm-up is an optional performance optimization.
            // A failed preload must never affect the application.
        }
    }

    private void OnLanguageChanged(
        object? sender,
        EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        RebuildLocalizedContent();
    }

    private static string LanguageCode =>
        T("CarbGuideLanguageCode");

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }
}
