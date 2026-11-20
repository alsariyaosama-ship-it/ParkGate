using System.Drawing;
using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

public sealed class RtlFormTests
{
    public static TheoryData<Type> Dialogs()
    {
        var data = new TheoryData<Type>();
        foreach (var type in UiCompositionTests.DesignerTypes().Where(t => typeof(Form).IsAssignableFrom(t) && t != typeof(MainForm)))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Dialogs))]
    public void Dialog_SetsAcceptAndCancelButtons(Type dialogType)
    {
        // R7.4: Enter confirms and Esc closes in every dialog (AGENTS.md §5.11).
        // Management lists have no single confirm action: Enter stays with the grid, Esc still closes.
        Type[] lists = [typeof(OrganizationsForm), typeof(AgeCategoriesForm), typeof(UsersForm), typeof(WorkstationsForm), typeof(CardPatternsForm)];
        using var dialog = (Form)TestUi.Create(dialogType);
        if (dialogType == typeof(RegisterVisitorDialog))
        {
            // A host: its buttons are the hosted panel's (see RegisterVisitorDialogTests).
            return;
        }

        if (!lists.Contains(dialogType))
        {
            dialog.AcceptButton.ShouldNotBeNull($"{dialogType.Name}.AcceptButton");
        }

        dialog.CancelButton.ShouldNotBeNull($"{dialogType.Name}.CancelButton");
    }

    [Theory]
    [MemberData(nameof(Dialogs))]
    public void Dialog_HasExplicitTabOrder(Type dialogType)
    {
        // R7.4: no two focusable siblings share a TabIndex, so the order is the one the designer states.
        using var dialog = (Form)TestUi.Create(dialogType);
        var clashes = AllControls(dialog)
            .Where(c => c.Controls.Count > 1)
            .SelectMany(parent => parent.Controls.Cast<Control>()
                .Where(c => c.TabStop || c.Controls.Count > 0)
                .GroupBy(c => c.TabIndex)
                .Where(g => g.Count() > 1)
                .Select(g => $"{parent.Name}: {string.Join(", ", g.Select(c => c.Name))} share {g.Key}"))
            .ToList();

        clashes.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Dialogs))]
    public void Dialog_ButtonsUseThemeStyles(Type dialogType)
    {
        // R7.4: Theme.ApplyPrimaryButtonStyle / ApplySecondaryButtonStyle (or the danger/warning variant of primary).
        Color[] themed = [Theme.Primary, Theme.SurfaceMuted, Theme.Danger, Theme.Warning];
        using var dialog = (Form)TestUi.Create(dialogType);
        var offenders = AllControls(dialog).OfType<Button>()
            .Where(b => b.FlatStyle != FlatStyle.Flat || !themed.Any(c => c.ToArgb() == b.BackColor.ToArgb()))
            .Select(b => $"{b.Name}: {b.FlatStyle} {b.BackColor}")
            .ToList();

        offenders.ShouldBeEmpty();
    }

    private static IEnumerable<Control> AllControls(Control root) =>
        root.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(AllControls(c)));

    [Fact]
    public void RtlFormBase_ShouldHaveCorrectRtlProperties()
    {
        using var form = new RtlFormBase();

        form.RightToLeft.ShouldBe(RightToLeft.Yes);
        form.RightToLeftLayout.ShouldBeTrue();
        form.AutoScaleMode.ShouldBe(AutoScaleMode.Dpi);
        form.Font.Name.ShouldBe("Cairo");
        form.KeyPreview.ShouldBeTrue();
    }

    [Fact]
    public void AppFont_CreateFont_ShouldReturnCairoFont()
    {
        using var font = AppFont.CreateFont(10F);
        font.Name.ShouldBe("Cairo");
    }

    [Fact]
    public void AgeCategoriesForm_ShouldHaveCairoFont()
    {
        using var form = TestUi.Create<AgeCategoriesForm>();
        form.Font.Name.ShouldBe("Cairo");
    }

    [Fact]
    public void MainForm_ShouldInheritRtlFormBaseAndHaveArabicTitle()
    {
        using var form = TestUi.Create<MainForm>();

        form.ShouldBeAssignableTo<RtlFormBase>();
        form.RightToLeft.ShouldBe(RightToLeft.Yes);
        form.RightToLeftLayout.ShouldBeTrue();
        form.Text.ShouldBe(Strings.App_Title);
        form.Text.ShouldContain("منتزه المرح العائلي");
    }

    [Theory]
    [InlineData(typeof(MainForm))]
    [InlineData(typeof(ConnectionSettingsForm))]
    [InlineData(typeof(FirstRunAdminForm))]
    [InlineData(typeof(LoginForm))]
    [InlineData(typeof(ChangePasswordForm))]
    [InlineData(typeof(SupervisorAuthDialog))]
    [InlineData(typeof(SelectOrganizationDialog))]
    public void AllForms_ShouldInheritRtlFormBase(Type formType)
    {
        typeof(RtlFormBase).IsAssignableFrom(formType).ShouldBeTrue($"Form '{formType.Name}' must inherit from RtlFormBase.");
    }

    [Fact]
    public void Strings_ShouldContainCoreArabicResources()
    {
        Strings.App_Title.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Info.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Warning.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Error.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Confirm.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_ConnectionSettings.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_FirstRunAdmin.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Login.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_ChangePassword.ShouldNotBeNullOrWhiteSpace();
    }
}
