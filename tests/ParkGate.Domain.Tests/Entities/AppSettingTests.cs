using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class AppSettingTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        var setting = new AppSetting("Park.Name", "منتزه المرح العائلي", nowUtc, 1);

        setting.Key.ShouldBe("Park.Name");
        setting.Value.ShouldBe("منتزه المرح العائلي");
        setting.UpdatedAtUtc.ShouldBe(nowUtc);
        setting.UpdatedByUserId.ShouldBe(1);
    }

    [Fact]
    public void Update_ModifiesValueAndTimestamp()
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        var setting = new AppSetting("Entry.MaxGroupSize", "10", nowUtc);

        var updateUtc = nowUtc.AddHours(1);
        setting.Update("15", updateUtc, 2);

        setting.Value.ShouldBe("15");
        setting.UpdatedAtUtc.ShouldBe(updateUtc);
        setting.UpdatedByUserId.ShouldBe(2);
    }
}
