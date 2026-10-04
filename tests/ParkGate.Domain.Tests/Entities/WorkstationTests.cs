using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class WorkstationTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var ws = new Workstation("  DESK-01  ", "الشباك الأول", WorkstationType.Desk);

        ws.MachineName.ShouldBe("DESK-01");
        ws.DisplayName.ShouldBe("الشباك الأول");
        ws.Type.ShouldBe(WorkstationType.Desk);
        ws.IsActive.ShouldBeTrue();
        ws.LastSeenUtc.ShouldBeNull();
    }

    [Fact]
    public void RecordHeartbeat_UpdatesLastSeenUtc()
    {
        var ws = new Workstation("DESK-01", "الشباك الأول");
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

        ws.RecordHeartbeat(nowUtc);
        ws.LastSeenUtc.ShouldBe(nowUtc);
    }

    [Fact]
    public void Update_ModifiesDetails()
    {
        var ws = new Workstation("DESK-01", "الشباك الأول");
        ws.Update("البوابة الرئيسية", WorkstationType.Gate);

        ws.DisplayName.ShouldBe("البوابة الرئيسية");
        ws.Type.ShouldBe(WorkstationType.Gate);
    }

    [Fact]
    public void ActivateAndDeactivate_TogglesStatus()
    {
        var ws = new Workstation("DESK-01", "الشباك الأول");
        ws.Deactivate();
        ws.IsActive.ShouldBeFalse();

        ws.Activate();
        ws.IsActive.ShouldBeTrue();
    }
}
