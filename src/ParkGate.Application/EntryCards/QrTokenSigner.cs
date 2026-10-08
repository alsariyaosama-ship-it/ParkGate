using System.Security.Cryptography;
using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.EntryCards;

/// <summary>
/// يوقّع رموز البطاقات. يُستخدم على جهاز الإصدار فقط — المفتاح الخاص لا يوجد على أجهزة البوابة.
/// </summary>
public sealed class QrTokenSigner : IDisposable
{
    private readonly ECDsa _privateKey;
    private readonly object _sync = new();

    public byte KeyId { get; }

    public QrTokenSigner(ECDsa privateKey, byte keyId)
    {
        ArgumentNullException.ThrowIfNull(privateKey);
        if (privateKey.KeySize != 256)
        {
            throw new ArgumentException("مطلوب مفتاح ECDSA P-256.", nameof(privateKey));
        }

        _privateKey = privateKey;
        KeyId = keyId;
    }

    public string Encode(Guid cardId, DateOnly expiresOn)
    {
        Span<byte> raw = stackalloc byte[QrTokenFormat.TotalLength];
        Span<byte> body = raw[..QrTokenFormat.BodyLength];
        Span<byte> signature = raw[QrTokenFormat.BodyLength..];

        QrTokenFormat.WriteBody(body, KeyId, cardId, expiresOn);
        byte[] message = QrTokenFormat.MessageToSign(body);

        bool ok;
        int written;
        lock (_sync)
        {
            ok = _privateKey.TrySignData(message, signature, HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation, out written);
        }

        if (!ok || written != QrTokenFormat.SignatureLength)
        {
            throw new CryptographicException("فشل توقيع رمز البطاقة.");
        }

        return QrTokenFormat.ToDigits(raw);
    }

    /// <summary>معرّف بطاقة عشوائي من مولّد تشفيري (وليس Guid.NewGuid).</summary>
    public static Guid NewCardId() => new(RandomNumberGenerator.GetBytes(16));

    public void Dispose() => _privateKey.Dispose();
}
