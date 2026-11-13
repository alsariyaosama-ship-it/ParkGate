using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IEntryAttemptRepository
{
    Task<EntryAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EntryAttempt?> GetLatestAttemptForBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default);
    Task<EntryAttempt?> GetLatestDenialAsync(Guid beneficiaryId, int workstationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryAttempt>> GetHistoryByBeneficiaryIdAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryAttempt>> GetTodayByWorkstationIdAsync(int workstationId, DateOnly businessDate, int maxResults = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates every attempt of one workstation on one business date (R3.1). Granted, overridden, persons and
    /// companions exclude voided rows; denied attempts cannot be voided and are all counted.
    /// </summary>
    Task<TodayAttemptTotals> GetTodayTotalsAsync(int workstationId, DateOnly businessDate, CancellationToken cancellationToken = default);

    void Add(EntryAttempt attempt);
    void Update(EntryAttempt attempt);
}

/// <summary>Day totals for one workstation. <see cref="CompanionsByCategory"/> maps age category id to companion count.</summary>
public sealed record TodayAttemptTotals(
    int Granted,
    int Overridden,
    int Denied,
    int Persons,
    IReadOnlyDictionary<int, int> CompanionsByCategory)
{
    public static TodayAttemptTotals Empty { get; } = new(0, 0, 0, 0, new Dictionary<int, int>());
}
