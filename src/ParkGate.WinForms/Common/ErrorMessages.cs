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
            _ => !string.IsNullOrWhiteSpace(error.Message) ? error.Message : Strings.Msg_UnexpectedError
        };
    }
}
