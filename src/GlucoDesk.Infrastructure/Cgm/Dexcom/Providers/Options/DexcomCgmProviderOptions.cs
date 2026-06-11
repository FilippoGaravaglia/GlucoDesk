namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;

/// <summary>
/// Represents configuration options for the Dexcom Official CGM provider.
/// </summary>
public sealed record DexcomCgmProviderOptions
{
    /// <summary>
    /// Gets the default Dexcom CGM provider options.
    /// </summary>
    public static DexcomCgmProviderOptions Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomCgmProviderOptions"/> class.
    /// </summary>
    /// <param name="clientSecret">The optional Dexcom application client secret.</param>
    /// <param name="latestReadingLookback">The lookback window used to find the latest available delayed reading.</param>
    /// <param name="displayName">The provider display name.</param>
    public DexcomCgmProviderOptions(
        string? clientSecret = null,
        TimeSpan? latestReadingLookback = null,
        string displayName = "Dexcom Official API")
    {
        var effectiveLatestReadingLookback = latestReadingLookback ?? TimeSpan.FromHours(24);

        if (effectiveLatestReadingLookback <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latestReadingLookback),
                latestReadingLookback,
                "Latest reading lookback must be greater than zero.");
        }

        if (effectiveLatestReadingLookback > TimeSpan.FromDays(30))
        {
            throw new ArgumentOutOfRangeException(
                nameof(latestReadingLookback),
                latestReadingLookback,
                "Latest reading lookback cannot exceed 30 days.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Provider display name must be specified.", nameof(displayName));
        }

        ClientSecret = string.IsNullOrWhiteSpace(clientSecret)
            ? null
            : clientSecret.Trim();

        LatestReadingLookback = effectiveLatestReadingLookback;
        DisplayName = displayName.Trim();
    }

    /// <summary>
    /// Gets the optional Dexcom application client secret.
    /// </summary>
    public string? ClientSecret { get; }

    /// <summary>
    /// Gets the lookback window used to find the latest available delayed reading.
    /// </summary>
    public TimeSpan LatestReadingLookback { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets a value indicating whether the provider has a configured client secret.
    /// </summary>
    public bool HasClientSecret => !string.IsNullOrWhiteSpace(ClientSecret);
}