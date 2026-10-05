using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class MainFormTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    [Fact]
    public void MainForm_ShouldHaveArabicMenusAndStatusLabels()
    {
        using var form = new MainForm();

        var menuStrip = GetField<MenuStrip>(form, "_mainMenuStrip");
        var statusStrip = GetField<StatusStrip>(form, "_statusStrip");

        menuStrip.RightToLeft.ShouldBe(RightToLeft.Yes);
        statusStrip.RightToLeft.ShouldBe(RightToLeft.Yes);

        var menuFile = GetField<ToolStripMenuItem>(form, "_menuFile");
        var menuOperations = GetField<ToolStripMenuItem>(form, "_menuOperations");
        var menuManagement = GetField<ToolStripMenuItem>(form, "_menuManagement");
        var menuReports = GetField<ToolStripMenuItem>(form, "_menuReports");
        var menuHelp = GetField<ToolStripMenuItem>(form, "_menuHelp");

        menuFile.Text.ShouldBe(Strings.Menu_File);
        menuOperations.Text.ShouldBe(Strings.Menu_Operations);
        menuManagement.Text.ShouldBe(Strings.Menu_Management);
        menuReports.Text.ShouldBe(Strings.Menu_Reports);
        menuHelp.Text.ShouldBe(Strings.Menu_Help);

        var statusConnection = GetField<ToolStripStatusLabel>(form, "_statusConnection");
        statusConnection.Text.ShouldBe(Strings.Status_Connected);
    }

    [Fact]
    public void MainForm_ForOperator_ShouldHideAdminAndReportsMenus()
    {
        var userContext = new CurrentUserContext();
        userContext.SetUser(1, "operator1", "علي أحمد", UserRole.Operator);

        var wsContext = new CurrentWorkstationContext("DESK-01");

        using var form = new MainForm(userContext, userContext, wsContext, null, null, null, null, null, null);

        var menuOperations = GetField<ToolStripMenuItem>(form, "_menuOperations");
        var menuManagement = GetField<ToolStripMenuItem>(form, "_menuManagement");
        var menuReports = GetField<ToolStripMenuItem>(form, "_menuReports");

        menuOperations.Available.ShouldBeTrue();
        menuManagement.Available.ShouldBeFalse();
        menuReports.Available.ShouldBeFalse();
    }

    [Fact]
    public void MainForm_ForSupervisor_ShouldShowBeneficiariesAndReports_ButHideAdminModules()
    {
        var userContext = new CurrentUserContext();
        userContext.SetUser(2, "super1", "خالد محمود", UserRole.Supervisor);

        var wsContext = new CurrentWorkstationContext("DESK-02");

        using var form = new MainForm(userContext, userContext, wsContext, null, null, null, null, null, null);

        var menuOperations = GetField<ToolStripMenuItem>(form, "_menuOperations");
        var menuManagement = GetField<ToolStripMenuItem>(form, "_menuManagement");
        var menuBeneficiaries = GetField<ToolStripMenuItem>(form, "_menuBeneficiaries");
        var menuUsers = GetField<ToolStripMenuItem>(form, "_menuUsers");
        var menuSettings = GetField<ToolStripMenuItem>(form, "_menuSettings");
        var menuReports = GetField<ToolStripMenuItem>(form, "_menuReports");

        menuOperations.Available.ShouldBeTrue();
        menuManagement.Available.ShouldBeTrue();
        menuBeneficiaries.Available.ShouldBeTrue();
        menuUsers.Available.ShouldBeFalse();
        menuSettings.Available.ShouldBeFalse();
        menuReports.Available.ShouldBeTrue();
    }

    [Fact]
    public void MainForm_ForAdmin_ShouldShowAllMenus()
    {
        var userContext = new CurrentUserContext();
        userContext.SetUser(3, "admin", "أسامة علي", UserRole.Admin);

        var wsContext = new CurrentWorkstationContext("ADMIN-PC");

        using var form = new MainForm(userContext, userContext, wsContext, null, null, null, null, null, null);

        var menuOperations = GetField<ToolStripMenuItem>(form, "_menuOperations");
        var menuManagement = GetField<ToolStripMenuItem>(form, "_menuManagement");
        var menuBeneficiaries = GetField<ToolStripMenuItem>(form, "_menuBeneficiaries");
        var menuUsers = GetField<ToolStripMenuItem>(form, "_menuUsers");
        var menuSettings = GetField<ToolStripMenuItem>(form, "_menuSettings");
        var menuReports = GetField<ToolStripMenuItem>(form, "_menuReports");

        menuOperations.Available.ShouldBeTrue();
        menuManagement.Available.ShouldBeTrue();
        menuBeneficiaries.Available.ShouldBeTrue();
        menuUsers.Available.ShouldBeTrue();
        menuSettings.Available.ShouldBeTrue();
        menuReports.Available.ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserRole.Operator, "موظف الشباك")]
    [InlineData(UserRole.Supervisor, "مشرف")]
    [InlineData(UserRole.Admin, "مدير النظام")]
    public void MainForm_StatusStrip_ShouldDisplayCorrectArabicRole(UserRole role, string expectedRoleArabic)
    {
        var userContext = new CurrentUserContext();
        userContext.SetUser(10, "user_test", "طارق فتحي", role);

        var wsContext = new CurrentWorkstationContext("GATE-01");

        using var form = new MainForm(userContext, userContext, wsContext, null, null, null, null, null, null);

        var statusUser = GetField<ToolStripStatusLabel>(form, "_statusUser");
        statusUser.Text.ShouldNotBeNull();
        statusUser.Text.ShouldContain("طارق فتحي");
        statusUser.Text.ShouldContain(expectedRoleArabic);

        var statusWorkstation = GetField<ToolStripStatusLabel>(form, "_statusWorkstation");
        statusWorkstation.Text.ShouldNotBeNull();
        statusWorkstation.Text.ShouldContain("GATE-01");
    }
}
