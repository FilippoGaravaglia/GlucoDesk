using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

/// <summary>
/// Stores Dexcom OAuth tokens in memory for the current application process.
/// </summary>
/// <remarks>
/// This implementation intentionally does not persist tokens to disk.
/// A production persistent implementation should use platform-secure storage.
/// </remarks>
public sealed class InMemoryDexcomOAuthTokenStore : IDexcomOAuthTokenStore
{
    private readonly object _syncRoot = new();

    private DexcomOAuthTokenSet? _tokenSet;

    /// <inheritdoc />
    public Task<Result> SaveTokenSetAsync(
        DexcomOAuthTokenSet tokenSet,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tokenSet);

        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            _tokenSet = tokenSet;
        }

        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result<DexcomOAuthTokenSet>> GetTokenSetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_tokenSet is null)
            {
                return Task.FromResult(Result<DexcomOAuthTokenSet>.Failure(
                    new Error("Dexcom.TokenStoreEmpty", "No Dexcom OAuth token set is currently stored.")));
            }

            return Task.FromResult(Result<DexcomOAuthTokenSet>.Success(_tokenSet));
        }
    }

    /// <inheritdoc />
    public Task<Result<bool>> HasTokenSetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(Result<bool>.Success(_tokenSet is not null));
        }
    }

    /// <inheritdoc />
    public Task<Result> ClearTokenSetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            _tokenSet = null;
        }

        return Task.FromResult(Result.Success());
    }
}