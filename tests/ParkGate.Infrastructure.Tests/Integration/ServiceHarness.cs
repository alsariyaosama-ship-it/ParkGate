using System.Security.Cryptography;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.EntryCards;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.GiftVouchers.Validators;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Organizations.Validators;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Entities;
using ParkGate.Domain.EntryCards;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// Builds the real check-in and gift services on the test database, one instance per simulated desk, so the
/// concurrency tests exercise production code, retries included (ADR-026, R6.13).
/// </summary>
internal sealed class ServiceHarness : IDisposable
{
    private const byte KeyId = 1;

    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly QrTokenVerifier _verifier;
    private readonly SupervisorApprovalStore _approvals = new();

    public ServiceHarness(LocalDbTestFixture fixture)
    {
        _uowFactory = new FixtureUnitOfWorkFactory(fixture);
        _verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _key.ExportSubjectPublicKeyInfo())]);
    }

    public CheckInService CheckIn(User user, Workstation desk) => new(
        _uowFactory,
        new RollingWindowEligibilityPolicy(),
        new DeskUser(user),
        new Desk(desk),
        new FixedSettings(),
        _approvals,
        new RecordEntryRequestValidator(),
        new OverrideEntryRequestValidator(),
        new VoidEntryRequestValidator());

    public GiftVoucherService Gifts(User user, Workstation desk) => new(
        _uowFactory,
        new RollingIntervalGiftEligibilityPolicy(),
        new QrTokenSigner(ECDsa.Create(_key.ExportParameters(true)), KeyId),
        _verifier,
        new DeskUser(user),
        new Desk(desk),
        _approvals,
        new IssueGiftOverrideRequestValidator(),
        new VoidGiftVoucherRequestValidator());

    public OrganizationService Organizations(User user, Workstation desk) => new(
        _uowFactory,
        new DeskUser(user),
        new Desk(desk),
        new CreateOrganizationRequestValidator(),
        new UpdateOrganizationRequestValidator(),
        new SetOrganizationPolicyRequestValidator());

    /// <summary>Starts every action at the same moment so the database work really overlaps.</summary>
    public static async Task<T[]> RunTogetherAsync<T>(int parallelism, Func<int, Task<T>> action)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = Enumerable.Range(0, parallelism)
            .Select(i => Task.Run(async () =>
            {
                await gate.Task;
                return await action(i);
            }))
            .ToArray();

        await Task.Delay(100);
        gate.SetResult();
        return await Task.WhenAll(tasks);
    }

    public void Dispose() => _key.Dispose();

    private sealed class FixtureUnitOfWorkFactory(LocalDbTestFixture fixture) : IUnitOfWorkFactory
    {
        public IUnitOfWork Create() => fixture.CreateUnitOfWork();
    }

    private sealed class DeskUser(User user) : ICurrentUser
    {
        public int? UserId => user.Id;
        public string? UserName => user.UserName;
        public string? FullName => user.FullName;
        public UserRole? Role => user.Role;
        public bool IsAuthenticated => true;
        public bool Has(Permission permission) => RolePermissions.GetPermissions(user.Role).Contains(permission);
        public bool HasAny(params Permission[] permissions) => permissions.Any(Has);
        public bool IsInRole(UserRole role) => user.Role == role;
    }

    private sealed class Desk(Workstation workstation) : ICurrentWorkstation
    {
        public int? WorkstationId => workstation.Id;
        public string MachineName => workstation.MachineName;
        public string? DisplayName => workstation.DisplayName;
        public WorkstationType? Type => workstation.Type;
        public bool IsRegistered => true;
    }

    private sealed class FixedSettings : ISettingsService
    {
        private static readonly AppSettingsModel Current = new(
            DefaultCooldownMinutes: 720,
            MaxGroupSize: 10,
            DenialDedupMinutes: 5,
            PrefillCompanionsFromLastVisit: true,
            DenialSoundEnabled: true,
            MaxFailedLogins: 5,
            LockoutMinutes: 15,
            AutoLockMinutes: 0,
            ParkName: "Test Park",
            TimeZoneId: "Libya Standard Time");

        public Task<AppSettingsModel> GetSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Current);

        public Task<Result> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public void InvalidateCache()
        {
        }
    }
}
