using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Providers.Metadata;

/// <summary>
/// Represents metadata describing a CGM provider capability set.
/// </summary>
public sealed record CgmProviderMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmProviderMetadata"/> class.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="displayName">The provider display name.</param>
    /// <param name="expectedFreshness">The expected data freshness.</param>
    /// <param name="supportsLiveReadings">Whether the provider supports live or near real-time readings.</param>
    /// <param name="supportsHistoricalReadings">Whether the provider supports historical readings.</param>
    /// <exception cref="ArgumentException">Thrown when the metadata is invalid.</exception>
    public CgmProviderMetadata(
        CgmProviderKind providerKind,
        string displayName,
        GlucoseDataFreshness expectedFreshness,
        bool supportsLiveReadings,
        bool supportsHistoricalReadings)
    {
        if (providerKind == CgmProviderKind.Unknown)
        {
            throw new ArgumentException("Provider kind must be specified.", nameof(providerKind));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Provider display name must be specified.", nameof(displayName));
        }

        if (expectedFreshness == GlucoseDataFreshness.Unknown)
        {
            throw new ArgumentException("Expected freshness must be specified.", nameof(expectedFreshness));
        }

        if (!supportsLiveReadings && !supportsHistoricalReadings)
        {
            throw new ArgumentException("A provider must support at least one reading mode.", nameof(supportsLiveReadings));
        }

        ProviderKind = providerKind;
        DisplayName = displayName.Trim();
        ExpectedFreshness = expectedFreshness;
        SupportsLiveReadings = supportsLiveReadings;
        SupportsHistoricalReadings = supportsHistoricalReadings;
    }

    /// <summary>
    /// Gets the provider kind.
    /// </summary>
    public CgmProviderKind ProviderKind { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the expected data freshness.
    /// </summary>
    public GlucoseDataFreshness ExpectedFreshness { get; }

    /// <summary>
    /// Gets a value indicating whether the provider supports live or near real-time readings.
    /// </summary>
    public bool SupportsLiveReadings { get; }

    /// <summary>
    /// Gets a value indicating whether the provider supports historical readings.
    /// </summary>
    public bool SupportsHistoricalReadings { get; }
}