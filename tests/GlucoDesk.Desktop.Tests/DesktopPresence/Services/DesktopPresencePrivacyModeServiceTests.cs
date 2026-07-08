using GlucoDesk.Desktop.DesktopPresence.Services;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Services;

public sealed class DesktopPresencePrivacyModeServiceTests
{
    [Fact]
    public void IsEnabled_ShouldLoadPersistedStateLazily()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: true);
        var service = new DesktopPresencePrivacyModeService(store);

        Assert.True(service.IsEnabled);
        Assert.Equal(1, store.LoadCount);
    }

    [Fact]
    public void Reload_ShouldRefreshStateFromStore()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: false);
        var service = new DesktopPresencePrivacyModeService(store);

        Assert.False(service.IsEnabled);

        store.CurrentValue = true;
        service.Reload();

        Assert.True(service.IsEnabled);
    }

    [Fact]
    public void SetEnabled_ShouldPersistState()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: false);
        var service = new DesktopPresencePrivacyModeService(store);

        service.SetEnabled(true);

        Assert.True(service.IsEnabled);
        Assert.True(store.CurrentValue);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public void Toggle_ShouldInvertAndPersistState()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: false);
        var service = new DesktopPresencePrivacyModeService(store);

        service.Toggle();

        Assert.True(service.IsEnabled);
        Assert.True(store.CurrentValue);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public void SetEnabled_ShouldRaiseStateChanged_WhenStateChanges()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: false);
        var service = new DesktopPresencePrivacyModeService(store);

        var eventCount = 0;
        service.StateChanged += (_, _) => eventCount++;

        service.SetEnabled(true);

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void SetEnabled_ShouldNotRaiseStateChanged_WhenStateDoesNotChange()
    {
        var store = new InMemoryPrivacyModeStore(initialValue: false);
        var service = new DesktopPresencePrivacyModeService(store);

        _ = service.IsEnabled;

        var eventCount = 0;
        service.StateChanged += (_, _) => eventCount++;

        service.SetEnabled(false);

        Assert.Equal(0, eventCount);
    }

    private sealed class InMemoryPrivacyModeStore : IDesktopPresencePrivacyModeStore
    {
        public InMemoryPrivacyModeStore(bool initialValue)
        {
            CurrentValue = initialValue;
        }

        public bool CurrentValue { get; set; }

        public int LoadCount { get; private set; }

        public int SaveCount { get; private set; }

        public bool Load()
        {
            LoadCount++;
            return CurrentValue;
        }

        public void Save(bool isEnabled)
        {
            SaveCount++;
            CurrentValue = isEnabled;
        }
    }
}
