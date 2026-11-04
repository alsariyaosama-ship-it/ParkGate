#nullable disable

namespace ParkGate.WinForms.Controls;

partial class GiftRedeemControl
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
        this._pnlScan = new System.Windows.Forms.Panel();
        this._lblScan = new System.Windows.Forms.Label();
        this._txtScan = new System.Windows.Forms.TextBox();
        this._btnRedeem = new System.Windows.Forms.Button();
        this._btnClear = new System.Windows.Forms.Button();
        this._statusBanner = new ParkGate.WinForms.Controls.StatusBanner();
        this._grpDetails = new System.Windows.Forms.GroupBox();
        this._tblDetails = new System.Windows.Forms.TableLayoutPanel();
        this._lblReceiptNoTitle = new System.Windows.Forms.Label();
        this._lblReceiptNo = new System.Windows.Forms.Label();
        this._lblHolderNameTitle = new System.Windows.Forms.Label();
        this._lblHolderName = new System.Windows.Forms.Label();
        this._lblCardNumberTitle = new System.Windows.Forms.Label();
        this._lblCardNumber = new System.Windows.Forms.Label();
        this._lblOrganizationTitle = new System.Windows.Forms.Label();
        this._lblOrganization = new System.Windows.Forms.Label();
        this._lblValueTitle = new System.Windows.Forms.Label();
        this._lblValue = new System.Windows.Forms.Label();
        this._lblRedeemedAtTitle = new System.Windows.Forms.Label();
        this._lblRedeemedAt = new System.Windows.Forms.Label();
        this._grpToday = new System.Windows.Forms.GroupBox();
        this._gridToday = new System.Windows.Forms.DataGridView();
        this._colRedeemedTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colReceiptNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colHolderName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colCardNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colOrganization = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._lblTodaySummary = new System.Windows.Forms.Label();
        this._mainLayout.SuspendLayout();
        this._pnlScan.SuspendLayout();
        this._grpDetails.SuspendLayout();
        this._tblDetails.SuspendLayout();
        this._grpToday.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).BeginInit();
        this.SuspendLayout();
        // 
        // _mainLayout
        // 
        this._mainLayout.ColumnCount = 1;
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._pnlScan, 0, 0);
        this._mainLayout.Controls.Add(this._statusBanner, 0, 1);
        this._mainLayout.Controls.Add(this._grpDetails, 0, 2);
        this._mainLayout.Controls.Add(this._grpToday, 0, 3);
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.Location = new System.Drawing.Point(10, 10);
        this._mainLayout.Margin = new System.Windows.Forms.Padding(0);
        this._mainLayout.Name = "_mainLayout";
        this._mainLayout.RowCount = 4;
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Size = new System.Drawing.Size(988, 641);
        this._mainLayout.TabIndex = 0;
        // 
        // _pnlScan
        // 
        this._pnlScan.BackColor = System.Drawing.Color.White;
        this._pnlScan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._pnlScan.Controls.Add(this._btnClear);
        this._pnlScan.Controls.Add(this._btnRedeem);
        this._pnlScan.Controls.Add(this._txtScan);
        this._pnlScan.Controls.Add(this._lblScan);
        this._pnlScan.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlScan.Location = new System.Drawing.Point(0, 0);
        this._pnlScan.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._pnlScan.Name = "_pnlScan";
        this._pnlScan.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
        this._pnlScan.Size = new System.Drawing.Size(988, 54);
        this._pnlScan.TabIndex = 0;
        // 
        // _lblScan
        // 
        this._lblScan.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblScan.AutoSize = true;
        this._lblScan.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblScan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._lblScan.Location = new System.Drawing.Point(700, 12);
        this._lblScan.Name = "_lblScan";
        this._lblScan.Size = new System.Drawing.Size(275, 26);
        this._lblScan.TabIndex = 0;
        this._lblScan.Text = "امسح رمز QR الخاص بالهدية أو أدخل رقم الوصل:";
        // 
        // _txtScan
        // 
        this._txtScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtScan.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._txtScan.Location = new System.Drawing.Point(260, 8);
        this._txtScan.MaxLength = 500;
        this._txtScan.Name = "_txtScan";
        this._txtScan.Size = new System.Drawing.Size(430, 37);
        this._txtScan.TabIndex = 1;
        // 
        // _btnRedeem
        // 
        this._btnRedeem.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._btnRedeem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnRedeem.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnRedeem.FlatAppearance.BorderSize = 0;
        this._btnRedeem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnRedeem.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnRedeem.ForeColor = System.Drawing.Color.White;
        this._btnRedeem.Location = new System.Drawing.Point(120, 7);
        this._btnRedeem.Name = "_btnRedeem";
        this._btnRedeem.Size = new System.Drawing.Size(130, 38);
        this._btnRedeem.TabIndex = 2;
        this._btnRedeem.Text = "صرف الهدية (Enter)";
        this._btnRedeem.UseVisualStyleBackColor = false;
        // 
        // _btnClear
        // 
        this._btnClear.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        this._btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(224)))));
        this._btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClear.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._btnClear.Location = new System.Drawing.Point(8, 7);
        this._btnClear.Name = "_btnClear";
        this._btnClear.Size = new System.Drawing.Size(105, 38);
        this._btnClear.TabIndex = 3;
        this._btnClear.Text = "مسح (Esc)";
        this._btnClear.UseVisualStyleBackColor = false;
        // 
        // _statusBanner
        // 
        this._statusBanner.Dock = System.Windows.Forms.DockStyle.Fill;
        this._statusBanner.Location = new System.Drawing.Point(0, 60);
        this._statusBanner.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._statusBanner.Name = "_statusBanner";
        this._statusBanner.Size = new System.Drawing.Size(988, 70);
        this._statusBanner.TabIndex = 1;
        // 
        // _grpDetails
        // 
        this._grpDetails.BackColor = System.Drawing.Color.White;
        this._grpDetails.Controls.Add(this._tblDetails);
        this._grpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpDetails.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._grpDetails.Location = new System.Drawing.Point(0, 136);
        this._grpDetails.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this._grpDetails.Name = "_grpDetails";
        this._grpDetails.Padding = new System.Windows.Forms.Padding(8);
        this._grpDetails.Size = new System.Drawing.Size(988, 124);
        this._grpDetails.TabIndex = 2;
        this._grpDetails.TabStop = false;
        this._grpDetails.Text = "تفاصيل القسيمة المصروفة";
        // 
        // _tblDetails
        // 
        this._tblDetails.ColumnCount = 6;
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        this._tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
        this._tblDetails.Controls.Add(this._lblReceiptNoTitle, 0, 0);
        this._tblDetails.Controls.Add(this._lblReceiptNo, 1, 0);
        this._tblDetails.Controls.Add(this._lblHolderNameTitle, 2, 0);
        this._tblDetails.Controls.Add(this._lblHolderName, 3, 0);
        this._tblDetails.Controls.Add(this._lblValueTitle, 4, 0);
        this._tblDetails.Controls.Add(this._lblValue, 5, 0);
        this._tblDetails.Controls.Add(this._lblCardNumberTitle, 0, 1);
        this._tblDetails.Controls.Add(this._lblCardNumber, 1, 1);
        this._tblDetails.Controls.Add(this._lblOrganizationTitle, 2, 1);
        this._tblDetails.Controls.Add(this._lblOrganization, 3, 1);
        this._tblDetails.Controls.Add(this._lblRedeemedAtTitle, 4, 1);
        this._tblDetails.Controls.Add(this._lblRedeemedAt, 5, 1);
        this._tblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblDetails.Location = new System.Drawing.Point(8, 33);
        this._tblDetails.Name = "_tblDetails";
        this._tblDetails.RowCount = 2;
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this._tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this._tblDetails.Size = new System.Drawing.Size(972, 83);
        this._tblDetails.TabIndex = 0;
        // 
        // _lblReceiptNoTitle
        // 
        this._lblReceiptNoTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblReceiptNoTitle.AutoSize = true;
        this._lblReceiptNoTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblReceiptNoTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblReceiptNoTitle.Location = new System.Drawing.Point(900, 8);
        this._lblReceiptNoTitle.Name = "_lblReceiptNoTitle";
        this._lblReceiptNoTitle.Size = new System.Drawing.Size(69, 24);
        this._lblReceiptNoTitle.TabIndex = 0;
        this._lblReceiptNoTitle.Text = "رقم الوصل:";
        // 
        // _lblReceiptNo
        // 
        this._lblReceiptNo.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblReceiptNo.AutoSize = true;
        this._lblReceiptNo.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblReceiptNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblReceiptNo.Location = new System.Drawing.Point(750, 7);
        this._lblReceiptNo.Name = "_lblReceiptNo";
        this._lblReceiptNo.Size = new System.Drawing.Size(18, 26);
        this._lblReceiptNo.TabIndex = 1;
        this._lblReceiptNo.Text = "-";
        // 
        // _lblHolderNameTitle
        // 
        this._lblHolderNameTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderNameTitle.AutoSize = true;
        this._lblHolderNameTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblHolderNameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblHolderNameTitle.Location = new System.Drawing.Point(570, 8);
        this._lblHolderNameTitle.Name = "_lblHolderNameTitle";
        this._lblHolderNameTitle.Size = new System.Drawing.Size(76, 24);
        this._lblHolderNameTitle.TabIndex = 2;
        this._lblHolderNameTitle.Text = "اسم المستفيد:";
        // 
        // _lblHolderName
        // 
        this._lblHolderName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblHolderName.AutoSize = true;
        this._lblHolderName.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHolderName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this._lblHolderName.Location = new System.Drawing.Point(420, 8);
        this._lblHolderName.Name = "_lblHolderName";
        this._lblHolderName.Size = new System.Drawing.Size(18, 26);
        this._lblHolderName.TabIndex = 3;
        this._lblHolderName.Text = "-";
        // 
        // _lblValueTitle
        // 
        this._lblValueTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblValueTitle.AutoSize = true;
        this._lblValueTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblValueTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblValueTitle.Location = new System.Drawing.Point(240, 8);
        this._lblValueTitle.Name = "_lblValueTitle";
        this._lblValueTitle.Size = new System.Drawing.Size(84, 24);
        this._lblValueTitle.TabIndex = 4;
        this._lblValueTitle.Text = "قيمة القسيمة:";
        // 
        // _lblValue
        // 
        this._lblValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblValue.AutoSize = true;
        this._lblValue.Font = new System.Drawing.Font("Cairo", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblValue.Location = new System.Drawing.Point(100, 4);
        this._lblValue.Name = "_lblValue";
        this._lblValue.Size = new System.Drawing.Size(22, 33);
        this._lblValue.TabIndex = 5;
        this._lblValue.Text = "-";
        // 
        // _lblCardNumberTitle
        // 
        this._lblCardNumberTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCardNumberTitle.AutoSize = true;
        this._lblCardNumberTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblCardNumberTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblCardNumberTitle.Location = new System.Drawing.Point(900, 49);
        this._lblCardNumberTitle.Name = "_lblCardNumberTitle";
        this._lblCardNumberTitle.Size = new System.Drawing.Size(73, 24);
        this._lblCardNumberTitle.TabIndex = 6;
        this._lblCardNumberTitle.Text = "رقم البطاقة:";
        // 
        // _lblCardNumber
        // 
        this._lblCardNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblCardNumber.AutoSize = true;
        this._lblCardNumber.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblCardNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this._lblCardNumber.Location = new System.Drawing.Point(750, 48);
        this._lblCardNumber.Name = "_lblCardNumber";
        this._lblCardNumber.Size = new System.Drawing.Size(18, 26);
        this._lblCardNumber.TabIndex = 7;
        this._lblCardNumber.Text = "-";
        // 
        // _lblOrganizationTitle
        // 
        this._lblOrganizationTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblOrganizationTitle.AutoSize = true;
        this._lblOrganizationTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblOrganizationTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblOrganizationTitle.Location = new System.Drawing.Point(570, 49);
        this._lblOrganizationTitle.Name = "_lblOrganizationTitle";
        this._lblOrganizationTitle.Size = new System.Drawing.Size(46, 24);
        this._lblOrganizationTitle.TabIndex = 8;
        this._lblOrganizationTitle.Text = "الجهة:";
        // 
        // _lblOrganization
        // 
        this._lblOrganization.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblOrganization.AutoSize = true;
        this._lblOrganization.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblOrganization.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this._lblOrganization.Location = new System.Drawing.Point(420, 48);
        this._lblOrganization.Name = "_lblOrganization";
        this._lblOrganization.Size = new System.Drawing.Size(18, 26);
        this._lblOrganization.TabIndex = 9;
        this._lblOrganization.Text = "-";
        // 
        // _lblRedeemedAtTitle
        // 
        this._lblRedeemedAtTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRedeemedAtTitle.AutoSize = true;
        this._lblRedeemedAtTitle.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblRedeemedAtTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblRedeemedAtTitle.Location = new System.Drawing.Point(240, 49);
        this._lblRedeemedAtTitle.Name = "_lblRedeemedAtTitle";
        this._lblRedeemedAtTitle.Size = new System.Drawing.Size(76, 24);
        this._lblRedeemedAtTitle.TabIndex = 10;
        this._lblRedeemedAtTitle.Text = "وقت الصرف:";
        // 
        // _lblRedeemedAt
        // 
        this._lblRedeemedAt.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRedeemedAt.AutoSize = true;
        this._lblRedeemedAt.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblRedeemedAt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this._lblRedeemedAt.Location = new System.Drawing.Point(100, 48);
        this._lblRedeemedAt.Name = "_lblRedeemedAt";
        this._lblRedeemedAt.Size = new System.Drawing.Size(18, 26);
        this._lblRedeemedAt.TabIndex = 11;
        this._lblRedeemedAt.Text = "-";
        // 
        // _grpToday
        // 
        this._grpToday.BackColor = System.Drawing.Color.White;
        this._grpToday.Controls.Add(this._gridToday);
        this._grpToday.Controls.Add(this._lblTodaySummary);
        this._grpToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpToday.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._grpToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._grpToday.Location = new System.Drawing.Point(0, 266);
        this._grpToday.Margin = new System.Windows.Forms.Padding(0);
        this._grpToday.Name = "_grpToday";
        this._grpToday.Padding = new System.Windows.Forms.Padding(6);
        this._grpToday.Size = new System.Drawing.Size(988, 375);
        this._grpToday.TabIndex = 3;
        this._grpToday.TabStop = false;
        this._grpToday.Text = "سجل الهدايا المصروفة اليوم بهذا الشباك (آخر 20 قسيمة)";
        // 
        // _gridToday
        // 
        this._gridToday.AllowUserToAddRows = false;
        this._gridToday.AllowUserToDeleteRows = false;
        this._gridToday.AllowUserToResizeRows = false;
        this._gridToday.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridToday.BackgroundColor = System.Drawing.Color.White;
        this._gridToday.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridToday.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this._gridToday.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        this._gridToday.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridToday.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colRedeemedTime,
            this._colReceiptNo,
            this._colHolderName,
            this._colCardNumber,
            this._colOrganization,
            this._colValue});
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this._gridToday.DefaultCellStyle = dataGridViewCellStyle2;
        this._gridToday.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridToday.EnableHeadersVisualStyles = false;
        this._gridToday.Location = new System.Drawing.Point(6, 30);
        this._gridToday.MultiSelect = false;
        this._gridToday.Name = "_gridToday";
        this._gridToday.ReadOnly = true;
        this._gridToday.RowHeadersVisible = false;
        this._gridToday.RowTemplate.Height = 32;
        this._gridToday.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridToday.Size = new System.Drawing.Size(976, 307);
        this._gridToday.TabIndex = 0;
        // 
        // _colRedeemedTime
        // 
        this._colRedeemedTime.FillWeight = 85F;
        this._colRedeemedTime.HeaderText = "وقت الصرف";
        this._colRedeemedTime.Name = "_colRedeemedTime";
        this._colRedeemedTime.ReadOnly = true;
        // 
        // _colReceiptNo
        // 
        this._colReceiptNo.FillWeight = 80F;
        this._colReceiptNo.HeaderText = "رقم الوصل";
        this._colReceiptNo.Name = "_colReceiptNo";
        this._colReceiptNo.ReadOnly = true;
        // 
        // _colHolderName
        // 
        this._colHolderName.FillWeight = 140F;
        this._colHolderName.HeaderText = "اسم الزبون";
        this._colHolderName.Name = "_colHolderName";
        this._colHolderName.ReadOnly = true;
        // 
        // _colCardNumber
        // 
        this._colCardNumber.FillWeight = 85F;
        this._colCardNumber.HeaderText = "رقم البطاقة";
        this._colCardNumber.Name = "_colCardNumber";
        this._colCardNumber.ReadOnly = true;
        // 
        // _colOrganization
        // 
        this._colOrganization.FillWeight = 110F;
        this._colOrganization.HeaderText = "الجهة";
        this._colOrganization.Name = "_colOrganization";
        this._colOrganization.ReadOnly = true;
        // 
        // _colValue
        // 
        this._colValue.FillWeight = 80F;
        this._colValue.HeaderText = "القيمة (د.ل)";
        this._colValue.Name = "_colValue";
        this._colValue.ReadOnly = true;
        // 
        // _lblTodaySummary
        // 
        this._lblTodaySummary.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lblTodaySummary.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblTodaySummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblTodaySummary.Location = new System.Drawing.Point(6, 337);
        this._lblTodaySummary.Name = "_lblTodaySummary";
        this._lblTodaySummary.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        this._lblTodaySummary.Size = new System.Drawing.Size(976, 32);
        this._lblTodaySummary.TabIndex = 1;
        this._lblTodaySummary.Text = "إجمالي المصروف اليوم: 0 قسيمة بقيمة 0 د.ل";
        this._lblTodaySummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // GiftRedeemControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        this.Controls.Add(this._mainLayout);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Name = "GiftRedeemControl";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.Size = new System.Drawing.Size(1008, 661);
        this._mainLayout.ResumeLayout(false);
        this._pnlScan.ResumeLayout(false);
        this._pnlScan.PerformLayout();
        this._grpDetails.ResumeLayout(false);
        this._tblDetails.ResumeLayout(false);
        this._tblDetails.PerformLayout();
        this._grpToday.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridToday)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.Panel _pnlScan;
    private System.Windows.Forms.Label _lblScan;
    private System.Windows.Forms.TextBox _txtScan;
    private System.Windows.Forms.Button _btnRedeem;
    private System.Windows.Forms.Button _btnClear;
    private ParkGate.WinForms.Controls.StatusBanner _statusBanner;
    private System.Windows.Forms.GroupBox _grpDetails;
    private System.Windows.Forms.TableLayoutPanel _tblDetails;
    private System.Windows.Forms.Label _lblReceiptNoTitle;
    private System.Windows.Forms.Label _lblReceiptNo;
    private System.Windows.Forms.Label _lblHolderNameTitle;
    private System.Windows.Forms.Label _lblHolderName;
    private System.Windows.Forms.Label _lblCardNumberTitle;
    private System.Windows.Forms.Label _lblCardNumber;
    private System.Windows.Forms.Label _lblOrganizationTitle;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.Label _lblValueTitle;
    private System.Windows.Forms.Label _lblValue;
    private System.Windows.Forms.Label _lblRedeemedAtTitle;
    private System.Windows.Forms.Label _lblRedeemedAt;
    private System.Windows.Forms.GroupBox _grpToday;
    private System.Windows.Forms.DataGridView _gridToday;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colRedeemedTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colReceiptNo;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colHolderName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCardNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colOrganization;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colValue;
    private System.Windows.Forms.Label _lblTodaySummary;
}
