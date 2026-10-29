using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class GiftVoucherTests
{
    private readonly DateTime _nowUtc = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);
    private readonly DateOnly _businessDate = new(2026, 10, 29);

    [Fact]
    public void Issue_ValidParameters_SetsExpectedProperties()
    {
        var token = Guid.NewGuid();
        var benId = Guid.NewGuid();

        var voucher = GiftVoucher.Issue(
            token: token,
            keyId: 1,
            beneficiaryId: benId,
            organizationId: 2,
            value: 25.5m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 10,
            issuedWorkstationId: 5);

        voucher.Token.ShouldBe(token);
        voucher.KeyId.ShouldBe((byte)1);
        voucher.BeneficiaryId.ShouldBe(benId);
        voucher.OrganizationId.ShouldBe(2);
        voucher.Value.ShouldBe(25.5m);
        voucher.IssuedAtUtc.ShouldBe(_nowUtc);
        voucher.BusinessDate.ShouldBe(_businessDate);
        voucher.ExpiresOn.ShouldBe(_businessDate);
        voucher.Status.ShouldBe(GiftVoucherStatus.Issued);
        voucher.IssuedByUserId.ShouldBe(10);
        voucher.IssuedWorkstationId.ShouldBe(5);
        voucher.IsOverride.ShouldBeFalse();
        voucher.OverrideByUserId.ShouldBeNull();
        voucher.OverrideReason.ShouldBeNull();
    }

    [Fact]
    public void Issue_WithOverride_SetsOverrideFields()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1,
            overrideByUserId: 2,
            overrideReason: "Valid reason override");

        voucher.IsOverride.ShouldBeTrue();
        voucher.OverrideByUserId.ShouldBe(2);
        voucher.OverrideReason.ShouldBe("Valid reason override");
    }

    [Fact]
    public void Issue_OverrideUserWithoutReason_Throws()
    {
        Should.Throw<ArgumentException>(() => GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1,
            overrideByUserId: 2,
            overrideReason: null));
    }

    [Fact]
    public void Issue_ReasonWithoutOverrideUser_Throws()
    {
        Should.Throw<ArgumentException>(() => GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1,
            overrideByUserId: null,
            overrideReason: "Reason without supervisor"));
    }

    [Fact]
    public void Issue_EmptyToken_Throws()
    {
        Should.Throw<ArgumentException>(() => GiftVoucher.Issue(
            token: Guid.Empty,
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1));
    }

    [Fact]
    public void Issue_ZeroValue_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 0m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1));
    }

    [Fact]
    public void Issue_ShortOverrideReason_Throws()
    {
        Should.Throw<ArgumentException>(() => GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1,
            overrideByUserId: 2,
            overrideReason: "four"));
    }

    [Fact]
    public void Redeem_FromIssued_SetsRedeemed()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.Redeem(userId: 3, workstationId: 4, nowUtc: _nowUtc.AddMinutes(5), businessDate: _businessDate);

        voucher.Status.ShouldBe(GiftVoucherStatus.Redeemed);
        voucher.RedeemedByUserId.ShouldBe(3);
        voucher.RedeemedWorkstationId.ShouldBe(4);
        voucher.RedeemedAtUtc.ShouldBe(_nowUtc.AddMinutes(5));
        voucher.RedeemedBusinessDate.ShouldBe(_businessDate);
    }

    [Fact]
    public void Redeem_FromIssued_Twice_Throws()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.Redeem(userId: 3, workstationId: 4, nowUtc: _nowUtc, businessDate: _businessDate);

        Should.Throw<InvalidOperationException>(() =>
            voucher.Redeem(userId: 3, workstationId: 4, nowUtc: _nowUtc, businessDate: _businessDate));
    }

    [Fact]
    public void Void_FromIssued_SetsVoided()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.Void(userId: 5, reason: "Printed by mistake", nowUtc: _nowUtc.AddMinutes(2));

        voucher.Status.ShouldBe(GiftVoucherStatus.Voided);
        voucher.VoidedByUserId.ShouldBe(5);
        voucher.VoidReason.ShouldBe("Printed by mistake");
        voucher.VoidedAtUtc.ShouldBe(_nowUtc.AddMinutes(2));
    }

    [Fact]
    public void Void_FromRedeemed_Throws()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.Redeem(userId: 3, workstationId: 4, nowUtc: _nowUtc, businessDate: _businessDate);

        Should.Throw<InvalidOperationException>(() =>
            voucher.Void(userId: 5, reason: "Attempt void redeemed", nowUtc: _nowUtc));
    }

    [Fact]
    public void Redeem_FromVoided_Throws()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.Void(userId: 5, reason: "Cancelled correctly", nowUtc: _nowUtc);

        Should.Throw<InvalidOperationException>(() =>
            voucher.Redeem(userId: 3, workstationId: 4, nowUtc: _nowUtc, businessDate: _businessDate));
    }

    [Fact]
    public void IsExpired_SameDay_NotExpired()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.IsExpired(_businessDate).ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_NextDay_Expired()
    {
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: Guid.NewGuid(),
            organizationId: 1,
            value: 10m,
            issuedAtUtc: _nowUtc,
            businessDate: _businessDate,
            issuedByUserId: 1,
            issuedWorkstationId: 1);

        voucher.IsExpired(_businessDate.AddDays(1)).ShouldBeTrue();
    }
}

