using System.Security.Cryptography;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.EntryCards;
using Shouldly;

namespace ParkGate.Application.Tests.EntryCards;

public sealed class EntryCardsTests
{
    private readonly ECDsa _issuingKey;
    private readonly ECDsa _attackerKey;
    private const byte KeyId = 1;

    public EntryCardsTests()
    {
        _issuingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _attackerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    }

    [Fact]
    public void Encode_ShouldProduce203AsciiDigits()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);

        string qr = signer.Encode(cardId, expiry);

        qr.Length.ShouldBe(QrTokenFormat.DigitLength);
        qr.All(char.IsAsciiDigit).ShouldBeTrue();
    }

    [Fact]
    public void Verify_ValidToken_ShouldSucceedWithIntactPayload()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);

        string qr = signer.Encode(cardId, expiry);
        var result = verifier.Verify(qr);

        result.Status.ShouldBe(QrVerifyStatus.Valid);
        result.Token.ShouldNotBeNull();
        result.Token.CardId.ShouldBe(cardId);
        result.Token.ExpiresOn.ShouldBe(expiry);
        result.Token.KeyId.ShouldBe(KeyId);
    }

    [Fact]
    public void Verify_SingleDigitModifications_ShouldAllBeRejected()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);
        string qr = signer.Encode(cardId, expiry);

        int accepted = 0;
        for (int i = 0; i < qr.Length; i++)
        {
            var chars = qr.ToCharArray();
            chars[i] = (char)('0' + (chars[i] - '0' + 1) % 10);
            if (verifier.Verify(new string(chars)).Status == QrVerifyStatus.Valid)
            {
                accepted++;
            }
        }

        accepted.ShouldBe(0);
    }

    [Fact]
    public void Verify_BitFlipsInRawBytes_ShouldAllBeRejected()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);
        string qr = signer.Encode(cardId, expiry);

        var raw = new byte[QrTokenFormat.TotalLength];
        QrTokenFormat.TryFromDigits(qr, raw).ShouldBeTrue();

        int accepted = 0;
        for (int bit = 0; bit < raw.Length * 8; bit++)
        {
            var copy = (byte[])raw.Clone();
            copy[bit / 8] ^= (byte)(1 << (bit % 8));
            if (verifier.Verify(QrTokenFormat.ToDigits(copy)).Status == QrVerifyStatus.Valid)
            {
                accepted++;
            }
        }

        accepted.ShouldBe(0);
    }

    [Fact]
    public void Verify_ForgedWithAttackerKey_ShouldReturnInvalidSignature()
    {
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        using var forger = new QrTokenSigner(ECDsa.Create(_attackerKey.ExportParameters(true)), KeyId);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);

        string forgedQr = forger.Encode(cardId, expiry);
        var result = verifier.Verify(forgedQr);

        result.Status.ShouldBe(QrVerifyStatus.InvalidSignature);
    }

    [Fact]
    public void Verify_ExtendedExpiryOnGenuineCard_ShouldReturnInvalidSignature()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);
        string qr = signer.Encode(cardId, expiry);

        var raw = new byte[QrTokenFormat.TotalLength];
        QrTokenFormat.TryFromDigits(qr, raw);
        QrTokenFormat.WriteBody(raw.AsSpan(0, QrTokenFormat.BodyLength), KeyId, cardId, expiry.AddYears(5));

        var tamperedQr = QrTokenFormat.ToDigits(raw);
        var result = verifier.Verify(tamperedQr);

        result.Status.ShouldBe(QrVerifyStatus.InvalidSignature);
    }

    [Fact]
    public void Verify_UnknownKeyId_ShouldReturnUnknownKey()
    {
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        using var otherSigner = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), 99);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);

        string qr = otherSigner.Encode(cardId, expiry);
        var result = verifier.Verify(qr);

        result.Status.ShouldBe(QrVerifyStatus.UnknownKey);
    }

    [Fact]
    public void Verify_ArabicIndicAndPersianDigits_ShouldBeAccepted()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var cardId = QrTokenSigner.NewCardId();
        var expiry = new DateOnly(2027, 10, 8);
        string qr = signer.Encode(cardId, expiry);

        string arabicIndic = new(qr.Select(c => (char)('٠' + (c - '0'))).ToArray());
        string persian = new(qr.Select(c => (char)('۰' + (c - '0'))).ToArray());

        verifier.Verify(arabicIndic).Status.ShouldBe(QrVerifyStatus.Valid);
        verifier.Verify(persian).Status.ShouldBe(QrVerifyStatus.Valid);
        verifier.Verify(qr + "\r\n").Status.ShouldBe(QrVerifyStatus.Valid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("https://example.com/ticket")]
    public void Verify_GarbageInputs_ShouldReturnUnreadable(string? input)
    {
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);

        var result = verifier.Verify(input);

        result.Status.ShouldBe(QrVerifyStatus.Unreadable);
    }

    [Fact]
    public void Signer_NonP256Key_ShouldThrowArgumentException()
    {
        using var p384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);

        Should.Throw<ArgumentException>(() => new QrTokenSigner(p384, 1));
    }

    [Fact]
    public void Signer_ExpiryBeforeEpoch_ShouldThrowArgumentOutOfRangeException()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            signer.Encode(QrTokenSigner.NewCardId(), new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public async Task CardScanService_FullIssueAndScanWorkflow_ShouldOperateCorrectly()
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);

        var repo = new FakeEntryCardRepository();
        var log = new FakeCardScanLog();
        var station = new GateStation("GATE-1");

        var parkClock = new ParkClock(ScanTestUow(), new StubSettingsService());
        var issuer = new EntryCardIssuer(signer, repo, parkClock, new EntryCardOptions { ValidityDays = 365 });
        var scanner = new CardScanService(verifier, repo, log, parkClock, station);

        var beneficiaryId = Guid.NewGuid();

        // 1. Issue first card
        var first = await issuer.IssueAsync(beneficiaryId);
        first.ExpiresOn.ShouldBe(new DateOnly(2027, 10, 8));

        // 2. Scan first card -> Accepted
        var scan1 = await scanner.ScanAsync(first.QrText);
        scan1.IsAccepted.ShouldBeTrue();
        scan1.BeneficiaryId.ShouldBe(beneficiaryId);

        // 3. Issue second card (re-issue)
        var second = await issuer.IssueAsync(beneficiaryId);

        // 4. First card should now be Revoked
        var scanOld = await scanner.ScanAsync(first.QrText);
        scanOld.Status.ShouldBe(CardScanStatus.Revoked);

        // 5. Second card accepted
        var scanNew = await scanner.ScanAsync(second.QrText);
        scanNew.IsAccepted.ShouldBeTrue();

        // 6. Organization permit suspended
        repo.PermitActive = false;
        var scanSuspended = await scanner.ScanAsync(second.QrText);
        scanSuspended.Status.ShouldBe(CardScanStatus.PermitSuspended);

        // 7. Verify audit logs were recorded
        log.Entries.Count.ShouldBe(4);
    }

    // ── R3.7: expiry is checked against the park's business date from the database clock ──

    [Theory]
    [InlineData("2026-11-13T21:30:00Z", CardScanStatus.Accepted)] // 23:30 in Tripoli: still the 13th
    [InlineData("2026-11-13T22:30:00Z", CardScanStatus.Expired)]  // 00:30 in Tripoli: already the 14th
    public async Task ScanAsync_CardExpiringOnThe13th_UsesTheDatabaseClockInTheParkTimeZone(string dbUtcNow, CardScanStatus expected)
    {
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        using var verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _issuingKey.ExportSubjectPublicKeyInfo())]);
        var repo = new FakeEntryCardRepository();
        var cardId = QrTokenSigner.NewCardId();
        await repo.ReplaceActiveCardAsync(Guid.NewGuid(), cardId, KeyId, new DateOnly(2026, 11, 13));

        var uowFactory = new FakeUnitOfWorkFactory();
        uowFactory.CurrentUow.CurrentDbTime = DateTime.Parse(dbUtcNow, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
        var parkClock = new ParkClock(uowFactory, new StubSettingsService());
        var scanner = new CardScanService(verifier, repo, new FakeCardScanLog(), parkClock, new GateStation("GATE-1"));

        var result = await scanner.ScanAsync(signer.Encode(cardId, new DateOnly(2026, 11, 13)));

        result.Status.ShouldBe(expected);
    }

    [Fact]
    public async Task IssueAsync_AfterMidnightInTheParkButNotInUtc_CountsValidityFromTheParkBusinessDate()
    {
        // R3.10: 22:30 UTC on the 13th is 00:30 on the 14th in Tripoli.
        using var signer = new QrTokenSigner(ECDsa.Create(_issuingKey.ExportParameters(true)), KeyId);
        var uowFactory = new FakeUnitOfWorkFactory();
        uowFactory.CurrentUow.CurrentDbTime = new DateTime(2026, 11, 13, 22, 30, 0, DateTimeKind.Utc);
        var issuer = new EntryCardIssuer(
            signer, new FakeEntryCardRepository(), new ParkClock(uowFactory, new StubSettingsService()),
            new EntryCardOptions { ValidityDays = 365 });

        var card = await issuer.IssueAsync(Guid.NewGuid());

        card.ExpiresOn.ShouldBe(new DateOnly(2026, 11, 14).AddDays(365));
    }

    /// <summary>Database clock of the workflow test: 21:00 on 8 October 2026 in Tripoli.</summary>
    private static FakeUnitOfWorkFactory ScanTestUow()
    {
        var uowFactory = new FakeUnitOfWorkFactory();
        uowFactory.CurrentUow.CurrentDbTime = new DateTime(2026, 10, 8, 19, 0, 0, DateTimeKind.Utc);
        return uowFactory;
    }

    private sealed class FakeEntryCardRepository : IEntryCardRepository
    {
        private readonly Dictionary<Guid, (Guid BeneficiaryId, bool IsRevoked)> _cards = new();
        public bool PermitActive { get; set; } = true;

        public Task<EntryCardRecord?> FindAsync(Guid cardId, CancellationToken ct = default)
        {
            if (!_cards.TryGetValue(cardId, out var c))
            {
                return Task.FromResult<EntryCardRecord?>(null);
            }

            return Task.FromResult<EntryCardRecord?>(new EntryCardRecord(cardId, c.BeneficiaryId, c.IsRevoked, PermitActive));
        }

        public Task ReplaceActiveCardAsync(Guid beneficiaryId, Guid cardId, byte keyId, DateOnly expiresOn, CancellationToken ct = default)
        {
            foreach (var existing in _cards.Where(kvp => kvp.Value.BeneficiaryId == beneficiaryId && !kvp.Value.IsRevoked).ToList())
            {
                _cards[existing.Key] = (beneficiaryId, true);
            }

            _cards[cardId] = (beneficiaryId, false);
            return Task.CompletedTask;
        }

        public Task RevokeAsync(Guid cardId, string reason, CancellationToken ct = default)
        {
            if (_cards.TryGetValue(cardId, out var c))
            {
                _cards[cardId] = (c.BeneficiaryId, true);
            }
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCardScanLog : ICardScanLog
    {
        public List<CardScanLogEntry> Entries { get; } = [];

        public Task WriteAsync(CardScanLogEntry entry, CancellationToken ct = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }
}
