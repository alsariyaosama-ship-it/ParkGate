using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.EntryCards;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.EntryCards;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// R6.1 / R6.2: the entry-card tables are EF entities created by migrations, and the repositories use EF only.
/// </summary>
[Trait("Category", "Integration")]
public sealed class EntryCardPersistenceTests : IClassFixture<LocalDbTestFixture>
{
    private static readonly DateOnly Expiry = new(2027, 11, 18);

    private readonly LocalDbTestFixture _fixture;
    private readonly FixtureContextFactory _factory;

    public EntryCardPersistenceTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
        _factory = new FixtureContextFactory(fixture);
    }

    [Fact]
    public async Task Migrations_CreateEntryCardTablesAndIndexes()
    {
        await using var context = _fixture.CreateDbContext();
        var model = context.Model;

        model.FindEntityType(typeof(QrSigningKey))!.GetTableName().ShouldBe("QrSigningKeys");
        model.FindEntityType(typeof(EntryCard))!.GetTableName().ShouldBe("EntryCards");
        model.FindEntityType(typeof(CardScan))!.GetTableName().ShouldBe("CardScanLog");

        (await context.QrSigningKeys.CountAsync()).ShouldBeGreaterThanOrEqualTo(0);
        (await context.EntryCards.CountAsync()).ShouldBeGreaterThanOrEqualTo(0);
        (await context.CardScanLog.CountAsync()).ShouldBeGreaterThanOrEqualTo(0);

        var indexNames = model.FindEntityType(typeof(EntryCard))!.GetIndexes().Select(i => i.GetDatabaseName()).ToList();
        indexNames.ShouldContain("UX_EntryCards_OneActivePerBeneficiary");
        indexNames.ShouldContain("UX_EntryCards_CardId");
        model.FindEntityType(typeof(CardScan))!.GetIndexes().Select(i => i.GetDatabaseName()).ShouldContain("IX_CardScanLog_CardId");
    }

    [Fact]
    public async Task RegisterAsync_SameKeyTwice_ReturnsSameIdAndNewKeyGetsNext()
    {
        var repo = new QrKeyRepository(_factory);
        var first = NewKey();
        var second = NewKey();

        var id1 = await repo.RegisterAsync(first, "DESK-A");
        var again = await repo.RegisterAsync(first, "DESK-A");
        var id2 = await repo.RegisterAsync(second, "DESK-B");

        again.ShouldBe(id1);
        id2.ShouldBe((byte)(id1 + 1));
        var trusted = await repo.GetTrustedKeysAsync();
        trusted.ShouldContain(k => k.KeyId == id1 && k.SubjectPublicKeyInfo.SequenceEqual(first));
    }

    [Fact]
    public async Task RegisterAsync_RevokedKey_Throws()
    {
        var repo = new QrKeyRepository(_factory);
        var key = NewKey();
        var id = await repo.RegisterAsync(key, "DESK-R");

        await using (var context = _fixture.CreateDbContext())
        {
            var stored = await context.QrSigningKeys.SingleAsync(k => k.KeyId == id);
            stored.Revoke(DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        await Should.ThrowAsync<InvalidOperationException>(() => repo.RegisterAsync(key, "DESK-R"));
        (await repo.GetTrustedKeysAsync()).ShouldNotContain(k => k.KeyId == id);
    }

    [Fact]
    public async Task ReplaceActiveCardAsync_Reissue_RevokesOldAndFindsBoth()
    {
        var (holderId, keyId) = await SeedHolderAndKeyAsync(organizationActive: true);
        var repo = new EntryCardRepository(_factory);
        var oldCard = Guid.NewGuid();
        var newCard = Guid.NewGuid();

        await repo.ReplaceActiveCardAsync(holderId, oldCard, keyId, Expiry);
        await repo.ReplaceActiveCardAsync(holderId, newCard, keyId, Expiry);

        var old = await repo.FindAsync(oldCard);
        var current = await repo.FindAsync(newCard);
        old.ShouldNotBeNull();
        old.IsRevoked.ShouldBeTrue();
        current.ShouldNotBeNull();
        current.IsRevoked.ShouldBeFalse();
        current.BeneficiaryId.ShouldBe(holderId);
        current.PermitActive.ShouldBeTrue();

        await using var context = _fixture.CreateDbContext();
        var revoked = await context.EntryCards.SingleAsync(c => c.CardId == oldCard);
        revoked.RevokedAtUtc.ShouldNotBeNull();
        revoked.RevokeReason.ShouldBe(EntryCard.ReplacedReason);
        revoked.IssuedAtUtc.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-10));
    }

    [Fact]
    public async Task FindAsync_InactiveOrganization_ReportsPermitInactive()
    {
        var (holderId, keyId) = await SeedHolderAndKeyAsync(organizationActive: false);
        var repo = new EntryCardRepository(_factory);
        var card = Guid.NewGuid();

        await repo.ReplaceActiveCardAsync(holderId, card, keyId, Expiry);

        (await repo.FindAsync(card))!.PermitActive.ShouldBeFalse();
        (await repo.FindAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact]
    public async Task RevokeAsync_ActiveCard_RecordsReason()
    {
        var (holderId, keyId) = await SeedHolderAndKeyAsync(organizationActive: true);
        var repo = new EntryCardRepository(_factory);
        var card = Guid.NewGuid();
        await repo.ReplaceActiveCardAsync(holderId, card, keyId, Expiry);

        await repo.RevokeAsync(card, "lost");

        await using var context = _fixture.CreateDbContext();
        var stored = await context.EntryCards.SingleAsync(c => c.CardId == card);
        stored.RevokedAtUtc.ShouldNotBeNull();
        stored.RevokeReason.ShouldBe("lost");
    }

    [Fact]
    public async Task ReplaceActiveCardAsync_ParallelIssues_LeaveExactlyOneActiveCard()
    {
        var (holderId, keyId) = await SeedHolderAndKeyAsync(organizationActive: true);
        var repo = new EntryCardRepository(_factory);

        var issues = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() => repo.ReplaceActiveCardAsync(holderId, Guid.NewGuid(), keyId, Expiry)));
        await Task.WhenAll(issues);

        await using var context = _fixture.CreateDbContext();
        var cards = await context.EntryCards.Where(c => c.BeneficiaryId == holderId).ToListAsync();
        cards.Count.ShouldBe(8);
        cards.Count(c => c.RevokedAtUtc == null).ShouldBe(1);
    }

    [Fact]
    public async Task CardScanLog_WriteAsync_StoresStationWorkstationAndStatus()
    {
        int workstationId;
        await using (var context = _fixture.CreateDbContext())
        {
            var ws = new Workstation($"SCAN-{Guid.NewGuid():N}"[..12], "بوابة");
            context.Workstations.Add(ws);
            await context.SaveChangesAsync();
            workstationId = ws.Id;
        }

        var log = new CardScanLogRepository(_factory, new FixedWorkstation(workstationId));
        var cardId = Guid.NewGuid();

        await log.WriteAsync(new CardScanLogEntry("GATE-1", CardScanStatus.Revoked, cardId, "PG1.abc"));

        await using var read = _fixture.CreateDbContext();
        var row = await read.CardScanLog.SingleAsync(s => s.CardId == cardId);
        row.StationName.ShouldBe("GATE-1");
        row.WorkstationId.ShouldBe(workstationId);
        row.Status.ShouldBe(CardScanStatus.Revoked);
        row.RawPrefix.ShouldBe("PG1.abc");
        row.ScannedAtUtc.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-10));
    }

    private async Task<(Guid HolderId, byte KeyId)> SeedHolderAndKeyAsync(bool organizationActive)
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"EC{Guid.NewGuid():N}"[..10], "جهة البطاقات");
        if (!organizationActive)
        {
            org.Deactivate();
        }

        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var holder = new Beneficiary(
            Guid.NewGuid(),
            org.Id,
            CardNumber.Create($"EC-{Guid.NewGuid():N}"[..12]),
            "حامل بطاقة QR",
            PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        var keyId = await new QrKeyRepository(_factory).RegisterAsync(NewKey(), "DESK-SEED");
        return (holder.Id, keyId);
    }

    private static byte[] NewKey()
    {
        var key = new byte[91];
        Random.Shared.NextBytes(key);
        return key;
    }

    private sealed class FixtureContextFactory(LocalDbTestFixture fixture) : IDbContextFactory<ParkGateDbContext>
    {
        public ParkGateDbContext CreateDbContext() => fixture.CreateDbContext();
    }

    private sealed class FixedWorkstation(int id) : ICurrentWorkstation
    {
        public int? WorkstationId => id;
        public string MachineName => "TEST";
        public string? DisplayName => "TEST";
        public WorkstationType? Type => WorkstationType.Desk;
        public bool IsRegistered => true;
    }
}
