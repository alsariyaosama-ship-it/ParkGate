namespace ParkGate.Application.Ports.Persistence;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    IBeneficiaryRepository Beneficiaries { get; }
    IOrganizationRepository Organizations { get; }
    IEntryAttemptRepository EntryAttempts { get; }
    IAgeCategoryRepository AgeCategories { get; }
    IUserRepository Users { get; }
    IWorkstationRepository Workstations { get; }
    IAppSettingRepository AppSettings { get; }
    IAuditLogRepository AuditLogs { get; }
    ISyncOutboxRepository SyncOutboxes { get; }
    IGiftVoucherRepository GiftVouchers { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Saves tracked changes. Throws <see cref="ParkGate.Application.Common.ConcurrencyConflictException"/> when
    /// another desk changed one of the rows first (ADR-026); run the use case through
    /// <see cref="ParkGate.Application.Common.ConcurrencyRetry"/>.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<DateTime> GetDbUtcNowAsync(CancellationToken cancellationToken = default);
}
