using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Services;

public sealed class NotifyService : INotifyService
{
    public void Info(string message, string? title = null) => Notify.Info(message, title);
    public void Warning(string message, string? title = null) => Notify.Warning(message, title);
    public void Error(string message, string? title = null) => Notify.Error(message, title);
    public bool Confirm(string message, string? title = null) => Notify.Confirm(message, title);
}
