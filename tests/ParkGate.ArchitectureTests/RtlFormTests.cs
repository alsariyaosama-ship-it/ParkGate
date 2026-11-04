using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

public sealed class RtlFormTests
{
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
        using var form = new AgeCategoriesForm();
        form.Font.Name.ShouldBe("Cairo");
    }

    [Fact]
    public void MainForm_ShouldInheritRtlFormBaseAndHaveArabicTitle()
    {
        using var form = new MainForm();

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
