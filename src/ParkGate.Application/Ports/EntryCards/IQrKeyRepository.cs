using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.Ports.EntryCards;

public interface IQrKeyRepository
{
    /// <summary>المفاتيح العامة غير الملغاة (RevokedAt IS NULL).</summary>
    Task<IReadOnlyList<QrPublicKey>> GetTrustedKeysAsync(CancellationToken ct = default);

    /// <summary>يسجّل المفتاح العام لجهاز الإصدار ويعيد KeyId (أو رقمه الحالي إن كان مسجلاً) — usp_RegisterQrSigningKey.</summary>
    Task<byte> RegisterAsync(byte[] subjectPublicKeyInfo, string stationName, CancellationToken ct = default);
}
