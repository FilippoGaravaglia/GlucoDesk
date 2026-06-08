namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

/// <summary>
/// Represents Dexcom OAuth token refresh behavior options.
/// </summary>
public sealed record DexcomOAuthTokenRefreshOptions
{
    /// <summary>
    /// Gets the default Dexcom OAuth token refresh options.
    /// </summary>
    public static DexcomOAuthTokenRefreshOptions Default { get; } = new(TimeSpan.FromMinutes(5));

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthTokenRefreshOptions"/> class.
    /// </summary>
    /// <param name="refreshSafetyWindow">The safety window before access token expiration.</param>
    public DexcomOAuthTokenRefreshOptions(TimeSpan refreshSafetyWindow)
    {
        if (refreshSafetyWindow < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(refreshSafetyWindow),
                refreshSafetyWindow,
                "Refresh safety window cannot be negative.");
        }

        if (refreshSafetyWindow > TimeSpan.FromHours(1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(refreshSafetyWindow),
                refreshSafetyWindow,
                "Refresh safety window cannot exceed 1 hour.");
        }

        RefreshSafetyWindow = refreshSafetyWindow;
    }

    /// <summary>
    /// Gets the safety window before access token expiration.
    /// </summary>
    public TimeSpan RefreshSafetyWindow { get; }
}