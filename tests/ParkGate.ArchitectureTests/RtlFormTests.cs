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
        form.Font.Name.ShouldBe("Segoe UI");
        form.KeyPreview.ShouldBeTrue();
    }

    [Fact]
    public void MainForm_ShouldInheritRtlFormBaseAndHaveArabicTitle()
    {
        using var form = new MainForm();

        form.ShouldBeAssignableTo<RtlFormBase>();
        form.RightToLeft.ShouldBe(RightToLeft.Yes);
        form.RightToLeftLayout.ShouldBeTrue();
        form.Text.ShouldBe(Strings.App_Title);
        form.Text.ShouldContain("ParkGate");
    }

    [Fact]
    public void Strings_ShouldContainCoreArabicResources()
    {
        Strings.App_Title.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Info.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Warning.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Error.ShouldNotBeNullOrWhiteSpace();
        Strings.Title_Confirm.ShouldNotBeNullOrWhiteSpace();
    }
}
