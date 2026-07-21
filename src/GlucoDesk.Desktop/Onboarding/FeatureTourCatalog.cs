namespace GlucoDesk.Desktop.Onboarding;

/// <summary>
/// Provides the ordered first-run GlucoDesk product tour.
/// </summary>
public static class FeatureTourCatalog
{
    private static readonly IReadOnlyList<FeatureTourStep> Steps =
    [
        new(
            Number: "01",
            EyebrowKey: "FeatureTourWelcomeEyebrow",
            TitleKey: "FeatureTourWelcomeTitle",
            DescriptionKey: "FeatureTourWelcomeDescription",
            FirstHighlightKey: "FeatureTourWelcomeHighlightOne",
            SecondHighlightKey: "FeatureTourWelcomeHighlightTwo",
            ThirdHighlightKey: "FeatureTourWelcomeHighlightThree",
            VisualKind: "Welcome"),

        new(
            Number: "02",
            EyebrowKey: "FeatureTourDashboardEyebrow",
            TitleKey: "FeatureTourDashboardTitle",
            DescriptionKey: "FeatureTourDashboardDescription",
            FirstHighlightKey: "FeatureTourDashboardHighlightOne",
            SecondHighlightKey: "FeatureTourDashboardHighlightTwo",
            ThirdHighlightKey: "FeatureTourDashboardHighlightThree",
            VisualKind: "Dashboard"),

        new(
            Number: "03",
            EyebrowKey: "FeatureTourHistoryEyebrow",
            TitleKey: "FeatureTourHistoryTitle",
            DescriptionKey: "FeatureTourHistoryDescription",
            FirstHighlightKey: "FeatureTourHistoryHighlightOne",
            SecondHighlightKey: "FeatureTourHistoryHighlightTwo",
            ThirdHighlightKey: "FeatureTourHistoryHighlightThree",
            VisualKind: "History"),

        new(
            Number: "04",
            EyebrowKey: "FeatureTourDiaryEyebrow",
            TitleKey: "FeatureTourDiaryTitle",
            DescriptionKey: "FeatureTourDiaryDescription",
            FirstHighlightKey: "FeatureTourDiaryHighlightOne",
            SecondHighlightKey: "FeatureTourDiaryHighlightTwo",
            ThirdHighlightKey: "FeatureTourDiaryHighlightThree",
            VisualKind: "Diary"),

        new(
            Number: "05",
            EyebrowKey: "FeatureTourAccountEyebrow",
            TitleKey: "FeatureTourAccountTitle",
            DescriptionKey: "FeatureTourAccountDescription",
            FirstHighlightKey: "FeatureTourAccountHighlightOne",
            SecondHighlightKey: "FeatureTourAccountHighlightTwo",
            ThirdHighlightKey: "FeatureTourAccountHighlightThree",
            VisualKind: "Account"),

        new(
            Number: "06",
            EyebrowKey: "FeatureTourDesktopEyebrow",
            TitleKey: "FeatureTourDesktopTitle",
            DescriptionKey: "FeatureTourDesktopDescription",
            FirstHighlightKey: "FeatureTourDesktopHighlightOne",
            SecondHighlightKey: "FeatureTourDesktopHighlightTwo",
            ThirdHighlightKey: "FeatureTourDesktopHighlightThree",
            VisualKind: "Desktop"),

        new(
            Number: "07",
            EyebrowKey: "FeatureTourReadyEyebrow",
            TitleKey: "FeatureTourReadyTitle",
            DescriptionKey: "FeatureTourReadyDescription",
            FirstHighlightKey: "FeatureTourReadyHighlightOne",
            SecondHighlightKey: "FeatureTourReadyHighlightTwo",
            ThirdHighlightKey: "FeatureTourReadyHighlightThree",
            VisualKind: "Ready")
    ];

    /// <summary>
    /// Gets the ordered feature tour steps.
    /// </summary>
    public static IReadOnlyList<FeatureTourStep> GetSteps()
    {
        return Steps;
    }
}
