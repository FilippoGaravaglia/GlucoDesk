
namespace GlucoDesk.Desktop.Features.CarbGuide.Models;

/// <summary>
/// Represents a carbohydrate-guide food item with one or more visible portions.
/// </summary>
public sealed record CarbGuideFoodItem(
    string Id,
    CarbGuideCategoryId CategoryId,
    int DisplayOrder,
    LocalizedText Name,
    IReadOnlyList<CarbGuidePortion> Portions,
    LocalizedText? PortionReferenceNote = null);
