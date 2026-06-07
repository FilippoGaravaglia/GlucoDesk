namespace GlucoDesk.Core.Tests;

public sealed class CoreAssemblyMarkerTests
{
    [Fact]
    public void CoreAssemblyMarker_ShouldReferenceCoreAssembly()
    {
        Assert.Equal(
            "GlucoDesk.Core",
            typeof(CoreAssemblyMarker).Assembly.GetName().Name);
    }
}