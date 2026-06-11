using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;

/// <summary>
/// Represents the current Dexcom connection status without exposing OAuth secrets.
/// </summary>
public sealed record DexcomConnectionStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomConnectionStatus"/> class.
    /// </summary>
    /// <param name="state">The Dexcom connection state.</param>
    /// <param name="checkedAtUtc">The UTC timestamp when the status was checked.</param>
    /// <param name="message">The human-readable status message.</param>
    /// <param name="accessTokenExpiresAtUtc">The optional access token expiration timestamp.</param>
    /// <param name="refreshTokenExpiresAtUtc">The optional refresh token expiration timestamp.</param>
    public DexcomConnectionStatus(
        DexcomConnectionState state,
        DateTimeOffset checkedAtUtc,
        string message,
        DateTimeOffset? accessTokenExpiresAtUtc = null,
        DateTimeOffset? refreshTokenExpiresAtUtc = null)
    {
        if (!Enum.IsDefined(state) || state == DexcomConnectionState.Unknown)
        {
            throw new ArgumentException("Dexcom connection state must be specified.", nameof(state));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Connection status message must be specified.", nameof(message));
        }

        State = state;
        CheckedAtUtc = checkedAtUtc;
        Message = message.Trim();
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
    }

    /// <summary>
    /// Gets the Dexcom connection state.
    /// </summary>
    public DexcomConnectionState State { get; }

    /// <summary>
    /// Gets the UTC timestamp when the status was checked.
    /// </summary>
    public DateTimeOffset CheckedAtUtc { get; }

    /// <summary>
    /// Gets the human-readable status message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional access token expiration timestamp.
    /// </summary>
    public DateTimeOffset? AccessTokenExpiresAtUtc { get; }

    /// <summary>
    /// Gets the optional refresh token expiration timestamp.
    /// </summary>
    public DateTimeOffset? RefreshTokenExpiresAtUtc { get; }

    /// <summary>
    /// Gets a value indicating whether Dexcom is currently connected with a usable access token.
    /// </summary>
    public bool IsConnected => State == DexcomConnectionState.Connected;

    /// <summary>
    /// Gets a value indicating whether a token refresh can be attempted.
    /// </summary>
    public bool CanAttemptRefresh => State == DexcomConnectionState.AccessTokenRefreshRequired;
}