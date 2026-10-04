using ParkGate.Domain.Common;

namespace ParkGate.Domain.Entities;

/// <summary>
/// Organization (الجهة) issuing membership cards.
/// </summary>
public sealed class Organization : IAuditableEntity
{
    public const int MaxCodeLength = 20;
    public const int MaxNameLength = 150;
    public const int MinCooldownMinutes = 1;
    public const int MaxCooldownMinutes = 10080; // 7 days in minutes

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public int? CooldownMinutes { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    private Organization() { }

    public Organization(string code, string nameAr, int? cooldownMinutes = null, bool isDefault = false, bool isActive = true)
    {
        ValidateCode(code);
        ValidateName(nameAr);
        ValidateCooldown(cooldownMinutes);

        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        CooldownMinutes = cooldownMinutes;
        IsDefault = isDefault;
        IsActive = isActive;
    }

    public void Update(string code, string nameAr, int? cooldownMinutes)
    {
        ValidateCode(code);
        ValidateName(nameAr);
        ValidateCooldown(cooldownMinutes);

        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        CooldownMinutes = cooldownMinutes;
    }

    public void SetDefault(bool isDefault) => IsDefault = isDefault;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > MaxCodeLength)
        {
            throw new ArgumentException($"Organization code must be between 1 and {MaxCodeLength} characters.", nameof(code));
        }
    }

    private static void ValidateName(string nameAr)
    {
        if (string.IsNullOrWhiteSpace(nameAr) || nameAr.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException($"Organization Arabic name must be between 1 and {MaxNameLength} characters.", nameof(nameAr));
        }
    }

    private static void ValidateCooldown(int? cooldownMinutes)
    {
        if (cooldownMinutes.HasValue && (cooldownMinutes.Value < MinCooldownMinutes || cooldownMinutes.Value > MaxCooldownMinutes))
        {
            throw new ArgumentOutOfRangeException(nameof(cooldownMinutes), $"Cooldown must be between {MinCooldownMinutes} and {MaxCooldownMinutes} minutes.");
        }
    }
}
