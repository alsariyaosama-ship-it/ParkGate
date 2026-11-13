using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class OutboxCaptureTests
{
    private static ParkGateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;")
            .Options;

        return new ParkGateDbContext(options);
    }

    [Fact]
    public void IsSyncable_IdentifiesSyncableEntitiesCorrectly()
    {
        OutboxEntityExtractor.IsSyncable(nameof(Beneficiary)).ShouldBeTrue();
        OutboxEntityExtractor.IsSyncable(nameof(EntryAttempt)).ShouldBeTrue();
        OutboxEntityExtractor.IsSyncable(nameof(EntryAttemptCompanion)).ShouldBeTrue();
        OutboxEntityExtractor.IsSyncable(nameof(Organization)).ShouldBeTrue();
        OutboxEntityExtractor.IsSyncable(nameof(AgeCategory)).ShouldBeTrue();
        OutboxEntityExtractor.IsSyncable(nameof(GiftVoucher)).ShouldBeTrue();

        OutboxEntityExtractor.IsSyncable(nameof(User)).ShouldBeFalse();
        OutboxEntityExtractor.IsSyncable(nameof(Workstation)).ShouldBeFalse();
        OutboxEntityExtractor.IsSyncable(nameof(AppSetting)).ShouldBeFalse();
        OutboxEntityExtractor.IsSyncable(nameof(AuditLog)).ShouldBeFalse();
        OutboxEntityExtractor.IsSyncable(nameof(SyncOutbox)).ShouldBeFalse();
    }

    [Fact]
    public void FormatValue_FormatsSpecialTypesConsistently()
    {
        var card = CardNumber.Create("12345");
        OutboxEntityExtractor.FormatValue(card).ShouldBe("12345");

        var phone = PhoneNumber.Create("0912345678");
        OutboxEntityExtractor.FormatValue(phone).ShouldBe("0912345678");

        var nid = NationalId.Create("119901234567");
        OutboxEntityExtractor.FormatValue(nid).ShouldBe("119901234567");

        var date = new DateOnly(2026, 10, 13);
        OutboxEntityExtractor.FormatValue(date).ShouldBe("2026-10-13");

        var status = BeneficiaryStatus.Active;
        OutboxEntityExtractor.FormatValue(status).ShouldBe(1);

        OutboxEntityExtractor.FormatValue(null).ShouldBeNull();
    }

    [Fact]
    public void ExtractEntityId_And_SerializePayload_OnBeneficiary_ProducesValidJson()
    {
        using var context = CreateContext();
        var id = Guid.NewGuid();
        var beneficiary = new Beneficiary(
            id: id,
            organizationId: 1,
            cardNumber: CardNumber.Create("CARD-999"),
            fullName: "أسامة علي",
            phone: PhoneNumber.Create("0912345678"),
            nationalId: NationalId.Create("119901234567"),
            cardExpiryDate: new DateOnly(2027, 12, 31),
            notes: "ملاحظة");

        var entry = context.Beneficiaries.Add(beneficiary);

        var extractedId = OutboxEntityExtractor.ExtractEntityId(entry);
        extractedId.ShouldNotBeNullOrEmpty();
        extractedId.ShouldBe(id.ToString());

        var json = OutboxEntityExtractor.SerializePayload(entry);
        json.ShouldContain("\"CardNumber\":\"CARD-999\"");
        json.ShouldContain("\"FullName\":\"أسامة علي\"");
        json.ShouldContain("\"CardExpiryDate\":\"2027-12-31\"");
    }

    [Fact]
    public void SerializePayload_OnBeneficiary_OmitsPhoneAndNationalId()
    {
        using var context = CreateContext();
        var beneficiary = new Beneficiary(
            id: Guid.NewGuid(),
            organizationId: 1,
            cardNumber: CardNumber.Create("CARD-999"),
            fullName: "أسامة علي",
            phone: PhoneNumber.Create("0912345678"),
            nationalId: NationalId.Create("119901234567"),
            cardExpiryDate: new DateOnly(2027, 12, 31),
            notes: "ملاحظة");

        var entry = context.Beneficiaries.Add(beneficiary);

        var json = OutboxEntityExtractor.SerializePayload(entry);

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Phone", out _).ShouldBeFalse();
        doc.RootElement.TryGetProperty("NationalId", out _).ShouldBeFalse();
        json.ShouldNotContain("0912345678");
        json.ShouldNotContain("119901234567");
        // Q-R2b: notes and status reason keep syncing.
        doc.RootElement.GetProperty("Notes").GetString().ShouldBe("ملاحظة");
    }

    [Fact]
    public void SerializePayload_OnGiftVoucher_OmitsTheRedeemableTokenAndKeepsBusinessFields()
    {
        using var context = CreateContext();
        var token = Guid.NewGuid();
        var voucher = GiftVoucher.Issue(
            token: token,
            keyId: 7,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 2,
            value: 25.5m,
            issuedAtUtc: new DateTime(2026, 11, 13, 10, 0, 0, DateTimeKind.Utc),
            businessDate: new DateOnly(2026, 11, 13),
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        var entry = context.GiftVouchers.Add(voucher);

        var json = OutboxEntityExtractor.SerializePayload(entry);

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        // The token is the bearer credential printed on the QR card; the cloud only reports on vouchers.
        root.TryGetProperty("Token", out _).ShouldBeFalse();
        root.TryGetProperty("KeyId", out _).ShouldBeFalse();
        json.ShouldNotContain(token.ToString());
        root.GetProperty("OrganizationId").GetInt32().ShouldBe(2);
        root.GetProperty("Value").GetDecimal().ShouldBe(25.5m);
        root.GetProperty("Status").GetInt32().ShouldBe((int)GiftVoucherStatus.Issued);
        root.GetProperty("BusinessDate").GetString().ShouldBe("2026-11-13");
        root.GetProperty("ExpiresOn").GetString().ShouldBe("2026-11-13");
    }

    [Fact]
    public void SerializePayload_OnOrganization_CarriesCompanionLimitAndGiftPolicy()
    {
        using var context = CreateContext();
        var organization = new Organization("SSF", "صندوق التضامن");
        organization.SetMaxFreeCompanions(3);
        organization.ConfigureGift(enabled: true, value: 10m, intervalDays: 14);

        var entry = context.Organizations.Add(organization);

        using var doc = System.Text.Json.JsonDocument.Parse(OutboxEntityExtractor.SerializePayload(entry));
        var root = doc.RootElement;
        root.GetProperty("MaxFreeCompanions").GetInt32().ShouldBe(3);
        root.GetProperty("GiftEnabled").GetBoolean().ShouldBeTrue();
        root.GetProperty("GiftValue").GetDecimal().ShouldBe(10m);
        root.GetProperty("GiftIntervalDays").GetInt32().ShouldBe(14);
    }

    [Fact]
    public void SerializePayload_OnAgeCategory_CarriesCompanionLimitExemption()
    {
        using var context = CreateContext();
        var category = new AgeCategory("رضيع", null, 1);
        category.SetCompanionLimitExempt(true);

        var entry = context.AgeCategories.Add(category);

        using var doc = System.Text.Json.JsonDocument.Parse(OutboxEntityExtractor.SerializePayload(entry));
        doc.RootElement.GetProperty("IsCompanionLimitExempt").GetBoolean().ShouldBeTrue();
    }
}
