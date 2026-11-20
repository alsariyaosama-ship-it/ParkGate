using System.Globalization;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.CardPatterns;

/// <summary>
/// Checks a registered or changed card number against its organization's patterns (F8, BR-29), inside the caller's
/// unit of work so the alert, any proposal and their audit rows save with the holder. It never blocks the save.
/// </summary>
internal static class CardPatternMonitor
{
    public const string AlertRaised = "CardPatternAlert.Raised";
    public const string PatternProposed = "CardPattern.Proposed";

    /// <summary>True when the number raised an alert for the Admin (the operator sees an amber note).</summary>
    public static async Task<bool> CheckAsync(IUnitOfWork uow, Beneficiary holder, int? userId, int? workstationId, CancellationToken ct)
    {
        var card = holder.CardNumber.Value;
        var settings = await LoadSettingsAsync(uow, ct);
        var patterns = await uow.CardPatterns.GetByOrganizationAsync(holder.OrganizationId, ct);
        var nowUtc = await uow.GetDbUtcNowAsync(ct);

        switch (CardPatternLearner.Check(card, patterns))
        {
            case CardPatternVerdict.Matches:
                return false;

            case CardPatternVerdict.Learning:
                var numbers = (await uow.Beneficiaries.GetCardNumbersByOrganizationAsync(holder.OrganizationId, ct)).ToList();
                if (!numbers.Contains(card))
                {
                    numbers.Add(card); // the holder of this use case is not saved yet
                }

                var first = CardPatternLearner.ProposeDominant(numbers, settings);
                if (first is null)
                {
                    return false;
                }

                var dominant = await ProposeAsync(uow, holder.OrganizationId, first, CardPatternSource.Dominant, nowUtc, userId, workstationId, ct);
                return !dominant.Matches(card) && await RaiseAsync(uow, holder, nowUtc, userId, workstationId, patterns.Append(dominant).ToList(), settings, ct);

            default:
                return await RaiseAsync(uow, holder, nowUtc, userId, workstationId, patterns, settings, ct);
        }
    }

    private static async Task<bool> RaiseAsync(
        IUnitOfWork uow,
        Beneficiary holder,
        DateTime nowUtc,
        int? userId,
        int? workstationId,
        IReadOnlyCollection<OrganizationCardPattern> patterns,
        CardPatternSettings settings,
        CancellationToken ct)
    {
        if (userId is not { } user || workstationId is not { } desk)
        {
            return false; // no actor to record; the use case already refused an unknown user or desk
        }

        var card = holder.CardNumber.Value;
        var alert = CardPatternAlert.Raise(holder.OrganizationId, holder.Id, card, nowUtc, user, desk);
        uow.CardPatternAlerts.Add(alert);
        await uow.AddAuditAsync(
            action: AlertRaised,
            userId: user,
            workstationId: desk,
            entityName: nameof(Beneficiary),
            entityId: holder.Id.ToString(),
            detailsJson: AuditDetails.ToJson(new
            {
                BeneficiaryId = holder.Id,
                CardNumber = card,
                HolderName = holder.FullName,
                holder.OrganizationId,
                alert.Shape,
            }),
            cancellationToken: ct);

        // A shape that keeps coming back may be a new series of genuine cards: propose it, but it stops
        // alerting only once an Admin approves it (CardPatternSource.NewSeries is never active by itself).
        var sameShape = (await uow.CardPatternAlerts.GetOpenCardNumbersAsync(holder.OrganizationId, alert.Shape, ct)).ToList();
        if (!sameShape.Contains(card))
        {
            sameShape.Add(card);
        }

        var series = CardPatternLearner.ProposeNewSeries(sameShape, settings);
        if (series is not null && !patterns.Any(p => p.Shape == series.Shape && p.Prefix == series.Prefix))
        {
            await ProposeAsync(uow, holder.OrganizationId, series, CardPatternSource.NewSeries, nowUtc, user, desk, ct);
        }

        return true;
    }

    private static async Task<OrganizationCardPattern> ProposeAsync(
        IUnitOfWork uow,
        int organizationId,
        ProposedCardPattern proposal,
        CardPatternSource source,
        DateTime nowUtc,
        int? userId,
        int? workstationId,
        CancellationToken ct)
    {
        var pattern = OrganizationCardPattern.Propose(organizationId, proposal.Shape, proposal.Prefix, source, proposal.SampleCount, nowUtc);
        uow.CardPatterns.Add(pattern);
        await uow.AddAuditAsync(
            action: PatternProposed,
            userId: userId,
            workstationId: workstationId,
            entityName: nameof(OrganizationCardPattern),
            entityId: null,
            detailsJson: AuditDetails.ToJson(new
            {
                OrganizationId = organizationId,
                proposal.Shape,
                proposal.Prefix,
                Source = source.ToString(),
                proposal.SampleCount,
            }),
            cancellationToken: ct);
        return pattern;
    }

    /// <summary>DATABASE.md §5 <c>CardPattern.*</c>; not seeded, so a missing or bad value keeps the default.</summary>
    internal static async Task<CardPatternSettings> LoadSettingsAsync(IUnitOfWork uow, CancellationToken ct)
    {
        async Task<int> Read(string key, int fallback)
        {
            var setting = await uow.AppSettings.GetByKeyAsync(key, ct);
            return setting is not null && int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
        }

        try
        {
            return new CardPatternSettings(
                await Read("CardPattern.MinSample", CardPatternSettings.DefaultMinSample),
                await Read("CardPattern.NewSeriesThreshold", CardPatternSettings.DefaultNewSeriesThreshold),
                await Read("CardPattern.PrefixLength", CardPatternSettings.DefaultPrefixLength),
                await Read("CardPattern.PrefixSharePercent", CardPatternSettings.DefaultPrefixSharePercent));
        }
        catch (ArgumentOutOfRangeException)
        {
            return CardPatternSettings.Default;
        }
    }
}
