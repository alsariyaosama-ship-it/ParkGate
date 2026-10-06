#nullable disable

namespace ParkGate.WinForms.Forms;

partial class EditBeneficiaryDialog
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
        this._tblLayout = new System.Windows.Forms.TableLayoutPanel();
        this._lblFullName = new System.Windows.Forms.Label();
        this._txtFullName = new System.Windows.Forms.TextBox();
        this._lblOrganization = new System.Windows.Forms.Label();
        this._cboOrganization = new System.Windows.Forms.ComboBox();
        this._lblCardNumber = new System.Windows.Forms.Label();
        this._txtCardNumber = new System.Windows.Forms.TextBox();
        this._lblPhone = new System.Windows.Forms.Label();
        this._txtPhone = new System.Windows.Forms.TextBox();
        this._lblNationalId = new System.Windows.Forms.Label();
        this._txtNationalId = new System.Windows.Forms.TextBox();
        this._lblExpiryDate = new System.Windows.Forms.Label();
        this._pnlExpiry = new System.Windows.Forms.FlowLayoutPanel();
        this._chkHasExpiry = new System.Windows.Forms.CheckBox();
        this._dtpExpiryDate = new System.Windows.Forms.DateTimePicker();
        this._lblNotes = new System.Windows.Forms.Label();
        this._txtNotes = new System.Windows.Forms.TextBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        this._pnlExpiry.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(520, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        this._lblHeader.Size = new System.Drawing.Size(520, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "تعديل بيانات حامل البطاقة";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblFullName, 0, 0);
        this._tblLayout.Controls.Add(this._txtFullName, 1, 0);
        this._tblLayout.Controls.Add(this._lblOrganization, 0, 1);
        this._tblLayout.Controls.Add(this._cboOrganization, 1, 1);
        this._tblLayout.Controls.Add(this._lblCardNumber, 0, 2);
        this._tblLayout.Controls.Add(this._txtCardNumber, 1, 2);
        this._tblLayout.Controls.Add(this._lblPhone, 0, 3);
        this._tblLayout.Controls.Add(this._txtPhone, 1, 3);
        this._tblLayout.Controls.Add(this._lblNationalId, 0, 4);
        this._tblLayout.Controls.Add(this._txtNationalId, 1, 4);
        this._tblLayout.Controls.Add(this._lblExpiryDate, 0, 5);
        this._tblLayout.Controls.Add(this._pnlExpiry, 1, 5);
        this._tblLayout.Controls.Add(this._lblNotes, 0, 6);
        this._tblLayout.Controls.Add(this._txtNotes, 1, 6);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16, 16, 16, 8);
        this._tblLayout.RowCount = 7;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Size = new System.Drawing.Size(520, 340);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblFullName
        // 
        this._lblFullName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblFullName.AutoSize = true;
        this._lblFullName.Location = new System.Drawing.Point(393, 24);
        this._lblFullName.Name = "_lblFullName";
        this._lblFullName.Size = new System.Drawing.Size(81, 19);
        this._lblFullName.TabIndex = 0;
        this._lblFullName.Text = "الاسم الرباعي:";
        // 
        // _txtFullName
        // 
        this._txtFullName.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtFullName.Location = new System.Drawing.Point(19, 19);
        this._txtFullName.MaxLength = 100;
        this._txtFullName.Name = "_txtFullName";
        this._txtFullName.Size = new System.Drawing.Size(352, 25);
        this._txtFullName.TabIndex = 1;
        // 
        // _lblOrganization
        // 
        this._lblOrganization.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblOrganization.AutoSize = true;
        this._lblOrganization.Location = new System.Drawing.Point(433, 60);
        this._lblOrganization.Name = "_lblOrganization";
        this._lblOrganization.Size = new System.Drawing.Size(41, 19);
        this._lblOrganization.TabIndex = 2;
        this._lblOrganization.Text = "الجهة:";
        // 
        // _cboOrganization
        // 
        this._cboOrganization.Dock = System.Windows.Forms.DockStyle.Fill;
        this._cboOrganization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cboOrganization.FormattingEnabled = true;
        this._cboOrganization.Location = new System.Drawing.Point(19, 55);
        this._cboOrganization.Name = "_cboOrganization";
        this._cboOrganization.Size = new System.Drawing.Size(352, 25);
        this._cboOrganization.TabIndex = 3;
        // 
        // _lblCardNumber
        // 
        this._lblCardNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCardNumber.AutoSize = true;
        this._lblCardNumber.Location = new System.Drawing.Point(401, 96);
        this._lblCardNumber.Name = "_lblCardNumber";
        this._lblCardNumber.Size = new System.Drawing.Size(73, 19);
        this._lblCardNumber.TabIndex = 4;
        this._lblCardNumber.Text = "رقم البطاقة:";
        // 
        // _txtCardNumber
        // 
        this._txtCardNumber.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtCardNumber.Location = new System.Drawing.Point(19, 91);
        this._txtCardNumber.MaxLength = 30;
        this._txtCardNumber.Name = "_txtCardNumber";
        this._txtCardNumber.Size = new System.Drawing.Size(352, 25);
        this._txtCardNumber.TabIndex = 5;
        // 
        // _lblPhone
        // 
        this._lblPhone.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblPhone.AutoSize = true;
        this._lblPhone.Location = new System.Drawing.Point(401, 132);
        this._lblPhone.Name = "_lblPhone";
        this._lblPhone.Size = new System.Drawing.Size(73, 19);
        this._lblPhone.TabIndex = 6;
        this._lblPhone.Text = "رقم الهاتف:";
        // 
        // _txtPhone
        // 
        this._txtPhone.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtPhone.Location = new System.Drawing.Point(19, 127);
        this._txtPhone.MaxLength = 20;
        this._txtPhone.Name = "_txtPhone";
        this._txtPhone.Size = new System.Drawing.Size(352, 25);
        this._txtPhone.TabIndex = 7;
        // 
        // _lblNationalId
        // 
        this._lblNationalId.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblNationalId.AutoSize = true;
        this._lblNationalId.Location = new System.Drawing.Point(391, 168);
        this._lblNationalId.Name = "_lblNationalId";
        this._lblNationalId.Size = new System.Drawing.Size(83, 19);
        this._lblNationalId.TabIndex = 8;
        this._lblNationalId.Text = "الرقم الوطني:";
        // 
        // _txtNationalId
        // 
        this._txtNationalId.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtNationalId.Location = new System.Drawing.Point(19, 163);
        this._txtNationalId.MaxLength = 12;
        this._txtNationalId.Name = "_txtNationalId";
        this._txtNationalId.Size = new System.Drawing.Size(352, 25);
        this._txtNationalId.TabIndex = 9;
        // 
        // _lblExpiryDate
        // 
        this._lblExpiryDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblExpiryDate.AutoSize = true;
        this._lblExpiryDate.Location = new System.Drawing.Point(389, 204);
        this._lblExpiryDate.Name = "_lblExpiryDate";
        this._lblExpiryDate.Size = new System.Drawing.Size(85, 19);
        this._lblExpiryDate.TabIndex = 10;
        this._lblExpiryDate.Text = "انتهاء البطاقة:";
        // 
        // _pnlExpiry
        // 
        this._pnlExpiry.Controls.Add(this._chkHasExpiry);
        this._pnlExpiry.Controls.Add(this._dtpExpiryDate);
        this._pnlExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlExpiry.Location = new System.Drawing.Point(19, 199);
        this._pnlExpiry.Name = "_pnlExpiry";
        this._pnlExpiry.Size = new System.Drawing.Size(352, 30);
        this._pnlExpiry.TabIndex = 11;
        // 
        // _chkHasExpiry
        // 
        this._chkHasExpiry.AutoSize = true;
        this._chkHasExpiry.Location = new System.Drawing.Point(288, 3);
        this._chkHasExpiry.Name = "_chkHasExpiry";
        this._chkHasExpiry.Size = new System.Drawing.Size(61, 23);
        this._chkHasExpiry.TabIndex = 0;
        this._chkHasExpiry.Text = "محددة";
        this._chkHasExpiry.UseVisualStyleBackColor = true;
        // 
        // _dtpExpiryDate
        // 
        this._dtpExpiryDate.CustomFormat = "dd/MM/yyyy";
        this._dtpExpiryDate.Enabled = false;
        this._dtpExpiryDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this._dtpExpiryDate.Location = new System.Drawing.Point(142, 3);
        this._dtpExpiryDate.Name = "_dtpExpiryDate";
        this._dtpExpiryDate.Size = new System.Drawing.Size(140, 25);
        this._dtpExpiryDate.TabIndex = 1;
        // 
        // _lblNotes
        // 
        this._lblNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
        this._lblNotes.AutoSize = true;
        this._lblNotes.Location = new System.Drawing.Point(409, 232);
        this._lblNotes.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        this._lblNotes.Name = "_lblNotes";
        this._lblNotes.Size = new System.Drawing.Size(65, 19);
        this._lblNotes.TabIndex = 12;
        this._lblNotes.Text = "ملاحظات:";
        // 
        // _txtNotes
        // 
        this._txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtNotes.Location = new System.Drawing.Point(19, 235);
        this._txtNotes.MaxLength = 500;
        this._txtNotes.Multiline = true;
        this._txtNotes.Name = "_txtNotes";
        this._txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this._txtNotes.Size = new System.Drawing.Size(352, 62);
        this._txtNotes.TabIndex = 13;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 390);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(520, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(368, 11);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(120, 32);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ التعديلات";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Location = new System.Drawing.Point(262, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // EditBeneficiaryDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(520, 440);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditBeneficiaryDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "تعديل بيانات حامل البطاقة";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlExpiry.ResumeLayout(false);
        this._pnlExpiry.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblFullName;
    private System.Windows.Forms.TextBox _txtFullName;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.ComboBox _cboOrganization;
    private System.Windows.Forms.Label _lblCardNumber;
    private System.Windows.Forms.TextBox _txtCardNumber;
    private System.Windows.Forms.Label _lblPhone;
    private System.Windows.Forms.TextBox _txtPhone;
    private System.Windows.Forms.Label _lblNationalId;
    private System.Windows.Forms.TextBox _txtNationalId;
    private System.Windows.Forms.Label _lblExpiryDate;
    private System.Windows.Forms.FlowLayoutPanel _pnlExpiry;
    private System.Windows.Forms.CheckBox _chkHasExpiry;
    private System.Windows.Forms.DateTimePicker _dtpExpiryDate;
    private System.Windows.Forms.Label _lblNotes;
    private System.Windows.Forms.TextBox _txtNotes;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
