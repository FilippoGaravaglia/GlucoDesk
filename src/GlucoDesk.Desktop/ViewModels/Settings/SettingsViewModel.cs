using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;

namespace GlucoDesk.Desktop.ViewModels.Settings;

/// <summary>
/// Represents the settings screen view model.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly IApplicationSettingsService _settingsService;

    [ObservableProperty]
    private ProviderSelectionItem? _selectedLiveProvider;

    [ObservableProperty]
    private ProviderSelectionItem? _selectedHistoricalProvider;

    [ObservableProperty]
    private GlucoseUnitSelectionItem? _selectedPreferredUnit;

    [ObservableProperty]
    private string _targetLowMgDlText = "70";

    [ObservableProperty]
    private string _targetHighMgDlText = "180";

    [ObservableProperty]
    private string _dashboardRefreshIntervalSecondsText = "30";

    [ObservableProperty]
    private string _statusMessage = "Settings not loaded";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">The application settings service.</param>
    public SettingsViewModel(IApplicationSettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        _settingsService = settingsService;

        ProviderOptions = BuildProviderOptions();
        PreferredUnitOptions = BuildPreferredUnitOptions();

        SelectedLiveProvider = ProviderOptions[0];
        SelectedHistoricalProvider = ProviderOptions[0];
        SelectedPreferredUnit = PreferredUnitOptions[0];
    }

    /// <summary>
    /// Gets the available CGM provider options.
    /// </summary>
    public IReadOnlyList<ProviderSelectionItem> ProviderOptions { get; }

    /// <summary>
    /// Gets the available glucose unit options.
    /// </summary>
    public IReadOnlyList<GlucoseUnitSelectionItem> PreferredUnitOptions { get; }

    /// <summary>
    /// Loads settings from the configured application settings service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Loading settings...";

        try
        {
            var result = await _settingsService.GetSettingsAsync(cancellationToken);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Unable to load settings");
                return;
            }

            ApplySettings(result.Value);
            StatusMessage = "Settings loaded";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Settings load cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while loading settings");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves settings through the configured application settings service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        var settings = CreateSettingsFromForm(out var validationMessage);

        if (settings is null)
        {
            ApplyValidationFailure(validationMessage ?? "Invalid settings.");
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Saving settings...";

        try
        {
            var result = await _settingsService.SaveSettingsAsync(settings, cancellationToken);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Unable to save settings");
                return;
            }

            StatusMessage = "Settings saved. Restart or reload the dashboard to apply all changes.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Settings save cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while saving settings");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Helpers

    /// <summary>
    /// Applies loaded application settings to the editable form.
    /// </summary>
    /// <param name="settings">The loaded application settings.</param>
    private void ApplySettings(ApplicationSettings settings)
    {
        SelectedLiveProvider = FindProviderOption(settings.ActiveLiveProvider);
        SelectedHistoricalProvider = FindProviderOption(settings.HistoricalProvider);
        SelectedPreferredUnit = FindPreferredUnitOption(settings.PreferredUnit);

        TargetLowMgDlText = settings.TargetLowMgDl.ToString(CultureInfo.InvariantCulture);
        TargetHighMgDlText = settings.TargetHighMgDl.ToString(CultureInfo.InvariantCulture);
        DashboardRefreshIntervalSecondsText = ((int)settings.DashboardRefreshInterval.TotalSeconds)
            .ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Creates application settings from the editable form values.
    /// </summary>
    /// <param name="validationMessage">The validation error message when settings cannot be created.</param>
    /// <returns>The application settings when valid; otherwise null.</returns>
    private ApplicationSettings? CreateSettingsFromForm(out string? validationMessage)
    {
        validationMessage = null;

        if (SelectedLiveProvider is null)
        {
            validationMessage = "Select a live provider.";
            return null;
        }

        if (SelectedHistoricalProvider is null)
        {
            validationMessage = "Select a historical provider.";
            return null;
        }

        if (SelectedPreferredUnit is null)
        {
            validationMessage = "Select a preferred glucose unit.";
            return null;
        }

        if (!TryParsePositiveInteger(TargetLowMgDlText, out var targetLowMgDl))
        {
            validationMessage = "Target low must be a positive integer.";
            return null;
        }

        if (!TryParsePositiveInteger(TargetHighMgDlText, out var targetHighMgDl))
        {
            validationMessage = "Target high must be a positive integer.";
            return null;
        }

        if (!TryParsePositiveInteger(DashboardRefreshIntervalSecondsText, out var refreshIntervalSeconds))
        {
            validationMessage = "Refresh interval must be a positive integer.";
            return null;
        }

        try
        {
            return new ApplicationSettings(
                SelectedLiveProvider.Kind,
                SelectedHistoricalProvider.Kind,
                SelectedPreferredUnit.Unit,
                targetLowMgDl,
                targetHighMgDl,
                TimeSpan.FromSeconds(refreshIntervalSeconds));
        }
        catch (ArgumentException exception)
        {
            validationMessage = exception.Message;
            return null;
        }
    }

    /// <summary>
    /// Applies a failed application result to the settings view model.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyFailure<TValue>(
        Result<TValue> result,
        string statusMessage)
        where TValue : notnull
    {
        HasError = true;
        ErrorMessage = $"{result.Error.Code}: {result.Error.Message}";
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Applies a failed application result to the settings view model.
    /// </summary>
    /// <param name="result">The failed result.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyFailure(
        Result result,
        string statusMessage)
    {
        HasError = true;
        ErrorMessage = $"{result.Error.Code}: {result.Error.Message}";
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Applies a validation failure to the settings view model.
    /// </summary>
    /// <param name="message">The validation message.</param>
    private void ApplyValidationFailure(string message)
    {
        HasError = true;
        ErrorMessage = message;
        StatusMessage = "Settings validation failed";
    }

    /// <summary>
    /// Applies an unexpected exception to the settings view model.
    /// </summary>
    /// <param name="exception">The unexpected exception.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyUnexpectedFailure(
        Exception exception,
        string statusMessage)
    {
        HasError = true;
        ErrorMessage = exception.Message;
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Finds a provider option by provider kind.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <returns>The matching provider option.</returns>
    private ProviderSelectionItem FindProviderOption(CgmProviderKind kind)
    {
        return ProviderOptions.FirstOrDefault(option => option.Kind == kind)
            ?? ProviderOptions[0];
    }

    /// <summary>
    /// Finds a glucose unit option by unit.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The matching glucose unit option.</returns>
    private GlucoseUnitSelectionItem FindPreferredUnitOption(GlucoseUnit unit)
    {
        return PreferredUnitOptions.FirstOrDefault(option => option.Unit == unit)
            ?? PreferredUnitOptions[0];
    }

    /// <summary>
    /// Parses a positive integer using invariant culture.
    /// </summary>
    /// <param name="value">The text value to parse.</param>
    /// <param name="result">The parsed integer value.</param>
    /// <returns>True when parsing succeeds and the value is positive; otherwise false.</returns>
    private static bool TryParsePositiveInteger(
        string value,
        out int result)
    {
        return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result)
            && result > 0;
    }

    /// <summary>
    /// Builds selectable provider options from the CGM provider enum.
    /// </summary>
    /// <returns>The selectable provider options.</returns>
    private static IReadOnlyList<ProviderSelectionItem> BuildProviderOptions()
    {
        return Enum
            .GetValues<CgmProviderKind>()
            .Where(kind => kind != CgmProviderKind.Unknown)
            .Select(kind => new ProviderSelectionItem(kind, FormatEnumName(kind.ToString())))
            .ToArray();
    }

    /// <summary>
    /// Builds selectable glucose unit options from the glucose unit enum.
    /// </summary>
    /// <returns>The selectable glucose unit options.</returns>
    private static IReadOnlyList<GlucoseUnitSelectionItem> BuildPreferredUnitOptions()
    {
        return Enum
            .GetValues<GlucoseUnit>()
            .Select(unit => new GlucoseUnitSelectionItem(unit, FormatEnumName(unit.ToString())))
            .ToArray();
    }

    /// <summary>
    /// Formats enum names into display-friendly text.
    /// </summary>
    /// <param name="value">The enum name.</param>
    /// <returns>The display-friendly enum name.</returns>
    private static string FormatEnumName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];

            if (index > 0 && char.IsUpper(character))
            {
                builder.Append(' ');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    #endregion
}