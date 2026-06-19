namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

/// <summary>
/// Represents Dexcom Share provider configuration.
/// </summary>
public sealed record DexcomShareOptions
{
    private const int MaximumDexcomShareReadingCount = 288;

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
    /// <param name="sessionCacheDuration">The in-memory Dexcom Share session cache duration.</param>
    public DexcomShareOptions(
        string? username,
        string? password,
        DexcomShareRegion region,
        string? applicationId = null,
        string? displayName = null,
        TimeSpan? latestReadingLookback = null,
        TimeSpan? recentReadingsLookback = null,
        int maximumRecentReadings = MaximumDexcomShareReadingCount,
        TimeSpan? sessionCacheDuration = null)
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
        RecentReadingsLookback = recentReadingsLookback ?? TimeSpan.FromHours(24);
        MaximumRecentReadings = maximumRecentReadings <= 0
            ? MaximumDexcomShareReadingCount
            : Math.Min(maximumRecentReadings, MaximumDexcomShareReadingCount);
        SessionCacheDuration = sessionCacheDuration is null || sessionCacheDuration <= TimeSpan.Zero
            ? TimeSpan.FromMinutes(25)
            : sessionCacheDuration.Value;
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
    /// Gets the in-memory Dexcom Share session cache duration.
    /// </summary>
    public TimeSpan SessionCacheDuration { get; }

    /// <summary>
    /// Gets a value indicating whether Dexcom Share is configured.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && Region is not DexcomShareRegion.Unknown;
}