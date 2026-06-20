namespace GlucoDesk.Application.Cgm.Backfill.Enums;

/// <summary>
/// Defines the support status for historical CGM backfill.
/// </summary>
public enum CgmBackfillSupportStatus
{
    Supported = 0,
    Disabled = 1,
    ProviderDoesNotSupportHistoricalReadings = 2
}