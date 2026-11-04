using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;

namespace ParkGate.Application.GiftVouchers.Services;

public interface IGiftReportService
{
    Task<Result<GiftReceiptReportDto>> GetReceiptsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        int? workstationId = null,
        GiftReceiptFilter filter = GiftReceiptFilter.All,
        CancellationToken cancellationToken = default);
}
