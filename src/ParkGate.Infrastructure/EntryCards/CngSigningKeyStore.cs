using System.Runtime.Versioning;
using System.Security.Cryptography;
using ParkGate.Application.EntryCards;

namespace ParkGate.Infrastructure.EntryCards;

/// <summary>
/// مفتاح التوقيع الخاص محفوظ في مخزن مفاتيح ويندوز (CNG) وغير قابل للتصدير:
/// لا يوجد كملف، ولا يمكن نسخه إلى جهاز آخر — يُستخدم فقط من حساب ويندوز الذي أنشأه على جهاز الإصدار.
/// فقدان الجهاز لا يُبطل البطاقات القديمة (مفاتيحها العامة باقية في القاعدة)؛ يُنشأ مفتاح جديد برقم KeyId جديد.
/// </summary>
[SupportedOSPlatform("windows")]
public static class CngSigningKeyStore
{
    public const string DefaultKeyName = "ParkGate.EntryCards.QrSigning";

    public static ECDsa OpenOrCreate(string keyName = DefaultKeyName)
    {
        using CngKey key = CngKey.Exists(keyName)
            ? CngKey.Open(keyName)
            : CngKey.Create(CngAlgorithm.ECDsaP256, keyName, new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.None,
                KeyUsage = CngKeyUsages.Signing,
            });

        return new ECDsaCng(key);
    }

    /// <summary>عند إلغاء مفتاح هذا الجهاز (اشتباه اختراق): احذفه ثم أعد تشغيل برنامج الإصدار لإنشاء مفتاح جديد.</summary>
    public static void Delete(string keyName = DefaultKeyName)
    {
        if (!CngKey.Exists(keyName))
        {
            return;
        }

        using var key = CngKey.Open(keyName);
        key.Delete();
    }
}

[SupportedOSPlatform("windows")]
public static class QrSigningBootstrap
{
    /// <summary>لجهاز الإصدار فقط: يفتح/ينشئ المفتاح، ويسجّل مفتاحه العام في القاعدة، ويعيد الموقِّع.</summary>
    public static async Task<QrTokenSigner> CreateSignerAsync(IQrKeyRepository keys, string stationName, CancellationToken ct = default)
    {
        ECDsa privateKey = CngSigningKeyStore.OpenOrCreate();
        try
        {
            byte keyId = await keys.RegisterAsync(privateKey.ExportSubjectPublicKeyInfo(), stationName, ct);
            return new QrTokenSigner(privateKey, keyId);
        }
        catch
        {
            privateKey.Dispose();
            throw;
        }
    }
}
