using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Forms;

partial class IssueQrDialog
{
    private IContainer components = null;
    private PictureBox _picQrCode;
    private Label _lblInstruction;
    private Button _btnPrint;
    private Button _btnSave;
    private Button _btnClose;
    private Label _lblCustomerName;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _picQrCode?.Image?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _picQrCode = new PictureBox();
        _lblInstruction = new Label();
        _btnPrint = new Button();
        _btnSave = new Button();
        _btnClose = new Button();
        _lblCustomerName = new Label();
        ((ISupportInitialize)_picQrCode).BeginInit();
        SuspendLayout();
        
        // _lblCustomerName
        _lblCustomerName.Dock = DockStyle.Top;
        _lblCustomerName.Font = new Font("Cairo", 12F, FontStyle.Bold);
        _lblCustomerName.ForeColor = Theme.Primary;
        _lblCustomerName.Location = new Point(20, 20);
        _lblCustomerName.Name = "_lblCustomerName";
        _lblCustomerName.Size = new Size(350, 40);
        _lblCustomerName.TabIndex = 0;
        _lblCustomerName.Text = "اسم الزبون";
        _lblCustomerName.TextAlign = ContentAlignment.MiddleCenter;
        
        // _picQrCode
        _picQrCode.Location = new Point(70, 70);
        _picQrCode.Name = "_picQrCode";
        _picQrCode.Size = new Size(250, 250);
        _picQrCode.SizeMode = PictureBoxSizeMode.Zoom;
        _picQrCode.TabIndex = 1;
        _picQrCode.TabStop = false;
        
        // _lblInstruction
        _lblInstruction.Location = new Point(20, 330);
        _lblInstruction.Name = "_lblInstruction";
        _lblInstruction.Size = new Size(350, 30);
        _lblInstruction.TabIndex = 2;
        _lblInstruction.Text = "هذا الرمز مخصص للاستخدام عبر تطبيق البوابة";
        _lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
        
        // _btnPrint
        _btnPrint.Location = new Point(255, 375);
        _btnPrint.Name = "_btnPrint";
        _btnPrint.Size = new Size(95, 38);
        _btnPrint.TabIndex = 3;
        _btnPrint.Text = "طباعة";
        _btnPrint.UseVisualStyleBackColor = true;

        // _btnSave
        _btnSave.Location = new Point(148, 375);
        _btnSave.Name = "_btnSave";
        _btnSave.Size = new Size(95, 38);
        _btnSave.TabIndex = 4;
        _btnSave.Text = "حفظ";
        _btnSave.UseVisualStyleBackColor = true;

        // _btnClose
        _btnClose.Location = new Point(40, 375);
        _btnClose.Name = "_btnClose";
        _btnClose.Size = new Size(95, 38);
        _btnClose.TabIndex = 5;
        _btnClose.Text = "إغلاق";
        _btnClose.UseVisualStyleBackColor = true;
        
        // IssueQrDialog
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Theme.Surface;
        ClientSize = new Size(390, 435);
        Controls.Add(_btnPrint);
        Controls.Add(_btnSave);
        Controls.Add(_btnClose);
        Controls.Add(_lblInstruction);
        Controls.Add(_picQrCode);
        Controls.Add(_lblCustomerName);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "IssueQrDialog";
        Padding = new Padding(20);
        StartPosition = FormStartPosition.CenterParent;
        Text = "إصدار بطاقة QR";
        ((ISupportInitialize)_picQrCode).EndInit();
        ResumeLayout(false);
    }
}
