using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Mapping;

/// <summary>
/// Provides local result composition helpers for Dexcom Share mapping.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Returns the current successful result or evaluates the fallback.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="fallbackFactory">The fallback result factory.</param>
    /// <returns>The current successful result or fallback result.</returns>
    public static Result<TValue> Or<TValue>(
        this Result<TValue> result,
        Func<Result<TValue>> fallbackFactory)
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);

        return result.IsSuccess
            ? result
            : fallbackFactory();
    }
}