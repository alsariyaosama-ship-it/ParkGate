using NSubstitute;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.17: a gift voucher can be voided from the gift receipts window.</summary>
public sealed class GiftVoidScreenTests
{
    [Theory]
    [InlineData(GiftVoucherStatus.Issued, true)]
    [InlineData(GiftVoucherStatus.Redeemed, false)]
    [InlineData(GiftVoucherStatus.Voided, false)]
    public void OnlyAnIssuedVoucher_CanBeVoided(GiftVoucherStatus status, bool canVoid)
    {
        GiftReceiptsForm.CanVoid(Row(status)).ShouldBe(canVoid);
        GiftReceiptsForm.CanVoid(null).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("token-1")]
    public async Task Void_SendsTheReceiptReasonAndApproval_ThenReloads(string? token)
    {
        var gifts = Substitute.For<IGiftVoucherService>();
        gifts.VoidAsync(Arg.Any<VoidGiftVoucherRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        var reports = Reports();
        var notify = Substitute.For<INotifyService>();
        notify.Confirm(Arg.Any<string>(), Arg.Any<string?>()).Returns(true);
        using var form = TestUi.Create<GiftReceiptsForm>(gifts, reports, notify);
        form.AskVoidReason = _ => ("printing mistake", token);

        await form.VoidReceiptAsync(Row(GiftVoucherStatus.Issued));

        await gifts.Received(1).VoidAsync(Arg.Is<VoidGiftVoucherRequest>(r => r.VoucherId == 42 && r.Reason == "printing mistake"), token, Arg.Any<CancellationToken>());
        await reports.Received(1).GetReceiptsAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<GiftReceiptFilter>(), Arg.Any<CancellationToken>());
        notify.Received(1).Info(Arg.Is<string>(m => m.Contains("42")), Arg.Any<string?>());
    }

    [Fact]
    public async Task Void_Refused_ShowsTheReason()
    {
        var gifts = Substitute.For<IGiftVoucherService>();
        gifts.VoidAsync(Arg.Any<VoidGiftVoucherRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Result.Failure(Errors.Gift.CannotVoidRedeemed));
        var notify = Substitute.For<INotifyService>();
        notify.Confirm(Arg.Any<string>(), Arg.Any<string?>()).Returns(true);
        using var form = TestUi.Create<GiftReceiptsForm>(gifts, Reports(), notify);
        form.AskVoidReason = _ => ("printing mistake", null);

        await form.VoidReceiptAsync(Row(GiftVoucherStatus.Issued));

        notify.Received(1).Warning(ErrorMessages.GetMessage(Errors.Gift.CannotVoidRedeemed), Arg.Any<string?>());
    }

    [Theory]
    [InlineData(false, true)]   // reason dialog cancelled
    [InlineData(true, false)]   // confirmation answered no
    public async Task Void_CancelledOrNotConfirmed_DoesNothing(bool giveReason, bool confirm)
    {
        var gifts = Substitute.For<IGiftVoucherService>();
        var notify = Substitute.For<INotifyService>();
        notify.Confirm(Arg.Any<string>(), Arg.Any<string?>()).Returns(confirm);
        using var form = TestUi.Create<GiftReceiptsForm>(gifts, Reports(), notify);
        form.AskVoidReason = _ => giveReason ? ("printing mistake", null) : null;

        await form.VoidReceiptAsync(Row(GiftVoucherStatus.Issued));

        await gifts.DidNotReceiveWithAnyArgs().VoidAsync(default!, default, default);
    }

    [Fact]
    public void Form_HasAVoidButton_DisabledWithoutSelection()
    {
        using var form = TestUi.Create<GiftReceiptsForm>();
        var button = (System.Windows.Forms.Button)typeof(GiftReceiptsForm)
            .GetField("_btnVoidGift", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;

        button.Text.ShouldBe(Strings.Btn_VoidGift);
        button.Enabled.ShouldBeFalse();
    }

    private static IGiftReportService Reports()
    {
        var reports = Substitute.For<IGiftReportService>();
        reports.GetReceiptsAsync(default, default).ReturnsForAnyArgs(Result.Success(new GiftReceiptReportDto(
            DateOnly.MinValue, DateOnly.MinValue, null, null, GiftReceiptFilter.All, [], [], [], 0, 0m)));
        return reports;
    }

    private static GiftReceiptRowDto Row(GiftVoucherStatus status) => new(
        42, Guid.NewGuid(), "محمد أحمد", "100200", 1, "صندوق التضامن", 10m, DateTime.UtcNow, "موظف",
        null, null, null, status, false, null);
}
