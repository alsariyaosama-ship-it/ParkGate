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
}
