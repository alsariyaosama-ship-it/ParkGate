using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Enums;

public sealed class EnumTests
{
    [Fact]
    public void BeneficiaryStatus_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(BeneficiaryStatus)).ShouldBe(typeof(byte));
        ((byte)BeneficiaryStatus.Active).ShouldBe((byte)1);
        ((byte)BeneficiaryStatus.Suspended).ShouldBe((byte)2);
        ((byte)BeneficiaryStatus.Blocked).ShouldBe((byte)3);
    }

    [Fact]
    public void EntryResult_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(EntryResult)).ShouldBe(typeof(byte));
        ((byte)EntryResult.Allowed).ShouldBe((byte)1);
        ((byte)EntryResult.Denied).ShouldBe((byte)2);
        ((byte)EntryResult.Overridden).ShouldBe((byte)3);
    }

    [Fact]
    public void DenialReason_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(DenialReason)).ShouldBe(typeof(byte));
        ((byte)DenialReason.CooldownActive).ShouldBe((byte)1);
        ((byte)DenialReason.HolderSuspended).ShouldBe((byte)2);
        ((byte)DenialReason.HolderBlocked).ShouldBe((byte)3);
        ((byte)DenialReason.CardExpired).ShouldBe((byte)4);
        ((byte)DenialReason.OrganizationInactive).ShouldBe((byte)5);
    }

    [Fact]
    public void EntryChannel_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(EntryChannel)).ShouldBe(typeof(byte));
        ((byte)EntryChannel.Desk).ShouldBe((byte)1);
        ((byte)EntryChannel.QrDesk).ShouldBe((byte)2);
        ((byte)EntryChannel.Gate).ShouldBe((byte)3);
    }

    [Fact]
    public void UserRole_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(UserRole)).ShouldBe(typeof(byte));
        ((byte)UserRole.Operator).ShouldBe((byte)1);
        ((byte)UserRole.Supervisor).ShouldBe((byte)2);
        ((byte)UserRole.Admin).ShouldBe((byte)3);
    }

    [Fact]
    public void WorkstationType_HasExpectedUnderlyingTypeAndValues()
    {
        Enum.GetUnderlyingType(typeof(WorkstationType)).ShouldBe(typeof(byte));
        ((byte)WorkstationType.Desk).ShouldBe((byte)1);
        ((byte)WorkstationType.Gate).ShouldBe((byte)2);
    }
}
