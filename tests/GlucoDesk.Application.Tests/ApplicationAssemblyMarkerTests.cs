using FluentAssertions;

namespace GlucoDesk.Application.Tests;

public sealed class ApplicationAssemblyMarkerTests
{
    [Fact]
    public void ApplicationAssemblyMarker_ShouldReferenceApplicationAssembly()
    {
        typeof(ApplicationAssemblyMarker)
            .Assembly
            .GetName()
            .Name
            .Should()
            .Be("GlucoDesk.Application");
    }
}