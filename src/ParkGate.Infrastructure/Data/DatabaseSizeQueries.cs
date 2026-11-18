using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

public sealed class DatabaseSizeQueries : IDatabaseSizeQueries
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public DatabaseSizeQueries(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<long> GetDataFilesSizeBytesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var pages = await context.Set<DatabaseFileRow>()
            .Where(f => f.Type == DatabaseFileRow.DataFileType)
            .SumAsync(f => (long)f.SizePages, cancellationToken);

        return pages * DatabaseFileRow.PageBytes;
    }
}
