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
    private IssuedEntryCard? _issuedCard;

    public IssueQrDialog() : this(null, null)
    {
    }

    public IssueQrDialog(EntryCardIssuer? issuer, INotifyService? notifyService)
    {
        InitializeComponent();
        _issuer = issuer;
        _notifyService = notifyService;

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

        if (_issuer == null)
        {
            _notifyService?.Error("خدمة إصدار البطاقات غير متوفرة.");
            return;
        }

        try
        {
            _lblInstruction.Text = "جاري إصدار وتوقيع الرمز الرقمي...";
            var card = await _issuer.IssueAsync(beneficiary.Id);
            _issuedCard = card;
            
            // Generate QR Image using QrImageRenderer
            byte[] qrPngBytes = QrImageRenderer.RenderPng(card.QrText);
            
            using var ms = new MemoryStream(qrPngBytes);
            using var rawImage = Image.FromStream(ms);
            _picQrCode.Image?.Dispose();
            _picQrCode.Image = new Bitmap(rawImage);

            _lblInstruction.Text = "هذا الرمز مخصص للاستخدام عبر تطبيق البوابة";
        }
        catch (Exception ex)
        {
            _notifyService?.Error("حدث خطأ غير متوقع أثناء إنتاج الرمز: " + ex.Message, "خطأ");
            _lblInstruction.Text = "فشل إصدار الرمز";
            _picQrCode.Image?.Dispose();
            _picQrCode.Image = null;
        }
    }

    private void SaveQrImage()
    {
        if (_picQrCode.Image == null)
        {
            _notifyService?.Warning("لا يوجد رمز QR متاح للحفظ.");
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Title = "حفظ رمز QR",
            Filter = "PNG Image (*.png)|*.png",
            FileName = $"QR_{_beneficiary?.CardNumber ?? "Card"}.png"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _picQrCode.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                _notifyService?.Info("تم حفظ صورة رمز QR بنجاح.");
            }
            catch (Exception ex)
            {
                _notifyService?.Error("تعذر حفظ الصورة: " + ex.Message);
            }
        }
    }

    private void PrintQrCard()
    {
        if (_picQrCode.Image == null || _beneficiary == null)
        {
            _notifyService?.Warning("لا توجد بطاقة جاهزة للطباعة.");
            return;
        }

        try
        {
            using var doc = new System.Drawing.Printing.PrintDocument();
            doc.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                if (g == null)
                {
                    return;
                }

                using var subFont = AppFont.CreateFont(10F, FontStyle.Bold);
                using var regFont = AppFont.CreateFont(9F, FontStyle.Regular);
                using var smallFont = AppFont.CreateFont(8F, FontStyle.Regular);

                // Draw card frame (85mm x 54mm approx in 100dpi is ~320x200)
                var cardRect = new Rectangle(50, 50, 320, 190);
                using var borderPen = new Pen(Theme.Primary, 2);
                using var fillBrush = new SolidBrush(Color.White);
                g.FillRectangle(fillBrush, cardRect);
                g.DrawRectangle(borderPen, cardRect);

                // Header
                var headerRect = new Rectangle(cardRect.Left, cardRect.Top, cardRect.Width, 32);
                using var headerBrush = new SolidBrush(Theme.Primary);
                g.FillRectangle(headerBrush, headerRect);

                using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("الحديقة العائلية الترفيهية — بطاقة دخول", regFont, Brushes.White, headerRect, sfCenter);

                // Customer info on the right (RTL layout)
                var infoRect = new Rectangle(cardRect.Left + 140, cardRect.Top + 40, 170, 140);
                using var sfRtl = new StringFormat { FormatFlags = StringFormatFlags.DirectionRightToLeft };
                g.DrawString($"الاسم: {_beneficiary.FullName}", subFont, Brushes.Black, infoRect, sfRtl);

                infoRect.Y += 28;
                g.DrawString($"رقم البطاقة: {_beneficiary.CardNumber}", regFont, Brushes.Black, infoRect, sfRtl);

                infoRect.Y += 26;
                g.DrawString($"الجهة: {_beneficiary.OrganizationName}", smallFont, Brushes.DarkSlateGray, infoRect, sfRtl);

                if (_issuedCard != null)
                {
                    infoRect.Y += 24;
                    g.DrawString($"الصلاحية: {_issuedCard.ExpiresOn:dd/MM/yyyy}", smallFont, Brushes.DarkSlateGray, infoRect, sfRtl);
                }

                // QR Code on the left
                var qrRect = new Rectangle(cardRect.Left + 15, cardRect.Top + 45, 120, 120);
                g.DrawImage(_picQrCode.Image, qrRect);
            };

            using var dlg = new PrintDialog { Document = doc, UseEXDialog = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                doc.Print();
            }
        }
        catch (Exception ex)
        {
            _notifyService?.Error("تعذر إرسال أمر الطباعة: " + ex.Message, "خطأ");
        }
    }
}
