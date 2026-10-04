using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using ParkGate.Application.Ports.Security;

namespace ParkGate.Infrastructure.Security;

/// <summary>
/// Secure connection settings store using Windows DPAPI (ProtectedData) with LocalMachine scope (AGENTS.md §7).
/// Stored in %ProgramData%\ParkGate\connection.dat.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DpapiConnectionSettingsStore : IConnectionSettingsStore
{
    private static readonly byte[] OptionalEntropy = "ParkGate.ConnectionProtection"u8.ToArray();
    private readonly string _filePath;

    public DpapiConnectionSettingsStore(string? filePath = null)
    {
        if (filePath != null)
        {
            _filePath = filePath;
        }
        else
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            _filePath = Path.Combine(programData, "ParkGate", "connection.dat");
        }
    }

    public bool Exists()
    {
        return File.Exists(_filePath);
    }

    public string? GetConnectionString()
    {
        if (!Exists())
        {
            return null;
        }

        try
        {
            var encryptedBytes = File.ReadAllBytes(_filePath);
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, OptionalEntropy, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    public void SaveConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var plaintextBytes = Encoding.UTF8.GetBytes(connectionString);
        var encryptedBytes = ProtectedData.Protect(plaintextBytes, OptionalEntropy, DataProtectionScope.LocalMachine);
        File.WriteAllBytes(_filePath, encryptedBytes);
    }

    public void Clear()
    {
        if (Exists())
        {
            File.Delete(_filePath);
        }
    }
}
