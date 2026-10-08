namespace ParkGate.Application.EntryCards;

/// <summary>
/// إصدار (أو إعادة إصدار) بطاقة لمستفيد. الإصدار الجديد يلغي القديمة تلقائياً،
/// فأي نسخة مصورة من البطاقة السابقة تتوقف عن العمل.
/// </summary>
public sealed class EntryCardIssuer
{
    private readonly QrTokenSigner _signer;
    private readonly IEntryCardRepository _cards;
    private readonly TimeProvider _clock;
    private readonly EntryCardOptions _options;

    public EntryCardIssuer(QrTokenSigner signer, IEntryCardRepository cards, TimeProvider clock, EntryCardOptions options)
    {
        _signer = signer;
        _cards = cards;
        _clock = clock;
        _options = options;
    }

    public async Task<IssuedEntryCard> IssueAsync(Guid beneficiaryId, DateOnly? expiresOn = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        var expiry = expiresOn ?? today.AddDays(_options.ValidityDays);
        if (expiry < today)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresOn), "تاريخ الانتهاء قبل اليوم.");
        }

        var cardId = QrTokenSigner.NewCardId();
        string qrText = _signer.Encode(cardId, expiry);

        await _cards.ReplaceActiveCardAsync(beneficiaryId, cardId, _signer.KeyId, expiry, ct);
        return new IssuedEntryCard(beneficiaryId, cardId, expiry, qrText);
    }
}
