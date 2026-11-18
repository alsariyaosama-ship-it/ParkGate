using Microsoft.EntityFrameworkCore;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// The single place that reads the database clock (BR-05). R6.12 replaces the SQL text here with a mapped
/// built-in function; every caller stays unchanged.
/// </summary>
internal static class DatabaseClock
{
    public static async Task<DateTime> GetUtcNowAsync(DbContext context, CancellationToken cancellationToken)
    {
        var dbTime = await context.Database
            .SqlQueryRaw<DateTime>("SELECT SYSUTCDATETIME() AS [Value]")
            .SingleAsync(cancellationToken);

        return DateTime.SpecifyKind(dbTime, DateTimeKind.Utc);
    }
}
