using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings.Providers;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;

namespace GlucoDesk.Desktop.Tests.ViewModels.Settings.Providers;

public sealed class ProviderActivationSelectorTests
{
    [Fact]
    public void CanActivate_ShouldReturnTrue_WhenProviderIsAvailable()
    {
        var providerOptions = CreateProviderOptions(
            new ProviderSelectionItem(
                CgmProviderKind.Nightscout,
                "Nightscout",
                true,
                "Provider is available."));

        var canActivate = ProviderActivationSelector.CanActivate(
            providerOptions,
            CgmProviderKind.Nightscout);

        Assert.True(canActivate);
    }

    [Fact]
    public void CanActivate_ShouldReturnFalse_WhenProviderIsUnavailable()
    {
        var providerOptions = CreateProviderOptions(
            new ProviderSelectionItem(
                CgmProviderKind.Nightscout,
                "Nightscout",
                false,
                "Provider is not configured."));

        var canActivate = ProviderActivationSelector.CanActivate(
            providerOptions,
            CgmProviderKind.Nightscout);

        Assert.False(canActivate);
    }

    [Fact]
    public void SelectAvailableProvider_ShouldReturnProvider_WhenProviderIsAvailable()
    {
        var providerOptions = CreateProviderOptions(
            new ProviderSelectionItem(
                CgmProviderKind.Nightscout,
                "Nightscout",
                true,
                "Provider is available."));

        var result = ProviderActivationSelector.SelectAvailableProvider(
            providerOptions,
            CgmProviderKind.Nightscout,
            "Nightscout.ProviderUnavailable",
            "Nightscout is not available.");

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Nightscout, result.Value.Kind);
    }

    [Fact]
    public void SelectAvailableProvider_ShouldReturnFailure_WhenProviderIsUnavailable()
    {
        var providerOptions = CreateProviderOptions(
            new ProviderSelectionItem(
                CgmProviderKind.Nightscout,
                "Nightscout",
                false,
                "Provider is not configured."));

        var result = ProviderActivationSelector.SelectAvailableProvider(
            providerOptions,
            CgmProviderKind.Nightscout,
            "Nightscout.ProviderUnavailable",
            "Nightscout is not available.");

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.ProviderUnavailable", result.Error.Code);
    }

    [Fact]
    public void SelectAvailableProvider_ShouldReturnFailure_WhenProviderIsMissing()
    {
        var providerOptions = CreateProviderOptions(
            new ProviderSelectionItem(
                CgmProviderKind.Mock,
                "Mock",
                true,
                "Provider is available."));

        var result = ProviderActivationSelector.SelectAvailableProvider(
            providerOptions,
            CgmProviderKind.Nightscout,
            "Nightscout.ProviderUnavailable",
            "Nightscout is not available.");

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.ProviderUnavailable", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates provider options for provider activation tests.
    /// </summary>
    /// <param name="providerOptions">The provider options.</param>
    /// <returns>The provider options collection.</returns>
    private static IReadOnlyList<ProviderSelectionItem> CreateProviderOptions(
        params ProviderSelectionItem[] providerOptions)
    {
        return providerOptions;
    }

    #endregion
}