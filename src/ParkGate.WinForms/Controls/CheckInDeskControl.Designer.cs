#nullable disable

namespace ParkGate.WinForms.Controls;

partial class CheckInDeskControl
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

    #region Component Designer generated code

    private void InitializeComponent()
    {
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
        this._pnlSearch = new System.Windows.Forms.Panel();
        this._btnSearch = new System.Windows.Forms.Button();
        this._cmbOrganization = new System.Windows.Forms.ComboBox();
        this._lblOrganization = new System.Windows.Forms.Label();
        this._txtCardNumber = new System.Windows.Forms.TextBox();
        this._lblCardNumber = new System.Windows.Forms.Label();
        this._statusBanner = new ParkGate.WinForms.Controls.StatusBanner();
        this._splitMiddle = new System.Windows.Forms.SplitContainer();
        this._grpRegistration = new System.Windows.Forms.GroupBox();
        this._tblRegistration = new System.Windows.Forms.TableLayoutPanel();
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
        this._grpHolder = new System.Windows.Forms.GroupBox();
        this._tblHolder = new System.Windows.Forms.TableLayoutPanel();
        this._lblHolderNameTitle = new System.Windows.Forms.Label();
        this._lblHolderName = new System.Windows.Forms.Label();
        this._lblHolderPhoneTitle = new System.Windows.Forms.Label();
        this._lblHolderPhone = new System.Windows.Forms.Label();
        this._lblHolderOrgTitle = new System.Windows.Forms.Label();
        this._lblHolderOrg = new System.Windows.Forms.Label();
        this._lblHolderNatIdTitle = new System.Windows.Forms.Label();
        this._lblHolderNatId = new System.Windows.Forms.Label();
        this._lblHolderExpiryTitle = new System.Windows.Forms.Label();
        this._lblHolderExpiry = new System.Windows.Forms.Label();
        this._lblHolderLastEntryTitle = new System.Windows.Forms.Label();
        this._lblHolderLastEntry = new System.Windows.Forms.Label();
        this._lblHolderStatusTitle = new System.Windows.Forms.Label();
        this._lblHolderStatus = new System.Windows.Forms.Label();
        this._grpCompanions = new System.Windows.Forms.GroupBox();
        this._pnlCompanionsTotal = new System.Windows.Forms.Panel();
        this._lblCompanionsNotice = new System.Windows.Forms.Label();
        this._lblTotalPersons = new System.Windows.Forms.Label();
        this._flowCounters = new System.Windows.Forms.FlowLayoutPanel();
        this._pnlActions = new System.Windows.Forms.Panel();
        this._flowActions = new System.Windows.Forms.FlowLayoutPanel();
        this._btnRecordEntry = new System.Windows.Forms.Button();
        this._btnOverride = new System.Windows.Forms.Button();
        this._btnNewHolder = new System.Windows.Forms.Button();
        this._btnClear = new System.Windows.Forms.Button();
        this._grpToday = new System.Windows.Forms.GroupBox();
        this._gridToday = new System.Windows.Forms.DataGridView();
        this._colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPersons = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._lblTodaySummary = new System.Windows.Forms.Label();
        this._mainLayout.SuspendLayout();
        this._pnlSearch.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._splitMiddle)).BeginInit();
        this._splitMiddle.Panel1.SuspendLayout();
        this._splitMiddle.Panel2.SuspendLayout();
        this._splitMiddle.SuspendLayout();
        this._grpRegistration.SuspendLayout();
        this._tblRegistration.SuspendLayout();
        this._pnlRegActions.SuspendLayout();
        this._grpHolder.SuspendLayout();
        this._tblHolder.SuspendLayout();
        this._grpCompanions.SuspendLayout();
        this._pnlCompanionsTotal.SuspendLayout();
        this._pnlActions.SuspendLayout();
        this._flowActions.SuspendLayout();
        this._grpToday.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).BeginInit();
        this.SuspendLayout();
        // 
        // _mainLayout
        // 
        this._mainLayout.ColumnCount = 1;
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._pnlSearch, 0, 0);
        this._mainLayout.Controls.Add(this._statusBanner, 0, 1);
        this._mainLayout.Controls.Add(this._splitMiddle, 0, 2);
        this._mainLayout.Controls.Add(this._pnlActions, 0, 3);
        this._mainLayout.Controls.Add(this._grpToday, 0, 4);
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.Location = new System.Drawing.Point(10, 10);
        this._mainLayout.Margin = new System.Windows.Forms.Padding(0);
        this._mainLayout.Name = "_mainLayout";
        this._mainLayout.RowCount = 5;
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
        this._mainLayout.Size = new System.Drawing.Size(988, 641);
        this._mainLayout.TabIndex = 0;
        // 
        // _pnlSearch
        // 
        this._pnlSearch.BackColor = System.Drawing.Color.White;
        this._pnlSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._pnlSearch.Controls.Add(this._btnSearch);
        this._pnlSearch.Controls.Add(this._cmbOrganization);
        this._pnlSearch.Controls.Add(this._lblOrganization);
        this._pnlSearch.Controls.Add(this._txtCardNumber);
        this._pnlSearch.Controls.Add(this._lblCardNumber);
        this._pnlSearch.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlSearch.Location = new System.Drawing.Point(0, 0);
        this._pnlSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._pnlSearch.Name = "_pnlSearch";
        this._pnlSearch.Padding = new System.Windows.Forms.Padding(6);
        this._pnlSearch.Size = new System.Drawing.Size(988, 44);
        this._pnlSearch.TabIndex = 0;
        // 
        // _btnSearch
        // 
        this._btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnSearch.FlatAppearance.BorderSize = 0;
        this._btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSearch.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSearch.ForeColor = System.Drawing.Color.White;
        this._btnSearch.Location = new System.Drawing.Point(8, 6);
        this._btnSearch.Name = "_btnSearch";
        this._btnSearch.Size = new System.Drawing.Size(120, 30);
        this._btnSearch.TabIndex = 4;
        this._btnSearch.Text = "بحث (Enter)";
        this._btnSearch.UseVisualStyleBackColor = false;
        // 
        // _cmbOrganization
        // 
        this._cmbOrganization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this._cmbOrganization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbOrganization.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._cmbOrganization.FormattingEnabled = true;
        this._cmbOrganization.Location = new System.Drawing.Point(140, 7);
        this._cmbOrganization.Name = "_cmbOrganization";
        this._cmbOrganization.Size = new System.Drawing.Size(260, 27);
        this._cmbOrganization.TabIndex = 3;
        // 
        // _lblOrganization
        // 
        this._lblOrganization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this._lblOrganization.AutoSize = true;
        this._lblOrganization.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblOrganization.Location = new System.Drawing.Point(406, 11);
        this._lblOrganization.Name = "_lblOrganization";
        this._lblOrganization.Size = new System.Drawing.Size(46, 19);
        this._lblOrganization.TabIndex = 2;
        this._lblOrganization.Text = "الجهة:";
        // 
        // _txtCardNumber
        // 
        this._txtCardNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this._txtCardNumber.Font = new System.Drawing.Font("Cairo", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._txtCardNumber.Location = new System.Drawing.Point(470, 5);
        this._txtCardNumber.MaxLength = 50;
        this._txtCardNumber.Name = "_txtCardNumber";
        this._txtCardNumber.Size = new System.Drawing.Size(390, 31);
        this._txtCardNumber.TabIndex = 1;
        // 
        // _lblCardNumber
        // 
        this._lblCardNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this._lblCardNumber.AutoSize = true;
        this._lblCardNumber.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblCardNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._lblCardNumber.Location = new System.Drawing.Point(866, 11);
        this._lblCardNumber.Name = "_lblCardNumber";
        this._lblCardNumber.Size = new System.Drawing.Size(89, 20);
        this._lblCardNumber.TabIndex = 0;
        this._lblCardNumber.Text = "رقم البطاقة:";
        // 
        // _statusBanner
        // 
        this._statusBanner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._statusBanner.Dock = System.Windows.Forms.DockStyle.Fill;
        this._statusBanner.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._statusBanner.Location = new System.Drawing.Point(0, 50);
        this._statusBanner.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._statusBanner.MinimumSize = new System.Drawing.Size(300, 55);
        this._statusBanner.Name = "_statusBanner";
        this._statusBanner.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._statusBanner.Size = new System.Drawing.Size(988, 62);
        this._statusBanner.TabIndex = 1;
        // 
        // _splitMiddle
        // 
        this._splitMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
        this._splitMiddle.Location = new System.Drawing.Point(0, 118);
        this._splitMiddle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._splitMiddle.Name = "_splitMiddle";
        // 
        // _splitMiddle.Panel1
        // 
        this._splitMiddle.Panel1.Controls.Add(this._grpRegistration);
        this._splitMiddle.Panel1.Controls.Add(this._grpHolder);
        this._splitMiddle.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        // 
        // _splitMiddle.Panel2
        // 
        this._splitMiddle.Panel2.Controls.Add(this._grpCompanions);
        this._splitMiddle.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._splitMiddle.Size = new System.Drawing.Size(988, 254);
        this._splitMiddle.SplitterDistance = 500;
        this._splitMiddle.TabIndex = 2;
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
        this._grpRegistration.Size = new System.Drawing.Size(500, 254);
        this._grpRegistration.TabIndex = 1;
        this._grpRegistration.TabStop = false;
        this._grpRegistration.Text = "تسجيل حامل بطاقة جديد (فوري)";
        this._grpRegistration.Visible = false;
        // 
        // _tblRegistration
        // 
        this._tblRegistration.ColumnCount = 2;
        this._tblRegistration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblRegistration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblRegistration.Controls.Add(this._lblRegFullName, 0, 0);
        this._tblRegistration.Controls.Add(this._txtRegFullName, 1, 0);
        this._tblRegistration.Controls.Add(this._lblRegPhone, 0, 1);
        this._tblRegistration.Controls.Add(this._txtRegPhone, 1, 1);
        this._tblRegistration.Controls.Add(this._lblRegNationalId, 0, 2);
        this._tblRegistration.Controls.Add(this._txtRegNationalId, 1, 2);
        this._tblRegistration.Controls.Add(this._lblRegExpiry, 0, 3);
        this._tblRegistration.Controls.Add(this._dtpRegExpiry, 1, 3);
        this._tblRegistration.Controls.Add(this._lblRegNotes, 0, 4);
        this._tblRegistration.Controls.Add(this._txtRegNotes, 1, 4);
        this._tblRegistration.Controls.Add(this._pnlRegActions, 1, 5);
        this._tblRegistration.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblRegistration.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._tblRegistration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._tblRegistration.Location = new System.Drawing.Point(8, 26);
        this._tblRegistration.Name = "_tblRegistration";
        this._tblRegistration.RowCount = 6;
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblRegistration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblRegistration.Size = new System.Drawing.Size(484, 220);
        this._tblRegistration.TabIndex = 0;
        // 
        // _lblRegFullName
        // 
        this._lblRegFullName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegFullName.AutoSize = true;
        this._lblRegFullName.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRegFullName.Location = new System.Drawing.Point(395, 7);
        this._lblRegFullName.Name = "_lblRegFullName";
        this._lblRegFullName.Size = new System.Drawing.Size(86, 17);
        this._lblRegFullName.TabIndex = 0;
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
        this._txtRegFullName.TabIndex = 1;
        // 
        // _lblRegPhone
        // 
        this._lblRegPhone.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegPhone.AutoSize = true;
        this._lblRegPhone.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRegPhone.Location = new System.Drawing.Point(407, 39);
        this._lblRegPhone.Name = "_lblRegPhone";
        this._lblRegPhone.Size = new System.Drawing.Size(74, 17);
        this._lblRegPhone.TabIndex = 2;
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
        this._txtRegPhone.TabIndex = 3;
        // 
        // _lblRegNationalId
        // 
        this._lblRegNationalId.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegNationalId.AutoSize = true;
        this._lblRegNationalId.Location = new System.Drawing.Point(365, 71);
        this._lblRegNationalId.Name = "_lblRegNationalId";
        this._lblRegNationalId.Size = new System.Drawing.Size(116, 17);
        this._lblRegNationalId.TabIndex = 4;
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
        this._txtRegNationalId.TabIndex = 5;
        // 
        // _lblRegExpiry
        // 
        this._lblRegExpiry.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegExpiry.AutoSize = true;
        this._lblRegExpiry.Location = new System.Drawing.Point(367, 103);
        this._lblRegExpiry.Name = "_lblRegExpiry";
        this._lblRegExpiry.Size = new System.Drawing.Size(114, 17);
        this._lblRegExpiry.TabIndex = 6;
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
        this._dtpRegExpiry.TabIndex = 7;
        // 
        // _lblRegNotes
        // 
        this._lblRegNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRegNotes.AutoSize = true;
        this._lblRegNotes.Location = new System.Drawing.Point(427, 137);
        this._lblRegNotes.Name = "_lblRegNotes";
        this._lblRegNotes.Size = new System.Drawing.Size(54, 17);
        this._lblRegNotes.TabIndex = 8;
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
        this._txtRegNotes.TabIndex = 9;
        // 
        // _pnlRegActions
        // 
        this._pnlRegActions.Controls.Add(this._btnSaveReg);
        this._pnlRegActions.Controls.Add(this._btnCancelReg);
        this._pnlRegActions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlRegActions.Location = new System.Drawing.Point(3, 163);
        this._pnlRegActions.Name = "_pnlRegActions";
        this._pnlRegActions.Size = new System.Drawing.Size(348, 54);
        this._pnlRegActions.TabIndex = 10;
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
        // _grpHolder
        // 
        this._grpHolder.BackColor = System.Drawing.Color.White;
        this._grpHolder.Controls.Add(this._tblHolder);
        this._grpHolder.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpHolder.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpHolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._grpHolder.Location = new System.Drawing.Point(0, 0);
        this._grpHolder.Name = "_grpHolder";
        this._grpHolder.Padding = new System.Windows.Forms.Padding(8);
        this._grpHolder.Size = new System.Drawing.Size(500, 254);
        this._grpHolder.TabIndex = 0;
        this._grpHolder.TabStop = false;
        this._grpHolder.Text = "بيانات حامل البطاقة";
        // 
        // _tblHolder
        // 
        this._tblHolder.ColumnCount = 2;
        this._tblHolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblHolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblHolder.Controls.Add(this._lblHolderNameTitle, 0, 0);
        this._tblHolder.Controls.Add(this._lblHolderName, 1, 0);
        this._tblHolder.Controls.Add(this._lblHolderPhoneTitle, 0, 1);
        this._tblHolder.Controls.Add(this._lblHolderPhone, 1, 1);
        this._tblHolder.Controls.Add(this._lblHolderOrgTitle, 0, 2);
        this._tblHolder.Controls.Add(this._lblHolderOrg, 1, 2);
        this._tblHolder.Controls.Add(this._lblHolderNatIdTitle, 0, 3);
        this._tblHolder.Controls.Add(this._lblHolderNatId, 1, 3);
        this._tblHolder.Controls.Add(this._lblHolderExpiryTitle, 0, 4);
        this._tblHolder.Controls.Add(this._lblHolderExpiry, 1, 4);
        this._tblHolder.Controls.Add(this._lblHolderLastEntryTitle, 0, 5);
        this._tblHolder.Controls.Add(this._lblHolderLastEntry, 1, 5);
        this._tblHolder.Controls.Add(this._lblHolderStatusTitle, 0, 6);
        this._tblHolder.Controls.Add(this._lblHolderStatus, 1, 6);
        this._tblHolder.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblHolder.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._tblHolder.Location = new System.Drawing.Point(8, 26);
        this._tblHolder.Name = "_tblHolder";
        this._tblHolder.RowCount = 7;
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
        this._tblHolder.Size = new System.Drawing.Size(484, 220);
        this._tblHolder.TabIndex = 0;
        // 
        // _lblHolderNameTitle
        // 
        this._lblHolderNameTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderNameTitle.AutoSize = true;
        this._lblHolderNameTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderNameTitle.Location = new System.Drawing.Point(404, 7);
        this._lblHolderNameTitle.Name = "_lblHolderNameTitle";
        this._lblHolderNameTitle.Size = new System.Drawing.Size(77, 17);
        this._lblHolderNameTitle.TabIndex = 0;
        this._lblHolderNameTitle.Text = "حامل البطاقة:";
        // 
        // _lblHolderName
        // 
        this._lblHolderName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderName.AutoSize = true;
        this._lblHolderName.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblHolderName.Location = new System.Drawing.Point(336, 5);
        this._lblHolderName.Name = "_lblHolderName";
        this._lblHolderName.Size = new System.Drawing.Size(15, 20);
        this._lblHolderName.TabIndex = 1;
        this._lblHolderName.Text = "-";
        // 
        // _lblHolderPhoneTitle
        // 
        this._lblHolderPhoneTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderPhoneTitle.AutoSize = true;
        this._lblHolderPhoneTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderPhoneTitle.Location = new System.Drawing.Point(407, 38);
        this._lblHolderPhoneTitle.Name = "_lblHolderPhoneTitle";
        this._lblHolderPhoneTitle.Size = new System.Drawing.Size(74, 17);
        this._lblHolderPhoneTitle.TabIndex = 2;
        this._lblHolderPhoneTitle.Text = "رقم الهاتف:";
        // 
        // _lblHolderPhone
        // 
        this._lblHolderPhone.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderPhone.AutoSize = true;
        this._lblHolderPhone.Location = new System.Drawing.Point(338, 38);
        this._lblHolderPhone.Name = "_lblHolderPhone";
        this._lblHolderPhone.Size = new System.Drawing.Size(13, 17);
        this._lblHolderPhone.TabIndex = 3;
        this._lblHolderPhone.Text = "-";
        // 
        // _lblHolderOrgTitle
        // 
        this._lblHolderOrgTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderOrgTitle.AutoSize = true;
        this._lblHolderOrgTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderOrgTitle.Location = new System.Drawing.Point(435, 69);
        this._lblHolderOrgTitle.Name = "_lblHolderOrgTitle";
        this._lblHolderOrgTitle.Size = new System.Drawing.Size(46, 17);
        this._lblHolderOrgTitle.TabIndex = 4;
        this._lblHolderOrgTitle.Text = "الجهة:";
        // 
        // _lblHolderOrg
        // 
        this._lblHolderOrg.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderOrg.AutoSize = true;
        this._lblHolderOrg.Location = new System.Drawing.Point(338, 69);
        this._lblHolderOrg.Name = "_lblHolderOrg";
        this._lblHolderOrg.Size = new System.Drawing.Size(13, 17);
        this._lblHolderOrg.TabIndex = 5;
        this._lblHolderOrg.Text = "-";
        // 
        // _lblHolderNatIdTitle
        // 
        this._lblHolderNatIdTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderNatIdTitle.AutoSize = true;
        this._lblHolderNatIdTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderNatIdTitle.Location = new System.Drawing.Point(398, 100);
        this._lblHolderNatIdTitle.Name = "_lblHolderNatIdTitle";
        this._lblHolderNatIdTitle.Size = new System.Drawing.Size(83, 17);
        this._lblHolderNatIdTitle.TabIndex = 6;
        this._lblHolderNatIdTitle.Text = "الرقم الوطني:";
        // 
        // _lblHolderNatId
        // 
        this._lblHolderNatId.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderNatId.AutoSize = true;
        this._lblHolderNatId.Location = new System.Drawing.Point(338, 100);
        this._lblHolderNatId.Name = "_lblHolderNatId";
        this._lblHolderNatId.Size = new System.Drawing.Size(13, 17);
        this._lblHolderNatId.TabIndex = 7;
        this._lblHolderNatId.Text = "-";
        // 
        // _lblHolderExpiryTitle
        // 
        this._lblHolderExpiryTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderExpiryTitle.AutoSize = true;
        this._lblHolderExpiryTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderExpiryTitle.Location = new System.Drawing.Point(404, 131);
        this._lblHolderExpiryTitle.Name = "_lblHolderExpiryTitle";
        this._lblHolderExpiryTitle.Size = new System.Drawing.Size(77, 17);
        this._lblHolderExpiryTitle.TabIndex = 8;
        this._lblHolderExpiryTitle.Text = "صلاحية البطاقة:";
        // 
        // _lblHolderExpiry
        // 
        this._lblHolderExpiry.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderExpiry.AutoSize = true;
        this._lblHolderExpiry.Location = new System.Drawing.Point(338, 131);
        this._lblHolderExpiry.Name = "_lblHolderExpiry";
        this._lblHolderExpiry.Size = new System.Drawing.Size(13, 17);
        this._lblHolderExpiry.TabIndex = 9;
        this._lblHolderExpiry.Text = "-";
        // 
        // _lblHolderLastEntryTitle
        // 
        this._lblHolderLastEntryTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderLastEntryTitle.AutoSize = true;
        this._lblHolderLastEntryTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderLastEntryTitle.Location = new System.Drawing.Point(397, 162);
        this._lblHolderLastEntryTitle.Name = "_lblHolderLastEntryTitle";
        this._lblHolderLastEntryTitle.Size = new System.Drawing.Size(84, 17);
        this._lblHolderLastEntryTitle.TabIndex = 10;
        this._lblHolderLastEntryTitle.Text = "آخر دخول مسجل:";
        // 
        // _lblHolderLastEntry
        // 
        this._lblHolderLastEntry.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderLastEntry.AutoSize = true;
        this._lblHolderLastEntry.Location = new System.Drawing.Point(338, 162);
        this._lblHolderLastEntry.Name = "_lblHolderLastEntry";
        this._lblHolderLastEntry.Size = new System.Drawing.Size(13, 17);
        this._lblHolderLastEntry.TabIndex = 11;
        this._lblHolderLastEntry.Text = "-";
        // 
        // _lblHolderStatusTitle
        // 
        this._lblHolderStatusTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderStatusTitle.AutoSize = true;
        this._lblHolderStatusTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderStatusTitle.Location = new System.Drawing.Point(403, 198);
        this._lblHolderStatusTitle.Name = "_lblHolderStatusTitle";
        this._lblHolderStatusTitle.Size = new System.Drawing.Size(78, 17);
        this._lblHolderStatusTitle.TabIndex = 12;
        this._lblHolderStatusTitle.Text = "حالة البطاقة:";
        // 
        // _lblHolderStatus
        // 
        this._lblHolderStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderStatus.AutoSize = true;
        this._lblHolderStatus.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderStatus.Location = new System.Drawing.Point(338, 198);
        this._lblHolderStatus.Name = "_lblHolderStatus";
        this._lblHolderStatus.Size = new System.Drawing.Size(13, 17);
        this._lblHolderStatus.TabIndex = 13;
        this._lblHolderStatus.Text = "-";
        // 
        // _grpCompanions
        // 
        this._grpCompanions.BackColor = System.Drawing.Color.White;
        this._grpCompanions.Controls.Add(this._pnlCompanionsTotal);
        this._grpCompanions.Controls.Add(this._flowCounters);
        this._grpCompanions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpCompanions.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpCompanions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._grpCompanions.Location = new System.Drawing.Point(0, 0);
        this._grpCompanions.Name = "_grpCompanions";
        this._grpCompanions.Padding = new System.Windows.Forms.Padding(8);
        this._grpCompanions.Size = new System.Drawing.Size(484, 254);
        this._grpCompanions.TabIndex = 0;
        this._grpCompanions.TabStop = false;
        this._grpCompanions.Text = "المرافقون مع حامل البطاقة";
        // 
        // _pnlCompanionsTotal
        // 
        this._pnlCompanionsTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
        this._pnlCompanionsTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._pnlCompanionsTotal.Controls.Add(this._lblCompanionsNotice);
        this._pnlCompanionsTotal.Controls.Add(this._lblTotalPersons);
        this._pnlCompanionsTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlCompanionsTotal.Location = new System.Drawing.Point(8, 196);
        this._pnlCompanionsTotal.Name = "_pnlCompanionsTotal";
        this._pnlCompanionsTotal.Padding = new System.Windows.Forms.Padding(6);
        this._pnlCompanionsTotal.Size = new System.Drawing.Size(468, 50);
        this._pnlCompanionsTotal.TabIndex = 1;
        // 
        // _lblCompanionsNotice
        // 
        this._lblCompanionsNotice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
        this._lblCompanionsNotice.AutoSize = true;
        this._lblCompanionsNotice.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblCompanionsNotice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._lblCompanionsNotice.Location = new System.Drawing.Point(6, 16);
        this._lblCompanionsNotice.Name = "_lblCompanionsNotice";
        this._lblCompanionsNotice.Size = new System.Drawing.Size(185, 15);
        this._lblCompanionsNotice.TabIndex = 1;
        this._lblCompanionsNotice.Text = "(حامل البطاقة محتسب تلقائياً +1 فرد)";
        // 
        // _lblTotalPersons
        // 
        this._lblTotalPersons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this._lblTotalPersons.AutoSize = true;
        this._lblTotalPersons.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTotalPersons.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblTotalPersons.Location = new System.Drawing.Point(210, 13);
        this._lblTotalPersons.Name = "_lblTotalPersons";
        this._lblTotalPersons.Size = new System.Drawing.Size(248, 21);
        this._lblTotalPersons.TabIndex = 0;
        this._lblTotalPersons.Text = "الإجمالي مع حامل البطاقة: 1 فرد";
        // 
        // _flowCounters
        // 
        this._flowCounters.AutoScroll = true;
        this._flowCounters.Dock = System.Windows.Forms.DockStyle.Fill;
        this._flowCounters.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this._flowCounters.Location = new System.Drawing.Point(8, 26);
        this._flowCounters.Name = "_flowCounters";
        this._flowCounters.Padding = new System.Windows.Forms.Padding(4);
        this._flowCounters.Size = new System.Drawing.Size(468, 220);
        this._flowCounters.TabIndex = 0;
        this._flowCounters.WrapContents = false;
        // 
        // _pnlActions
        // 
        this._pnlActions.Controls.Add(this._flowActions);
        this._pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlActions.Location = new System.Drawing.Point(0, 378);
        this._pnlActions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._pnlActions.Name = "_pnlActions";
        this._pnlActions.Size = new System.Drawing.Size(988, 44);
        this._pnlActions.TabIndex = 3;
        // 
        // _flowActions
        // 
        this._flowActions.Controls.Add(this._btnRecordEntry);
        this._flowActions.Controls.Add(this._btnOverride);
        this._flowActions.Controls.Add(this._btnNewHolder);
        this._flowActions.Controls.Add(this._btnClear);
        this._flowActions.Dock = System.Windows.Forms.DockStyle.Fill;
        this._flowActions.Location = new System.Drawing.Point(0, 0);
        this._flowActions.Name = "_flowActions";
        this._flowActions.Size = new System.Drawing.Size(988, 44);
        this._flowActions.TabIndex = 0;
        // 
        // _btnRecordEntry
        // 
        this._btnRecordEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnRecordEntry.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnRecordEntry.FlatAppearance.BorderSize = 0;
        this._btnRecordEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnRecordEntry.Font = new System.Drawing.Font("Cairo", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnRecordEntry.ForeColor = System.Drawing.Color.White;
        this._btnRecordEntry.Location = new System.Drawing.Point(780, 2);
        this._btnRecordEntry.Margin = new System.Windows.Forms.Padding(3, 2, 8, 2);
        this._btnRecordEntry.Name = "_btnRecordEntry";
        this._btnRecordEntry.Size = new System.Drawing.Size(200, 38);
        this._btnRecordEntry.TabIndex = 0;
        this._btnRecordEntry.Text = "تسجيل الدخول (F5)";
        this._btnRecordEntry.UseVisualStyleBackColor = false;
        // 
        // _btnOverride
        // 
        this._btnOverride.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(106)))), ((int)(((byte)(0)))));
        this._btnOverride.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnOverride.Enabled = false;
        this._btnOverride.FlatAppearance.BorderSize = 0;
        this._btnOverride.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnOverride.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnOverride.ForeColor = System.Drawing.Color.White;
        this._btnOverride.Location = new System.Drawing.Point(599, 2);
        this._btnOverride.Margin = new System.Windows.Forms.Padding(3, 2, 8, 2);
        this._btnOverride.Name = "_btnOverride";
        this._btnOverride.Size = new System.Drawing.Size(170, 38);
        this._btnOverride.TabIndex = 1;
        this._btnOverride.Text = "تجاوز المشرف (F9)";
        this._btnOverride.UseVisualStyleBackColor = false;
        // 
        // _btnNewHolder
        // 
        this._btnNewHolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._btnNewHolder.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnNewHolder.FlatAppearance.BorderSize = 0;
        this._btnNewHolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnNewHolder.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnNewHolder.ForeColor = System.Drawing.Color.White;
        this._btnNewHolder.Location = new System.Drawing.Point(428, 2);
        this._btnNewHolder.Margin = new System.Windows.Forms.Padding(3, 2, 8, 2);
        this._btnNewHolder.Name = "_btnNewHolder";
        this._btnNewHolder.Size = new System.Drawing.Size(160, 38);
        this._btnNewHolder.TabIndex = 2;
        this._btnNewHolder.Text = "حامل جديد (F2)";
        this._btnNewHolder.UseVisualStyleBackColor = false;
        // 
        // _btnClear
        // 
        this._btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClear.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._btnClear.Location = new System.Drawing.Point(287, 2);
        this._btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 8, 2);
        this._btnClear.Name = "_btnClear";
        this._btnClear.Size = new System.Drawing.Size(130, 38);
        this._btnClear.TabIndex = 3;
        this._btnClear.Text = "مسح الشاشة (Esc)";
        this._btnClear.UseVisualStyleBackColor = false;
        // 
        // _grpToday
        // 
        this._grpToday.BackColor = System.Drawing.Color.White;
        this._grpToday.Controls.Add(this._gridToday);
        this._grpToday.Controls.Add(this._lblTodaySummary);
        this._grpToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpToday.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._grpToday.Location = new System.Drawing.Point(0, 428);
        this._grpToday.Margin = new System.Windows.Forms.Padding(0);
        this._grpToday.Name = "_grpToday";
        this._grpToday.Padding = new System.Windows.Forms.Padding(6);
        this._grpToday.Size = new System.Drawing.Size(988, 213);
        this._grpToday.TabIndex = 4;
        this._grpToday.TabStop = false;
        this._grpToday.Text = "سجل الدخول لهذا اليوم بالمحطة (آخر 20 محاولة)";
        // 
        // _gridToday
        // 
        this._gridToday.AllowUserToAddRows = false;
        this._gridToday.AllowUserToDeleteRows = false;
        this._gridToday.AllowUserToResizeRows = false;
        this._gridToday.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridToday.BackgroundColor = System.Drawing.Color.White;
        this._gridToday.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridToday.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
        this._gridToday.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(4);
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this._gridToday.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        this._gridToday.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridToday.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colTime,
            this._colCard,
            this._colName,
            this._colOrg,
            this._colPersons,
            this._colResult});
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(3);
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this._gridToday.DefaultCellStyle = dataGridViewCellStyle2;
        this._gridToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridToday.EnableHeadersVisualStyles = false;
        this._gridToday.Location = new System.Drawing.Point(6, 23);
        this._gridToday.MultiSelect = false;
        this._gridToday.Name = "_gridToday";
        this._gridToday.ReadOnly = true;
        this._gridToday.RowHeadersVisible = false;
        this._gridToday.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridToday.Size = new System.Drawing.Size(976, 158);
        this._gridToday.TabIndex = 0;
        // 
        // _colTime
        // 
        this._colTime.FillWeight = 60F;
        this._colTime.HeaderText = "الوقت";
        this._colTime.Name = "_colTime";
        this._colTime.ReadOnly = true;
        // 
        // _colCard
        // 
        this._colCard.FillWeight = 80F;
        this._colCard.HeaderText = "رقم البطاقة";
        this._colCard.Name = "_colCard";
        this._colCard.ReadOnly = true;
        // 
        // _colName
        // 
        this._colName.FillWeight = 120F;
        this._colName.HeaderText = "حامل البطاقة";
        this._colName.Name = "_colName";
        this._colName.ReadOnly = true;
        // 
        // _colOrg
        // 
        this._colOrg.FillWeight = 100F;
        this._colOrg.HeaderText = "الجهة";
        this._colOrg.Name = "_colOrg";
        this._colOrg.ReadOnly = true;
        // 
        // _colPersons
        // 
        this._colPersons.FillWeight = 50F;
        this._colPersons.HeaderText = "الأفراد";
        this._colPersons.Name = "_colPersons";
        this._colPersons.ReadOnly = true;
        // 
        // _colResult
        // 
        this._colResult.FillWeight = 70F;
        this._colResult.HeaderText = "النتيجة";
        this._colResult.Name = "_colResult";
        this._colResult.ReadOnly = true;
        // 
        // _lblTodaySummary
        // 
        this._lblTodaySummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        this._lblTodaySummary.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblTodaySummary.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTodaySummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._lblTodaySummary.Location = new System.Drawing.Point(6, 181);
        this._lblTodaySummary.Name = "_lblTodaySummary";
        this._lblTodaySummary.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this._lblTodaySummary.Size = new System.Drawing.Size(976, 26);
        this._lblTodaySummary.TabIndex = 1;
        this._lblTodaySummary.Text = "المقبول: 0 | التجاوز: 0 | المرفوض: 0 | إجمالي الأفراد: 0";
        this._lblTodaySummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // CheckInDeskControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        this.Controls.Add(this._mainLayout);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Name = "CheckInDeskControl";
        this.Padding = new System.Windows.Forms.Padding(10);
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(1008, 661);
        this._mainLayout.ResumeLayout(false);
        this._pnlSearch.ResumeLayout(false);
        this._pnlSearch.PerformLayout();
        this._splitMiddle.Panel1.ResumeLayout(false);
        this._splitMiddle.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._splitMiddle)).EndInit();
        this._splitMiddle.ResumeLayout(false);
        this._grpRegistration.ResumeLayout(false);
        this._tblRegistration.ResumeLayout(false);
        this._tblRegistration.PerformLayout();
        this._pnlRegActions.ResumeLayout(false);
        this._grpHolder.ResumeLayout(false);
        this._tblHolder.ResumeLayout(false);
        this._tblHolder.PerformLayout();
        this._grpCompanions.ResumeLayout(false);
        this._pnlCompanionsTotal.ResumeLayout(false);
        this._pnlCompanionsTotal.PerformLayout();
        this._pnlActions.ResumeLayout(false);
        this._flowActions.ResumeLayout(false);
        this._grpToday.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.Panel _pnlSearch;
    private System.Windows.Forms.Label _lblCardNumber;
    private System.Windows.Forms.TextBox _txtCardNumber;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.ComboBox _cmbOrganization;
    private System.Windows.Forms.Button _btnSearch;
    private ParkGate.WinForms.Controls.StatusBanner _statusBanner;
    private System.Windows.Forms.SplitContainer _splitMiddle;
    private System.Windows.Forms.GroupBox _grpHolder;
    private System.Windows.Forms.TableLayoutPanel _tblHolder;
    private System.Windows.Forms.Label _lblHolderNameTitle;
    private System.Windows.Forms.Label _lblHolderName;
    private System.Windows.Forms.Label _lblHolderPhoneTitle;
    private System.Windows.Forms.Label _lblHolderPhone;
    private System.Windows.Forms.Label _lblHolderOrgTitle;
    private System.Windows.Forms.Label _lblHolderOrg;
    private System.Windows.Forms.Label _lblHolderNatIdTitle;
    private System.Windows.Forms.Label _lblHolderNatId;
    private System.Windows.Forms.Label _lblHolderExpiryTitle;
    private System.Windows.Forms.Label _lblHolderExpiry;
    private System.Windows.Forms.Label _lblHolderLastEntryTitle;
    private System.Windows.Forms.Label _lblHolderLastEntry;
    private System.Windows.Forms.Label _lblHolderStatusTitle;
    private System.Windows.Forms.Label _lblHolderStatus;
    private System.Windows.Forms.GroupBox _grpRegistration;
    private System.Windows.Forms.TableLayoutPanel _tblRegistration;
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
    private System.Windows.Forms.GroupBox _grpCompanions;
    private System.Windows.Forms.FlowLayoutPanel _flowCounters;
    private System.Windows.Forms.Panel _pnlCompanionsTotal;
    private System.Windows.Forms.Label _lblTotalPersons;
    private System.Windows.Forms.Label _lblCompanionsNotice;
    private System.Windows.Forms.Panel _pnlActions;
    private System.Windows.Forms.FlowLayoutPanel _flowActions;
    private System.Windows.Forms.Button _btnRecordEntry;
    private System.Windows.Forms.Button _btnOverride;
    private System.Windows.Forms.Button _btnNewHolder;
    private System.Windows.Forms.Button _btnClear;
    private System.Windows.Forms.GroupBox _grpToday;
    private System.Windows.Forms.DataGridView _gridToday;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colOrg;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPersons;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colResult;
    private System.Windows.Forms.Label _lblTodaySummary;
}
