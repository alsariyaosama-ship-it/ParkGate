namespace ParkGate.Application.Audit;

/// <summary>Which audit actions belong to which <see cref="AuditCategory"/> (R5.20).</summary>
public static class AuditActions
{
    private static readonly IReadOnlyDictionary<AuditCategory, string[]> ByCategory = new Dictionary<AuditCategory, string[]>
    {
        [AuditCategory.Entries] = ["EntryGranted", "EntryOverridden", "EntryVoided"],
        [AuditCategory.Gifts] = ["Gift.Issued", "Gift.IssuedOverride", "Gift.Redeemed", "Gift.Voided"],
        [AuditCategory.Visitors] =
        [
            "BeneficiaryRegistered", "BeneficiaryUpdated", "BeneficiaryStatusChanged",
            "CardPatternAlert.Raised", "CardPatternAlert.Reviewed",
        ],
        [AuditCategory.Users] =
        [
            "UserLogin", "UserLoginFailed", "UserLockedOut", "UserCreated", "UserDeactivated", "UserReactivated",
            "UserRoleChanged", "UserPasswordReset", "PasswordChanged", "FirstRunAdminCreated",
        ],
        [AuditCategory.Setup] =
        [
            "OrganizationCreated", "OrganizationUpdated", "OrganizationDeactivated", "OrganizationReactivated",
            "Organization.PolicyChanged", "CardPattern.Proposed", "CardPattern.Approved", "CardPattern.Rejected",
            "AgeCategoryCreated", "AgeCategoryUpdated", "AgeCategoryDeactivated",
            "AgeCategoryReactivated", "WorkstationAutoRegistered", "WorkstationUpdated", "WorkstationDeactivated",
            "WorkstationReactivated", "SettingsUpdated",
        ],
    };

    /// <summary>The actions of <paramref name="category"/>; null for <see cref="AuditCategory.All"/>.</summary>
    public static IReadOnlyCollection<string>? Of(AuditCategory category) =>
        ByCategory.TryGetValue(category, out var actions) ? actions : null;
}
