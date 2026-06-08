using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Application.Settings.Services;

namespace GlucoDesk.Application.Tests.Settings.Services;

public sealed class ApplicationSettingsServiceTests
{
    [Fact]
    public async Task GetSettingsAsync_ShouldReturnSettingsFromStore()
    {
        var expectedSettings = new ApplicationSettings(dashboardRefreshInterval: TimeSpan.FromSeconds(45));
        var store = new FakeApplicationSettingsStore(expectedSettings);
        var service = new ApplicationSettingsService(store);

        var result = await service.GetSettingsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedSettings, result.Value);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldSaveSettingsThroughStore()
    {
        var settings = new ApplicationSettings(dashboardRefreshInterval: TimeSpan.FromSeconds(45));
        var store = new FakeApplicationSettingsStore(ApplicationSettings.Default);
        var service = new ApplicationSettingsService(store);

        var result = await service.SaveSettingsAsync(settings, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(settings, store.SavedSettings);
    }

    #region Helpers

    private sealed class FakeApplicationSettingsStore : IApplicationSettingsStore
    {
        private readonly ApplicationSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeApplicationSettingsStore"/> class.
        /// </summary>
        /// <param name="settings">The settings returned by the fake store.</param>
        public FakeApplicationSettingsStore(ApplicationSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Gets the last saved settings.
        /// </summary>
        public ApplicationSettings? SavedSettings { get; private set; }

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<ApplicationSettings>.Success(_settings));
        }

        /// <inheritdoc />
        public Task<Result> SaveAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            SavedSettings = settings;

            return Task.FromResult(Result.Success());
        }
    }

    #endregion
}