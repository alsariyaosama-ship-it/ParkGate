using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace ParkGate.Domain.EntryCards;

/// <summary>
/// بنية رمز QR لبطاقة الدخول — 84 بايت تُكتب كرقم عشري ثابت الطول (203 خانة):
///   [0]       إصدار التنسيق
///   [1]       رقم مفتاح التوقيع (KeyId)
///   [2..17]   CardId — معرّف عشوائي 128-bit (ليس رقم المستفيد في قاعدة البيانات)
///   [18..19]  تاريخ الانتهاء: عدد الأيام منذ 2025-01-01
///   [20..83]  توقيع ECDSA P-256 / SHA-256 على (Context + البايتات 0..19)
///
/// لا توجد أي بيانات شخصية داخل الرمز.
/// أرقام فقط: يُشفَّر QR بنمط Numeric (أصغر حجماً)، ولا يتأثر بلغة لوحة المفاتيح
/// عند استخدام قارئ USB يعمل كلوحة مفاتيح (Keyboard Wedge).
/// </summary>
public static class QrTokenFormat
{
    public const byte FormatVersion = 1;
    public const int BodyLength = 20;
    public const int SignatureLength = 64;
    public const int TotalLength = BodyLength + SignatureLength;

    /// <summary>طول النص الممسوح بالخانات (203).</summary>
    public static readonly int DigitLength =
        (BigInteger.Pow(256, TotalLength) - 1).ToString(CultureInfo.InvariantCulture).Length;

    public static readonly DateOnly ExpiryEpoch = new(2025, 1, 1);

    // يربط التوقيع بهذا الاستخدام تحديداً، فلا يصلح توقيع من سياق آخر.
    private static readonly byte[] Context = Encoding.ASCII.GetBytes("ParkGate.EntryCard/v1");

    public static void WriteBody(Span<byte> body, byte keyId, Guid cardId, DateOnly expiresOn)
    {
        int days = expiresOn.DayNumber - ExpiryEpoch.DayNumber;
        if (days is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresOn), "تاريخ الانتهاء خارج النطاق المدعوم.");
        }

        body[0] = FormatVersion;
        body[1] = keyId;
        cardId.TryWriteBytes(body.Slice(2, 16));
        BinaryPrimitives.WriteUInt16BigEndian(body.Slice(18, 2), (ushort)days);
    }

    public static EntryCardToken ReadBody(ReadOnlySpan<byte> body) => new(
        KeyId: body[1],
        CardId: new Guid(body.Slice(2, 16)),
        ExpiresOn: ExpiryEpoch.AddDays(BinaryPrimitives.ReadUInt16BigEndian(body.Slice(18, 2))));

    public static byte[] MessageToSign(ReadOnlySpan<byte> body)
    {
        var message = new byte[Context.Length + body.Length];
        Context.CopyTo(message, 0);
        body.CopyTo(message.AsSpan(Context.Length));
        return message;
    }

    public static string ToDigits(ReadOnlySpan<byte> raw) =>
        new BigInteger(raw, isUnsigned: true, isBigEndian: true)
            .ToString("D" + DigitLength, CultureInfo.InvariantCulture);

    /// <summary>
    /// يقبل الأرقام اللاتينية والعربية-الهندية (٠-٩ و ۰-۹) لأن بعض القارئات/التخطيطات تُخرجها،
    /// ويتجاهل المسافات و Enter في الطرفين.
    /// </summary>
    public static bool TryFromDigits(string? text, Span<byte> raw)
    {
        if (text is null)
        {
            return false;
        }

        ReadOnlySpan<char> s = text.AsSpan().Trim();
        if (s.Length != DigitLength)
        {
            return false;
        }

        Span<char> ascii = stackalloc char[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            char d = c switch
            {
                >= '0' and <= '9' => c,
                >= '٠' and <= '٩' => (char)('0' + (c - '٠')),
                >= '۰' and <= '۹' => (char)('0' + (c - '۰')),
                _ => '\0',
            };
            if (d == '\0')
            {
                return false;
            }
            ascii[i] = d;
        }

        var n = BigInteger.Parse(ascii, NumberStyles.None, CultureInfo.InvariantCulture);
        int count = n.GetByteCount(isUnsigned: true);
        if (count > raw.Length)
        {
            return false;
        }

        raw.Clear();
        return n.TryWriteBytes(raw[(raw.Length - count)..], out _, isUnsigned: true, isBigEndian: true);
    }
}
