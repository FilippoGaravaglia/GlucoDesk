using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.States;

public sealed class DexcomOAuthStateOptionsTests
{
    [Fact]
    public void Default_ShouldUseExpectedLength()
    {
        Assert.Equal(32, DexcomOAuthStateOptions.Default.StateLengthBytes);
    }

    [Fact]
    public void Constructor_ShouldCreateOptions_WhenLengthIsValid()
    {
        var options = new DexcomOAuthStateOptions(64);

        Assert.Equal(64, options.StateLengthBytes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(129)]
    public void Constructor_ShouldRejectInvalidLength(int stateLengthBytes)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthStateOptions(stateLengthBytes));

        Assert.Equal("stateLengthBytes", exception.ParamName);
    }
}