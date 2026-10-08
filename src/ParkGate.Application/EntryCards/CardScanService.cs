using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.EntryCards;

/// <summary>
/// خطوات التحقق عند المسح:
///   1) التوقيع (بدون قاعدة بيانات) ← يرفض المزور والمعدّل فوراً
///   2) تاريخ الانتهاء الموقّع داخل الرمز
///   3) قاعدة البيانات: موجودة؟ غير ملغاة؟ تصريح الجهة فعّال؟
/// عند القبول يُمرَّر BeneficiaryId لمنطق تسجيل الدخول الحالي (قاعدة منع إعادة الدخول خلال المدة المحددة).
/// كل محاولة — مقبولة أو مرفوضة — تُسجَّل في CardScanLog.
/// </summary>
public sealed class CardScanService
{
    private const int RawPrefixLength = 32;

    private readonly QrTokenVerifier _verifier;
    private readonly IEntryCardRepository _cards;
    private readonly ICardScanLog _log;
    private readonly TimeProvider _clock;
    private readonly GateStation _station;

    public CardScanService(QrTokenVerifier verifier, IEntryCardRepository cards, ICardScanLog log,
        TimeProvider clock, GateStation station)
    {
        _verifier = verifier;
        _cards = cards;
        _log = log;
        _clock = clock;
        _station = station;
    }

    public async Task<CardScanResult> ScanAsync(string? scannedText, CancellationToken ct = default)
    {
        var verified = _verifier.Verify(scannedText);

        CardScanResult result = verified.Status switch
        {
            QrVerifyStatus.Valid => await CheckCardAsync(verified.Token!, ct),
            QrVerifyStatus.UnknownKey => new(CardScanStatus.UnknownKey, null),
            QrVerifyStatus.InvalidSignature => new(CardScanStatus.InvalidSignature, null),
            _ => new(CardScanStatus.Unreadable, null),
        };

        string? rawPrefix = result.IsAccepted ? null : Truncate(scannedText?.Trim());
        await _log.WriteAsync(new CardScanLogEntry(_station.Name, result.Status, verified.Token?.CardId, rawPrefix), ct);

        return result;
    }

    private async Task<CardScanResult> CheckCardAsync(EntryCardToken token, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        if (token.ExpiresOn < today)
        {
            return new(CardScanStatus.Expired, null);
        }

        var card = await _cards.FindAsync(token.CardId, ct);
        if (card is null)
        {
            return new(CardScanStatus.NotFound, null);
        }

        if (card.IsRevoked)
        {
            return new(CardScanStatus.Revoked, card.BeneficiaryId);
        }

        if (!card.PermitActive)
        {
            return new(CardScanStatus.PermitSuspended, card.BeneficiaryId);
        }

        return new(CardScanStatus.Accepted, card.BeneficiaryId);
    }

    private static string? Truncate(string? s)
    {
        if (s is null)
        {
            return null;
        }

        return s.Length <= RawPrefixLength ? s : s[..RawPrefixLength];
    }
}
