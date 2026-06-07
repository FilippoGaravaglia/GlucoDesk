using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Core.Tests.Glucose;

public sealed class GlucoseValueTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidAmount(decimal amount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseValue(amount, GlucoseUnit.MgDl));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void ConvertTo_ShouldConvertMgDlToMmolL()
    {
        var value = new GlucoseValue(180, GlucoseUnit.MgDl);

        var converted = value.ConvertTo(GlucoseUnit.MmolL);

        Assert.Equal(10.0m, converted.Amount);
        Assert.Equal(GlucoseUnit.MmolL, converted.Unit);
    }

    [Fact]
    public void ConvertTo_ShouldConvertMmolLToMgDl()
    {
        var value = new GlucoseValue(10.0m, GlucoseUnit.MmolL);

        var converted = value.ConvertTo(GlucoseUnit.MgDl);

        Assert.Equal(180m, converted.Amount);
        Assert.Equal(GlucoseUnit.MgDl, converted.Unit);
    }

    [Fact]
    public void ConvertTo_ShouldReturnSameInstance_WhenUnitIsAlreadyTheRequestedOne()
    {
        var value = new GlucoseValue(120, GlucoseUnit.MgDl);

        var converted = value.ConvertTo(GlucoseUnit.MgDl);

        Assert.Same(value, converted);
    }

    [Fact]
    public void ToString_ShouldFormatMgDlValue()
    {
        var value = new GlucoseValue(123, GlucoseUnit.MgDl);

        Assert.Equal("123 mg/dL", value.ToString());
    }

    [Fact]
    public void ToString_ShouldFormatMmolLValue()
    {
        var value = new GlucoseValue(6.8m, GlucoseUnit.MmolL);

        Assert.Equal("6.8 mmol/L", value.ToString());
    }
}