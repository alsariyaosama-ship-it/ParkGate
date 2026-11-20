using System.Reflection;
using System.Windows.Forms;
using NSubstitute;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CardPatterns;
using ParkGate.Application.Common;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>F8, T8.4b: the card-number patterns screen and the desk's amber note.</summary>
public sealed class CardPatternsFormTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc);

    private static CardPatternDto Pattern(int id, CardPatternStatus status, CardPatternSource source, bool active) =>
        new(id, 1, "صندوق التضامن", "DDDDDD", "10", "10####", status, source, active, 12, Now, null, null);

    private static CardPatternAlertDto Alert(long id, CardPatternAlertStatus status) =>
        new(id, 1, "صندوق التضامن", Guid.NewGuid(), "محمد", "AB-1234", "LL-DDDD", Now, "موظف", "الشباك 1", status, null, null, null);

    private static (CardPatternsForm Form, ICardPatternService Service) Create(IReadOnlyList<CardPatternDto> patterns, IReadOnlyList<CardPatternAlertDto> alerts)
    {
        var service = Substitute.For<ICardPatternService>();
        service.GetPatternsAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(patterns));
        service.GetAlertsAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(alerts));
        service.ApprovePatternAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Result.Success(3));
        service.ReviewAlertAsync(Arg.Any<long>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        return (TestUi.Create<CardPatternsForm>(service), service);
    }

    [Fact]
    public async Task Load_ShowsPatternsAlertsAndWhatWaits()
    {
        var (form, _) = Create(
            [Pattern(1, CardPatternStatus.Proposed, CardPatternSource.NewSeries, false), Pattern(2, CardPatternStatus.Approved, CardPatternSource.Dominant, true)],
            [Alert(5, CardPatternAlertStatus.Open)]);
        using (form)
        {
            await form.ReloadAsync();

            form.ShownPatterns.Count.ShouldBe(2);
            form.ShownAlerts.Count.ShouldBe(1);
            Field<Label>(form, "_lblSummary").Text.ShouldBe(string.Format(Strings.CardPattern_Summary, 1, 1));
            var grid = Field<DataGridView>(form, "_gridPatterns");
            grid.Rows[0].Cells[1].Value.ShouldBe(CardPatternsForm.LeftToRight("10####"));
            grid.Rows[0].Cells[3].Value.ShouldBe(Strings.CardPattern_StatusProposed);
        }
    }

    [Fact]
    public void Window_CreatesItsHandle()
    {
        // The real screen failed with "Error creating window handle"; the other tests never create one.
        var (form, _) = Create([Pattern(1, CardPatternStatus.Proposed, CardPatternSource.NewSeries, false)], [Alert(5, CardPatternAlertStatus.Open)]);
        using (form)
        {
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            Should.NotThrow(() => form.Show());
            form.IsHandleCreated.ShouldBeTrue();
            var tabs = Field<TabControl>(form, "_tabs");
            Should.NotThrow(() => tabs.SelectedIndex = 1);
        }
    }

    [Theory]
    [InlineData(CardPatternStatus.Proposed, true, "CardPattern_StatusProposedActive")]
    [InlineData(CardPatternStatus.Proposed, false, "CardPattern_StatusProposed")]
    [InlineData(CardPatternStatus.Approved, true, "CardPattern_StatusApproved")]
    [InlineData(CardPatternStatus.Rejected, false, "CardPattern_StatusRejected")]
    public void StatusText_SaysWhetherAProposalIsInUse(CardPatternStatus status, bool active, string key) =>
        CardPatternsForm.StatusText(Pattern(1, status, CardPatternSource.Dominant, active)).ShouldBe(Strings.GetString(key));

    [Fact]
    public async Task Approve_AsksThenCallsTheService()
    {
        var (form, service) = Create([Pattern(7, CardPatternStatus.Proposed, CardPatternSource.NewSeries, false)], []);
        using (form)
        {
            await form.ReloadAsync();
            string? asked = null;
            form.ConfirmOverride = q => { asked = q; return true; };

            await form.DecideAsync(approve: true);

            asked.ShouldNotBeNull();
            asked.ShouldContain("10####");
            await service.Received(1).ApprovePatternAsync(7, Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Approve_Cancelled_ChangesNothing()
    {
        var (form, service) = Create([Pattern(7, CardPatternStatus.Proposed, CardPatternSource.NewSeries, false)], []);
        using (form)
        {
            await form.ReloadAsync();
            form.ConfirmOverride = _ => false;

            await form.DecideAsync(approve: true);

            await service.DidNotReceive().ApprovePatternAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task DecidedPattern_HasTheButtonsOff()
    {
        var (form, _) = Create([Pattern(7, CardPatternStatus.Approved, CardPatternSource.Dominant, true)], []);
        using (form)
        {
            await form.ReloadAsync();

            Field<Button>(form, "_btnApprove").Enabled.ShouldBeFalse();
            Field<Button>(form, "_btnReject").Enabled.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task ReviewAlert_SendsTheNote()
    {
        var (form, service) = Create([], [Alert(5, CardPatternAlertStatus.Open)]);
        using (form)
        {
            await form.ReloadAsync();
            Field<TextBox>(form, "_txtReviewNote").Text = "بطاقة صحيحة";

            await form.ReviewSelectedAlertAsync();

            await service.Received(1).ReviewAlertAsync(5, "بطاقة صحيحة", Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    public void Desk_HolderWithAnOpenAlert_ShowsTheAmberNoteBesideTheName(bool alerted, int nameSpan)
    {
        using var control = TestUi.Create<CheckInDeskControl>();
        var holder = new BeneficiaryDto(Guid.NewGuid(), 1, "صندوق التضامن", "AB-1234", "محمد أحمد", "0912345678", null, null,
            BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow, HasOpenCardAlert: alerted);

        control.ShowHolderInfo(holder, null);

        control.IsCardAlertShown.ShouldBe(alerted);
        var table = Field<TableLayoutPanel>(control, "_tblHolder");
        table.GetColumnSpan(Field<Label>(control, "_lblHolderName")).ShouldBe(nameSpan);
        Field<Label>(control, "_lblCardAlert").Text.ShouldBe(Strings.Desk_CardPatternAlert);
    }

    [Theory]
    [InlineData("CardPattern.Proposed")]
    [InlineData("CardPattern.Approved")]
    [InlineData("CardPattern.Rejected")]
    [InlineData("CardPatternAlert.Raised")]
    [InlineData("CardPatternAlert.Reviewed")]
    public void AuditScreen_NamesThePatternOperations(string action) =>
        AuditLogForm.ActionName(action).ShouldNotBe(action);

    private static T Field<T>(object owner, string name) =>
        (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(owner)!;
}
