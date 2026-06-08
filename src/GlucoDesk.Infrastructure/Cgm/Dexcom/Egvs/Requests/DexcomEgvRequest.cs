namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;

/// <summary>
/// Represents a Dexcom EGV request.
/// </summary>
public sealed record DexcomEgvRequest
{
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomEgvRequest"/> class.
    /// </summary>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    /// <param name="startDateUtc">The inclusive UTC start date.</param>
    /// <param name="endDateUtc">The exclusive UTC end date.</param>
    /// <param name="forceTokenRefresh">Whether to force an access token refresh before the request.</param>
    public DexcomEgvRequest(
        string clientSecret,
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc,
        bool forceTokenRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified.", nameof(clientSecret));
        }

        var normalizedStartDateUtc = startDateUtc.ToUniversalTime();
        var normalizedEndDateUtc = endDateUtc.ToUniversalTime();

        if (normalizedStartDateUtc >= normalizedEndDateUtc)
        {
            throw new ArgumentException(
                "Dexcom EGV start date must be earlier than end date.",
                nameof(startDateUtc));
        }

        if (normalizedEndDateUtc - normalizedStartDateUtc > MaxRange)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDateUtc),
                endDateUtc,
                "Dexcom EGV date range cannot exceed 30 days.");
        }

        ClientSecret = clientSecret.Trim();
        StartDateUtc = normalizedStartDateUtc;
        EndDateUtc = normalizedEndDateUtc;
        ForceTokenRefresh = forceTokenRefresh;
    }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string ClientSecret { get; }

    /// <summary>
    /// Gets the inclusive UTC start date.
    /// </summary>
    public DateTimeOffset StartDateUtc { get; }

    /// <summary>
    /// Gets the exclusive UTC end date.
    /// </summary>
    public DateTimeOffset EndDateUtc { get; }

    /// <summary>
    /// Gets a value indicating whether to force an access token refresh before the request.
    /// </summary>
    public bool ForceTokenRefresh { get; }
}