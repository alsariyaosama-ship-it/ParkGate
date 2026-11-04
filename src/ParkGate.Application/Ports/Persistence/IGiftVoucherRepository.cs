using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IGiftVoucherRepository
{
    Task<GiftVoucher?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Lock the row with UPDLOCK, ROWLOCK for atomic redemption.</summary>
    Task<GiftVoucher?> GetByTokenForUpdateAsync(Guid token, CancellationToken ct = default);

    /// <summary>Latest IssuedAtUtc among non-voided vouchers; null if none.</summary>
    Task<DateTime?> GetLatestIssuedUtcAsync(Guid beneficiaryId, CancellationToken ct = default);

    Task<IReadOnlyList<GiftVoucher>> GetRecentByBeneficiaryAsync(Guid beneficiaryId, int maxResults = 20, CancellationToken ct = default);

    void Add(GiftVoucher voucher);
}
