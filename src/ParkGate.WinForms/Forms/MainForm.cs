using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

public partial class MainForm : RtlFormBase
{
    public MainForm()
    {
        InitializeComponent();
        Text = Strings.App_Title;
    }
}
