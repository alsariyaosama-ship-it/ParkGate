using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Application.Ports.Settings;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

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
        _nextId = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
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

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(_users);

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
        _nextId = workstations.Count > 0 ? workstations.Max(w => w.Id) + 1 : 1;
    }

    public Task<Workstation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workstations.FirstOrDefault(w => w.Id == id));
    }

    public Task<Workstation?> GetByMachineNameAsync(string machineName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workstations.FirstOrDefault(w => w.MachineName.Equals(machineName, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<IReadOnlyList<Workstation>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Workstation>>(_workstations);

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

public sealed class FakeAppSettingRepository : IAppSettingRepository, IAppSettingsStore
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

    public Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        _settings.TryGetValue(key, out var setting);
        return Task.FromResult(setting?.Value);
    }

    public Task SetValueAsync(string key, string value, int? updatedByUserId = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        if (_settings.TryGetValue(key, out var existing))
        {
            existing.Update(value, now, updatedByUserId);
        }
        else
        {
            _settings[key] = new AppSetting(key, value, now, updatedByUserId);
        }

        return Task.CompletedTask;
    }

    public void Add(AppSetting setting) => _settings[setting.Key] = setting;

    public void Update(AppSetting setting) => _settings[setting.Key] = setting;
}

public sealed class FakeOrganizationRepository : IOrganizationRepository
{
    private readonly List<Organization> _organizations;
    private int _nextId = 1;

    public FakeOrganizationRepository(List<Organization> organizations)
    {
        _organizations = organizations;
        _nextId = organizations.Count > 0 ? organizations.Max(o => o.Id) + 1 : 1;
    }

    public Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_organizations.FirstOrDefault(o => o.Id == id));

    public Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_organizations.FirstOrDefault(o => o.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));

    public Task<Organization?> GetByNameAsync(string nameAr, CancellationToken cancellationToken = default) =>
        Task.FromResult(_organizations.FirstOrDefault(o => o.NameAr.Equals(nameAr, StringComparison.OrdinalIgnoreCase)));

    public Task<Organization?> GetDefaultAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_organizations.FirstOrDefault(o => o.IsDefault));

    public Task<IReadOnlyList<Organization>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Organization>>(activeOnly ? _organizations.Where(o => o.IsActive).ToList() : _organizations);

    public void Add(Organization organization)
    {
        if (organization.Id == 0)
        {
            typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(organization, _nextId++);
        }
        _organizations.Add(organization);
    }

    public void Update(Organization organization) { }
}

public sealed class FakeBeneficiaryRepository : IBeneficiaryRepository
{
    private readonly List<Beneficiary> _beneficiaries;
    private readonly List<EntryAttempt> _attempts;

    public FakeBeneficiaryRepository(List<Beneficiary> beneficiaries, List<EntryAttempt> attempts)
    {
        _beneficiaries = beneficiaries;
        _attempts = attempts;
    }

    public Task<Beneficiary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_beneficiaries.FirstOrDefault(b => b.Id == id));

    public Task<Beneficiary?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_beneficiaries.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyList<Beneficiary>> GetByCardNumberAsync(CardNumber cardNumber, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Beneficiary> matches = _beneficiaries.Where(b => b.CardNumber == cardNumber).ToList();
        return Task.FromResult(matches);
    }

    public Task<Beneficiary?> GetByOrgAndCardAsync(int organizationId, CardNumber cardNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(_beneficiaries.FirstOrDefault(b => b.OrganizationId == organizationId && b.CardNumber == cardNumber));

    public Task<bool> ExistsNationalIdAsync(NationalId nationalId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var exists = _beneficiaries.Any(b => b.NationalId == nationalId && (!excludeId.HasValue || b.Id != excludeId.Value));
        return Task.FromResult(exists);
    }

    public Task<DateTime?> GetLatestGrantedEntryUtcAsync(Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        var latest = _attempts
            .Where(a => a.BeneficiaryId == beneficiaryId && !a.IsVoided && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden))
            .OrderByDescending(a => a.AttemptedAtUtc)
            .FirstOrDefault();

        return Task.FromResult(latest?.AttemptedAtUtc);
    }

    public Task<IReadOnlyList<Beneficiary>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Task.FromResult<IReadOnlyList<Beneficiary>>([]);
        }

        var trimmed = searchTerm.Trim();
        var hasCard = CardNumber.TryCreate(trimmed, out var card);
        var hasPhone = PhoneNumber.TryCreate(trimmed, out var phone);
        var hasNatId = NationalId.TryCreate(trimmed, out var natId);

        IReadOnlyList<Beneficiary> matches = _beneficiaries
            .Where(b => b.FullName.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
                        (hasCard && b.CardNumber == card) ||
                        (hasPhone && b.Phone == phone) ||
                        (hasNatId && b.NationalId == natId))
            .Take(maxResults)
            .ToList();

        return Task.FromResult(matches);
    }

    public void Add(Beneficiary beneficiary) => _beneficiaries.Add(beneficiary);

    public void Update(Beneficiary beneficiary) { }
}

public sealed class FakeEntryAttemptRepository : IEntryAttemptRepository
{
    private readonly List<EntryAttempt> _attempts;

