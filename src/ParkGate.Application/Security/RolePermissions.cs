using ParkGate.Domain.Enums;

namespace ParkGate.Application.Security;

/// <summary>
/// Role-to-permission mapping defining the authoritative authorization matrix per SPEC §4.
/// </summary>
public static class RolePermissions
{
    private static readonly Dictionary<UserRole, HashSet<Permission>> Map = new()
    {
        [UserRole.Operator] =
        [
            Permission.CheckIn,
            Permission.ViewTodayDeskEntries
        ],

        [UserRole.Supervisor] =
        [
            Permission.CheckIn,
            Permission.ViewTodayDeskEntries,
            Permission.EditBeneficiary,
            Permission.ChangeBeneficiaryStatus,
            Permission.OverrideDenial,
            Permission.VoidEntry,
            Permission.ViewReports
        ],

        [UserRole.Admin] =
        [
            Permission.CheckIn,
            Permission.ViewTodayDeskEntries,
            Permission.EditBeneficiary,
            Permission.ChangeBeneficiaryStatus,
            Permission.OverrideDenial,
            Permission.VoidEntry,
            Permission.ViewReports,
            Permission.ManageOrganizations,
            Permission.ManageAgeCategories,
            Permission.ManageSettings,
            Permission.ManageUsers,
            Permission.ManageWorkstations,
            Permission.TriggerBackup
        ]
    };

    public static IReadOnlySet<Permission> GetPermissions(UserRole role) =>
        Map.TryGetValue(role, out var permissions) ? permissions : new HashSet<Permission>();

    public static bool HasPermission(UserRole role, Permission permission) =>
        Map.TryGetValue(role, out var permissions) && permissions.Contains(permission);
}
