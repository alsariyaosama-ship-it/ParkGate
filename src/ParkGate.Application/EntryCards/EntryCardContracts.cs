using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.EntryCards;

/// <summary>ما تحتاجه البوابة من قاعدة البيانات عن البطاقة الممسوحة.</summary>
public sealed record EntryCardRecord(Guid CardId, Guid BeneficiaryId, bool IsRevoked, bool PermitActive);

public interface IEntryCardRepository
{
    Task<EntryCardRecord?> FindAsync(Guid cardId, CancellationToken ct = default);

    /// <summary>يلغي البطاقة الفعالة الحالية للمستفيد (إن وجدت) ويضيف الجديدة — في معاملة واحدة (usp_IssueEntryCard).</summary>
    Task ReplaceActiveCardAsync(Guid beneficiaryId, Guid cardId, byte keyId, DateOnly expiresOn, CancellationToken ct = default);

    Task RevokeAsync(Guid cardId, string reason, CancellationToken ct = default);
}

public interface IQrKeyRepository
{
    /// <summary>المفاتيح العامة غير الملغاة (RevokedAt IS NULL).</summary>
    Task<IReadOnlyList<QrPublicKey>> GetTrustedKeysAsync(CancellationToken ct = default);

    /// <summary>يسجّل المفتاح العام لجهاز الإصدار ويعيد KeyId (أو رقمه الحالي إن كان مسجلاً) — usp_RegisterQrSigningKey.</summary>
    Task<byte> RegisterAsync(byte[] subjectPublicKeyInfo, string stationName, CancellationToken ct = default);
}

public interface ICardScanLog
{
    Task WriteAsync(CardScanLogEntry entry, CancellationToken ct = default);
}

public sealed record CardScanLogEntry(string StationName, CardScanStatus Status, Guid? CardId, string? RawPrefix);

/// <summary>اسم الجهاز الحالي (للسجل). يُسجَّل في DI من الإعدادات أو Environment.MachineName.</summary>
public sealed record GateStation(string Name);

public sealed record EntryCardOptions
{
    /// <summary>مدة صلاحية البطاقة الافتراضية عند الإصدار.</summary>
    public int ValidityDays { get; init; } = 365;
}

public sealed record IssuedEntryCard(Guid BeneficiaryId, Guid CardId, DateOnly ExpiresOn, string QrText);

public sealed record CardScanResult(CardScanStatus Status, Guid? BeneficiaryId)
{
    public bool IsAccepted => Status == CardScanStatus.Accepted;
    public string MessageAr => Status.ToArabic();
}
