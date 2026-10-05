using Microsoft.EntityFrameworkCore;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Data;

[Trait("Category", "Integration")]
public sealed class AuditWriterTests
{
    private static IDbContextFactory<ParkGateDbContext> CreateContextFactory()
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dev;Trusted_Connection=True;")
            .Options;

        return new SimpleDbContextFactory(options);
    }

    private sealed class SimpleDbContextFactory : IDbContextFactory<ParkGateDbContext>
    {
        private readonly DbContextOptions<ParkGateDbContext> _options;

        public SimpleDbContextFactory(DbContextOptions<ParkGateDbContext> options)
        {
            _options = options;
        }

        public ParkGateDbContext CreateDbContext()
        {
            return new ParkGateDbContext(_options);
        }
    }

    [Fact]
    public async Task WriteAsync_ShouldPersistAuditLogRow()
    {
        var factory = CreateContextFactory();
        var writer = new AuditWriter(factory);

        var action = $"Test.Action.{Guid.NewGuid():N}";
        await writer.WriteAsync(
            action: action,
            userId: 1,
            workstationId: null,
            entityName: "Beneficiary",
            entityId: "123",
            detailsJson: "{\"field\":\"value\"}");

        await using var context = factory.CreateDbContext();
        var savedLog = await context.AuditLogs
            .SingleOrDefaultAsync(a => a.Action == action);

        savedLog.ShouldNotBeNull();
        savedLog.UserId.ShouldBe(1);
        savedLog.EntityName.ShouldBe("Beneficiary");
        savedLog.EntityId.ShouldBe("123");
        savedLog.DetailsJson.ShouldBe("{\"field\":\"value\"}");
        savedLog.AtUtc.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
