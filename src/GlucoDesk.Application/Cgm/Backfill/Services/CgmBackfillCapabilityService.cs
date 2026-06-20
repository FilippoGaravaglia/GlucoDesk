using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Options;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Default implementation of <see cref="ICgmBackfillCapabilityService"/>.
/// </summary>
public sealed class CgmBackfillCapabilityService : ICgmBackfillCapabilityService
{
    private readonly IGlucoseDataService _glucoseDataService;
    private readonly CgmBackfillCapabilityOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillCapabilityService"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    /// <param name="options">The backfill capability options.</param>
    public CgmBackfillCapabilityService(
        IGlucoseDataService glucoseDataService,
        CgmBackfillCapabilityOptions options)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        _glucoseDataService = glucoseDataService;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillCapability>> GetCapabilityAsync(
        CancellationToken cancellationToken)
    {
        var metadataResult = await _glucoseDataService.GetProviderMetadataAsync(cancellationToken);

        if (metadataResult.IsFailure)
        {
            return Result<CgmBackfillCapability>.Failure(metadataResult.Error);
        }

        if (!_options.IsEnabled)
        {
            return Result<CgmBackfillCapability>.Success(
                new CgmBackfillCapability(
                    IsSupported: false,
                    Status: CgmBackfillSupportStatus.Disabled,
                    MaximumLookback: null,
                    MinimumGapDuration: null,
                    Message: "Historical backfill is disabled by application configuration."));
        }

        if (!metadataResult.Value.SupportsHistoricalReadings)
        {
            return Result<CgmBackfillCapability>.Success(
                new CgmBackfillCapability(
                    IsSupported: false,
                    Status: CgmBackfillSupportStatus.ProviderDoesNotSupportHistoricalReadings,
                    MaximumLookback: null,
                    MinimumGapDuration: null,
                    Message: "The active CGM provider does not support historical readings."));
        }

        return Result<CgmBackfillCapability>.Success(
            new CgmBackfillCapability(
                IsSupported: true,
                Status: CgmBackfillSupportStatus.Supported,
                MaximumLookback: _options.MaximumLookback,
                MinimumGapDuration: _options.MinimumGapDuration,
                Message: "The active CGM provider supports historical backfill."));
    }

    #region Helpers

    /// <summary>
    /// Validates the backfill capability options.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    private static void ValidateOptions(CgmBackfillCapabilityOptions options)
    {
        if (options.MaximumLookback <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Maximum lookback must be greater than zero.");
        }

        if (options.MinimumGapDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Minimum gap duration must be greater than zero.");
        }
    }

    #endregion
}