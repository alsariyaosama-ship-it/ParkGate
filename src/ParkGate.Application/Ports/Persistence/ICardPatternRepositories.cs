using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

/// <summary>Organizations' usual card-number patterns (F8, BR-29).</summary>
public interface ICardPatternRepository
{
    /// <summary>Tracked patterns of one organization, every status.</summary>
    Task<IReadOnlyList<OrganizationCardPattern>> GetByOrganizationAsync(int organizationId, CancellationToken ct = default);

    /// <summary>Tracked; the Admin's decision fails at save when someone else decided first (RowVersion).</summary>
    Task<OrganizationCardPattern?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Every pattern for the admin screen, newest proposal first.</summary>
    Task<IReadOnlyList<OrganizationCardPattern>> ListAsync(CancellationToken ct = default);

    void Add(OrganizationCardPattern pattern);
}

/// <summary>Card numbers that do not look like their organization's usual numbers (F8, BR-29).</summary>
public interface ICardPatternAlertRepository
{
    void Add(CardPatternAlert alert);

    /// <summary>Tracked.</summary>
    Task<CardPatternAlert?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Card numbers of the organization's open alerts with this shape (to spot a new series).</summary>
    Task<IReadOnlyList<string>> GetOpenCardNumbersAsync(int organizationId, string shape, CancellationToken ct = default);

    /// <summary>Tracked open alerts of the organization with this shape (closed when their pattern is approved).</summary>
    Task<IReadOnlyList<CardPatternAlert>> GetOpenForUpdateAsync(int organizationId, string shape, CancellationToken ct = default);

    /// <summary>Alerts for the admin screen, newest first, at most <paramref name="maxRows"/>.</summary>
    Task<IReadOnlyList<CardPatternAlert>> ListAsync(bool openOnly, int maxRows, CancellationToken ct = default);

    /// <summary>Which of these holders have an open alert (the desk's amber note).</summary>
    Task<IReadOnlySet<Guid>> GetBeneficiariesWithOpenAlertsAsync(IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken ct = default);
}
