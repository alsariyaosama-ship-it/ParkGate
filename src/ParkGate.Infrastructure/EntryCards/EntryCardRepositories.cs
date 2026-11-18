using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.EntryCards;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.EntryCards;

/// <summary>Entry cards through EF only (ADR-025, R6.1).</summary>
public sealed class EntryCardRepository : IEntryCardRepository
{
    // Two desks reissuing the same holder at once collide on UX_EntryCards_OneActivePerBeneficiary;
    // the loser reads the winner's card and revokes it, so the last issue wins and one card stays active.
    private const int MaxAttempts = 10;

    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public EntryCardRepository(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<EntryCardRecord?> FindAsync(Guid cardId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        return await (
                from card in context.EntryCards.AsNoTracking()
                join holder in context.Beneficiaries on card.BeneficiaryId equals holder.Id
                join org in context.Organizations on holder.OrganizationId equals org.Id
                where card.CardId == cardId
                select new EntryCardRecord(card.CardId, card.BeneficiaryId, card.RevokedAtUtc != null, org.IsActive))
            .SingleOrDefaultAsync(ct);
    }

    public async Task ReplaceActiveCardAsync(Guid beneficiaryId, Guid cardId, byte keyId, DateOnly expiresOn, CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {
                var nowUtc = await DatabaseClock.GetUtcNowAsync(context, ct);
                var active = await context.EntryCards
                    .Where(c => c.BeneficiaryId == beneficiaryId && c.RevokedAtUtc == null)
                    .ToListAsync(ct);

                foreach (var card in active)
                {
                    card.Revoke(nowUtc, EntryCard.ReplacedReason);
                }

                // Revoke first in its own save: the filtered unique index must never see two active cards.
                await context.SaveChangesAsync(ct);

                context.EntryCards.Add(EntryCard.Issue(cardId, beneficiaryId, keyId, expiresOn));
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts && TransientConflict.Is(ex))
            {
                await transaction.RollbackAsync(ct);
            }
        }
    }

    public async Task RevokeAsync(Guid cardId, string reason, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var card = await context.EntryCards.SingleOrDefaultAsync(c => c.CardId == cardId && c.RevokedAtUtc == null, ct);
        if (card is null)
        {
            return;
        }

        card.Revoke(await DatabaseClock.GetUtcNowAsync(context, ct), reason);
        await context.SaveChangesAsync(ct);
    }
}

/// <summary>Public keys of the signing desks through EF only (ADR-025, R6.1).</summary>
public sealed class QrKeyRepository : IQrKeyRepository
{
    // Two desks registering their first key at once may pick the same next id; the loser retries.
    private const int MaxAttempts = 10;

    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public QrKeyRepository(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<QrPublicKey>> GetTrustedKeysAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.QrSigningKeys
            .AsNoTracking()
            .Where(k => k.RevokedAtUtc == null)
            .Select(k => new QrPublicKey(k.KeyId, k.PublicKeySpki))
            .ToListAsync(ct);
    }

    public async Task<byte> RegisterAsync(byte[] subjectPublicKeyInfo, string stationName, CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);

            var existing = await context.QrSigningKeys
                .AsNoTracking()
                .SingleOrDefaultAsync(k => k.PublicKeySpki == subjectPublicKeyInfo, ct);
            if (existing is not null)
            {
                return existing.IsRevoked
                    ? throw new InvalidOperationException(
                        "The QR signing key of this desk is revoked. Delete the local key (CngSigningKeyStore.Delete) and restart.")
                    : existing.KeyId;
            }

            var lastId = await context.QrSigningKeys.MaxAsync(k => (int?)k.KeyId, ct) ?? 0;
            if (lastId >= byte.MaxValue)
            {
                throw new InvalidOperationException("No QR signing key id is left (255 keys registered).");
            }

            var key = QrSigningKey.Register((byte)(lastId + 1), subjectPublicKeyInfo, stationName);
            context.QrSigningKeys.Add(key);
            try
            {
                await context.SaveChangesAsync(ct);
                return key.KeyId;
            }
            catch (Exception ex) when (attempt < MaxAttempts && TransientConflict.Is(ex))
            {
                // Another desk took this id or registered the same key; read again.
            }
        }
    }
}

/// <summary>Scan log through EF only (ADR-025, R6.1). Records the current workstation when it is registered (R6.2).</summary>
public sealed class CardScanLogRepository : ICardScanLog
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;
    private readonly ICurrentWorkstation _workstation;

    public CardScanLogRepository(IDbContextFactory<ParkGateDbContext> contextFactory, ICurrentWorkstation workstation)
    {
        _contextFactory = contextFactory;
        _workstation = workstation;
    }

    public async Task WriteAsync(CardScanLogEntry entry, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.CardScanLog.Add(CardScan.Record(entry.StationName, _workstation.WorkstationId, entry.Status, entry.CardId, entry.RawPrefix));
        await context.SaveChangesAsync(ct);
    }
}

/// <summary>Conflicts that a fresh attempt resolves: a unique-index or concurrency clash, or a deadlock victim.</summary>
internal static class TransientConflict
{
    private const int DeadlockVictim = 1205;

    public static bool Is(Exception ex) =>
        ex is DbUpdateException ||
        ex is SqlException { Number: DeadlockVictim } ||
        ex.InnerException is SqlException { Number: DeadlockVictim };
}
