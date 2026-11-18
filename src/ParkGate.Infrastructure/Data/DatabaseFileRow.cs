using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Read-only row of SQL Server's <c>sys.database_files</c> catalog view, mapped as a keyless entity so the size
/// check needs no SQL text (ADR-025, R6.8). Migrations never create or change it.
/// </summary>
public sealed class DatabaseFileRow
{
    /// <summary>0 = rows (data), 1 = log.</summary>
    public byte Type { get; private set; }

    /// <summary>Current size in 8 KB pages.</summary>
    public int SizePages { get; private set; }

    public const byte DataFileType = 0;
    public const long PageBytes = 8 * 1024;
}

internal sealed class DatabaseFileRowConfiguration : IEntityTypeConfiguration<DatabaseFileRow>
{
    public void Configure(EntityTypeBuilder<DatabaseFileRow> builder)
    {
        builder.HasNoKey();
        builder.ToView("database_files", "sys");
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.SizePages).HasColumnName("size");
    }
}
