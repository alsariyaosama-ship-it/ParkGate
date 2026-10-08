#nullable disable

namespace ParkGate.WinForms.Forms;

partial class SettingsForm
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
        _pnlHeader = new Panel();
        _lblHeader = new Label();
        _tabSettings = new TabControl();
        _tabGeneral = new TabPage();
        _tblGeneral = new TableLayoutPanel();
        _lblParkName = new Label();
        _txtParkName = new TextBox();
        _lblTimeZone = new Label();
        _txtTimeZone = new TextBox();
        _tabEntry = new TabPage();
        _tblEntry = new TableLayoutPanel();
        _lblCooldownHours = new Label();
        _numCooldownHours = new NumericUpDown();
        _lblMaxGroupSize = new Label();
        _numMaxGroupSize = new NumericUpDown();
        _lblDenialDedupMinutes = new Label();
        _numDenialDedupMinutes = new NumericUpDown();
        _chkPrefillCompanions = new CheckBox();
        _chkDenialSound = new CheckBox();
        _tabSecurity = new TabPage();
        _tblSecurity = new TableLayoutPanel();
        _lblMaxFailedLogins = new Label();
        _numMaxFailedLogins = new NumericUpDown();
        _lblLockoutMinutes = new Label();
        _numLockoutMinutes = new NumericUpDown();
        _lblAutoLockMinutes = new Label();
        _numAutoLockMinutes = new NumericUpDown();
        _pnlButtons = new FlowLayoutPanel();
        _btnSave = new Button();
        _btnReload = new Button();
        _btnClose = new Button();
        _pnlHeader.SuspendLayout();
        _tabSettings.SuspendLayout();
        _tabGeneral.SuspendLayout();
        _tblGeneral.SuspendLayout();
        _tabEntry.SuspendLayout();
        _tblEntry.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numCooldownHours).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_numMaxGroupSize).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_numDenialDedupMinutes).BeginInit();
        _tabSecurity.SuspendLayout();
        _tblSecurity.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numMaxFailedLogins).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_numLockoutMinutes).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_numAutoLockMinutes).BeginInit();
        _pnlButtons.SuspendLayout();
        SuspendLayout();
        // 
        // _pnlHeader
        // 
        _pnlHeader.BackColor = Color.FromArgb(27, 135, 63);
        _pnlHeader.Controls.Add(_lblHeader);
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Location = new Point(0, 0);
        _pnlHeader.Name = "_pnlHeader";
        _pnlHeader.Size = new Size(650, 50);
        _pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        _lblHeader.Dock = DockStyle.Fill;
        _lblHeader.Font = new Font("Cairo", 12F, FontStyle.Bold);
        _lblHeader.ForeColor = Color.White;
        _lblHeader.Location = new Point(0, 0);
        _lblHeader.Name = "_lblHeader";
        _lblHeader.Padding = new Padding(16, 0, 16, 0);
        _lblHeader.Size = new Size(650, 50);
        _lblHeader.TabIndex = 0;
        _lblHeader.Text = "إعدادات النظام";
        _lblHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _tabSettings
        // 
        _tabSettings.Controls.Add(_tabGeneral);
        _tabSettings.Controls.Add(_tabEntry);
        _tabSettings.Controls.Add(_tabSecurity);
        _tabSettings.Dock = DockStyle.Fill;
        _tabSettings.Location = new Point(0, 50);
        _tabSettings.Name = "_tabSettings";
        _tabSettings.SelectedIndex = 0;
        _tabSettings.Size = new Size(650, 310);
        _tabSettings.TabIndex = 1;
        // 
        // _tabGeneral
        // 
        _tabGeneral.Controls.Add(_tblGeneral);
        _tabGeneral.Location = new Point(4, 41);
        _tabGeneral.Name = "_tabGeneral";
        _tabGeneral.Padding = new Padding(12);
        _tabGeneral.Size = new Size(642, 265);
        _tabGeneral.TabIndex = 0;
        _tabGeneral.Text = "إعدادات عامة";
        _tabGeneral.UseVisualStyleBackColor = true;
        // 
        // _tblGeneral
        // 
        _tblGeneral.ColumnCount = 2;
        _tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        _tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tblGeneral.Controls.Add(_lblParkName, 0, 0);
        _tblGeneral.Controls.Add(_txtParkName, 1, 0);
        _tblGeneral.Controls.Add(_lblTimeZone, 0, 1);
        _tblGeneral.Controls.Add(_txtTimeZone, 1, 1);
        _tblGeneral.Dock = DockStyle.Top;
        _tblGeneral.Location = new Point(12, 12);
        _tblGeneral.Name = "_tblGeneral";
        _tblGeneral.RowCount = 2;
        _tblGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tblGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tblGeneral.Size = new Size(618, 80);
        _tblGeneral.TabIndex = 0;
        // 
        // _lblParkName
        // 
        _lblParkName.Anchor = AnchorStyles.Left;
        _lblParkName.AutoSize = true;
        _lblParkName.Location = new Point(408, 0);
        _lblParkName.Name = "_lblParkName";
        _lblParkName.Size = new Size(207, 40);
        _lblParkName.TabIndex = 0;
        _lblParkName.Text = "اسم المنتزه / المرفق العائلي:";
        // 
        // _txtParkName
        // 
        _txtParkName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtParkName.Location = new Point(3, 3);
        _txtParkName.MaxLength = 100;
        _txtParkName.Name = "_txtParkName";
        _txtParkName.Size = new Size(392, 39);
        _txtParkName.TabIndex = 1;
        // 
        // _lblTimeZone
        // 
        _lblTimeZone.Anchor = AnchorStyles.Left;
        _lblTimeZone.AutoSize = true;
        _lblTimeZone.Location = new Point(487, 44);
        _lblTimeZone.Name = "_lblTimeZone";
        _lblTimeZone.Size = new Size(128, 32);
        _lblTimeZone.TabIndex = 2;
        _lblTimeZone.Text = "المنطقة الزمنية:";
        // 
        // _txtTimeZone
        // 
        _txtTimeZone.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _txtTimeZone.Location = new Point(3, 43);
        _txtTimeZone.MaxLength = 100;
        _txtTimeZone.Name = "_txtTimeZone";
        _txtTimeZone.Size = new Size(392, 39);
        _txtTimeZone.TabIndex = 3;
        // 
        // _tabEntry
        // 
        _tabEntry.Controls.Add(_tblEntry);
        _tabEntry.Location = new Point(4, 41);
        _tabEntry.Name = "_tabEntry";
        _tabEntry.Padding = new Padding(12);
        _tabEntry.Size = new Size(642, 265);
        _tabEntry.TabIndex = 1;
        _tabEntry.Text = "إعدادات الدخول وفترة السماح";
        _tabEntry.UseVisualStyleBackColor = true;
        // 
        // _tblEntry
        // 
        _tblEntry.ColumnCount = 2;
        _tblEntry.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        _tblEntry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tblEntry.Controls.Add(_lblCooldownHours, 0, 0);
        _tblEntry.Controls.Add(_numCooldownHours, 1, 0);
        _tblEntry.Controls.Add(_lblMaxGroupSize, 0, 1);
        _tblEntry.Controls.Add(_numMaxGroupSize, 1, 1);
        _tblEntry.Controls.Add(_lblDenialDedupMinutes, 0, 2);
        _tblEntry.Controls.Add(_numDenialDedupMinutes, 1, 2);
        _tblEntry.Controls.Add(_chkPrefillCompanions, 0, 3);
        _tblEntry.Controls.Add(_chkDenialSound, 0, 4);
        _tblEntry.Dock = DockStyle.Top;
        _tblEntry.Location = new Point(12, 12);
        _tblEntry.Name = "_tblEntry";
        _tblEntry.RowCount = 5;
        _tblEntry.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblEntry.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblEntry.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblEntry.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblEntry.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblEntry.Size = new Size(618, 190);
        _tblEntry.TabIndex = 0;
        // 
        // _lblCooldownHours
        // 
        _lblCooldownHours.Anchor = AnchorStyles.Left;
        _lblCooldownHours.AutoSize = true;
        _lblCooldownHours.Location = new Point(266, 0);
        _lblCooldownHours.Name = "_lblCooldownHours";
        _lblCooldownHours.Size = new Size(349, 38);
        _lblCooldownHours.TabIndex = 0;
        _lblCooldownHours.Text = "فترة السماح الافتراضية بين الزيارات للزبون (بالساعات):";
        // 
        // _numCooldownHours
        // 
        _numCooldownHours.Anchor = AnchorStyles.Left;
        _numCooldownHours.Location = new Point(135, 3);
        _numCooldownHours.Maximum = new decimal(new int[] { 168, 0, 0, 0 });
        _numCooldownHours.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numCooldownHours.Name = "_numCooldownHours";
        _numCooldownHours.Size = new Size(120, 39);
        _numCooldownHours.TabIndex = 1;
        _numCooldownHours.Value = new decimal(new int[] { 12, 0, 0, 0 });
        // 
        // _lblMaxGroupSize
        // 
        _lblMaxGroupSize.Anchor = AnchorStyles.Left;
        _lblMaxGroupSize.AutoSize = true;
        _lblMaxGroupSize.Location = new Point(267, 38);
        _lblMaxGroupSize.Name = "_lblMaxGroupSize";
        _lblMaxGroupSize.Size = new Size(348, 38);
        _lblMaxGroupSize.TabIndex = 2;
        _lblMaxGroupSize.Text = "الحد الأقصى لعدد الأفراد في المجموعة (شامل الزبون):";
        // 
        // _numMaxGroupSize
        // 
        _numMaxGroupSize.Anchor = AnchorStyles.Left;
        _numMaxGroupSize.Location = new Point(135, 41);
        _numMaxGroupSize.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        _numMaxGroupSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numMaxGroupSize.Name = "_numMaxGroupSize";
        _numMaxGroupSize.Size = new Size(120, 39);
        _numMaxGroupSize.TabIndex = 3;
        _numMaxGroupSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // _lblDenialDedupMinutes
        // 
        _lblDenialDedupMinutes.Anchor = AnchorStyles.Left;
        _lblDenialDedupMinutes.AutoSize = true;
        _lblDenialDedupMinutes.Location = new Point(265, 76);
        _lblDenialDedupMinutes.Name = "_lblDenialDedupMinutes";
        _lblDenialDedupMinutes.Size = new Size(350, 38);
        _lblDenialDedupMinutes.TabIndex = 4;
        _lblDenialDedupMinutes.Text = "مدة عدم تكرار تسجيل المحاولات المرفوضة المتتالية (بالدقائق):";
        // 
        // _numDenialDedupMinutes
        // 
        _numDenialDedupMinutes.Anchor = AnchorStyles.Left;
        _numDenialDedupMinutes.Location = new Point(135, 79);
        _numDenialDedupMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        _numDenialDedupMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numDenialDedupMinutes.Name = "_numDenialDedupMinutes";
        _numDenialDedupMinutes.Size = new Size(120, 39);
        _numDenialDedupMinutes.TabIndex = 5;
        _numDenialDedupMinutes.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // _chkPrefillCompanions
        // 
        _chkPrefillCompanions.Anchor = AnchorStyles.Left;
        _chkPrefillCompanions.AutoSize = true;
        _tblEntry.SetColumnSpan(_chkPrefillCompanions, 2);
        _chkPrefillCompanions.Location = new Point(183, 117);
        _chkPrefillCompanions.Name = "_chkPrefillCompanions";
        _chkPrefillCompanions.Size = new Size(432, 32);
        _chkPrefillCompanions.TabIndex = 6;
        _chkPrefillCompanions.Text = "تعبئة أعداد المرافقين تلقائياً من الزيارة السابقة للمستفيد";
        _chkPrefillCompanions.UseVisualStyleBackColor = true;
        // 
        // _chkDenialSound
        // 
        _chkDenialSound.Anchor = AnchorStyles.Left;
        _chkDenialSound.AutoSize = true;
        _tblEntry.SetColumnSpan(_chkDenialSound, 2);
        _chkDenialSound.Location = new Point(329, 155);
        _chkDenialSound.Name = "_chkDenialSound";
        _chkDenialSound.Size = new Size(286, 32);
        _chkDenialSound.TabIndex = 7;
        _chkDenialSound.Text = "تشغيل تنبيه صوتي عند رفض الدخول";
        _chkDenialSound.UseVisualStyleBackColor = true;
        // 
        // _tabSecurity
        // 
        _tabSecurity.Controls.Add(_tblSecurity);
        _tabSecurity.Location = new Point(4, 41);
        _tabSecurity.Name = "_tabSecurity";
        _tabSecurity.Padding = new Padding(12);
        _tabSecurity.Size = new Size(642, 265);
        _tabSecurity.TabIndex = 2;
        _tabSecurity.Text = "إعدادات الأمان والقفل";
        _tabSecurity.UseVisualStyleBackColor = true;
        // 
        // _tblSecurity
        // 
        _tblSecurity.ColumnCount = 2;
        _tblSecurity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        _tblSecurity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tblSecurity.Controls.Add(_lblMaxFailedLogins, 0, 0);
        _tblSecurity.Controls.Add(_numMaxFailedLogins, 1, 0);
        _tblSecurity.Controls.Add(_lblLockoutMinutes, 0, 1);
        _tblSecurity.Controls.Add(_numLockoutMinutes, 1, 1);
        _tblSecurity.Controls.Add(_lblAutoLockMinutes, 0, 2);
        _tblSecurity.Controls.Add(_numAutoLockMinutes, 1, 2);
        _tblSecurity.Dock = DockStyle.Top;
        _tblSecurity.Location = new Point(12, 12);
        _tblSecurity.Name = "_tblSecurity";
        _tblSecurity.RowCount = 3;
        _tblSecurity.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblSecurity.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblSecurity.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _tblSecurity.Size = new Size(618, 114);
        _tblSecurity.TabIndex = 0;
        // 
        // _lblMaxFailedLogins
        // 
        _lblMaxFailedLogins.Anchor = AnchorStyles.Left;
        _lblMaxFailedLogins.AutoSize = true;
        _lblMaxFailedLogins.Location = new Point(266, 3);
        _lblMaxFailedLogins.Name = "_lblMaxFailedLogins";
        _lblMaxFailedLogins.Size = new Size(349, 32);
        _lblMaxFailedLogins.TabIndex = 0;
        _lblMaxFailedLogins.Text = "أقصى عدد لمحاولات الدخول الخاطئة قبل القفل:";
        // 
        // _numMaxFailedLogins
        // 
        _numMaxFailedLogins.Anchor = AnchorStyles.Left;
        _numMaxFailedLogins.Location = new Point(135, 3);
        _numMaxFailedLogins.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        _numMaxFailedLogins.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
        _numMaxFailedLogins.Name = "_numMaxFailedLogins";
        _numMaxFailedLogins.Size = new Size(120, 39);
        _numMaxFailedLogins.TabIndex = 1;
        _numMaxFailedLogins.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // _lblLockoutMinutes
        // 
        _lblLockoutMinutes.Anchor = AnchorStyles.Left;
        _lblLockoutMinutes.AutoSize = true;
        _lblLockoutMinutes.Location = new Point(303, 41);
        _lblLockoutMinutes.Name = "_lblLockoutMinutes";
        _lblLockoutMinutes.Size = new Size(312, 32);
        _lblLockoutMinutes.TabIndex = 2;
        _lblLockoutMinutes.Text = "مدة قفل الحساب بعد الاستنفاد (بالدقائق):";
        // 
        // _numLockoutMinutes
        // 
        _numLockoutMinutes.Anchor = AnchorStyles.Left;
        _numLockoutMinutes.Location = new Point(135, 41);
        _numLockoutMinutes.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
        _numLockoutMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numLockoutMinutes.Name = "_numLockoutMinutes";
        _numLockoutMinutes.Size = new Size(120, 39);
        _numLockoutMinutes.TabIndex = 3;
        _numLockoutMinutes.Value = new decimal(new int[] { 15, 0, 0, 0 });
        // 
        // _lblAutoLockMinutes
        // 
        _lblAutoLockMinutes.Anchor = AnchorStyles.Left;
        _lblAutoLockMinutes.AutoSize = true;
        _lblAutoLockMinutes.Location = new Point(264, 76);
        _lblAutoLockMinutes.Name = "_lblAutoLockMinutes";
        _lblAutoLockMinutes.Size = new Size(351, 38);
        _lblAutoLockMinutes.TabIndex = 4;
        _lblAutoLockMinutes.Text = "مدة قفل الشاشة التلقائي عند الخمول (بالدقائق - 0 للتعطيل):";
        // 
        // _numAutoLockMinutes
        // 
        _numAutoLockMinutes.Anchor = AnchorStyles.Left;
        _numAutoLockMinutes.Location = new Point(135, 79);
        _numAutoLockMinutes.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
        _numAutoLockMinutes.Name = "_numAutoLockMinutes";
        _numAutoLockMinutes.Size = new Size(120, 39);
        _numAutoLockMinutes.TabIndex = 5;
        _numAutoLockMinutes.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _pnlButtons
        // 
        _pnlButtons.Controls.Add(_btnSave);
        _pnlButtons.Controls.Add(_btnReload);
        _pnlButtons.Controls.Add(_btnClose);
        _pnlButtons.Dock = DockStyle.Bottom;
        _pnlButtons.Location = new Point(0, 360);
        _pnlButtons.Name = "_pnlButtons";
        _pnlButtons.Padding = new Padding(16, 8, 16, 8);
        _pnlButtons.Size = new Size(650, 56);
        _pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        _btnSave.BackColor = Color.FromArgb(27, 135, 63);
        _btnSave.Cursor = Cursors.Hand;
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.Font = new Font("Cairo", 10F, FontStyle.Bold);
        _btnSave.ForeColor = Color.White;
        _btnSave.Location = new Point(505, 11);
        _btnSave.Name = "_btnSave";
        _btnSave.Size = new Size(110, 38);
        _btnSave.TabIndex = 0;
        _btnSave.Text = "حفظ";
        _btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnReload
        // 
        _btnReload.BackColor = Color.FromArgb(241, 245, 249);
        _btnReload.Cursor = Cursors.Hand;
        _btnReload.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnReload.FlatStyle = FlatStyle.Flat;
        _btnReload.Font = new Font("Cairo", 9.5F);
        _btnReload.ForeColor = Color.FromArgb(30, 41, 59);
        _btnReload.Location = new Point(389, 11);
        _btnReload.Name = "_btnReload";
        _btnReload.Size = new Size(110, 38);
        _btnReload.TabIndex = 1;
        _btnReload.Text = "إعادة تحميل";
        _btnReload.UseVisualStyleBackColor = false;
        // 
        // _btnClose
        // 
        _btnClose.BackColor = Color.FromArgb(241, 245, 249);
        _btnClose.Cursor = Cursors.Hand;
        _btnClose.DialogResult = DialogResult.Cancel;
        _btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnClose.FlatStyle = FlatStyle.Flat;
        _btnClose.Font = new Font("Cairo", 9.5F);
        _btnClose.ForeColor = Color.FromArgb(30, 41, 59);
        _btnClose.Location = new Point(293, 11);
        _btnClose.Name = "_btnClose";
        _btnClose.Size = new Size(90, 38);
        _btnClose.TabIndex = 2;
        _btnClose.Text = "إغلاق";
        _btnClose.UseVisualStyleBackColor = false;
        // 
        // SettingsForm
        // 
        AcceptButton = _btnSave;
        AutoScaleDimensions = new SizeF(9F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _btnClose;
        ClientSize = new Size(650, 416);
        Controls.Add(_tabSettings);
        Controls.Add(_pnlButtons);
        Controls.Add(_pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "إعدادات النظام";
        _pnlHeader.ResumeLayout(false);
        _tabSettings.ResumeLayout(false);
        _tabGeneral.ResumeLayout(false);
        _tblGeneral.ResumeLayout(false);
        _tblGeneral.PerformLayout();
        _tabEntry.ResumeLayout(false);
        _tblEntry.ResumeLayout(false);
        _tblEntry.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numCooldownHours).EndInit();
        ((System.ComponentModel.ISupportInitialize)_numMaxGroupSize).EndInit();
        ((System.ComponentModel.ISupportInitialize)_numDenialDedupMinutes).EndInit();
        _tabSecurity.ResumeLayout(false);
        _tblSecurity.ResumeLayout(false);
        _tblSecurity.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numMaxFailedLogins).EndInit();
        ((System.ComponentModel.ISupportInitialize)_numLockoutMinutes).EndInit();
        ((System.ComponentModel.ISupportInitialize)_numAutoLockMinutes).EndInit();
        _pnlButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TabControl _tabSettings;
    private System.Windows.Forms.TabPage _tabGeneral;
    private System.Windows.Forms.TableLayoutPanel _tblGeneral;
    private System.Windows.Forms.Label _lblParkName;
    private System.Windows.Forms.TextBox _txtParkName;
    private System.Windows.Forms.Label _lblTimeZone;
    private System.Windows.Forms.TextBox _txtTimeZone;
    private System.Windows.Forms.TabPage _tabEntry;
    private System.Windows.Forms.TableLayoutPanel _tblEntry;
    private System.Windows.Forms.Label _lblCooldownHours;
    private System.Windows.Forms.NumericUpDown _numCooldownHours;
    private System.Windows.Forms.Label _lblMaxGroupSize;
    private System.Windows.Forms.NumericUpDown _numMaxGroupSize;
    private System.Windows.Forms.Label _lblDenialDedupMinutes;
    private System.Windows.Forms.NumericUpDown _numDenialDedupMinutes;
    private System.Windows.Forms.CheckBox _chkPrefillCompanions;
    private System.Windows.Forms.CheckBox _chkDenialSound;
    private System.Windows.Forms.TabPage _tabSecurity;
    private System.Windows.Forms.TableLayoutPanel _tblSecurity;
    private System.Windows.Forms.Label _lblMaxFailedLogins;
    private System.Windows.Forms.NumericUpDown _numMaxFailedLogins;
    private System.Windows.Forms.Label _lblLockoutMinutes;
    private System.Windows.Forms.NumericUpDown _numLockoutMinutes;
    private System.Windows.Forms.Label _lblAutoLockMinutes;
    private System.Windows.Forms.NumericUpDown _numAutoLockMinutes;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnReload;
    private System.Windows.Forms.Button _btnClose;
}
