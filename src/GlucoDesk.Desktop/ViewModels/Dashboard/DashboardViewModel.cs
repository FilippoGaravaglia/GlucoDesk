using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Events;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Services;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;
using GlucoDesk.Desktop.ViewModels.Dashboard.Errors;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Dashboard.Providers;
using GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;

namespace GlucoDesk.Desktop.ViewModels.Dashboard;

/// <summary>
/// Represents the dashboard view model used by the desktop shell.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private const int ThreeHourChartWindow = 3;
    private const int SixHourChartWindow = 6;
    private const int TwelveHourChartWindow = 12;
    private const int TwentyFourHourChartWindow = 24;
    private const double MinimumStatisticsCoverageRatio = 0.70d;

    private static readonly StatisticsWindowSelectionItem TwentyFourHourStatisticsWindow = new(
        "24H",
        TimeSpan.FromHours(24),
        "24 hours");

    private static readonly StatisticsWindowSelectionItem ThreeDayStatisticsWindow = new(
        "3D",
        TimeSpan.FromDays(3),
        "3 days");

    private static readonly StatisticsWindowSelectionItem SevenDayStatisticsWindow = new(
        "7D",
        TimeSpan.FromDays(7),
        "7 days");

    private static readonly StatisticsWindowSelectionItem FourteenDayStatisticsWindow = new(
        "14D",
        TimeSpan.FromDays(14),
        "14 days");

    private static readonly StatisticsWindowSelectionItem ThirtyDayStatisticsWindow = new(
        "30D",
        TimeSpan.FromDays(30),
        "30 days");

    private static readonly StatisticsWindowSelectionItem NinetyDayStatisticsWindow = new(
        "90D",
        TimeSpan.FromDays(90),
        "90 days");

    private readonly IGlucoseDataService _glucoseDataService;
    private readonly IApplicationSettingsService _settingsService;
    private readonly IApplicationSettingsChangeNotifier? _settingsChangeNotifier;
    private readonly IGlucoseHistoryService? _glucoseHistoryService;
    private readonly IGlucoseStatisticsService? _glucoseStatisticsService;
    private readonly IWidgetStatePublisher? _widgetStatePublisher;
    private readonly GlucoseAlertCoordinator _glucoseAlertCoordinator;
    private readonly GlucoseAlertSnoozeState _glucoseAlertSnoozeState = new();
    private readonly GlucoseAlertStabilityGate _glucoseAlertStabilityGate = new();
    private readonly DashboardRefreshOptions _refreshOptions;

    private GlucoseDashboardSnapshot? _lastDashboardSnapshot;
    private ApplicationSettings _currentSettings = ApplicationSettings.Default;
    private GlucoseAlertKind _currentGlucoseAlertKind = GlucoseAlertKind.None;
    private GlucoseAlertKind _dismissedGlucoseAlertKind = GlucoseAlertKind.None;

    private bool _isInitialized;
    private TimeSpan _autoRefreshInterval;
    private IReadOnlyList<GlucoseChartPoint> _allChartPoints = [];

    [ObservableProperty]
    private string _providerDisplayName = "Not loaded";

    [ObservableProperty]
    private string _latestValueText = "—";

    [ObservableProperty]
    private string _trendText = "—";

    [ObservableProperty]
    private string _freshnessText = "—";

    [ObservableProperty]
    private string _lastUpdatedText = "—";

    [ObservableProperty]
    private string _statusText = "Waiting for data";

    [ObservableProperty]
    private string _recentReadingsCountText = "0 readings";

    [ObservableProperty]
    private IReadOnlyList<GlucoseChartPoint> _chartPoints = [];

    [ObservableProperty]
    private string _chartSummaryText = "No chart data";

    [ObservableProperty]
    private int _selectedChartWindowHours = ThreeHourChartWindow;

    [ObservableProperty]
    private bool _isThreeHourChartWindowSelected = true;

    [ObservableProperty]
    private bool _isSixHourChartWindowSelected;

    [ObservableProperty]
    private bool _isTwelveHourChartWindowSelected;

    [ObservableProperty]
    private bool _isTwentyFourHourChartWindowSelected;

    [ObservableProperty]
    private decimal _targetLowMgDl = 70m;

    [ObservableProperty]
    private decimal _targetHighMgDl = 180m;

    [ObservableProperty]
    private string _targetRangeText = "Target range: 70-180 mg/dL";

    [ObservableProperty]
    private string _autoRefreshStatusText = "Auto-refresh not started";

    [ObservableProperty]
    private string _settingsStatusText = "Settings not loaded";

    [ObservableProperty]
    private string _historyStatusText = "History not updated";

    [ObservableProperty]
    private string _statisticsStatusText = "Statistics are not available in the current desktop runtime.";

    [ObservableProperty]
    private string _statisticsAverageGlucoseText = "—";

    [ObservableProperty]
    private string _statisticsTimeInRangeText = "—";

    [ObservableProperty]
    private string _statisticsBelowRangeText = "—";

    [ObservableProperty]
    private string _statisticsAboveRangeText = "—";

    [ObservableProperty]
    private string _statisticsReadingsAnalyzedText = "—";

    [ObservableProperty]
    private string _statisticsTargetRangeText = "Target range: —";

    [ObservableProperty]
    private bool _isStatisticsEnabled;

    [ObservableProperty]
    private bool _hasStatisticsData;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _dataSourceStatusText = "Data source not checked";

    [ObservableProperty]
    private string _providerStatusTitle = "Using Mock data";

    [ObservableProperty]
    private string _providerStatusMessage = "Mock is the active live provider. Configure and select Dexcom or Nightscout in Settings to use real glucose data.";

    [ObservableProperty]
    private string _providerStatusBadgeText = "Mock";

    [ObservableProperty]
    private bool _isRealProviderActive;

    [ObservableProperty]
    private bool _isMockProviderActive = true;

    [ObservableProperty]
    private string _dataHealthTitle = "Demo data active";

    [ObservableProperty]
    private string _dataHealthMessage = "The dashboard is currently showing Mock provider data.";

    [ObservableProperty]
    private string _dataHealthBadgeText = "Demo";

    [ObservableProperty]
    private bool _isDataStale;

    [ObservableProperty]
    private bool _isDataUnavailable;

    [ObservableProperty]
    private bool _isShowingRealProviderData;

    [ObservableProperty]
    private string _dashboardContextText = "Waiting for dashboard data";

    [ObservableProperty]
    private double _statisticsTimeInRangePercentValue;

    [ObservableProperty]
    private double _statisticsBelowRangePercentValue;

    [ObservableProperty]
    private double _statisticsAboveRangePercentValue;

    [ObservableProperty]
    private string _statisticsAverageInsightText = "Waiting for enough readings.";

    [ObservableProperty]
    private string _statisticsTimeInRangeInsightText = "Waiting for enough readings.";

    [ObservableProperty]
    private string _statisticsBelowRangeInsightText = "Waiting for enough readings.";

    [ObservableProperty]
    private string _statisticsAboveRangeInsightText = "Waiting for enough readings.";

    [ObservableProperty]
    private string _statisticsWindowStatusText = "Local insights · 24H";

    [ObservableProperty]
    private string _statisticsHistoryAvailabilityText = "Calculated from available local GlucoDesk history.";

    [ObservableProperty]
    private string _statisticsInsufficientHistoryTitle = "Not enough local history yet";

    [ObservableProperty]
    private string _statisticsInsufficientHistoryMessage = "Keep GlucoDesk running to build reliable local insights for this time window.";

    [ObservableProperty]
    private bool _isStatisticsCardsVisible;

    [ObservableProperty]
    private bool _isStatisticsHistoryInsufficient;

    [ObservableProperty]
    private bool _isTwentyFourHourStatisticsWindowSelected = true;

    [ObservableProperty]
    private bool _isThreeDayStatisticsWindowSelected;

    [ObservableProperty]
    private bool _isSevenDayStatisticsWindowSelected;

    [ObservableProperty]
    private bool _isFourteenDayStatisticsWindowSelected;

    [ObservableProperty]
    private bool _isThirtyDayStatisticsWindowSelected;

    [ObservableProperty]
    private bool _isNinetyDayStatisticsWindowSelected;

    [ObservableProperty]
    private GlucoseUnit _preferredUnit = GlucoseUnit.MgDl;

    [ObservableProperty]
    private int _chartMaximumMgDl = 300;

    [ObservableProperty]
    private bool _isGlucoseAlertBannerVisible;

    [ObservableProperty]
    private string _glucoseAlertTitle = string.Empty;

    [ObservableProperty]
    private string _glucoseAlertMessage = string.Empty;


    [ObservableProperty]
    private string _glucoseAlertSnoozeStatusText = string.Empty;
    [ObservableProperty]
    private string _glucoseAlertBadgeText = string.Empty;

    [ObservableProperty]
    private string _glucoseAlertActionText = "Open official app";

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="refreshOptions">The optional dashboard refresh fallback options.</param>
    /// <param name="settingsChangeNotifier">The optional application settings change notifier.</param>
    /// <param name="glucoseHistoryService">The optional glucose history service.</param>
    /// <param name="glucoseStatisticsService">The glucose statistics service.</param>
    /// <param name="widgetStatePublisher">The optional widget state publisher.</param>
    /// <param name="glucoseAlertNotificationService">The optional native glucose alert notification service.</param>
    /// <param name="glucoseAlertClock">The optional glucose alert clock.</param>
    public DashboardViewModel(
        IGlucoseDataService glucoseDataService,
        IApplicationSettingsService settingsService,
        DashboardRefreshOptions? refreshOptions = null,
        IApplicationSettingsChangeNotifier? settingsChangeNotifier = null,
        IGlucoseHistoryService? glucoseHistoryService = null,
        IGlucoseStatisticsService? glucoseStatisticsService = null,
        IWidgetStatePublisher? widgetStatePublisher = null,
        IGlucoseAlertNotificationService? glucoseAlertNotificationService = null,
        IGlucoseAlertClock? glucoseAlertClock = null)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);
        ArgumentNullException.ThrowIfNull(settingsService);

        _glucoseDataService = glucoseDataService;
        _settingsService = settingsService;
        _refreshOptions = refreshOptions ?? DashboardRefreshOptions.Default;
        _settingsChangeNotifier = settingsChangeNotifier;
        _glucoseHistoryService = glucoseHistoryService;
        _autoRefreshInterval = _refreshOptions.AutoRefreshInterval;
        _glucoseStatisticsService = glucoseStatisticsService;
        _widgetStatePublisher = widgetStatePublisher;
        _glucoseAlertCoordinator = new GlucoseAlertCoordinator(
            glucoseAlertNotificationService ?? OperatingSystemGlucoseAlertNotificationService.Create(),
            glucoseAlertClock ?? SystemGlucoseAlertClock.Instance);
        IsStatisticsEnabled = _glucoseStatisticsService is not null;
        ApplyStatisticsPresentation(DashboardStatisticsPresenter.Disabled());

        if (_settingsChangeNotifier is not null)
        {
            _settingsChangeNotifier.SettingsChanged += OnSettingsChanged;
        }

        AutoRefreshStatusText = $"Auto-refresh every {FormatInterval(_autoRefreshInterval)}";
    }

    /// <summary>
    /// Gets the configured automatic refresh interval.
    /// </summary>
    public TimeSpan AutoRefreshInterval => _autoRefreshInterval;

    /// <summary>
    /// Initializes dashboard settings before the view starts automatic refresh.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        var result = await _settingsService
            .GetSettingsAsync(cancellationToken);

        if (result.IsFailure)
        {
            ApplySettingsFallback(result);
            return;
        }

        ApplySettings(result.Value);
    }

    /// <summary>
    /// Refreshes the dashboard using the configured glucose data service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusText = "Refreshing...";

        try
        {
            var result = await _glucoseDataService
                .GetDashboardSnapshotAsync(CreateDashboardRequest(), cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result);

                await PublishUnavailableWidgetStateAsync(
                        CgmProviderKind.Unknown,
                        StatusText,
                        cancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            ApplySnapshot(result.Value);

            await PublishSnapshotToWidgetStateAsync(result.Value, cancellationToken)
                .ConfigureAwait(false);

            await PersistSnapshotToHistoryAsync(result.Value, cancellationToken)
                .ConfigureAwait(false);

            await RefreshStatisticsAsync(result.Value, cancellationToken)
                .ConfigureAwait(false);

            AutoRefreshStatusText = $"Last refresh: {DateTimeOffset.Now:HH:mm:ss}";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Refresh cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception);

            await PublishUnavailableWidgetStateAsync(
                    CgmProviderKind.Unknown,
                    "Unexpected dashboard error",
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Selects the three-hour chart window.
    /// </summary>
    [RelayCommand]
    private void SelectThreeHourChartWindow()
    {
        SelectChartWindow(ThreeHourChartWindow);
    }

    /// <summary>
    /// Selects the six-hour chart window.
    /// </summary>
    [RelayCommand]
    private void SelectSixHourChartWindow()
    {
        SelectChartWindow(SixHourChartWindow);
    }

    /// <summary>
    /// Selects the twelve-hour chart window.
    /// </summary>
    [RelayCommand]
    private void SelectTwelveHourChartWindow()
    {
        SelectChartWindow(TwelveHourChartWindow);
    }

    /// <summary>
    /// Selects the twenty-four-hour chart window.
    /// </summary>
    [RelayCommand]
    private void ShowTwentyFourHourChartWindow()
    {
        SelectChartWindow(TwentyFourHourChartWindow);
    }

    /// <summary>
    /// Selects the twenty-four-hour statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectTwentyFourHourStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(TwentyFourHourStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Selects the three-day statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectThreeDayStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(ThreeDayStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Selects the seven-day statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectSevenDayStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(SevenDayStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Selects the fourteen-day statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectFourteenDayStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(FourteenDayStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Selects the thirty-day statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectThirtyDayStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(ThirtyDayStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Selects the ninety-day statistics window.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private Task SelectNinetyDayStatisticsWindowAsync(CancellationToken cancellationToken)
    {
        return SelectStatisticsWindowAsync(NinetyDayStatisticsWindow, cancellationToken);
    }

    /// <summary>
    /// Dismisses the current glucose awareness banner until the condition changes.
    /// </summary>
    /// <summary>
    /// Snoozes the current glucose alert banner for the configured repeat cooldown duration.
    /// </summary>
    [RelayCommand]
    private void SnoozeGlucoseAlertBanner()
    {
        if (_currentGlucoseAlertKind == GlucoseAlertKind.None)
        {
            return;
        }

        var now = DateTimeOffset.Now;
        var snoozedUntil = _glucoseAlertSnoozeState.Snooze(
            _currentGlucoseAlertKind,
            _currentSettings.GlucoseAlertRepeatInterval,
            now);

        _dismissedGlucoseAlertKind = _currentGlucoseAlertKind;
        GlucoseAlertSnoozeStatusText = $"Snoozed until {snoozedUntil:HH:mm}.";
        ClearGlucoseAlertBanner();
    }

    /// <summary>
    /// Dismisses the current glucose alert banner.
    /// </summary>
    [RelayCommand]
    private void DismissGlucoseAlertBanner()
    {
        _dismissedGlucoseAlertKind = _currentGlucoseAlertKind;
        IsGlucoseAlertBannerVisible = false;
    }

    /// <summary>
    /// Releases event subscriptions owned by the dashboard view model.
    /// </summary>
    public void Dispose()
    {
        if (_settingsChangeNotifier is not null)
        {
            _settingsChangeNotifier.SettingsChanged -= OnSettingsChanged;
        }
    }

    #region Helpers

    /// <summary>
    /// Publishes the current dashboard snapshot to the local widget state store.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PublishSnapshotToWidgetStateAsync(
        GlucoseDashboardSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        if (_widgetStatePublisher is null)
        {
            return;
        }

        if (snapshot.LatestReading is not null)
        {
            await PublishWidgetStateSafelyAsync(
                    () => _widgetStatePublisher.PublishReadingAsync(
                        snapshot.LatestReading,
                        cancellationToken),
                    cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        await PublishUnavailableWidgetStateAsync(
                snapshot.Metadata.ProviderKind,
                "No current glucose reading available",
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes an unavailable widget state without affecting the dashboard refresh flow.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="statusMessage">The status message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PublishUnavailableWidgetStateAsync(
        CgmProviderKind providerKind,
        string statusMessage,
        CancellationToken cancellationToken)
    {
        if (_widgetStatePublisher is null)
        {
            return;
        }

        await PublishWidgetStateSafelyAsync(
                () => _widgetStatePublisher.PublishUnavailableAsync(
                    providerKind,
                    statusMessage,
                    cancellationToken),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes widget state as a best-effort side effect.
    /// </summary>
    /// <param name="publishOperation">The publish operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task PublishWidgetStateSafelyAsync(
        Func<Task<Result>> publishOperation,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var result = await publishOperation()
                .ConfigureAwait(false);

            _ = result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Widget state publishing is best-effort during cancellation.
        }
        catch
        {
            // Widget state publishing must never break the dashboard refresh flow.
        }
    }

    /// <summary>
    /// Builds a compact dashboard context message for the consumer dashboard.
    /// </summary>
    /// <param name="providerDisplayName">The active provider display name.</param>
    /// <param name="freshnessText">The data freshness text.</param>
    /// <param name="readingsCountText">The readings count text.</param>
    /// <param name="lastUpdatedText">The last update text.</param>
    /// <returns>The dashboard context text.</returns>
    private static string BuildDashboardContextText(
        string providerDisplayName,
        string freshnessText,
        string readingsCountText,
        string lastUpdatedText)
    {
        return $"{providerDisplayName} · {freshnessText} · {readingsCountText} · Updated {lastUpdatedText}";
    }

    /// <summary>
    /// Parses a percentage text into a numeric percentage value.
    /// </summary>
    /// <param name="percentageText">The percentage text.</param>
    /// <returns>The parsed percentage value.</returns>
    private static double ParsePercentageValue(string percentageText)
    {
        if (string.IsNullOrWhiteSpace(percentageText))
        {
            return 0d;
        }

        var normalizedText = percentageText
            .Replace("%", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (double.TryParse(
                normalizedText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var invariantValue))
        {
            return Math.Clamp(invariantValue, 0d, 100d);
        }

        if (double.TryParse(
                normalizedText,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var currentCultureValue))
        {
            return Math.Clamp(currentCultureValue, 0d, 100d);
        }

        return 0d;
    }

    /// <summary>
    /// Builds the average glucose insight text.
    /// </summary>
    /// <param name="hasStatisticsData">A value indicating whether statistics data is available.</param>
    /// <returns>The average glucose insight text.</returns>
    private static string BuildAverageInsight(bool hasStatisticsData)
    {
        return hasStatisticsData
            ? "Average glucose for the selected local history window."
            : "Average glucose will appear after enough readings are available.";
    }

    /// <summary>
    /// Builds the time in range insight text.
    /// </summary>
    /// <param name="percentage">The time in range percentage.</param>
    /// <param name="hasStatisticsData">A value indicating whether statistics data is available.</param>
    /// <returns>The time in range insight text.</returns>
    private static string BuildTimeInRangeInsight(
        double percentage,
        bool hasStatisticsData)
    {
        if (!hasStatisticsData)
        {
            return "Time in range will appear after enough readings are available.";
        }

        return percentage switch
        {
            >= 90d => "Excellent stability across the selected window.",
            >= 75d => "Good time in target range.",
            >= 60d => "Partially in range. Worth monitoring patterns.",
            _ => "Low time in range. Review the trend with your official diabetes tools."
        };
    }

    /// <summary>
    /// Builds the below range insight text.
    /// </summary>
    /// <param name="percentage">The below range percentage.</param>
    /// <param name="hasStatisticsData">A value indicating whether statistics data is available.</param>
    /// <returns>The below range insight text.</returns>
    private static string BuildBelowRangeInsight(
        double percentage,
        bool hasStatisticsData)
    {
        if (!hasStatisticsData)
        {
            return "Low exposure will appear after enough readings are available.";
        }

        return percentage switch
        {
            <= 1d => "Minimal low exposure.",
            <= 4d => "Some low exposure detected.",
            _ => "Frequent low exposure. Check official diabetes apps and alerts."
        };
    }

    /// <summary>
    /// Builds the above range insight text.
    /// </summary>
    /// <param name="percentage">The above range percentage.</param>
    /// <param name="hasStatisticsData">A value indicating whether statistics data is available.</param>
    /// <returns>The above range insight text.</returns>
    private static string BuildAboveRangeInsight(
        double percentage,
        bool hasStatisticsData)
    {
        if (!hasStatisticsData)
        {
            return "High exposure will appear after enough readings are available.";
        }

        return percentage switch
        {
            <= 5d => "Mostly controlled above-range exposure.",
            <= 20d => "Some above-range time detected.",
            _ => "High exposure is elevated. Review the day pattern carefully."
        };
    }

    /// <summary>
    /// Creates the dashboard request used to load enough readings for all supported chart windows.
    /// </summary>
    /// <returns>The dashboard request.</returns>
    private static GlucoseDashboardRequest CreateDashboardRequest()
    {
        return new GlucoseDashboardRequest(
            TimeSpan.FromHours(TwentyFourHourChartWindow),
            TimeSpan.FromMinutes(15),
            maxReadings: CalculateDashboardMaxReadings(TwentyFourHourChartWindow));
    }
    
    /// <summary>
    /// Calculates the expected number of CGM readings for a chart window.
    /// </summary>
    /// <param name="windowHours">The chart window expressed in hours.</param>
    /// <returns>The expected number of readings.</returns>
    private static int CalculateDashboardMaxReadings(int windowHours)
    {
        const int readingsPerHour = 12;
    
        return windowHours * readingsPerHour;
    }

    /// <summary>
    /// Refreshes dashboard statistics from local glucose history.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RefreshStatisticsAsync(
        GlucoseDashboardSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        if (_glucoseStatisticsService is null)
        {
            IsStatisticsEnabled = false;
            IsStatisticsCardsVisible = false;
            IsStatisticsHistoryInsufficient = false;
            ApplyStatisticsPresentation(DashboardStatisticsPresenter.Disabled());
            return;
        }

        IsStatisticsEnabled = true;

        var selectedWindow = GetSelectedStatisticsWindow();
        var targetRange = GlucoseStatisticsTargetRange.DefaultMgDl();
        var request = BuildStatisticsRequest(snapshot, targetRange, selectedWindow);

        var result = await _glucoseStatisticsService
            .CalculateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            IsStatisticsCardsVisible = false;
            IsStatisticsHistoryInsufficient = false;
            ApplyStatisticsPresentation(DashboardStatisticsPresenter.Failed(result.Error.Code));
            return;
        }

        var presentation = DashboardStatisticsPresenter.Present(result.Value, targetRange);

        ApplyStatisticsPresentation(
            presentation,
            result.Value,
            selectedWindow);
    }

    /// <summary>
    /// Builds the statistics request for the selected local history window.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <param name="targetRange">The target range.</param>
    /// <param name="selectedWindow">The selected statistics window.</param>
    /// <returns>The statistics request.</returns>
    private static GlucoseStatisticsRequest BuildStatisticsRequest(
        GlucoseDashboardSnapshot snapshot,
        GlucoseStatisticsTargetRange targetRange,
        StatisticsWindowSelectionItem selectedWindow)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(targetRange);
        ArgumentNullException.ThrowIfNull(selectedWindow);

        var to = snapshot.RecentReadings.Count > 0
            ? snapshot.RecentReadings.Max(reading => reading.Timestamp)
            : snapshot.SnapshotCreatedAt;

        var from = to.Subtract(selectedWindow.Duration);

        if (to <= from)
        {
            to = from.AddMinutes(1);
        }

        return new GlucoseStatisticsRequest(
            from,
            to,
            targetRange,
            includeMockData: snapshot.Metadata.ProviderKind is CgmProviderKind.Mock);
    }

    /// <summary>
    /// Applies a statistics presentation to the dashboard.
    /// </summary>
    /// <param name="presentation">The statistics presentation.</param>
    private void ApplyStatisticsPresentation(DashboardStatisticsPresentation presentation)
    {
        StatisticsStatusText = presentation.StatusText;
        StatisticsAverageGlucoseText = presentation.AverageGlucoseText;
        StatisticsTimeInRangeText = presentation.TimeInRangeText;
        StatisticsBelowRangeText = presentation.BelowRangeText;
        StatisticsAboveRangeText = presentation.AboveRangeText;
        StatisticsReadingsAnalyzedText = presentation.ReadingsAnalyzedText;
        StatisticsTargetRangeText = presentation.TargetRangeText;
        HasStatisticsData = presentation.HasStatisticsData;

        StatisticsTimeInRangePercentValue = ParsePercentageValue(presentation.TimeInRangeText);
        StatisticsBelowRangePercentValue = ParsePercentageValue(presentation.BelowRangeText);
        StatisticsAboveRangePercentValue = ParsePercentageValue(presentation.AboveRangeText);

        StatisticsAverageInsightText = BuildAverageInsight(presentation.HasStatisticsData);

        StatisticsTimeInRangeInsightText = BuildTimeInRangeInsight(
            StatisticsTimeInRangePercentValue,
            presentation.HasStatisticsData);

        StatisticsBelowRangeInsightText = BuildBelowRangeInsight(
            StatisticsBelowRangePercentValue,
            presentation.HasStatisticsData);

        StatisticsAboveRangeInsightText = BuildAboveRangeInsight(
            StatisticsAboveRangePercentValue,
            presentation.HasStatisticsData);

        IsStatisticsHistoryInsufficient = false;
        IsStatisticsCardsVisible = presentation.HasStatisticsData;
    }

    /// <summary>
    /// Applies a statistics presentation with local history availability information.
    /// </summary>
    /// <param name="presentation">The statistics presentation.</param>
    /// <param name="result">The statistics result.</param>
    /// <param name="selectedWindow">The selected statistics window.</param>
    private void ApplyStatisticsPresentation(
        DashboardStatisticsPresentation presentation,
        GlucoseStatisticsResult result,
        StatisticsWindowSelectionItem selectedWindow)
    {
        ApplyStatisticsPresentation(presentation);

        var availability = BuildStatisticsHistoryAvailability(result, selectedWindow);

        StatisticsWindowStatusText = BuildStatisticsWindowStatusText(
            selectedWindow,
            result,
            availability);

        StatisticsHistoryAvailabilityText = availability.HasEnoughData
            ? $"Calculated from local GlucoDesk history for the selected {selectedWindow.Label} window."
            : "Local insights are calculated only from readings saved by GlucoDesk.";

        IsStatisticsHistoryInsufficient = !availability.HasEnoughData;
        IsStatisticsCardsVisible = presentation.HasStatisticsData && availability.HasEnoughData;

        if (!availability.HasEnoughData)
        {
            StatisticsInsufficientHistoryTitle = $"Not enough history for {selectedWindow.Label}";
            StatisticsInsufficientHistoryMessage = BuildInsufficientStatisticsHistoryMessage(
                selectedWindow,
                availability);
        }
    }

    /// <summary>
    /// Selects the active statistics time window and refreshes local insights.
    /// </summary>
    /// <param name="selectedWindow">The selected statistics window.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task SelectStatisticsWindowAsync(
        StatisticsWindowSelectionItem selectedWindow,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selectedWindow);

        ApplyStatisticsWindowSelectionState(selectedWindow);

        if (_lastDashboardSnapshot is null)
        {
            StatisticsWindowStatusText = $"Local insights · {selectedWindow.Label}";
            StatisticsHistoryAvailabilityText = "Refresh the dashboard to calculate local insights for this window.";
            return;
        }

        try
        {
            await RefreshStatisticsAsync(_lastDashboardSnapshot, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            StatisticsStatusText = "Statistics refresh cancelled";
        }
        catch (Exception exception)
        {
            HasError = true;
            ErrorMessage = exception.Message;
            StatisticsStatusText = "Unable to refresh local statistics";
        }
    }

    /// <summary>
    /// Applies the selected statistics window state.
    /// </summary>
    /// <param name="selectedWindow">The selected statistics window.</param>
    private void ApplyStatisticsWindowSelectionState(StatisticsWindowSelectionItem selectedWindow)
    {
        IsTwentyFourHourStatisticsWindowSelected = ReferenceEquals(selectedWindow, TwentyFourHourStatisticsWindow);
        IsThreeDayStatisticsWindowSelected = ReferenceEquals(selectedWindow, ThreeDayStatisticsWindow);
        IsSevenDayStatisticsWindowSelected = ReferenceEquals(selectedWindow, SevenDayStatisticsWindow);
        IsFourteenDayStatisticsWindowSelected = ReferenceEquals(selectedWindow, FourteenDayStatisticsWindow);
        IsThirtyDayStatisticsWindowSelected = ReferenceEquals(selectedWindow, ThirtyDayStatisticsWindow);
        IsNinetyDayStatisticsWindowSelected = ReferenceEquals(selectedWindow, NinetyDayStatisticsWindow);
    }

    /// <summary>
    /// Gets the currently selected statistics window.
    /// </summary>
    /// <returns>The currently selected statistics window.</returns>
    private StatisticsWindowSelectionItem GetSelectedStatisticsWindow()
    {
        if (IsThreeDayStatisticsWindowSelected)
        {
            return ThreeDayStatisticsWindow;
        }

        if (IsSevenDayStatisticsWindowSelected)
        {
            return SevenDayStatisticsWindow;
        }

        if (IsFourteenDayStatisticsWindowSelected)
        {
            return FourteenDayStatisticsWindow;
        }

        if (IsThirtyDayStatisticsWindowSelected)
        {
            return ThirtyDayStatisticsWindow;
        }

        if (IsNinetyDayStatisticsWindowSelected)
        {
            return NinetyDayStatisticsWindow;
        }

        return TwentyFourHourStatisticsWindow;
    }

    /// <summary>
    /// Builds local history availability information for a statistics result.
    /// </summary>
    /// <param name="result">The statistics result.</param>
    /// <param name="selectedWindow">The selected statistics window.</param>
    /// <returns>The statistics history availability information.</returns>
    private static StatisticsHistoryAvailability BuildStatisticsHistoryAvailability(
        GlucoseStatisticsResult result,
        StatisticsWindowSelectionItem selectedWindow)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selectedWindow);

        if (!result.HasData
            || result.FirstReadingAt is null
            || result.LastReadingAt is null
            || result.AnalyzedReadingsCount == 0)
        {
            return new StatisticsHistoryAvailability(
                false,
                "no local history",
                TimeSpan.Zero,
                result.AnalyzedReadingsCount);
        }

        var availableDuration = result.LastReadingAt.Value - result.FirstReadingAt.Value;

        if (availableDuration < TimeSpan.Zero)
        {
            availableDuration = TimeSpan.Zero;
        }

        var coverageRatio = selectedWindow.Duration.TotalMilliseconds <= 0
            ? 0d
            : availableDuration.TotalMilliseconds / selectedWindow.Duration.TotalMilliseconds;

        var hasEnoughData = coverageRatio >= MinimumStatisticsCoverageRatio;

        return new StatisticsHistoryAvailability(
            hasEnoughData,
            FormatAvailableHistoryDuration(availableDuration),
            availableDuration,
            result.AnalyzedReadingsCount);
    }

    /// <summary>
    /// Builds the selected statistics window status text.
    /// </summary>
    /// <param name="selectedWindow">The selected statistics window.</param>
    /// <param name="result">The statistics result.</param>
    /// <param name="availability">The statistics history availability.</param>
    /// <returns>The selected statistics window status text.</returns>
    private static string BuildStatisticsWindowStatusText(
        StatisticsWindowSelectionItem selectedWindow,
        GlucoseStatisticsResult result,
        StatisticsHistoryAvailability availability)
    {
        ArgumentNullException.ThrowIfNull(selectedWindow);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(availability);

        if (!result.HasData)
        {
            return $"{selectedWindow.Label} selected · no local readings yet";
        }

        return availability.HasEnoughData
            ? $"{selectedWindow.Label} selected · {result.AnalyzedReadingsCount} readings analyzed"
            : $"{selectedWindow.Label} selected · {availability.AvailablePeriodText} available locally";
    }

    /// <summary>
    /// Builds the insufficient local statistics history message.
    /// </summary>
    /// <param name="selectedWindow">The selected statistics window.</param>
    /// <param name="availability">The statistics history availability.</param>
    /// <returns>The insufficient local statistics history message.</returns>
    private static string BuildInsufficientStatisticsHistoryMessage(
        StatisticsWindowSelectionItem selectedWindow,
        StatisticsHistoryAvailability availability)
    {
        ArgumentNullException.ThrowIfNull(selectedWindow);
        ArgumentNullException.ThrowIfNull(availability);

        if (availability.AnalyzedReadingsCount == 0)
        {
            return $"GlucoDesk has not saved local readings for the selected {selectedWindow.Label} window yet. Keep the app running and refresh the dashboard to build local insights.";
        }

        return $"The selected {selectedWindow.Label} window needs about {selectedWindow.Description} of local history. GlucoDesk currently has {availability.AvailablePeriodText} saved locally. Keep the app running to unlock this insight automatically.";
    }

    /// <summary>
    /// Formats the available local history duration.
    /// </summary>
    /// <param name="duration">The available duration.</param>
    /// <returns>The formatted available duration.</returns>
    private static string FormatAvailableHistoryDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 2d)
        {
            return $"{Math.Floor(duration.TotalDays):0} days";
        }

        if (duration.TotalDays >= 1d)
        {
            return "1 day";
        }

        if (duration.TotalHours >= 2d)
        {
            return $"{Math.Floor(duration.TotalHours):0} hours";
        }

        if (duration.TotalHours >= 1d)
        {
            return "1 hour";
        }

        if (duration.TotalMinutes >= 1d)
        {
            return $"{Math.Max(1d, Math.Floor(duration.TotalMinutes)):0} minutes";
        }

        return "less than 1 minute";
    }

    private sealed record StatisticsHistoryAvailability(
        bool HasEnoughData,
        string AvailablePeriodText,
        TimeSpan AvailableDuration,
        int AnalyzedReadingsCount);

    /// <summary>
    /// Builds the dashboard history status text from a detailed history save result.
    /// </summary>
    /// <param name="result">The history save result.</param>
    /// <returns>The dashboard history status text.</returns>
    private static string BuildHistoryStatusText(GlucoseHistorySaveResult result)
    {
        if (result.IncomingReadingsCount == 0)
        {
            return $"History updated: no incoming reading(s), {result.StoredReadingsCount} stored.";
        }

        if (result.AddedReadingsCount == 0)
        {
            return $"History updated: 0 new reading(s), {result.DuplicateReadingsCount} duplicate(s), {result.StoredReadingsCount} stored.";
        }

        return $"History updated: {result.AddedReadingsCount} new reading(s), {result.DuplicateReadingsCount} duplicate(s), {result.StoredReadingsCount} stored.";
    }

    /// <summary>
    /// Applies the data health presentation to the dashboard.
    /// </summary>
    /// <param name="providerKind">The active provider kind.</param>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <param name="readingCount">The number of recent readings.</param>
    private void ApplyDataHealth(
        CgmProviderKind providerKind,
        GlucoseDataFreshness freshness,
        int readingCount)
    {
        var presentation = DashboardDataHealthPresenter.Present(
            providerKind,
            freshness,
            readingCount,
            false,
            null);

        ApplyDataHealthPresentation(presentation);
    }

    /// <summary>
    /// Applies the provider error data health presentation to the dashboard.
    /// </summary>
    /// <param name="providerErrorMessage">The provider error message.</param>
    private void ApplyProviderErrorDataHealth(string? providerErrorMessage)
    {
        var presentation = DashboardDataHealthPresenter.PresentProviderError(providerErrorMessage);

        ApplyDataHealthPresentation(presentation);
    }

    /// <summary>
    /// Applies a data health presentation to the dashboard.
    /// </summary>
    /// <param name="presentation">The data health presentation.</param>
    private void ApplyDataHealthPresentation(DashboardDataHealthPresentation presentation)
    {
        DataHealthTitle = presentation.Title;
        DataHealthMessage = presentation.Message;
        DataHealthBadgeText = presentation.BadgeText;
        IsDataStale = presentation.IsDataStale;
        IsDataUnavailable = presentation.IsDataUnavailable;
        IsShowingRealProviderData = presentation.IsShowingRealProviderData;
    }

    /// <summary>
    /// Handles application settings change notifications.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The settings changed event arguments.</param>
    private void OnSettingsChanged(
        object? sender,
        ApplicationSettingsChangedEventArgs eventArgs)
    {
        ApplySettings(eventArgs.Settings);

        if (_lastDashboardSnapshot is not null)
        {
            ApplySnapshot(_lastDashboardSnapshot);
        }

        SettingsStatusText = "Settings updated";
    }

    /// <summary>
    /// Applies application settings to the dashboard view model.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    private void ApplySettings(ApplicationSettings settings)
    {
        _currentSettings = settings;
        _autoRefreshInterval = settings.DashboardRefreshInterval;
        OnPropertyChanged(nameof(AutoRefreshInterval));
    
        PreferredUnit = NormalizeDisplayUnit(settings.PreferredUnit);
        TargetLowMgDl = settings.TargetLowMgDl;
        TargetHighMgDl = settings.TargetHighMgDl;
        ChartMaximumMgDl = NormalizeChartMaximumMgDl(settings.ChartMaximumMgDl);

        if (!settings.GlucoseAlertsEnabled)
        {
            ClearGlucoseAlertBanner();
        }

        TargetRangeText = FormatTargetRangeText(
            TargetLowMgDl,
            TargetHighMgDl,
            PreferredUnit);
    
        AutoRefreshStatusText = $"Auto-refresh every {FormatInterval(_autoRefreshInterval)}";
        SettingsStatusText = "Settings loaded";
    }

    /// <summary>
    /// Applies fallback dashboard settings when local settings cannot be loaded.
    /// </summary>
    /// <param name="result">The failed settings result.</param>
    private void ApplySettingsFallback(Result<ApplicationSettings> result)
    {
        _autoRefreshInterval = _refreshOptions.AutoRefreshInterval;
        OnPropertyChanged(nameof(AutoRefreshInterval));

        _currentSettings = ApplicationSettings.Default;
        PreferredUnit = GlucoseUnit.MgDl;
        TargetLowMgDl = 70m;
        TargetHighMgDl = 180m;
        ChartMaximumMgDl = 300;
        ClearGlucoseAlertBanner();
        TargetRangeText = FormatTargetRangeText(
            TargetLowMgDl,
            TargetHighMgDl,
            PreferredUnit);

        AutoRefreshStatusText = $"Auto-refresh every {FormatInterval(_autoRefreshInterval)}";
        SettingsStatusText = $"Using default settings · {result.Error.Code}";
    }

    /// <summary>
    /// Applies a successful dashboard snapshot to the view model.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    private void ApplySnapshot(GlucoseDashboardSnapshot snapshot)
    {
        _lastDashboardSnapshot = snapshot;

        var targetRange = CreateTargetRange();

        ProviderDisplayName = snapshot.Metadata.DisplayName;
        DataSourceStatusText = BuildDataSourceStatusText(snapshot);
        LatestValueText = snapshot.LatestReading is null
            ? "—"
            : FormatGlucoseValueText(snapshot.LatestReading.Value, PreferredUnit);

        ApplyProviderStatus(
            snapshot.Metadata.ProviderKind,
            snapshot.LatestReading?.Freshness ?? snapshot.Metadata.ExpectedFreshness);

        ApplyDataHealth(
            snapshot.Metadata.ProviderKind,
            snapshot.LatestReading?.Freshness ?? snapshot.Metadata.ExpectedFreshness,
            snapshot.RecentReadings.Count);

        TrendText = snapshot.LatestReading is null
            ? "No trend"
            : FormatTrend(snapshot.LatestReading.Trend);

        FreshnessText = snapshot.LatestReading is null
            ? FormatFreshness(snapshot.Metadata.ExpectedFreshness)
            : FormatFreshness(snapshot.LatestReading.Freshness);

        LastUpdatedText = snapshot.LatestReading is null
            ? "No reading available"
            : FormatTimestamp(snapshot.LatestReading.Timestamp);

        StatusText = snapshot.LatestReading is null
            ? "No glucose reading available"
            : FormatStatus(snapshot.LatestReading.GetStatus(targetRange), snapshot.IsLatestReadingStale);

        RecentReadingsCountText = $"{snapshot.RecentReadings.Count} readings";

        DashboardContextText = BuildDashboardContextText(
            snapshot.Metadata.DisplayName,
            FreshnessText,
            RecentReadingsCountText,
            LastUpdatedText);

        UpdateChart(snapshot.RecentReadings, targetRange);
        ApplyGlucoseAlert(snapshot);
    }

    /// <summary>
    /// Applies the glucose awareness alert state for the latest dashboard reading.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    private void ApplyGlucoseAlert(GlucoseDashboardSnapshot snapshot)
    {
        if (snapshot.LatestReading is null || snapshot.IsLatestReadingStale)
        {
            ClearGlucoseAlertBanner();
            return;
        }

        var glucoseMgDl = ConvertGlucoseValueToMgDl(snapshot.LatestReading.Value);
        var presentation = _glucoseAlertCoordinator.Evaluate(
            glucoseMgDl,
            _currentSettings,
            PreferredUnit);

        _currentGlucoseAlertKind = presentation.Kind;

        if (presentation.Kind == GlucoseAlertKind.None)
        {
            _glucoseAlertSnoozeState.Clear();
            _glucoseAlertStabilityGate.Reset();
            GlucoseAlertSnoozeStatusText = string.Empty;
            ClearGlucoseAlertBanner();
            return;
        }

        if (!_glucoseAlertStabilityGate.ShouldPresent(presentation.Kind))
        {
            ClearGlucoseAlertBanner();
            return;
        }

        var now = DateTimeOffset.Now;

        if (_glucoseAlertSnoozeState.IsSnoozed(presentation.Kind, now))
        {
            GlucoseAlertSnoozeStatusText = BuildGlucoseAlertSnoozeStatusText(
                _glucoseAlertSnoozeState.GetRemaining(presentation.Kind, now));

            ClearGlucoseAlertBanner();
            return;
        }

        GlucoseAlertSnoozeStatusText = string.Empty;

        GlucoseAlertTitle = presentation.Title;
        GlucoseAlertMessage = presentation.Message;
        GlucoseAlertBadgeText = presentation.BadgeText;
        GlucoseAlertActionText = presentation.ActionText;
        IsGlucoseAlertBannerVisible = _dismissedGlucoseAlertKind != presentation.Kind;

        if (presentation.ShouldSendNativeNotification)
        {
            _ = SendNativeGlucoseAlertNotificationAsync(presentation);
        }
    }

    /// <summary>
    /// Clears the glucose awareness alert banner.
    /// </summary>
    private void ClearGlucoseAlertBanner()
    {
        
        GlucoseAlertSnoozeStatusText = string.Empty;
_currentGlucoseAlertKind = GlucoseAlertKind.None;
        _dismissedGlucoseAlertKind = GlucoseAlertKind.None;
        IsGlucoseAlertBannerVisible = false;
        GlucoseAlertTitle = string.Empty;
        GlucoseAlertMessage = string.Empty;
        GlucoseAlertBadgeText = string.Empty;
    }

    /// <summary>
    /// Sends a native glucose awareness notification without breaking dashboard refresh.
    /// </summary>
    /// <param name="presentation">The alert presentation to send.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    private async Task SendNativeGlucoseAlertNotificationAsync(GlucoseAlertPresentation presentation)
    {
        try
        {
            await _glucoseAlertCoordinator
                .SendNativeNotificationAsync(presentation, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // Native notifications are best-effort and must never break dashboard refresh.
        }
    }

    /// <summary>
    /// Builds the user-facing snooze status text.
    /// </summary>
    /// <param name="remaining">The remaining snooze duration.</param>
    /// <returns>The snooze status text.</returns>
    private static string BuildGlucoseAlertSnoozeStatusText(TimeSpan remaining)
    {
        var totalMinutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));

        return totalMinutes == 1
            ? "Alert snoozed for less than 1 minute."
            : $"Alert snoozed for about {totalMinutes} minutes.";
    }

    /// <summary>
    /// Converts a glucose value to mg/dL.
    /// </summary>
    /// <param name="value">The glucose value.</param>
    /// <returns>The glucose amount expressed in mg/dL.</returns>
    private static decimal ConvertGlucoseValueToMgDl(GlucoseValue value)
    {
        return value.Unit == GlucoseUnit.MgDl
            ? value.Amount
            : value.ConvertTo(GlucoseUnit.MgDl).Amount;
    }

    /// <summary>
    /// Applies the provider status presentation to the dashboard.
    /// </summary>
    /// <param name="providerKind">The active provider kind.</param>
    /// <param name="freshness">The glucose data freshness.</param>
    private void ApplyProviderStatus(
        CgmProviderKind providerKind,
        GlucoseDataFreshness freshness)
    {
        var presentation = DashboardProviderStatusPresenter.Present(providerKind, freshness);

        ProviderStatusTitle = presentation.Title;
        ProviderStatusMessage = presentation.Message;
        ProviderStatusBadgeText = presentation.BadgeText;
        IsRealProviderActive = presentation.IsRealProvider;
        IsMockProviderActive = presentation.IsMockProvider;
    }

    /// <summary>
    /// Builds a data source status message for a successful dashboard snapshot.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <returns>The data source status message.</returns>
    private static string BuildDataSourceStatusText(GlucoseDashboardSnapshot snapshot)
    {
        var freshnessText = snapshot.LatestReading is null
            ? FormatFreshness(snapshot.Metadata.ExpectedFreshness)
            : FormatFreshness(snapshot.LatestReading.Freshness);

        if (snapshot.LatestReading is null)
        {
            return $"{snapshot.Metadata.DisplayName} returned no current glucose reading.";
        }

        return snapshot.IsLatestReadingStale
            ? $"{snapshot.Metadata.DisplayName} returned {freshnessText.ToLowerInvariant()} stale data."
            : $"{snapshot.Metadata.DisplayName} returned {freshnessText.ToLowerInvariant()} data.";
    }

    /// <summary>
    /// Persists dashboard snapshot readings into local glucose history.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task PersistSnapshotToHistoryAsync(
        GlucoseDashboardSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        if (_glucoseHistoryService is null)
        {
            HistoryStatusText = "History disabled";
            return;
        }

        var readingsToPersist = BuildReadingsToPersist(snapshot);

        if (readingsToPersist.Count == 0)
        {
            HistoryStatusText = "No readings to cache";
            return;
        }

        var result = await _glucoseHistoryService
            .SaveReadingsWithSummaryAsync(readingsToPersist, cancellationToken)
            .ConfigureAwait(false);

        HistoryStatusText = result.IsSuccess
            ? BuildHistoryStatusText(result.Value)
            : $"History update failed · {result.Error.Code}";
    }

    /// <summary>
    /// Builds the list of readings to persist from a dashboard snapshot.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <returns>The readings to persist.</returns>
    private static IReadOnlyCollection<GlucoseReading> BuildReadingsToPersist(
        GlucoseDashboardSnapshot snapshot)
    {
        var readings = snapshot.RecentReadings
            .ToList();

        if (snapshot.LatestReading is not null)
        {
            readings.Add(snapshot.LatestReading);
        }

        return readings
            .GroupBy(BuildReadingIdentityKey, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(reading => reading.Timestamp)
            .ToArray();
    }

    /// <summary>
    /// Builds a stable identity key for a glucose reading.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <returns>The reading identity key.</returns>
    private static string BuildReadingIdentityKey(GlucoseReading reading)
    {
        return string.Join(
            "|",
            reading.Timestamp.ToUniversalTime().Ticks,
            reading.Provider);
    }

    /// <summary>
    /// Applies a failed dashboard refresh result to the view model using a user-facing presentation.
    /// </summary>
    /// <param name="result">The failed result.</param>
    private void ApplyFailure(Result<GlucoseDashboardSnapshot> result)
    {
        var presentation = DashboardRefreshErrorPresenter.Present(result.Error);

        HasError = true;
        ErrorMessage = presentation.FullMessage;
        StatusText = presentation.StatusText;
        DataSourceStatusText = presentation.Message;

        ApplyProviderErrorDataHealth(DataSourceStatusText);
    }

    /// <summary>
    /// Applies an unexpected exception to the view model.
    /// </summary>
    /// <param name="exception">The unexpected exception.</param>
    private void ApplyUnexpectedFailure(Exception exception)
    {
        HasError = true;
        ErrorMessage = exception.Message;
        StatusText = "Unexpected error";
        DataSourceStatusText = "An unexpected dashboard error occurred.";

        ApplyProviderErrorDataHealth("An unexpected dashboard error occurred.");
    }

    /// <summary>
    /// Updates the dashboard chart using the recent readings included in the snapshot.
    /// </summary>
    /// <param name="readings">The recent glucose readings.</param>
    /// <param name="targetRange">The active glucose target range.</param>
    private void UpdateChart(
        IReadOnlyCollection<GlucoseReading> readings,
        GlucoseRange targetRange)
    {
        _allChartPoints = readings
            .OrderBy(reading => reading.Timestamp)
            .Select(reading => CreateChartPoint(reading, targetRange))
            .ToArray();

        ApplySelectedChartWindowToChart();
    }

    /// <summary>
    /// Selects the chart time window and updates the visible chart points.
    /// </summary>
    /// <param name="windowHours">The selected chart window in hours.</param>
    private void SelectChartWindow(int windowHours)
    {
        SelectedChartWindowHours = NormalizeChartWindowHours(windowHours);
        ApplyChartWindowSelectionState(SelectedChartWindowHours);
        ApplySelectedChartWindowToChart();
    }

    /// <summary>
    /// Applies the currently selected chart window to the visible chart points.
    /// </summary>
    private void ApplySelectedChartWindowToChart()
    {
        var filteredChartPoints = FilterChartPointsByWindow(
            _allChartPoints,
            SelectedChartWindowHours);  

        ChartPoints = filteredChartPoints;
        ChartSummaryText = BuildChartSummary(
            filteredChartPoints,
            SelectedChartWindowHours,
            PreferredUnit);
    }

    /// <summary>
    /// Applies the selected chart window visual state.
    /// </summary>
    /// <param name="windowHours">The selected chart window in hours.</param>
    private void ApplyChartWindowSelectionState(int windowHours)
    {
        IsThreeHourChartWindowSelected = windowHours == ThreeHourChartWindow;
        IsSixHourChartWindowSelected = windowHours == SixHourChartWindow;
        IsTwelveHourChartWindowSelected = windowHours == TwelveHourChartWindow;
        IsTwentyFourHourChartWindowSelected = windowHours == TwentyFourHourChartWindow;
    }

    /// <summary>
    /// Filters chart points using the selected time window.
    /// </summary>
    /// <param name="chartPoints">The full chart point collection.</param>
    /// <param name="windowHours">The selected chart window in hours.</param>
    /// <returns>The filtered chart points.</returns>
    private static IReadOnlyList<GlucoseChartPoint> FilterChartPointsByWindow(
        IReadOnlyCollection<GlucoseChartPoint> chartPoints,
        int windowHours)
    {
        if (chartPoints.Count == 0)
        {
            return [];
        }

        var normalizedWindowHours = NormalizeChartWindowHours(windowHours);
        var latestTimestamp = chartPoints.Max(point => point.Timestamp);
        var minimumTimestamp = latestTimestamp.AddHours(-normalizedWindowHours);

        return chartPoints
            .Where(point => point.Timestamp >= minimumTimestamp)
            .OrderBy(point => point.Timestamp)
            .ToArray();
    }

    /// <summary>
    /// Normalizes the selected chart window to one of the supported values.
    /// </summary>
    /// <param name="windowHours">The requested chart window in hours.</param>
    /// <returns>The normalized chart window in hours.</returns>
    private static int NormalizeChartWindowHours(int windowHours)
    {
        return windowHours switch
        {
            ThreeHourChartWindow => ThreeHourChartWindow,
            SixHourChartWindow => SixHourChartWindow,
            TwelveHourChartWindow => TwelveHourChartWindow,
            TwentyFourHourChartWindow => TwentyFourHourChartWindow,
            _ => ThreeHourChartWindow
        };
    }

    /// <summary>
    /// Creates a chart point from a glucose reading.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <param name="targetRange">The active glucose target range.</param>
    /// <returns>The chart point.</returns>
    private static GlucoseChartPoint CreateChartPoint(
        GlucoseReading reading,
        GlucoseRange targetRange)
    {
        var normalizedValue = reading.Value.Unit == GlucoseUnit.MgDl
            ? reading.Value
            : reading.Value.ConvertTo(GlucoseUnit.MgDl);

        return new GlucoseChartPoint(
            reading.Timestamp,
            normalizedValue.Amount,
            reading.GetStatus(targetRange));
    }

    /// <summary>
    /// Creates the active dashboard target range.
    /// </summary>
    /// <returns>The active glucose target range.</returns>
    private GlucoseRange CreateTargetRange()
    {
        return new GlucoseRange(
            new GlucoseValue(TargetLowMgDl, GlucoseUnit.MgDl),
            new GlucoseValue(TargetHighMgDl, GlucoseUnit.MgDl));
    }

    /// <summary>
    /// Builds a display-friendly chart summary.
    /// </summary>
    /// <param name="chartPoints">The chart points.</param>
    /// <param name="windowHours">The selected chart window in hours.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The chart summary.</returns>
    private static string BuildChartSummary(
        IReadOnlyCollection<GlucoseChartPoint> chartPoints,
        int windowHours,
        GlucoseUnit displayUnit)
    {
        var normalizedWindowHours = NormalizeChartWindowHours(windowHours);
        var normalizedDisplayUnit = NormalizeDisplayUnit(displayUnit);

        if (chartPoints.Count == 0)
        {
            return $"Last {normalizedWindowHours}H · no chart data";
        }

        var minimumValue = chartPoints.Min(point => point.ValueMgDl);
        var maximumValue = chartPoints.Max(point => point.ValueMgDl);

        return $"Last {normalizedWindowHours}H · {chartPoints.Count} readings · {FormatGlucoseValueLabel(minimumValue, normalizedDisplayUnit)}-{FormatGlucoseValueLabel(maximumValue, normalizedDisplayUnit)} {FormatGlucoseUnitLabel(normalizedDisplayUnit)}";
    }

    /// <summary>
    /// Normalizes unsupported glucose display units to the default display unit.
    /// </summary>
    /// <param name="displayUnit">The requested display unit.</param>
    /// <returns>The normalized display unit.</returns>
    private static GlucoseUnit NormalizeDisplayUnit(GlucoseUnit displayUnit)
    {
        return Enum.IsDefined(displayUnit)
            ? displayUnit
            : GlucoseUnit.MgDl;
    }

    /// <summary>
    /// Normalizes supported chart maximum values.
    /// </summary>
    /// <param name="chartMaximumMgDl">The requested chart maximum value.</param>
    /// <returns>The normalized chart maximum value.</returns>
    private static int NormalizeChartMaximumMgDl(int chartMaximumMgDl)
    {
        return chartMaximumMgDl is 400
            ? 400
            : 300;
    }

    /// <summary>
    /// Formats the configured target range using the selected display unit.
    /// </summary>
    /// <param name="targetLowMgDl">The lower target value expressed in mg/dL.</param>
    /// <param name="targetHighMgDl">The upper target value expressed in mg/dL.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted target range text.</returns>
    private static string FormatTargetRangeText(
        decimal targetLowMgDl,
        decimal targetHighMgDl,
        GlucoseUnit displayUnit)
    {
        var normalizedDisplayUnit = NormalizeDisplayUnit(displayUnit);

        return $"Target range: {FormatGlucoseValueLabel(targetLowMgDl, normalizedDisplayUnit)}-{FormatGlucoseValueLabel(targetHighMgDl, normalizedDisplayUnit)} {FormatGlucoseUnitLabel(normalizedDisplayUnit)}";
    }

    /// <summary>
    /// Formats a glucose value using the selected display unit.
    /// </summary>
    /// <param name="value">The glucose value.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted glucose value text.</returns>
    private static string FormatGlucoseValueText(
        GlucoseValue value,
        GlucoseUnit displayUnit)
    {
        var normalizedDisplayUnit = NormalizeDisplayUnit(displayUnit);
        var convertedValue = value.Unit == normalizedDisplayUnit
            ? value
            : value.ConvertTo(normalizedDisplayUnit);

        return $"{FormatGlucoseAmount(convertedValue.Amount, normalizedDisplayUnit)} {FormatGlucoseUnitLabel(normalizedDisplayUnit)}";
    }

    /// <summary>
    /// Formats a glucose value stored in mg/dL using the selected display unit.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted glucose value without unit suffix.</returns>
    private static string FormatGlucoseValueLabel(
        decimal valueMgDl,
        GlucoseUnit displayUnit)
    {
        var normalizedDisplayUnit = NormalizeDisplayUnit(displayUnit);
        var convertedValue = new GlucoseValue(valueMgDl, GlucoseUnit.MgDl)
            .ConvertTo(normalizedDisplayUnit);

        return FormatGlucoseAmount(convertedValue.Amount, normalizedDisplayUnit);
    }

    /// <summary>
    /// Formats a glucose amount using the selected display unit.
    /// </summary>
    /// <param name="amount">The glucose amount.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted glucose amount.</returns>
    private static string FormatGlucoseAmount(
        decimal amount,
        GlucoseUnit displayUnit)
    {
        return displayUnit switch
        {
            GlucoseUnit.MgDl => amount.ToString("0", CultureInfo.InvariantCulture),
            GlucoseUnit.MmolL => amount.ToString("0.0", CultureInfo.InvariantCulture),
            _ => amount.ToString("0", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Formats glucose unit labels for dashboard text.
    /// </summary>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted unit label.</returns>
    private static string FormatGlucoseUnitLabel(GlucoseUnit displayUnit)
    {
        return displayUnit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            GlucoseUnit.MmolL => "mmol/L",
            _ => "mg/dL"
        };
    }

    /// <summary>
    /// Formats a glucose trend direction for display.
    /// </summary>
    /// <param name="trend">The glucose trend direction.</param>
    /// <returns>The display-friendly trend text.</returns>
    private static string FormatTrend(TrendDirection trend)
    {
        return trend switch
        {
            TrendDirection.DoubleUp => "↑↑ Rising very fast",
            TrendDirection.SingleUp => "↑ Rising fast",
            TrendDirection.FortyFiveUp => "↗ Rising",
            TrendDirection.Flat => "→ Stable",
            TrendDirection.FortyFiveDown => "↘ Falling",
            TrendDirection.SingleDown => "↓ Falling fast",
            TrendDirection.DoubleDown => "↓↓ Falling very fast",
            TrendDirection.NotComputable => "Not computable",
            TrendDirection.RateOutOfRange => "Rate out of range",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Formats glucose data freshness for display.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The display-friendly freshness text.</returns>
    private static string FormatFreshness(GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.Live => "Live",
            GlucoseDataFreshness.NearRealTime => "Near real-time",
            GlucoseDataFreshness.Delayed => "Delayed",
            GlucoseDataFreshness.Historical => "Historical",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Formats a timestamp for display.
    /// </summary>
    /// <param name="timestamp">The timestamp to format.</param>
    /// <returns>The display-friendly timestamp.</returns>
    private static string FormatTimestamp(DateTimeOffset timestamp)
    {
        return timestamp
            .ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm:ss");
    }

    /// <summary>
    /// Formats the glucose status for display.
    /// </summary>
    /// <param name="status">The glucose status.</param>
    /// <param name="isStale">Whether the latest reading is stale.</param>
    /// <returns>The display-friendly glucose status.</returns>
    private static string FormatStatus(GlucoseStatus status, bool isStale)
    {
        var statusText = status switch
        {
            GlucoseStatus.Low => "Low",
            GlucoseStatus.InRange => "In range",
            GlucoseStatus.High => "High",
            _ => "Unknown"
        };

        return isStale
            ? $"{statusText} · stale data"
            : statusText;
    }

    /// <summary>
    /// Formats the automatic refresh interval for display.
    /// </summary>
    /// <param name="interval">The automatic refresh interval.</param>
    /// <returns>The display-friendly interval.</returns>
    private static string FormatInterval(TimeSpan interval)
    {
        if (interval.TotalMinutes >= 1)
        {
            return $"{interval.TotalMinutes:0.#} minute(s)";
        }

        return $"{interval.TotalSeconds:0.#} second(s)";
    }

    #endregion
}
