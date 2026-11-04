using ParkGate.Application.GiftVouchers.DTOs;

namespace ParkGate.Application.Ports.Reports;

/// <summary>
/// Port for exporting gift voucher receipt reports to formatted Excel (.xlsx) workbooks with RTL sheets and styled tables.
/// </summary>
public interface IGiftExcelExportService
{
    byte[] ExportGiftReceipts(GiftReceiptReportDto report, string? timeZoneId = null);
}
