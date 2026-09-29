using GlucoDesk.Application.Updates;

namespace GlucoDesk.Application.Tests.Updates;

public sealed class ReleaseVersionComparerTests
{
    [Theory]
    [InlineData("0.4.0-preview", "0.3.0-preview", true)]
    [InlineData("v0.4.0-preview", "0.3.0-preview", true)]
    [InlineData("0.3.0-preview", "0.3.0-preview", false)]
    [InlineData("0.2.0-preview", "0.3.0-preview", false)]
    [InlineData("0.4.0", "0.4.0-preview", true)]
    [InlineData("0.4.0-preview", "0.4.0", false)]
    public void IsNewer_ReturnsExpectedResult(
        string candidate,
        string current,
        bool expected)
    {
        var result = ReleaseVersionComparer.IsNewer(
            candidate,
            current);

        Assert.Equal(expected, result);
    }
}
