using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.WidgetState.Snapshots;

/// <summary>
/// Represents the stable glucose snapshot exposed to external widget surfaces.
/// </summary>
public sealed record GlucoseWidgetState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseWidgetState"/> class.
    /// </summary>
    public GlucoseWidgetState(
        int schemaVersion,
        DateTimeOffset generatedAt,
        DateTimeOffset? readingTimestamp,
        DateTimeOffset? expiresAt,
        decimal? glucoseAmount,
        GlucoseUnit? glucoseUnit,
        TrendDirection? trend,
        CgmProviderKind providerKind,
        GlucoseDataFreshness freshness,
        WidgetGlucoseStatusLevel statusLevel,
        string displayValue,
        string unitLabel,
        string trendLabel,
        string statusMessage)
    {
        if (schemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(schemaVersion),
                schemaVersion,
                "Schema version must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(displayValue))
        {
            throw new ArgumentException(
                "Display value cannot be empty.",
                nameof(displayValue));
        }

        if (string.IsNullOrWhiteSpace(unitLabel))
        {
            throw new ArgumentException(
                "Unit label cannot be empty.",
                nameof(unitLabel));
        }

        if (string.IsNullOrWhiteSpace(trendLabel))
        {
            throw new ArgumentException(
                "Trend label cannot be empty.",
                nameof(trendLabel));
        }

        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            throw new ArgumentException(
                "Status message cannot be empty.",
                nameof(statusMessage));
        }

        SchemaVersion = schemaVersion;
        GeneratedAt = generatedAt;
        ReadingTimestamp = readingTimestamp;
        ExpiresAt = expiresAt;
        GlucoseAmount = glucoseAmount;
        GlucoseUnit = glucoseUnit;
        Trend = trend;
        ProviderKind = providerKind;
        Freshness = freshness;
        StatusLevel = statusLevel;
        DisplayValue = displayValue;
        UnitLabel = unitLabel;
        TrendLabel = trendLabel;
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Gets the widget state schema version.
    /// </summary>
    public int SchemaVersion { get; }

    /// <summary>
    /// Gets the timestamp at which the widget state was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; }

    /// <summary>
    /// Gets the glucose reading timestamp.
    /// </summary>
    public DateTimeOffset? ReadingTimestamp { get; }

    /// <summary>
    /// Gets the timestamp after which the widget state should be considered stale.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>
    /// Gets the glucose amount.
    /// </summary>
    public decimal? GlucoseAmount { get; }

    /// <summary>
    /// Gets the glucose unit.
    /// </summary>
    public GlucoseUnit? GlucoseUnit { get; }

    /// <summary>
    /// Gets the trend direction.
    /// </summary>
    public TrendDirection? Trend { get; }

    /// <summary>
    /// Gets the CGM provider kind.
    /// </summary>
    public CgmProviderKind ProviderKind { get; }

    /// <summary>
    /// Gets the glucose data freshness.
    /// </summary>
    public GlucoseDataFreshness Freshness { get; }

    /// <summary>
    /// Gets the widget glucose status level.
    /// </summary>
    public WidgetGlucoseStatusLevel StatusLevel { get; }

    /// <summary>
    /// Gets the display-ready glucose value.
    /// </summary>
    public string DisplayValue { get; }

    /// <summary>
    /// Gets the display-ready glucose unit label.
    /// </summary>
    public string UnitLabel { get; }

    /// <summary>
    /// Gets the display-ready trend label.
    /// </summary>
    public string TrendLabel { get; }

    /// <summary>
    /// Gets the display-ready widget status message.
    /// </summary>
    public string StatusMessage { get; }

    /// <summary>
    /// Gets a value indicating whether the widget state contains a glucose reading.
    /// </summary>
    public bool HasReading => GlucoseAmount.HasValue && ReadingTimestamp.HasValue;
}