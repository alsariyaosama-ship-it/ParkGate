using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class BeneficiaryTests
{
    private static Beneficiary CreateSampleBeneficiary()
    {
        return new Beneficiary(
            Guid.NewGuid(),
            organizationId: 1,
            CardNumber.Create("SSF-001"),
            "علي بن سالم",
            PhoneNumber.Create("0912345678"),
            NationalId.Create("119850123456"),
            new DateOnly(2027, 12, 31),
            "ملاحظة أولية");
    }

    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var id = Guid.NewGuid();
        var card = CardNumber.Create("SSF-001");
        var phone = PhoneNumber.Create("0912345678");
        var nationalId = NationalId.Create("119850123456");
        var expiry = new DateOnly(2027, 12, 31);

        var beneficiary = new Beneficiary(id, 1, card, "علي بن سالم", phone, nationalId, expiry, "ملاحظات");

        beneficiary.Id.ShouldBe(id);
        beneficiary.OrganizationId.ShouldBe(1);
        beneficiary.CardNumber.ShouldBe(card);
        beneficiary.FullName.ShouldBe("علي بن سالم");
        beneficiary.Phone.ShouldBe(phone);
        beneficiary.NationalId.ShouldBe(nationalId);
        beneficiary.CardExpiryDate.ShouldBe(expiry);
        beneficiary.Status.ShouldBe(BeneficiaryStatus.Active);
        beneficiary.StatusReason.ShouldBeNull();
        beneficiary.Notes.ShouldBe("ملاحظات");
        beneficiary.LastGrantedEntryUtc.ShouldBeNull();
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Beneficiary(
            Guid.Empty,
            1,
            CardNumber.Create("SSF-001"),
            "علي",
            PhoneNumber.Create("0912345678")));
    }

    [Fact]
    public void Suspend_ValidReason_SetsSuspendedStatus()
    {
        var b = CreateSampleBeneficiary();
        b.Suspend("إيقاف مؤقت للتحقق");

        b.Status.ShouldBe(BeneficiaryStatus.Suspended);
        b.StatusReason.ShouldBe("إيقاف مؤقت للتحقق");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Suspend_EmptyReason_ThrowsArgumentException(string? reason)
    {
        var b = CreateSampleBeneficiary();
        Should.Throw<ArgumentException>(() => b.Suspend(reason!));
    }

    [Fact]
    public void Block_ValidReason_SetsBlockedStatus()
    {
        var b = CreateSampleBeneficiary();
        b.Block("مخالفة قواعد المنتزه");

        b.Status.ShouldBe(BeneficiaryStatus.Blocked);
        b.StatusReason.ShouldBe("مخالفة قواعد المنتزه");
    }

    [Fact]
    public void Reactivate_ClearsReasonAndSetsActive()
    {
        var b = CreateSampleBeneficiary();
        b.Suspend("إيقاف مؤقت");
        b.Reactivate();

        b.Status.ShouldBe(BeneficiaryStatus.Active);
        b.StatusReason.ShouldBeNull();
    }

    [Fact]
    public void ChangeCardNumber_UpdatesCardNumber()
    {
        var b = CreateSampleBeneficiary();
        var newCard = CardNumber.Create("SSF-999");
        b.ChangeCardNumber(newCard);

        b.CardNumber.ShouldBe(newCard);
    }

    [Fact]
    public void RecordGrantedEntry_UtcTime_UpdatesTimestamp()
    {
        var b = CreateSampleBeneficiary();
        var nowUtc = DateTime.UtcNow;

        b.RecordGrantedEntry(nowUtc);
        b.LastGrantedEntryUtc.ShouldBe(nowUtc);
    }

    [Fact]
    public void RecordGrantedEntry_UnspecifiedTime_ThrowsArgumentException()
    {
        var b = CreateSampleBeneficiary();
        var unspecTime = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Unspecified);

        Should.Throw<ArgumentException>(() => b.RecordGrantedEntry(unspecTime));
    }

    [Fact]
    public void RecalculateLastGrantedEntry_UpdatesTimestamp()
    {
        var b = CreateSampleBeneficiary();
        var entryUtc = DateTime.UtcNow;
        b.RecordGrantedEntry(entryUtc);

        b.RecalculateLastGrantedEntry(null);
        b.LastGrantedEntryUtc.ShouldBeNull();
    }
}
