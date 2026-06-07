using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Tests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var error = new Error("Provider.Failed", "Provider failed.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_ShouldRejectEmptyError()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Result.Failure(Error.None));

        Assert.Equal("A failed result must contain an error.", exception.Message);
    }

    [Fact]
    public void GenericSuccess_ShouldExposeValue()
    {
        var result = Result<string>.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public void GenericFailure_ShouldRejectValueAccess()
    {
        var result = Result<string>.Failure(new Error("Test.Failed", "Test failed."));

        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);

        Assert.Equal("The value of a failed result cannot be accessed.", exception.Message);
    }
}