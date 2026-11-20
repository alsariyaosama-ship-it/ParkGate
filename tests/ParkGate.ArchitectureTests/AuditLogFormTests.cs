using NSubstitute;
using ParkGate.Application.Audit;
using ParkGate.Application.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.20: the audit trail screen.</summary>
public sealed class AuditLogFormTests
{
    [Theory]
    [InlineData("EntryGranted")]
    [InlineData("EntryOverridden")]
    [InlineData("EntryVoided")]
    [InlineData("Gift.Issued")]
    [InlineData("Gift.IssuedOverride")]
    [InlineData("Gift.Redeemed")]
    [InlineData("Gift.Voided")]
    [InlineData("UserLogin")]
    [InlineData("SettingsUpdated")]
    public void EveryTracedOperation_HasAnArabicName(string action)
    {
        var name = AuditLogForm.ActionName(action);

        name.ShouldNotBe(action);
        name.Any(c => c is >= '؀' and <= 'ۿ').ShouldBeTrue(name);
    }

    [Fact]
    public void UnknownAction_IsShownAsItIs()
    {
        AuditLogForm.ActionName("Some.NewThing").ShouldBe("Some.NewThing");
    }

    [Fact]
    public void Details_ShowEveryFieldWithAnArabicLabel_AndResolvedNames()
    {
        var row = new AuditEntryDto(
            1, DateTime.UtcNow, "Gift.Voided", "المشرف سالم", "الشباك 1", "GiftVoucher", "42", "100200", "محمد أحمد",
            "طباعة خطأ", "موظف الشباك", "المشرف سالم",
            """{"CardNumber":"100200","HolderName":"محمد أحمد","OperatorUserId":1,"ApprovedByUserId":2,"ReceiptNo":42,"Value":10,"Reason":"طباعة خطأ","NewField":"x"}""");

        var text = AuditLogForm.FormatDetails(row);

        text.ShouldContain($"{Strings.Audit_Field_ReceiptNo}: 42");
        text.ShouldContain($"{Strings.Audit_Field_Value}: 10");
        text.ShouldContain($"{Strings.Audit_Field_OperatorUserId}: موظف الشباك");
        text.ShouldContain($"{Strings.Audit_Field_ApprovedByUserId}: المشرف سالم");
        text.ShouldContain("NewField: x");
    }

    [Fact]
    public void Details_BusinessDate_IsDayMonthYear()
    {
        var row = new AuditEntryDto(1, DateTime.UtcNow, "EntryGranted", null, null, null, null, null, null, null, null, null, """{"BusinessDate":"2026-11-20"}""");

        AuditLogForm.FormatDetails(row).ShouldEndWith("20/11/2026");
    }

    [Fact]
    public void Details_NotJson_AreShownRaw()
    {
        var row = new AuditEntryDto(1, DateTime.UtcNow, "UserLogin", null, null, null, null, null, null, null, null, null, "old text");

        AuditLogForm.FormatDetails(row).ShouldBe("old text");
    }

    [Fact]
    public async Task Search_ShowsTheRowsAndSaysWhenTheListWasCut()
    {
        var service = Substitute.For<IAuditLogService>();
        service.SearchAsync(Arg.Any<AuditLogSearchRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Success(new AuditLogPage(
            [new AuditEntryDto(1, DateTime.UtcNow, "EntryGranted", "موظف", "الشباك 1", null, null, "100200", "محمد", null, "موظف", null, null)],
            IsTruncated: true)));
        using var form = TestUi.Create<AuditLogForm>(service);

        await form.SearchAsync();

        await service.Received(1).SearchAsync(Arg.Is<AuditLogSearchRequest>(r => r.Category == AuditCategory.All), Arg.Any<CancellationToken>());
        form.ShownRows.Count.ShouldBe(1);
        form.SummaryText.ShouldBe(string.Format(Strings.Audit_SummaryTruncated, 1));
    }

    [Fact]
    public void FormatDetails_EntryDaysChange_ShowsTheDaysBeforeAndAfterByName()
    {
        var row = new AuditEntryDto(1, DateTime.UtcNow, "Organization.PolicyChanged", null, null, "Organization", "1", null, null, null, null, null,
            """{"OldAllowedEntryDays":72,"AllowedEntryDays":9}""");

        var lines = AuditLogForm.FormatDetails(row).Split(Environment.NewLine);

        lines[0].ShouldBe(Strings.GetString("Audit_Field_OldAllowedEntryDays") + ": السبت" + Strings.Text_ListSeparator + "الأربعاء");
        lines[1].ShouldBe(Strings.GetString("Audit_Field_AllowedEntryDays") + ": الأحد" + Strings.Text_ListSeparator + "الأربعاء");
    }
}
