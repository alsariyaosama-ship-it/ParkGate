using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IGiftVoucherRepository
{
    Task<GiftVoucher?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tracked read of a voucher that the use case will change; a concurrent change fails at save (ADR-026).</summary>
    Task<GiftVoucher?> GetByTokenForUpdateAsync(Guid token, CancellationToken ct = default);

    /// <summary>Latest IssuedAtUtc among non-voided vouchers; null if none.</summary>
    Task<DateTime?> GetLatestIssuedUtcAsync(Guid beneficiaryId, CancellationToken ct = default);

    Task<IReadOnlyList<GiftVoucher>> GetRecentByBeneficiaryAsync(Guid beneficiaryId, int maxResults = 20, CancellationToken ct = default);

    void Add(GiftVoucher voucher);
}
