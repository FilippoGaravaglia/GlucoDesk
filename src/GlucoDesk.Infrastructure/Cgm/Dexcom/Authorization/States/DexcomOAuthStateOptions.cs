namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;

/// <summary>
/// Represents Dexcom OAuth state generation options.
/// </summary>
public sealed record DexcomOAuthStateOptions
{
    /// <summary>
    /// Gets the default Dexcom OAuth state options.
    /// </summary>
    public static DexcomOAuthStateOptions Default { get; } = new(32);

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthStateOptions"/> class.
    /// </summary>
    /// <param name="stateLengthBytes">The random state length in bytes.</param>
    public DexcomOAuthStateOptions(int stateLengthBytes)
    {
        if (stateLengthBytes < 16)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stateLengthBytes),
                stateLengthBytes,
                "OAuth state length must be at least 16 bytes.");
        }

        if (stateLengthBytes > 128)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stateLengthBytes),
                stateLengthBytes,
                "OAuth state length cannot exceed 128 bytes.");
        }

        StateLengthBytes = stateLengthBytes;
    }

    /// <summary>
    /// Gets the random state length in bytes.
    /// </summary>
    public int StateLengthBytes { get; }
}