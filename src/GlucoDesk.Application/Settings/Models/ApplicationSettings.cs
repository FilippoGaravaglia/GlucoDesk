using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Settings.Models;

/// <summary>
/// Represents user-configurable GlucoDesk application settings.
/// </summary>
public sealed record ApplicationSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
    /// </summary>
    /// <param name="activeLiveProvider">The active live CGM provider.</param>
    /// <param name="historicalProvider">The active historical CGM provider.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <param name="targetLowMgDl">The lower glucose target expressed in mg/dL.</param>
    /// <param name="targetHighMgDl">The upper glucose target expressed in mg/dL.</param>
    /// <param name="dashboardRefreshInterval">The dashboard refresh interval.</param>
    /// <param name="chartMaximumMgDl">The maximum chart value expressed in mg/dL.</param>
    /// <exception cref="ArgumentException">Thrown when provider or unit values are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when target range, refresh interval or chart maximum values are invalid.</exception>
    public ApplicationSettings(
        CgmProviderKind activeLiveProvider = CgmProviderKind.Mock,
        CgmProviderKind historicalProvider = CgmProviderKind.Mock,
        GlucoseUnit preferredUnit = GlucoseUnit.MgDl,
        int targetLowMgDl = 70,
        int targetHighMgDl = 180,
        TimeSpan? dashboardRefreshInterval = null,
        int chartMaximumMgDl = 300)
    {
        var effectiveDashboardRefreshInterval = dashboardRefreshInterval ?? TimeSpan.FromSeconds(30);

        if (activeLiveProvider == CgmProviderKind.Unknown)
        {
            throw new ArgumentException("Active live provider must be specified.", nameof(activeLiveProvider));
        }

        if (historicalProvider == CgmProviderKind.Unknown)
        {
            throw new ArgumentException("Historical provider must be specified.", nameof(historicalProvider));
        }

        if (!Enum.IsDefined(preferredUnit))
        {
            throw new ArgumentException("Preferred glucose unit is not valid.", nameof(preferredUnit));
        }

        if (targetLowMgDl <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetLowMgDl),
                targetLowMgDl,
                "Target low value must be greater than zero.");
        }

        if (targetHighMgDl <= targetLowMgDl)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetHighMgDl),
                targetHighMgDl,
                "Target high value must be greater than target low value.");
        }

        if (effectiveDashboardRefreshInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dashboardRefreshInterval),
                dashboardRefreshInterval,
                "Dashboard refresh interval must be greater than zero.");
        }

        if (chartMaximumMgDl is not 300 and not 400)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chartMaximumMgDl),
                chartMaximumMgDl,
                "Chart maximum value must be either 300 or 400 mg/dL.");
        }

        ActiveLiveProvider = activeLiveProvider;
        HistoricalProvider = historicalProvider;
        PreferredUnit = preferredUnit;
        TargetLowMgDl = targetLowMgDl;
        TargetHighMgDl = targetHighMgDl;
        DashboardRefreshInterval = effectiveDashboardRefreshInterval;
        ChartMaximumMgDl = chartMaximumMgDl;
    }

    /// <summary>
    /// Gets the default application settings.
    /// </summary>
    public static ApplicationSettings Default { get; } = new();

    /// <summary>
    /// Gets the active live CGM provider.
    /// </summary>
    public CgmProviderKind ActiveLiveProvider { get; }

    /// <summary>
    /// Gets the active historical CGM provider.
    /// </summary>
    public CgmProviderKind HistoricalProvider { get; }

    /// <summary>
    /// Gets the preferred glucose display unit.
    /// </summary>
    public GlucoseUnit PreferredUnit { get; }

    /// <summary>
    /// Gets the lower glucose target expressed in mg/dL.
    /// </summary>
    public int TargetLowMgDl { get; }

    /// <summary>
    /// Gets the upper glucose target expressed in mg/dL.
    /// </summary>
    public int TargetHighMgDl { get; }

    /// <summary>
    /// Gets the dashboard refresh interval.
    /// </summary>
    public TimeSpan DashboardRefreshInterval { get; }

    /// <summary>
    /// Gets the maximum chart value expressed in mg/dL.
    /// </summary>
    public int ChartMaximumMgDl { get; }
}