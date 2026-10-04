using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class EntryAttemptTests
{
    private readonly Guid _attemptId = Guid.NewGuid();
    private readonly Guid _beneficiaryId = Guid.NewGuid();
    private readonly DateTime _nowUtc = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
    private readonly DateOnly _businessDate = new(2026, 10, 4);

    [Fact]
    public void CreateAllowed_WithCompanions_CalculatesTotalPersonsCorrectly()
    {
        var companions = new[]
        {
            new EntryAttemptCompanion(ageCategoryId: 1, count: 2),
            new EntryAttemptCompanion(ageCategoryId: 2, count: 3)
        };

        var attempt = EntryAttempt.CreateAllowed(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(720),
            workstationId: 1,
            userId: 2,
            EntryChannel.Desk,
            companions);

        attempt.Result.ShouldBe(EntryResult.Allowed);
        attempt.DenialReason.ShouldBeNull();
        attempt.Companions.Count.ShouldBe(2);
        // 1 holder + 2 adult + 3 child = 6 persons (BR-08)
        attempt.TotalPersons.ShouldBe(6);
        attempt.IsVoided.ShouldBeFalse();
    }

    [Fact]
    public void CreateDenied_SetsReasonAndZeroPersons()
    {
        var attempt = EntryAttempt.CreateDenied(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            DenialReason.CooldownActive,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(120),
            workstationId: 1,
            userId: 2,
            EntryChannel.Desk);

        attempt.Result.ShouldBe(EntryResult.Denied);
        attempt.DenialReason.ShouldBe(DenialReason.CooldownActive);
        attempt.TotalPersons.ShouldBe(0);
        attempt.IsVoided.ShouldBeFalse();
    }

    [Fact]
    public void CreateOverridden_ValidReason_SetsOverriddenResult()
    {
        var attempt = EntryAttempt.CreateOverridden(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(720),
            workstationId: 1,
            userId: 2,
            overrideByUserId: 5,
            overrideReason: "استثناء خاص من المشرف",
            EntryChannel.Desk);

        attempt.Result.ShouldBe(EntryResult.Overridden);
        attempt.DenialReason.ShouldBeNull();
        attempt.OverrideByUserId.ShouldBe(5);
        attempt.OverrideReason.ShouldBe("استثناء خاص من المشرف");
        attempt.TotalPersons.ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")] // 4 characters (< 5 chars, BR-11)
    public void CreateOverridden_ReasonTooShort_ThrowsArgumentException(string? shortReason)
    {
        Should.Throw<ArgumentException>(() => EntryAttempt.CreateOverridden(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(720),
            workstationId: 1,
            userId: 2,
            overrideByUserId: 5,
            overrideReason: shortReason!,
            EntryChannel.Desk));
    }

    [Fact]
    public void Void_AllowedAttempt_SetsVoidedProperties()
    {
        var attempt = EntryAttempt.CreateAllowed(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(720),
            workstationId: 1,
            userId: 2,
            EntryChannel.Desk);

        var voidTimeUtc = _nowUtc.AddMinutes(10);
        attempt.Void(voidedByUserId: 5, voidReason: "إلغاء بناء على طلب المشرف", voidTimeUtc);

        attempt.IsVoided.ShouldBeTrue();
        attempt.VoidedByUserId.ShouldBe(5);
        attempt.VoidReason.ShouldBe("إلغاء بناء على طلب المشرف");
        attempt.VoidedAtUtc.ShouldBe(voidTimeUtc);
    }

    [Fact]
    public void Void_DeniedAttempt_ThrowsInvalidOperationException()
    {
        var attempt = EntryAttempt.CreateDenied(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            DenialReason.CooldownActive,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(120),
            workstationId: 1,
            userId: 2,
            EntryChannel.Desk);

        Should.Throw<InvalidOperationException>(() =>
            attempt.Void(5, "محاولة إلغاء فاشلة", _nowUtc.AddMinutes(5)));
    }

    [Fact]
    public void Void_AlreadyVoided_ThrowsInvalidOperationException()
    {
        var attempt = EntryAttempt.CreateAllowed(
            _attemptId,
            _beneficiaryId,
            organizationId: 1,
            _nowUtc,
            _businessDate,
            cooldownMinutesApplied: 720,
            nextEligibleAtUtc: _nowUtc.AddMinutes(720),
            workstationId: 1,
            userId: 2,
            EntryChannel.Desk);

        attempt.Void(5, "إلغاء لخطأ الإدخال", _nowUtc.AddMinutes(5));

        Should.Throw<InvalidOperationException>(() =>
            attempt.Void(5, "إلغاء مرة ثانية", _nowUtc.AddMinutes(10)));
    }
}
