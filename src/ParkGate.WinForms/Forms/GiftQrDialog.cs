using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.EntryCards;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog displaying an issued gift voucher with signed QR code, printing and export options (T-G8).
/// </summary>
public partial class GiftQrDialog : RtlFormBase
{
    private readonly IQrImageRenderer? _qrRenderer;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<GiftQrDialog>? _logger;

    private IssuedGiftVoucherDto? _voucher;

    public GiftQrDialog() : this(null, null, null)
    {
    }

    public GiftQrDialog(
        IQrImageRenderer? qrRenderer,
        INotifyService? notifyService,
        ILogger<GiftQrDialog>? logger)
    {
        InitializeComponent();

        _qrRenderer = qrRenderer;
        _notifyService = notifyService;
        _logger = logger;

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

        if (_qrRenderer != null && !string.IsNullOrWhiteSpace(voucher.QrText))
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
                _logger?.LogError(ex, "Failed to render gift voucher QR image.");
                _notifyService?.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void SaveQrImage()
    {
        if (_picQr.Image == null || _voucher == null)
        {
            _notifyService?.Warning(Strings.Msg_NoQrImageAvailable);
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
                _notifyService?.Info(Strings.Msg_SaveImageSuccess);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save QR image to file.");
                _notifyService?.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void PrintQrVoucher()
    {
        if (_picQr.Image == null || _voucher == null)
        {
            _notifyService?.Warning(Strings.Msg_NoQrImageAvailable);
            return;
        }

        try
        {
            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                _notifyService?.Warning(Strings.Msg_NoPrinterInstalled, Strings.Title_Warning);
                return;
            }

            using var doc = new PrintDocument();
            if (!doc.PrinterSettings.IsValid && PrinterSettings.InstalledPrinters.Count > 0)
            {
                doc.PrinterSettings.PrinterName = PrinterSettings.InstalledPrinters[0];
            }
            doc.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                if (g == null)
                {
                    return;
                }

                using var titleFont = new Font("Cairo", 12F, FontStyle.Bold);
                using var boldFont = new Font("Cairo", 10F, FontStyle.Bold);
                using var regularFont = new Font("Cairo", 9F, FontStyle.Regular);
                using var smallFont = new Font("Cairo", 8.5F, FontStyle.Regular);

                // Card rectangle (approx A6 receipt card)
                var cardRect = new Rectangle(40, 40, 360, 220);
                using var borderPen = new Pen(Color.FromArgb(27, 135, 63), 2);
                using var fillBrush = new SolidBrush(Color.White);
                g.FillRectangle(fillBrush, cardRect);
                g.DrawRectangle(borderPen, cardRect);

                // Header banner
                var headerRect = new Rectangle(cardRect.Left, cardRect.Top, cardRect.Width, 36);
                using var headerBrush = new SolidBrush(Color.FromArgb(27, 135, 63));
                g.FillRectangle(headerBrush, headerRect);

                using var sfCenter = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(Strings.Header_GiftVoucher, titleFont, Brushes.White, headerRect, sfCenter);

                // Customer info on the right (RTL layout)
                var infoRect = new Rectangle(cardRect.Left + 160, cardRect.Top + 44, 190, 165);
                using var sfRtl = new StringFormat
                {
                    FormatFlags = StringFormatFlags.DirectionRightToLeft,
                    Alignment = StringAlignment.Near
                };

                g.DrawString(string.Format(Strings.Label_ReceiptNo, _voucher.ReceiptNo), boldFont, Brushes.DarkGreen, infoRect, sfRtl);

                infoRect.Y += 26;
                g.DrawString($"{Strings.Col_BeneficiaryName}: {_voucher.HolderName}", regularFont, Brushes.Black, infoRect, sfRtl);

                infoRect.Y += 24;
                g.DrawString($"{Strings.Label_CardNumber}: {_voucher.CardNumber}", regularFont, Brushes.Black, infoRect, sfRtl);

                infoRect.Y += 24;
                g.DrawString($"{Strings.Label_Organization}: {_voucher.OrganizationName}", smallFont, Brushes.DimGray, infoRect, sfRtl);

                infoRect.Y += 24;
                g.DrawString(string.Format(Strings.Label_GiftValueDisplay, _voucher.Value, Strings.Unit_Currency), boldFont, Brushes.Black, infoRect, sfRtl);

                infoRect.Y += 24;
                g.DrawString(string.Format(Strings.Label_GiftExpiresOn, _voucher.ExpiresOn.ToDateTime(TimeOnly.MinValue)), smallFont, Brushes.DimGray, infoRect, sfRtl);

                if (_voucher.IsOverride)
                {
                    infoRect.Y += 20;
                    g.DrawString(Strings.Label_GiftOverrideBadge, smallFont, Brushes.DarkOrange, infoRect, sfRtl);
                }

                // QR Code on the left
                var qrRect = new Rectangle(cardRect.Left + 15, cardRect.Top + 48, 140, 140);
                g.DrawImage(_picQr.Image, qrRect);
            };

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
            _logger?.LogError(ex, "Failed to print gift voucher receipt.");
            _notifyService?.Error(string.Format(Strings.Msg_PrintFailed, UserMessages.Describe(ex)));
        }
    }
}
