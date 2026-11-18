#nullable disable

namespace ParkGate.WinForms.Controls;

partial class InlineRegistrationPanel
{
    /// <summary>Required designer variable.</summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>Clean up any resources being used.</summary>
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
        this._grpRegistration = new System.Windows.Forms.GroupBox();
        this._tblRegistration = new System.Windows.Forms.TableLayoutPanel();
        this._lblRegOrganization = new System.Windows.Forms.Label();
        this._cmbRegOrganization = new System.Windows.Forms.ComboBox();
        this._lblRegFullName = new System.Windows.Forms.Label();
        this._txtRegFullName = new System.Windows.Forms.TextBox();
        this._lblRegPhone = new System.Windows.Forms.Label();
        this._txtRegPhone = new System.Windows.Forms.TextBox();
        this._lblRegNationalId = new System.Windows.Forms.Label();
        this._txtRegNationalId = new System.Windows.Forms.TextBox();
        this._lblRegExpiry = new System.Windows.Forms.Label();
        this._dtpRegExpiry = new System.Windows.Forms.DateTimePicker();
        this._lblRegNotes = new System.Windows.Forms.Label();
        this._txtRegNotes = new System.Windows.Forms.TextBox();
        this._pnlRegActions = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSaveReg = new System.Windows.Forms.Button();
        this._btnCancelReg = new System.Windows.Forms.Button();
        this._grpRegistration.SuspendLayout();
        this._tblRegistration.SuspendLayout();
        this._pnlRegActions.SuspendLayout();
        this.SuspendLayout();
        // 
        // _grpRegistration
        // 
        this._grpRegistration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(252)))), ((int)(((byte)(232)))));
        this._grpRegistration.Controls.Add(this._tblRegistration);
        this._grpRegistration.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpRegistration.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpRegistration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(77)))), ((int)(((byte)(14)))));
        this._grpRegistration.Location = new System.Drawing.Point(0, 0);
        this._grpRegistration.Name = "_grpRegistration";
        this._grpRegistration.Padding = new System.Windows.Forms.Padding(8);
        this._grpRegistration.Size = new System.Drawing.Size(500, 289);
        this._grpRegistration.TabIndex = 1;
        this._grpRegistration.TabStop = false;
        this._grpRegistration.Text = "تسجيل زبون جديد (فوري)";
        // 
        // _tblRegistration
        // 
        this._tblRegistration.ColumnCount = 2;
        this._tblRegistration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
        this._tblRegistration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblRegistration.Controls.Add(this._lblRegOrganization, 0, 0);
        this._tblRegistration.Controls.Add(this._cmbRegOrganization, 1, 0);
        this._tblRegistration.Controls.Add(this._lblRegFullName, 0, 1);
        this._tblRegistration.Controls.Add(this._txtRegFullName, 1, 1);
        this._tblRegistration.Controls.Add(this._lblRegPhone, 0, 2);
        this._tblRegistration.Controls.Add(this._txtRegPhone, 1, 2);
        this._tblRegistration.Controls.Add(this._lblRegNationalId, 0, 3);
        this._tblRegistration.Controls.Add(this._txtRegNationalId, 1, 3);
        this._tblRegistration.Controls.Add(this._lblRegExpiry, 0, 4);
        this._tblRegistration.Controls.Add(this._dtpRegExpiry, 1, 4);
        this._tblRegistration.Controls.Add(this._lblRegNotes, 0, 5);
        this._tblRegistration.Controls.Add(this._txtRegNotes, 1, 5);
        this._tblRegistration.Controls.Add(this._pnlRegActions, 1, 6);
        this._tblRegistration.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblRegistration.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._tblRegistration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._tblRegistration.Location = new System.Drawing.Point(8, 26);
        this._tblRegistration.Name = "_tblRegistration";
        this._tblRegistration.RowCount = 7;
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        this._tblRegistration.Size = new System.Drawing.Size(484, 255);
        this._tblRegistration.TabIndex = 0;
        // 
        // _lblRegOrganization
        // 
        this._lblRegOrganization.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegOrganization.AutoSize = true;
        this._lblRegOrganization.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRegOrganization.Location = new System.Drawing.Point(437, 7);
        this._lblRegOrganization.Name = "_lblRegOrganization";
        this._lblRegOrganization.Size = new System.Drawing.Size(44, 17);
        this._lblRegOrganization.TabIndex = 0;
        this._lblRegOrganization.Text = "الجهة:";
        // 
        // _cmbRegOrganization
        // 
        this._cmbRegOrganization.Dock = System.Windows.Forms.DockStyle.Fill;
        this._cmbRegOrganization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbRegOrganization.FormattingEnabled = true;
        this._cmbRegOrganization.Location = new System.Drawing.Point(3, 4);
        this._cmbRegOrganization.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._cmbRegOrganization.Name = "_cmbRegOrganization";
        this._cmbRegOrganization.Size = new System.Drawing.Size(348, 24);
        this._cmbRegOrganization.TabIndex = 1;
        // 
        // _lblRegFullName
        // 
        this._lblRegFullName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegFullName.AutoSize = true;
        this._lblRegFullName.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRegFullName.Location = new System.Drawing.Point(395, 7);
        this._lblRegFullName.Name = "_lblRegFullName";
        this._lblRegFullName.Size = new System.Drawing.Size(86, 17);
        this._lblRegFullName.TabIndex = 2;
        this._lblRegFullName.Text = "الاسم الرباعي:";
        // 
        // _txtRegFullName
        // 
        this._txtRegFullName.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtRegFullName.Location = new System.Drawing.Point(3, 4);
        this._txtRegFullName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._txtRegFullName.MaxLength = 100;
        this._txtRegFullName.Name = "_txtRegFullName";
        this._txtRegFullName.Size = new System.Drawing.Size(348, 24);
        this._txtRegFullName.TabIndex = 3;
        // 
        // _lblRegPhone
        // 
        this._lblRegPhone.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegPhone.AutoSize = true;
        this._lblRegPhone.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRegPhone.Location = new System.Drawing.Point(407, 39);
        this._lblRegPhone.Name = "_lblRegPhone";
        this._lblRegPhone.Size = new System.Drawing.Size(74, 17);
        this._lblRegPhone.TabIndex = 4;
        this._lblRegPhone.Text = "رقم الهاتف:";
        // 
        // _txtRegPhone
        // 
        this._txtRegPhone.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtRegPhone.Location = new System.Drawing.Point(3, 36);
        this._txtRegPhone.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._txtRegPhone.MaxLength = 20;
        this._txtRegPhone.Name = "_txtRegPhone";
        this._txtRegPhone.Size = new System.Drawing.Size(348, 24);
        this._txtRegPhone.TabIndex = 5;
        // 
        // _lblRegNationalId
        // 
        this._lblRegNationalId.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegNationalId.AutoSize = true;
        this._lblRegNationalId.Location = new System.Drawing.Point(365, 71);
        this._lblRegNationalId.Name = "_lblRegNationalId";
        this._lblRegNationalId.Size = new System.Drawing.Size(116, 17);
        this._lblRegNationalId.TabIndex = 6;
        this._lblRegNationalId.Text = "الرقم الوطني (اختياري):";
        // 
        // _txtRegNationalId
        // 
        this._txtRegNationalId.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtRegNationalId.Location = new System.Drawing.Point(3, 68);
        this._txtRegNationalId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._txtRegNationalId.MaxLength = 12;
        this._txtRegNationalId.Name = "_txtRegNationalId";
        this._txtRegNationalId.Size = new System.Drawing.Size(348, 24);
        this._txtRegNationalId.TabIndex = 7;
        // 
        // _lblRegExpiry
        // 
        this._lblRegExpiry.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegExpiry.AutoSize = true;
        this._lblRegExpiry.Location = new System.Drawing.Point(367, 103);
        this._lblRegExpiry.Name = "_lblRegExpiry";
        this._lblRegExpiry.Size = new System.Drawing.Size(114, 17);
        this._lblRegExpiry.TabIndex = 8;
        this._lblRegExpiry.Text = "صلاحية البطاقة (اختياري):";
        // 
        // _dtpRegExpiry
        // 
        this._dtpRegExpiry.Checked = false;
        this._dtpRegExpiry.CustomFormat = "dd/MM/yyyy";
        this._dtpRegExpiry.Dock = System.Windows.Forms.DockStyle.Fill;
        this._dtpRegExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this._dtpRegExpiry.Location = new System.Drawing.Point(3, 100);
        this._dtpRegExpiry.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._dtpRegExpiry.Name = "_dtpRegExpiry";
        this._dtpRegExpiry.ShowCheckBox = true;
        this._dtpRegExpiry.Size = new System.Drawing.Size(348, 24);
        this._dtpRegExpiry.TabIndex = 9;
        // 
        // _lblRegNotes
        // 
        this._lblRegNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegNotes.AutoSize = true;
        this._lblRegNotes.Location = new System.Drawing.Point(427, 137);
        this._lblRegNotes.Name = "_lblRegNotes";
        this._lblRegNotes.Size = new System.Drawing.Size(54, 17);
        this._lblRegNotes.TabIndex = 10;
        this._lblRegNotes.Text = "ملاحظات:";
        // 
        // _txtRegNotes
        // 
        this._txtRegNotes.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtRegNotes.Location = new System.Drawing.Point(3, 132);
        this._txtRegNotes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this._txtRegNotes.MaxLength = 500;
        this._txtRegNotes.Name = "_txtRegNotes";
        this._txtRegNotes.Size = new System.Drawing.Size(348, 24);
        this._txtRegNotes.TabIndex = 11;
        // 
        // _pnlRegActions
        // 
        this._pnlRegActions.Controls.Add(this._btnSaveReg);
        this._pnlRegActions.Controls.Add(this._btnCancelReg);
        this._pnlRegActions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlRegActions.Location = new System.Drawing.Point(3, 163);
        this._pnlRegActions.Name = "_pnlRegActions";
        this._pnlRegActions.Size = new System.Drawing.Size(348, 54);
        this._pnlRegActions.TabIndex = 12;
        // 
        // _btnSaveReg
        // 
        this._btnSaveReg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSaveReg.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSaveReg.FlatAppearance.BorderSize = 0;
        this._btnSaveReg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSaveReg.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSaveReg.ForeColor = System.Drawing.Color.White;
        this._btnSaveReg.Location = new System.Drawing.Point(195, 3);
        this._btnSaveReg.Name = "_btnSaveReg";
        this._btnSaveReg.Size = new System.Drawing.Size(150, 32);
        this._btnSaveReg.TabIndex = 0;
        this._btnSaveReg.Text = "حفظ ومتابعة الدخول";
        this._btnSaveReg.UseVisualStyleBackColor = false;
        // 
        // _btnCancelReg
        // 
        this._btnCancelReg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnCancelReg.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnCancelReg.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnCancelReg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancelReg.Location = new System.Drawing.Point(109, 3);
        this._btnCancelReg.Name = "_btnCancelReg";
        this._btnCancelReg.Size = new System.Drawing.Size(80, 32);
        this._btnCancelReg.TabIndex = 1;
        this._btnCancelReg.Text = "إلغاء";
        this._btnCancelReg.UseVisualStyleBackColor = false;
        //
        // InlineRegistrationPanel
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this._grpRegistration);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Margin = new System.Windows.Forms.Padding(0);
        this.Name = "InlineRegistrationPanel";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(500, 289);
        this._grpRegistration.ResumeLayout(false);
        this._tblRegistration.ResumeLayout(false);
        this._tblRegistration.PerformLayout();
        this._pnlRegActions.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.GroupBox _grpRegistration;
    private System.Windows.Forms.TableLayoutPanel _tblRegistration;
    private System.Windows.Forms.Label _lblRegOrganization;
    private System.Windows.Forms.ComboBox _cmbRegOrganization;
    private System.Windows.Forms.Label _lblRegFullName;
    private System.Windows.Forms.TextBox _txtRegFullName;
    private System.Windows.Forms.Label _lblRegPhone;
    private System.Windows.Forms.TextBox _txtRegPhone;
    private System.Windows.Forms.Label _lblRegNationalId;
    private System.Windows.Forms.TextBox _txtRegNationalId;
    private System.Windows.Forms.Label _lblRegExpiry;
    private System.Windows.Forms.DateTimePicker _dtpRegExpiry;
    private System.Windows.Forms.Label _lblRegNotes;
    private System.Windows.Forms.TextBox _txtRegNotes;
    private System.Windows.Forms.FlowLayoutPanel _pnlRegActions;
    private System.Windows.Forms.Button _btnSaveReg;
    private System.Windows.Forms.Button _btnCancelReg;
}
