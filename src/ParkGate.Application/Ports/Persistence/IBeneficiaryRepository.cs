using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Ports.Persistence;

public interface IBeneficiaryRepository
{
    Task<Beneficiary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Beneficiary>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    /// <summary>Tracked read of a holder that the use case will change; a concurrent change fails at save (ADR-026).</summary>
    Task<Beneficiary?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Beneficiary>> GetByCardNumberAsync(CardNumber cardNumber, CancellationToken cancellationToken = default);
    Task<Beneficiary?> GetByOrgAndCardAsync(int organizationId, CardNumber cardNumber, CancellationToken cancellationToken = default);

    /// <summary>F8, BR-29: every registered card number of the organization (learning its usual pattern).</summary>
    Task<IReadOnlyList<string>> GetCardNumbersByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsNationalIdAsync(NationalId nationalId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLatestGrantedEntryUtcAsync(Guid beneficiaryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Beneficiary>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default);
    void Add(Beneficiary beneficiary);
    void Update(Beneficiary beneficiary);
}
