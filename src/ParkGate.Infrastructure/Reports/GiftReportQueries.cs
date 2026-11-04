using Microsoft.EntityFrameworkCore;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.Reports;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Reports;

public sealed class GiftReportQueries : IGiftReportQueries
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public GiftReportQueries(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<GiftReceiptReportDto> GetReceiptsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        int? workstationId = null,
        GiftReceiptFilter filter = GiftReceiptFilter.All,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.GiftVouchers.AsNoTracking();

        if (filter == GiftReceiptFilter.Redeemed)
        {
            query = query.Where(v => v.Status == GiftVoucherStatus.Redeemed
                                     && v.RedeemedBusinessDate.HasValue
                                     && v.RedeemedBusinessDate.Value >= fromDate
                                     && v.RedeemedBusinessDate.Value <= toDate);
        }
        else if (filter == GiftReceiptFilter.Issued)
        {
            query = query.Where(v => v.Status == GiftVoucherStatus.Issued
                                     && v.BusinessDate >= fromDate
                                     && v.BusinessDate <= toDate);
        }
        else if (filter == GiftReceiptFilter.Voided)
        {
            query = query.Where(v => v.Status == GiftVoucherStatus.Voided
                                     && v.BusinessDate >= fromDate
                                     && v.BusinessDate <= toDate);
        }
        else
        {
            query = query.Where(v => v.BusinessDate >= fromDate && v.BusinessDate <= toDate);
        }

        if (organizationId.HasValue)
        {
            query = query.Where(v => v.OrganizationId == organizationId.Value);
        }

        if (workstationId.HasValue)
        {
            if (filter == GiftReceiptFilter.Redeemed)
            {
                query = query.Where(v => v.RedeemedWorkstationId == workstationId.Value);
            }
            else if (filter == GiftReceiptFilter.All)
            {
                query = query.Where(v => v.IssuedWorkstationId == workstationId.Value || v.RedeemedWorkstationId == workstationId.Value);
            }
            else
            {
                query = query.Where(v => v.IssuedWorkstationId == workstationId.Value);
            }
        }

        var joinedQuery = from v in query
                          join b in context.Beneficiaries.AsNoTracking() on v.BeneficiaryId equals b.Id
                          join o in context.Organizations.AsNoTracking() on v.OrganizationId equals o.Id
                          join uIssued in context.Users.AsNoTracking() on v.IssuedByUserId equals uIssued.Id
                          join uRedeemed in context.Users.AsNoTracking() on v.RedeemedByUserId equals uRedeemed.Id into redUsers
                          from uRed in redUsers.DefaultIfEmpty()
                          join wRedeemed in context.Workstations.AsNoTracking() on v.RedeemedWorkstationId equals wRedeemed.Id into redWs
                          from wRed in redWs.DefaultIfEmpty()
                          join uVoided in context.Users.AsNoTracking() on v.VoidedByUserId equals uVoided.Id into voidUsers
                          from uVoid in voidUsers.DefaultIfEmpty()
                          orderby v.Id descending
                          select new GiftReceiptRowDto(
                              v.Id,
                              b.Id,
                              b.FullName,
                              b.CardNumber.Value,
                              o.Id,
                              o.NameAr,
                              v.Value,
                              v.IssuedAtUtc,
                              uIssued.FullName,
                              v.RedeemedAtUtc,
                              uRed != null ? uRed.FullName : null,
                              wRed != null ? wRed.DisplayName : null,
                              v.Status,
                              v.IsOverride,
                              v.OverrideReason,
                              v.VoidedAtUtc,
                              uVoid != null ? uVoid.FullName : null,
                              v.VoidReason);

        var rows = await joinedQuery.ToListAsync(cancellationToken);

        int totalCount = rows.Count;
        decimal totalValue = rows.Sum(r => r.Value);

        var statusTotals = rows
            .GroupBy(r => r.Status)
            .Select(g => new GiftReceiptStatusTotalDto(g.Key, g.Count(), g.Sum(r => r.Value)))
            .OrderBy(t => t.Status)
            .ToList();

        var orgTotals = rows
            .GroupBy(r => new { r.OrganizationId, r.OrganizationName })
            .Select(g => new GiftReceiptOrganizationTotalDto(g.Key.OrganizationId, g.Key.OrganizationName, g.Count(), g.Sum(r => r.Value)))
            .OrderBy(t => t.OrganizationName)
            .ToList();

        return new GiftReceiptReportDto(
            fromDate,
            toDate,
            organizationId,
            workstationId,
            filter,
            rows,
            statusTotals,
            orgTotals,
            totalCount,
            totalValue);
    }
}
