using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace ParkGate.WinForms.Services;

public sealed class FormFactory : IFormFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FormFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TForm Create<TForm>() where TForm : Form
    {
        return _serviceProvider.GetRequiredService<TForm>();
    }

    public TControl CreateControl<TControl>() where TControl : Control
    {
        return _serviceProvider.GetRequiredService<TControl>();
    }
}
