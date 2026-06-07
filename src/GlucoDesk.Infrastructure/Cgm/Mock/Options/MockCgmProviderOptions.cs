namespace GlucoDesk.Infrastructure.Cgm.Mock.Options;

/// <summary>
/// Represents configuration options used by the mock CGM provider.
/// </summary>
public sealed record MockCgmProviderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockCgmProviderOptions"/> class.
    /// </summary>
    /// <param name="baseValue">The baseline glucose value expressed in mg/dL.</param>
    /// <param name="minimumValue">The minimum generated glucose value expressed in mg/dL.</param>
    /// <param name="maximumValue">The maximum generated glucose value expressed in mg/dL.</param>
    /// <param name="variation">The maximum variation applied around the baseline value.</param>
    /// <param name="readingInterval">The simulated CGM reading interval.</param>
    /// <param name="deviceName">The simulated device name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when numeric options are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the device name is invalid.</exception>
    public MockCgmProviderOptions(
        int baseValue = 120,
        int minimumValue = 55,
        int maximumValue = 250,
        int variation = 35,
        TimeSpan? readingInterval = null,
        string deviceName = "GlucoDesk Mock CGM")
    {
        var effectiveReadingInterval = readingInterval ?? TimeSpan.FromMinutes(5);

        if (minimumValue <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumValue),
                minimumValue,
                "Minimum glucose value must be greater than zero.");
        }

        if (maximumValue <= minimumValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumValue),
                maximumValue,
                "Maximum glucose value must be greater than the minimum value.");
        }

        if (baseValue < minimumValue || baseValue > maximumValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseValue),
                baseValue,
                "Base glucose value must be inside the configured generation range.");
        }

        if (variation < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(variation),
                variation,
                "Variation must be greater than or equal to zero.");
        }

        if (effectiveReadingInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingInterval),
                readingInterval,
                "Reading interval must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            throw new ArgumentException("Device name must be specified.", nameof(deviceName));
        }

        BaseValue = baseValue;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        Variation = variation;
        ReadingInterval = effectiveReadingInterval;
        DeviceName = deviceName.Trim();
    }

    /// <summary>
    /// Gets the default mock CGM provider options.
    /// </summary>
    public static MockCgmProviderOptions Default { get; } = new();

    /// <summary>
    /// Gets the baseline glucose value expressed in mg/dL.
    /// </summary>
    public int BaseValue { get; }

    /// <summary>
    /// Gets the minimum generated glucose value expressed in mg/dL.
    /// </summary>
    public int MinimumValue { get; }

    /// <summary>
    /// Gets the maximum generated glucose value expressed in mg/dL.
    /// </summary>
    public int MaximumValue { get; }

    /// <summary>
    /// Gets the maximum variation applied around the baseline value.
    /// </summary>
    public int Variation { get; }

    /// <summary>
    /// Gets the simulated CGM reading interval.
    /// </summary>
    public TimeSpan ReadingInterval { get; }

    /// <summary>
    /// Gets the simulated device name.
    /// </summary>
    public string DeviceName { get; }
}