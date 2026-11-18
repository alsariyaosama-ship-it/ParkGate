namespace ParkGate.Application.Ports.Persistence;

/// <summary>Reads how much space the database's data files take (R6.8).</summary>
public interface IDatabaseSizeQueries
{
    /// <summary>Total size of the data files, log excluded: the SQL Server Express limit counts data only.</summary>
    Task<long> GetDataFilesSizeBytesAsync(CancellationToken cancellationToken = default);
}
