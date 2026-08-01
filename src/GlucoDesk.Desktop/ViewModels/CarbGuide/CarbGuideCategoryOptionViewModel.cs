using CommunityToolkit.Mvvm.ComponentModel;
using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.ViewModels.CarbGuide;

/// <summary>
/// Represents a selectable carbohydrate-guide category.
/// </summary>
public sealed partial class CarbGuideCategoryOptionViewModel :
    ObservableObject
{
    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Initializes a new category option.
    /// </summary>
    public CarbGuideCategoryOptionViewModel(
        CarbGuideCategoryId? categoryId,
        string displayName)
    {
        CategoryId = categoryId;
        _displayName = displayName;
    }

    /// <summary>
    /// Gets the category identifier, or null for the all-foods option.
    /// </summary>
    public CarbGuideCategoryId? CategoryId { get; }
}
