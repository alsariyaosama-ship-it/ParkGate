using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class BeneficiaryRepository : IBeneficiaryRepository
{
    private readonly ParkGateDbContext _context;

    public BeneficiaryRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<Beneficiary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Beneficiary>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Beneficiary?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .FromSqlInterpolated($"SELECT * FROM dbo.Beneficiaries WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Beneficiary>> GetByCardNumberAsync(CardNumber cardNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .Where(b => b.CardNumber == cardNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Beneficiary?> GetByOrgAndCardAsync(int organizationId, CardNumber cardNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .SingleOrDefaultAsync(b => b.OrganizationId == organizationId && b.CardNumber == cardNumber, cancellationToken);
    }

    public async Task<bool> ExistsNationalIdAsync(NationalId nationalId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .AnyAsync(b => b.NationalId == nationalId && (!excludeId.HasValue || b.Id != excludeId.Value), cancellationToken);
    }

    public async Task<DateTime?> GetLatestGrantedEntryUtcAsync(Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        return await _context.EntryAttempts
            .Where(e => e.BeneficiaryId == beneficiaryId &&
                        !e.IsVoided &&
                        (e.Result == EntryResult.Allowed || e.Result == EntryResult.Overridden))
            .OrderByDescending(e => e.AttemptedAtUtc)
            .Select(e => (DateTime?)e.AttemptedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Beneficiary>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _context.Beneficiaries
                .OrderBy(b => b.FullName)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }

        var trimmed = searchTerm.Trim();
        var hasCard = CardNumber.TryCreate(trimmed, out var card);
        var hasPhone = PhoneNumber.TryCreate(trimmed, out var phone);
        var hasNationalId = NationalId.TryCreate(trimmed, out var nationalId);

        return await _context.Beneficiaries
            .Where(b => EF.Functions.Like(b.FullName, $"%{trimmed}%") ||
                        (hasCard && b.CardNumber == card) ||
                        (hasPhone && b.Phone == phone) ||
                        (hasNationalId && b.NationalId == nationalId))
            .OrderBy(b => b.FullName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public void Add(Beneficiary beneficiary)
    {
        _context.Beneficiaries.Add(beneficiary);
    }

    public void Update(Beneficiary beneficiary)
    {
        _context.Beneficiaries.Update(beneficiary);
    }
}
