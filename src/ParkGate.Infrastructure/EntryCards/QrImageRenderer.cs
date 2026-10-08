using QRCoder;

namespace ParkGate.Infrastructure.EntryCards;

/// <summary>
/// يرسم رمز البطاقة كصورة PNG (حزمة NuGet: QRCoder).
/// النص أرقام فقط ← QRCoder يختار نمط Numeric تلقائياً ← QR إصدار 7 (45×45 وحدة) بمستوى تصحيح Q (~25%)،
/// مناسب لبطاقة مطبوعة قد تُخدش. اطبعه بعرض 2.5 سم على الأقل.
/// </summary>
public static class QrImageRenderer
{
    public static byte[] RenderPng(string qrText, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}
