using System.Reflection;
using GlucoDesk.Application.Updates;

namespace GlucoDesk.Infrastructure.Updates;

/// <summary>
/// Provides the current GlucoDesk version from assembly metadata.
/// </summary>
public sealed class ApplicationVersionProvider :
    IApplicationVersionProvider
{
    /// <inheritdoc />
    public string CurrentVersion
    {
        get
        {
            var assembly =
                Assembly.GetEntryAssembly()
                ?? Assembly.GetExecutingAssembly();

            var informationalVersion = assembly
                .GetCustomAttribute<
                    AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                var metadataSeparator =
                    informationalVersion.IndexOf('+');

                return metadataSeparator >= 0
                    ? informationalVersion[..metadataSeparator]
                    : informationalVersion;
            }

            return assembly.GetName().Version?.ToString()
                   ?? "0.0.0";
        }
    }
}
