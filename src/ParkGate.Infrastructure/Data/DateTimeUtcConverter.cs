using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Value converter ensuring that DateTime values read from SQL Server have DateTimeKind.Utc.
/// </summary>
public sealed class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

/// <summary>
/// Value converter ensuring that nullable DateTime values read from SQL Server have DateTimeKind.Utc.
/// </summary>
public sealed class NullableDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableDateTimeUtcConverter()
        : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null)
    {
    }
}
