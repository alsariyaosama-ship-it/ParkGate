#nullable disable

namespace ParkGate.WinForms.Controls;

partial class BeneficiariesControl
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
        this._pnlSearch = new System.Windows.Forms.Panel();
        this._lblSearch = new System.Windows.Forms.Label();
        this._txtSearch = new System.Windows.Forms.TextBox();
        this._btnSearch = new System.Windows.Forms.Button();
        this._btnClearSearch = new System.Windows.Forms.Button();
        this._lblSearchResultCount = new System.Windows.Forms.Label();
        this._splitMain = new System.Windows.Forms.SplitContainer();
        this._grpResults = new System.Windows.Forms.GroupBox();
        this._gridBeneficiaries = new System.Windows.Forms.DataGridView();
        this._colCardNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colOrganization = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colNationalId = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colLastEntry = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colExpiryDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._splitBottom = new System.Windows.Forms.SplitContainer();
        this._grpHolderDetails = new System.Windows.Forms.GroupBox();
        this._tblDetails = new System.Windows.Forms.TableLayoutPanel();
        this._lblDetailNameTitle = new System.Windows.Forms.Label();
        this._lblDetailName = new System.Windows.Forms.Label();
        this._lblDetailCardTitle = new System.Windows.Forms.Label();
        this._lblDetailCard = new System.Windows.Forms.Label();
        this._lblDetailOrgTitle = new System.Windows.Forms.Label();
        this._lblDetailOrg = new System.Windows.Forms.Label();
        this._lblDetailPhoneTitle = new System.Windows.Forms.Label();
        this._lblDetailPhone = new System.Windows.Forms.Label();
        this._lblDetailNidTitle = new System.Windows.Forms.Label();
        this._lblDetailNid = new System.Windows.Forms.Label();
        this._lblDetailExpiryTitle = new System.Windows.Forms.Label();
        this._lblDetailExpiry = new System.Windows.Forms.Label();
        this._lblDetailStatusTitle = new System.Windows.Forms.Label();
        this._lblDetailStatus = new System.Windows.Forms.Label();
        this._lblDetailReasonTitle = new System.Windows.Forms.Label();
        this._lblDetailReason = new System.Windows.Forms.Label();
        this._lblDetailNotesTitle = new System.Windows.Forms.Label();
        this._lblDetailNotes = new System.Windows.Forms.Label();
        this._pnlDetailsActions = new System.Windows.Forms.FlowLayoutPanel();
        this._btnEdit = new System.Windows.Forms.Button();
        this._btnChangeStatus = new System.Windows.Forms.Button();
        this._btnCheckInDirect = new System.Windows.Forms.Button();
        this._grpHistory = new System.Windows.Forms.GroupBox();
        this._gridHistory = new System.Windows.Forms.DataGridView();
        this._colHistTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistBusinessDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistWorkstation = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistPersons = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistDetails = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHistVoidReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._pnlHistoryActions = new System.Windows.Forms.Panel();
        this._btnVoidHistoryEntry = new System.Windows.Forms.Button();
        this._pnlSearch.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._splitMain)).BeginInit();
        this._splitMain.Panel1.SuspendLayout();
        this._splitMain.Panel2.SuspendLayout();
        this._splitMain.SuspendLayout();
        this._grpResults.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridBeneficiaries)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._splitBottom)).BeginInit();
        this._splitBottom.Panel1.SuspendLayout();
        this._splitBottom.Panel2.SuspendLayout();
        this._splitBottom.SuspendLayout();
        this._grpHolderDetails.SuspendLayout();
        this._tblDetails.SuspendLayout();
        this._pnlDetailsActions.SuspendLayout();
        this._grpHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridHistory)).BeginInit();
        this._pnlHistoryActions.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlSearch
        // 
        this._pnlSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(247)))));
        this._pnlSearch.Controls.Add(this._lblSearch);
        this._pnlSearch.Controls.Add(this._txtSearch);
        this._pnlSearch.Controls.Add(this._btnSearch);
        this._pnlSearch.Controls.Add(this._btnClearSearch);
        this._pnlSearch.Controls.Add(this._lblSearchResultCount);
        this._pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlSearch.Location = new System.Drawing.Point(0, 0);
        this._pnlSearch.Name = "_pnlSearch";
        this._pnlSearch.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
        this._pnlSearch.Size = new System.Drawing.Size(1000, 52);
        this._pnlSearch.TabIndex = 0;
        // 
        // _lblSearch
        // 
        this._lblSearch.AutoSize = true;
        this._lblSearch.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblSearch.Location = new System.Drawing.Point(900, 16);
        this._lblSearch.Name = "_lblSearch";
        this._lblSearch.Size = new System.Drawing.Size(90, 19);
        this._lblSearch.TabIndex = 0;
        this._lblSearch.Text = "بحث سريع:";
        // 
        // _txtSearch
        // 
        this._txtSearch.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._txtSearch.Location = new System.Drawing.Point(540, 12);
        this._txtSearch.MaxLength = 100;
        this._txtSearch.Name = "_txtSearch";
        this._txtSearch.PlaceholderText = "رقم البطاقة / الاسم / الهاتف / الرقم الوطني...";
        this._txtSearch.Size = new System.Drawing.Size(350, 27);
        this._txtSearch.TabIndex = 1;
        // 
        // _btnSearch
        // 
        this._btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSearch.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSearch.ForeColor = System.Drawing.Color.White;
        this._btnSearch.Location = new System.Drawing.Point(450, 11);
        this._btnSearch.Name = "_btnSearch";
        this._btnSearch.Size = new System.Drawing.Size(80, 29);
        this._btnSearch.TabIndex = 2;
        this._btnSearch.Text = "بحث";
        this._btnSearch.UseVisualStyleBackColor = false;
        // 
        // _btnClearSearch
        // 
        this._btnClearSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnClearSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClearSearch.Location = new System.Drawing.Point(360, 11);
        this._btnClearSearch.Name = "_btnClearSearch";
        this._btnClearSearch.Size = new System.Drawing.Size(80, 29);
        this._btnClearSearch.TabIndex = 3;
        this._btnClearSearch.Text = "مسح";
        this._btnClearSearch.UseVisualStyleBackColor = false;
        // 
        // _lblSearchResultCount
        // 
        this._lblSearchResultCount.AutoSize = true;
        this._lblSearchResultCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._lblSearchResultCount.Location = new System.Drawing.Point(20, 16);
        this._lblSearchResultCount.Name = "_lblSearchResultCount";
        this._lblSearchResultCount.Size = new System.Drawing.Size(100, 19);
        this._lblSearchResultCount.TabIndex = 4;
        this._lblSearchResultCount.Text = "عدد النتائج: 0";
        // 
        // _splitMain
        // 
        this._splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this._splitMain.Location = new System.Drawing.Point(0, 52);
        this._splitMain.Name = "_splitMain";
        this._splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // _splitMain.Panel1
        // 
        this._splitMain.Panel1.Controls.Add(this._grpResults);
        this._splitMain.Panel1.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        // 
        // _splitMain.Panel2
        // 
        this._splitMain.Panel2.Controls.Add(this._splitBottom);
        this._splitMain.Panel2.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
        this._splitMain.Size = new System.Drawing.Size(1000, 588);
        this._splitMain.SplitterDistance = 240;
        this._splitMain.TabIndex = 1;
        // 
        // _grpResults
        // 
        this._grpResults.Controls.Add(this._gridBeneficiaries);
        this._grpResults.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpResults.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpResults.Location = new System.Drawing.Point(8, 4);
        this._grpResults.Name = "_grpResults";
        this._grpResults.Padding = new System.Windows.Forms.Padding(6);
        this._grpResults.Size = new System.Drawing.Size(984, 232);
        this._grpResults.TabIndex = 0;
        this._grpResults.TabStop = false;
        this._grpResults.Text = "قائمة المستفيدين المطابقين";
        // 
        // _gridBeneficiaries
        // 
        this._gridBeneficiaries.AllowUserToAddRows = false;
        this._gridBeneficiaries.AllowUserToDeleteRows = false;
        this._gridBeneficiaries.BackgroundColor = System.Drawing.Color.White;
        this._gridBeneficiaries.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridBeneficiaries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridBeneficiaries.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colCardNumber,
            this._colOrganization,
            this._colFullName,
            this._colPhone,
            this._colNationalId,
            this._colStatus,
            this._colLastEntry,
            this._colExpiryDate});
        this._gridBeneficiaries.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridBeneficiaries.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._gridBeneficiaries.Location = new System.Drawing.Point(6, 23);
        this._gridBeneficiaries.MultiSelect = false;
        this._gridBeneficiaries.Name = "_gridBeneficiaries";
        this._gridBeneficiaries.ReadOnly = true;
        this._gridBeneficiaries.RowHeadersVisible = false;
        this._gridBeneficiaries.RowTemplate.Height = 28;
        this._gridBeneficiaries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridBeneficiaries.Size = new System.Drawing.Size(972, 203);
        this._gridBeneficiaries.TabIndex = 0;
        // 
        // _colCardNumber
        // 
        this._colCardNumber.HeaderText = "رقم البطاقة";
        this._colCardNumber.Name = "_colCardNumber";
        this._colCardNumber.ReadOnly = true;
        this._colCardNumber.Width = 110;
        // 
        // _colOrganization
        // 
        this._colOrganization.HeaderText = "الجهة";
        this._colOrganization.Name = "_colOrganization";
        this._colOrganization.ReadOnly = true;
        this._colOrganization.Width = 130;
        // 
        // _colFullName
        // 
        this._colFullName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this._colFullName.HeaderText = "الاسم الكامل";
        this._colFullName.Name = "_colFullName";
        this._colFullName.ReadOnly = true;
        // 
        // _colPhone
        // 
        this._colPhone.HeaderText = "رقم الهاتف";
        this._colPhone.Name = "_colPhone";
        this._colPhone.ReadOnly = true;
        this._colPhone.Width = 110;
        // 
        // _colNationalId
        // 
        this._colNationalId.HeaderText = "الرقم الوطني";
        this._colNationalId.Name = "_colNationalId";
        this._colNationalId.ReadOnly = true;
        this._colNationalId.Width = 120;
        // 
        // _colStatus
        // 
        this._colStatus.HeaderText = "الحالة";
        this._colStatus.Name = "_colStatus";
        this._colStatus.ReadOnly = true;
        this._colStatus.Width = 90;
        // 
        // _colLastEntry
        // 
        this._colLastEntry.HeaderText = "آخر دخول";
        this._colLastEntry.Name = "_colLastEntry";
        this._colLastEntry.ReadOnly = true;
        this._colLastEntry.Width = 130;
        // 
        // _colExpiryDate
        // 
        this._colExpiryDate.HeaderText = "انتهاء البطاقة";
        this._colExpiryDate.Name = "_colExpiryDate";
        this._colExpiryDate.ReadOnly = true;
        this._colExpiryDate.Width = 110;
        // 
        // _splitBottom
        // 
        this._splitBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        this._splitBottom.Location = new System.Drawing.Point(8, 4);
        this._splitBottom.Name = "_splitBottom";
        // 
        // _splitBottom.Panel1
        // 
        this._splitBottom.Panel1.Controls.Add(this._grpHolderDetails);
        this._splitBottom.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        // 
        // _splitBottom.Panel2
        // 
        this._splitBottom.Panel2.Controls.Add(this._grpHistory);
        this._splitBottom.Panel2.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
        this._splitBottom.Size = new System.Drawing.Size(984, 332);
        this._splitBottom.SplitterDistance = 380;
        this._splitBottom.TabIndex = 0;
        // 
        // _grpHolderDetails
        // 
        this._grpHolderDetails.Controls.Add(this._tblDetails);
        this._grpHolderDetails.Controls.Add(this._pnlDetailsActions);
        this._grpHolderDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpHolderDetails.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpHolderDetails.Location = new System.Drawing.Point(0, 0);
        this._grpHolderDetails.Name = "_grpHolderDetails";
        this._grpHolderDetails.Padding = new System.Windows.Forms.Padding(6);
        this._grpHolderDetails.Size = new System.Drawing.Size(376, 332);
        this._grpHolderDetails.TabIndex = 0;
        this._grpHolderDetails.TabStop = false;
        this._grpHolderDetails.Text = "تفاصيل الزبون";
        // 
        // _tblDetails
        // 
        this._tblDetails.ColumnCount = 2;
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblDetails.Controls.Add(this._lblDetailNameTitle, 0, 0);
        this._tblDetails.Controls.Add(this._lblDetailName, 1, 0);
        this._tblDetails.Controls.Add(this._lblDetailCardTitle, 0, 1);
        this._tblDetails.Controls.Add(this._lblDetailCard, 1, 1);
        this._tblDetails.Controls.Add(this._lblDetailOrgTitle, 0, 2);
        this._tblDetails.Controls.Add(this._lblDetailOrg, 1, 2);
        this._tblDetails.Controls.Add(this._lblDetailPhoneTitle, 0, 3);
        this._tblDetails.Controls.Add(this._lblDetailPhone, 1, 3);
        this._tblDetails.Controls.Add(this._lblDetailNidTitle, 0, 4);
        this._tblDetails.Controls.Add(this._lblDetailNid, 1, 4);
        this._tblDetails.Controls.Add(this._lblDetailExpiryTitle, 0, 5);
        this._tblDetails.Controls.Add(this._lblDetailExpiry, 1, 5);
        this._tblDetails.Controls.Add(this._lblDetailStatusTitle, 0, 6);
        this._tblDetails.Controls.Add(this._lblDetailStatus, 1, 6);
        this._tblDetails.Controls.Add(this._lblDetailReasonTitle, 0, 7);
        this._tblDetails.Controls.Add(this._lblDetailReason, 1, 7);
        this._tblDetails.Controls.Add(this._lblDetailNotesTitle, 0, 8);
        this._tblDetails.Controls.Add(this._lblDetailNotes, 1, 8);
        this._tblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblDetails.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._tblDetails.Location = new System.Drawing.Point(6, 23);
        this._tblDetails.Name = "_tblDetails";
        this._tblDetails.RowCount = 9;
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblDetails.Size = new System.Drawing.Size(364, 258);
        this._tblDetails.TabIndex = 0;
        // 
        // _lblDetailNameTitle
        // 
        this._lblDetailNameTitle.AutoSize = true;
        this._lblDetailNameTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailNameTitle.Location = new System.Drawing.Point(280, 0);
        this._lblDetailNameTitle.Name = "_lblDetailNameTitle";
        this._lblDetailNameTitle.Size = new System.Drawing.Size(81, 15);
        this._lblDetailNameTitle.TabIndex = 0;
        this._lblDetailNameTitle.Text = "الاسم الرباعي:";
        // 
        // _lblDetailName
        // 
        this._lblDetailName.AutoSize = true;
        this._lblDetailName.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailName.Location = new System.Drawing.Point(256, 0);
        this._lblDetailName.Name = "_lblDetailName";
        this._lblDetailName.Size = new System.Drawing.Size(10, 15);
        this._lblDetailName.TabIndex = 1;
        this._lblDetailName.Text = "-";
        // 
        // _lblDetailCardTitle
        // 
        this._lblDetailCardTitle.AutoSize = true;
        this._lblDetailCardTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailCardTitle.Location = new System.Drawing.Point(293, 26);
        this._lblDetailCardTitle.Name = "_lblDetailCardTitle";
        this._lblDetailCardTitle.Size = new System.Drawing.Size(68, 15);
        this._lblDetailCardTitle.TabIndex = 2;
        this._lblDetailCardTitle.Text = "رقم البطاقة:";
        // 
        // _lblDetailCard
        // 
        this._lblDetailCard.AutoSize = true;
        this._lblDetailCard.Location = new System.Drawing.Point(256, 26);
        this._lblDetailCard.Name = "_lblDetailCard";
        this._lblDetailCard.Size = new System.Drawing.Size(10, 15);
        this._lblDetailCard.TabIndex = 3;
        this._lblDetailCard.Text = "-";
        // 
        // _lblDetailOrgTitle
        // 
        this._lblDetailOrgTitle.AutoSize = true;
        this._lblDetailOrgTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailOrgTitle.Location = new System.Drawing.Point(323, 52);
        this._lblDetailOrgTitle.Name = "_lblDetailOrgTitle";
        this._lblDetailOrgTitle.Size = new System.Drawing.Size(38, 15);
        this._lblDetailOrgTitle.TabIndex = 4;
        this._lblDetailOrgTitle.Text = "الجهة:";
        // 
        // _lblDetailOrg
        // 
        this._lblDetailOrg.AutoSize = true;
        this._lblDetailOrg.Location = new System.Drawing.Point(256, 52);
        this._lblDetailOrg.Name = "_lblDetailOrg";
        this._lblDetailOrg.Size = new System.Drawing.Size(10, 15);
        this._lblDetailOrg.TabIndex = 5;
        this._lblDetailOrg.Text = "-";
        // 
        // _lblDetailPhoneTitle
        // 
        this._lblDetailPhoneTitle.AutoSize = true;
        this._lblDetailPhoneTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailPhoneTitle.Location = new System.Drawing.Point(295, 78);
        this._lblDetailPhoneTitle.Name = "_lblDetailPhoneTitle";
        this._lblDetailPhoneTitle.Size = new System.Drawing.Size(66, 15);
        this._lblDetailPhoneTitle.TabIndex = 6;
        this._lblDetailPhoneTitle.Text = "رقم الهاتف:";
        // 
        // _lblDetailPhone
        // 
        this._lblDetailPhone.AutoSize = true;
        this._lblDetailPhone.Location = new System.Drawing.Point(256, 78);
        this._lblDetailPhone.Name = "_lblDetailPhone";
        this._lblDetailPhone.Size = new System.Drawing.Size(10, 15);
        this._lblDetailPhone.TabIndex = 7;
        this._lblDetailPhone.Text = "-";
        // 
        // _lblDetailNidTitle
        // 
        this._lblDetailNidTitle.AutoSize = true;
        this._lblDetailNidTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailNidTitle.Location = new System.Drawing.Point(286, 104);
        this._lblDetailNidTitle.Name = "_lblDetailNidTitle";
        this._lblDetailNidTitle.Size = new System.Drawing.Size(75, 15);
        this._lblDetailNidTitle.TabIndex = 8;
        this._lblDetailNidTitle.Text = "الرقم الوطني:";
        // 
        // _lblDetailNid
        // 
        this._lblDetailNid.AutoSize = true;
        this._lblDetailNid.Location = new System.Drawing.Point(256, 104);
        this._lblDetailNid.Name = "_lblDetailNid";
        this._lblDetailNid.Size = new System.Drawing.Size(10, 15);
        this._lblDetailNid.TabIndex = 9;
        this._lblDetailNid.Text = "-";
        // 
        // _lblDetailExpiryTitle
        // 
        this._lblDetailExpiryTitle.AutoSize = true;
        this._lblDetailExpiryTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailExpiryTitle.Location = new System.Drawing.Point(285, 130);
        this._lblDetailExpiryTitle.Name = "_lblDetailExpiryTitle";
        this._lblDetailExpiryTitle.Size = new System.Drawing.Size(76, 15);
        this._lblDetailExpiryTitle.TabIndex = 10;
        this._lblDetailExpiryTitle.Text = "تاريخ الانتهاء:";
        // 
        // _lblDetailExpiry
        // 
        this._lblDetailExpiry.AutoSize = true;
        this._lblDetailExpiry.Location = new System.Drawing.Point(256, 130);
        this._lblDetailExpiry.Name = "_lblDetailExpiry";
        this._lblDetailExpiry.Size = new System.Drawing.Size(10, 15);
        this._lblDetailExpiry.TabIndex = 11;
        this._lblDetailExpiry.Text = "-";
        // 
        // _lblDetailStatusTitle
        // 
        this._lblDetailStatusTitle.AutoSize = true;
        this._lblDetailStatusTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailStatusTitle.Location = new System.Drawing.Point(322, 156);
        this._lblDetailStatusTitle.Name = "_lblDetailStatusTitle";
        this._lblDetailStatusTitle.Size = new System.Drawing.Size(39, 15);
        this._lblDetailStatusTitle.TabIndex = 12;
        this._lblDetailStatusTitle.Text = "الحالة:";
        // 
        // _lblDetailStatus
        // 
        this._lblDetailStatus.AutoSize = true;
        this._lblDetailStatus.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailStatus.Location = new System.Drawing.Point(256, 156);
        this._lblDetailStatus.Name = "_lblDetailStatus";
        this._lblDetailStatus.Size = new System.Drawing.Size(10, 15);
        this._lblDetailStatus.TabIndex = 13;
        this._lblDetailStatus.Text = "-";
        // 
        // _lblDetailReasonTitle
        // 
        this._lblDetailReasonTitle.AutoSize = true;
        this._lblDetailReasonTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailReasonTitle.Location = new System.Drawing.Point(295, 182);
        this._lblDetailReasonTitle.Name = "_lblDetailReasonTitle";
        this._lblDetailReasonTitle.Size = new System.Drawing.Size(66, 15);
        this._lblDetailReasonTitle.TabIndex = 14;
        this._lblDetailReasonTitle.Text = "سبب الحالة:";
        // 
        // _lblDetailReason
        // 
        this._lblDetailReason.AutoSize = true;
        this._lblDetailReason.Location = new System.Drawing.Point(256, 182);
        this._lblDetailReason.Name = "_lblDetailReason";
        this._lblDetailReason.Size = new System.Drawing.Size(10, 15);
        this._lblDetailReason.TabIndex = 15;
        this._lblDetailReason.Text = "-";
        // 
        // _lblDetailNotesTitle
        // 
        this._lblDetailNotesTitle.AutoSize = true;
        this._lblDetailNotesTitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblDetailNotesTitle.Location = new System.Drawing.Point(308, 208);
        this._lblDetailNotesTitle.Name = "_lblDetailNotesTitle";
        this._lblDetailNotesTitle.Size = new System.Drawing.Size(53, 15);
        this._lblDetailNotesTitle.TabIndex = 16;
        this._lblDetailNotesTitle.Text = "ملاحظات:";
        // 
        // _lblDetailNotes
        // 
        this._lblDetailNotes.AutoSize = true;
        this._lblDetailNotes.Location = new System.Drawing.Point(256, 208);
        this._lblDetailNotes.Name = "_lblDetailNotes";
        this._lblDetailNotes.Size = new System.Drawing.Size(10, 15);
        this._lblDetailNotes.TabIndex = 17;
        this._lblDetailNotes.Text = "-";
        // 
        // _pnlDetailsActions
        // 
        this._pnlDetailsActions.Controls.Add(this._btnEdit);
        this._pnlDetailsActions.Controls.Add(this._btnChangeStatus);
        this._pnlDetailsActions.Controls.Add(this._btnCheckInDirect);
        this._pnlDetailsActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlDetailsActions.Location = new System.Drawing.Point(6, 281);
        this._pnlDetailsActions.Name = "_pnlDetailsActions";
        this._pnlDetailsActions.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
        this._pnlDetailsActions.Size = new System.Drawing.Size(364, 45);
        this._pnlDetailsActions.TabIndex = 1;
        // 
        // _btnEdit
        // 
        this._btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnEdit.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnEdit.ForeColor = System.Drawing.Color.White;
        this._btnEdit.Location = new System.Drawing.Point(260, 7);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new System.Drawing.Size(101, 30);
        this._btnEdit.TabIndex = 0;
        this._btnEdit.Text = "تعديل البيانات...";
        this._btnEdit.UseVisualStyleBackColor = false;
        // 
        // _btnChangeStatus
        // 
        this._btnChangeStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._btnChangeStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnChangeStatus.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnChangeStatus.ForeColor = System.Drawing.Color.White;
        this._btnChangeStatus.Location = new System.Drawing.Point(155, 7);
        this._btnChangeStatus.Name = "_btnChangeStatus";
        this._btnChangeStatus.Size = new System.Drawing.Size(99, 30);
        this._btnChangeStatus.TabIndex = 1;
        this._btnChangeStatus.Text = "تغيير الحالة...";
        this._btnChangeStatus.UseVisualStyleBackColor = false;
        // 
        // _btnCheckInDirect
        // 
        this._btnCheckInDirect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnCheckInDirect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCheckInDirect.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnCheckInDirect.ForeColor = System.Drawing.Color.White;
        this._btnCheckInDirect.Location = new System.Drawing.Point(50, 7);
        this._btnCheckInDirect.Name = "_btnCheckInDirect";
        this._btnCheckInDirect.Size = new System.Drawing.Size(99, 30);
        this._btnCheckInDirect.TabIndex = 2;
        this._btnCheckInDirect.Text = "فتح بالشباك";
        this._btnCheckInDirect.UseVisualStyleBackColor = false;
        // 
        // _grpHistory
        // 
        this._grpHistory.Controls.Add(this._gridHistory);
        this._grpHistory.Controls.Add(this._pnlHistoryActions);
        this._grpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpHistory.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpHistory.Location = new System.Drawing.Point(4, 0);
        this._grpHistory.Name = "_grpHistory";
        this._grpHistory.Padding = new System.Windows.Forms.Padding(6);
        this._grpHistory.Size = new System.Drawing.Size(596, 332);
        this._grpHistory.TabIndex = 0;
        this._grpHistory.TabStop = false;
        this._grpHistory.Text = "سجل محاولات وحركات الدخول";
        // 
        // _gridHistory
        // 
        this._gridHistory.AllowUserToAddRows = false;
        this._gridHistory.AllowUserToDeleteRows = false;
        this._gridHistory.BackgroundColor = System.Drawing.Color.White;
        this._gridHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colHistTime,
            this._colHistBusinessDate,
            this._colHistWorkstation,
            this._colHistResult,
            this._colHistPersons,
            this._colHistDetails,
            this._colHistStatus,
            this._colHistVoidReason});
        this._gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridHistory.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._gridHistory.Location = new System.Drawing.Point(6, 23);
        this._gridHistory.MultiSelect = false;
        this._gridHistory.Name = "_gridHistory";
        this._gridHistory.ReadOnly = true;
        this._gridHistory.RowHeadersVisible = false;
        this._gridHistory.RowTemplate.Height = 26;
        this._gridHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridHistory.Size = new System.Drawing.Size(584, 263);
        this._gridHistory.TabIndex = 0;
        // 
        // _colHistTime
        // 
        this._colHistTime.HeaderText = "الوقت والتاريخ";
        this._colHistTime.Name = "_colHistTime";
        this._colHistTime.ReadOnly = true;
        this._colHistTime.Width = 120;
        // 
        // _colHistBusinessDate
        // 
        this._colHistBusinessDate.HeaderText = "تاريخ العمل";
        this._colHistBusinessDate.Name = "_colHistBusinessDate";
        this._colHistBusinessDate.ReadOnly = true;
        this._colHistBusinessDate.Width = 95;
        // 
        // _colHistWorkstation
        // 
        this._colHistWorkstation.HeaderText = "الشباك";
        this._colHistWorkstation.Name = "_colHistWorkstation";
        this._colHistWorkstation.ReadOnly = true;
        this._colHistWorkstation.Width = 70;
        // 
        // _colHistResult
        // 
        this._colHistResult.HeaderText = "النتيجة";
        this._colHistResult.Name = "_colHistResult";
        this._colHistResult.ReadOnly = true;
        this._colHistResult.Width = 85;
        // 
        // _colHistPersons
        // 
        this._colHistPersons.HeaderText = "الأفراد";
        this._colHistPersons.Name = "_colHistPersons";
        this._colHistPersons.ReadOnly = true;
        this._colHistPersons.Width = 65;
        // 
        // _colHistDetails
        // 
        this._colHistDetails.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this._colHistDetails.HeaderText = "التفاصيل / المرافقون";
        this._colHistDetails.Name = "_colHistDetails";
        this._colHistDetails.ReadOnly = true;
        // 
        // _colHistStatus
        // 
        this._colHistStatus.HeaderText = "الحالة";
        this._colHistStatus.Name = "_colHistStatus";
        this._colHistStatus.ReadOnly = true;
        this._colHistStatus.Width = 75;
        // 
        // _colHistVoidReason
        // 
        this._colHistVoidReason.HeaderText = "سبب الإلغاء";
        this._colHistVoidReason.Name = "_colHistVoidReason";
        this._colHistVoidReason.ReadOnly = true;
        this._colHistVoidReason.Width = 110;
        // 
        // _pnlHistoryActions
        // 
        this._pnlHistoryActions.Controls.Add(this._btnVoidHistoryEntry);
        this._pnlHistoryActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlHistoryActions.Location = new System.Drawing.Point(6, 286);
        this._pnlHistoryActions.Name = "_pnlHistoryActions";
        this._pnlHistoryActions.Size = new System.Drawing.Size(584, 40);
        this._pnlHistoryActions.TabIndex = 1;
        // 
        // _btnVoidHistoryEntry
        // 
        this._btnVoidHistoryEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this._btnVoidHistoryEntry.Enabled = false;
        this._btnVoidHistoryEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnVoidHistoryEntry.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnVoidHistoryEntry.ForeColor = System.Drawing.Color.White;
        this._btnVoidHistoryEntry.Location = new System.Drawing.Point(444, 5);
        this._btnVoidHistoryEntry.Name = "_btnVoidHistoryEntry";
        this._btnVoidHistoryEntry.Size = new System.Drawing.Size(130, 30);
        this._btnVoidHistoryEntry.TabIndex = 0;
        this._btnVoidHistoryEntry.Text = "إلغاء العملية (Void)...";
        this._btnVoidHistoryEntry.UseVisualStyleBackColor = false;
        // 
        // BeneficiariesControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this._splitMain);
        this.Controls.Add(this._pnlSearch);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Name = "BeneficiariesControl";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(1000, 640);
        this._pnlSearch.ResumeLayout(false);
        this._pnlSearch.PerformLayout();
        this._splitMain.Panel1.ResumeLayout(false);
        this._splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._splitMain)).EndInit();
        this._splitMain.ResumeLayout(false);
        this._grpResults.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridBeneficiaries)).EndInit();
        this._splitBottom.Panel1.ResumeLayout(false);
        this._splitBottom.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._splitBottom)).EndInit();
        this._splitBottom.ResumeLayout(false);
        this._grpHolderDetails.ResumeLayout(false);
        this._tblDetails.ResumeLayout(false);
        this._tblDetails.PerformLayout();
        this._pnlDetailsActions.ResumeLayout(false);
        this._grpHistory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridHistory)).EndInit();
        this._pnlHistoryActions.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlSearch;
    private System.Windows.Forms.Label _lblSearch;
    private System.Windows.Forms.TextBox _txtSearch;
    private System.Windows.Forms.Button _btnSearch;
    private System.Windows.Forms.Button _btnClearSearch;
    private System.Windows.Forms.Label _lblSearchResultCount;
    private System.Windows.Forms.SplitContainer _splitMain;
    private System.Windows.Forms.GroupBox _grpResults;
    private System.Windows.Forms.DataGridView _gridBeneficiaries;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCardNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colOrganization;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colFullName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPhone;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colNationalId;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colLastEntry;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colExpiryDate;
    private System.Windows.Forms.SplitContainer _splitBottom;
    private System.Windows.Forms.GroupBox _grpHolderDetails;
    private System.Windows.Forms.TableLayoutPanel _tblDetails;
    private System.Windows.Forms.Label _lblDetailNameTitle;
    private System.Windows.Forms.Label _lblDetailName;
    private System.Windows.Forms.Label _lblDetailCardTitle;
    private System.Windows.Forms.Label _lblDetailCard;
    private System.Windows.Forms.Label _lblDetailOrgTitle;
    private System.Windows.Forms.Label _lblDetailOrg;
    private System.Windows.Forms.Label _lblDetailPhoneTitle;
    private System.Windows.Forms.Label _lblDetailPhone;
    private System.Windows.Forms.Label _lblDetailNidTitle;
    private System.Windows.Forms.Label _lblDetailNid;
    private System.Windows.Forms.Label _lblDetailExpiryTitle;
    private System.Windows.Forms.Label _lblDetailExpiry;
    private System.Windows.Forms.Label _lblDetailStatusTitle;
    private System.Windows.Forms.Label _lblDetailStatus;
    private System.Windows.Forms.Label _lblDetailReasonTitle;
    private System.Windows.Forms.Label _lblDetailReason;
    private System.Windows.Forms.Label _lblDetailNotesTitle;
    private System.Windows.Forms.Label _lblDetailNotes;
    private System.Windows.Forms.FlowLayoutPanel _pnlDetailsActions;
    private System.Windows.Forms.Button _btnEdit;
    private System.Windows.Forms.Button _btnChangeStatus;
    private System.Windows.Forms.Button _btnCheckInDirect;
    private System.Windows.Forms.GroupBox _grpHistory;
    private System.Windows.Forms.DataGridView _gridHistory;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistBusinessDate;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistWorkstation;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistResult;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistPersons;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistDetails;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHistVoidReason;
    private System.Windows.Forms.Panel _pnlHistoryActions;
    private System.Windows.Forms.Button _btnVoidHistoryEntry;
}
