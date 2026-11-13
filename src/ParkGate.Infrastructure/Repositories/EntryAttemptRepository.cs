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

    public async Task<TodayAttemptTotals> GetTodayTotalsAsync(int workstationId, DateOnly businessDate, CancellationToken cancellationToken = default)
    {
        var day = _context.EntryAttempts
            .AsNoTracking()
            .Where(e => e.WorkstationId == workstationId && e.BusinessDate == businessDate);

        var counts = await day
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Granted = g.Count(e => e.Result == EntryResult.Allowed && !e.IsVoided),
                Overridden = g.Count(e => e.Result == EntryResult.Overridden && !e.IsVoided),
                Denied = g.Count(e => e.Result == EntryResult.Denied),
                Persons = g.Sum(e => (e.Result == EntryResult.Allowed || e.Result == EntryResult.Overridden) && !e.IsVoided
                    ? e.TotalPersons
                    : 0)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (counts is null)
        {
            return TodayAttemptTotals.Empty;
        }

        var companions = await day
            .Where(e => (e.Result == EntryResult.Allowed || e.Result == EntryResult.Overridden) && !e.IsVoided)
            .SelectMany(e => e.Companions)
            .GroupBy(c => c.AgeCategoryId)
            .Select(g => new { AgeCategoryId = g.Key, Count = g.Sum(c => c.Count) })
            .ToDictionaryAsync(x => x.AgeCategoryId, x => x.Count, cancellationToken);

        return new TodayAttemptTotals(counts.Granted, counts.Overridden, counts.Denied, counts.Persons, companions);
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
