using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.Onboarding;

namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Drives the localized first-run GlucoDesk feature tour.
/// </summary>
public sealed class FeatureTourViewModel :
    ObservableObject,
    IDisposable
{
    private readonly IReadOnlyList<FeatureTourStep> _steps;
    private readonly Action _persistCompletion;

    private int _currentStepIndex;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private bool _isDisposed;

    /// <summary>
    /// Initializes the production feature tour ViewModel.
    /// </summary>
    public FeatureTourViewModel(
        FeatureTourPreferenceStore preferenceStore)
        : this(
            FeatureTourCatalog.GetSteps(),
            CreatePersistCompletionAction(preferenceStore))
    {
    }

    /// <summary>
    /// Initializes a testable feature tour ViewModel.
    /// </summary>
    public FeatureTourViewModel(
        IReadOnlyList<FeatureTourStep> steps,
        Action persistCompletion)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(persistCompletion);

        if (steps.Count == 0)
        {
            throw new ArgumentException(
                "At least one feature tour step is required.",
                nameof(steps));
        }

        _steps = steps;
        _persistCompletion = persistCompletion;

        BackCommand = new RelayCommand(
            MoveBack,
            () => CanGoBack);

        NextCommand = new RelayCommand(
            MoveNextOrComplete);

        SkipCommand = new RelayCommand(
            () => Complete(wasSkipped: true));

        LocalizationManager.LanguageChanged +=
            OnLanguageChanged;
    }

    /// <summary>
    /// Raised when the feature tour has been completed or skipped.
    /// </summary>
    public event EventHandler<FeatureTourCompletedEventArgs>?
        Completed;

    /// <summary>
    /// Gets the back command.
    /// </summary>
    public IRelayCommand BackCommand { get; }

    /// <summary>
    /// Gets the next command.
    /// </summary>
    public IRelayCommand NextCommand { get; }

    /// <summary>
    /// Gets the skip command.
    /// </summary>
    public IRelayCommand SkipCommand { get; }

    /// <summary>
    /// Gets the zero-based current step index.
    /// </summary>
    public int CurrentStepIndex =>
        _currentStepIndex;

    /// <summary>
    /// Gets the one-based current step number.
    /// </summary>
    public int CurrentStepNumber =>
        _currentStepIndex + 1;

    /// <summary>
    /// Gets the total number of tour steps.
    /// </summary>
    public int TotalSteps =>
        _steps.Count;

    /// <summary>
    /// Gets the progress bar maximum.
    /// </summary>
    public double ProgressMaximum =>
        TotalSteps;

    /// <summary>
    /// Gets the progress bar current value.
    /// </summary>
    public double ProgressValue =>
        CurrentStepNumber;

    /// <summary>
    /// Gets the progress text used by existing tests and consumers.
    /// </summary>
    public string ProgressText =>
        $"{CurrentStepNumber} / {TotalSteps}";

    /// <summary>
    /// Gets the compact progress text used by the new UI.
    /// </summary>
    public string StepCounterText =>
        $"{CurrentStepNumber}/{TotalSteps}";

    /// <summary>
    /// Gets whether the current page is the first step.
    /// </summary>
    public bool IsFirstStep =>
        _currentStepIndex == 0;

    /// <summary>
    /// Gets whether the current page is the last step.
    /// </summary>
    public bool IsLastStep =>
        _currentStepIndex == _steps.Count - 1;

    /// <summary>
    /// Gets whether navigation to the previous page is allowed.
    /// </summary>
    public bool CanGoBack =>
        !IsFirstStep;

    /// <summary>
    /// Gets the step number stored by the catalog.
    /// </summary>
    public string StepNumber =>
        CurrentStep.Number;

    /// <summary>
    /// Gets the step number used by the new UI.
    /// </summary>
    public string StepNumberText =>
        StepNumber;

    /// <summary>
    /// Gets the localized eyebrow.
    /// </summary>
    public string Eyebrow =>
        T(CurrentStep.EyebrowKey);

    /// <summary>
    /// Gets the localized eyebrow used by the new UI.
    /// </summary>
    public string EyebrowText =>
        Eyebrow;

    /// <summary>
    /// Gets the localized title.
    /// </summary>
    public string Title =>
        T(CurrentStep.TitleKey);

    /// <summary>
    /// Gets the localized title used by the new UI.
    /// </summary>
    public string TitleText =>
        Title;

    /// <summary>
    /// Gets the localized description.
    /// </summary>
    public string Description =>
        T(CurrentStep.DescriptionKey);

    /// <summary>
    /// Gets the localized description used by the new UI.
    /// </summary>
    public string DescriptionText =>
        Description;

    /// <summary>
    /// Gets the first localized highlight.
    /// </summary>
    public string FirstHighlight =>
        T(CurrentStep.FirstHighlightKey);

    /// <summary>
    /// Gets the first localized highlight used by the new UI.
    /// </summary>
    public string HighlightOneText =>
        FirstHighlight;

    /// <summary>
    /// Gets the second localized highlight.
    /// </summary>
    public string SecondHighlight =>
        T(CurrentStep.SecondHighlightKey);

    /// <summary>
    /// Gets the second localized highlight used by the new UI.
    /// </summary>
    public string HighlightTwoText =>
        SecondHighlight;

    /// <summary>
    /// Gets the third localized highlight.
    /// </summary>
    public string ThirdHighlight =>
        T(CurrentStep.ThirdHighlightKey);

    /// <summary>
    /// Gets the third localized highlight used by the new UI.
    /// </summary>
    public string HighlightThreeText =>
        ThirdHighlight;

    /// <summary>
    /// Gets the localized primary action text.
    /// </summary>
    public string PrimaryButtonText =>
        IsLastStep
            ? T("FeatureTourStartButton")
            : T("FeatureTourNextButton");

    /// <summary>
    /// Gets the localized skip action text.
    /// </summary>
    public string SkipButtonText =>
        T("FeatureTourSkipButton");

    /// <summary>
    /// Gets the localized back action text.
    /// </summary>
    public string BackButtonText =>
        T("FeatureTourBackButton");

    /// <summary>
    /// Gets the localized footer text.
    /// </summary>
    public string FooterText =>
        T("FeatureTourFooter");

    /// <summary>
    /// Gets the current visual type.
    /// </summary>
    public string CurrentVisualKind =>
        CurrentStep.VisualKind;

    public bool IsWelcomeVisual =>
        IsVisual("Welcome");

    public bool IsDashboardVisual =>
        IsVisual("Dashboard");

    public bool IsHistoryVisual =>
        IsVisual("History");

    public bool IsDiaryVisual =>
        IsVisual("Diary");

    public bool IsAccountVisual =>
        IsVisual("Account");

    public bool IsDesktopVisual =>
        IsVisual("Desktop");

    public bool IsReadyVisual =>
        IsVisual("Ready");

    /// <summary>
    /// Gets whether the completion state could not be persisted.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(
            ref _hasError,
            value);
    }

    /// <summary>
    /// Gets the localized persistence error.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(
            ref _errorMessage,
            value);
    }

    private FeatureTourStep CurrentStep =>
        _steps[_currentStepIndex];

    private void MoveBack()
    {
        if (IsFirstStep)
        {
            return;
        }

        _currentStepIndex--;
        RefreshStep();
    }

    private void MoveNextOrComplete()
    {
        if (IsLastStep)
        {
            Complete(wasSkipped: false);
            return;
        }

        _currentStepIndex++;
        RefreshStep();
    }

    private void Complete(bool wasSkipped)
    {
        try
        {
            _persistCompletion();

            HasError = false;
            ErrorMessage = string.Empty;

            Completed?.Invoke(
                this,
                new FeatureTourCompletedEventArgs(
                    wasSkipped));
        }
        catch
        {
            HasError = true;
            ErrorMessage =
                T("FeatureTourSaveError");
        }
    }

    private void RefreshStep()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        OnPropertyChanged(nameof(CurrentStepIndex));
        OnPropertyChanged(nameof(CurrentStepNumber));
        OnPropertyChanged(nameof(TotalSteps));
        OnPropertyChanged(nameof(ProgressMaximum));
        OnPropertyChanged(nameof(ProgressValue));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(StepCounterText));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(StepNumber));
        OnPropertyChanged(nameof(StepNumberText));
        OnPropertyChanged(nameof(Eyebrow));
        OnPropertyChanged(nameof(EyebrowText));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(DescriptionText));
        OnPropertyChanged(nameof(FirstHighlight));
        OnPropertyChanged(nameof(HighlightOneText));
        OnPropertyChanged(nameof(SecondHighlight));
        OnPropertyChanged(nameof(HighlightTwoText));
        OnPropertyChanged(nameof(ThirdHighlight));
        OnPropertyChanged(nameof(HighlightThreeText));
        OnPropertyChanged(nameof(PrimaryButtonText));
        OnPropertyChanged(nameof(SkipButtonText));
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(FooterText));
        OnPropertyChanged(nameof(CurrentVisualKind));
        OnPropertyChanged(nameof(IsWelcomeVisual));
        OnPropertyChanged(nameof(IsDashboardVisual));
        OnPropertyChanged(nameof(IsHistoryVisual));
        OnPropertyChanged(nameof(IsDiaryVisual));
        OnPropertyChanged(nameof(IsAccountVisual));
        OnPropertyChanged(nameof(IsDesktopVisual));
        OnPropertyChanged(nameof(IsReadyVisual));

        BackCommand.NotifyCanExecuteChanged();
    }

    private void OnLanguageChanged(
        object? sender,
        EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        RefreshStep();
    }

    private bool IsVisual(string visualKind)
    {
        return string.Equals(
            CurrentVisualKind,
            visualKind,
            StringComparison.OrdinalIgnoreCase);
    }

    private static Action CreatePersistCompletionAction(
        FeatureTourPreferenceStore preferenceStore)
    {
        ArgumentNullException.ThrowIfNull(preferenceStore);

        return preferenceStore.MarkCurrentTourCompleted;
    }

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        LocalizationManager.LanguageChanged -=
            OnLanguageChanged;

        _isDisposed = true;
    }
}
