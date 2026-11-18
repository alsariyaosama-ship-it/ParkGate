using Microsoft.EntityFrameworkCore;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// The single place that reads the database clock (BR-05), through SQL Server's built-in
/// <c>SYSUTCDATETIME()</c> mapped as an EF function, so no SQL text is needed (ADR-025, R6.12).
/// </summary>
public static class DatabaseClock
{
    /// <summary>Mapped to <c>SYSUTCDATETIME()</c>; usable only inside a LINQ to Entities query.</summary>
    public static DateTime SysUtcDateTime() =>
        throw new InvalidOperationException("SysUtcDateTime runs only inside a database query.");

    internal static void MapFunctions(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(DatabaseClock).GetMethod(nameof(SysUtcDateTime))!)
            .HasName("SYSUTCDATETIME")
            .IsBuiltIn();
    }

    internal static async Task<DateTime> GetUtcNowAsync(ParkGateDbContext context, CancellationToken cancellationToken)
    {
        // Any always-present row gives the query a FROM clause; the default organization is seeded and,
        // like every organization, never hard-deleted (BR-14).
        var dbTime = await context.Organizations
            .AsNoTracking()
            .Select(_ => SysUtcDateTime())
            .FirstAsync(cancellationToken);

        return DateTime.SpecifyKind(dbTime, DateTimeKind.Utc);
    }
}
