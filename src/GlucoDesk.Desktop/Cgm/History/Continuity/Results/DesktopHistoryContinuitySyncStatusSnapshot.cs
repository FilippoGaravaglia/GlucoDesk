using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Enums;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Results;

/// <summary>
/// Represents a desktop-visible snapshot of the history continuity synchronization state.
/// </summary>
/// <param name="State">The current synchronization state.</param>
/// <param name="Trigger">The trigger that started the current or latest synchronization.</param>
/// <param name="StartedAtUtc">The UTC timestamp when the current or latest synchronization started.</param>
/// <param name="CompletedAtUtc">The UTC timestamp when the latest synchronization completed.</param>
/// <param name="LastSuccessfulSyncAtUtc">The UTC timestamp of the latest successful synchronization.</param>
/// <param name="Message">The user-facing status message.</param>
/// <param name="ErrorCode">The latest error code, when available.</param>
/// <param name="ErrorDescription">The latest error description, when available.</param>
/// <param name="TotalFetchedReadings">The number of readings fetched by the latest synchronization.</param>
/// <param name="AddedReadingsCount">The number of readings added to local history.</param>
/// <param name="DuplicateReadingsCount">The number of duplicate readings ignored.</param>
/// <param name="StoredReadingsCount">The number of readings stored after synchronization.</param>
/// <param name="HasNewReadings">Whether the latest synchronization added new readings.</param>
public sealed record DesktopHistoryContinuitySyncStatusSnapshot(
    DesktopHistoryContinuitySyncRunState State,
    CgmHistoryContinuitySyncTrigger? Trigger,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? LastSuccessfulSyncAtUtc,
    string Message,
    string? ErrorCode,
    string? ErrorDescription,
    int TotalFetchedReadings,
    int AddedReadingsCount,
    int DuplicateReadingsCount,
    int StoredReadingsCount,
    bool HasNewReadings)
{
    /// <summary>
    /// Gets the initial idle status snapshot.
    /// </summary>
    public static DesktopHistoryContinuitySyncStatusSnapshot Idle { get; } = new(
        DesktopHistoryContinuitySyncRunState.Idle,
        Trigger: null,
        StartedAtUtc: null,
        CompletedAtUtc: null,
        LastSuccessfulSyncAtUtc: null,
        Message: "History continuity synchronization has not run yet.",
        ErrorCode: null,
        ErrorDescription: null,
        TotalFetchedReadings: 0,
        AddedReadingsCount: 0,
        DuplicateReadingsCount: 0,
        StoredReadingsCount: 0,
        HasNewReadings: false);
}