using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Providers;

public sealed class DexcomOfficialCgmProviderTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnSandboxMetadata_WhenEnvironmentIsSandbox()
    {
        var provider = CreateProvider(environment: DexcomApiEnvironment.Sandbox);

        var result = await provider.GetMetadataAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.DexcomSandbox, result.Value.ProviderKind);
        Assert.Equal("Dexcom Official API", result.Value.DisplayName);
        Assert.Equal(GlucoseDataFreshness.Delayed, result.Value.ExpectedFreshness);
        Assert.True(result.Value.SupportsLiveReadings);
        Assert.True(result.Value.SupportsHistoricalReadings);
    }

    [Theory]
    [InlineData(DexcomApiEnvironment.ProductionUs)]
    [InlineData(DexcomApiEnvironment.ProductionEu)]
    [InlineData(DexcomApiEnvironment.ProductionJapan)]
    public async Task GetMetadataAsync_ShouldReturnOfficialMetadata_WhenEnvironmentIsProduction(
        DexcomApiEnvironment environment)
    {
        var provider = CreateProvider(environment: environment);

        var result = await provider.GetMetadataAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.DexcomOfficial, result.Value.ProviderKind);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnMappedReadings_WhenDexcomCallSucceeds()
    {
        var reading = CreateReading(FixedNow.AddMinutes(-10), 101);
        var egvClient = new FakeDexcomEgvClient();
        var egvMapper = new FakeDexcomEgvMapper
        {
            MapResponseResult = Result<IReadOnlyList<GlucoseReading>>.Success(
                new List<GlucoseReading> { reading })
        };

        var provider = CreateProvider(egvClient: egvClient, egvMapper: egvMapper);

        var request = new GlucoseReadingsRequest(
            FixedNow.AddHours(-2),
            FixedNow);

        var result = await provider.GetReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReadings);
        Assert.Same(reading, result.Value.Readings.Single());

        Assert.NotNull(egvClient.LastRequest);
        Assert.Equal("client-secret", egvClient.LastRequest.ClientSecret);
        Assert.Equal(request.From.ToUniversalTime(), egvClient.LastRequest.StartDateUtc);
        Assert.Equal(request.To.ToUniversalTime(), egvClient.LastRequest.EndDateUtc);
        Assert.False(egvClient.LastRequest.ForceTokenRefresh);

        Assert.Same(egvClient.ResponseResult.Value, egvMapper.LastResponse);
        Assert.Equal(DexcomApiEnvironment.Sandbox, egvMapper.LastEnvironment);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldApplyLimitToLatestMappedReadings()
    {
        var oldReading = CreateReading(FixedNow.AddMinutes(-20), 90);
        var middleReading = CreateReading(FixedNow.AddMinutes(-10), 100);
        var latestReading = CreateReading(FixedNow.AddMinutes(-5), 110);

        var egvMapper = new FakeDexcomEgvMapper
        {
            MapResponseResult = Result<IReadOnlyList<GlucoseReading>>.Success(
                new List<GlucoseReading>
                {
                    oldReading,
                    middleReading,
                    latestReading
                })
        };

        var provider = CreateProvider(egvMapper: egvMapper);

        var request = new GlucoseReadingsRequest(
            FixedNow.AddHours(-2),
            FixedNow,
            limit: 2);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Readings.Count);
        Assert.DoesNotContain(oldReading, result.Value.Readings);
        Assert.Contains(middleReading, result.Value.Readings);
        Assert.Contains(latestReading, result.Value.Readings);
    }

    [Fact]
    public async Task GetLatestReadingAsync_ShouldReturnLatestMappedReading()
    {
        var olderReading = CreateReading(FixedNow.AddMinutes(-20), 90);
        var latestReading = CreateReading(FixedNow.AddMinutes(-5), 110);

        var egvMapper = new FakeDexcomEgvMapper
        {
            MapResponseResult = Result<IReadOnlyList<GlucoseReading>>.Success(
                new List<GlucoseReading>
                {
                    latestReading,
                    olderReading
                })
        };

        var provider = CreateProvider(egvMapper: egvMapper);

        var result = await provider.GetLatestReadingAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReading);
        Assert.Same(latestReading, result.Value.Reading);
        Assert.Equal(FixedNow, result.Value.RetrievedAt);
    }

    [Fact]
    public async Task GetLatestReadingAsync_ShouldReturnEmptyResult_WhenNoReadingsAreAvailable()
    {
        var egvMapper = new FakeDexcomEgvMapper
        {
            MapResponseResult = Result<IReadOnlyList<GlucoseReading>>.Success(
                Array.Empty<GlucoseReading>())
        };

        var provider = CreateProvider(egvMapper: egvMapper);

        var result = await provider.GetLatestReadingAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasReading);
        Assert.Null(result.Value.Reading);
        Assert.Equal(FixedNow, result.Value.RetrievedAt);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnFailure_WhenClientSecretIsMissing()
    {
        var egvClient = new FakeDexcomEgvClient();

        var provider = CreateProvider(
            providerOptions: DexcomCgmProviderOptions.Default,
            egvClient: egvClient);

        var result = await provider.GetReadingsAsync(
            new GlucoseReadingsRequest(FixedNow.AddHours(-1), FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.ProviderClientSecretMissing", result.Error.Code);
        Assert.Null(egvClient.LastRequest);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnFailure_WhenRequestRangeIsNotSupported()
    {
        var provider = CreateProvider();

        var result = await provider.GetReadingsAsync(
            new GlucoseReadingsRequest(
                FixedNow.AddDays(-31),
                FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.ProviderInvalidReadingsRequest", result.Error.Code);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnFailure_WhenEgvClientFails()
    {
        var egvClient = new FakeDexcomEgvClient
        {
            ResponseResult = Result<DexcomEgvResponseDto>.Failure(
                new Error("Dexcom.EgvRequestFailed", "EGV request failed."))
        };

        var provider = CreateProvider(egvClient: egvClient);

        var result = await provider.GetReadingsAsync(
            new GlucoseReadingsRequest(FixedNow.AddHours(-1), FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvRequestFailed", result.Error.Code);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnFailure_WhenMapperFails()
    {
        var egvMapper = new FakeDexcomEgvMapper
        {
            MapResponseResult = Result<IReadOnlyList<GlucoseReading>>.Failure(
                new Error("Dexcom.EgvInvalidSystemTime", "Invalid system time."))
        };

        var provider = CreateProvider(egvMapper: egvMapper);

        var result = await provider.GetReadingsAsync(
            new GlucoseReadingsRequest(FixedNow.AddHours(-1), FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvInvalidSystemTime", result.Error.Code);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldRejectNullRequest()
    {
        var provider = CreateProvider();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetReadingsAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldRejectNullRequest()
    {
        var provider = CreateProvider();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetRecentReadingsAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a Dexcom Official CGM provider for tests.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="providerOptions">The optional provider options.</param>
    /// <param name="egvClient">The optional fake EGV client.</param>
    /// <param name="egvMapper">The optional fake EGV mapper.</param>
    /// <returns>The Dexcom Official CGM provider.</returns>
    private static DexcomOfficialCgmProvider CreateProvider(
        DexcomApiEnvironment environment = DexcomApiEnvironment.Sandbox,
        DexcomCgmProviderOptions? providerOptions = null,
        FakeDexcomEgvClient? egvClient = null,
        FakeDexcomEgvMapper? egvMapper = null)
    {
        return new DexcomOfficialCgmProvider(
            CreateApiOptions(environment),
            providerOptions ?? new DexcomCgmProviderOptions("client-secret"),
            egvClient ?? new FakeDexcomEgvClient(),
            egvMapper ?? new FakeDexcomEgvMapper(),
            new TestTimeProvider(FixedNow));
    }

    /// <summary>
    /// Creates Dexcom API options for tests.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The Dexcom API options.</returns>
    private static DexcomApiOptions CreateApiOptions(DexcomApiEnvironment environment)
    {
        return new DexcomApiOptions(
            environment,
            "client-id",
            new Uri("http://127.0.0.1:51234/callback"),
            ["egv", "offline_access"]);
    }

    /// <summary>
    /// Creates a normalized glucose reading.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="value">The glucose value.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp, int value)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(value, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.DexcomSandbox,
            GlucoseDataFreshness.Delayed,
            "Dexcom G7");
    }

    private sealed class FakeDexcomEgvClient : IDexcomEgvClient
    {
        /// <summary>
        /// Gets or sets the Dexcom EGV response result.
        /// </summary>
        public Result<DexcomEgvResponseDto> ResponseResult { get; set; } =
            Result<DexcomEgvResponseDto>.Success(
                new DexcomEgvResponseDto
                {
                    RecordType = "egv",
                    RecordVersion = "3.0",
                    UserId = "user-id",
                    Records = []
                });

        /// <summary>
        /// Gets the last Dexcom EGV request.
        /// </summary>
        public DexcomEgvRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomEgvResponseDto>> GetEgvsAsync(
            DexcomEgvRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(ResponseResult);
        }
    }

    private sealed class FakeDexcomEgvMapper : IDexcomEgvMapper
    {
        /// <summary>
        /// Gets or sets the map response result.
        /// </summary>
        public Result<IReadOnlyList<GlucoseReading>> MapResponseResult { get; set; } =
            Result<IReadOnlyList<GlucoseReading>>.Success(
                Array.Empty<GlucoseReading>());

        /// <summary>
        /// Gets the last Dexcom EGV response.
        /// </summary>
        public DexcomEgvResponseDto? LastResponse { get; private set; }

        /// <summary>
        /// Gets the last Dexcom API environment.
        /// </summary>
        public DexcomApiEnvironment? LastEnvironment { get; private set; }

        /// <inheritdoc />
        public Result<IReadOnlyList<GlucoseReading>> MapResponse(
            DexcomEgvResponseDto response,
            DexcomApiEnvironment environment)
        {
            LastResponse = response;
            LastEnvironment = environment;

            return MapResponseResult;
        }

        /// <inheritdoc />
        public Result<GlucoseReading> MapRecord(
            DexcomEgvRecordDto record,
            DexcomApiEnvironment environment)
        {
            return Result<GlucoseReading>.Failure(
                new Error("Dexcom.MapRecordNotSupported", "MapRecord is not supported by this fake."));
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTimeProvider"/> class.
        /// </summary>
        /// <param name="utcNow">The fixed UTC timestamp.</param>
        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    #endregion
}