using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Providers;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Providers;

public sealed class NightscoutCgmProviderTests
{
    private static readonly DateTimeOffset FixedNow = DateTimeOffset.Parse("2026-06-12T09:00:00Z");

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnNightscoutMetadata()
    {
        var provider = CreateProvider();

        var result = await provider.GetMetadataAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Nightscout, result.Value.ProviderKind);
        Assert.Equal("Nightscout Test", result.Value.DisplayName);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, result.Value.ExpectedFreshness);
        Assert.True(result.Value.SupportsLiveReadings);
        Assert.True(result.Value.SupportsHistoricalReadings);
    }

    [Fact]
    public async Task GetLatestReadingAsync_ShouldReturnLatestReading()
    {
        var provider = CreateProvider(
            new FakeNightscoutEntriesClient
            {
                EntriesResult = Result<IReadOnlyList<NightscoutEntryDto>>.Success(
                [
                    new NightscoutEntryDto
                    {
                        Sgv = 120,
                        DateString = "2026-06-12T08:55:00.000Z",
                        Direction = "Flat"
                    }
                ])
            });

        var result = await provider.GetLatestReadingAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Reading);
        Assert.Equal(120m, result.Value.Reading!.Value.Amount);
        Assert.Equal(FixedNow, result.Value.RetrievedAt);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldReturnMappedReadings()
    {
        var provider = CreateProvider(
            new FakeNightscoutEntriesClient
            {
                EntriesResult = Result<IReadOnlyList<NightscoutEntryDto>>.Success(
                [
                    new NightscoutEntryDto
                    {
                        Sgv = 120,
                        DateString = "2026-06-12T08:50:00.000Z",
                        Direction = "Flat"
                    },
                    new NightscoutEntryDto
                    {
                        Sgv = 124,
                        DateString = "2026-06-12T08:55:00.000Z",
                        Direction = "SingleUp"
                    }
                ])
            });

        var request = GlucoseReadingsRequest.ForLast(
            TimeSpan.FromMinutes(30),
            FixedNow,
            limit: 10);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Readings.Count);
        Assert.Equal(FixedNow, result.Value.RetrievedAt);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldPropagateClientFailure()
    {
        var provider = CreateProvider(
            new FakeNightscoutEntriesClient
            {
                EntriesResult = Result<IReadOnlyList<NightscoutEntryDto>>.Failure(
                    new Error("Nightscout.EntriesNetworkError", "Network error."))
            });

        var request = GlucoseReadingsRequest.ForLast(
            TimeSpan.FromMinutes(30),
            FixedNow,
            limit: 10);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.EntriesNetworkError", result.Error.Code);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldApplyLimit()
    {
        var provider = CreateProvider(
            new FakeNightscoutEntriesClient
            {
                EntriesResult = Result<IReadOnlyList<NightscoutEntryDto>>.Success(
                [
                    new NightscoutEntryDto
                    {
                        Sgv = 110,
                        DateString = "2026-06-12T08:45:00.000Z"
                    },
                    new NightscoutEntryDto
                    {
                        Sgv = 120,
                        DateString = "2026-06-12T08:50:00.000Z"
                    },
                    new NightscoutEntryDto
                    {
                        Sgv = 130,
                        DateString = "2026-06-12T08:55:00.000Z"
                    }
                ])
            });

        var request = GlucoseReadingsRequest.ForLast(
            TimeSpan.FromMinutes(30),
            FixedNow,
            limit: 2);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value.Readings,
            first => Assert.Equal(120m, first.Value.Amount),
            second => Assert.Equal(130m, second.Value.Amount));
    }

    private static NightscoutCgmProvider CreateProvider(
        INightscoutEntriesClient? entriesClient = null)
    {
        return new NightscoutCgmProvider(
            new NightscoutOptions(
                new Uri("https://example-nightscout.test"),
                "Nightscout Test"),
            entriesClient ?? new FakeNightscoutEntriesClient
            {
                EntriesResult = Result<IReadOnlyList<NightscoutEntryDto>>.Success([])
            },
            new NightscoutEntryMapper(),
            new FakeTimeProvider(FixedNow));
    }

    private sealed class FakeNightscoutEntriesClient : INightscoutEntriesClient
    {
        public Result<IReadOnlyList<NightscoutEntryDto>> EntriesResult { get; init; } =
            Result<IReadOnlyList<NightscoutEntryDto>>.Success([]);

        /// <inheritdoc />
        public Task<Result<IReadOnlyList<NightscoutEntryDto>>> GetEntriesAsync(
            NightscoutEntriesRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(EntriesResult);
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}