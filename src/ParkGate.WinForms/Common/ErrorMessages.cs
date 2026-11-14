using ParkGate.Application.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Single point of mapping application error codes and validation errors to localized Arabic messages (AGENTS.md §4.4).
/// </summary>
public static class ErrorMessages
{
    public static string GetMessage(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        // 1. If it's a validation error list, format bulleted validation messages
        if (error.ValidationErrors != null && error.ValidationErrors.Count > 0)
        {
            var lines = error.ValidationErrors.Select(v =>
            {
                var valKey = "Val_" + v.Code;
                var localized = Strings.GetOptionalString(valKey);
                return !string.IsNullOrWhiteSpace(localized) ? localized : v.Message;
            });

            return string.Join(Environment.NewLine, lines);
        }

        // 2. Try looking up resource key "Error_" + sanitized error code
        var sanitizedCode = error.Code.Replace('.', '_');
        var resourceKey = "Error_" + sanitizedCode;
        var directMatch = Strings.GetOptionalString(resourceKey);
        if (!string.IsNullOrWhiteSpace(directMatch))
        {
            return directMatch;
        }

        // 3. Known fallbacks
        return error.Code switch
        {
            "General.NotFound" => Strings.Error_General_NotFound,
            "General.Unauthorized" => Strings.Error_General_Unauthorized,
            "General.Forbidden" => Strings.Error_General_Forbidden,
            "General.ConcurrencyConflict" => Strings.Error_General_ConcurrencyConflict,
            "Auth.InvalidCredentials" => Strings.Error_Auth_InvalidCredentials,
            "Auth.AccountLocked" => Strings.Error_Auth_AccountLocked,
            "Auth.AccountInactive" => Strings.Error_Auth_AccountInactive,
            "Auth.MustChangePassword" => Strings.Error_Auth_MustChangePassword,
            "Auth.PasswordMismatch" => Strings.Error_Auth_PasswordMismatch,
            "Auth.FirstRunAlreadyCompleted" => Strings.Error_Auth_FirstRunAlreadyCompleted,
            "Auth.WorkstationNotFound" => Strings.Error_Auth_WorkstationNotFound,
            "Auth.WorkstationInactive" => Strings.Error_Auth_WorkstationInactive,
            "Auth.SupervisorRequired" => Strings.Error_Auth_SupervisorRequired,

            // CheckIn
            "CheckIn.CooldownActive" => Strings.GetString("Error_CheckIn_CooldownActive"),
            "CheckIn.OrganizationInactive" => Strings.GetString("Error_CheckIn_OrganizationInactive"),
            "CheckIn.ExceedsMaxGroupSize" => Strings.GetString("Error_CheckIn_ExceedsMaxGroupSize"),
            "CheckIn.NegativeCompanions" => Strings.GetString("Error_CheckIn_NegativeCompanions"),
            "CheckIn.OverrideReasonRequired" => Strings.GetString("Error_CheckIn_OverrideReasonRequired"),
            "CheckIn.OverrideNotAllowed" => Strings.GetString("Error_CheckIn_OverrideNotAllowed"),
            "CheckIn.AttemptNotFound" => Strings.GetString("Error_CheckIn_AttemptNotFound"),
            "CheckIn.CannotVoidDeniedAttempt" => Strings.GetString("Error_CheckIn_CannotVoidDeniedAttempt"),
            "CheckIn.AlreadyVoided" => Strings.GetString("Error_CheckIn_AlreadyVoided"),
            "CheckIn.VoidReasonRequired" => Strings.GetString("Error_CheckIn_VoidReasonRequired"),
            "CheckIn.ExceedsFreeCompanionLimit" => Strings.GetString("Error_CheckIn_ExceedsFreeCompanionLimit"),

            // Gift
            "Gift.OrganizationInactive" => Strings.GetString("Error_Gift_OrganizationInactive"),
            "Gift.NotEnabled" => Strings.GetString("Error_Gift_NotEnabled"),
            "Gift.HolderBlocked" => Strings.GetString("Error_Gift_HolderBlocked"),
            "Gift.HolderSuspended" => Strings.GetString("Error_Gift_HolderSuspended"),
            "Gift.IntervalNotElapsed" => Strings.GetString("Error_Gift_IntervalNotElapsed"),
            "Gift.OverrideReasonRequired" => Strings.GetString("Error_Gift_OverrideReasonRequired"),
            "Gift.OverrideNotAllowed" => Strings.GetString("Error_Gift_OverrideNotAllowed"),
            "Gift.NotFound" => Strings.GetString("Error_Gift_NotFound"),
            "Gift.AlreadyRedeemed" => Strings.GetString("Error_Gift_AlreadyRedeemed"),
            "Gift.Voided" => Strings.GetString("Error_Gift_Voided"),
            "Gift.Expired" => Strings.GetString("Error_Gift_Expired"),
            "Gift.InvalidCode" => Strings.GetString("Error_Gift_InvalidCode"),
            "Gift.Forged" => Strings.GetString("Error_Gift_Forged"),
            "Gift.CannotVoidRedeemed" => Strings.GetString("Error_Gift_CannotVoidRedeemed"),
            "Gift.VoidReasonRequired" => Strings.GetString("Error_Gift_VoidReasonRequired"),
            "Gift.InvalidPolicy" => Strings.GetString("Error_Gift_InvalidPolicy"),

            // Beneficiary
            "Beneficiary.NotFound" => Strings.GetString("Error_Beneficiary_NotFound"),
            "Beneficiary.CardNotFound" => Strings.GetString("Error_Beneficiary_CardNotFound"),
            "Beneficiary.CardAlreadyExists" => Strings.GetString("Error_Beneficiary_CardAlreadyExists"),
            "Beneficiary.NationalIdAlreadyExists" => Strings.GetString("Error_Beneficiary_NationalIdAlreadyExists"),
            "Beneficiary.MultipleHoldersFound" => Strings.GetString("Error_Beneficiary_MultipleHoldersFound"),
            "Beneficiary.Blocked" => Strings.GetString("Error_Beneficiary_Blocked"),
            "Beneficiary.Suspended" => Strings.GetString("Error_Beneficiary_Suspended"),
            "Beneficiary.CardExpired" => Strings.GetString("Error_Beneficiary_CardExpired"),
            "Beneficiary.InvalidStatusTransition" => Strings.GetString("Error_Beneficiary_InvalidStatusTransition"),

            // Organization
            "Organization.NotFound" => Strings.GetString("Error_Organization_NotFound"),
            "Organization.CodeAlreadyExists" => Strings.GetString("Error_Organization_CodeAlreadyExists"),
            "Organization.CodeAlreadyExistsInactive" => Strings.GetString("Error_Organization_CodeAlreadyExistsInactive"),
            "Organization.NameAlreadyExists" => Strings.GetString("Error_Organization_NameAlreadyExists"),
            "Organization.NameAlreadyExistsInactive" => Strings.GetString("Error_Organization_NameAlreadyExistsInactive"),
            "Organization.CannotDeactivateDefault" => Strings.GetString("Error_Organization_CannotDeactivateDefault"),

            // AgeCategory
            "AgeCategory.NotFound" => Strings.GetString("Error_AgeCategory_NotFound"),
            "AgeCategory.NameAlreadyExists" => Strings.GetString("Error_AgeCategory_NameAlreadyExists"),
            "AgeCategory.NameAlreadyExistsInactive" => Strings.GetString("Error_AgeCategory_NameAlreadyExistsInactive"),

            // User
            "User.NotFound" => Strings.GetString("Error_User_NotFound"),
            "User.UsernameAlreadyExists" => Strings.GetString("Error_User_UsernameAlreadyExists"),
            "User.UsernameAlreadyExistsInactive" => Strings.GetString("Error_User_UsernameAlreadyExistsInactive"),
            "User.CannotDeactivateSelf" => Strings.GetString("Error_User_CannotDeactivateSelf"),
            "User.CannotChangeOwnRole" => Strings.GetString("Error_User_CannotChangeOwnRole"),

            // Workstation
            "Workstation.NotFound" => Strings.GetString("Error_Workstation_NotFound"),
            "Workstation.MachineNameAlreadyExists" => Strings.GetString("Error_Workstation_MachineNameAlreadyExists"),

            // Settings
            "Settings.KeyNotFound" => Strings.GetString("Error_Settings_KeyNotFound"),
            "Settings.InvalidValue" => Strings.GetString("Error_Settings_InvalidValue"),

            // Reports
            "Reports.InvalidDateRange" => Strings.GetString("Error_Reports_InvalidDateRange"),

            _ => !string.IsNullOrWhiteSpace(error.Message) ? error.Message : Strings.Msg_UnexpectedError
        };
    }
}
