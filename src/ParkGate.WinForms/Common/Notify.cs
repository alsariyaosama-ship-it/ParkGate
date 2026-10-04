using System.Windows.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Helper for presenting RTL-compliant message boxes with Arabic text.
/// </summary>
public static class Notify
{
    private const MessageBoxOptions Options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

    public static void Info(string message, string? title = null, IWin32Window? owner = null)
    {
        MessageBox.Show(
            owner,
            message,
            title ?? Strings.Title_Info,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1,
            Options);
    }

    public static void Warning(string message, string? title = null, IWin32Window? owner = null)
    {
        MessageBox.Show(
            owner,
            message,
            title ?? Strings.Title_Warning,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button1,
            Options);
    }

    public static void Error(string message, string? title = null, IWin32Window? owner = null)
    {
        MessageBox.Show(
            owner,
            message,
            title ?? Strings.Title_Error,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1,
            Options);
    }

    public static bool Confirm(string message, string? title = null, IWin32Window? owner = null)
    {
        return MessageBox.Show(
            owner,
            message,
            title ?? Strings.Title_Confirm,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            Options) == DialogResult.Yes;
    }
}
