namespace ParkGate.Domain.Entities;

/// <summary>
/// System configuration key-value pair stored in database.
/// </summary>
public sealed class AppSetting
{
    public const int MaxKeyLength = 100;
    public const int MaxValueLength = 500;

    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public DateTime UpdatedAtUtc { get; private set; }
    public int? UpdatedByUserId { get; private set; }

    private AppSetting() { }

    public AppSetting(string key, string value, DateTime updatedAtUtc, int? updatedByUserId = null)
    {
        ValidateKey(key);
        ValidateValue(value);

        if (updatedAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(updatedAtUtc));
        }

        Key = key.Trim();
        Value = value.Trim();
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = updatedByUserId;
    }

    public void Update(string value, DateTime updatedAtUtc, int? updatedByUserId = null)
    {
        ValidateValue(value);

        if (updatedAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(updatedAtUtc));
        }

        Value = value.Trim();
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = updatedByUserId;
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Trim().Length > MaxKeyLength)
        {
            throw new ArgumentException($"Setting key must be between 1 and {MaxKeyLength} characters.", nameof(key));
        }
    }

    private static void ValidateValue(string value)
    {
        if (value == null || value.Length > MaxValueLength)
        {
            throw new ArgumentException($"Setting value cannot be null or exceed {MaxValueLength} characters.", nameof(value));
        }
    }
}
