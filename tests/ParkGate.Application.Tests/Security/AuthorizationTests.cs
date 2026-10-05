using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Tests.Security;

public sealed class AuthorizationTests
{
    [Fact]
    public void Operator_ShouldHaveDeskPermissionsOnly()
    {
        var role = UserRole.Operator;

        RolePermissions.HasPermission(role, Permission.CheckIn).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.ViewTodayDeskEntries).ShouldBeTrue();

        RolePermissions.HasPermission(role, Permission.EditBeneficiary).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ChangeBeneficiaryStatus).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.OverrideDenial).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.VoidEntry).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ViewReports).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageOrganizations).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageAgeCategories).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageSettings).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageUsers).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageWorkstations).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.TriggerBackup).ShouldBeFalse();
    }

    [Fact]
    public void Supervisor_ShouldHaveDeskAndSupervisorPermissions_ButNotAdmin()
    {
        var role = UserRole.Supervisor;

        RolePermissions.HasPermission(role, Permission.CheckIn).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.ViewTodayDeskEntries).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.EditBeneficiary).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.ChangeBeneficiaryStatus).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.OverrideDenial).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.VoidEntry).ShouldBeTrue();
        RolePermissions.HasPermission(role, Permission.ViewReports).ShouldBeTrue();

        RolePermissions.HasPermission(role, Permission.ManageOrganizations).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageAgeCategories).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageSettings).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageUsers).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.ManageWorkstations).ShouldBeFalse();
        RolePermissions.HasPermission(role, Permission.TriggerBackup).ShouldBeFalse();
    }

    [Fact]
    public void Admin_ShouldHaveAllPermissions()
    {
        var role = UserRole.Admin;

        foreach (Permission permission in Enum.GetValues<Permission>())
        {
            RolePermissions.HasPermission(role, permission).ShouldBeTrue($"Admin should have {permission}");
        }
    }

    [Fact]
    public void CurrentUserContext_WhenUnauthenticated_ShouldHaveNoPermissions()
    {
        var context = new CurrentUserContext();

        context.IsAuthenticated.ShouldBeFalse();
        context.UserId.ShouldBeNull();
        context.UserName.ShouldBeNull();
        context.Role.ShouldBeNull();
        context.Has(Permission.CheckIn).ShouldBeFalse();
        context.HasAny(Permission.CheckIn, Permission.ManageUsers).ShouldBeFalse();
        context.IsInRole(UserRole.Operator).ShouldBeFalse();
    }

    [Fact]
    public void CurrentUserContext_WhenSet_ShouldReflectUserAndPermissions()
    {
        var context = new CurrentUserContext();
        var user = new User("ahmed", "Ahmed Ali", "hashed", UserRole.Supervisor);

        context.SetUser(user);

        context.IsAuthenticated.ShouldBeTrue();
        context.UserName.ShouldBe("ahmed");
        context.FullName.ShouldBe("Ahmed Ali");
        context.Role.ShouldBe(UserRole.Supervisor);
        context.IsInRole(UserRole.Supervisor).ShouldBeTrue();
        context.Has(Permission.OverrideDenial).ShouldBeTrue();
        context.Has(Permission.ManageUsers).ShouldBeFalse();

        context.Clear();
        context.IsAuthenticated.ShouldBeFalse();
        context.UserName.ShouldBeNull();
    }

    [Fact]
    public void CurrentWorkstationContext_ShouldNormalizeAndTrackRegistration()
    {
        var context = new CurrentWorkstationContext("gate-pc-01");

        context.MachineName.ShouldBe("GATE-PC-01");
        context.IsRegistered.ShouldBeFalse();
        context.WorkstationId.ShouldBeNull();

        var workstation = new Workstation("gate-pc-01", "Gate Desk 1", WorkstationType.Desk);
        context.SetWorkstation(workstation);

        context.DisplayName.ShouldBe("Gate Desk 1");
        context.Type.ShouldBe(WorkstationType.Desk);
    }
}
