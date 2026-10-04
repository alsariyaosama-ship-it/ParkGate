using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class AuditLogTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        var log = new AuditLog(
            nowUtc,
            "Beneficiary.Updated",
            userId: 1,
            workstationId: 2,
            entityName: "Beneficiary",
            entityId: "123",
            detailsJson: "{\"field\":\"Notes\"}");

        log.AtUtc.ShouldBe(nowUtc);
        log.Action.ShouldBe("Beneficiary.Updated");
        log.UserId.ShouldBe(1);
        log.WorkstationId.ShouldBe(2);
        log.EntityName.ShouldBe("Beneficiary");
        log.EntityId.ShouldBe("123");
        log.DetailsJson.ShouldBe("{\"field\":\"Notes\"}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidAction_ThrowsArgumentException(string? invalidAction)
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        Should.Throw<ArgumentException>(() => new AuditLog(nowUtc, invalidAction!));
    }
}
