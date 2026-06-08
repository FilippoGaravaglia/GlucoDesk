namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

/// <summary>
/// Represents Dexcom local OAuth callback listener options.
/// </summary>
public sealed record DexcomLocalOAuthCallbackOptions
{
    /// <summary>
    /// Gets the default Dexcom local OAuth callback listener options.
    /// </summary>
    public static DexcomLocalOAuthCallbackOptions Default { get; } = new(TimeSpan.FromMinutes(2));

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomLocalOAuthCallbackOptions"/> class.
    /// </summary>
    /// <param name="defaultTimeout">The default callback timeout.</param>
    public DexcomLocalOAuthCallbackOptions(TimeSpan defaultTimeout)
    {
        if (defaultTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultTimeout),
                defaultTimeout,
                "Dexcom OAuth callback timeout must be greater than zero.");
        }

        if (defaultTimeout > TimeSpan.FromMinutes(10))
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultTimeout),
                defaultTimeout,
                "Dexcom OAuth callback timeout cannot exceed 10 minutes.");
        }

        DefaultTimeout = defaultTimeout;
    }

    /// <summary>
    /// Gets the default callback timeout.
    /// </summary>
    public TimeSpan DefaultTimeout { get; }
}