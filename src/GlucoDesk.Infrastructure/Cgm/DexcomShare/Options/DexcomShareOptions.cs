namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

/// <summary>
/// Represents Dexcom Share provider configuration.
/// </summary>
public sealed record DexcomShareOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareOptions"/> class.
    /// </summary>
    /// <param name="username">The Dexcom account username or email.</param>
    /// <param name="password">The Dexcom account password.</param>
    /// <param name="region">The Dexcom Share region.</param>
    /// <param name="applicationId">The Dexcom Share application identifier.</param>
    /// <param name="displayName">The provider display name.</param>
    /// <param name="latestReadingLookback">The latest reading lookback window.</param>
    /// <param name="recentReadingsLookback">The recent readings lookback window.</param>
    /// <param name="maximumRecentReadings">The maximum number of recent readings requested from Dexcom Share.</param>
    public DexcomShareOptions(
        string? username,
        string? password,
        DexcomShareRegion region,
        string? applicationId = null,
        string? displayName = null,
        TimeSpan? latestReadingLookback = null,
        TimeSpan? recentReadingsLookback = null,
        int maximumRecentReadings = 144)
    {
        Username = username?.Trim() ?? string.Empty;
        Password = password ?? string.Empty;
        Region = region;
        ApplicationId = string.IsNullOrWhiteSpace(applicationId)
            ? "d89443d2-327c-4a6f-89e5-496bbb0317db"
            : applicationId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? "Dexcom Share"
            : displayName.Trim();
        LatestReadingLookback = latestReadingLookback ?? TimeSpan.FromMinutes(30);
        RecentReadingsLookback = recentReadingsLookback ?? TimeSpan.FromHours(12);
        MaximumRecentReadings = maximumRecentReadings <= 0
            ? 144
            : Math.Min(maximumRecentReadings, 144);
    }

    /// <summary>
    /// Gets the Dexcom account username or email.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the Dexcom account password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the Dexcom Share region.
    /// </summary>
    public DexcomShareRegion Region { get; }

    /// <summary>
    /// Gets the Dexcom Share application identifier.
    /// </summary>
    public string ApplicationId { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the latest reading lookback window.
    /// </summary>
    public TimeSpan LatestReadingLookback { get; }

    /// <summary>
    /// Gets the recent readings lookback window.
    /// </summary>
    public TimeSpan RecentReadingsLookback { get; }

    /// <summary>
    /// Gets the maximum number of recent readings requested from Dexcom Share.
    /// </summary>
    public int MaximumRecentReadings { get; }

    /// <summary>
    /// Gets a value indicating whether the provider has enough configuration to run.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && Region is not DexcomShareRegion.Unknown;
}