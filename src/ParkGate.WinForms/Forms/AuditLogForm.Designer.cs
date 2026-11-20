#nullable disable

namespace ParkGate.WinForms.Forms;

partial class AuditLogForm
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
        this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
        this._flowFilters = new System.Windows.Forms.FlowLayoutPanel();
        this._lblFrom = new System.Windows.Forms.Label();
        this._dtpFrom = new System.Windows.Forms.DateTimePicker();
        this._lblTo = new System.Windows.Forms.Label();
        this._dtpTo = new System.Windows.Forms.DateTimePicker();
        this._lblCategory = new System.Windows.Forms.Label();
        this._cmbCategory = new System.Windows.Forms.ComboBox();
        this._lblCard = new System.Windows.Forms.Label();
        this._txtCard = new System.Windows.Forms.TextBox();
        this._btnSearch = new ParkGate.WinForms.Controls.RtlButton();
        this._gridAudit = new System.Windows.Forms.DataGridView();
        this._colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colDesk = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colVisitor = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colOperator = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colApprover = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._grpDetails = new ParkGate.WinForms.Controls.RtlGroupBox();
        this._txtDetails = new System.Windows.Forms.TextBox();
        this._pnlBottom = new System.Windows.Forms.TableLayoutPanel();
        this._lblSummary = new System.Windows.Forms.Label();
        this._btnClose = new ParkGate.WinForms.Controls.RtlButton();
        this._mainLayout.SuspendLayout();
        this._flowFilters.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridAudit)).BeginInit();
        this._grpDetails.SuspendLayout();
        this._pnlBottom.SuspendLayout();
        this.SuspendLayout();
        //
        // _mainLayout
        //
        this._mainLayout.ColumnCount = 1;
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._flowFilters, 0, 0);
        this._mainLayout.Controls.Add(this._gridAudit, 0, 1);
        this._mainLayout.Controls.Add(this._grpDetails, 0, 2);
        this._mainLayout.Controls.Add(this._pnlBottom, 0, 3);
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.Location = new System.Drawing.Point(10, 10);
        this._mainLayout.Name = "_mainLayout";
        this._mainLayout.RowCount = 4;
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        this._mainLayout.Size = new System.Drawing.Size(1160, 680);
        this._mainLayout.TabIndex = 0;
        //
        // _flowFilters
        //
        this._flowFilters.AutoSize = true;
        this._flowFilters.Controls.Add(this._lblFrom);
        this._flowFilters.Controls.Add(this._dtpFrom);
        this._flowFilters.Controls.Add(this._lblTo);
        this._flowFilters.Controls.Add(this._dtpTo);
        this._flowFilters.Controls.Add(this._lblCategory);
        this._flowFilters.Controls.Add(this._cmbCategory);
        this._flowFilters.Controls.Add(this._lblCard);
        this._flowFilters.Controls.Add(this._txtCard);
        this._flowFilters.Controls.Add(this._btnSearch);
        this._flowFilters.Dock = System.Windows.Forms.DockStyle.Fill;
        this._flowFilters.Location = new System.Drawing.Point(3, 3);
        this._flowFilters.Name = "_flowFilters";
        this._flowFilters.Size = new System.Drawing.Size(1154, 46);
        this._flowFilters.TabIndex = 0;
        //
        // _lblFrom
        //
        this._lblFrom.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._lblFrom.AutoSize = true;
        this._lblFrom.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this._lblFrom.Name = "_lblFrom";
        this._lblFrom.TabIndex = 0;
        this._lblFrom.Text = "من تاريخ:";
        //
        // _dtpFrom
        //
        this._dtpFrom.CustomFormat = "dd/MM/yyyy";
        this._dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this._dtpFrom.Name = "_dtpFrom";
        this._dtpFrom.Size = new System.Drawing.Size(140, 32);
        this._dtpFrom.TabIndex = 1;
        //
        // _lblTo
        //
        this._lblTo.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._lblTo.AutoSize = true;
        this._lblTo.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
        this._lblTo.Name = "_lblTo";
        this._lblTo.TabIndex = 2;
        this._lblTo.Text = "إلى تاريخ:";
        //
        // _dtpTo
        //
        this._dtpTo.CustomFormat = "dd/MM/yyyy";
        this._dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this._dtpTo.Name = "_dtpTo";
        this._dtpTo.Size = new System.Drawing.Size(140, 32);
        this._dtpTo.TabIndex = 3;
        //
        // _lblCategory
        //
        this._lblCategory.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._lblCategory.AutoSize = true;
        this._lblCategory.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
        this._lblCategory.Name = "_lblCategory";
        this._lblCategory.TabIndex = 4;
        this._lblCategory.Text = "نوع العملية:";
        //
        // _cmbCategory
        //
        this._cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbCategory.Name = "_cmbCategory";
        this._cmbCategory.Size = new System.Drawing.Size(260, 32);
        this._cmbCategory.TabIndex = 5;
        //
        // _lblCard
        //
        this._lblCard.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._lblCard.AutoSize = true;
        this._lblCard.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
        this._lblCard.Name = "_lblCard";
        this._lblCard.TabIndex = 6;
        this._lblCard.Text = "رقم البطاقة:";
        //
        // _txtCard
        //
        this._txtCard.MaxLength = 50;
        this._txtCard.Name = "_txtCard";
        this._txtCard.Size = new System.Drawing.Size(160, 32);
        this._txtCard.TabIndex = 7;
        //
        // _btnSearch
        //
        this._btnSearch.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
        this._btnSearch.Name = "_btnSearch";
        this._btnSearch.Size = new System.Drawing.Size(110, 34);
        this._btnSearch.TabIndex = 8;
        this._btnSearch.Text = "تحديث";
        //
        // _gridAudit
        //
        this._gridAudit.AllowUserToAddRows = false;
        this._gridAudit.AllowUserToDeleteRows = false;
        this._gridAudit.AutoGenerateColumns = false;
        this._gridAudit.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colTime,
            this._colAction,
            this._colUser,
            this._colDesk,
            this._colCard,
            this._colVisitor,
            this._colReason,
            this._colOperator,
            this._colApprover});
        this._gridAudit.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridAudit.MultiSelect = false;
        this._gridAudit.Name = "_gridAudit";
        this._gridAudit.ReadOnly = true;
        this._gridAudit.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._gridAudit.RowHeadersVisible = false;
        this._gridAudit.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridAudit.Size = new System.Drawing.Size(1154, 400);
        this._gridAudit.TabIndex = 1;
        //
        // _colTime
        //
        this._colTime.FillWeight = 120F;
        this._colTime.HeaderText = "الوقت";
        this._colTime.Name = "_colTime";
        this._colTime.ReadOnly = true;
        //
        // _colAction
        //
        this._colAction.FillWeight = 120F;
        this._colAction.HeaderText = "العملية";
        this._colAction.Name = "_colAction";
        this._colAction.ReadOnly = true;
        //
        // _colUser
        //
        this._colUser.HeaderText = "المستخدم";
        this._colUser.Name = "_colUser";
        this._colUser.ReadOnly = true;
        //
        // _colDesk
        //
        this._colDesk.FillWeight = 70F;
        this._colDesk.HeaderText = "الشباك";
        this._colDesk.Name = "_colDesk";
        this._colDesk.ReadOnly = true;
        //
        // _colCard
        //
        this._colCard.FillWeight = 80F;
        this._colCard.HeaderText = "رقم البطاقة";
        this._colCard.Name = "_colCard";
        this._colCard.ReadOnly = true;
        //
        // _colVisitor
        //
        this._colVisitor.FillWeight = 120F;
        this._colVisitor.HeaderText = "الزائر";
        this._colVisitor.Name = "_colVisitor";
        this._colVisitor.ReadOnly = true;
        //
        // _colReason
        //
        this._colReason.FillWeight = 130F;
        this._colReason.HeaderText = "السبب";
        this._colReason.Name = "_colReason";
        this._colReason.ReadOnly = true;
        //
        // _colOperator
        //
        this._colOperator.HeaderText = "نفّذها";
        this._colOperator.Name = "_colOperator";
        this._colOperator.ReadOnly = true;
        //
        // _colApprover
        //
        this._colApprover.HeaderText = "وافق عليها";
        this._colApprover.Name = "_colApprover";
        this._colApprover.ReadOnly = true;
        //
        // _grpDetails
        //
        this._grpDetails.Controls.Add(this._txtDetails);
        this._grpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._grpDetails.Name = "_grpDetails";
        this._grpDetails.Padding = new System.Windows.Forms.Padding(6);
        this._grpDetails.Size = new System.Drawing.Size(1154, 164);
        this._grpDetails.TabIndex = 2;
        this._grpDetails.TabStop = false;
        this._grpDetails.Text = "تفاصيل العملية المحددة";
        //
        // _txtDetails
        //
        this._txtDetails.BackColor = System.Drawing.Color.White;
        this._txtDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this._txtDetails.Multiline = true;
        this._txtDetails.Name = "_txtDetails";
        this._txtDetails.ReadOnly = true;
        this._txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this._txtDetails.TabIndex = 0;
        //
        // _pnlBottom
        //
        this._pnlBottom.ColumnCount = 2;
        this._pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._pnlBottom.Controls.Add(this._lblSummary, 0, 0);
        this._pnlBottom.Controls.Add(this._btnClose, 1, 0);
        this._pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlBottom.Name = "_pnlBottom";
        this._pnlBottom.RowCount = 1;
        this._pnlBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._pnlBottom.Size = new System.Drawing.Size(1154, 46);
        this._pnlBottom.TabIndex = 3;
        //
        // _lblSummary
        //
        this._lblSummary.AutoEllipsis = true;
        this._lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblSummary.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblSummary.Name = "_lblSummary";
        this._lblSummary.TabIndex = 0;
        this._lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // _btnClose
        //
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(110, 38);
        this._btnClose.TabIndex = 1;
        this._btnClose.Text = "إغلاق";
        //
        // AuditLogForm
        //
        this.AcceptButton = this._btnSearch;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this._btnClose;
        this.ClientSize = new System.Drawing.Size(1180, 700);
        this.Controls.Add(this._mainLayout);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(900, 560);
        this.Name = "AuditLogForm";
        this.Padding = new System.Windows.Forms.Padding(10);
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "سجل التتبع";
        this._mainLayout.ResumeLayout(false);
        this._mainLayout.PerformLayout();
        this._flowFilters.ResumeLayout(false);
        this._flowFilters.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridAudit)).EndInit();
        this._grpDetails.ResumeLayout(false);
        this._grpDetails.PerformLayout();
        this._pnlBottom.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.FlowLayoutPanel _flowFilters;
    private System.Windows.Forms.Label _lblFrom;
    private System.Windows.Forms.DateTimePicker _dtpFrom;
    private System.Windows.Forms.Label _lblTo;
    private System.Windows.Forms.DateTimePicker _dtpTo;
    private System.Windows.Forms.Label _lblCategory;
    private System.Windows.Forms.ComboBox _cmbCategory;
    private System.Windows.Forms.Label _lblCard;
    private System.Windows.Forms.TextBox _txtCard;
    private ParkGate.WinForms.Controls.RtlButton _btnSearch;
    private System.Windows.Forms.DataGridView _gridAudit;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAction;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colUser;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colDesk;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colVisitor;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colReason;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colOperator;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colApprover;
    private ParkGate.WinForms.Controls.RtlGroupBox _grpDetails;
    private System.Windows.Forms.TextBox _txtDetails;
    private System.Windows.Forms.TableLayoutPanel _pnlBottom;
    private System.Windows.Forms.Label _lblSummary;
    private ParkGate.WinForms.Controls.RtlButton _btnClose;
}
