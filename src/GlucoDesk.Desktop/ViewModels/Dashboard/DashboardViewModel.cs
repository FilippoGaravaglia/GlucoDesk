using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Common;

namespace GlucoDesk.Desktop.ViewModels.Dashboard;

/// <summary>
/// Represents the dashboard view model used by the desktop shell.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IGlucoseDataService _glucoseDataService;

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
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    public DashboardViewModel(IGlucoseDataService glucoseDataService)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);

        _glucoseDataService = glucoseDataService;
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
                .GetDashboardSnapshotAsync(GlucoseDashboardRequest.Default, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result);
                return;
            }

            ApplySnapshot(result.Value);
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
    /// Applies a successful dashboard snapshot to the view model.
    /// </summary>
    /// <param name="snapshot">The dashboard snapshot.</param>
    private void ApplySnapshot(GlucoseDashboardSnapshot snapshot)
    {
        ProviderDisplayName = snapshot.Metadata.DisplayName;
        LatestValueText = snapshot.LatestReading?.Value.ToString() ?? "—";
        TrendText = snapshot.LatestReading is null ? "No trend" : FormatTrend(snapshot.LatestReading.Trend);
        FreshnessText = snapshot.LatestReading is null
            ? FormatFreshness(snapshot.Metadata.ExpectedFreshness)
            : FormatFreshness(snapshot.LatestReading.Freshness);

        LastUpdatedText = snapshot.LatestReading is null
            ? "No reading available"
            : FormatTimestamp(snapshot.LatestReading.Timestamp);

        StatusText = snapshot.LatestReading is null
            ? "No glucose reading available"
            : FormatStatus(snapshot.LatestReading.GetStatus(GlucoseRange.StandardMgDl), snapshot.IsLatestReadingStale);

        RecentReadingsCountText = $"{snapshot.RecentReadings.Count} readings";
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

    #endregion
}