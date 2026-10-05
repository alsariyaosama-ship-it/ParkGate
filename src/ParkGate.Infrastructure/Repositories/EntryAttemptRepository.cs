using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class EntryAttemptRepository : IEntryAttemptRepository
{
    private readonly ParkGateDbContext _context;

    public EntryAttemptRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<EntryAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Include(e => e.Companions)
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EntryAttempt?> GetLatestAttemptForBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Where(e => e.BeneficiaryId == beneficiaryId)
            .OrderByDescending(e => e.AttemptedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EntryAttempt?> GetLatestDenialAsync(Guid beneficiaryId, int workstationId, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Where(e => e.BeneficiaryId == beneficiaryId &&
                        e.WorkstationId == workstationId &&
                        e.Result == EntryResult.Denied)
            .OrderByDescending(e => e.AttemptedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EntryAttempt>> GetHistoryByBeneficiaryIdAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Include(e => e.Companions)
            .Where(e => e.BeneficiaryId == beneficiaryId)
            .OrderByDescending(e => e.AttemptedAtUtc)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EntryAttempt>> GetTodayByWorkstationIdAsync(int workstationId, DateOnly businessDate, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Include(e => e.Companions)
            .Where(e => e.WorkstationId == workstationId && e.BusinessDate == businessDate)
            .OrderByDescending(e => e.AttemptedAtUtc)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public void Add(EntryAttempt attempt)
    {
        _context.EntryAttempts.Add(attempt);
    }

    public void Update(EntryAttempt attempt)
    {
        _context.EntryAttempts.Update(attempt);
    }
}
