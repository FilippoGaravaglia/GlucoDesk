namespace GlucoDesk.Desktop.ViewModels.CarbGuide;

/// <summary>
/// Represents a visible portion reference in a food card.
/// </summary>
public sealed record CarbGuidePortionViewModel(
    int WeightGrams,
    decimal CarbohydratesGrams)
{
    /// <summary>
    /// Gets the formatted weight.
    /// </summary>
    public string WeightText => $"{WeightGrams} g";

    /// <summary>
    /// Gets the formatted carbohydrate value.
    /// </summary>
    public string CarbohydratesText =>
        $"{CarbohydratesGrams:0.##} g";

    /// <summary>
    /// Gets a normalized visual scale for the portion indicator.
    /// </summary>
    public double VisualScale =>
        Math.Clamp(
            0.62 + (WeightGrams / 450d),
            0.68,
            1.15);
}
