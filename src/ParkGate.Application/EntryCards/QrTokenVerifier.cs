using System.Security.Cryptography;
using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.EntryCards;

/// <summary>
/// يتحقق من توقيع الرمز الممسوح بالمفاتيح العامة فقط — آمن للتثبيت على كل أجهزة البوابة.
/// </summary>
public sealed class QrTokenVerifier : IDisposable
{
    private readonly Dictionary<byte, ECDsa> _keys = new();
    private readonly object _sync = new();

    /// <param name="trustedKeys">المفاتيح غير الملغاة فقط. إلغاء مفتاح يُبطل كل البطاقات الموقعة به.</param>
    public QrTokenVerifier(IEnumerable<QrPublicKey> trustedKeys)
    {
        foreach (var k in trustedKeys)
        {
            var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(k.SubjectPublicKeyInfo, out _);
            if (ecdsa.KeySize != 256)
            {
                ecdsa.Dispose();
                throw new ArgumentException($"المفتاح {k.KeyId} ليس P-256.");
            }
            _keys.Add(k.KeyId, ecdsa);
        }
    }

    public QrVerifyResult Verify(string? scannedText)
    {
        Span<byte> raw = stackalloc byte[QrTokenFormat.TotalLength];
        if (!QrTokenFormat.TryFromDigits(scannedText, raw) || raw[0] != QrTokenFormat.FormatVersion)
        {
            return new(QrVerifyStatus.Unreadable, null);
        }

        if (!_keys.TryGetValue(raw[1], out var key))
        {
            return new(QrVerifyStatus.UnknownKey, null);
        }

        ReadOnlySpan<byte> body = raw[..QrTokenFormat.BodyLength];
        ReadOnlySpan<byte> signature = raw[QrTokenFormat.BodyLength..];
        byte[] message = QrTokenFormat.MessageToSign(body);

        bool valid;
        lock (_sync)
        {
            valid = key.VerifyData(message, signature, HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }

        return valid
            ? new(QrVerifyStatus.Valid, QrTokenFormat.ReadBody(body))
            : new(QrVerifyStatus.InvalidSignature, null);
    }

    public void Dispose()
    {
        foreach (var k in _keys.Values)
        {
            k.Dispose();
        }
    }
}
