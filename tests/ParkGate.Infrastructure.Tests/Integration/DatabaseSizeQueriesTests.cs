using Microsoft.EntityFrameworkCore;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>The size check reads sys.database_files through EF, without SQL text (R6.8, ADR-025).</summary>
[Trait("Category", "Integration")]
public sealed class DatabaseSizeQueriesTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public DatabaseSizeQueriesTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetDataFilesSizeBytesAsync_ShouldReturnTheDataFileSizeInWholePages()
    {
        var sut = new DatabaseSizeQueries(new FixtureContextFactory(_fixture));

        var bytes = await sut.GetDataFilesSizeBytesAsync();

        bytes.ShouldBeGreaterThan(0);
        (bytes % DatabaseFileRow.PageBytes).ShouldBe(0);
    }

    private sealed class FixtureContextFactory(LocalDbTestFixture fixture) : IDbContextFactory<ParkGateDbContext>
    {
        public ParkGateDbContext CreateDbContext() => fixture.CreateDbContext();
    }
}
