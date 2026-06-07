using FluentAssertions;

namespace GlucoDesk.Core.Tests;

public sealed class CoreAssemblyMarkerTests
{
    [Fact]
    public void CoreAssemblyMarker_ShouldReferenceCoreAssembly()
    {
        typeof(CoreAssemblyMarker)
            .Assembly
            .GetName()
            .Name
            .Should()
            .Be("GlucoDesk.Core");
    }
}