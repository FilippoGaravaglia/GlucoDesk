namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;

/// <summary>
/// Defines secure Dexcom OAuth state generation.
/// </summary>
public interface IDexcomOAuthStateGenerator
{
    /// <summary>
    /// Generates a URL-safe OAuth state value.
    /// </summary>
    /// <returns>The generated OAuth state value.</returns>
    string GenerateState();
}