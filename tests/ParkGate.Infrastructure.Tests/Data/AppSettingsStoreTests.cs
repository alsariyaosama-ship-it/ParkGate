using Microsoft.EntityFrameworkCore;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Data;

[Trait("Category", "Integration")]
public sealed class AppSettingsStoreTests
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
    public async Task GetValueAsync_ForSeededSetting_ShouldReturnValue()
    {
        var factory = CreateContextFactory();
        var store = new AppSettingsStore(factory);

        var parkName = await store.GetValueAsync("Park.Name");

        parkName.ShouldBe("منتزه المرح العائلي");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSeededSettings()
    {
        var factory = CreateContextFactory();
        var store = new AppSettingsStore(factory);

        var all = await store.GetAllAsync();

        all.ShouldContainKey("Park.Name");
        all.ShouldContainKey("Entry.DefaultCooldownMinutes");
        all["Entry.DefaultCooldownMinutes"].ShouldBe("720");
    }

    [Fact]
    public async Task SetValueAsync_ShouldUpdateAndPersist()
    {
        var factory = CreateContextFactory();
        var store = new AppSettingsStore(factory);

        var testKey = $"Test.DynamicSetting.{Guid.NewGuid():N}";
        await store.SetValueAsync(testKey, "TestValue123");

        var retrieved = await store.GetValueAsync(testKey);
        retrieved.ShouldBe("TestValue123");

        await store.SetValueAsync(testKey, "UpdatedValue456");
        var updated = await store.GetValueAsync(testKey);
        updated.ShouldBe("UpdatedValue456");
    }
}
