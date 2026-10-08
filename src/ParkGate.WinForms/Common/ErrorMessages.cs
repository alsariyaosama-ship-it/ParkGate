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
            "CheckIn.CooldownActive" => Strings.GetOptionalString("Error_CheckIn_CooldownActive") ?? "فترة التهدئة نشطة — سبق الدخول ضمن الفترة المحددة ولا يسمح بالدخول حالياً.",
            "CheckIn.OrganizationInactive" => Strings.GetOptionalString("Error_CheckIn_OrganizationInactive") ?? "الجهة التابع لها الزبون معطّلة أو غير نشطة حالياً.",
            "CheckIn.ExceedsMaxGroupSize" => Strings.GetOptionalString("Error_CheckIn_ExceedsMaxGroupSize") ?? "إجمالي عدد الأفراد يتجاوز الحد الأقصى المسموح به للمجموعة في المرة الواحدة.",
            "CheckIn.NegativeCompanions" => Strings.GetOptionalString("Error_CheckIn_NegativeCompanions") ?? "أعداد المرافقين لا يمكن أن تكون سالبة.",
            "CheckIn.OverrideReasonRequired" => Strings.GetOptionalString("Error_CheckIn_OverrideReasonRequired") ?? "يرجى كتابة سبب التجاوز (5 أحرف على الأقل).",
            "CheckIn.OverrideNotAllowed" => Strings.GetOptionalString("Error_CheckIn_OverrideNotAllowed") ?? "لا يُسمح بالتجاوز للزبائن المحظورين أو الجهات غير النشطة.",
            "CheckIn.AttemptNotFound" => Strings.GetOptionalString("Error_CheckIn_AttemptNotFound") ?? "لم يتم العثور على محاولة الدخول المطلوبة.",
            "CheckIn.CannotVoidDeniedAttempt" => Strings.GetOptionalString("Error_CheckIn_CannotVoidDeniedAttempt") ?? "لا يمكن إلغاء محاولة دخول تم رفضها مسبقاً.",
            "CheckIn.AlreadyVoided" => Strings.GetOptionalString("Error_CheckIn_AlreadyVoided") ?? "تم إلغاء محاولة الدخول هذه مسبقاً.",
            "CheckIn.VoidReasonRequired" => Strings.GetOptionalString("Error_CheckIn_VoidReasonRequired") ?? "يرجى كتابة سبب الإلغاء (5 أحرف على الأقل).",

            // Beneficiary
            "Beneficiary.NotFound" => Strings.GetOptionalString("Error_Beneficiary_NotFound") ?? "لم يتم العثور على بيانات الزبون المطلوبة.",
            "Beneficiary.CardNotFound" => Strings.GetOptionalString("Error_Beneficiary_CardNotFound") ?? "رقم البطاقة غير مسجل بالمنظومة.",
            "Beneficiary.CardAlreadyExists" => Strings.GetOptionalString("Error_Beneficiary_CardAlreadyExists") ?? "رقم البطاقة مسجل مسبقاً لنفس الجهة الراعية.",
            "Beneficiary.NationalIdAlreadyExists" => Strings.GetOptionalString("Error_Beneficiary_NationalIdAlreadyExists") ?? "الرقم الوطني مسجل مسبقاً لزبون آخر.",
            "Beneficiary.MultipleHoldersFound" => Strings.GetOptionalString("Error_Beneficiary_MultipleHoldersFound") ?? "تم العثور على رقم البطاقة في أكثر من جهة راعية.",
            "Beneficiary.Blocked" => Strings.GetOptionalString("Error_Beneficiary_Blocked") ?? "حساب الزبون محظور تماماً من الدخول.",
            "Beneficiary.Suspended" => Strings.GetOptionalString("Error_Beneficiary_Suspended") ?? "حساب الزبون موقوف مؤقتاً.",
            "Beneficiary.CardExpired" => Strings.GetOptionalString("Error_Beneficiary_CardExpired") ?? "صلاحية البطاقة منتهية.",
            "Beneficiary.InvalidStatusTransition" => Strings.GetOptionalString("Error_Beneficiary_InvalidStatusTransition") ?? "تغيير حالة الزبون المطلوب غير صالح.",

            // Organization
            "Organization.NotFound" => Strings.GetOptionalString("Error_Organization_NotFound") ?? "لم يتم العثور على الجهة المطلوبة.",
            "Organization.CodeAlreadyExists" => Strings.GetOptionalString("Error_Organization_CodeAlreadyExists") ?? "رمز الجهة مسجل مسبقاً لجهة أخرى.",
            "Organization.NameAlreadyExists" => Strings.GetOptionalString("Error_Organization_NameAlreadyExists") ?? "اسم الجهة مسجل مسبقاً لجهة أخرى.",
            "Organization.CannotDeactivateDefault" => Strings.GetOptionalString("Error_Organization_CannotDeactivateDefault") ?? "لا يمكن تعطيل الجهة الافتراضية للمنظومة.",

            // AgeCategory
            "AgeCategory.NotFound" => Strings.GetOptionalString("Error_AgeCategory_NotFound") ?? "لم يتم العثور على الفئة العمرية المطلوبة.",
            "AgeCategory.NameAlreadyExists" => Strings.GetOptionalString("Error_AgeCategory_NameAlreadyExists") ?? "اسم الفئة العمرية مسجل مسبقاً.",

            // User
            "User.NotFound" => Strings.GetOptionalString("Error_User_NotFound") ?? "لم يتم العثور على المستخدم المطلوب.",
            "User.UsernameAlreadyExists" => Strings.GetOptionalString("Error_User_UsernameAlreadyExists") ?? "اسم المستخدم مستخدم مسبقاً، يرجى اختيار اسم آخر.",
            "User.CannotDeactivateSelf" => Strings.GetOptionalString("Error_User_CannotDeactivateSelf") ?? "لا يمكنك تعطيل حسابك الخاص أثناء تسجيل الدخول به.",
            "User.CannotChangeOwnRole" => Strings.GetOptionalString("Error_User_CannotChangeOwnRole") ?? "لا يمكنك تغيير صلاحيات حسابك الخاص.",

            // Workstation
            "Workstation.NotFound" => Strings.GetOptionalString("Error_Workstation_NotFound") ?? "لم يتم العثور على محطة العمل المطلوبة.",
            "Workstation.MachineNameAlreadyExists" => Strings.GetOptionalString("Error_Workstation_MachineNameAlreadyExists") ?? "اسم الجهاز مسجل مسبقاً لمحطة عمل أخرى.",

            // Settings
            "Settings.KeyNotFound" => Strings.GetOptionalString("Error_Settings_KeyNotFound") ?? "مفتاح الإعداد المطلوب غير موجود.",
            "Settings.InvalidValue" => Strings.GetOptionalString("Error_Settings_InvalidValue") ?? "قيمة الإعداد المدخلة غير صالحة.",

            // Reports
            "Reports.InvalidDateRange" => Strings.GetOptionalString("Error_Reports_InvalidDateRange") ?? "يجب أن يكون تاريخ البداية يسبق أو يساوي تاريخ النهاية.",

            _ => !string.IsNullOrWhiteSpace(error.Message) ? error.Message : Strings.Msg_UnexpectedError
        };
    }
}
