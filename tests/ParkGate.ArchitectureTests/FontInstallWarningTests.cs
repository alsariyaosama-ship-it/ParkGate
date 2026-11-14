using ParkGate.WinForms.Common;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R4.10: designer files use new Font("Cairo", …), which only resolves when Cairo is installed for GDI+.
/// When it is not, admins are told once per run; everyone else just gets the log line.
/// </summary>
public sealed class FontInstallWarningTests
{
    [Fact]
    public void ShouldNotify_CairoMissingAndAdmin_OnlyTheFirstTime()
    {
        var warning = new FontInstallWarning(isCairoInstalled: () => false);

        warning.ShouldNotify(isAdmin: true).ShouldBeTrue();
        warning.ShouldNotify(isAdmin: true).ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotify_CairoMissingButNotAdmin_IsFalseAndAdminStillGetsItLater()
    {
        var warning = new FontInstallWarning(isCairoInstalled: () => false);

        warning.ShouldNotify(isAdmin: false).ShouldBeFalse();
        warning.ShouldNotify(isAdmin: true).ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotify_CairoInstalled_IsNeverTrue()
    {
        var warning = new FontInstallWarning(isCairoInstalled: () => true);

        warning.ShouldNotify(isAdmin: true).ShouldBeFalse();
        warning.IsMissing.ShouldBeFalse();
    }
}
