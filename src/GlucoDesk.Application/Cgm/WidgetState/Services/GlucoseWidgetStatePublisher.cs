using GlucoDesk.Application.Cgm.WidgetState.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Factories;
using GlucoDesk.Application.Cgm.WidgetState.Options;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.WidgetState.Services;

/// <summary>
/// Publishes glucose widget state snapshots to the configured widget state store.
/// </summary>
public sealed class GlucoseWidgetStatePublisher : IWidgetStatePublisher
{
    private readonly IWidgetStateStore _widgetStateStore;
    private readonly TimeProvider _timeProvider;
    private readonly WidgetStatePublisherOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseWidgetStatePublisher"/> class.
    /// </summary>
    /// <param name="widgetStateStore">The widget state store.</param>
    /// <param name="timeProvider">The time provider.</param>
    public GlucoseWidgetStatePublisher(
        IWidgetStateStore widgetStateStore,
        TimeProvider timeProvider)
        : this(
            widgetStateStore,
            timeProvider,
            WidgetStatePublisherOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseWidgetStatePublisher"/> class.
    /// </summary>
    /// <param name="widgetStateStore">The widget state store.</param>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="options">The widget state publisher options.</param>
    public GlucoseWidgetStatePublisher(
        IWidgetStateStore widgetStateStore,
        TimeProvider timeProvider,
        WidgetStatePublisherOptions options)
    {
        ArgumentNullException.ThrowIfNull(widgetStateStore);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);

        _widgetStateStore = widgetStateStore;
        _timeProvider = timeProvider;
        _options = options;
    }

    /// <inheritdoc />
    public Task<Result> PublishReadingAsync(
        GlucoseReading reading,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reading);

        var generatedAt = _timeProvider.GetUtcNow();

        var state = GlucoseWidgetStateFactory.FromReading(
            reading,
            generatedAt,
            _options.StaleAfter);

        return _widgetStateStore.SaveAsync(state, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> PublishLatestReadingAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CgmProviderKind providerKind,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(readings);

        if (readings.Count == 0)
        {
            return PublishUnavailableAsync(
                providerKind,
                _options.UnavailableStatusMessage,
                cancellationToken);
        }

        var latestReading = readings
            .OrderByDescending(reading => reading.Timestamp)
            .First();

        return PublishReadingAsync(latestReading, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> PublishUnavailableAsync(
        CgmProviderKind providerKind,
        string? statusMessage,
        CancellationToken cancellationToken)
    {
        var effectiveStatusMessage = string.IsNullOrWhiteSpace(statusMessage)
            ? _options.UnavailableStatusMessage
            : statusMessage;

        var state = GlucoseWidgetStateFactory.Unavailable(
            _timeProvider.GetUtcNow(),
            providerKind,
            effectiveStatusMessage);

        return _widgetStateStore.SaveAsync(state, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> ClearAsync(CancellationToken cancellationToken)
    {
        return _widgetStateStore.ClearAsync(cancellationToken);
    }
}