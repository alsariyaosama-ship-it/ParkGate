using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private const int DeadlockVictim = 1205;

    private readonly ParkGateDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ParkGateDbContext context)
    {
        _context = context;
        Beneficiaries = new BeneficiaryRepository(context);
        Organizations = new OrganizationRepository(context);
        EntryAttempts = new EntryAttemptRepository(context);
        AgeCategories = new AgeCategoryRepository(context);
        Cities = new CityRepository(context);
        Users = new UserRepository(context);
        Workstations = new WorkstationRepository(context);
        AppSettings = new AppSettingRepository(context);
        AuditLogs = new AuditLogRepository(context);
        SyncOutboxes = new SyncOutboxRepository(context);
        GiftVouchers = new GiftVoucherRepository(context);
        CardPatterns = new CardPatternRepository(context);
        CardPatternAlerts = new CardPatternAlertRepository(context);
    }

    public IBeneficiaryRepository Beneficiaries { get; }
    public IOrganizationRepository Organizations { get; }
    public IEntryAttemptRepository EntryAttempts { get; }
    public IAgeCategoryRepository AgeCategories { get; }
    public ICityRepository Cities { get; }
    public IUserRepository Users { get; }
    public IWorkstationRepository Workstations { get; }
    public IAppSettingRepository AppSettings { get; }
    public IAuditLogRepository AuditLogs { get; }
    public ISyncOutboxRepository SyncOutboxes { get; }
    public IGiftVoucherRepository GiftVouchers { get; }
    public ICardPatternRepository CardPatterns { get; }
    public ICardPatternAlertRepository CardPatternAlerts { get; }

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
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("Another operation changed the same row first.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: DeadlockVictim })
        {
            throw new ConcurrencyConflictException("The database chose this operation as a deadlock victim.", ex);
        }
    }

    public async Task<DateTime> GetDbUtcNowAsync(CancellationToken cancellationToken = default)
    {
        return await DatabaseClock.GetUtcNowAsync(_context, cancellationToken);
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
