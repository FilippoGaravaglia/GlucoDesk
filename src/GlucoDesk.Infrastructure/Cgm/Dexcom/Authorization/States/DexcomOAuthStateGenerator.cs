using System.Security.Cryptography;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;

/// <summary>
/// Generates cryptographically secure Dexcom OAuth state values.
/// </summary>
public sealed class DexcomOAuthStateGenerator : IDexcomOAuthStateGenerator
{
    private readonly DexcomOAuthStateOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthStateGenerator"/> class.
    /// </summary>
    /// <param name="options">The OAuth state generation options.</param>
    public DexcomOAuthStateGenerator(DexcomOAuthStateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public string GenerateState()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(_options.StateLengthBytes);

        return ToBase64Url(randomBytes);
    }

    #region Helpers

    /// <summary>
    /// Converts bytes to a Base64 URL-safe string without padding.
    /// </summary>
    /// <param name="bytes">The bytes to convert.</param>
    /// <returns>The Base64 URL-safe value.</returns>
    private static string ToBase64Url(byte[] bytes)
    {
        return Convert
            .ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    #endregion
}