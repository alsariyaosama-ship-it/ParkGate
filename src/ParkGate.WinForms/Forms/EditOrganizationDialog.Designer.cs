#nullable disable

namespace ParkGate.WinForms.Forms;

partial class EditOrganizationDialog
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
        this._lblCode = new System.Windows.Forms.Label();
        this._txtCode = new System.Windows.Forms.TextBox();
        this._lblNameAr = new System.Windows.Forms.Label();
        this._txtNameAr = new System.Windows.Forms.TextBox();
        this._chkCustomCooldown = new System.Windows.Forms.CheckBox();
        this._numCooldownHours = new System.Windows.Forms.NumericUpDown();
        this._chkIsDefault = new System.Windows.Forms.CheckBox();
        this._grpPolicy = new System.Windows.Forms.GroupBox();
        this._tblPolicyLayout = new System.Windows.Forms.TableLayoutPanel();
        this._chkMaxFreeCompanions = new System.Windows.Forms.CheckBox();
        this._numMaxFreeCompanions = new System.Windows.Forms.NumericUpDown();
        this._chkGiftEnabled = new System.Windows.Forms.CheckBox();
        this._lblGiftValue = new System.Windows.Forms.Label();
        this._numGiftValue = new System.Windows.Forms.NumericUpDown();
        this._lblGiftInterval = new System.Windows.Forms.Label();
        this._numGiftIntervalDays = new System.Windows.Forms.NumericUpDown();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numCooldownHours)).BeginInit();
        this._grpPolicy.SuspendLayout();
        this._tblPolicyLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxFreeCompanions)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numGiftValue)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numGiftIntervalDays)).BeginInit();
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
        this._pnlHeader.Size = new System.Drawing.Size(520, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        this._lblHeader.Size = new System.Drawing.Size(520, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "بيانات الجهة";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblCode, 0, 0);
        this._tblLayout.Controls.Add(this._txtCode, 1, 0);
        this._tblLayout.Controls.Add(this._lblNameAr, 0, 1);
        this._tblLayout.Controls.Add(this._txtNameAr, 1, 1);
        this._tblLayout.Controls.Add(this._chkCustomCooldown, 0, 2);
        this._tblLayout.Controls.Add(this._numCooldownHours, 1, 2);
        this._tblLayout.Controls.Add(this._chkIsDefault, 1, 3);
        this._tblLayout.Controls.Add(this._grpPolicy, 0, 4);
        this._tblLayout.SetColumnSpan(this._grpPolicy, 2);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
        this._tblLayout.RowCount = 5;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 184F));
        this._tblLayout.Size = new System.Drawing.Size(520, 364);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblCode
        // 
        this._lblCode.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCode.AutoSize = true;
        this._lblCode.Location = new System.Drawing.Point(380, 20);
        this._lblCode.Name = "_lblCode";
        this._lblCode.Size = new System.Drawing.Size(65, 19);
        this._lblCode.TabIndex = 0;
        this._lblCode.Text = "رمز الجهة:";
        // 
        // _txtCode
        // 
        this._txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        this._txtCode.Location = new System.Drawing.Point(19, 17);
        this._txtCode.MaxLength = 50;
        this._txtCode.Name = "_txtCode";
        this._txtCode.Size = new System.Drawing.Size(322, 25);
        this._txtCode.TabIndex = 1;
        // 
        // _lblNameAr
        // 
        this._lblNameAr.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblNameAr.AutoSize = true;
        this._lblNameAr.Location = new System.Drawing.Point(380, 56);
        this._lblNameAr.Name = "_lblNameAr";
        this._lblNameAr.Size = new System.Drawing.Size(117, 19);
        this._lblNameAr.TabIndex = 2;
        this._lblNameAr.Text = "اسم الجهة بالعربية:";
        // 
        // _txtNameAr
        // 
        this._txtNameAr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtNameAr.Location = new System.Drawing.Point(19, 53);
        this._txtNameAr.MaxLength = 200;
        this._txtNameAr.Name = "_txtNameAr";
        this._txtNameAr.Size = new System.Drawing.Size(322, 25);
        this._txtNameAr.TabIndex = 3;
        // 
        // _chkCustomCooldown
        // 
        this._chkCustomCooldown.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkCustomCooldown.AutoSize = true;
        this._chkCustomCooldown.Location = new System.Drawing.Point(363, 92);
        this._chkCustomCooldown.Name = "_chkCustomCooldown";
        this._chkCustomCooldown.Size = new System.Drawing.Size(138, 23);
        this._chkCustomCooldown.TabIndex = 4;
        this._chkCustomCooldown.Text = "فترة سماح خاصة:";
        this._chkCustomCooldown.UseVisualStyleBackColor = true;
        // 
        // _numCooldownHours
        // 
        this._numCooldownHours.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numCooldownHours.Enabled = false;
        this._numCooldownHours.Location = new System.Drawing.Point(221, 91);
        this._numCooldownHours.Maximum = new decimal(new int[] { 168, 0, 0, 0 });
        this._numCooldownHours.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numCooldownHours.Name = "_numCooldownHours";
        this._numCooldownHours.Size = new System.Drawing.Size(120, 25);
        this._numCooldownHours.TabIndex = 5;
        this._numCooldownHours.Value = new decimal(new int[] { 12, 0, 0, 0 });
        // 
        // _chkIsDefault
        // 
        this._chkIsDefault.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkIsDefault.AutoSize = true;
        this._chkIsDefault.Location = new System.Drawing.Point(195, 128);
        this._chkIsDefault.Name = "_chkIsDefault";
        this._chkIsDefault.Size = new System.Drawing.Size(146, 23);
        this._chkIsDefault.TabIndex = 6;
        this._chkIsDefault.Text = "الجهة الافتراضية للنظام";
        this._chkIsDefault.UseVisualStyleBackColor = true;
        // 
        // _grpPolicy
        // 
        this._grpPolicy.Controls.Add(this._tblPolicyLayout);
        this._grpPolicy.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpPolicy.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpPolicy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this._grpPolicy.Location = new System.Drawing.Point(19, 159);
        this._grpPolicy.Name = "_grpPolicy";
        this._grpPolicy.Padding = new System.Windows.Forms.Padding(8);
        this._grpPolicy.Size = new System.Drawing.Size(482, 190);
        this._grpPolicy.TabIndex = 7;
        this._grpPolicy.TabStop = false;
        this._grpPolicy.Text = "سياسة الهدايا والمرافقين";
        // 
        // _tblPolicyLayout
        // 
        this._tblPolicyLayout.ColumnCount = 2;
        this._tblPolicyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
        this._tblPolicyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblPolicyLayout.Controls.Add(this._chkMaxFreeCompanions, 0, 0);
        this._tblPolicyLayout.Controls.Add(this._numMaxFreeCompanions, 1, 0);
        this._tblPolicyLayout.Controls.Add(this._chkGiftEnabled, 0, 1);
        this._tblPolicyLayout.SetColumnSpan(this._chkGiftEnabled, 2);
        this._tblPolicyLayout.Controls.Add(this._lblGiftValue, 0, 2);
        this._tblPolicyLayout.Controls.Add(this._numGiftValue, 1, 2);
        this._tblPolicyLayout.Controls.Add(this._lblGiftInterval, 0, 3);
        this._tblPolicyLayout.Controls.Add(this._numGiftIntervalDays, 1, 3);
        this._tblPolicyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblPolicyLayout.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._tblPolicyLayout.Location = new System.Drawing.Point(8, 26);
        this._tblPolicyLayout.Name = "_tblPolicyLayout";
        this._tblPolicyLayout.RowCount = 4;
        this._tblPolicyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblPolicyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblPolicyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblPolicyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblPolicyLayout.Size = new System.Drawing.Size(466, 156);
        this._tblPolicyLayout.TabIndex = 0;
        // 
        // _chkMaxFreeCompanions
        // 
        this._chkMaxFreeCompanions.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkMaxFreeCompanions.AutoSize = true;
        this._chkMaxFreeCompanions.Location = new System.Drawing.Point(286, 6);
        this._chkMaxFreeCompanions.Name = "_chkMaxFreeCompanions";
        this._chkMaxFreeCompanions.Size = new System.Drawing.Size(177, 23);
        this._chkMaxFreeCompanions.TabIndex = 0;
        this._chkMaxFreeCompanions.Text = "تحديد حد للمرافقين المجانيين:";
        this._chkMaxFreeCompanions.UseVisualStyleBackColor = true;
        // 
        // _numMaxFreeCompanions
        // 
        this._numMaxFreeCompanions.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numMaxFreeCompanions.Enabled = false;
        this._numMaxFreeCompanions.Location = new System.Drawing.Point(153, 5);
        this._numMaxFreeCompanions.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        this._numMaxFreeCompanions.Name = "_numMaxFreeCompanions";
        this._numMaxFreeCompanions.Size = new System.Drawing.Size(120, 25);
        this._numMaxFreeCompanions.TabIndex = 1;
        // 
        // _chkGiftEnabled
        // 
        this._chkGiftEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkGiftEnabled.AutoSize = true;
        this._chkGiftEnabled.Location = new System.Drawing.Point(344, 42);
        this._chkGiftEnabled.Name = "_chkGiftEnabled";
        this._chkGiftEnabled.Size = new System.Drawing.Size(119, 23);
        this._chkGiftEnabled.TabIndex = 2;
        this._chkGiftEnabled.Text = "تفعيل هدايا الألعاب";
        this._chkGiftEnabled.UseVisualStyleBackColor = true;
        // 
        // _lblGiftValue
        // 
        this._lblGiftValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblGiftValue.AutoSize = true;
        this._lblGiftValue.Location = new System.Drawing.Point(391, 80);
        this._lblGiftValue.Name = "_lblGiftValue";
        this._lblGiftValue.Size = new System.Drawing.Size(72, 19);
        this._lblGiftValue.TabIndex = 3;
        this._lblGiftValue.Text = "قيمة الهدية:";
        // 
        // _numGiftValue
        // 
        this._numGiftValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numGiftValue.DecimalPlaces = 3;
        this._numGiftValue.Enabled = false;
        this._numGiftValue.Location = new System.Drawing.Point(153, 77);
        this._numGiftValue.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this._numGiftValue.Name = "_numGiftValue";
        this._numGiftValue.Size = new System.Drawing.Size(120, 25);
        this._numGiftValue.TabIndex = 4;
        this._numGiftValue.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // _lblGiftInterval
        // 
        this._lblGiftInterval.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblGiftInterval.AutoSize = true;
        this._lblGiftInterval.Location = new System.Drawing.Point(298, 117);
        this._lblGiftInterval.Name = "_lblGiftInterval";
        this._lblGiftInterval.Size = new System.Drawing.Size(165, 19);
        this._lblGiftInterval.TabIndex = 5;
        this._lblGiftInterval.Text = "مدة الانتظار بين هديتين (أيام):";
        // 
        // _numGiftIntervalDays
        // 
        this._numGiftIntervalDays.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numGiftIntervalDays.Enabled = false;
        this._numGiftIntervalDays.Location = new System.Drawing.Point(153, 114);
        this._numGiftIntervalDays.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
        this._numGiftIntervalDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numGiftIntervalDays.Name = "_numGiftIntervalDays";
        this._numGiftIntervalDays.Size = new System.Drawing.Size(120, 25);
        this._numGiftIntervalDays.TabIndex = 6;
        this._numGiftIntervalDays.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 414);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(520, 56);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSave.FlatAppearance.BorderSize = 0;
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(378, 9);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(110, 38);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnCancel.Location = new System.Drawing.Point(268, 9);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 38);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // EditOrganizationDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(520, 470);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditOrganizationDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "بيانات الجهة";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numCooldownHours)).EndInit();
        this._grpPolicy.ResumeLayout(false);
        this._tblPolicyLayout.ResumeLayout(false);
        this._tblPolicyLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxFreeCompanions)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numGiftValue)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numGiftIntervalDays)).EndInit();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblCode;
    private System.Windows.Forms.TextBox _txtCode;
    private System.Windows.Forms.Label _lblNameAr;
    private System.Windows.Forms.TextBox _txtNameAr;
    private System.Windows.Forms.CheckBox _chkCustomCooldown;
    private System.Windows.Forms.NumericUpDown _numCooldownHours;
    private System.Windows.Forms.CheckBox _chkIsDefault;
    private System.Windows.Forms.GroupBox _grpPolicy;
    private System.Windows.Forms.TableLayoutPanel _tblPolicyLayout;
    private System.Windows.Forms.CheckBox _chkMaxFreeCompanions;
    private System.Windows.Forms.NumericUpDown _numMaxFreeCompanions;
    private System.Windows.Forms.CheckBox _chkGiftEnabled;
    private System.Windows.Forms.Label _lblGiftValue;
    private System.Windows.Forms.NumericUpDown _numGiftValue;
    private System.Windows.Forms.Label _lblGiftInterval;
    private System.Windows.Forms.NumericUpDown _numGiftIntervalDays;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
