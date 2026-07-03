using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Settings.Models;

/// <summary>
/// Represents user-configurable GlucoDesk application settings.
/// </summary>
public sealed record ApplicationSettings
{
    /// <summary>
    /// Gets the minimum supported glucose alert repeat interval.
    /// </summary>
    public static readonly TimeSpan MinimumGlucoseAlertRepeatInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the maximum supported glucose alert repeat interval.
    /// </summary>
    public static readonly TimeSpan MaximumGlucoseAlertRepeatInterval = TimeSpan.FromMinutes(180);

    /// <summary>
    /// Gets the default glucose alert repeat interval.
    /// </summary>
    public static readonly TimeSpan DefaultGlucoseAlertRepeatInterval = TimeSpan.FromMinutes(30);

    /// <summary>
    /// The minimum number of consecutive out-of-range readings required before showing a glucose alert.
    /// </summary>
    public const int MinimumGlucoseAlertRequiredConsecutiveReadings = 1;

    /// <summary>
    /// The maximum number of consecutive out-of-range readings required before showing a glucose alert.
    /// </summary>
    public const int MaximumGlucoseAlertRequiredConsecutiveReadings = 5;

    /// <summary>
    /// The default number of consecutive out-of-range readings required before showing a glucose alert.
    /// </summary>
    public const int DefaultGlucoseAlertRequiredConsecutiveReadings = 2;

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
    /// <param name="glucoseAlertsEnabled">A value indicating whether in-app glucose awareness alerts are enabled.</param>
    /// <param name="lowGlucoseAlertsEnabled">A value indicating whether below-target glucose awareness alerts are enabled.</param>
    /// <param name="highGlucoseAlertsEnabled">A value indicating whether above-target glucose awareness alerts are enabled.</param>
    /// <param name="nativeGlucoseNotificationsEnabled">A value indicating whether native OS glucose awareness notifications are enabled.</param>
    /// <param name="glucoseAlertPrivacyModeEnabled">A value indicating whether alert messages should hide glucose values.</param>
    /// <param name="glucoseAlertRepeatInterval">The minimum repeat interval for repeated native notifications of the same condition.</param>
    /// <param name="glucoseAlertRequiredConsecutiveReadings">The number of consecutive out-of-range readings required before showing a glucose alert.</param>
    /// <exception cref="ArgumentException">Thrown when provider or unit values are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when target range, refresh interval, chart maximum or notification interval values are invalid.</exception>
    public ApplicationSettings(
        CgmProviderKind activeLiveProvider = CgmProviderKind.Mock,
        CgmProviderKind historicalProvider = CgmProviderKind.Mock,
        GlucoseUnit preferredUnit = GlucoseUnit.MgDl,
        int targetLowMgDl = 70,
        int targetHighMgDl = 180,
        TimeSpan? dashboardRefreshInterval = null,
        int chartMaximumMgDl = 300,
        bool glucoseAlertsEnabled = true,
        bool lowGlucoseAlertsEnabled = true,
        bool highGlucoseAlertsEnabled = true,
        bool nativeGlucoseNotificationsEnabled = false,
        bool glucoseAlertPrivacyModeEnabled = true,
        TimeSpan? glucoseAlertRepeatInterval = null,
        int glucoseAlertRequiredConsecutiveReadings = DefaultGlucoseAlertRequiredConsecutiveReadings)
    {
        var effectiveDashboardRefreshInterval = dashboardRefreshInterval ?? TimeSpan.FromSeconds(30);
        var effectiveGlucoseAlertRepeatInterval = glucoseAlertRepeatInterval ?? DefaultGlucoseAlertRepeatInterval;

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

        if (effectiveGlucoseAlertRepeatInterval < MinimumGlucoseAlertRepeatInterval ||
            effectiveGlucoseAlertRepeatInterval > MaximumGlucoseAlertRepeatInterval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(glucoseAlertRepeatInterval),
                glucoseAlertRepeatInterval,
                "Glucose alert repeat interval must be between 5 and 180 minutes.");
        }

        ActiveLiveProvider = activeLiveProvider;
        HistoricalProvider = historicalProvider;
        PreferredUnit = preferredUnit;
        TargetLowMgDl = targetLowMgDl;
        TargetHighMgDl = targetHighMgDl;
        DashboardRefreshInterval = effectiveDashboardRefreshInterval;
        ChartMaximumMgDl = chartMaximumMgDl;
        GlucoseAlertsEnabled = glucoseAlertsEnabled;
        LowGlucoseAlertsEnabled = lowGlucoseAlertsEnabled;
        HighGlucoseAlertsEnabled = highGlucoseAlertsEnabled;
        NativeGlucoseNotificationsEnabled = nativeGlucoseNotificationsEnabled;
        GlucoseAlertPrivacyModeEnabled = glucoseAlertPrivacyModeEnabled;
        GlucoseAlertRepeatInterval = effectiveGlucoseAlertRepeatInterval;
    
        GlucoseAlertRequiredConsecutiveReadings = NormalizeGlucoseAlertRequiredConsecutiveReadings(
            glucoseAlertRequiredConsecutiveReadings);
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

    /// <summary>
    /// Gets a value indicating whether in-app glucose awareness alerts are enabled.
    /// </summary>
    public bool GlucoseAlertsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether below-target glucose awareness alerts are enabled.
    /// </summary>
    public bool LowGlucoseAlertsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether above-target glucose awareness alerts are enabled.
    /// </summary>
    public bool HighGlucoseAlertsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether native OS glucose awareness notifications are enabled.
    /// </summary>
    public bool NativeGlucoseNotificationsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether alert messages should hide glucose values.
    /// </summary>
    public bool GlucoseAlertPrivacyModeEnabled { get; }

    /// <summary>
    /// Gets the minimum repeat interval for repeated native notifications of the same condition.
    /// </summary>
    public TimeSpan GlucoseAlertRepeatInterval { get; }

    /// <summary>
    /// Gets the number of consecutive out-of-range readings required before showing a glucose alert.
    /// </summary>
    public int GlucoseAlertRequiredConsecutiveReadings { get; }
    /// <summary>
    /// Normalizes and validates the number of consecutive out-of-range readings required before showing a glucose alert.
    /// </summary>
    /// <param name="requiredConsecutiveReadings">The required consecutive readings.</param>
    /// <returns>The validated required consecutive readings.</returns>
    private static int NormalizeGlucoseAlertRequiredConsecutiveReadings(int requiredConsecutiveReadings)
    {
        if (requiredConsecutiveReadings is < MinimumGlucoseAlertRequiredConsecutiveReadings or > MaximumGlucoseAlertRequiredConsecutiveReadings)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredConsecutiveReadings),
                requiredConsecutiveReadings,
                $"Glucose alert stability must be between {MinimumGlucoseAlertRequiredConsecutiveReadings} and {MaximumGlucoseAlertRequiredConsecutiveReadings} consecutive readings.");
        }

        return requiredConsecutiveReadings;
    }

}
