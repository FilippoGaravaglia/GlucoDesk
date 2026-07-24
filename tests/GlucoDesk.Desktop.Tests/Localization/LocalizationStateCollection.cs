namespace GlucoDesk.Desktop.Tests.Localization;

/// <summary>
/// Prevents tests that mutate process-wide localization state from running
/// concurrently with other desktop tests.
/// </summary>
[CollectionDefinition(
    Name,
    DisableParallelization = true)]
public sealed class LocalizationStateCollection
{
    /// <summary>
    /// The xUnit collection name.
    /// </summary>
    public const string Name =
        "Process-wide localization state";
}
