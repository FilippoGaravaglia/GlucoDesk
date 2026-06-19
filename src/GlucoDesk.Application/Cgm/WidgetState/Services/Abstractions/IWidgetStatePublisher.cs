using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;

/// <summary>
/// Defines application-level operations for publishing glucose widget state snapshots.
/// </summary>
public interface IWidgetStatePublisher
{
    /// <summary>
    /// Publishes a widget state from a glucose reading.
    /// </summary>
    /// <param name="reading">The glucose reading to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publish operation result.</returns>
    Task<Result> PublishReadingAsync(
        GlucoseReading reading,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a widget state from the latest glucose reading in a batch.
    /// </summary>
    /// <param name="readings">The available glucose readings.</param>
    /// <param name="providerKind">The provider kind used when no reading is available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publish operation result.</returns>
    Task<Result> PublishLatestReadingAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CgmProviderKind providerKind,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes an unavailable widget state.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="statusMessage">The status message to expose to the widget.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The publish operation result.</returns>
    Task<Result> PublishUnavailableAsync(
        CgmProviderKind providerKind,
        string? statusMessage,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears the current widget state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The clear operation result.</returns>
    Task<Result> ClearAsync(CancellationToken cancellationToken);
}