namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Models;

/// <summary>
/// Represents the result of a successful desktop Dexcom connection action without exposing OAuth secrets.
/// </summary>
public sealed record DexcomDesktopConnectionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomDesktopConnectionResult"/> class.
    /// </summary>
    /// <param name="connectedAtUtc">The UTC timestamp when the connection completed.</param>
    /// <param name="accessTokenExpiresAtUtc">The access token expiration timestamp.</param>
    /// <param name="refreshTokenExpiresAtUtc">The optional refresh token expiration timestamp.</param>
    public DexcomDesktopConnectionResult(
        DateTimeOffset connectedAtUtc,
        DateTimeOffset accessTokenExpiresAtUtc,
        DateTimeOffset? refreshTokenExpiresAtUtc)
    {
        if (accessTokenExpiresAtUtc <= connectedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(accessTokenExpiresAtUtc),
                accessTokenExpiresAtUtc,
                "Access token expiration must be greater than the connection timestamp.");
        }

        if (refreshTokenExpiresAtUtc is not null && refreshTokenExpiresAtUtc <= connectedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(refreshTokenExpiresAtUtc),
                refreshTokenExpiresAtUtc,
                "Refresh token expiration must be greater than the connection timestamp.");
        }

        ConnectedAtUtc = connectedAtUtc;
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
    }

    /// <summary>
    /// Gets the UTC timestamp when the connection completed.
    /// </summary>
    public DateTimeOffset ConnectedAtUtc { get; }

    /// <summary>
    /// Gets the access token expiration timestamp.
    /// </summary>
    public DateTimeOffset AccessTokenExpiresAtUtc { get; }

    /// <summary>
    /// Gets the optional refresh token expiration timestamp.
    /// </summary>
    public DateTimeOffset? RefreshTokenExpiresAtUtc { get; }
}