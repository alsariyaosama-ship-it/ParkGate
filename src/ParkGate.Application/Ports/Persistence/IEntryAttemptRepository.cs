using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IEntryAttemptRepository
{
    Task<EntryAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EntryAttempt?> GetLatestAttemptForBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default);
    Task<EntryAttempt?> GetLatestDenialAsync(Guid beneficiaryId, int workstationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryAttempt>> GetHistoryByBeneficiaryIdAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryAttempt>> GetTodayByWorkstationIdAsync(int workstationId, DateOnly businessDate, int maxResults = 20, CancellationToken cancellationToken = default);
    void Add(EntryAttempt attempt);
    void Update(EntryAttempt attempt);
}
