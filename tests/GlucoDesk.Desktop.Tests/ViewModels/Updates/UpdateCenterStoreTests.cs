using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;
using GlucoDesk.Desktop.ViewModels.Updates;

namespace GlucoDesk.Desktop.Tests.ViewModels.Updates;

public sealed class UpdateCenterStoreTests
{
    [Fact]
    public void Snapshot_UpdateAvailable_ExposesBadge()
    {
        var store = new UpdateCenterStore
        {
            Snapshot = CreateSnapshot(
                UpdateCenterState.UpdateAvailable)
        };

        Assert.True(store.HasUpdate);
        Assert.False(store.IsUpToDate);
        Assert.False(store.HasError);
        Assert.Equal("1", store.BadgeText);
    }

    [Fact]
    public void Snapshot_UpToDate_DoesNotExposeBadge()
    {
        var store = new UpdateCenterStore
        {
            Snapshot = CreateSnapshot(
                UpdateCenterState.UpToDate)
        };

        Assert.False(store.HasUpdate);
        Assert.True(store.IsUpToDate);
        Assert.Null(store.BadgeText);
    }

    [Fact]
    public void Snapshot_Error_ExposesErrorState()
    {
        var store = new UpdateCenterStore
        {
            Snapshot = CreateSnapshot(
                UpdateCenterState.Error)
        };

        Assert.True(store.HasError);
        Assert.False(store.HasUpdate);
        Assert.Null(store.BadgeText);
    }

    #region Helpers

    /// <summary>
    /// Creates an update-center snapshot for store tests.
    /// </summary>
    private static UpdateCenterSnapshot CreateSnapshot(
        UpdateCenterState state)
    {
        return new UpdateCenterSnapshot
        {
            State = state,
            CurrentVersion = "0.3.0-preview"
        };
    }

    #endregion
}
