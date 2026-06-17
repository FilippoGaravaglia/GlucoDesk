using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Events;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;
using GlucoDesk.Desktop.ViewModels.Dashboard.Errors;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Dashboard.Providers;
using GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;
using System.Globalization;

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
    private readonly IGlucoseDataService _glucoseDataService;
    private readonly IApplicationSettingsService _settingsService;
    private readonly IApplicationSettingsChangeNotifier? _settingsChangeNotifier;
    private readonly IGlucoseHistoryService? _glucoseHistoryService;
    private readonly IGlucoseStatisticsService? _glucoseStatisticsService;
    private readonly DashboardRefreshOptions _refreshOptions;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="refreshOptions">The optional dashboard refresh fallback options.</param>
    /// <param name="settingsChangeNotifier">The optional application settings change notifier.</param>
    /// <param name="glucoseHistoryService">The optional glucose history service.</param>
    /// <param name="glucoseStatisticsService">The glucose statistics service.</param>
    public DashboardViewModel(
        IGlucoseDataService glucoseDataService,
        IApplicationSettingsService settingsService,
        DashboardRefreshOptions? refreshOptions = null,
        IApplicationSettingsChangeNotifier? settingsChangeNotifier = null,
        IGlucoseHistoryService? glucoseHistoryService = null,
        IGlucoseStatisticsService? glucoseStatisticsService = null)
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
                .GetDashboardSnapshotAsync(CreateDashboardRequest(), cancellationToken);

            if (result.IsFailure)
            {
                ApplyFailure(result);
                return;
            }

            ApplySnapshot(result.Value);
            await PersistSnapshotToHistoryAsync(result.Value, cancellationToken);

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
            ? "Your 24-hour average based on the latest analyzed readings."
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
            >= 90d => "Excellent stability across the selected day.",
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
            ApplyStatisticsPresentation(DashboardStatisticsPresenter.Disabled());
            return;
        }

        IsStatisticsEnabled = true;

        var targetRange = GlucoseStatisticsTargetRange.DefaultMgDl();
        var request = BuildStatisticsRequest(snapshot, targetRange);

        var result = await _glucoseStatisticsService
            .CalculateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            ApplyStatisticsPresentation(DashboardStatisticsPresenter.Failed(result.Error.Code));
            return;
        }

        var presentation = DashboardStatisticsPresenter.Present(result.Value, targetRange);

        ApplyStatisticsPresentation(presentation);
    }

    /// <summary>
    /// Builds the statistics request for the current dashboard snapshot.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    /// <param name="targetRange">The target range.</param>
    /// <returns>The statistics request.</returns>
    private static GlucoseStatisticsRequest BuildStatisticsRequest(
        GlucoseDashboardSnapshot snapshot,
        GlucoseStatisticsTargetRange targetRange)
    {
        var from = snapshot.RecentReadings.Count > 0
            ? snapshot.RecentReadings.Min(reading => reading.Timestamp)
            : snapshot.SnapshotCreatedAt.AddHours(-12);

        var to = snapshot.SnapshotCreatedAt;

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
    }

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
        SettingsStatusText = "Settings updated";
    }

    /// <summary>
    /// Applies application settings to the dashboard view model.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    private void ApplySettings(ApplicationSettings settings)
    {
        _autoRefreshInterval = settings.DashboardRefreshInterval;
        OnPropertyChanged(nameof(AutoRefreshInterval));

        TargetLowMgDl = settings.TargetLowMgDl;
        TargetHighMgDl = settings.TargetHighMgDl;
        TargetRangeText = $"Target range: {settings.TargetLowMgDl}-{settings.TargetHighMgDl} mg/dL";

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

        TargetLowMgDl = 70m;
        TargetHighMgDl = 180m;
        TargetRangeText = "Target range: 70-180 mg/dL";

        AutoRefreshStatusText = $"Auto-refresh every {FormatInterval(_autoRefreshInterval)}";
        SettingsStatusText = $"Using default settings · {result.Error.Code}";
    }

    /// <summary>
    /// Applies a successful dashboard snapshot to the view model.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    private void ApplySnapshot(GlucoseDashboardSnapshot snapshot)
    {
        var targetRange = CreateTargetRange();

        ProviderDisplayName = snapshot.Metadata.DisplayName;
        DataSourceStatusText = BuildDataSourceStatusText(snapshot);
        LatestValueText = snapshot.LatestReading?.Value.ToString() ?? "—";

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
        ChartSummaryText = BuildChartSummary(filteredChartPoints, SelectedChartWindowHours);
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
    /// <returns>The chart summary.</returns>
    private static string BuildChartSummary(
        IReadOnlyCollection<GlucoseChartPoint> chartPoints,
        int windowHours)
    {
        var normalizedWindowHours = NormalizeChartWindowHours(windowHours);

        if (chartPoints.Count == 0)
        {
            return $"Last {normalizedWindowHours}H · no chart data";
        }

        var minimumValue = chartPoints.Min(point => point.ValueMgDl);
        var maximumValue = chartPoints.Max(point => point.ValueMgDl);

        return $"Last {normalizedWindowHours}H · {chartPoints.Count} readings · {minimumValue:0}-{maximumValue:0} mg/dL";
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