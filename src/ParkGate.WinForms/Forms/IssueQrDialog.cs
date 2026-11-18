using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.Application.Settings.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Printing;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

public partial class IssueQrDialog : RtlFormBase
{
    private readonly IEntryCardIssuer _issuer;
    private readonly IQrImageRenderer _qrRenderer;
    private readonly IOrganizationService _organizationService;
    private readonly ISettingsService _settingsService;
    private readonly INotifyService _notifyService;
    private BeneficiaryDto? _beneficiary;
    private IssuedEntryCard? _issuedCard;
    private int _cooldownMinutes;

    public IssueQrDialog(
        IEntryCardIssuer issuer,
        IQrImageRenderer qrRenderer,
        IOrganizationService organizationService,
        ISettingsService settingsService,
        INotifyService notifyService)
    {
        InitializeComponent();
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _qrRenderer = qrRenderer ?? throw new ArgumentNullException(nameof(qrRenderer));
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));

        Theme.ApplyPrimaryButtonStyle(_btnPrint);
        Theme.ApplySecondaryButtonStyle(_btnSave);
        Theme.ApplySecondaryButtonStyle(_btnClose);

        _btnClose.Click += (_, _) => Close();
        _btnSave.Click += (_, _) => SaveQrImage();
        _btnPrint.Click += (_, _) => PrintQrCard();
    }

    public async Task LoadAndShowAsync(BeneficiaryDto beneficiary)
    {
        _beneficiary = beneficiary;
        _lblCustomerName.Text = beneficiary.FullName;

        try
        {
            _lblInstruction.Text = Strings.Ui_IssueQr_02;
            var card = await _issuer.IssueAsync(beneficiary.Id);
            _issuedCard = card;
            _cooldownMinutes = await GetCooldownMinutesAsync(beneficiary.OrganizationId);

            byte[] qrPngBytes = _qrRenderer.RenderPng(card.QrText);

            using var ms = new MemoryStream(qrPngBytes);
            using var rawImage = Image.FromStream(ms);
            _picQrCode.Image?.Dispose();
            _picQrCode.Image = new Bitmap(rawImage);

            _lblInstruction.Text = Strings.Ui_IssueQr_03;
        }
        catch (Exception ex)
        {
            _notifyService.Error($"{Strings.Msg_QrGenerationFailed} {UserMessages.LogAndDescribe(ex, "Generate QR card")}", Strings.Title_Error);
            _lblInstruction.Text = Strings.Ui_IssueQr_04;
            _picQrCode.Image?.Dispose();
            _picQrCode.Image = null;
        }
    }

    /// <summary>The card's printed rule: the organization's waiting period, else the park default (BR-03).</summary>
    private async Task<int> GetCooldownMinutesAsync(int organizationId)
    {
        var organization = await _organizationService.GetByIdAsync(organizationId);
        if (organization.IsSuccess && organization.Value.CooldownMinutes is int minutes)
        {
            return minutes;
        }

        return (await _settingsService.GetSettingsAsync()).DefaultCooldownMinutes;
    }

    private void SaveQrImage()
    {
        if (_picQrCode.Image == null)
        {
            _notifyService.Warning(Strings.Ui_IssueQr_05);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Title = Strings.Ui_IssueQr_06,
            Filter = "PNG Image (*.png)|*.png",
            FileName = $"QR_{_beneficiary?.CardNumber ?? "Card"}.png"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _picQrCode.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                _notifyService.Info(Strings.Ui_IssueQr_07);
            }
            catch (Exception ex)
            {
                _notifyService.Error($"{Strings.Msg_QrSaveFailed} {UserMessages.LogAndDescribe(ex, "Save QR image")}");
            }
        }
    }

    private void PrintQrCard()
    {
        if (_picQrCode.Image == null || _beneficiary == null)
        {
            _notifyService.Warning(Strings.Ui_IssueQr_08);
            return;
        }

        try
        {
            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                _notifyService.Warning(Strings.Msg_NoPrinterInstalled, Strings.Title_Warning);
                return;
            }

            using var doc = ReceiptPrinter.CreateDocument(ReceiptSlips.EntryCard(_beneficiary.OrganizationName, _cooldownMinutes), _picQrCode.Image);
            if (!doc.PrinterSettings.IsValid && PrinterSettings.InstalledPrinters.Count > 0)
            {
                doc.PrinterSettings.PrinterName = PrinterSettings.InstalledPrinters[0];
            }

            using var dlg = new PrintDialog
            {
                Document = doc,
                UseEXDialog = false,
                AllowSomePages = false,
                AllowSelection = false
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                doc.Print();
            }
        }
        catch (Exception ex)
        {
            _notifyService.Error(string.Format(Strings.Msg_PrintFailed, UserMessages.LogAndDescribe(ex, "Print QR card")), Strings.Title_Error);
        }
    }
}
