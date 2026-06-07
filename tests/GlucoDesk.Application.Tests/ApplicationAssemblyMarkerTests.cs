namespace GlucoDesk.Application.Tests;

public sealed class ApplicationAssemblyMarkerTests
{
    [Fact]
    public void ApplicationAssemblyMarker_ShouldReferenceApplicationAssembly()
    {
        Assert.Equal(
            "GlucoDesk.Application",
            typeof(ApplicationAssemblyMarker).Assembly.GetName().Name);
    }
}