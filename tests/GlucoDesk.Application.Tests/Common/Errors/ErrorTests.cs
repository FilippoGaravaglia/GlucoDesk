using GlucoDesk.Application.Common.Errors;

namespace GlucoDesk.Application.Tests.Common.Errors;

public sealed class ErrorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidCode(string code)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Error(code, "Message"));

        Assert.Equal("code", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidMessage(string message)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Error("Code", message));

        Assert.Equal("message", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimCodeAndMessage()
    {
        var error = new Error("  Provider.Error  ", "  Something failed  ");

        Assert.Equal("Provider.Error", error.Code);
        Assert.Equal("Something failed", error.Message);
    }
}