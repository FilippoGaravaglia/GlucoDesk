namespace GlucoDesk.Application.Common.Errors;

/// <summary>
/// Represents an application error with a stable code and a human-readable message.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The stable application error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <exception cref="ArgumentException">Thrown when code or message is empty.</exception>
    public Error(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code must be specified.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message must be specified.", nameof(message));
        }

        Code = code.Trim();
        Message = message.Trim();
    }

    /// <summary>
    /// Gets the empty error used by successful results.
    /// </summary>
    public static Error None { get; } = new("None", "No error.");

    /// <summary>
    /// Gets the stable application error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; }
}