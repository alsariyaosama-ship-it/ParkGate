using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations tools.
/// </summary>
public sealed class ParkGateDbContextFactory : IDesignTimeDbContextFactory<ParkGateDbContext>
{
    private const string DefaultDevConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public ParkGateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkGateDbContext>();
        optionsBuilder.UseSqlServer(DefaultDevConnectionString, b =>
        {
            b.MigrationsAssembly(typeof(ParkGateDbContext).Assembly.FullName);
        });

        return new ParkGateDbContext(optionsBuilder.Options);
    }
}
