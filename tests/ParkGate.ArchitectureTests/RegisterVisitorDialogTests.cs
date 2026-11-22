using System.Reflection;
using System.Windows.Forms;
using NSubstitute;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.15: a new visitor is registered in a pop-up, because the compact desk (R5.8) has no room for the fields.</summary>
public sealed class RegisterVisitorDialogTests
{
    [Fact]
    public void Dialog_HostAndRelease_MoveThePanelInAndOut()
    {
        using var dialog = new RegisterVisitorDialog();
        using var panel = new InlineRegistrationPanel();

        dialog.Host(panel);
        panel.Parent.ShouldBe(dialog);
        panel.Dock.ShouldBe(DockStyle.Fill);
        dialog.AcceptButton.ShouldBe(panel.SaveButton);
        dialog.CancelButton.ShouldBe(panel.CancelButtonControl);

        dialog.Release(panel);
        panel.Parent.ShouldBeNull();
        dialog.AcceptButton.ShouldBeNull();
    }

    [Fact]
    public void Desk_NotOnAShownWindow_KeepsTheInlinePanel()
    {
        var factory = DialogFactory(out _);
        using var desk = TestUi.Create<CheckInDeskControl>(factory);
        var shown = 0;
        desk.ShowRegistrationModal = _ => { shown++; return DialogResult.Cancel; };

        desk.ShowInlineRegistration("555");

        shown.ShouldBe(0);
        Panel(desk).Visible.ShouldBeTrue();
    }

    [Fact]
    public void Desk_RegistrationDialog_HostsThePanelWhileOpen()
    {
        var factory = DialogFactory(out var dialog);
        using var desk = TestUi.Create<CheckInDeskControl>(factory);
        var originalParent = Panel(desk).Parent;
        Control? parentWhileOpen = null;
        desk.ShowRegistrationModal = _ => { parentWhileOpen = Panel(desk).Parent; return DialogResult.OK; };
        desk.ShowInlineRegistration("555");

        desk.OpenRegistrationDialog();

        parentWhileOpen.ShouldBe(dialog);
        Panel(desk).Parent.ShouldBe(originalParent);
        dialog.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void Desk_RegistrationDialogCancelled_ReturnsToTheEmptyDesk()
    {
        var factory = DialogFactory(out _);
        using var desk = TestUi.Create<CheckInDeskControl>(factory);
        desk.ShowRegistrationModal = _ => DialogResult.Cancel;
        GetField<TextBox>(desk, "_txtCardNumber").Text = "555";
        desk.ShowInlineRegistration("555");

        desk.OpenRegistrationDialog();

        Panel(desk).Visible.ShouldBeFalse();
        GetField<GroupBox>(desk, "_grpHolder").Visible.ShouldBeTrue();
        GetField<StatusBanner>(desk, "_statusBanner").State.ShouldBe(BannerState.Neutral);
        GetField<TextBox>(desk, "_txtCardNumber").Text.ShouldBe("555");
    }

    [Theory]
    [InlineData(false, true)]  // operator keeps the form
    [InlineData(true, false)]  // operator discards it
    public void Dialog_Closing_AsksBeforeDiscarding(bool discard, bool cancelled)
    {
        using var dialog = new RegisterVisitorDialog { ConfirmDiscard = () => discard };
        var e = new FormClosingEventArgs(CloseReason.UserClosing, cancel: false);

        typeof(RegisterVisitorDialog).GetMethod("OnFormClosing", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(dialog, [e]);

        e.Cancel.ShouldBe(cancelled);
    }

    private static IFormFactory DialogFactory(out RegisterVisitorDialog dialog)
    {
        var created = new RegisterVisitorDialog();
        dialog = created;
        var factory = Substitute.For<IFormFactory>();
        factory.Create<RegisterVisitorDialog>().Returns(created);
        return factory;
    }

    private static InlineRegistrationPanel Panel(CheckInDeskControl desk) => GetField<InlineRegistrationPanel>(desk, "_registrationPanel");

    private static T GetField<T>(object target, string name) where T : class =>
        (target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target) as T)!;

    // ── F9, BR-32: the city row of the registration panel ──

    private static SearchableComboBox CityBox(InlineRegistrationPanel panel) =>
        (SearchableComboBox)typeof(InlineRegistrationPanel).GetField("_cmbRegCity", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(panel)!;

    [Fact]
    public void Panel_TabOrder_PutsTheCityRightAfterThePhone()
    {
        using var panel = new InlineRegistrationPanel();
        int TabOf(string field) => ((Control)typeof(InlineRegistrationPanel).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(panel)!).TabIndex;

        var order = new[] { "_cmbRegOrganization", "_txtRegFullName", "_txtRegPhone", "_cmbRegCity", "_txtRegNationalId", "_dtpRegExpiry", "_txtRegNotes" }
            .Select(TabOf).ToList();

        order.ShouldBe(order.Order().ToList());
        order.Distinct().Count().ShouldBe(order.Count);
    }

    [Fact]
    public void Panel_SelectedCityId_ResolvesTheTypedText()
    {
        using var panel = new InlineRegistrationPanel();
        panel.SetCities([(1, "الخمس"), (2, "زليتن"), (3, "مصراتة")]);
        panel.SelectedCityId.ShouldBeNull();

        CityBox(panel).Text = "مصراته";

        panel.SelectedCityId.ShouldBe(3);
    }

    [Fact]
    public void Panel_SelectedCityId_UnknownText_IsNull()
    {
        using var panel = new InlineRegistrationPanel();
        panel.SetCities([(1, "الخمس")]);
        CityBox(panel).Text = "غير موجودة";

        panel.SelectedCityId.ShouldBeNull();
        CityBox(panel).Text.ShouldBeEmpty();
    }

    [Fact]
    public void Panel_HasInput_CountsTheCity_AndClearFieldsResetsIt()
    {
        using var panel = new InlineRegistrationPanel();
        panel.SetCities([(1, "الخمس")]);
        panel.HasInput.ShouldBeFalse();

        CityBox(panel).Text = "خمس";
        panel.HasInput.ShouldBeTrue();
        panel.SelectedCityId.ShouldBe(1);

        panel.ClearFields();

        panel.HasInput.ShouldBeFalse();
        panel.SelectedCityId.ShouldBeNull();
    }

    [Fact]
    public void Dialog_ClientArea_FitsThePanelWithItsCityRow()
    {
        using var dialog = new RegisterVisitorDialog();
        using var panel = new InlineRegistrationPanel();

        dialog.ClientSize.Height.ShouldBeGreaterThanOrEqualTo(panel.Height + dialog.Padding.Vertical);
        dialog.ClientSize.Width.ShouldBeGreaterThanOrEqualTo(panel.Width + dialog.Padding.Horizontal);
    }
}
