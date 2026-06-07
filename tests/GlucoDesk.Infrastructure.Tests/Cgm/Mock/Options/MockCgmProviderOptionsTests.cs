using GlucoDesk.Infrastructure.Cgm.Mock.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Mock.Options;

public sealed class MockCgmProviderOptionsTests
{
    [Fact]
    public void Constructor_ShouldCreateDefaultOptions()
    {
        var options = MockCgmProviderOptions.Default;

        Assert.Equal(120, options.BaseValue);
        Assert.Equal(55, options.MinimumValue);
        Assert.Equal(250, options.MaximumValue);
        Assert.Equal(35, options.Variation);
        Assert.Equal(TimeSpan.FromMinutes(5), options.ReadingInterval);
        Assert.Equal("GlucoDesk Mock CGM", options.DeviceName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidMinimumValue()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MockCgmProviderOptions(minimumValue: 0));

        Assert.Equal("minimumValue", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectMaximumValueLowerThanMinimumValue()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MockCgmProviderOptions(minimumValue: 100, maximumValue: 100));

        Assert.Equal("maximumValue", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectBaseValueOutsideRange()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MockCgmProviderOptions(baseValue: 300));

        Assert.Equal("baseValue", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeVariation()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MockCgmProviderOptions(variation: -1));

        Assert.Equal("variation", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidReadingInterval()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MockCgmProviderOptions(readingInterval: TimeSpan.Zero));

        Assert.Equal("readingInterval", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidDeviceName(string deviceName)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MockCgmProviderOptions(deviceName: deviceName));

        Assert.Equal("deviceName", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimDeviceName()
    {
        var options = new MockCgmProviderOptions(deviceName: "  Test CGM  ");

        Assert.Equal("Test CGM", options.DeviceName);
    }
}