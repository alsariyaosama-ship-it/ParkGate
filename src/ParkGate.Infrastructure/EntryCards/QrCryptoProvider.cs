using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Ports.EntryCards;

namespace ParkGate.Infrastructure.EntryCards;

public sealed class QrCryptoProvider : IQrCryptoProvider, IDisposable
{
    private readonly IQrKeyRepository _keyRepository;
    private readonly string _stationName;
    private readonly ILogger<QrCryptoProvider>? _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _initialized;

    public QrTokenVerifier? Verifier { get; private set; }
    public QrTokenSigner? Signer { get; private set; }

    public QrCryptoProvider(
        IQrKeyRepository keyRepository,
        string? stationName = null,
        ILogger<QrCryptoProvider>? logger = null)
    {
        _keyRepository = keyRepository;
        _stationName = stationName ?? Environment.MachineName;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            _logger?.LogInformation("Initializing QR crypto provider (trusted keys and station signer)...");

            var trustedKeys = await _keyRepository.GetTrustedKeysAsync(cancellationToken);
            Verifier = new QrTokenVerifier(trustedKeys);

            if (OperatingSystem.IsWindows())
            {
                Signer = await QrSigningBootstrap.CreateSignerAsync(_keyRepository, _stationName, cancellationToken);
            }

            _initialized = true;
            _logger?.LogInformation("QR crypto provider initialized successfully with {KeyCount} trusted keys.", trustedKeys.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize QR crypto provider.");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        Verifier?.Dispose();
        _lock.Dispose();
    }
}
