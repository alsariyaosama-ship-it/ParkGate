using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Tests.Common;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"HASHED_{password}";

    public bool VerifyPassword(string password, string passwordHash, out bool needsRehash)
    {
        needsRehash = false;
        return passwordHash == $"HASHED_{password}";
    }
}

public sealed class FakeAuditWriter : IAuditWriter
{
    public List<(string Action, int? UserId, int? WorkstationId, string? EntityName, string? EntityId)> Writes { get; } = [];

    public Task WriteAsync(
        string action,
        int? userId = null,
        int? workstationId = null,
        string? entityName = null,
        string? entityId = null,
        string? detailsJson = null,
        CancellationToken cancellationToken = default)
    {
        Writes.Add((action, userId, workstationId, entityName, entityId));
        return Task.CompletedTask;
    }
}

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private int _nextId = 1;

    public FakeUserRepository(List<User> users)
    {
        _users = users;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Count);
    }

    public void Add(User user)
    {
        if (user.Id == 0)
        {
            typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, _nextId++);
        }
        _users.Add(user);
    }

    public void Update(User user) { }
}

public sealed class FakeWorkstationRepository : IWorkstationRepository
{
    private readonly List<Workstation> _workstations;
    private int _nextId = 1;

    public FakeWorkstationRepository(List<Workstation> workstations)
    {
        _workstations = workstations;
    }

    public Task<Workstation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workstations.FirstOrDefault(w => w.Id == id));
    }

    public Task<Workstation?> GetByMachineNameAsync(string machineName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workstations.FirstOrDefault(w => w.MachineName.Equals(machineName, StringComparison.OrdinalIgnoreCase)));
    }

    public void Add(Workstation workstation)
    {
        if (workstation.Id == 0)
        {
            typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(workstation, _nextId++);
        }
        _workstations.Add(workstation);
    }

    public void Update(Workstation workstation) { }
}

public sealed class FakeAppSettingRepository : IAppSettingRepository
{
    private readonly Dictionary<string, AppSetting> _settings = new(StringComparer.OrdinalIgnoreCase);

    public Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        _settings.TryGetValue(key, out var setting);
        return Task.FromResult(setting);
    }

    public Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, string> dict = _settings.ToDictionary(k => k.Key, v => v.Value.Value);
        return Task.FromResult(dict);
    }

    public void Add(AppSetting setting) => _settings[setting.Key] = setting;

    public void Update(AppSetting setting) => _settings[setting.Key] = setting;
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public IUserRepository Users { get; }
    public IWorkstationRepository Workstations { get; }
    public IAppSettingRepository AppSettings { get; }

    public IBeneficiaryRepository Beneficiaries => throw new NotImplementedException();
    public IOrganizationRepository Organizations => throw new NotImplementedException();
    public IEntryAttemptRepository EntryAttempts => throw new NotImplementedException();
    public IAgeCategoryRepository AgeCategories => throw new NotImplementedException();
    public IAuditLogRepository AuditLogs => throw new NotImplementedException();

    public DateTime CurrentDbTime { get; set; } = new(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);
    public int SaveChangesCalls { get; private set; }

    public FakeUnitOfWork(
        List<User> users,
        List<Workstation> workstations,
        FakeAppSettingRepository appSettings)
    {
        Users = new FakeUserRepository(users);
        Workstations = new FakeWorkstationRepository(workstations);
        AppSettings = appSettings;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }

    public Task<DateTime> GetDbUtcNowAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CurrentDbTime);

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public sealed class FakeUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly List<User> _users;
    private readonly List<Workstation> _workstations;
    private readonly FakeAppSettingRepository _appSettings;

    public FakeUnitOfWork CurrentUow { get; }

    public FakeUnitOfWorkFactory(
        List<User>? users = null,
        List<Workstation>? workstations = null,
        FakeAppSettingRepository? appSettings = null)
    {
        _users = users ?? [];
        _workstations = workstations ?? [];
        _appSettings = appSettings ?? new FakeAppSettingRepository();
        CurrentUow = new FakeUnitOfWork(_users, _workstations, _appSettings);
    }

    public IUnitOfWork Create() => CurrentUow;
}
