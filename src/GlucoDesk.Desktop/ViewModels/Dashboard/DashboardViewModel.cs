using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;

namespace GlucoDesk.Desktop.ViewModels.Dashboard;

/// <summary>
/// Represents the dashboard view model used by the desktop shell.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IGlucoseDataService _glucoseDataService;
    private readonly IApplicationSettingsService _settingsService;
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
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="refreshOptions">The optional dashboard refresh fallback options.</param>
    public DashboardViewModel(
        IGlucoseDataService glucoseDataService,
        IApplicationSettingsService settingsService,
        DashboardRefreshOptions? refreshOptions = null)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);
        ArgumentNullException.ThrowIfNull(settingsService);

        _glucoseDataService = glucoseDataService;
        _settingsService = settingsService;
        _refreshOptions = refreshOptions ?? DashboardRefreshOptions.Default;
        _autoRefreshInterval = _refreshOptions.AutoRefreshInterval;

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

    #region Helpers

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
        LatestValueText = snapshot.LatestReading?.Value.ToString() ?? "—";

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
    /// Applies a failed result to the view model.
    /// </summary>
    /// <param name="result">The failed result.</param>
    private void ApplyFailure(Result<GlucoseDashboardSnapshot> result)
    {
        HasError = true;
        ErrorMessage = $"{result.Error.Code}: {result.Error.Message}";
        StatusText = "Unable to refresh glucose data";
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