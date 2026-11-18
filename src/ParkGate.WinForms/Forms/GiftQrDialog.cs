using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Printing;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog displaying an issued gift voucher with signed QR code, printing and export options (T-G8).
/// </summary>
public partial class GiftQrDialog : RtlFormBase
{
    private readonly IQrImageRenderer _qrRenderer;
    private readonly INotifyService _notifyService;
    private readonly ILogger<GiftQrDialog> _logger;

    private IssuedGiftVoucherDto? _voucher;

    public GiftQrDialog(
        IQrImageRenderer qrRenderer,
        INotifyService notifyService,
        ILogger<GiftQrDialog> logger)
    {
        InitializeComponent();

        _qrRenderer = qrRenderer ?? throw new ArgumentNullException(nameof(qrRenderer));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_GiftQrDialog;
        _lblHeader.Text = Strings.Header_GiftVoucher;
        _btnPrint.Text = Strings.Btn_Print;
        _btnSave.Text = Strings.Btn_SaveImage;
        _btnClose.Text = Strings.Btn_Close;
        _lblOverrideNotice.Text = Strings.Label_GiftOverrideBadge;
    }

    private void WireEvents()
    {
        _btnPrint.Click += (_, _) => PrintQrVoucher();
        _btnSave.Click += (_, _) => SaveQrImage();
        _btnClose.Click += (_, _) => Close();
    }

    public void SetVoucher(IssuedGiftVoucherDto voucher)
    {
        _voucher = voucher;

        _lblReceiptNo.Text = string.Format(Strings.Label_ReceiptNo, voucher.ReceiptNo);
        _lblCustomerName.Text = $"{Strings.Col_BeneficiaryName}: {voucher.HolderName}";
        _lblCardNumber.Text = $"{Strings.Label_CardNumber}: {voucher.CardNumber}";
        _lblOrganization.Text = $"{Strings.Label_Organization}: {voucher.OrganizationName}";
        _lblValue.Text = string.Format(Strings.Label_GiftValueDisplay, voucher.Value, Strings.Unit_Currency);
        _lblExpiresOn.Text = string.Format(Strings.Label_GiftExpiresOn, voucher.ExpiresOn.ToDateTime(TimeOnly.MinValue));
        _lblOverrideNotice.Visible = voucher.IsOverride;

        if (!string.IsNullOrWhiteSpace(voucher.QrText))
        {
            try
            {
                var pngBytes = _qrRenderer.RenderPng(voucher.QrText, 10);
                using var ms = new MemoryStream(pngBytes);
                using var rawImage = Image.FromStream(ms);

                _picQr.Image?.Dispose();
                _picQr.Image = new Bitmap(rawImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render gift voucher QR image.");
                _notifyService.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void SaveQrImage()
    {
        if (_picQr.Image == null || _voucher == null)
        {
            _notifyService.Warning(Strings.Msg_NoQrImageAvailable);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Title = Strings.Btn_SaveImage,
            Filter = Strings.Filter_PngImages,
            FileName = $"GiftReceipt_{_voucher.ReceiptNo}_{_voucher.CardNumber}.png"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _picQr.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                _notifyService.Info(Strings.Msg_SaveImageSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save QR image to file.");
                _notifyService.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void PrintQrVoucher()
    {
        if (_picQr.Image == null || _voucher == null)
        {
            _notifyService.Warning(Strings.Msg_NoQrImageAvailable);
            return;
        }

        try
        {
            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                _notifyService.Warning(Strings.Msg_NoPrinterInstalled, Strings.Title_Warning);
                return;
            }

            using var doc = ReceiptPrinter.CreateDocument(ReceiptSlips.GiftVoucher(_voucher.OrganizationName, _voucher.ReceiptNo), _picQr.Image);
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
            _logger.LogError(ex, "Failed to print gift voucher receipt.");
            _notifyService.Error(string.Format(Strings.Msg_PrintFailed, UserMessages.Describe(ex)));
        }
    }
}
