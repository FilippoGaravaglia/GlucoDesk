using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.Providers.Metadata;

public sealed class CgmProviderMetadataTests
{
    [Fact]
    public void Constructor_ShouldRejectUnknownProviderKind()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CgmProviderMetadata(
                CgmProviderKind.Unknown,
                "Mock",
                GlucoseDataFreshness.Live,
                supportsLiveReadings: true,
                supportsHistoricalReadings: false));

        Assert.Equal("providerKind", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidDisplayName(string displayName)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CgmProviderMetadata(
                CgmProviderKind.Mock,
                displayName,
                GlucoseDataFreshness.Live,
                supportsLiveReadings: true,
                supportsHistoricalReadings: false));

        Assert.Equal("displayName", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownFreshness()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CgmProviderMetadata(
                CgmProviderKind.Mock,
                "Mock",
                GlucoseDataFreshness.Unknown,
                supportsLiveReadings: true,
                supportsHistoricalReadings: false));

        Assert.Equal("expectedFreshness", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectProviderWithoutReadingCapabilities()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CgmProviderMetadata(
                CgmProviderKind.Mock,
                "Mock",
                GlucoseDataFreshness.Live,
                supportsLiveReadings: false,
                supportsHistoricalReadings: false));

        Assert.Equal("supportsLiveReadings", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimDisplayName()
    {
        var metadata = new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "  Mock Provider  ",
            GlucoseDataFreshness.Live,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);

        Assert.Equal("Mock Provider", metadata.DisplayName);
    }
}