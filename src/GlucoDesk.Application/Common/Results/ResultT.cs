using GlucoDesk.Application.Common.Errors;

namespace GlucoDesk.Application.Common.Results;

/// <summary>
/// Represents the result of an application operation that returns a value.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class Result<T> : Result
    where T : notnull
{
    private readonly T? _value;

    private Result(T value)
        : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
        _value = default;
    }

    /// <summary>
    /// Gets the operation value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is failed.</exception>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("The value of a failed result cannot be accessed.");
            }

            return _value!;
        }
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The successful operation value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The failure error.</param>
    /// <returns>A failed result.</returns>
    public new static Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }
}