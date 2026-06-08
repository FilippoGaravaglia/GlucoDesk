using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.States;

public sealed class DexcomOAuthStateGeneratorTests
{
    [Fact]
    public void GenerateState_ShouldCreateUrlSafeState()
    {
        var generator = new DexcomOAuthStateGenerator(new DexcomOAuthStateOptions(32));

        var state = generator.GenerateState();

        Assert.False(string.IsNullOrWhiteSpace(state));
        Assert.DoesNotContain("+", state, StringComparison.Ordinal);
        Assert.DoesNotContain("/", state, StringComparison.Ordinal);
        Assert.DoesNotContain("=", state, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateState_ShouldCreateDifferentValues()
    {
        var generator = new DexcomOAuthStateGenerator(new DexcomOAuthStateOptions(32));

        var firstState = generator.GenerateState();
        var secondState = generator.GenerateState();

        Assert.NotEqual(firstState, secondState);
    }

    [Fact]
    public void Constructor_ShouldRejectNullOptions()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomOAuthStateGenerator(null!));

        Assert.Equal("options", exception.ParamName);
    }
}