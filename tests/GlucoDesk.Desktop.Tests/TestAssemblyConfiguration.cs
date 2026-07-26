[assembly: CollectionBehavior(
    DisableTestParallelization = true)]

namespace GlucoDesk.Desktop.Tests;

/// <summary>
/// Configures deterministic execution for desktop tests.
/// </summary>
/// <remarks>
/// Desktop tests share process-wide UI state such as the active localization,
/// dynamic resources and desktop preference stores. Running tests that mutate
/// those values in parallel can cause one test to observe another test's
/// language or preferences.
///
/// Disabling intra-assembly parallelization keeps every test isolated without
/// changing production behavior. The other GlucoDesk test projects can still
/// execute concurrently when the complete solution is tested.
/// </remarks>
internal static class TestAssemblyConfiguration
{
}
