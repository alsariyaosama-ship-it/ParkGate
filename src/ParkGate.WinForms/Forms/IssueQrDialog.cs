using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.EntryCards;
using ParkGate.Infrastructure.EntryCards;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

public partial class IssueQrDialog : RtlFormBase
{
    private readonly EntryCardIssuer? _issuer;
    private readonly INotifyService? _notifyService;
    private BeneficiaryDto? _beneficiary;

    public IssueQrDialog() : this(null, null)
    {
    }

    public IssueQrDialog(EntryCardIssuer? issuer, INotifyService? notifyService)
    {
        InitializeComponent();
        _issuer = issuer;
        _notifyService = notifyService;

        Theme.ApplyPrimaryButtonStyle(_btnClose);
        _btnClose.Click += (_, _) => Close();
    }

    public async Task LoadAndShowAsync(BeneficiaryDto beneficiary)
    {
        _beneficiary = beneficiary;
        _lblCustomerName.Text = beneficiary.FullName;

        if (_issuer == null)
        {
            _notifyService?.Error("خدمة إصدار البطاقات غير متوفرة.");
            return;
        }

        try
        {
            var card = await _issuer.IssueAsync(beneficiary.Id);
            
            // Generate QR Image using QrImageRenderer
            byte[] qrPngBytes = QrImageRenderer.RenderPng(card.QrText);
            
            using var ms = new MemoryStream(qrPngBytes);
            _picQrCode.Image = Image.FromStream(ms);
        }
        catch (Exception ex)
        {
            _notifyService?.Error("حدث خطأ غير متوقع أثناء إنتاج الرمز: " + ex.Message, "خطأ");
            _picQrCode.Image = null;
        }
    }
}
