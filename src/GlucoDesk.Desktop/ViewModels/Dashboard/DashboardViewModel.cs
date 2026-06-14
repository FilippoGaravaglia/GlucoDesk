using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Events;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Dashboard.Errors;
using GlucoDesk.Desktop.ViewModels.Dashboard.Providers;
using GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;
using GlucoDesk.Application.Cgm.History.Results;

namespace GlucoDesk.Desktop.ViewModels.Dashboard;

/// <summary>
/// Represents the dashboard view model used by the desktop shell.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly IGlucoseDataService _glucoseDataService;
    private readonly IApplicationSettingsService _settingsService;
    private readonly IApplicationSettingsChangeNotifier? _settingsChangeNotifier;
    private readonly IGlucoseHistoryService? _glucoseHistoryService;
    private readonly DashboardRefreshOptions _refreshOptions;

    private bool _isInitialized;
    private TimeSpan _autoRefreshInterval;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="refreshOptions">The optional dashboard refresh fallback options.</param>
    /// <param name="settingsChangeNotifier">The optional application settings change notifier.</param>
    /// <param name="glucoseHistoryService">The optional glucose history service.</param>
    public DashboardViewModel(
        IGlucoseDataService glucoseDataService,
        IApplicationSettingsService settingsService,
        DashboardRefreshOptions? refreshOptions = null,
        IApplicationSettingsChangeNotifier? settingsChangeNotifier = null,
        IGlucoseHistoryService? glucoseHistoryService = null)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);
        ArgumentNullException.ThrowIfNull(settingsService);

        _glucoseDataService = glucoseDataService;
        _settingsService = settingsService;
        _refreshOptions = refreshOptions ?? DashboardRefreshOptions.Default;
        _settingsChangeNotifier = settingsChangeNotifier;
        _glucoseHistoryService = glucoseHistoryService;
        _autoRefreshInterval = _refreshOptions.AutoRefreshInterval;

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
                .GetDashboardSnapshotAsync(GlucoseDashboardRequest.Default, cancellationToken);

            if (result.IsFailure)
            {
                ApplyFailure(result);
                return;
            }

            ApplySnapshot(result.Value);
            await PersistSnapshotToHistoryAsync(result.Value, cancellationToken);

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
        var chartPoints = readings
            .OrderBy(reading => reading.Timestamp)
            .Select(reading => CreateChartPoint(reading, targetRange))
            .ToArray();

        ChartPoints = chartPoints;
        ChartSummaryText = BuildChartSummary(chartPoints);
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
    /// <returns>The chart summary.</returns>
    private static string BuildChartSummary(IReadOnlyCollection<GlucoseChartPoint> chartPoints)
    {
        if (chartPoints.Count == 0)
        {
            return "No chart data";
        }

        var minimumValue = chartPoints.Min(point => point.ValueMgDl);
        var maximumValue = chartPoints.Max(point => point.ValueMgDl);

        return $"{chartPoints.Count} readings · {minimumValue:0}-{maximumValue:0} mg/dL";
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