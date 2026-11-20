using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ParkGate.Application.Sync;

namespace ParkGate.Infrastructure.Sync;

/// <summary>Where the Sync Agent keeps its cloud API key (R8.3).</summary>
public interface ICloudApiKeyStore
{
    /// <summary>The stored key, or null when none is stored or it cannot be decrypted on this machine.</summary>
    string? GetApiKey();

    void SaveApiKey(string apiKey);

    /// <summary>Removes the stored key; true when there was one.</summary>
    bool Clear();
}

/// <summary>
/// The cloud API key protected with DPAPI (<c>LocalMachine</c>), in <c>%ProgramData%\ParkGate\cloudkey.dat</c> next to
/// <c>connection.dat</c> (AGENTS.md §7, R8.3). LocalMachine scope lets the Windows service account read what an
/// administrator saved; the file is useless on another machine.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DpapiCloudApiKeyStore : ICloudApiKeyStore
{
    private static readonly byte[] OptionalEntropy = "ParkGate.CloudApiKey"u8.ToArray();
    private readonly string _filePath;

    public DpapiCloudApiKeyStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ParkGate", "cloudkey.dat");
    }

    public string FilePath => _filePath;

    public string? GetApiKey()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var plain = ProtectedData.Unprotect(File.ReadAllBytes(_filePath), OptionalEntropy, DataProtectionScope.LocalMachine);
            var key = Encoding.UTF8.GetString(plain);
            return string.IsNullOrWhiteSpace(key) ? null : key;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    public void SaveApiKey(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(apiKey.Trim()), OptionalEntropy, DataProtectionScope.LocalMachine);
        File.WriteAllBytes(_filePath, protectedBytes);
    }

    public bool Clear()
    {
        if (!File.Exists(_filePath))
        {
            return false;
        }

        File.Delete(_filePath);
        return true;
    }
}

/// <summary>
/// The DPAPI-stored key wins over a key in plain JSON (R8.3). A plain key still works, so an agent upgraded from
/// 1.0.0 keeps syncing until the administrator runs <c>set-cloud-key</c> and removes it from the JSON file.
/// </summary>
public sealed class CloudApiKeyPostConfigure : IPostConfigureOptions<CloudApiOptions>
{
    private readonly ICloudApiKeyStore _store;

    public CloudApiKeyPostConfigure(ICloudApiKeyStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public void PostConfigure(string? name, CloudApiOptions options)
    {
        if (_store.GetApiKey() is { } storedKey)
        {
            options.ApiKey = storedKey;
        }
    }
}
