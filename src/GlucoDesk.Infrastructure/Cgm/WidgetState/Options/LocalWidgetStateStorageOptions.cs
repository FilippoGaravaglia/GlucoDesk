using GlucoDesk.Infrastructure.Storage;

namespace GlucoDesk.Infrastructure.Cgm.WidgetState.Options;

/// <summary>
/// Provides local widget state storage options.
/// </summary>
public sealed record LocalWidgetStateStorageOptions
{
    /// <summary>
    /// Gets the default widget state storage options.
    /// </summary>
    public static LocalWidgetStateStorageOptions Default => new(
        LocalApplicationDataDirectory.GetFilePath("glucodesk-widget-state.json"));

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalWidgetStateStorageOptions"/> class.
    /// </summary>
    /// <param name="stateFilePath">The widget state file path.</param>
    public LocalWidgetStateStorageOptions(string stateFilePath)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath))
        {
            throw new ArgumentException(
                "Widget state file path cannot be empty.",
                nameof(stateFilePath));
        }

        StateFilePath = stateFilePath;
    }

    /// <summary>
    /// Gets the widget state file path.
    /// </summary>
    public string StateFilePath { get; }
}