    public FakeEntryAttemptRepository(List<EntryAttempt> attempts)
    {
        _attempts = attempts;
    }

    public Task<EntryAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public Task<EntryAttempt?> GetLatestAttemptForBeneficiaryAsync(Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        var latest = _attempts
            .Where(a => a.BeneficiaryId == beneficiaryId)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .FirstOrDefault();
        return Task.FromResult(latest);
    }

    public Task<EntryAttempt?> GetLatestDenialAsync(Guid beneficiaryId, int workstationId, CancellationToken cancellationToken = default)
    {
        var latest = _attempts
            .Where(a => a.BeneficiaryId == beneficiaryId && a.WorkstationId == workstationId && a.Result == EntryResult.Denied)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .FirstOrDefault();
        return Task.FromResult(latest);
    }

    public Task<IReadOnlyList<EntryAttempt>> GetHistoryByBeneficiaryIdAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntryAttempt> list = _attempts
            .Where(a => a.BeneficiaryId == beneficiaryId)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .Take(maxResults)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<EntryAttempt>> GetTodayByWorkstationIdAsync(int workstationId, DateOnly businessDate, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EntryAttempt> list = _attempts
            .Where(a => a.WorkstationId == workstationId && a.BusinessDate == businessDate)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .Take(maxResults)
            .ToList();
        return Task.FromResult(list);
    }

    public void Add(EntryAttempt attempt) => _attempts.Add(attempt);

    public void Update(EntryAttempt attempt) { }
}

public sealed class FakeAgeCategoryRepository : IAgeCategoryRepository
{
    private readonly List<AgeCategory> _categories;
    private int _nextId = 1;

    public FakeAgeCategoryRepository(List<AgeCategory> categories)
    {
        _categories = categories;
        _nextId = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1;
    }

    public Task<AgeCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<AgeCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AgeCategory>>(_categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList());

    public Task<IReadOnlyList<AgeCategory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AgeCategory>>(_categories.OrderBy(c => c.SortOrder).ToList());

    public Task<AgeCategory?> GetByNameAsync(string nameAr, CancellationToken cancellationToken = default) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.NameAr.Equals(nameAr, StringComparison.OrdinalIgnoreCase)));

    public void Add(AgeCategory category)
    {
        if (category.Id == 0)
        {
            typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(category, _nextId++);
        }
        _categories.Add(category);
    }

    public void Update(AgeCategory category) { }
}

public sealed class FakeAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = [];

    public void Add(AuditLog log) => _logs.Add(log);

    public Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AuditLog>>(_logs.OrderByDescending(l => l.AtUtc).Take(count).ToList());
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public IUserRepository Users { get; }
    public IWorkstationRepository Workstations { get; }
    public IAppSettingRepository AppSettings { get; }
    public IBeneficiaryRepository Beneficiaries { get; }
    public IOrganizationRepository Organizations { get; }
    public IEntryAttemptRepository EntryAttempts { get; }
    public IAgeCategoryRepository AgeCategories { get; }
    public IAuditLogRepository AuditLogs { get; }

    public DateTime CurrentDbTime { get; set; } = new(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);
    public int SaveChangesCalls { get; private set; }

    public FakeUnitOfWork(
        List<User> users,
        List<Workstation> workstations,
        FakeAppSettingRepository appSettings,
        List<Organization> organizations,
        List<Beneficiary> beneficiaries,
        List<EntryAttempt> attempts,
        List<AgeCategory> categories)
    {
        Users = new FakeUserRepository(users);
        Workstations = new FakeWorkstationRepository(workstations);
        AppSettings = appSettings;
        Organizations = new FakeOrganizationRepository(organizations);
        Beneficiaries = new FakeBeneficiaryRepository(beneficiaries, attempts);
        EntryAttempts = new FakeEntryAttemptRepository(attempts);
        AgeCategories = new FakeAgeCategoryRepository(categories);
        AuditLogs = new FakeAuditLogRepository();
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
    private readonly List<Organization> _organizations;
    private readonly List<Beneficiary> _beneficiaries;
    private readonly List<EntryAttempt> _attempts;
    private readonly List<AgeCategory> _categories;

    public FakeUnitOfWork CurrentUow { get; }

    public FakeUnitOfWorkFactory(
        List<User>? users = null,
        List<Workstation>? workstations = null,
        FakeAppSettingRepository? appSettings = null,
        List<Organization>? organizations = null,
        List<Beneficiary>? beneficiaries = null,
        List<EntryAttempt>? attempts = null,
        List<AgeCategory>? categories = null)
    {
        _users = users ?? [];
        _workstations = workstations ?? [];
        _appSettings = appSettings ?? new FakeAppSettingRepository();
        _organizations = organizations ?? [];
        _beneficiaries = beneficiaries ?? [];
        _attempts = attempts ?? [];
        _categories = categories ?? [];

        CurrentUow = new FakeUnitOfWork(
            _users,
            _workstations,
            _appSettings,
            _organizations,
            _beneficiaries,
            _attempts,
            _categories);
    }

    public IUnitOfWork Create() => CurrentUow;
}
