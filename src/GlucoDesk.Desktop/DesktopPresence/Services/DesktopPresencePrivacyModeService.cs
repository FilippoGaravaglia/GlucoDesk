using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

namespace GlucoDesk.Desktop.DesktopPresence.Services;

/// <summary>
/// Coordinates runtime privacy mode state and persists changes.
/// </summary>
public sealed class DesktopPresencePrivacyModeService : IDesktopPresencePrivacyModeService
{
    private readonly IDesktopPresencePrivacyModeStore _store;

    private bool _isEnabled;
    private bool _isLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopPresencePrivacyModeService"/> class.
    /// </summary>
    /// <param name="store">The privacy mode persistence store.</param>
    public DesktopPresencePrivacyModeService(IDesktopPresencePrivacyModeStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        _store = store;
    }

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public bool IsEnabled
    {
        get
        {
            EnsureLoaded();
            return _isEnabled;
        }
    }

    /// <inheritdoc />
    public void Reload()
    {
        var persistedValue = _store.Load();

        _isLoaded = true;
        ApplyState(persistedValue, persist: false);
    }

    /// <inheritdoc />
    public void SetEnabled(bool isEnabled)
    {
        EnsureLoaded();

        ApplyState(isEnabled, persist: true);
    }

    /// <inheritdoc />
    public void Toggle()
    {
        SetEnabled(!IsEnabled);
    }

    /// <summary>
    /// Ensures privacy mode state has been loaded from persistence.
    /// </summary>
    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        Reload();
    }

    /// <summary>
    /// Applies privacy mode state and optionally persists it.
    /// </summary>
    /// <param name="isEnabled">Whether privacy mode is enabled.</param>
    /// <param name="persist">Whether the new state should be persisted.</param>
    private void ApplyState(bool isEnabled, bool persist)
    {
        if (_isLoaded && _isEnabled == isEnabled)
        {
            if (persist)
            {
                _store.Save(isEnabled);
            }

            return;
        }

        _isEnabled = isEnabled;
        _isLoaded = true;

        if (persist)
        {
            _store.Save(isEnabled);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
