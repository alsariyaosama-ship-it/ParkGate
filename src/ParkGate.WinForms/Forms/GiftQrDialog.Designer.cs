#nullable disable

namespace ParkGate.WinForms.Forms;

partial class GiftQrDialog
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._tblMain = new System.Windows.Forms.TableLayoutPanel();
        this._picQr = new System.Windows.Forms.PictureBox();
        this._tblDetails = new System.Windows.Forms.TableLayoutPanel();
        this._lblReceiptNo = new System.Windows.Forms.Label();
        this._lblCustomerName = new System.Windows.Forms.Label();
        this._lblCardNumber = new System.Windows.Forms.Label();
        this._lblOrganization = new System.Windows.Forms.Label();
        this._lblValue = new System.Windows.Forms.Label();
        this._lblExpiresOn = new System.Windows.Forms.Label();
        this._lblOverrideNotice = new System.Windows.Forms.Label();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnPrint = new System.Windows.Forms.Button();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._picQr)).BeginInit();
        this._tblDetails.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(540, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
        this._lblHeader.Size = new System.Drawing.Size(540, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "وصل قسيمة ألعاب الأطفال";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblMain
        // 
        this._tblMain.ColumnCount = 2;
        this._tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
        this._tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblMain.Controls.Add(this._picQr, 0, 0);
        this._tblMain.Controls.Add(this._tblDetails, 1, 0);
        this._tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblMain.Location = new System.Drawing.Point(0, 50);
        this._tblMain.Name = "_tblMain";
        this._tblMain.Padding = new System.Windows.Forms.Padding(16);
        this._tblMain.RowCount = 1;
        this._tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblMain.Size = new System.Drawing.Size(540, 240);
        this._tblMain.TabIndex = 1;
        // 
        // _picQr
        // 
        this._picQr.BackColor = System.Drawing.Color.White;
        this._picQr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._picQr.Dock = System.Windows.Forms.DockStyle.Fill;
        this._picQr.Location = new System.Drawing.Point(317, 19);
        this._picQr.Name = "_picQr";
        this._picQr.Size = new System.Drawing.Size(204, 202);
        this._picQr.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this._picQr.TabIndex = 0;
        this._picQr.TabStop = false;
        // 
        // _tblDetails
        // 
        this._tblDetails.ColumnCount = 1;
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblDetails.Controls.Add(this._lblReceiptNo, 0, 0);
        this._tblDetails.Controls.Add(this._lblCustomerName, 0, 1);
        this._tblDetails.Controls.Add(this._lblCardNumber, 0, 2);
        this._tblDetails.Controls.Add(this._lblOrganization, 0, 3);
        this._tblDetails.Controls.Add(this._lblValue, 0, 4);
        this._tblDetails.Controls.Add(this._lblExpiresOn, 0, 5);
        this._tblDetails.Controls.Add(this._lblOverrideNotice, 0, 6);
        this._tblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblDetails.Location = new System.Drawing.Point(19, 19);
        this._tblDetails.Name = "_tblDetails";
        this._tblDetails.RowCount = 7;
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this._tblDetails.Size = new System.Drawing.Size(292, 202);
        this._tblDetails.TabIndex = 1;
        // 
        // _lblReceiptNo
        // 
        this._lblReceiptNo.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblReceiptNo.AutoSize = true;
        this._lblReceiptNo.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblReceiptNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblReceiptNo.Location = new System.Drawing.Point(180, 2);
        this._lblReceiptNo.Name = "_lblReceiptNo";
        this._lblReceiptNo.Size = new System.Drawing.Size(109, 26);
        this._lblReceiptNo.TabIndex = 0;
        this._lblReceiptNo.Text = "رقم الوصل: -";
        // 
        // _lblCustomerName
        // 
        this._lblCustomerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCustomerName.AutoSize = true;
        this._lblCustomerName.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblCustomerName.Location = new System.Drawing.Point(220, 32);
        this._lblCustomerName.Name = "_lblCustomerName";
        this._lblCustomerName.Size = new System.Drawing.Size(69, 24);
        this._lblCustomerName.TabIndex = 1;
        this._lblCustomerName.Text = "الزبون: -";
        // 
        // _lblCardNumber
        // 
        this._lblCardNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCardNumber.AutoSize = true;
        this._lblCardNumber.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblCardNumber.Location = new System.Drawing.Point(198, 60);
        this._lblCardNumber.Name = "_lblCardNumber";
        this._lblCardNumber.Size = new System.Drawing.Size(91, 23);
        this._lblCardNumber.TabIndex = 2;
        this._lblCardNumber.Text = "رقم البطاقة: -";
        // 
        // _lblOrganization
        // 
        this._lblOrganization.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblOrganization.AutoSize = true;
        this._lblOrganization.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblOrganization.Location = new System.Drawing.Point(234, 88);
        this._lblOrganization.Name = "_lblOrganization";
        this._lblOrganization.Size = new System.Drawing.Size(55, 23);
        this._lblOrganization.TabIndex = 3;
        this._lblOrganization.Text = "الجهة: -";
        // 
        // _lblValue
        // 
        this._lblValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblValue.AutoSize = true;
        this._lblValue.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this._lblValue.Location = new System.Drawing.Point(190, 116);
        this._lblValue.Name = "_lblValue";
        this._lblValue.Size = new System.Drawing.Size(99, 26);
        this._lblValue.TabIndex = 4;
        this._lblValue.Text = "قيمة القسيمة: -";
        // 
        // _lblExpiresOn
        // 
        this._lblExpiresOn.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblExpiresOn.AutoSize = true;
        this._lblExpiresOn.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblExpiresOn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblExpiresOn.Location = new System.Drawing.Point(196, 146);
        this._lblExpiresOn.Name = "_lblExpiresOn";
        this._lblExpiresOn.Size = new System.Drawing.Size(93, 23);
        this._lblExpiresOn.TabIndex = 5;
        this._lblExpiresOn.Text = "صالح حتى اليوم";
        // 
        // _lblOverrideNotice
        // 
        this._lblOverrideNotice.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblOverrideNotice.AutoSize = true;
        this._lblOverrideNotice.Font = new System.Drawing.Font("Cairo", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblOverrideNotice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._lblOverrideNotice.Location = new System.Drawing.Point(75, 175);
        this._lblOverrideNotice.Name = "_lblOverrideNotice";
        this._lblOverrideNotice.Size = new System.Drawing.Size(214, 22);
        this._lblOverrideNotice.TabIndex = 6;
        this._lblOverrideNotice.Text = "(إصدار استثنائي بموافقة المشرف)";
        this._lblOverrideNotice.Visible = false;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlButtons.Controls.Add(this._btnPrint);
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnClose);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 290);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(540, 56);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnPrint
        // 
        this._btnPrint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnPrint.FlatAppearance.BorderSize = 0;
        this._btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnPrint.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnPrint.ForeColor = System.Drawing.Color.White;
        this._btnPrint.Location = new System.Drawing.Point(398, 9);
        this._btnPrint.Name = "_btnPrint";
        this._btnPrint.Size = new System.Drawing.Size(110, 38);
        this._btnPrint.TabIndex = 0;
        this._btnPrint.Text = "طباعة (Enter)";
        this._btnPrint.UseVisualStyleBackColor = false;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnSave.Location = new System.Drawing.Point(272, 9);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(120, 38);
        this._btnSave.TabIndex = 1;
        this._btnSave.Text = "حفظ كصورة...";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnClose
        // 
        this._btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClose.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnClose.Location = new System.Drawing.Point(166, 9);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(100, 38);
        this._btnClose.TabIndex = 2;
        this._btnClose.Text = "إغلاق";
        this._btnClose.UseVisualStyleBackColor = false;
        // 
        // GiftQrDialog
        // 
        this.AcceptButton = this._btnPrint;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnClose;
        this.ClientSize = new System.Drawing.Size(540, 346);
        this.Controls.Add(this._tblMain);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GiftQrDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "قسيمة هدية الألعاب";
        this._pnlHeader.ResumeLayout(false);
        this._tblMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._picQr)).EndInit();
        this._tblDetails.ResumeLayout(false);
        this._tblDetails.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblMain;
    private System.Windows.Forms.PictureBox _picQr;
    private System.Windows.Forms.TableLayoutPanel _tblDetails;
    private System.Windows.Forms.Label _lblReceiptNo;
    private System.Windows.Forms.Label _lblCustomerName;
    private System.Windows.Forms.Label _lblCardNumber;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.Label _lblValue;
    private System.Windows.Forms.Label _lblExpiresOn;
    private System.Windows.Forms.Label _lblOverrideNotice;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnPrint;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnClose;
}
