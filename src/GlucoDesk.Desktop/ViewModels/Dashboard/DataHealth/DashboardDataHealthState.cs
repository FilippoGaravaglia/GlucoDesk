namespace GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;

/// <summary>
/// Represents the user-facing dashboard data health state.
/// </summary>
public enum DashboardDataHealthState
{
    Unknown = 0,
    MockData = 1,
    FreshRealData = 2,
    StaleRealData = 3,
    NoReadings = 4,
    ProviderError = 5
}