namespace ParkGate.Application.Common;

/// <summary>
/// Static catalog of typed application errors organized by functional area.
/// </summary>
public static class Errors
{
    public static class General
    {
        public static readonly Error NotFound = new("General.NotFound", "The requested entity was not found.");
        public static readonly Error Unauthorized = new("General.Unauthorized", "User is not authenticated.");
        public static readonly Error Forbidden = new("General.Forbidden", "User does not have permission to perform this action.");
        public static readonly Error ConcurrencyConflict = new("General.ConcurrencyConflict", "The record was modified by another operation.");

        public static Error ValidationFailed(IReadOnlyList<ValidationError> errors) =>
            new("General.ValidationFailed", "One or more validation errors occurred.", errors);
    }

    public static class Auth
    {
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid username or password.");
        public static readonly Error AccountLocked = new("Auth.AccountLocked", "Account is locked due to multiple failed login attempts.");
        public static readonly Error AccountInactive = new("Auth.AccountInactive", "User account is deactivated.");
        public static readonly Error MustChangePassword = new("Auth.MustChangePassword", "User must change password before continuing.");
        public static readonly Error PasswordMismatch = new("Auth.PasswordMismatch", "The current password provided is incorrect.");
        public static readonly Error FirstRunAlreadyCompleted = new("Auth.FirstRunAlreadyCompleted", "First-run setup has already been completed.");
        public static readonly Error WorkstationNotFound = new("Auth.WorkstationNotFound", "Workstation is not registered.");
        public static readonly Error WorkstationInactive = new("Auth.WorkstationInactive", "Workstation is inactive.");
        public static readonly Error SupervisorRequired = new("Auth.SupervisorRequired", "Supervisor or Admin authorization is required.");
    }

    public static class Beneficiary
    {
        public static readonly Error NotFound = new("Beneficiary.NotFound", "Card holder not found.");
        public static readonly Error CardNotFound = new("Beneficiary.CardNotFound", "Card number not found.");
        public static readonly Error CardAlreadyExists = new("Beneficiary.CardAlreadyExists", "Card number already exists for this organization.");
        public static readonly Error NationalIdAlreadyExists = new("Beneficiary.NationalIdAlreadyExists", "National ID already belongs to another holder.");
        public static readonly Error MultipleHoldersFound = new("Beneficiary.MultipleHoldersFound", "Card number found in multiple organizations.");
        public static readonly Error Blocked = new("Beneficiary.Blocked", "Holder is blocked.");
        public static readonly Error Suspended = new("Beneficiary.Suspended", "Holder is suspended.");
        public static readonly Error CardExpired = new("Beneficiary.CardExpired", "Card has expired.");
        public static readonly Error InvalidStatusTransition = new("Beneficiary.InvalidStatusTransition", "Requested status transition is invalid.");
    }

    public static class CheckIn
    {
        public static readonly Error CooldownActive = new("CheckIn.CooldownActive", "Entry cooldown period is still active.");
        public static readonly Error OrganizationInactive = new("CheckIn.OrganizationInactive", "Organization is inactive.");
        public static readonly Error ExceedsMaxGroupSize = new("CheckIn.ExceedsMaxGroupSize", "Total group size exceeds the maximum allowed limit.");
        public static readonly Error NegativeCompanions = new("CheckIn.NegativeCompanions", "Companion counts cannot be negative.");
        public static readonly Error OverrideReasonRequired = new("CheckIn.OverrideReasonRequired", "A reason of at least 5 characters is required for an override.");
        public static readonly Error OverrideNotAllowed = new("CheckIn.OverrideNotAllowed", "Override is not permitted for blocked holders or inactive organizations.");
        public static readonly Error AttemptNotFound = new("CheckIn.AttemptNotFound", "Entry attempt not found.");
        public static readonly Error CannotVoidDeniedAttempt = new("CheckIn.CannotVoidDeniedAttempt", "Denied entry attempts cannot be voided.");
        public static readonly Error AlreadyVoided = new("CheckIn.AlreadyVoided", "Entry attempt is already voided.");
        public static readonly Error VoidReasonRequired = new("CheckIn.VoidReasonRequired", "A reason of at least 5 characters is required to void an entry.");
    }

    public static class Organization
    {
        public static readonly Error NotFound = new("Organization.NotFound", "Organization not found.");
        public static readonly Error CodeAlreadyExists = new("Organization.CodeAlreadyExists", "Organization code already exists.");
        public static readonly Error NameAlreadyExists = new("Organization.NameAlreadyExists", "Organization name already exists.");
        public static readonly Error CannotDeactivateDefault = new("Organization.CannotDeactivateDefault", "Default organization cannot be deactivated.");
    }

    public static class AgeCategory
    {
        public static readonly Error NotFound = new("AgeCategory.NotFound", "Age category not found.");
        public static readonly Error NameAlreadyExists = new("AgeCategory.NameAlreadyExists", "Age category name already exists.");
    }

    public static class User
    {
        public static readonly Error NotFound = new("User.NotFound", "User not found.");
        public static readonly Error UsernameAlreadyExists = new("User.UsernameAlreadyExists", "Username is already taken.");
        public static readonly Error CannotDeactivateSelf = new("User.CannotDeactivateSelf", "You cannot deactivate your own account.");
        public static readonly Error CannotChangeOwnRole = new("User.CannotChangeOwnRole", "You cannot change your own role.");
    }

    public static class Workstation
    {
        public static readonly Error NotFound = new("Workstation.NotFound", "Workstation not found.");
        public static readonly Error MachineNameAlreadyExists = new("Workstation.MachineNameAlreadyExists", "Machine name is already registered.");
    }

    public static class Settings
    {
        public static readonly Error KeyNotFound = new("Settings.KeyNotFound", "Setting key not found.");
        public static readonly Error InvalidValue = new("Settings.InvalidValue", "Setting value is invalid.");
    }

    public static class Reports
    {
        public static readonly Error InvalidDateRange = new("Reports.InvalidDateRange", "Start date must be earlier than or equal to end date.");
    }
}
