namespace GlucoDesk.Desktop.Features.CarbGuide.Models;

/// <summary>
/// Represents a carbohydrate-guide category.
/// </summary>
public sealed record CarbGuideCategory(
    CarbGuideCategoryId Id,
    int DisplayOrder,
    LocalizedText Name);
