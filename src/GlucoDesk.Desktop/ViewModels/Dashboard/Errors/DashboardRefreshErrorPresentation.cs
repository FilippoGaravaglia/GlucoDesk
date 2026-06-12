namespace GlucoDesk.Desktop.ViewModels.Dashboard.Errors;

/// <summary>
/// Represents a user-facing dashboard refresh error presentation.
/// </summary>
public sealed record DashboardRefreshErrorPresentation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardRefreshErrorPresentation"/> class.
    /// </summary>
    /// <param name="statusText">The short dashboard status text.</param>
    /// <param name="message">The user-facing error message.</param>
    /// <param name="technicalCode">The technical error code.</param>
    public DashboardRefreshErrorPresentation(
        string statusText,
        string message,
        string technicalCode)
    {
        if (string.IsNullOrWhiteSpace(statusText))
        {
            throw new ArgumentException("Status text must be specified.", nameof(statusText));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message must be specified.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(technicalCode))
        {
            throw new ArgumentException("Technical code must be specified.", nameof(technicalCode));
        }

        StatusText = statusText.Trim();
        Message = message.Trim();
        TechnicalCode = technicalCode.Trim();
    }

    /// <summary>
    /// Gets the short dashboard status text.
    /// </summary>
    public string StatusText { get; }

    /// <summary>
    /// Gets the user-facing error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the technical error code.
    /// </summary>
    public string TechnicalCode { get; }

    /// <summary>
    /// Gets the full error text shown in the dashboard.
    /// </summary>
    public string FullMessage => $"{Message} ({TechnicalCode})";
}