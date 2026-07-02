using System.Globalization;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Coordinates glucose awareness alert state, privacy wording and native notification cooldown.
/// </summary>
public sealed class GlucoseAlertCoordinator
{
    private const decimal MmolLConversionFactor = 18.0182m;

    private readonly IGlucoseAlertNotificationService _notificationService;
    private readonly IGlucoseAlertClock _clock;

    private GlucoseAlertKind _activeAlertKind = GlucoseAlertKind.None;
    private DateTimeOffset? _lastLowNotificationAt;
    private DateTimeOffset? _lastHighNotificationAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseAlertCoordinator"/> class.
    /// </summary>
    /// <param name="notificationService">The native notification service.</param>
    /// <param name="clock">The alert clock.</param>
    public GlucoseAlertCoordinator(
        IGlucoseAlertNotificationService notificationService,
        IGlucoseAlertClock clock)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(clock);

        _notificationService = notificationService;
        _clock = clock;
    }

    /// <summary>
    /// Evaluates the current glucose state and returns the alert presentation.
    /// </summary>
    /// <param name="glucoseMgDl">The current glucose value expressed in mg/dL.</param>
    /// <param name="settings">The current application settings.</param>
    /// <param name="displayUnit">The preferred display unit.</param>
    /// <returns>The alert presentation.</returns>
    public GlucoseAlertPresentation Evaluate(
        decimal glucoseMgDl,
        ApplicationSettings settings,
        GlucoseUnit displayUnit)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.GlucoseAlertsEnabled)
        {
            ResetActiveAlert();
            return GlucoseAlertPresentation.None;
        }

        var kind = ResolveAlertKind(glucoseMgDl, settings);

        if (kind == GlucoseAlertKind.None)
        {
            ResetActiveAlert();
            return GlucoseAlertPresentation.None;
        }

        if (kind == GlucoseAlertKind.Low && !settings.LowGlucoseAlertsEnabled)
        {
            ResetActiveAlert();
            return GlucoseAlertPresentation.None;
        }

        if (kind == GlucoseAlertKind.High && !settings.HighGlucoseAlertsEnabled)
        {
            ResetActiveAlert();
            return GlucoseAlertPresentation.None;
        }

        var shouldSendNativeNotification = ShouldSendNativeNotification(kind, settings);
        var presentation = BuildPresentation(
            kind,
            glucoseMgDl,
            settings,
            NormalizeDisplayUnit(displayUnit),
            shouldSendNativeNotification);

        _activeAlertKind = kind;

        if (shouldSendNativeNotification)
        {
            MarkNativeNotificationSent(kind);
        }

        return presentation;
    }

    /// <summary>
    /// Sends a native notification for an alert presentation.
    /// </summary>
    /// <param name="presentation">The alert presentation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    public Task SendNativeNotificationAsync(
        GlucoseAlertPresentation presentation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        if (presentation.Kind == GlucoseAlertKind.None)
        {
            return Task.CompletedTask;
        }

        return _notificationService.ShowAsync(
            presentation.ToNativeNotification(),
            cancellationToken);
    }

    #region Helpers

    /// <summary>
    /// Resolves the alert kind for the current glucose value.
    /// </summary>
    /// <param name="glucoseMgDl">The current glucose value expressed in mg/dL.</param>
    /// <param name="settings">The current application settings.</param>
    /// <returns>The alert kind.</returns>
    private static GlucoseAlertKind ResolveAlertKind(
        decimal glucoseMgDl,
        ApplicationSettings settings)
    {
        if (glucoseMgDl < settings.TargetLowMgDl)
        {
            return GlucoseAlertKind.Low;
        }

        if (glucoseMgDl > settings.TargetHighMgDl)
        {
            return GlucoseAlertKind.High;
        }

        return GlucoseAlertKind.None;
    }

    /// <summary>
    /// Returns whether a native notification should be sent.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    /// <param name="settings">The current application settings.</param>
    /// <returns>True when a native notification should be sent; otherwise false.</returns>
    private bool ShouldSendNativeNotification(
        GlucoseAlertKind kind,
        ApplicationSettings settings)
    {
        if (!settings.NativeGlucoseNotificationsEnabled)
        {
            return false;
        }

        if (_activeAlertKind != kind)
        {
            return true;
        }

        var lastNotificationAt = kind == GlucoseAlertKind.Low
            ? _lastLowNotificationAt
            : _lastHighNotificationAt;

        if (lastNotificationAt is null)
        {
            return true;
        }

        return _clock.Now - lastNotificationAt.Value >= settings.GlucoseAlertRepeatInterval;
    }

    /// <summary>
    /// Marks a native notification as sent for the specified alert kind.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    private void MarkNativeNotificationSent(GlucoseAlertKind kind)
    {
        if (kind == GlucoseAlertKind.Low)
        {
            _lastLowNotificationAt = _clock.Now;
            return;
        }

        if (kind == GlucoseAlertKind.High)
        {
            _lastHighNotificationAt = _clock.Now;
        }
    }

    /// <summary>
    /// Resets the active alert state after the value returns in range or alerts are disabled.
    /// </summary>
    private void ResetActiveAlert()
    {
        _activeAlertKind = GlucoseAlertKind.None;
    }

    /// <summary>
    /// Builds the alert presentation.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    /// <param name="glucoseMgDl">The current glucose value expressed in mg/dL.</param>
    /// <param name="settings">The current application settings.</param>
    /// <param name="displayUnit">The preferred display unit.</param>
    /// <param name="shouldSendNativeNotification">A value indicating whether a native notification should be sent.</param>
    /// <returns>The alert presentation.</returns>
    private static GlucoseAlertPresentation BuildPresentation(
        GlucoseAlertKind kind,
        decimal glucoseMgDl,
        ApplicationSettings settings,
        GlucoseUnit displayUnit,
        bool shouldSendNativeNotification)
    {
        var title = kind == GlucoseAlertKind.Low
            ? "Glucose below target"
            : "Glucose above target";

        var badgeText = kind == GlucoseAlertKind.Low
            ? "Below target"
            : "Above target";

        var message = settings.GlucoseAlertPrivacyModeEnabled
            ? BuildPrivacyMessage(kind)
            : BuildDetailedMessage(kind, glucoseMgDl, settings, displayUnit);

        return new GlucoseAlertPresentation(
            kind,
            title,
            message,
            badgeText,
            "Check your official diabetes app before making therapy decisions.",
            shouldSendNativeNotification);
    }

    /// <summary>
    /// Builds a privacy-preserving alert message.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    /// <returns>The privacy-preserving message.</returns>
    private static string BuildPrivacyMessage(GlucoseAlertKind kind)
    {
        return kind == GlucoseAlertKind.Low
            ? "Your glucose is currently below your configured target range."
            : "Your glucose is currently above your configured target range.";
    }

    /// <summary>
    /// Builds a detailed alert message.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    /// <param name="glucoseMgDl">The current glucose value expressed in mg/dL.</param>
    /// <param name="settings">The current application settings.</param>
    /// <param name="displayUnit">The preferred display unit.</param>
    /// <returns>The detailed message.</returns>
    private static string BuildDetailedMessage(
        GlucoseAlertKind kind,
        decimal glucoseMgDl,
        ApplicationSettings settings,
        GlucoseUnit displayUnit)
    {
        var currentValue = FormatGlucoseValue(glucoseMgDl, displayUnit);
        var targetLow = FormatGlucoseAmount(settings.TargetLowMgDl, displayUnit);
        var targetHigh = FormatGlucoseAmount(settings.TargetHighMgDl, displayUnit);
        var unitLabel = FormatGlucoseUnitLabel(displayUnit);
        var direction = kind == GlucoseAlertKind.Low ? "below" : "above";

        return $"Current reading is {currentValue}, {direction} your target range of {targetLow}-{targetHigh} {unitLabel}.";
    }

    /// <summary>
    /// Formats a glucose value using the requested display unit.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="displayUnit">The display unit.</param>
    /// <returns>The formatted glucose value.</returns>
    private static string FormatGlucoseValue(
        decimal valueMgDl,
        GlucoseUnit displayUnit)
    {
        return $"{FormatGlucoseAmount(valueMgDl, displayUnit)} {FormatGlucoseUnitLabel(displayUnit)}";
    }

    /// <summary>
    /// Formats a glucose amount using the requested display unit.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="displayUnit">The display unit.</param>
    /// <returns>The formatted glucose amount.</returns>
    private static string FormatGlucoseAmount(
        decimal valueMgDl,
        GlucoseUnit displayUnit)
    {
        if (displayUnit == GlucoseUnit.MmolL)
        {
            return (valueMgDl / MmolLConversionFactor).ToString("0.0", CultureInfo.InvariantCulture);
        }

        return valueMgDl.ToString("0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the glucose unit label.
    /// </summary>
    /// <param name="displayUnit">The display unit.</param>
    /// <returns>The glucose unit label.</returns>
    private static string FormatGlucoseUnitLabel(GlucoseUnit displayUnit)
    {
        return displayUnit == GlucoseUnit.MmolL
            ? "mmol/L"
            : "mg/dL";
    }

    /// <summary>
    /// Normalizes unsupported display units to mg/dL.
    /// </summary>
    /// <param name="displayUnit">The display unit.</param>
    /// <returns>The normalized display unit.</returns>
    private static GlucoseUnit NormalizeDisplayUnit(GlucoseUnit displayUnit)
    {
        return displayUnit == GlucoseUnit.MmolL
            ? GlucoseUnit.MmolL
            : GlucoseUnit.MgDl;
    }

    #endregion
}
