namespace ParkGate.Application.Security;

/// <summary>
/// Fine-grained application permissions matching SPEC §4.
/// </summary>
public enum Permission
{
    // Desk & Operations
    CheckIn,
    ViewTodayDeskEntries,

    // Beneficiaries
    EditBeneficiary,
    ChangeBeneficiaryStatus,

    // Supervisor Actions
    OverrideDenial,
    VoidEntry,

    // Reports
    ViewReports,

    // Administration
    ManageOrganizations,
    ManageAgeCategories,
    ManageSettings,
    ManageUsers,
    ManageWorkstations,
    TriggerBackup,

    // Gift Vouchers
    IssueGift,
    RedeemGift,
    OverrideGiftInterval,
    VoidGift,

    // Audit trail (R5.20)
    ViewAuditLog,

    // Personal data (SPEC §8): without it, national IDs are masked except the last four digits
    ViewFullNationalId
}
