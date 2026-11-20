using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class CardPatternRepository : ICardPatternRepository
{
    private readonly ParkGateDbContext _context;

    public CardPatternRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OrganizationCardPattern>> GetByOrganizationAsync(int organizationId, CancellationToken ct = default) =>
        await _context.OrganizationCardPatterns.Where(p => p.OrganizationId == organizationId).ToListAsync(ct);

    public Task<OrganizationCardPattern?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.OrganizationCardPatterns.SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<OrganizationCardPattern>> ListAsync(CancellationToken ct = default) =>
        await _context.OrganizationCardPatterns.AsNoTracking().OrderByDescending(p => p.ProposedAtUtc).ThenByDescending(p => p.Id).ToListAsync(ct);

    public void Add(OrganizationCardPattern pattern) => _context.OrganizationCardPatterns.Add(pattern);
}

public sealed class CardPatternAlertRepository : ICardPatternAlertRepository
{
    private readonly ParkGateDbContext _context;

    public CardPatternAlertRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public void Add(CardPatternAlert alert) => _context.CardPatternAlerts.Add(alert);

    public Task<CardPatternAlert?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _context.CardPatternAlerts.SingleOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<string>> GetOpenCardNumbersAsync(int organizationId, string shape, CancellationToken ct = default) =>
        await _context.CardPatternAlerts
            .Where(a => a.OrganizationId == organizationId && a.Shape == shape && a.Status == CardPatternAlertStatus.Open)
            .Select(a => a.CardNumber)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CardPatternAlert>> GetOpenForUpdateAsync(int organizationId, string shape, CancellationToken ct = default) =>
        await _context.CardPatternAlerts
            .Where(a => a.OrganizationId == organizationId && a.Shape == shape && a.Status == CardPatternAlertStatus.Open)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CardPatternAlert>> ListAsync(bool openOnly, int maxRows, CancellationToken ct = default) =>
        await _context.CardPatternAlerts.AsNoTracking()
            .Where(a => !openOnly || a.Status == CardPatternAlertStatus.Open)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ThenByDescending(a => a.Id)
            .Take(maxRows)
            .ToListAsync(ct);

    public async Task<IReadOnlySet<Guid>> GetBeneficiariesWithOpenAlertsAsync(IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken ct = default)
    {
        if (beneficiaryIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var ids = await _context.CardPatternAlerts
            .Where(a => a.Status == CardPatternAlertStatus.Open && beneficiaryIds.Contains(a.BeneficiaryId))
            .Select(a => a.BeneficiaryId)
            .Distinct()
            .ToListAsync(ct);
        return ids.ToHashSet();
    }
}
