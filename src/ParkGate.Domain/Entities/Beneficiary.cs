using ParkGate.Domain.Common;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Domain.Entities;

/// <summary>
/// Beneficiary (حامل البطاقة) entity with protected invariants and lifecycle methods.
/// </summary>
public sealed class Beneficiary : IAuditableEntity
{
    public const int MaxFullNameLength = 150;
    public const int MaxStatusReasonLength = 250;
    public const int MaxNotesLength = 500;

    public Guid Id { get; private set; }
    public int OrganizationId { get; private set; }
    public CardNumber CardNumber { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;
    public NationalId? NationalId { get; private set; }
    public DateOnly? CardExpiryDate { get; private set; }
    public BeneficiaryStatus Status { get; private set; }
    public string? StatusReason { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? LastGrantedEntryUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    private Beneficiary() { }

    public Beneficiary(
        Guid id,
        int organizationId,
        CardNumber cardNumber,
        string fullName,
        PhoneNumber phone,
        NationalId? nationalId = null,
        DateOnly? cardExpiryDate = null,
        string? notes = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Beneficiary Id cannot be empty.", nameof(id));
        }

        if (organizationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(organizationId), "Organization Id must be positive.");
        }

        ArgumentNullException.ThrowIfNull(cardNumber);
        ArgumentNullException.ThrowIfNull(phone);
        ValidateFullName(fullName);
        ValidateNotes(notes);

        Id = id;
        OrganizationId = organizationId;
        CardNumber = cardNumber;
        FullName = fullName.Trim();
        Phone = phone;
        NationalId = nationalId;
        CardExpiryDate = cardExpiryDate;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Status = BeneficiaryStatus.Active;
        StatusReason = null;
    }

    public void UpdateDetails(
        string fullName,
        PhoneNumber phone,
        NationalId? nationalId,
        DateOnly? cardExpiryDate,
        string? notes)
    {
        ArgumentNullException.ThrowIfNull(phone);
        ValidateFullName(fullName);
        ValidateNotes(notes);

        FullName = fullName.Trim();
        Phone = phone;
        NationalId = nationalId;
        CardExpiryDate = cardExpiryDate;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void ChangeCardNumber(CardNumber newCardNumber)
    {
        ArgumentNullException.ThrowIfNull(newCardNumber);
        CardNumber = newCardNumber;
    }

    public void Suspend(string reason)
    {
        ValidateStatusReason(reason, "suspension");
        Status = BeneficiaryStatus.Suspended;
        StatusReason = reason.Trim();
    }

    public void Block(string reason)
    {
        ValidateStatusReason(reason, "block");
        Status = BeneficiaryStatus.Blocked;
        StatusReason = reason.Trim();
    }

    public void Reactivate()
    {
        Status = BeneficiaryStatus.Active;
        StatusReason = null;
    }

    public void RecordGrantedEntry(DateTime entryUtc)
    {
        if (entryUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Entry timestamp must have UTC kind.", nameof(entryUtc));
        }

        LastGrantedEntryUtc = entryUtc;
    }

    public void RecalculateLastGrantedEntry(DateTime? latestGrantedUtc)
    {
        if (latestGrantedUtc.HasValue && latestGrantedUtc.Value.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Entry timestamp must have UTC kind.", nameof(latestGrantedUtc));
        }

        LastGrantedEntryUtc = latestGrantedUtc;
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name must be between 1 and {MaxFullNameLength} characters.", nameof(fullName));
        }
    }

    private static void ValidateStatusReason(string reason, string action)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > MaxStatusReasonLength)
        {
            throw new ArgumentException($"A reason for {action} is required and cannot exceed {MaxStatusReasonLength} characters.", nameof(reason));
        }
    }

    private static void ValidateNotes(string? notes)
    {
        if (notes != null && notes.Trim().Length > MaxNotesLength)
        {
            throw new ArgumentException($"Notes cannot exceed {MaxNotesLength} characters.", nameof(notes));
        }
    }
}
