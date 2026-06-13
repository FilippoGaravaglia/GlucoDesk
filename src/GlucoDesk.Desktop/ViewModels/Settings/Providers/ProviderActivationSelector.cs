using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;

namespace GlucoDesk.Desktop.ViewModels.Settings.Providers;

/// <summary>
/// Provides reusable provider activation selection logic for the settings screen.
/// </summary>
public static class ProviderActivationSelector
{
    /// <summary>
    /// Checks whether the requested provider can be activated from the current provider options.
    /// </summary>
    /// <param name="providerOptions">The available provider options.</param>
    /// <param name="providerKind">The provider kind to activate.</param>
    /// <returns>True when the provider can be activated; otherwise false.</returns>
    public static bool CanActivate(
        IReadOnlyCollection<ProviderSelectionItem> providerOptions,
        CgmProviderKind providerKind)
    {
        ArgumentNullException.ThrowIfNull(providerOptions);

        return providerOptions.Any(provider =>
            provider.Kind == providerKind && provider.IsAvailable);
    }

    /// <summary>
    /// Selects an available provider option by provider kind.
    /// </summary>
    /// <param name="providerOptions">The available provider options.</param>
    /// <param name="providerKind">The provider kind to activate.</param>
    /// <param name="errorCode">The error code used when the provider cannot be selected.</param>
    /// <param name="errorMessage">The error message used when the provider cannot be selected.</param>
    /// <returns>The selected provider option when available; otherwise a failed result.</returns>
    public static Result<ProviderSelectionItem> SelectAvailableProvider(
        IReadOnlyCollection<ProviderSelectionItem> providerOptions,
        CgmProviderKind providerKind,
        string errorCode,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(providerOptions);

        var provider = providerOptions.FirstOrDefault(candidate =>
            candidate.Kind == providerKind && candidate.IsAvailable);

        return provider is not null
            ? Result<ProviderSelectionItem>.Success(provider)
            : Result<ProviderSelectionItem>.Failure(
                new Error(
                    errorCode,
                    errorMessage));
    }
}