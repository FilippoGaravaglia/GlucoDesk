namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

/// <summary>
/// Represents the supported Dexcom API environments.
/// </summary>
public enum DexcomApiEnvironment
{
    /// <summary>
    /// Dexcom sandbox environment.
    /// </summary>
    Sandbox = 0,

    /// <summary>
    /// Dexcom production environment for the United States.
    /// </summary>
    ProductionUs = 1,

    /// <summary>
    /// Dexcom production environment for Europe and outside of the United States.
    /// </summary>
    ProductionEu = 2,

    /// <summary>
    /// Dexcom production environment for Japan.
    /// </summary>
    ProductionJapan = 3
}