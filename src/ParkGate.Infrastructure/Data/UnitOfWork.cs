using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ParkGateDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ParkGateDbContext context)
    {
        _context = context;
        Beneficiaries = new BeneficiaryRepository(context);
        Organizations = new OrganizationRepository(context);
        EntryAttempts = new EntryAttemptRepository(context);
        AgeCategories = new AgeCategoryRepository(context);
        Users = new UserRepository(context);
        Workstations = new WorkstationRepository(context);
        AppSettings = new AppSettingRepository(context);
        AuditLogs = new AuditLogRepository(context);
        SyncOutboxes = new SyncOutboxRepository(context);
    }

    public IBeneficiaryRepository Beneficiaries { get; }
    public IOrganizationRepository Organizations { get; }
    public IEntryAttemptRepository EntryAttempts { get; }
    public IAgeCategoryRepository AgeCategories { get; }
    public IUserRepository Users { get; }
    public IWorkstationRepository Workstations { get; }
    public IAppSettingRepository AppSettings { get; }
    public IAuditLogRepository AuditLogs { get; }
    public ISyncOutboxRepository SyncOutboxes { get; }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress for this unit of work.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to rollback.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DateTime> GetDbUtcNowAsync(CancellationToken cancellationToken = default)
    {
        var dbTime = await _context.Database
            .SqlQueryRaw<DateTime>("SELECT SYSUTCDATETIME() AS [Value]")
            .SingleAsync(cancellationToken);

        return DateTime.SpecifyKind(dbTime, DateTimeKind.Utc);
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _context.DisposeAsync();
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
        _context.Dispose();
    }
}
