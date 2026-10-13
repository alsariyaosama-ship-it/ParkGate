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

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<DateTime> GetDbUtcNowAsync(CancellationToken cancellationToken = default);
}
