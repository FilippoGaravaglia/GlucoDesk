using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

/// <summary>
/// Coordinates the Dexcom OAuth authorization session flow.
/// </summary>
public sealed class DexcomOAuthAuthorizationSessionService : IDexcomOAuthAuthorizationSessionService
{
    private readonly DexcomApiOptions _options;
    private readonly IDexcomOAuthStateGenerator _stateGenerator;
    private readonly IDexcomAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IDexcomSystemBrowser _systemBrowser;
    private readonly IDexcomLocalOAuthCallbackListener _callbackListener;
    private readonly IDexcomTokenClient _tokenClient;
    private readonly IDexcomOAuthTokenStore _tokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthAuthorizationSessionService"/> class.
    /// </summary>
    /// <param name="options">The Dexcom API options.</param>
    /// <param name="stateGenerator">The Dexcom OAuth state generator.</param>
    /// <param name="authorizationUrlBuilder">The Dexcom authorization URL builder.</param>
    /// <param name="systemBrowser">The system browser abstraction.</param>
    /// <param name="callbackListener">The local OAuth callback listener.</param>
    /// <param name="tokenClient">The Dexcom token client.</param>
    /// <param name="tokenStore">The Dexcom OAuth token store.</param>
    public DexcomOAuthAuthorizationSessionService(
        DexcomApiOptions options,
        IDexcomOAuthStateGenerator stateGenerator,
        IDexcomAuthorizationUrlBuilder authorizationUrlBuilder,
        IDexcomSystemBrowser systemBrowser,
        IDexcomLocalOAuthCallbackListener callbackListener,
        IDexcomTokenClient tokenClient,
        IDexcomOAuthTokenStore tokenStore)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stateGenerator);
        ArgumentNullException.ThrowIfNull(authorizationUrlBuilder);
        ArgumentNullException.ThrowIfNull(systemBrowser);
        ArgumentNullException.ThrowIfNull(callbackListener);
        ArgumentNullException.ThrowIfNull(tokenClient);
        ArgumentNullException.ThrowIfNull(tokenStore);

        _options = options;
        _stateGenerator = stateGenerator;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _systemBrowser = systemBrowser;
        _callbackListener = callbackListener;
        _tokenClient = tokenClient;
        _tokenStore = tokenStore;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomOAuthAuthorizationSessionResult>> StartAuthorizationSessionAsync(
        DexcomOAuthAuthorizationSessionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var state = _stateGenerator.GenerateState();
        var authorizationUri = BuildAuthorizationUri(state);

        var browserResult = await _systemBrowser
            .OpenAsync(authorizationUri, cancellationToken)
            .ConfigureAwait(false);

        if (browserResult.IsFailure)
        {
            return Result<DexcomOAuthAuthorizationSessionResult>.Failure(browserResult.Error);
        }

        var callbackResult = await _callbackListener
            .ListenForCallbackAsync(
                new DexcomLocalOAuthCallbackListenRequest(
                    _options.RedirectUri,
                    state,
                    request.CallbackTimeout),
                cancellationToken)
            .ConfigureAwait(false);

        if (callbackResult.IsFailure)
        {
            return Result<DexcomOAuthAuthorizationSessionResult>.Failure(callbackResult.Error);
        }

        var tokenResult = await _tokenClient
            .ExchangeAuthorizationCodeAsync(
                new DexcomAuthorizationCodeTokenRequest(
                    callbackResult.Value.AuthorizationCode,
                    request.ClientSecret),
                cancellationToken)
            .ConfigureAwait(false);

        if (tokenResult.IsFailure)
        {
            return Result<DexcomOAuthAuthorizationSessionResult>.Failure(tokenResult.Error);
        }

        var saveTokenResult = await _tokenStore
            .SaveTokenSetAsync(tokenResult.Value, cancellationToken)
            .ConfigureAwait(false);

        if (saveTokenResult.IsFailure)
        {
            return Result<DexcomOAuthAuthorizationSessionResult>.Failure(saveTokenResult.Error);
        }

        return Result<DexcomOAuthAuthorizationSessionResult>.Success(
            new DexcomOAuthAuthorizationSessionResult(
                authorizationUri,
                state,
                callbackResult.Value.CallbackUri,
                tokenResult.Value));
    }

    #region Helpers

    /// <summary>
    /// Builds the Dexcom OAuth authorization URI for the generated state.
    /// </summary>
    /// <param name="state">The generated OAuth state.</param>
    /// <returns>The Dexcom OAuth authorization URI.</returns>
    private Uri BuildAuthorizationUri(string state)
    {
        return _authorizationUrlBuilder.BuildAuthorizationUri(
            _options.Environment,
            new DexcomAuthorizationRequest(
                _options.ClientId,
                _options.RedirectUri,
                _options.Scopes,
                state));
    }

    #endregion
}