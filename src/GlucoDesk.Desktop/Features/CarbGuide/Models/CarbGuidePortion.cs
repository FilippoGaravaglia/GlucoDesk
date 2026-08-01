namespace GlucoDesk.Desktop.Features.CarbGuide.Models;

/// <summary>
/// Represents one visible portion option for a food.
/// </summary>
public sealed record CarbGuidePortion(
    int WeightGrams,
    decimal CarbohydratesGrams);
