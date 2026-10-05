using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Integration;

public sealed class LocalDbTestFixture : IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly string _connectionString;
    private DbContextOptions<ParkGateDbContext> _options = null!;

    public LocalDbTestFixture()
    {
        _databaseName = $"ParkGate_Test_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    }

    public string ConnectionString => _connectionString;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            await context.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Best effort cleanup for integration test databases
        }
    }

    public ParkGateDbContext CreateDbContext()
    {
        return new ParkGateDbContext(_options);
    }

    public IUnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(CreateDbContext());
    }
}
