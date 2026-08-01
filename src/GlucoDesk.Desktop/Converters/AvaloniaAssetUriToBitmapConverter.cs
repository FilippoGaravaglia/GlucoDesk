using System.Collections.Concurrent;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GlucoDesk.Desktop.Converters;

/// <summary>
/// Converts packaged Avalonia asset URIs into cached bitmaps.
/// </summary>
/// <remarks>
/// Each asset is decoded at most once for the lifetime of the application.
/// The same bitmap instance is then reused by every image control that
/// requests the corresponding asset.
/// </remarks>
public sealed class AvaloniaAssetUriToBitmapConverter :
    IValueConverter
{
    private static readonly ConcurrentDictionary<
        string,
        Lazy<Bitmap?>> BitmapCache =
        new(StringComparer.Ordinal);

    /// <inheritdoc />
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        _ = targetType;
        _ = parameter;
        _ = culture;

        if (value is not string assetPath ||
            string.IsNullOrWhiteSpace(assetPath))
        {
            return AvaloniaProperty.UnsetValue;
        }

        return GetOrLoadBitmap(assetPath)
            ?? AvaloniaProperty.UnsetValue;
    }

    /// <inheritdoc />
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        _ = value;
        _ = targetType;
        _ = parameter;
        _ = culture;

        throw new NotSupportedException(
            $"{nameof(AvaloniaAssetUriToBitmapConverter)} " +
            "does not support reverse conversion.");
    }

    /// <summary>
    /// Preloads the supplied packaged images into the shared bitmap cache.
    /// </summary>
    /// <param name="assetPaths">
    /// Packaged Avalonia asset paths to preload.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the optional cache warm-up.
    /// </param>
    /// <returns>A task representing the background warm-up.</returns>
    public static Task PreloadAsync(
        IEnumerable<string> assetPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(assetPaths);

        var paths = assetPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return Task.Run(
            () =>
            {
                foreach (var assetPath in paths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _ = GetOrLoadBitmap(assetPath);
                }
            },
            cancellationToken);
    }

    private static Bitmap? GetOrLoadBitmap(string assetPath)
    {
        var lazyBitmap = BitmapCache.GetOrAdd(
            assetPath,
            static path =>
                new Lazy<Bitmap?>(
                    () => LoadBitmap(path),
                    LazyThreadSafetyMode.ExecutionAndPublication));

        return lazyBitmap.Value;
    }

    private static Bitmap? LoadBitmap(string assetPath)
    {
        try
        {
            var assetUri = new Uri(
                assetPath,
                UriKind.Absolute);

            using var stream =
                AssetLoader.Open(assetUri);

            return new Bitmap(stream);
        }
        catch
        {
            // Image loading is optional. The visual fallback remains visible
            // when an asset is missing or cannot be decoded.
            return null;
        }
    }
}
