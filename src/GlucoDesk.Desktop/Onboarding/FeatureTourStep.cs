namespace GlucoDesk.Desktop.Onboarding;

/// <summary>
/// Describes one localized page of the first-run GlucoDesk feature tour.
/// </summary>
public sealed record FeatureTourStep(
    string Number,
    string EyebrowKey,
    string TitleKey,
    string DescriptionKey,
    string FirstHighlightKey,
    string SecondHighlightKey,
    string ThirdHighlightKey,
    string VisualKind);
