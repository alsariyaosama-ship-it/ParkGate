namespace ParkGate.WinForms.Services;

public interface INotifyService
{
    void Info(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Error(string message, string? title = null);
    bool Confirm(string message, string? title = null);
}
