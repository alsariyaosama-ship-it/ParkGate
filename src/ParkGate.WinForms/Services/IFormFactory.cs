using System.Windows.Forms;

namespace ParkGate.WinForms.Services;

/// <summary>
/// Factory for creating forms with resolved dependencies from the DI container.
/// </summary>
public interface IFormFactory
{
    TForm Create<TForm>() where TForm : Form;
    TControl CreateControl<TControl>() where TControl : Control;
}
