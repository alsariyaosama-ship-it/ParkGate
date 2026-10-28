using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Infrastructure.Data;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class DynamicParkGateDbContextFactoryTests
{
    private sealed class FakeConnectionSettingsStore : IConnectionSettingsStore
    {
        private string? _connectionString;

        public FakeConnectionSettingsStore(string? initial = null)
        {
            _connectionString = initial;
        }

        public string? GetConnectionString() => _connectionString;

        public bool Exists() => !string.IsNullOrWhiteSpace(_connectionString);

        public void SaveConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Clear()
        {
            _connectionString = null;
        }
    }

    [Fact]
    public void CreateDbContext_ShouldUseInitialConnectionString()
    {
        // Arrange
        const string initialCs = "Server=InitialServer;Database=ParkGate_Init;Trusted_Connection=True;TrustServerCertificate=True;";
        var store = new FakeConnectionSettingsStore(initialCs);
        var provider = new AppConnectionStringProvider(store);
        var factory = new DynamicParkGateDbContextFactory(provider);

        // Act
        using var context = factory.CreateDbContext();

        // Assert
        var csBuilder = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
        csBuilder.DataSource.ShouldBe("InitialServer");
        csBuilder.InitialCatalog.ShouldBe("ParkGate_Init");
    }

    [Fact]
    public void CreateDbContext_AfterUpdateConnectionString_ShouldImmediatelyUseNewConnectionString()
    {
        // Arrange
        const string initialCs = "Server=InitialServer;Database=ParkGate_Init;Trusted_Connection=True;TrustServerCertificate=True;";
        const string updatedCs = "Server=UpdatedServer;Database=ParkGate_Updated;Trusted_Connection=True;TrustServerCertificate=True;";

        var store = new FakeConnectionSettingsStore(initialCs);
        var provider = new AppConnectionStringProvider(store);
        var factory = new DynamicParkGateDbContextFactory(provider);

        using (var initialContext = factory.CreateDbContext())
        {
            var initialBuilder = new SqlConnectionStringBuilder(initialContext.Database.GetConnectionString());
            initialBuilder.DataSource.ShouldBe("InitialServer");
            initialBuilder.InitialCatalog.ShouldBe("ParkGate_Init");
        }

        // Act - Simulate user updating connection in ConnectionSettingsForm
        provider.UpdateConnectionString(updatedCs);

        // Assert - Next created DbContext must immediately use updatedCs
        using (var updatedContext = factory.CreateDbContext())
        {
            var updatedBuilder = new SqlConnectionStringBuilder(updatedContext.Database.GetConnectionString());
            updatedBuilder.DataSource.ShouldBe("UpdatedServer");
            updatedBuilder.InitialCatalog.ShouldBe("ParkGate_Updated");
        }
    }

    [Fact]
    public async Task CreateDbContextAsync_AfterUpdateConnectionString_ShouldImmediatelyUseNewConnectionString()
    {
        // Arrange
        const string initialCs = "Server=InitialServer;Database=ParkGate_Init;Trusted_Connection=True;TrustServerCertificate=True;";
        const string updatedCs = "Server=UpdatedServer;Database=ParkGate_Updated;Trusted_Connection=True;TrustServerCertificate=True;";

        var store = new FakeConnectionSettingsStore(initialCs);
        var provider = new AppConnectionStringProvider(store);
        var factory = new DynamicParkGateDbContextFactory(provider);

        // Act
        provider.UpdateConnectionString(updatedCs);
        await using var context = await factory.CreateDbContextAsync();

        // Assert
        var csBuilder = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
        csBuilder.DataSource.ShouldBe("UpdatedServer");
        csBuilder.InitialCatalog.ShouldBe("ParkGate_Updated");
    }

    [Fact]
    public void DiContainer_ResolvesDynamicDbContextFactory_AndReactsToRuntimeConnectionChanges()
    {
        // Arrange - Setup DI container via AddInfrastructure with an isolated store
        var services = new ServiceCollection();
        const string initialCs = "Server=DiServer1;Database=ParkGate_Di1;Trusted_Connection=True;TrustServerCertificate=True;";
        services.AddInfrastructure();
        services.AddSingleton<IConnectionSettingsStore>(new FakeConnectionSettingsStore(initialCs));

        var serviceProvider = services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ParkGateDbContext>>();
        var connectionProvider = serviceProvider.GetRequiredService<IAppConnectionStringProvider>();

        // Verify initial connection string
        using (var context1 = contextFactory.CreateDbContext())
        {
            var builder1 = new SqlConnectionStringBuilder(context1.Database.GetConnectionString());
            builder1.DataSource.ShouldBe("DiServer1");
            builder1.InitialCatalog.ShouldBe("ParkGate_Di1");
        }

        // Act - Update connection string dynamically
        const string updatedCs = "Server=DiServer2;Database=ParkGate_Di2;Trusted_Connection=True;TrustServerCertificate=True;";
        connectionProvider.UpdateConnectionString(updatedCs);

        // Assert - Subsequent context created from the SAME factory instance must have the new connection string
        using (var context2 = contextFactory.CreateDbContext())
        {
            var builder2 = new SqlConnectionStringBuilder(context2.Database.GetConnectionString());
            builder2.DataSource.ShouldBe("DiServer2");
            builder2.InitialCatalog.ShouldBe("ParkGate_Di2");
        }
    }
}
