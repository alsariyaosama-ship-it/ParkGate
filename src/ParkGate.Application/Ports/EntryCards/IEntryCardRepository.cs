namespace ParkGate.Application.Ports.EntryCards;

/// <summary>ما تحتاجه البوابة من قاعدة البيانات عن البطاقة الممسوحة.</summary>
public sealed record EntryCardRecord(Guid CardId, Guid BeneficiaryId, bool IsRevoked, bool PermitActive);

public interface IEntryCardRepository
{
    Task<EntryCardRecord?> FindAsync(Guid cardId, CancellationToken ct = default);

    /// <summary>يلغي البطاقة الفعالة الحالية للمستفيد (إن وجدت) ويضيف الجديدة — في معاملة واحدة (usp_IssueEntryCard).</summary>
    Task ReplaceActiveCardAsync(Guid beneficiaryId, Guid cardId, byte keyId, DateOnly expiresOn, CancellationToken ct = default);

    Task RevokeAsync(Guid cardId, string reason, CancellationToken ct = default);
}
