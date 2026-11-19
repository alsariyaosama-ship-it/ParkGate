using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class GiftVoucherRepository : IGiftVoucherRepository
{
    private readonly ParkGateDbContext _context;

    public GiftVoucherRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<GiftVoucher?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.GiftVouchers
            .SingleOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<GiftVoucher?> GetByTokenForUpdateAsync(Guid token, CancellationToken ct = default)
    {
        // Tracked read; RowVersion makes a concurrent redeem or void fail at save (ADR-026).
        return await _context.GiftVouchers.SingleOrDefaultAsync(v => v.Token == token, ct);
    }

    public async Task<IReadOnlyList<GiftVoucher>> GetNotVoidedForUpdateOnAsync(Guid beneficiaryId, DateOnly businessDate, CancellationToken ct = default) =>
        await _context.GiftVouchers
            .Where(v => v.BeneficiaryId == beneficiaryId && v.BusinessDate == businessDate && v.Status != GiftVoucherStatus.Voided)
            .ToListAsync(ct);

    public async Task<DateTime?> GetLatestIssuedUtcAsync(Guid beneficiaryId, CancellationToken ct = default)
    {
        return await _context.GiftVouchers
            .Where(v => v.BeneficiaryId == beneficiaryId && v.Status != GiftVoucherStatus.Voided)
            .MaxAsync(v => (DateTime?)v.IssuedAtUtc, ct);
    }

    public async Task<IReadOnlyList<GiftVoucher>> GetRecentByBeneficiaryAsync(
        Guid beneficiaryId,
        int maxResults = 20,
        CancellationToken ct = default)
    {
        return await _context.GiftVouchers
            .AsNoTracking()
            .Where(v => v.BeneficiaryId == beneficiaryId)
            .OrderByDescending(v => v.IssuedAtUtc)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public void Add(GiftVoucher voucher)
    {
        _context.GiftVouchers.Add(voucher);
    }
}
