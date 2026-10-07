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
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._tabSettings = new System.Windows.Forms.TabControl();
        this._tabGeneral = new System.Windows.Forms.TabPage();
        this._tblGeneral = new System.Windows.Forms.TableLayoutPanel();
        this._lblParkName = new System.Windows.Forms.Label();
        this._txtParkName = new System.Windows.Forms.TextBox();
        this._lblTimeZone = new System.Windows.Forms.Label();
        this._txtTimeZone = new System.Windows.Forms.TextBox();
        this._tabEntry = new System.Windows.Forms.TabPage();
        this._tblEntry = new System.Windows.Forms.TableLayoutPanel();
        this._lblCooldownHours = new System.Windows.Forms.Label();
        this._numCooldownHours = new System.Windows.Forms.NumericUpDown();
        this._lblMaxGroupSize = new System.Windows.Forms.Label();
        this._numMaxGroupSize = new System.Windows.Forms.NumericUpDown();
        this._lblDenialDedupMinutes = new System.Windows.Forms.Label();
        this._numDenialDedupMinutes = new System.Windows.Forms.NumericUpDown();
        this._chkPrefillCompanions = new System.Windows.Forms.CheckBox();
        this._chkDenialSound = new System.Windows.Forms.CheckBox();
        this._tabSecurity = new System.Windows.Forms.TabPage();
        this._tblSecurity = new System.Windows.Forms.TableLayoutPanel();
        this._lblMaxFailedLogins = new System.Windows.Forms.Label();
        this._numMaxFailedLogins = new System.Windows.Forms.NumericUpDown();
        this._lblLockoutMinutes = new System.Windows.Forms.Label();
        this._numLockoutMinutes = new System.Windows.Forms.NumericUpDown();
        this._lblAutoLockMinutes = new System.Windows.Forms.Label();
        this._numAutoLockMinutes = new System.Windows.Forms.NumericUpDown();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnReload = new System.Windows.Forms.Button();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tabSettings.SuspendLayout();
        this._tabGeneral.SuspendLayout();
        this._tblGeneral.SuspendLayout();
        this._tabEntry.SuspendLayout();
        this._tblEntry.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numCooldownHours)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxGroupSize)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numDenialDedupMinutes)).BeginInit();
        this._tabSecurity.SuspendLayout();
        this._tblSecurity.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxFailedLogins)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numLockoutMinutes)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._numAutoLockMinutes)).BeginInit();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(650, 50);
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
        this._lblHeader.Size = new System.Drawing.Size(650, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إعدادات النظام";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tabSettings
        // 
        this._tabSettings.Controls.Add(this._tabGeneral);
        this._tabSettings.Controls.Add(this._tabEntry);
        this._tabSettings.Controls.Add(this._tabSecurity);
        this._tabSettings.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tabSettings.Location = new System.Drawing.Point(0, 50);
        this._tabSettings.Name = "_tabSettings";
        this._tabSettings.SelectedIndex = 0;
        this._tabSettings.Size = new System.Drawing.Size(650, 310);
        this._tabSettings.TabIndex = 1;
        // 
        // _tabGeneral
        // 
        this._tabGeneral.Controls.Add(this._tblGeneral);
        this._tabGeneral.Location = new System.Drawing.Point(4, 26);
        this._tabGeneral.Name = "_tabGeneral";
        this._tabGeneral.Padding = new System.Windows.Forms.Padding(12);
        this._tabGeneral.Size = new System.Drawing.Size(642, 280);
        this._tabGeneral.TabIndex = 0;
        this._tabGeneral.Text = "إعدادات عامة";
        this._tabGeneral.UseVisualStyleBackColor = true;
        // 
        // _tblGeneral
        // 
        this._tblGeneral.ColumnCount = 2;
        this._tblGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
        this._tblGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblGeneral.Controls.Add(this._lblParkName, 0, 0);
        this._tblGeneral.Controls.Add(this._txtParkName, 1, 0);
        this._tblGeneral.Controls.Add(this._lblTimeZone, 0, 1);
        this._tblGeneral.Controls.Add(this._txtTimeZone, 1, 1);
        this._tblGeneral.Dock = System.Windows.Forms.DockStyle.Top;
        this._tblGeneral.Location = new System.Drawing.Point(12, 12);
        this._tblGeneral.Name = "_tblGeneral";
        this._tblGeneral.RowCount = 2;
        this._tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblGeneral.Size = new System.Drawing.Size(618, 80);
        this._tblGeneral.TabIndex = 0;
        // 
        // _lblParkName
        // 
        this._lblParkName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblParkName.AutoSize = true;
        this._lblParkName.Location = new System.Drawing.Point(448, 10);
        this._lblParkName.Name = "_lblParkName";
        this._lblParkName.Size = new System.Drawing.Size(167, 19);
        this._lblParkName.TabIndex = 0;
        this._lblParkName.Text = "اسم المنتزه / المرفق العائلي:";
        // 
        // _txtParkName
        // 
        this._txtParkName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtParkName.Location = new System.Drawing.Point(3, 7);
        this._txtParkName.MaxLength = 100;
        this._txtParkName.Name = "_txtParkName";
        this._txtParkName.Size = new System.Drawing.Size(392, 25);
        this._txtParkName.TabIndex = 1;
        // 
        // _lblTimeZone
        // 
        this._lblTimeZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblTimeZone.AutoSize = true;
        this._lblTimeZone.Location = new System.Drawing.Point(510, 50);
        this._lblTimeZone.Name = "_lblTimeZone";
        this._lblTimeZone.Size = new System.Drawing.Size(105, 19);
        this._lblTimeZone.TabIndex = 2;
        this._lblTimeZone.Text = "المنطقة الزمنية:";
        // 
        // _txtTimeZone
        // 
        this._txtTimeZone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtTimeZone.Location = new System.Drawing.Point(3, 47);
        this._txtTimeZone.MaxLength = 100;
        this._txtTimeZone.Name = "_txtTimeZone";
        this._txtTimeZone.Size = new System.Drawing.Size(392, 25);
        this._txtTimeZone.TabIndex = 3;
        // 
        // _tabEntry
        // 
        this._tabEntry.Controls.Add(this._tblEntry);
        this._tabEntry.Location = new System.Drawing.Point(4, 24);
        this._tabEntry.Name = "_tabEntry";
        this._tabEntry.Padding = new System.Windows.Forms.Padding(12);
        this._tabEntry.Size = new System.Drawing.Size(642, 282);
        this._tabEntry.TabIndex = 1;
        this._tabEntry.Text = "إعدادات الدخول وفترة السماح";
        this._tabEntry.UseVisualStyleBackColor = true;
        // 
        // _tblEntry
        // 
        this._tblEntry.ColumnCount = 2;
        this._tblEntry.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
        this._tblEntry.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblEntry.Controls.Add(this._lblCooldownHours, 0, 0);
        this._tblEntry.Controls.Add(this._numCooldownHours, 1, 0);
        this._tblEntry.Controls.Add(this._lblMaxGroupSize, 0, 1);
        this._tblEntry.Controls.Add(this._numMaxGroupSize, 1, 1);
        this._tblEntry.Controls.Add(this._lblDenialDedupMinutes, 0, 2);
        this._tblEntry.Controls.Add(this._numDenialDedupMinutes, 1, 2);
        this._tblEntry.Controls.Add(this._chkPrefillCompanions, 0, 3);
        this._tblEntry.Controls.Add(this._chkDenialSound, 0, 4);
        this._tblEntry.Dock = System.Windows.Forms.DockStyle.Top;
        this._tblEntry.Location = new System.Drawing.Point(12, 12);
        this._tblEntry.Name = "_tblEntry";
        this._tblEntry.RowCount = 5;
        this._tblEntry.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblEntry.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblEntry.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblEntry.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblEntry.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblEntry.Size = new System.Drawing.Size(618, 190);
        this._tblEntry.TabIndex = 0;
        // 
        // _lblCooldownHours
        // 
        this._lblCooldownHours.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCooldownHours.AutoSize = true;
        this._lblCooldownHours.Location = new System.Drawing.Point(269, 9);
        this._lblCooldownHours.Name = "_lblCooldownHours";
        this._lblCooldownHours.Size = new System.Drawing.Size(346, 19);
        this._lblCooldownHours.TabIndex = 0;
        this._lblCooldownHours.Text = "فترة السماح الافتراضية بين الزيارات للزبون (بالساعات):";
        // 
        // _numCooldownHours
        // 
        this._numCooldownHours.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numCooldownHours.Location = new System.Drawing.Point(135, 6);
        this._numCooldownHours.Maximum = new decimal(new int[] { 168, 0, 0, 0 });
        this._numCooldownHours.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numCooldownHours.Name = "_numCooldownHours";
        this._numCooldownHours.Size = new System.Drawing.Size(120, 25);
        this._numCooldownHours.TabIndex = 1;
        this._numCooldownHours.Value = new decimal(new int[] { 12, 0, 0, 0 });
        // 
        // _lblMaxGroupSize
        // 
        this._lblMaxGroupSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblMaxGroupSize.AutoSize = true;
        this._lblMaxGroupSize.Location = new System.Drawing.Point(273, 47);
        this._lblMaxGroupSize.Name = "_lblMaxGroupSize";
        this._lblMaxGroupSize.Size = new System.Drawing.Size(342, 19);
        this._lblMaxGroupSize.TabIndex = 2;
        this._lblMaxGroupSize.Text = "الحد الأقصى لعدد الأفراد في المجموعة (شامل الزبون):";
        // 
        // _numMaxGroupSize
        // 
        this._numMaxGroupSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numMaxGroupSize.Location = new System.Drawing.Point(135, 44);
        this._numMaxGroupSize.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        this._numMaxGroupSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numMaxGroupSize.Name = "_numMaxGroupSize";
        this._numMaxGroupSize.Size = new System.Drawing.Size(120, 25);
        this._numMaxGroupSize.TabIndex = 3;
        this._numMaxGroupSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // _lblDenialDedupMinutes
        // 
        this._lblDenialDedupMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblDenialDedupMinutes.AutoSize = true;
        this._lblDenialDedupMinutes.Location = new System.Drawing.Point(271, 85);
        this._lblDenialDedupMinutes.Name = "_lblDenialDedupMinutes";
        this._lblDenialDedupMinutes.Size = new System.Drawing.Size(344, 19);
        this._lblDenialDedupMinutes.TabIndex = 4;
        this._lblDenialDedupMinutes.Text = "مدة عدم تكرار تسجيل المحاولات المرفوضة المتتالية (بالدقائق):";
        // 
        // _numDenialDedupMinutes
        // 
        this._numDenialDedupMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numDenialDedupMinutes.Location = new System.Drawing.Point(135, 82);
        this._numDenialDedupMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        this._numDenialDedupMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numDenialDedupMinutes.Name = "_numDenialDedupMinutes";
        this._numDenialDedupMinutes.Size = new System.Drawing.Size(120, 25);
        this._numDenialDedupMinutes.TabIndex = 5;
        this._numDenialDedupMinutes.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // _chkPrefillCompanions
        // 
        this._chkPrefillCompanions.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkPrefillCompanions.AutoSize = true;
        this._tblEntry.SetColumnSpan(this._chkPrefillCompanions, 2);
        this._chkPrefillCompanions.Location = new System.Drawing.Point(271, 121);
        this._chkPrefillCompanions.Name = "_chkPrefillCompanions";
        this._chkPrefillCompanions.Size = new System.Drawing.Size(344, 23);
        this._chkPrefillCompanions.TabIndex = 6;
        this._chkPrefillCompanions.Text = "تعبئة أعداد المرافقين تلقائياً من الزيارة السابقة للمستفيد";
        this._chkPrefillCompanions.UseVisualStyleBackColor = true;
        // 
        // _chkDenialSound
        // 
        this._chkDenialSound.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkDenialSound.AutoSize = true;
        this._tblEntry.SetColumnSpan(this._chkDenialSound, 2);
        this._chkDenialSound.Location = new System.Drawing.Point(389, 159);
        this._chkDenialSound.Name = "_chkDenialSound";
        this._chkDenialSound.Size = new System.Drawing.Size(226, 23);
        this._chkDenialSound.TabIndex = 7;
        this._chkDenialSound.Text = "تشغيل تنبيه صوتي عند رفض الدخول";
        this._chkDenialSound.UseVisualStyleBackColor = true;
        // 
        // _tabSecurity
        // 
        this._tabSecurity.Controls.Add(this._tblSecurity);
        this._tabSecurity.Location = new System.Drawing.Point(4, 24);
        this._tabSecurity.Name = "_tabSecurity";
        this._tabSecurity.Padding = new System.Windows.Forms.Padding(12);
        this._tabSecurity.Size = new System.Drawing.Size(642, 282);
        this._tabSecurity.TabIndex = 2;
        this._tabSecurity.Text = "إعدادات الأمان والقفل";
        this._tabSecurity.UseVisualStyleBackColor = true;
        // 
        // _tblSecurity
        // 
        this._tblSecurity.ColumnCount = 2;
        this._tblSecurity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
        this._tblSecurity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblSecurity.Controls.Add(this._lblMaxFailedLogins, 0, 0);
        this._tblSecurity.Controls.Add(this._numMaxFailedLogins, 1, 0);
        this._tblSecurity.Controls.Add(this._lblLockoutMinutes, 0, 1);
        this._tblSecurity.Controls.Add(this._numLockoutMinutes, 1, 1);
        this._tblSecurity.Controls.Add(this._lblAutoLockMinutes, 0, 2);
        this._tblSecurity.Controls.Add(this._numAutoLockMinutes, 1, 2);
        this._tblSecurity.Dock = System.Windows.Forms.DockStyle.Top;
        this._tblSecurity.Location = new System.Drawing.Point(12, 12);
        this._tblSecurity.Name = "_tblSecurity";
        this._tblSecurity.RowCount = 3;
        this._tblSecurity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblSecurity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblSecurity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
        this._tblSecurity.Size = new System.Drawing.Size(618, 114);
        this._tblSecurity.TabIndex = 0;
        // 
        // _lblMaxFailedLogins
        // 
        this._lblMaxFailedLogins.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblMaxFailedLogins.AutoSize = true;
        this._lblMaxFailedLogins.Location = new System.Drawing.Point(341, 9);
        this._lblMaxFailedLogins.Name = "_lblMaxFailedLogins";
        this._lblMaxFailedLogins.Size = new System.Drawing.Size(274, 19);
        this._lblMaxFailedLogins.TabIndex = 0;
        this._lblMaxFailedLogins.Text = "أقصى عدد لمحاولات الدخول الخاطئة قبل القفل:";
        // 
        // _numMaxFailedLogins
        // 
        this._numMaxFailedLogins.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numMaxFailedLogins.Location = new System.Drawing.Point(135, 6);
        this._numMaxFailedLogins.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        this._numMaxFailedLogins.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
        this._numMaxFailedLogins.Name = "_numMaxFailedLogins";
        this._numMaxFailedLogins.Size = new System.Drawing.Size(120, 25);
        this._numMaxFailedLogins.TabIndex = 1;
        this._numMaxFailedLogins.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // _lblLockoutMinutes
        // 
        this._lblLockoutMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblLockoutMinutes.AutoSize = true;
        this._lblLockoutMinutes.Location = new System.Drawing.Point(359, 47);
        this._lblLockoutMinutes.Name = "_lblLockoutMinutes";
        this._lblLockoutMinutes.Size = new System.Drawing.Size(256, 19);
        this._lblLockoutMinutes.TabIndex = 2;
        this._lblLockoutMinutes.Text = "مدة قفل الحساب بعد الاستنفاد (بالدقائق):";
        // 
        // _numLockoutMinutes
        // 
        this._numLockoutMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numLockoutMinutes.Location = new System.Drawing.Point(135, 44);
        this._numLockoutMinutes.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
        this._numLockoutMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this._numLockoutMinutes.Name = "_numLockoutMinutes";
        this._numLockoutMinutes.Size = new System.Drawing.Size(120, 25);
        this._numLockoutMinutes.TabIndex = 3;
        this._numLockoutMinutes.Value = new decimal(new int[] { 15, 0, 0, 0 });
        // 
        // _lblAutoLockMinutes
        // 
        this._lblAutoLockMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblAutoLockMinutes.AutoSize = true;
        this._lblAutoLockMinutes.Location = new System.Drawing.Point(267, 85);
        this._lblAutoLockMinutes.Name = "_lblAutoLockMinutes";
        this._lblAutoLockMinutes.Size = new System.Drawing.Size(348, 19);
        this._lblAutoLockMinutes.TabIndex = 4;
        this._lblAutoLockMinutes.Text = "مدة قفل الشاشة التلقائي عند الخمول (بالدقائق - 0 للتعطيل):";
        // 
        // _numAutoLockMinutes
        // 
        this._numAutoLockMinutes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._numAutoLockMinutes.Location = new System.Drawing.Point(135, 82);
        this._numAutoLockMinutes.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
        this._numAutoLockMinutes.Name = "_numAutoLockMinutes";
        this._numAutoLockMinutes.Size = new System.Drawing.Size(120, 25);
        this._numAutoLockMinutes.TabIndex = 5;
        this._numAutoLockMinutes.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnReload);
        this._pnlButtons.Controls.Add(this._btnClose);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 360);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(650, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(518, 11);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(100, 32);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnReload
        // 
        this._btnReload.Location = new System.Drawing.Point(412, 11);
        this._btnReload.Name = "_btnReload";
        this._btnReload.Size = new System.Drawing.Size(100, 32);
        this._btnReload.TabIndex = 1;
        this._btnReload.Text = "إعادة تحميل";
        this._btnReload.UseVisualStyleBackColor = true;
        // 
        // _btnClose
        // 
        this._btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClose.Location = new System.Drawing.Point(326, 11);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(80, 32);
        this._btnClose.TabIndex = 2;
        this._btnClose.Text = "إغلاق";
        this._btnClose.UseVisualStyleBackColor = false;
        // 
        // SettingsForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(650, 410);
        this.Controls.Add(this._tabSettings);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إعدادات النظام";
        this._pnlHeader.ResumeLayout(false);
        this._tabSettings.ResumeLayout(false);
        this._tabGeneral.ResumeLayout(false);
        this._tblGeneral.ResumeLayout(false);
        this._tblGeneral.PerformLayout();
        this._tabEntry.ResumeLayout(false);
        this._tblEntry.ResumeLayout(false);
        this._tblEntry.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numCooldownHours)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxGroupSize)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numDenialDedupMinutes)).EndInit();
        this._tabSecurity.ResumeLayout(false);
        this._tblSecurity.ResumeLayout(false);
        this._tblSecurity.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._numMaxFailedLogins)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numLockoutMinutes)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._numAutoLockMinutes)).EndInit();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
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
