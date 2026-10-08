namespace ParkGate.Domain.EntryCards;

/// <summary>
/// Status of scanned entry card. Numeric values are stored in dbo.CardScanLog.Status.
/// </summary>
public enum CardScanStatus : byte
{
    Accepted = 0,
    Unreadable = 1,
    UnknownKey = 2,
    InvalidSignature = 3,
    Expired = 4,
    NotFound = 5,
    Revoked = 6,
    PermitSuspended = 7,
}

public static class CardScanStatusExtensions
{
    public static string ToArabic(this CardScanStatus status) => status switch
    {
        CardScanStatus.Accepted => "بطاقة صالحة",
        CardScanStatus.Unreadable => "رمز غير مقروء أو ليس بطاقة المنتزه",
        CardScanStatus.UnknownKey => "بطاقة صادرة من جهاز غير معتمد",
        CardScanStatus.InvalidSignature => "بطاقة مزورة — التوقيع غير صحيح",
        CardScanStatus.Expired => "انتهت صلاحية البطاقة",
        CardScanStatus.NotFound => "البطاقة غير مسجلة في النظام",
        CardScanStatus.Revoked => "البطاقة ملغاة (صدرت بطاقة بديلة أو أُوقفت)",
        CardScanStatus.PermitSuspended => "تصريح الجهة التابع لها موقوف",
        _ => status.ToString(),
    };
}
