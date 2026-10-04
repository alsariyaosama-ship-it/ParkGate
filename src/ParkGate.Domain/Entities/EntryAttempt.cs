using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Entities;

/// <summary>
/// Entry attempt (محاولة دخول) record. Immutable history of access decisions.
/// </summary>
public sealed class EntryAttempt
{
    public const int MinOverrideReasonLength = 5;
    public const int MaxOverrideReasonLength = 250;
    public const int MinVoidReasonLength = 5;
    public const int MaxVoidReasonLength = 250;

    private readonly List<EntryAttemptCompanion> _companions = [];

    public Guid Id { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public int OrganizationId { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public EntryResult Result { get; private set; }
    public DenialReason? DenialReason { get; private set; }
    public int CooldownMinutesApplied { get; private set; }
    public DateTime? NextEligibleAtUtc { get; private set; }
    public int TotalPersons { get; private set; }
    public EntryChannel Channel { get; private set; }
    public int WorkstationId { get; private set; }
    public int UserId { get; private set; }
    public int? OverrideByUserId { get; private set; }
    public string? OverrideReason { get; private set; }
    public bool IsVoided { get; private set; }
    public DateTime? VoidedAtUtc { get; private set; }
    public int? VoidedByUserId { get; private set; }
    public string? VoidReason { get; private set; }
    public byte[] RowVersion { get; set; } = [];

    public IReadOnlyCollection<EntryAttemptCompanion> Companions => _companions.AsReadOnly();

    private EntryAttempt() { }

    public static EntryAttempt CreateAllowed(
        Guid id,
        Guid beneficiaryId,
        int organizationId,
        DateTime attemptedAtUtc,
        DateOnly businessDate,
        int cooldownMinutesApplied,
        DateTime nextEligibleAtUtc,
        int workstationId,
        int userId,
        EntryChannel channel,
        IEnumerable<EntryAttemptCompanion>? companions = null)
    {
        ValidateCommon(id, beneficiaryId, organizationId, attemptedAtUtc, cooldownMinutesApplied, workstationId, userId);

        var attempt = new EntryAttempt
        {
            Id = id,
            BeneficiaryId = beneficiaryId,
            OrganizationId = organizationId,
            AttemptedAtUtc = attemptedAtUtc,
            BusinessDate = businessDate,
            Result = EntryResult.Allowed,
            DenialReason = null,
            CooldownMinutesApplied = cooldownMinutesApplied,
            NextEligibleAtUtc = nextEligibleAtUtc,
            WorkstationId = workstationId,
            UserId = userId,
            Channel = channel,
            IsVoided = false
        };

        attempt.AttachCompanions(companions);
        return attempt;
    }

    public static EntryAttempt CreateDenied(
        Guid id,
        Guid beneficiaryId,
        int organizationId,
        DateTime attemptedAtUtc,
        DateOnly businessDate,
        DenialReason denialReason,
        int cooldownMinutesApplied,
        DateTime? nextEligibleAtUtc,
        int workstationId,
        int userId,
        EntryChannel channel)
    {
        ValidateCommon(id, beneficiaryId, organizationId, attemptedAtUtc, cooldownMinutesApplied, workstationId, userId);

        return new EntryAttempt
        {
            Id = id,
            BeneficiaryId = beneficiaryId,
            OrganizationId = organizationId,
            AttemptedAtUtc = attemptedAtUtc,
            BusinessDate = businessDate,
            Result = EntryResult.Denied,
            DenialReason = denialReason,
            CooldownMinutesApplied = cooldownMinutesApplied,
            NextEligibleAtUtc = nextEligibleAtUtc,
            TotalPersons = 0,
            WorkstationId = workstationId,
            UserId = userId,
            Channel = channel,
            IsVoided = false
        };
    }

    public static EntryAttempt CreateOverridden(
        Guid id,
        Guid beneficiaryId,
        int organizationId,
        DateTime attemptedAtUtc,
        DateOnly businessDate,
        int cooldownMinutesApplied,
        DateTime nextEligibleAtUtc,
        int workstationId,
        int userId,
        int overrideByUserId,
        string overrideReason,
        EntryChannel channel,
        IEnumerable<EntryAttemptCompanion>? companions = null)
    {
        ValidateCommon(id, beneficiaryId, organizationId, attemptedAtUtc, cooldownMinutesApplied, workstationId, userId);

        if (overrideByUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(overrideByUserId), "Override supervisor Id must be positive.");
        }

        if (string.IsNullOrWhiteSpace(overrideReason) ||
            overrideReason.Trim().Length < MinOverrideReasonLength ||
            overrideReason.Trim().Length > MaxOverrideReasonLength)
        {
            throw new ArgumentException($"Override reason must be between {MinOverrideReasonLength} and {MaxOverrideReasonLength} characters (BR-11).", nameof(overrideReason));
        }

        var attempt = new EntryAttempt
        {
            Id = id,
            BeneficiaryId = beneficiaryId,
            OrganizationId = organizationId,
            AttemptedAtUtc = attemptedAtUtc,
            BusinessDate = businessDate,
            Result = EntryResult.Overridden,
            DenialReason = null,
            CooldownMinutesApplied = cooldownMinutesApplied,
            NextEligibleAtUtc = nextEligibleAtUtc,
            WorkstationId = workstationId,
            UserId = userId,
            OverrideByUserId = overrideByUserId,
            OverrideReason = overrideReason.Trim(),
            Channel = channel,
            IsVoided = false
        };

        attempt.AttachCompanions(companions);
        return attempt;
    }

    public void Void(int voidedByUserId, string voidReason, DateTime voidedAtUtc)
    {
        if (Result == EntryResult.Denied)
        {
            throw new InvalidOperationException("Denied attempts cannot be voided (BR-13).");
        }

        if (IsVoided)
        {
            throw new InvalidOperationException("Entry attempt has already been voided.");
        }

        if (voidedByUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(voidedByUserId), "VoidedByUserId must be positive.");
        }

        if (string.IsNullOrWhiteSpace(voidReason) ||
            voidReason.Trim().Length < MinVoidReasonLength ||
            voidReason.Trim().Length > MaxVoidReasonLength)
        {
            throw new ArgumentException($"Void reason must be between {MinVoidReasonLength} and {MaxVoidReasonLength} characters (BR-13).", nameof(voidReason));
        }

        if (voidedAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Voided timestamp must have UTC kind.", nameof(voidedAtUtc));
        }

        IsVoided = true;
        VoidedByUserId = voidedByUserId;
        VoidReason = voidReason.Trim();
        VoidedAtUtc = voidedAtUtc;
    }

