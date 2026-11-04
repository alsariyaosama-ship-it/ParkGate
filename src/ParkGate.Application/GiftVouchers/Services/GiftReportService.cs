using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Security;

namespace ParkGate.Application.GiftVouchers.Services;

public sealed class GiftReportService : IGiftReportService
{
    private readonly IGiftReportQueries _reportQueries;
    private readonly ICurrentUser _currentUser;

    public GiftReportService(IGiftReportQueries reportQueries, ICurrentUser currentUser)
    {
        _reportQueries = reportQueries ?? throw new ArgumentNullException(nameof(reportQueries));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GiftReceiptReportDto>> GetReceiptsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        int? workstationId = null,
        GiftReceiptFilter filter = GiftReceiptFilter.All,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<GiftReceiptReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<GiftReceiptReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetReceiptsAsync(
            fromDate,
            toDate,
            organizationId,
            workstationId,
            filter,
            cancellationToken);

        return Result.Success(report);
    }
}
