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
        _lblCustomerName.Size = new Size(310, 40);
        _lblCustomerName.TabIndex = 0;
        _lblCustomerName.Text = "اسم الزبون";
        _lblCustomerName.TextAlign = ContentAlignment.MiddleCenter;
        
        // _picQrCode
        _picQrCode.Location = new Point(50, 70);
        _picQrCode.Name = "_picQrCode";
        _picQrCode.Size = new Size(250, 250);
        _picQrCode.SizeMode = PictureBoxSizeMode.Zoom;
        _picQrCode.TabIndex = 1;
        _picQrCode.TabStop = false;
        
        // _lblInstruction
        _lblInstruction.Location = new Point(20, 330);
        _lblInstruction.Name = "_lblInstruction";
        _lblInstruction.Size = new Size(310, 30);
        _lblInstruction.TabIndex = 2;
        _lblInstruction.Text = "هذا الرمز مخصص للاستخدام عبر تطبيق البوابة";
        _lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
        
        // _btnClose
        _btnClose.Location = new Point(125, 370);
        _btnClose.Name = "_btnClose";
        _btnClose.Size = new Size(100, 40);
        _btnClose.TabIndex = 3;
        _btnClose.Text = "إغلاق";
        _btnClose.UseVisualStyleBackColor = true;
        
        // IssueQrDialog
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Theme.Surface;
        ClientSize = new Size(350, 430);
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
