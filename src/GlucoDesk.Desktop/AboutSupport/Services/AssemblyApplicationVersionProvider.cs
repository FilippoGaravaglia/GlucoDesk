using System.Reflection;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

namespace GlucoDesk.Desktop.AboutSupport.Services;

/// <summary>
/// Reads the user-facing version from the GlucoDesk desktop assembly.
/// </summary>
public sealed class AssemblyApplicationVersionProvider :
    IApplicationVersionProvider
{
    private const string UnknownVersion = "Unknown";

    private readonly Assembly _assembly;

    /// <summary>
    /// Initializes the provider using the GlucoDesk desktop assembly.
    /// </summary>
    public AssemblyApplicationVersionProvider()
        : this(typeof(AssemblyApplicationVersionProvider).Assembly)
    {
    }

    /// <summary>
    /// Initializes the provider using a specific assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing version metadata.</param>
    public AssemblyApplicationVersionProvider(
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        _assembly = assembly;
    }

    /// <inheritdoc />
    public string GetVersion()
    {
        var informationalVersion = _assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        var normalizedInformationalVersion =
            NormalizeInformationalVersion(informationalVersion);

        if (!string.IsNullOrWhiteSpace(
                normalizedInformationalVersion))
        {
            return normalizedInformationalVersion;
        }

        var assemblyVersion = _assembly
            .GetName()
            .Version?
            .ToString();

        return string.IsNullOrWhiteSpace(assemblyVersion)
            ? UnknownVersion
            : assemblyVersion;
    }

    private static string? NormalizeInformationalVersion(
        string? informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return null;
        }

        var metadataSeparatorIndex =
            informationalVersion.IndexOf(
                '+',
                StringComparison.Ordinal);

        var normalizedVersion =
            metadataSeparatorIndex >= 0
                ? informationalVersion[..metadataSeparatorIndex]
                : informationalVersion;

        normalizedVersion = normalizedVersion.Trim();

        return normalizedVersion.Length == 0
            ? null
            : normalizedVersion;
    }
}
