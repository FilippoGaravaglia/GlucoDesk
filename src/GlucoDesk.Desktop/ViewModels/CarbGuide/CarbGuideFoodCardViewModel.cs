using GlucoDesk.Desktop.Features.CarbGuide.Models;

namespace GlucoDesk.Desktop.ViewModels.CarbGuide;

/// <summary>
/// Represents one localized food card displayed by the carbohydrate guide.
/// </summary>
public sealed class CarbGuideFoodCardViewModel
{
    private const string AssetBaseUri =
        "avares://GlucoDesk.Desktop/Assets/CarbGuide";

    /// <summary>
    /// Initializes a food card.
    /// </summary>
    public CarbGuideFoodCardViewModel(
        CarbGuideFoodItem source,
        string languageCode,
        string categoryName,
        string portionLabel,
        string carbohydratesLabel)
    {
        ArgumentNullException.ThrowIfNull(source);

        Id = source.Id;
        CategoryId = source.CategoryId;
        Name = source.Name.Get(languageCode);
        SearchText =
            $"{source.Name.It} {source.Name.En}"
                .ToUpperInvariant();

        CategoryName = categoryName;
        PortionLabel = portionLabel;
        CarbohydratesLabel = carbohydratesLabel;

        ReferenceNote =
            source.PortionReferenceNote?.Get(languageCode);

        Portions = source.Portions
            .Select(
                portion =>
                    new CarbGuidePortionViewModel(
                        portion.WeightGrams,
                        portion.CarbohydratesGrams))
            .ToArray();

        ImageAssetPath =
            $"{AssetBaseUri}/{source.Id}.png";

        VisualSymbol = ResolveVisualSymbol(source.Id);
    }

    /// <summary>
    /// Gets the stable item identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the source category.
    /// </summary>
    public CarbGuideCategoryId CategoryId { get; }

    /// <summary>
    /// Gets the localized food name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets text used by bilingual search.
    /// </summary>
    public string SearchText { get; }

    /// <summary>
    /// Gets the localized category name.
    /// </summary>
    public string CategoryName { get; }

    /// <summary>
    /// Gets the localized portion label.
    /// </summary>
    public string PortionLabel { get; }

    /// <summary>
    /// Gets the localized carbohydrate label.
    /// </summary>
    public string CarbohydratesLabel { get; }

    /// <summary>
    /// Gets the packaged Avalonia food illustration URI.
    /// </summary>
    public string ImageAssetPath { get; }

    /// <summary>
    /// Gets the optional reference note.
    /// </summary>
    public string? ReferenceNote { get; }

    /// <summary>
    /// Gets whether the reference note should be displayed.
    /// </summary>
    public bool HasReferenceNote =>
        !string.IsNullOrWhiteSpace(ReferenceNote);

    /// <summary>
    /// Gets the visible portion references.
    /// </summary>
    public IReadOnlyList<CarbGuidePortionViewModel>
        Portions { get; }

    /// <summary>
    /// Gets the fallback visual symbol.
    /// </summary>
    public string VisualSymbol { get; }

    private static string ResolveVisualSymbol(string id)
    {
        return id switch
        {
            "prosciutto-melone" => "M",
            "affettato-misto" => "A",
            "crostini" => "C",
            "riso-parboiled" => "R",
            "spaghetti" => "S",
            "tortelloni-ricotta-spinaci" => "T",
            "gnocchi-pomodoro" => "G",
            _ => "•",
        };
    }
}
