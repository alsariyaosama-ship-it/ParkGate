using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class MappingRoundTripTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public MappingRoundTripTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AllEntities_ShouldRoundTripThroughDatabaseAccurately()
    {
        await using var context = _fixture.CreateDbContext();

        // 1. Create Organization
        var org = new Organization("TESTORG", "جهة الاختبار", cooldownMinutes: 120, isDefault: false, isActive: true);
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        org.Id.ShouldBeGreaterThan(0);

        // 2. Create User
        var user = new User(
            userName: "operator1",
            fullName: "مشغل البوابة الأول",
            passwordHash: "PBKDF2$SHA256$600000$c2FsdA==$aGFzaA==",
            role: UserRole.Operator,
            mustChangePassword: false,
            isActive: true);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        user.Id.ShouldBeGreaterThan(0);

        // 3. Create Workstation
        var workstation = new Workstation("GATE-PC-01", "شباك 1", WorkstationType.Desk, isActive: true);
        context.Workstations.Add(workstation);
        await context.SaveChangesAsync();
        workstation.Id.ShouldBeGreaterThan(0);

        // 4. Create Beneficiary
        var beneficiaryId = Guid.NewGuid();
        var cardNumber = CardNumber.Create("CARD-12345");
        var phone = PhoneNumber.Create("0912345678");
        var nationalId = NationalId.Create("119900123456");
        var expiryDate = new DateOnly(2027, 12, 31);

        var beneficiary = new Beneficiary(
            id: beneficiaryId,
            organizationId: org.Id,
            cardNumber: cardNumber,
            fullName: "علي محمد سالم",
            phone: phone,
            nationalId: nationalId,
            cardExpiryDate: expiryDate,
            notes: "ملاحظة اختبار");
        context.Beneficiaries.Add(beneficiary);
        await context.SaveChangesAsync();

        // 5. Create AgeCategory
        var category = new AgeCategory("مرافق بالغ", "أكثر من 18 سنة", 10, isActive: true);
        context.AgeCategories.Add(category);
        await context.SaveChangesAsync();
        category.Id.ShouldBeGreaterThan(0);

        // 6. Create EntryAttempt with Companion
        var attemptId = Guid.NewGuid();
        var attemptedAtUtc = new DateTime(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);
        var businessDate = new DateOnly(2026, 10, 5);
        var companions = new List<EntryAttemptCompanion>
        {
            new(category.Id, count: 2)
        };

        var attempt = EntryAttempt.CreateAllowed(
            id: attemptId,
            beneficiaryId: beneficiaryId,
            organizationId: org.Id,
            attemptedAtUtc: attemptedAtUtc,
            businessDate: businessDate,
            cooldownMinutesApplied: 120,
            nextEligibleAtUtc: attemptedAtUtc.AddMinutes(120),
            workstationId: workstation.Id,
            userId: user.Id,
            channel: EntryChannel.Desk,
            companions: companions);
        context.EntryAttempts.Add(attempt);
        await context.SaveChangesAsync();

        // 7. Read back in a fresh DbContext and verify
        await using var readContext = _fixture.CreateDbContext();

        var loadedBeneficiary = await readContext.Beneficiaries
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == beneficiaryId);

        loadedBeneficiary.ShouldNotBeNull();
        loadedBeneficiary.CardNumber.ShouldBe(cardNumber);
        loadedBeneficiary.Phone.ShouldBe(phone);
        loadedBeneficiary.NationalId.ShouldBe(nationalId);
        loadedBeneficiary.CardExpiryDate.ShouldBe(expiryDate);
        loadedBeneficiary.FullName.ShouldBe("علي محمد سالم");
        loadedBeneficiary.Status.ShouldBe(BeneficiaryStatus.Active);
        loadedBeneficiary.Notes.ShouldBe("ملاحظة اختبار");
        loadedBeneficiary.RowVersion.Length.ShouldBeGreaterThan(0);

        var loadedAttempt = await readContext.EntryAttempts
            .Include(e => e.Companions)
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == attemptId);

        loadedAttempt.ShouldNotBeNull();
        loadedAttempt.BeneficiaryId.ShouldBe(beneficiaryId);
        loadedAttempt.Result.ShouldBe(EntryResult.Allowed);
        loadedAttempt.TotalPersons.ShouldBe(3); // 1 holder + 2 companions (BR-08)
        loadedAttempt.BusinessDate.ShouldBe(businessDate);
        loadedAttempt.AttemptedAtUtc.ShouldBe(attemptedAtUtc);
        loadedAttempt.Companions.Count.ShouldBe(1);
        loadedAttempt.Companions.First().Count.ShouldBe(2);
        loadedAttempt.Companions.First().AgeCategoryId.ShouldBe(category.Id);
    }
}
