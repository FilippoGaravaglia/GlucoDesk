using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

/// <summary>
/// Parses and validates Dexcom OAuth callback URIs.
/// </summary>
public sealed class DexcomOAuthCallbackParser : IDexcomOAuthCallbackParser
{
    /// <inheritdoc />
    public Result<DexcomOAuthCallbackResult> ParseCallback(
        Uri callbackUri,
        string expectedState)
    {
        ArgumentNullException.ThrowIfNull(callbackUri);

        if (string.IsNullOrWhiteSpace(expectedState))
        {
            throw new ArgumentException("Expected OAuth state must be specified.", nameof(expectedState));
        }

        if (!callbackUri.IsAbsoluteUri)
        {
            return Result<DexcomOAuthCallbackResult>.Failure(
                new Error("Dexcom.OAuthInvalidCallback", "Dexcom OAuth callback URI must be absolute."));
        }

        var queryParameters = ParseQueryString(callbackUri.Query);

        if (queryParameters.TryGetValue("error", out var oauthError))
        {
            return Result<DexcomOAuthCallbackResult>.Failure(
                new Error("Dexcom.OAuthRejected", BuildOAuthErrorMessage(oauthError, queryParameters)));
        }

        if (!queryParameters.TryGetValue("code", out var authorizationCode) ||
            string.IsNullOrWhiteSpace(authorizationCode))
        {
            return Result<DexcomOAuthCallbackResult>.Failure(
                new Error("Dexcom.OAuthMissingCode", "Dexcom OAuth callback does not contain an authorization code."));
        }

        if (!queryParameters.TryGetValue("state", out var returnedState) ||
            string.IsNullOrWhiteSpace(returnedState))
        {
            return Result<DexcomOAuthCallbackResult>.Failure(
                new Error("Dexcom.OAuthMissingState", "Dexcom OAuth callback does not contain a state value."));
        }

        if (!string.Equals(returnedState, expectedState.Trim(), StringComparison.Ordinal))
        {
            return Result<DexcomOAuthCallbackResult>.Failure(
                new Error("Dexcom.OAuthStateMismatch", "Dexcom OAuth callback state does not match the expected state."));
        }

        return Result<DexcomOAuthCallbackResult>.Success(
            new DexcomOAuthCallbackResult(authorizationCode, returnedState));
    }

    #region Helpers

    /// <summary>
    /// Parses a URI query string into a key/value dictionary.
    /// </summary>
    /// <param name="query">The URI query string.</param>
    /// <returns>The parsed query parameters.</returns>
    private static IReadOnlyDictionary<string, string> ParseQueryString(string query)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(query))
        {
            return values;
        }

        var trimmedQuery = query[0] == '?'
            ? query[1..]
            : query;

        foreach (var segment in trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValue = segment.Split('=', 2);

            if (keyValue.Length == 0 || string.IsNullOrWhiteSpace(keyValue[0]))
            {
                continue;
            }

            var key = DecodeQueryComponent(keyValue[0]);
            var value = keyValue.Length == 2
                ? DecodeQueryComponent(keyValue[1])
                : string.Empty;

            values.TryAdd(key, value);
        }

        return values;
    }

    /// <summary>
    /// Builds a display-safe OAuth error message.
    /// </summary>
    /// <param name="oauthError">The OAuth error code.</param>
    /// <param name="queryParameters">The callback query parameters.</param>
    /// <returns>The display-safe OAuth error message.</returns>
    private static string BuildOAuthErrorMessage(
        string oauthError,
        IReadOnlyDictionary<string, string> queryParameters)
    {
        if (queryParameters.TryGetValue("error_description", out var description) &&
            !string.IsNullOrWhiteSpace(description))
        {
            return $"Dexcom OAuth authorization failed: {oauthError}. {description}";
        }

        return $"Dexcom OAuth authorization failed: {oauthError}.";
    }

    /// <summary>
    /// Decodes a URI query component.
    /// </summary>
    /// <param name="value">The encoded query component.</param>
    /// <returns>The decoded query component.</returns>
    private static string DecodeQueryComponent(string value)
    {
        return Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal));
    }

    #endregion
}