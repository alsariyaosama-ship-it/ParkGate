using System.Drawing.Printing;
using Microsoft.Extensions.Logging;

namespace ParkGate.WinForms.Printing;

/// <summary>Silent printing to the Windows default printer (BR-33). The slip has no personal data, and neither does the log.</summary>
public sealed class EntryReceiptPrinter : IEntryReceiptPrinter
{
    private readonly ILogger<EntryReceiptPrinter> _logger;

    public EntryReceiptPrinter(ILogger<EntryReceiptPrinter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool TryPrint(ReceiptSlip slip)
    {
        try
        {
            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                _logger.LogWarning("Entry receipt not printed: no printer is installed.");
                return false;
            }

            using var document = ReceiptPrinter.CreateDocument(slip, qr: null);
            if (!document.PrinterSettings.IsValid)
            {
                _logger.LogWarning("Entry receipt not printed: the default printer is not valid.");
                return false;
            }

            // The standard controller prints without the "printing..." progress dialog.
            document.PrintController = new StandardPrintController();
            document.Print();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Entry receipt printing failed.");
            return false;
        }
    }
}
