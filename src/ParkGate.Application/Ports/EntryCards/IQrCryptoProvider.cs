using ParkGate.Application.EntryCards;

namespace ParkGate.Application.Ports.EntryCards;

/// <summary>
/// Provides asynchronously initialized QR verification and signing services.
/// </summary>
public interface IQrCryptoProvider
{
    QrTokenVerifier? Verifier { get; }
    QrTokenSigner? Signer { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
