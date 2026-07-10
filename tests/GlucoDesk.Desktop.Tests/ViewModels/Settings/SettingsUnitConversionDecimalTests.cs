using System.Reflection;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Settings;

/// <summary>
/// Regression tests for settings glucose unit conversion and decimal input handling.
/// </summary>
public sealed class SettingsUnitConversionDecimalTests
{
    [Theory]
    [InlineData("3.9", "3.9")]
    [InlineData("3,9", "3.9")]
    [InlineData("10.0", "10.0")]
    [InlineData("22.2", "22.2")]
    [InlineData("abc3.9xyz", "3.9")]
    [InlineData(".9", "0.9")]
    public void SanitizeTargetValueText_WhenUnitIsMmolL_ShouldPreserveSingleDecimalSeparator(
        string input,
        string expected)
    {
        // Act
        var actual = InvokeSanitizeTargetValueText(input, GlucoseUnit.MmolL);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("3.9", "39")]
    [InlineData("10.0", "100")]
    [InlineData("abc70xyz", "70")]
    public void SanitizeTargetValueText_WhenUnitIsMgDl_ShouldKeepIntegerOnlyBehavior(
        string input,
        string expected)
    {
        // Act
        var actual = InvokeSanitizeTargetValueText(input, GlucoseUnit.MgDl);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("70", GlucoseUnit.MgDl, GlucoseUnit.MmolL, "3.9")]
    [InlineData("180", GlucoseUnit.MgDl, GlucoseUnit.MmolL, "10.0")]
    [InlineData("400", GlucoseUnit.MgDl, GlucoseUnit.MmolL, "22.2")]
    [InlineData("3.9", GlucoseUnit.MmolL, GlucoseUnit.MgDl, "70")]
    [InlineData("10.0", GlucoseUnit.MmolL, GlucoseUnit.MgDl, "180")]
    [InlineData("22.2", GlucoseUnit.MmolL, GlucoseUnit.MgDl, "400")]
    public void ConvertEditableTargetText_ShouldRoundTripBetweenMgDlAndMmolL(
        string input,
        GlucoseUnit sourceUnit,
        GlucoseUnit targetUnit,
        string expected)
    {
        // Act
        var actual = InvokeConvertEditableTargetText(input, sourceUnit, targetUnit);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static string InvokeSanitizeTargetValueText(
        string input,
        GlucoseUnit unit)
    {
        return InvokePrivateStaticStringMethod(
            "SanitizeTargetValueText",
            input,
            unit);
    }

    private static string InvokeConvertEditableTargetText(
        string input,
        GlucoseUnit sourceUnit,
        GlucoseUnit targetUnit)
    {
        return InvokePrivateStaticStringMethod(
            "ConvertEditableTargetText",
            input,
            sourceUnit,
            targetUnit);
    }

    private static string InvokePrivateStaticStringMethod(
        string methodName,
        params object[] arguments)
    {
        var method = typeof(SettingsViewModel).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method!.Invoke(null, arguments);

        return Assert.IsType<string>(result);
    }
}
