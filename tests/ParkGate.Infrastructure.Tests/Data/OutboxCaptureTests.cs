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
        json.ShouldContain("\"Phone\":\"0912345678\"");
        json.ShouldContain("\"NationalId\":\"119901234567\"");
        json.ShouldContain("\"CardExpiryDate\":\"2027-12-31\"");
    }
}