    private void AttachCompanions(IEnumerable<EntryAttemptCompanion>? companions)
    {
        var companionSum = 0;
        if (companions != null)
        {
            foreach (var companion in companions)
            {
                companion.AttachToAttempt(Id);
                _companions.Add(companion);
                companionSum += companion.Count;
            }
        }

        // Holder counts as 1 person (BR-08)
        TotalPersons = 1 + companionSum;
    }

    private static void ValidateCommon(
        Guid id,
        Guid beneficiaryId,
        int organizationId,
        DateTime attemptedAtUtc,
        int cooldownMinutesApplied,
        int workstationId,
        int userId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Attempt Id cannot be empty.", nameof(id));
        }

        if (beneficiaryId == Guid.Empty)
        {
            throw new ArgumentException("Beneficiary Id cannot be empty.", nameof(beneficiaryId));
        }

        if (organizationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(organizationId), "Organization Id must be positive.");
        }

        if (workstationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workstationId), "Workstation Id must be positive.");
        }

        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), "User Id must be positive.");
        }

        if (cooldownMinutesApplied <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cooldownMinutesApplied), "Cooldown minutes applied must be positive.");
        }

        if (attemptedAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Attempt timestamp must have UTC kind.", nameof(attemptedAtUtc));
        }
    }
}
