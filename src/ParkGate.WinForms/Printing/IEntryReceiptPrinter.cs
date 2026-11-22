namespace ParkGate.WinForms.Printing;

/// <summary>
/// Prints the entry receipt on the desk's default printer without any dialog (BR-33). A busy desk cannot stop for a
/// printer problem, so failure is a return value, never an exception.
/// </summary>
public interface IEntryReceiptPrinter
{
    /// <summary>True when the slip was handed to the printer; false when there is no usable printer or printing failed.</summary>
    bool TryPrint(ReceiptSlip slip);
}
