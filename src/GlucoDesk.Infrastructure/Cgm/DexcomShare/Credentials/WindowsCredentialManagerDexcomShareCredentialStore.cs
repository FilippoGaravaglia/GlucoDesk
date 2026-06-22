using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Stores Dexcom Share credentials in Windows Credential Manager.
/// </summary>
public sealed class WindowsCredentialManagerDexcomShareCredentialStore : IDexcomShareCredentialStore
{
    private const string ServiceName = "GlucoDesk.DexcomShare";
    private const string UsernameTargetName = $"{ServiceName}.username";
    private const string PasswordTargetName = $"{ServiceName}.password";
    private const string RegionTargetName = $"{ServiceName}.region";

    private const uint CredentialTypeGeneric = 1;
    private const uint CredentialPersistLocalMachine = 2;

    private const int ErrorNotFound = 1168;

    /// <inheritdoc />
    public Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<DexcomShareCredentials?>(null);
        }

        var username = ReadSecret(UsernameTargetName);
        var password = ReadSecret(PasswordTargetName);
        var regionText = ReadSecret(RegionTargetName);
        var region = ParseRegion(regionText);

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password)
            || region is DexcomShareRegion.Unknown)
        {
            return Task.FromResult<DexcomShareCredentials?>(null);
        }

        return Task.FromResult<DexcomShareCredentials?>(
            new DexcomShareCredentials(
                username,
                password,
                region));
    }

    /// <inheritdoc />
    public Task SaveAsync(
        DexcomShareCredentials credentials,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "Windows Credential Manager storage is available only on Windows.");
        }

        WriteSecret(UsernameTargetName, credentials.Username);
        WriteSecret(PasswordTargetName, credentials.Password);
        WriteSecret(RegionTargetName, FormatRegion(credentials.Region));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.CompletedTask;
        }

        DeleteSecret(UsernameTargetName);
        DeleteSecret(PasswordTargetName);
        DeleteSecret(RegionTargetName);

        return Task.CompletedTask;
    }

    #region Helpers

    /// <summary>
    /// Reads a secret from Windows Credential Manager.
    /// </summary>
    /// <param name="targetName">The credential target name.</param>
    /// <returns>The secret value, or null when not found.</returns>
    private static string? ReadSecret(string targetName)
    {
        if (!CredRead(
                targetName,
                CredentialTypeGeneric,
                flags: 0,
                out var credentialPointer))
        {
            var errorCode = Marshal.GetLastWin32Error();

            if (errorCode == ErrorNotFound)
            {
                return null;
            }

            throw CreateCredentialManagerException(
                errorCode,
                $"Unable to read Windows credential '{targetName}'.");
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredentialRead>(credentialPointer);

            if (credential.CredentialBlob == IntPtr.Zero || credential.CredentialBlobSize == 0)
            {
                return null;
            }

            var secretBytes = new byte[credential.CredentialBlobSize];

            Marshal.Copy(
                credential.CredentialBlob,
                secretBytes,
                startIndex: 0,
                length: secretBytes.Length);

            return Encoding.Unicode.GetString(secretBytes).TrimEnd('\0');
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    /// <summary>
    /// Writes a secret to Windows Credential Manager.
    /// </summary>
    /// <param name="targetName">The credential target name.</param>
    /// <param name="secret">The secret value.</param>
    private static void WriteSecret(
        string targetName,
        string secret)
    {
        var secretBytes = Encoding.Unicode.GetBytes(secret);
        var secretPointer = Marshal.AllocCoTaskMem(secretBytes.Length);

        try
        {
            Marshal.Copy(
                secretBytes,
                startIndex: 0,
                secretPointer,
                length: secretBytes.Length);

            var credential = new NativeCredentialWrite
            {
                Type = CredentialTypeGeneric,
                TargetName = targetName,
                CredentialBlobSize = (uint)secretBytes.Length,
                CredentialBlob = secretPointer,
                Persist = CredentialPersistLocalMachine,
                UserName = Environment.UserName
            };

            if (!CredWrite(ref credential, flags: 0))
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw CreateCredentialManagerException(
                    errorCode,
                    $"Unable to write Windows credential '{targetName}'.");
            }
        }
        finally
        {
            ZeroMemory(secretPointer, secretBytes.Length);
            Marshal.FreeCoTaskMem(secretPointer);
            Array.Clear(secretBytes);
        }
    }

    /// <summary>
    /// Deletes a secret from Windows Credential Manager.
    /// </summary>
    /// <param name="targetName">The credential target name.</param>
    private static void DeleteSecret(string targetName)
    {
        if (CredDelete(
                targetName,
                CredentialTypeGeneric,
                flags: 0))
        {
            return;
        }

        var errorCode = Marshal.GetLastWin32Error();

        if (errorCode == ErrorNotFound)
        {
            return;
        }

        throw CreateCredentialManagerException(
            errorCode,
            $"Unable to delete Windows credential '{targetName}'.");
    }

    /// <summary>
    /// Parses a persisted Dexcom Share region value.
    /// </summary>
    /// <param name="value">The persisted region value.</param>
    /// <returns>The parsed Dexcom Share region.</returns>
    private static DexcomShareRegion ParseRegion(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "us" => DexcomShareRegion.Us,
            "ous" => DexcomShareRegion.OutsideUs,
            "outsideus" => DexcomShareRegion.OutsideUs,
            "outside-us" => DexcomShareRegion.OutsideUs,
            "eu" => DexcomShareRegion.OutsideUs,
            "europe" => DexcomShareRegion.OutsideUs,
            _ => DexcomShareRegion.Unknown
        };
    }

    /// <summary>
    /// Formats a Dexcom Share region for persistence.
    /// </summary>
    /// <param name="region">The Dexcom Share region.</param>
    /// <returns>The formatted region value.</returns>
    private static string FormatRegion(DexcomShareRegion region)
    {
        return region switch
        {
            DexcomShareRegion.Us => "us",
            DexcomShareRegion.OutsideUs => "ous",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Clears unmanaged memory that temporarily contained a secret.
    /// </summary>
    /// <param name="pointer">The unmanaged memory pointer.</param>
    /// <param name="length">The number of bytes to clear.</param>
    private static void ZeroMemory(
        IntPtr pointer,
        int length)
    {
        if (pointer == IntPtr.Zero || length <= 0)
        {
            return;
        }

        for (var index = 0; index < length; index++)
        {
            Marshal.WriteByte(pointer, index, 0);
        }
    }

    /// <summary>
    /// Creates a credential-manager exception from a Win32 error code.
    /// </summary>
    /// <param name="errorCode">The Win32 error code.</param>
    /// <param name="message">The operation message.</param>
    /// <returns>The exception.</returns>
    private static Win32Exception CreateCredentialManagerException(
        int errorCode,
        string message)
    {
        return new Win32Exception(
            errorCode,
            $"{message} Win32 error code: {errorCode}.");
    }

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(
        string target,
        uint type,
        uint flags,
        out IntPtr credentialPointer);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(
        ref NativeCredentialWrite credential,
        uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(
        string target,
        uint type,
        uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredFree")]
    private static extern void CredFree(IntPtr credentialPointer);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCredentialRead
    {
        public uint Flags;
        public uint Type;
        public IntPtr TargetName;
        public IntPtr Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public IntPtr TargetAlias;
        public IntPtr UserName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredentialWrite
    {
        public uint Flags;

        public uint Type;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;

        public IntPtr Comment;

        public long LastWritten;

        public uint CredentialBlobSize;

        public IntPtr CredentialBlob;

        public uint Persist;

        public uint AttributeCount;

        public IntPtr Attributes;

        public IntPtr TargetAlias;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string UserName;
    }

    #endregion
}