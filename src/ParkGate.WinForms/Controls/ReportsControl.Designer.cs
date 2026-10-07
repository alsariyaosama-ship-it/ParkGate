#nullable disable

namespace ParkGate.WinForms.Controls;

partial class ReportsControl
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
        this._pnlTop = new System.Windows.Forms.Panel();
        this._lblTitle = new System.Windows.Forms.Label();
        this._tblFilters = new System.Windows.Forms.TableLayoutPanel();
        this._lblFromDate = new System.Windows.Forms.Label();
        this._dtpFromDate = new System.Windows.Forms.DateTimePicker();
        this._lblToDate = new System.Windows.Forms.Label();
        this._dtpToDate = new System.Windows.Forms.DateTimePicker();
        this._lblOrganization = new System.Windows.Forms.Label();
        this._cboOrganization = new System.Windows.Forms.ComboBox();
        this._lblFilterType = new System.Windows.Forms.Label();
        this._cboFilterType = new System.Windows.Forms.ComboBox();
        this._pnlQuickDates = new System.Windows.Forms.FlowLayoutPanel();
        this._btnToday = new System.Windows.Forms.Button();
        this._btnYesterday = new System.Windows.Forms.Button();
        this._btnThisWeek = new System.Windows.Forms.Button();
        this._btnThisMonth = new System.Windows.Forms.Button();
        this._pnlActions = new System.Windows.Forms.FlowLayoutPanel();
        this._btnRunReport = new System.Windows.Forms.Button();
        this._btnExportExcel = new System.Windows.Forms.Button();
        this._btnPrintPreview = new System.Windows.Forms.Button();
        this._tabControl = new System.Windows.Forms.TabControl();
        this._tabDaily = new System.Windows.Forms.TabPage();
        this._pnlDailyKpi = new System.Windows.Forms.FlowLayoutPanel();
        this._splitDaily = new System.Windows.Forms.SplitContainer();
        this._grpDailyOrgs = new System.Windows.Forms.GroupBox();
        this._gridDailyOrgs = new System.Windows.Forms.DataGridView();
        this._grpDailyCategories = new System.Windows.Forms.GroupBox();
        this._gridDailyCategories = new System.Windows.Forms.DataGridView();
        this._tabDateRange = new System.Windows.Forms.TabPage();
        this._gridDateRange = new System.Windows.Forms.DataGridView();
        this._lblDateRangeTotals = new System.Windows.Forms.Label();
        this._tabOrgStats = new System.Windows.Forms.TabPage();
        this._gridOrgStats = new System.Windows.Forms.DataGridView();
        this._lblOrgStatsTotals = new System.Windows.Forms.Label();
        this._tabOperatorWs = new System.Windows.Forms.TabPage();
        this._splitOperatorWs = new System.Windows.Forms.SplitContainer();
        this._grpOperators = new System.Windows.Forms.GroupBox();
        this._gridOperators = new System.Windows.Forms.DataGridView();
        this._grpWorkstations = new System.Windows.Forms.GroupBox();
        this._gridWorkstations = new System.Windows.Forms.DataGridView();
        this._tabDenials = new System.Windows.Forms.TabPage();
        this._gridDenials = new System.Windows.Forms.DataGridView();
        this._lblDenialsCount = new System.Windows.Forms.Label();
        this._tabHourly = new System.Windows.Forms.TabPage();
        this._gridHourly = new System.Windows.Forms.DataGridView();
        this._lblHourlyTotals = new System.Windows.Forms.Label();
        this._lblStatus = new System.Windows.Forms.Label();

        this._pnlTop.SuspendLayout();
        this._tblFilters.SuspendLayout();
        this._pnlQuickDates.SuspendLayout();
        this._pnlActions.SuspendLayout();
        this._tabControl.SuspendLayout();
        this._tabDaily.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._splitDaily)).BeginInit();
        this._splitDaily.Panel1.SuspendLayout();
        this._splitDaily.Panel2.SuspendLayout();
        this._splitDaily.SuspendLayout();
        this._grpDailyOrgs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridDailyOrgs)).BeginInit();
        this._grpDailyCategories.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridDailyCategories)).BeginInit();
        this._tabDateRange.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridDateRange)).BeginInit();
        this._tabOrgStats.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridOrgStats)).BeginInit();
        this._tabOperatorWs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._splitOperatorWs)).BeginInit();
        this._splitOperatorWs.Panel1.SuspendLayout();
        this._splitOperatorWs.Panel2.SuspendLayout();
        this._splitOperatorWs.SuspendLayout();
        this._grpOperators.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridOperators)).BeginInit();
        this._grpWorkstations.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridWorkstations)).BeginInit();
        this._tabDenials.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridDenials)).BeginInit();
        this._tabHourly.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridHourly)).BeginInit();
        this.SuspendLayout();

        // _pnlTop
        this._pnlTop.Controls.Add(this._pnlActions);
        this._pnlTop.Controls.Add(this._pnlQuickDates);
        this._pnlTop.Controls.Add(this._tblFilters);
        this._pnlTop.Controls.Add(this._lblTitle);
        this._pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlTop.Padding = new System.Windows.Forms.Padding(12);
        this._pnlTop.Size = new System.Drawing.Size(1100, 150);
        this._pnlTop.TabIndex = 0;

        // _lblTitle
        this._lblTitle.AutoSize = true;
        this._lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this._lblTitle.Font = new System.Drawing.Font("Cairo", 14F, System.Drawing.FontStyle.Bold);
        this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this._lblTitle.Name = "_lblTitle";
        this._lblTitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
        this._lblTitle.Size = new System.Drawing.Size(180, 33);
        this._lblTitle.Text = "التقارير والإحصائيات";

        // _tblFilters
        this._tblFilters.ColumnCount = 8;
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
        this._tblFilters.Controls.Add(this._lblFromDate, 0, 0);
        this._tblFilters.Controls.Add(this._dtpFromDate, 1, 0);
        this._tblFilters.Controls.Add(this._lblToDate, 2, 0);
        this._tblFilters.Controls.Add(this._dtpToDate, 3, 0);
        this._tblFilters.Controls.Add(this._lblOrganization, 4, 0);
        this._tblFilters.Controls.Add(this._cboOrganization, 5, 0);
        this._tblFilters.Controls.Add(this._lblFilterType, 6, 0);
        this._tblFilters.Controls.Add(this._cboFilterType, 7, 0);
        this._tblFilters.Dock = System.Windows.Forms.DockStyle.Top;
        this._tblFilters.Location = new System.Drawing.Point(12, 45);
        this._tblFilters.Name = "_tblFilters";
        this._tblFilters.RowCount = 1;
        this._tblFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblFilters.Size = new System.Drawing.Size(1076, 36);
        this._tblFilters.TabIndex = 1;

        this._lblFromDate.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblFromDate.AutoSize = true;
        this._lblFromDate.Text = "من تاريخ:";
        this._dtpFromDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;

        this._lblToDate.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblToDate.AutoSize = true;
        this._lblToDate.Text = "إلى تاريخ:";
        this._dtpToDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;

        this._lblOrganization.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblOrganization.AutoSize = true;
        this._lblOrganization.Text = "الجهة التضامنية:";
        this._cboOrganization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._cboOrganization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

        this._lblFilterType.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblFilterType.AutoSize = true;
        this._lblFilterType.Text = "نوع السجل:";
        this._cboFilterType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._cboFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

        // _pnlQuickDates
        this._pnlQuickDates.Controls.Add(this._btnToday);
        this._pnlQuickDates.Controls.Add(this._btnYesterday);
        this._pnlQuickDates.Controls.Add(this._btnThisWeek);
        this._pnlQuickDates.Controls.Add(this._btnThisMonth);
        this._pnlQuickDates.Dock = System.Windows.Forms.DockStyle.Left;
        this._pnlQuickDates.Location = new System.Drawing.Point(12, 81);
        this._pnlQuickDates.Name = "_pnlQuickDates";
        this._pnlQuickDates.Size = new System.Drawing.Size(420, 42);
        this._pnlQuickDates.TabIndex = 2;

        this._btnToday.Text = "اليوم";
        this._btnToday.Size = new System.Drawing.Size(80, 32);
        this._btnYesterday.Text = "أمس";
        this._btnYesterday.Size = new System.Drawing.Size(80, 32);
        this._btnThisWeek.Text = "هذا الأسبوع";
        this._btnThisWeek.Size = new System.Drawing.Size(95, 32);
        this._btnThisMonth.Text = "هذا الشهر";
        this._btnThisMonth.Size = new System.Drawing.Size(95, 32);

        // _pnlActions
        this._pnlActions.Controls.Add(this._btnRunReport);
        this._pnlActions.Controls.Add(this._btnExportExcel);
        this._pnlActions.Controls.Add(this._btnPrintPreview);
        this._pnlActions.Dock = System.Windows.Forms.DockStyle.Right;
        this._pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this._pnlActions.Location = new System.Drawing.Point(600, 81);
        this._pnlActions.Name = "_pnlActions";
        this._pnlActions.Size = new System.Drawing.Size(488, 42);
        this._pnlActions.TabIndex = 3;

        this._btnRunReport.BackColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this._btnRunReport.ForeColor = System.Drawing.Color.White;
        this._btnRunReport.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold);
        this._btnRunReport.Size = new System.Drawing.Size(120, 34);
        this._btnRunReport.Text = "عرض التقرير";

        this._btnExportExcel.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        this._btnExportExcel.ForeColor = System.Drawing.Color.White;
        this._btnExportExcel.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold);
        this._btnExportExcel.Size = new System.Drawing.Size(140, 34);
        this._btnExportExcel.Text = "تصدير إلى Excel";

        this._btnPrintPreview.BackColor = System.Drawing.Color.FromArgb(55, 71, 79);
        this._btnPrintPreview.ForeColor = System.Drawing.Color.White;
        this._btnPrintPreview.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold);
        this._btnPrintPreview.Size = new System.Drawing.Size(130, 34);
        this._btnPrintPreview.Text = "معاينة الطباعة";

        // _tabControl
        this._tabControl.Controls.Add(this._tabDaily);
        this._tabControl.Controls.Add(this._tabDateRange);
        this._tabControl.Controls.Add(this._tabOrgStats);
        this._tabControl.Controls.Add(this._tabOperatorWs);
        this._tabControl.Controls.Add(this._tabDenials);
        this._tabControl.Controls.Add(this._tabHourly);
        this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tabControl.Font = new System.Drawing.Font("Cairo", 10F);
        this._tabControl.ItemSize = new System.Drawing.Size(150, 32);
        this._tabControl.Location = new System.Drawing.Point(0, 150);
        this._tabControl.Name = "_tabControl";
        this._tabControl.SelectedIndex = 0;
        this._tabControl.Size = new System.Drawing.Size(1100, 520);
        this._tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
        this._tabControl.TabIndex = 1;

        // Tab 1: Daily Summary
        this._tabDaily.Controls.Add(this._splitDaily);
        this._tabDaily.Controls.Add(this._pnlDailyKpi);
        this._tabDaily.Location = new System.Drawing.Point(4, 36);
        this._tabDaily.Name = "_tabDaily";
        this._tabDaily.Padding = new System.Windows.Forms.Padding(8);
        this._tabDaily.Size = new System.Drawing.Size(1092, 480);
        this._tabDaily.Text = "الملخص اليومي";
        this._tabDaily.UseVisualStyleBackColor = true;

        this._pnlDailyKpi.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlDailyKpi.Height = 85;
        this._pnlDailyKpi.Name = "_pnlDailyKpi";
        this._pnlDailyKpi.Padding = new System.Windows.Forms.Padding(4);

        this._splitDaily.Dock = System.Windows.Forms.DockStyle.Fill;
        this._splitDaily.Location = new System.Drawing.Point(8, 93);
        this._splitDaily.Name = "_splitDaily";
        this._splitDaily.Panel1.Controls.Add(this._grpDailyOrgs);
        this._splitDaily.Panel2.Controls.Add(this._grpDailyCategories);
        this._splitDaily.SplitterDistance = 700;
        this._splitDaily.Size = new System.Drawing.Size(1076, 379);

        this._grpDailyOrgs.Controls.Add(this._gridDailyOrgs);
        this._grpDailyOrgs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpDailyOrgs.Name = "_grpDailyOrgs";
        this._grpDailyOrgs.Size = new System.Drawing.Size(700, 379);
        this._grpDailyOrgs.Text = "حركة الدخول حسب الجهات";

        this._gridDailyOrgs.AllowUserToAddRows = false;
        this._gridDailyOrgs.AllowUserToDeleteRows = false;
        this._gridDailyOrgs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridDailyOrgs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridDailyOrgs.ReadOnly = true;
        this._gridDailyOrgs.RowHeadersVisible = false;

        this._grpDailyCategories.Controls.Add(this._gridDailyCategories);
        this._grpDailyCategories.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpDailyCategories.Name = "_grpDailyCategories";
        this._grpDailyCategories.Size = new System.Drawing.Size(372, 379);
        this._grpDailyCategories.Text = "المرافقون حسب الفئات العمرية";

        this._gridDailyCategories.AllowUserToAddRows = false;
        this._gridDailyCategories.AllowUserToDeleteRows = false;
        this._gridDailyCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridDailyCategories.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridDailyCategories.ReadOnly = true;
        this._gridDailyCategories.RowHeadersVisible = false;

        // Tab 2: Date Range
        this._tabDateRange.Controls.Add(this._gridDateRange);
        this._tabDateRange.Controls.Add(this._lblDateRangeTotals);
        this._tabDateRange.Location = new System.Drawing.Point(4, 36);
        this._tabDateRange.Name = "_tabDateRange";
        this._tabDateRange.Padding = new System.Windows.Forms.Padding(8);
        this._tabDateRange.Size = new System.Drawing.Size(1092, 480);
        this._tabDateRange.Text = "ملخص الفترة";
        this._tabDateRange.UseVisualStyleBackColor = true;

        this._lblDateRangeTotals.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblDateRangeTotals.Height = 40;
        this._lblDateRangeTotals.BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
        this._lblDateRangeTotals.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold);
        this._lblDateRangeTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._lblDateRangeTotals.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);

        this._gridDateRange.AllowUserToAddRows = false;
        this._gridDateRange.AllowUserToDeleteRows = false;
        this._gridDateRange.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridDateRange.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridDateRange.ReadOnly = true;
        this._gridDateRange.RowHeadersVisible = false;

        // Tab 3: Org Statistics (High Priority)
        this._tabOrgStats.Controls.Add(this._gridOrgStats);
        this._tabOrgStats.Controls.Add(this._lblOrgStatsTotals);
        this._tabOrgStats.Location = new System.Drawing.Point(4, 36);
        this._tabOrgStats.Name = "_tabOrgStats";
        this._tabOrgStats.Padding = new System.Windows.Forms.Padding(8);
        this._tabOrgStats.Size = new System.Drawing.Size(1092, 480);
        this._tabOrgStats.Text = "إحصائيات الجهات";
        this._tabOrgStats.UseVisualStyleBackColor = true;

        this._lblOrgStatsTotals.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblOrgStatsTotals.Height = 40;
        this._lblOrgStatsTotals.BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
        this._lblOrgStatsTotals.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold);
        this._lblOrgStatsTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._lblOrgStatsTotals.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);

        this._gridOrgStats.AllowUserToAddRows = false;
        this._gridOrgStats.AllowUserToDeleteRows = false;
        this._gridOrgStats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridOrgStats.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridOrgStats.ReadOnly = true;
        this._gridOrgStats.RowHeadersVisible = false;

        // Tab 4: Operator & Workstations
        this._tabOperatorWs.Controls.Add(this._splitOperatorWs);
        this._tabOperatorWs.Location = new System.Drawing.Point(4, 36);
        this._tabOperatorWs.Name = "_tabOperatorWs";
        this._tabOperatorWs.Padding = new System.Windows.Forms.Padding(8);
        this._tabOperatorWs.Size = new System.Drawing.Size(1092, 480);
        this._tabOperatorWs.Text = "المشغلون ونقاط الدخول";
        this._tabOperatorWs.UseVisualStyleBackColor = true;

        this._splitOperatorWs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._splitOperatorWs.Orientation = System.Windows.Forms.Orientation.Horizontal;
        this._splitOperatorWs.SplitterDistance = 230;
        this._splitOperatorWs.Panel1.Controls.Add(this._grpOperators);
        this._splitOperatorWs.Panel2.Controls.Add(this._grpWorkstations);

        this._grpOperators.Controls.Add(this._gridOperators);
        this._grpOperators.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpOperators.Name = "_grpOperators";
        this._grpOperators.Text = "إحصائيات المشغلين";

        this._gridOperators.AllowUserToAddRows = false;
        this._gridOperators.AllowUserToDeleteRows = false;
        this._gridOperators.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridOperators.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridOperators.ReadOnly = true;
        this._gridOperators.RowHeadersVisible = false;

        this._grpWorkstations.Controls.Add(this._gridWorkstations);
        this._grpWorkstations.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpWorkstations.Name = "_grpWorkstations";
        this._grpWorkstations.Text = "إحصائيات نقاط الدخول";

        this._gridWorkstations.AllowUserToAddRows = false;
        this._gridWorkstations.AllowUserToDeleteRows = false;
        this._gridWorkstations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridWorkstations.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridWorkstations.ReadOnly = true;
        this._gridWorkstations.RowHeadersVisible = false;

        // Tab 5: Denials & Overrides
        this._tabDenials.Controls.Add(this._gridDenials);
        this._tabDenials.Controls.Add(this._lblDenialsCount);
        this._tabDenials.Location = new System.Drawing.Point(4, 36);
        this._tabDenials.Name = "_tabDenials";
        this._tabDenials.Padding = new System.Windows.Forms.Padding(8);
        this._tabDenials.Size = new System.Drawing.Size(1092, 480);
        this._tabDenials.Text = "المرفوضات والاستثناءات";
        this._tabDenials.UseVisualStyleBackColor = true;

        this._lblDenialsCount.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblDenialsCount.Height = 35;
        this._lblDenialsCount.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold);
        this._lblDenialsCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._lblDenialsCount.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);

        this._gridDenials.AllowUserToAddRows = false;
        this._gridDenials.AllowUserToDeleteRows = false;
        this._gridDenials.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridDenials.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridDenials.ReadOnly = true;
        this._gridDenials.RowHeadersVisible = false;

        // Tab 6: Hourly Distribution
        this._tabHourly.Controls.Add(this._gridHourly);
        this._tabHourly.Controls.Add(this._lblHourlyTotals);
        this._tabHourly.Location = new System.Drawing.Point(4, 36);
        this._tabHourly.Name = "_tabHourly";
        this._tabHourly.Padding = new System.Windows.Forms.Padding(8);
        this._tabHourly.Size = new System.Drawing.Size(1092, 480);
        this._tabHourly.Text = "التوزيع الساعي";
        this._tabHourly.UseVisualStyleBackColor = true;

        this._lblHourlyTotals.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblHourlyTotals.Height = 40;
        this._lblHourlyTotals.BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
        this._lblHourlyTotals.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold);
        this._lblHourlyTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this._lblHourlyTotals.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);

        this._gridHourly.AllowUserToAddRows = false;
        this._gridHourly.AllowUserToDeleteRows = false;
        this._gridHourly.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridHourly.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridHourly.ReadOnly = true;
        this._gridHourly.RowHeadersVisible = false;

        // _lblStatus
        this._lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblStatus.Height = 25;
        this._lblStatus.Font = new System.Drawing.Font("Cairo", 9F);
        this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(69, 90, 100);
        this._lblStatus.Name = "_lblStatus";
        this._lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);

        // Control
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this._tabControl);
        this.Controls.Add(this._pnlTop);
        this.Controls.Add(this._lblStatus);
        this.Font = new System.Drawing.Font("Cairo", 10F);
        this.Name = "ReportsControl";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(1100, 695);

        this._pnlTop.ResumeLayout(false);
        this._pnlTop.PerformLayout();
        this._tblFilters.ResumeLayout(false);
        this._tblFilters.PerformLayout();
        this._pnlQuickDates.ResumeLayout(false);
        this._pnlActions.ResumeLayout(false);
        this._tabControl.ResumeLayout(false);
        this._tabDaily.ResumeLayout(false);
        this._splitDaily.Panel1.ResumeLayout(false);
        this._splitDaily.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._splitDaily)).EndInit();
        this._splitDaily.ResumeLayout(false);
        this._grpDailyOrgs.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridDailyOrgs)).EndInit();
        this._grpDailyCategories.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridDailyCategories)).EndInit();
        this._tabDateRange.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridDateRange)).EndInit();
        this._tabOrgStats.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridOrgStats)).EndInit();
        this._splitOperatorWs.Panel1.ResumeLayout(false);
        this._splitOperatorWs.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._splitOperatorWs)).EndInit();
        this._splitOperatorWs.ResumeLayout(false);
        this._grpOperators.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridOperators)).EndInit();
        this._grpWorkstations.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridWorkstations)).EndInit();
        this._tabDenials.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridDenials)).EndInit();
        this._tabHourly.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridHourly)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlTop;
    private System.Windows.Forms.Label _lblTitle;
    private System.Windows.Forms.TableLayoutPanel _tblFilters;
    private System.Windows.Forms.Label _lblFromDate;
    private System.Windows.Forms.DateTimePicker _dtpFromDate;
    private System.Windows.Forms.Label _lblToDate;
    private System.Windows.Forms.DateTimePicker _dtpToDate;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.ComboBox _cboOrganization;
    private System.Windows.Forms.Label _lblFilterType;
    private System.Windows.Forms.ComboBox _cboFilterType;
    private System.Windows.Forms.FlowLayoutPanel _pnlQuickDates;
    private System.Windows.Forms.Button _btnToday;
    private System.Windows.Forms.Button _btnYesterday;
    private System.Windows.Forms.Button _btnThisWeek;
    private System.Windows.Forms.Button _btnThisMonth;
    private System.Windows.Forms.FlowLayoutPanel _pnlActions;
    private System.Windows.Forms.Button _btnRunReport;
    private System.Windows.Forms.Button _btnExportExcel;
    private System.Windows.Forms.Button _btnPrintPreview;
    private System.Windows.Forms.TabControl _tabControl;
    private System.Windows.Forms.TabPage _tabDaily;
    private System.Windows.Forms.FlowLayoutPanel _pnlDailyKpi;
    private System.Windows.Forms.SplitContainer _splitDaily;
    private System.Windows.Forms.GroupBox _grpDailyOrgs;
    private System.Windows.Forms.DataGridView _gridDailyOrgs;
    private System.Windows.Forms.GroupBox _grpDailyCategories;
    private System.Windows.Forms.DataGridView _gridDailyCategories;
    private System.Windows.Forms.TabPage _tabDateRange;
    private System.Windows.Forms.DataGridView _gridDateRange;
    private System.Windows.Forms.Label _lblDateRangeTotals;
    private System.Windows.Forms.TabPage _tabOrgStats;
    private System.Windows.Forms.DataGridView _gridOrgStats;
    private System.Windows.Forms.Label _lblOrgStatsTotals;
    private System.Windows.Forms.TabPage _tabOperatorWs;
    private System.Windows.Forms.SplitContainer _splitOperatorWs;
    private System.Windows.Forms.GroupBox _grpOperators;
    private System.Windows.Forms.DataGridView _gridOperators;
    private System.Windows.Forms.GroupBox _grpWorkstations;
    private System.Windows.Forms.DataGridView _gridWorkstations;
    private System.Windows.Forms.TabPage _tabDenials;
    private System.Windows.Forms.DataGridView _gridDenials;
    private System.Windows.Forms.Label _lblDenialsCount;
    private System.Windows.Forms.TabPage _tabHourly;
    private System.Windows.Forms.DataGridView _gridHourly;
    private System.Windows.Forms.Label _lblHourlyTotals;
    private System.Windows.Forms.Label _lblStatus;
}
