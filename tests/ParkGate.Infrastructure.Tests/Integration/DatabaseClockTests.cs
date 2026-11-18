using Microsoft.EntityFrameworkCore;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>R6.12: the database clock is read through a mapped built-in function, not SQL text (ADR-025).</summary>
[Trait("Category", "Integration")]
public sealed class DatabaseClockTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public DatabaseClockTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetDbUtcNowAsync_ReturnsServerUtcTime()
    {
        await using var uow = _fixture.CreateUnitOfWork();

        var before = DateTime.UtcNow.AddSeconds(-5);
        var now = await uow.GetDbUtcNowAsync();
        var after = DateTime.UtcNow.AddSeconds(5);

        now.Kind.ShouldBe(DateTimeKind.Utc);
        now.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task SysUtcDateTime_IsMappedAsBuiltInFunction()
    {
        await using var context = _fixture.CreateDbContext();

        var function = context.Model.GetDbFunctions().SingleOrDefault(f => f.Name == "SYSUTCDATETIME");

        function.ShouldNotBeNull();
        function.IsBuiltIn.ShouldBeTrue();
    }
}
