using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Requests;

/// <summary>
/// Represents a request for glycemic diary generation.
/// </summary>
public sealed record GlycemicDiaryRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryRequest"/> class.
    /// </summary>
    /// <param name="periodStartsAt">The diary period start timestamp.</param>
    /// <param name="periodEndsAt">The diary period end timestamp.</param>
    /// <param name="providerKind">The CGM provider whose history must be used.</param>
    public GlycemicDiaryRequest(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        CgmProviderKind providerKind)
    {
        if (periodEndsAt <= periodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodEndsAt),
                periodEndsAt,
                "Diary period end timestamp must be greater than start timestamp.");
        }

        if (providerKind == CgmProviderKind.Unknown ||
            !Enum.IsDefined(providerKind))
        {
            throw new ArgumentException(
                "Diary provider kind must be specified.",
                nameof(providerKind));
        }

        PeriodStartsAt = periodStartsAt;
        PeriodEndsAt = periodEndsAt;
        ProviderKind = providerKind;
    }

    /// <summary>
    /// Gets the diary period start timestamp.
    /// </summary>
    public DateTimeOffset PeriodStartsAt { get; }

    /// <summary>
    /// Gets the diary period end timestamp.
    /// </summary>
    public DateTimeOffset PeriodEndsAt { get; }

    /// <summary>
    /// Gets the CGM provider whose readings must be included.
    /// </summary>
    public CgmProviderKind ProviderKind { get; }
}
