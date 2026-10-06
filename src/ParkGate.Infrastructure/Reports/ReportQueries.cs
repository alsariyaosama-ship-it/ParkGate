using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Reports;

public sealed class ReportQueries : IReportQueries
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public ReportQueries(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<DailySummaryReportDto> GetDailySummaryAsync(
        DateOnly date,
        int? organizationId = null,
        int? workstationId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        string? orgName = null;
        if (organizationId.HasValue)
        {
            orgName = await context.Organizations
                .Where(o => o.Id == organizationId.Value)
                .Select(o => o.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string? wsName = null;
        if (workstationId.HasValue)
        {
            wsName = await context.Workstations
                .Where(w => w.Id == workstationId.Value)
                .Select(w => w.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var attemptsQuery = context.EntryAttempts.AsNoTracking().Where(a => a.BusinessDate == date);
        if (organizationId.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.OrganizationId == organizationId.Value);
        }
        if (workstationId.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.WorkstationId == workstationId.Value);
        }

        var attempts = await attemptsQuery
            .Select(a => new
            {
                a.Id,
                a.Result,
                a.IsVoided,
                a.TotalPersons,
                a.OrganizationId
            })
            .ToListAsync(cancellationToken);

        int totalAttempts = attempts.Count;
        int allowedEntries = attempts.Count(a => a.Result == EntryResult.Allowed && !a.IsVoided);
        int overriddenEntries = attempts.Count(a => a.Result == EntryResult.Overridden && !a.IsVoided);
        int totalVisits = allowedEntries + overriddenEntries;
        int deniedEntries = attempts.Count(a => a.Result == EntryResult.Denied);
        int voidedEntries = attempts.Count(a => a.IsVoided);
        int totalHolders = totalVisits;
        int totalPersons = attempts
            .Where(a => (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden) && !a.IsVoided)
            .Sum(a => a.TotalPersons);
        int totalCompanions = Math.Max(0, totalPersons - totalHolders);

        // Companion counts by AgeCategory
        var companionQuery = from c in context.EntryAttemptCompanions.AsNoTracking()
                             join a in context.EntryAttempts.AsNoTracking() on c.EntryAttemptId equals a.Id
                             where a.BusinessDate == date
                                   && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)
                                   && !a.IsVoided
                             select new { c.AgeCategoryId, c.Count, a.OrganizationId, a.WorkstationId };

        if (organizationId.HasValue)
        {
            companionQuery = companionQuery.Where(x => x.OrganizationId == organizationId.Value);
        }
        if (workstationId.HasValue)
        {
            companionQuery = companionQuery.Where(x => x.WorkstationId == workstationId.Value);
        }

        var companionCounts = await companionQuery
            .GroupBy(c => c.AgeCategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Sum(x => x.Count) })
            .ToListAsync(cancellationToken);

        var activeCategories = await context.AgeCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        var companionDict = companionCounts.ToDictionary(x => x.CategoryId, x => x.Count);
        var ageCategoryBreakdown = activeCategories.Select(c => new AgeCategorySummaryDto(
            c.Id,
            c.NameAr,
            companionDict.GetValueOrDefault(c.Id, 0)
        )).ToList();

        // Breakdown by Organization
        var allOrgs = await context.Organizations.AsNoTracking()
            .ToDictionaryAsync(o => o.Id, cancellationToken);

        var orgGroups = attempts
            .Where(a => (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden) && !a.IsVoided)
            .GroupBy(a => a.OrganizationId)
            .ToList();

        var orgBreakdown = new List<DailySummaryOrganizationRowDto>();
        foreach (var grp in orgGroups)
        {
            if (!allOrgs.TryGetValue(grp.Key, out var org))
            {
                continue;
            }

            int orgAllowed = grp.Count(a => a.Result == EntryResult.Allowed);
            int orgOverridden = grp.Count(a => a.Result == EntryResult.Overridden);
            int orgVisits = orgAllowed + orgOverridden;
            int orgPersons = grp.Sum(a => a.TotalPersons);
            int orgHolders = orgVisits;
            int orgComps = Math.Max(0, orgPersons - orgHolders);

            orgBreakdown.Add(new DailySummaryOrganizationRowDto(
                org.Id,
                org.Code,
                org.NameAr,
                orgAllowed,
                orgOverridden,
                orgVisits,
                orgHolders,
                orgComps,
                orgPersons));
        }

        return new DailySummaryReportDto(
            date,
            organizationId,
            orgName,
            workstationId,
            wsName,
            totalAttempts,
            allowedEntries,
            overriddenEntries,
            totalVisits,
            deniedEntries,
            voidedEntries,
            totalHolders,
            totalCompanions,
            totalPersons,
            ageCategoryBreakdown,
            orgBreakdown.OrderByDescending(r => r.TotalPersons).ToList());
    }

    public async Task<DateRangeSummaryReportDto> GetDateRangeSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        string? orgName = null;
        if (organizationId.HasValue)
        {
            orgName = await context.Organizations
                .Where(o => o.Id == organizationId.Value)
                .Select(o => o.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var attemptsQuery = context.EntryAttempts.AsNoTracking()
            .Where(a => a.BusinessDate >= fromDate && a.BusinessDate <= toDate);

        if (organizationId.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.OrganizationId == organizationId.Value);
        }

        var attempts = await attemptsQuery
            .Select(a => new
            {
                a.BusinessDate,
                a.Result,
                a.IsVoided,
                a.TotalPersons
            })
            .ToListAsync(cancellationToken);

        var companionQuery = from c in context.EntryAttemptCompanions.AsNoTracking()
                             join a in context.EntryAttempts.AsNoTracking() on c.EntryAttemptId equals a.Id
                             where a.BusinessDate >= fromDate && a.BusinessDate <= toDate
                                   && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)
                                   && !a.IsVoided
                             select new { a.BusinessDate, c.AgeCategoryId, c.Count, a.OrganizationId };

        if (organizationId.HasValue)
        {
            companionQuery = companionQuery.Where(x => x.OrganizationId == organizationId.Value);
        }

        var companionBreakdowns = await companionQuery
            .GroupBy(x => new { x.BusinessDate, x.AgeCategoryId })
            .Select(g => new { g.Key.BusinessDate, g.Key.AgeCategoryId, Count = g.Sum(x => x.Count) })
            .ToListAsync(cancellationToken);

        var companionLookup = companionBreakdowns
            .ToLookup(x => x.BusinessDate, x => (x.AgeCategoryId, x.Count));

        var activeCategories = await context.AgeCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        var rows = new List<DateRangeSummaryRowDto>();
        var groupedAttempts = attempts.GroupBy(a => a.BusinessDate).ToDictionary(g => g.Key, g => g.ToList());

        // Iterate through all days between fromDate and toDate
        for (var current = fromDate; current <= toDate; current = current.AddDays(1))
        {
            if (!groupedAttempts.TryGetValue(current, out var dayAttempts))
            {
                continue; // Only include active dates or dates with attempts
            }

            int dayTotalAttempts = dayAttempts.Count;
            int dayAllowed = dayAttempts.Count(a => a.Result == EntryResult.Allowed && !a.IsVoided);
            int dayOverridden = dayAttempts.Count(a => a.Result == EntryResult.Overridden && !a.IsVoided);
            int dayTotalVisits = dayAllowed + dayOverridden;
            int dayDenied = dayAttempts.Count(a => a.Result == EntryResult.Denied);
            int dayVoided = dayAttempts.Count(a => a.IsVoided);
            int dayTotalHolders = dayTotalVisits;
            int dayTotalPersons = dayAttempts
                .Where(a => (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden) && !a.IsVoided)
                .Sum(a => a.TotalPersons);
            int dayCompanions = Math.Max(0, dayTotalPersons - dayTotalHolders);

            var dayCompDict = companionLookup[current].ToDictionary(x => x.AgeCategoryId, x => x.Count);
            var compList = activeCategories.Select(c => new AgeCategorySummaryDto(
                c.Id,
                c.NameAr,
                dayCompDict.GetValueOrDefault(c.Id, 0)
            )).ToList();

            var dt = current.ToDateTime(TimeOnly.MinValue);
            var dayOfWeekAr = GetArabicDayOfWeek(dt.DayOfWeek);

            rows.Add(new DateRangeSummaryRowDto(
                current,
                dayOfWeekAr,
                dayTotalAttempts,
                dayAllowed,
                dayOverridden,
                dayTotalVisits,
                dayDenied,
                dayVoided,
                dayTotalHolders,
                dayCompanions,
                dayTotalPersons,
                compList));
        }

        // Totals calculation
        int sumAttempts = rows.Sum(r => r.TotalAttempts);
        int sumAllowed = rows.Sum(r => r.AllowedVisits);
        int sumOverridden = rows.Sum(r => r.OverriddenVisits);
        int sumVisits = rows.Sum(r => r.TotalVisits);
        int sumDenied = rows.Sum(r => r.DeniedAttempts);
        int sumVoided = rows.Sum(r => r.VoidedAttempts);
        int sumHolders = rows.Sum(r => r.TotalHolders);
        int sumComps = rows.Sum(r => r.TotalCompanions);
        int sumPersons = rows.Sum(r => r.TotalPersons);

        var totalCompBreakdown = activeCategories.Select(c => new AgeCategorySummaryDto(
            c.Id,
            c.NameAr,
            rows.Sum(r => r.CompanionsByAgeCategory.FirstOrDefault(cat => cat.CategoryId == c.Id)?.Count ?? 0)
        )).ToList();

        var totals = new DateRangeTotalsDto(
            sumAttempts,
            sumAllowed,
            sumOverridden,
            sumVisits,
            sumDenied,
            sumVoided,
            sumHolders,
            sumComps,
            sumPersons,
            totalCompBreakdown);

        return new DateRangeSummaryReportDto(
            fromDate,
            toDate,
            organizationId,
            orgName,
            rows,
            totals);
    }

    public async Task<OrganizationStatisticsReportDto> GetOrganizationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        string? orgName = null;
        if (organizationId.HasValue)
        {
            orgName = await context.Organizations
                .Where(o => o.Id == organizationId.Value)
                .Select(o => o.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Park-wide totals in the date range
        var parkWideQuery = context.EntryAttempts.AsNoTracking()
            .Where(a => a.BusinessDate >= fromDate && a.BusinessDate <= toDate
                        && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)
                        && !a.IsVoided);

        int totalParkVisits = await parkWideQuery.CountAsync(cancellationToken);
        int totalParkPersons = await parkWideQuery.SumAsync(a => a.TotalPersons, cancellationToken);

        // Filtered organization entries
        var entriesQuery = parkWideQuery;
        if (organizationId.HasValue)
        {
            entriesQuery = entriesQuery.Where(a => a.OrganizationId == organizationId.Value);
        }

        var validEntries = await entriesQuery
            .Select(a => new
            {
                a.OrganizationId,
                a.BeneficiaryId,
                a.TotalPersons
            })
            .ToListAsync(cancellationToken);

        // Companions grouped by Org and AgeCategory
        var companionQuery = from c in context.EntryAttemptCompanions.AsNoTracking()
                             join a in context.EntryAttempts.AsNoTracking() on c.EntryAttemptId equals a.Id
                             where a.BusinessDate >= fromDate && a.BusinessDate <= toDate
                                   && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)
                                   && !a.IsVoided
                             select new { a.OrganizationId, c.AgeCategoryId, c.Count };

        if (organizationId.HasValue)
        {
            companionQuery = companionQuery.Where(x => x.OrganizationId == organizationId.Value);
        }

        var orgCompanions = await companionQuery
            .GroupBy(x => new { x.OrganizationId, x.AgeCategoryId })
            .Select(g => new { g.Key.OrganizationId, g.Key.AgeCategoryId, Count = g.Sum(x => x.Count) })
            .ToListAsync(cancellationToken);

        var compLookup = orgCompanions.ToLookup(x => x.OrganizationId, x => (x.AgeCategoryId, x.Count));

        var activeCategories = await context.AgeCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        var allOrgs = await context.Organizations.AsNoTracking()
            .OrderBy(o => o.NameAr)
            .ToListAsync(cancellationToken);

        var entriesByOrg = validEntries.GroupBy(x => x.OrganizationId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<OrganizationStatisticsRowDto>();
        foreach (var org in allOrgs)
        {
            if (organizationId.HasValue && org.Id != organizationId.Value)
            {
                continue;
            }

            if (!entriesByOrg.TryGetValue(org.Id, out var orgList) && organizationId.HasValue)
            {
                // If specific org requested and no entries, show row with zeros
                orgList = [];
            }
            else if (orgList == null)
            {
                continue; // Skip orgs with 0 visits when viewing all organizations
            }

            int visits = orgList.Count;
            int uniqueBeneficiaries = orgList.Select(x => x.BeneficiaryId).Distinct().Count();
            int totalPersons = orgList.Sum(x => x.TotalPersons);
            int holders = visits;
            int comps = Math.Max(0, totalPersons - holders);
            double pct = totalParkPersons > 0 ? (totalPersons * 100.0 / totalParkPersons) : 0.0;

            var orgCompDict = compLookup[org.Id].ToDictionary(x => x.AgeCategoryId, x => x.Count);
            var compList = activeCategories.Select(c => new AgeCategorySummaryDto(
                c.Id,
                c.NameAr,
                orgCompDict.GetValueOrDefault(c.Id, 0)
            )).ToList();

            rows.Add(new OrganizationStatisticsRowDto(
                org.Id,
                org.Code,
                org.NameAr,
                visits,
                uniqueBeneficiaries,
                holders,
                comps,
                totalPersons,
                Math.Round(pct, 2),
                compList));
        }

        int sumVisits = rows.Sum(r => r.TotalVisits);
        int sumUnique = rows.Sum(r => r.UniqueBeneficiariesCount);
        int sumHolders = rows.Sum(r => r.TotalHolders);
        int sumComps = rows.Sum(r => r.TotalCompanions);
        int sumPersons = rows.Sum(r => r.TotalPersons);

        var totalCompList = activeCategories.Select(c => new AgeCategorySummaryDto(
            c.Id,
            c.NameAr,
            rows.Sum(r => r.CompanionsByAgeCategory.FirstOrDefault(cat => cat.CategoryId == c.Id)?.Count ?? 0)
        )).ToList();

        var totals = new OrganizationStatisticsTotalsDto(
            sumVisits,
            sumUnique,
            sumHolders,
            sumComps,
            sumPersons,
            totalCompList);

        return new OrganizationStatisticsReportDto(
            fromDate,
            toDate,
            organizationId,
            orgName,
            totalParkVisits,
            totalParkPersons,
            rows.OrderByDescending(r => r.TotalPersons).ToList(),
            totals);
    }

    public async Task<OperatorWorkstationReportDto> GetOperatorWorkstationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var attempts = await context.EntryAttempts.AsNoTracking()
            .Where(a => a.BusinessDate >= fromDate && a.BusinessDate <= toDate)
            .Select(a => new
            {
                a.UserId,
                a.WorkstationId,
                a.Result,
                a.IsVoided,
                a.TotalPersons
            })
            .ToListAsync(cancellationToken);

        var users = await context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, cancellationToken);
        var workstations = await context.Workstations.AsNoTracking().ToDictionaryAsync(w => w.Id, cancellationToken);

        // Operator stats
        var opRows = new List<OperatorStatisticsRowDto>();
        foreach (var grp in attempts.GroupBy(a => a.UserId))
        {
            if (!users.TryGetValue(grp.Key, out var user))
            {
                continue;
            }

            int attemptsCount = grp.Count();
            int allowed = grp.Count(a => a.Result == EntryResult.Allowed && !a.IsVoided);
            int overridden = grp.Count(a => a.Result == EntryResult.Overridden && !a.IsVoided);
            int visits = allowed + overridden;
            int denied = grp.Count(a => a.Result == EntryResult.Denied);
            int voided = grp.Count(a => a.IsVoided);
            int totalPersons = grp
                .Where(a => (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden) && !a.IsVoided)
                .Sum(a => a.TotalPersons);

            opRows.Add(new OperatorStatisticsRowDto(
                user.Id,
                user.UserName,
                user.FullName,
                attemptsCount,
                allowed,
                overridden,
                visits,
                denied,
                voided,
                totalPersons));
        }

        // Workstation stats
        var wsRows = new List<WorkstationStatisticsRowDto>();
        foreach (var grp in attempts.GroupBy(a => a.WorkstationId))
        {
            if (!workstations.TryGetValue(grp.Key, out var ws))
            {
                continue;
            }

            int attemptsCount = grp.Count();
            int allowed = grp.Count(a => a.Result == EntryResult.Allowed && !a.IsVoided);
            int overridden = grp.Count(a => a.Result == EntryResult.Overridden && !a.IsVoided);
            int visits = allowed + overridden;
            int denied = grp.Count(a => a.Result == EntryResult.Denied);
            int voided = grp.Count(a => a.IsVoided);
            int totalPersons = grp
                .Where(a => (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden) && !a.IsVoided)
                .Sum(a => a.TotalPersons);

            wsRows.Add(new WorkstationStatisticsRowDto(
                ws.Id,
                ws.MachineName,
                ws.DisplayName,
                attemptsCount,
                allowed,
                overridden,
                visits,
                denied,
                voided,
                totalPersons));
        }

        return new OperatorWorkstationReportDto(
            fromDate,
            toDate,
            opRows.OrderByDescending(r => r.TotalVisits).ToList(),
            wsRows.OrderBy(r => r.DisplayName).ToList());
    }

    public async Task<DenialsAndOverridesReportDto> GetDenialsAndOverridesLogAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        DenialOverrideFilter filter = DenialOverrideFilter.All,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        string? orgName = null;
        if (organizationId.HasValue)
        {
            orgName = await context.Organizations
                .Where(o => o.Id == organizationId.Value)
                .Select(o => o.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var query = context.EntryAttempts.AsNoTracking()
            .Where(a => a.BusinessDate >= fromDate && a.BusinessDate <= toDate);

        if (organizationId.HasValue)
        {
            query = query.Where(a => a.OrganizationId == organizationId.Value);
        }

        if (filter == DenialOverrideFilter.DeniedOnly)
        {
            query = query.Where(a => a.Result == EntryResult.Denied);
        }
        else if (filter == DenialOverrideFilter.OverriddenOnly)
        {
            query = query.Where(a => a.Result == EntryResult.Overridden);
        }
        else
        {
            query = query.Where(a => a.Result == EntryResult.Denied || a.Result == EntryResult.Overridden);
        }

        var items = await (from a in query
                           join b in context.Beneficiaries.AsNoTracking() on a.BeneficiaryId equals b.Id
                           join o in context.Organizations.AsNoTracking() on a.OrganizationId equals o.Id
                           join u in context.Users.AsNoTracking() on a.UserId equals u.Id
                           join w in context.Workstations.AsNoTracking() on a.WorkstationId equals w.Id
                           join s in context.Users.AsNoTracking() on a.OverrideByUserId equals s.Id into sups
                           from sup in sups.DefaultIfEmpty()
                           orderby a.AttemptedAtUtc descending
                           select new DenialOverrideLogItemDto(
                               a.Id,
                               a.AttemptedAtUtc,
                               a.BusinessDate,
                               b.CardNumber.Value,
                               b.FullName,
                               o.Id,
                               o.NameAr,
                               a.Result,
                               a.DenialReason,
                               a.OverrideReason,
                               sup != null ? sup.FullName : null,
                               u.FullName,
                               w.DisplayName,
                               a.TotalPersons
                           )).ToListAsync(cancellationToken);

        return new DenialsAndOverridesReportDto(
            fromDate,
            toDate,
            organizationId,
            orgName,
            filter,
            items);
    }

    public async Task<HourlyDistributionReportDto> GetHourlyDistributionAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        string? orgName = null;
        if (organizationId.HasValue)
        {
            orgName = await context.Organizations
                .Where(o => o.Id == organizationId.Value)
                .Select(o => o.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var query = context.EntryAttempts.AsNoTracking()
            .Where(a => a.BusinessDate >= fromDate && a.BusinessDate <= toDate
                        && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)
                        && !a.IsVoided);

        if (organizationId.HasValue)
        {
            query = query.Where(a => a.OrganizationId == organizationId.Value);
        }

        var entries = await query
            .Select(a => new
            {
                a.AttemptedAtUtc,
                a.TotalPersons
            })
            .ToListAsync(cancellationToken);

        var tz = ResolveTimeZone(timeZoneId);
        var buckets = new (int Visits, int Persons)[24];

        foreach (var entry in entries)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(entry.AttemptedAtUtc, DateTimeKind.Utc), tz);
            int hour = local.Hour;
            if (hour >= 0 && hour < 24)
            {
                buckets[hour].Visits++;
                buckets[hour].Persons += entry.TotalPersons;
            }
        }

        int totalVisits = entries.Count;
        int totalPersons = entries.Sum(e => e.TotalPersons);

        var rows = new List<HourlyDistributionRowDto>();
        for (int h = 0; h < 24; h++)
        {
            int vCount = buckets[h].Visits;
            int pCount = buckets[h].Persons;
            double avgSize = vCount > 0 ? (double)pCount / vCount : 0.0;
            double pct = totalVisits > 0 ? ((double)vCount / totalVisits) * 100.0 : 0.0;

            var start = new DateTime(2000, 1, 1, h, 0, 0);
            var end = start.AddHours(1);
            var label = $"{start:hh:mm tt} - {end:hh:mm tt}";

            rows.Add(new HourlyDistributionRowDto(
                h,
                label,
                vCount,
                pCount,
                Math.Round(avgSize, 1),
                Math.Round(pct, 1)));
        }

        return new HourlyDistributionReportDto(
            fromDate,
            toDate,
            organizationId,
            orgName,
            totalVisits,
            totalPersons,
            rows);
    }

    private static string GetArabicDayOfWeek(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Sunday => "الأحد",
        DayOfWeek.Monday => "الاثنين",
        DayOfWeek.Tuesday => "الثلاثاء",
        DayOfWeek.Wednesday => "الأربعاء",
        DayOfWeek.Thursday => "الخميس",
        DayOfWeek.Friday => "الجمعة",
        DayOfWeek.Saturday => "السبت",
        _ => string.Empty
    };

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // Fall through
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Libya Standard Time");
        }
        catch
        {
            return TimeZoneInfo.CreateCustomTimeZone("Libya", TimeSpan.FromHours(2), "Libya Standard Time", "Libya Standard Time");
        }
    }
}
