using ParkGate.Application.GiftVouchers.DTOs;

namespace ParkGate.Application.Ports.Reports;

public interface IGiftReportQueries
{
    Task<GiftReceiptReportDto> GetReceiptsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        int? workstationId = null,
        GiftReceiptFilter filter = GiftReceiptFilter.All,
        CancellationToken cancellationToken = default);
}
