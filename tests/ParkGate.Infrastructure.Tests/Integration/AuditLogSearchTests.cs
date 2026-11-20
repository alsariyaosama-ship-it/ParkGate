using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>R5.20: the audit search runs on SQL Server (date window, action list, card inside the details JSON).</summary>
[Trait("Category", "Integration")]
public sealed class AuditLogSearchTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public AuditLogSearchTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchAsync_FiltersByWindowActionsAndCard_NewestFirst()
    {
        var marker = $"T{Guid.NewGuid():N}"[..12];
        var day = new DateTime(2031, 3, 4, 0, 0, 0, DateTimeKind.Utc);
        await using (var seed = _fixture.CreateDbContext())
        {
            seed.AuditLogs.AddRange(
                new AuditLog(day.AddHours(8), "EntryGranted", detailsJson: $"{{\"CardNumber\":\"{marker}\"}}"),
                new AuditLog(day.AddHours(9), "Gift.Issued", detailsJson: $"{{\"CardNumber\":\"{marker}\"}}"),
                new AuditLog(day.AddHours(10), "Gift.Voided", detailsJson: $"{{\"CardNumber\":\"{marker}9\"}}"),
                new AuditLog(day.AddDays(1).AddHours(1), "Gift.Redeemed", detailsJson: $"{{\"CardNumber\":\"{marker}\"}}"));
            await seed.SaveChangesAsync();
        }

        await using var context = _fixture.CreateDbContext();
        var repository = new AuditLogRepository(context);
        var fragment = $"\"CardNumber\":\"{marker}\"";

        var all = await repository.SearchAsync(day, day.AddDays(1), null, fragment, 10);
        var gifts = await repository.SearchAsync(day, day.AddDays(1), ["Gift.Issued", "Gift.Voided"], fragment, 10);
        var limited = await repository.SearchAsync(day, day.AddDays(1), null, fragment, 1);

        all.Select(a => a.Action).ShouldBe(["Gift.Issued", "EntryGranted"]);
        gifts.Select(a => a.Action).ShouldBe(["Gift.Issued"]);
        limited.Count.ShouldBe(1);
    }
}
