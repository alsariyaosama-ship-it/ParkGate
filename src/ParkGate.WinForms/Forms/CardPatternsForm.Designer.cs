#nullable disable

namespace ParkGate.WinForms.Forms;

partial class CardPatternsForm
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
        this._tabs = new ParkGate.WinForms.Controls.RtlTabControl();
        this._tabPatterns = new System.Windows.Forms.TabPage();
        this._tblPatterns = new System.Windows.Forms.TableLayoutPanel();
        this._gridPatterns = new System.Windows.Forms.DataGridView();
        this._colPatternOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternDisplay = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternSample = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternProposed = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colPatternDecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._flowPatternButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnApprove = new ParkGate.WinForms.Controls.RtlButton();
        this._btnReject = new ParkGate.WinForms.Controls.RtlButton();
        this._lblPatternHint = new System.Windows.Forms.Label();
        this._tabAlerts = new System.Windows.Forms.TabPage();
        this._tblAlerts = new System.Windows.Forms.TableLayoutPanel();
        this._chkShowReviewed = new ParkGate.WinForms.Controls.RtlCheckBox();
        this._gridAlerts = new System.Windows.Forms.DataGridView();
        this._colAlertTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertHolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertShape = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertDesk = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colAlertNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._tblReview = new System.Windows.Forms.TableLayoutPanel();
        this._lblReviewNote = new System.Windows.Forms.Label();
        this._txtReviewNote = new System.Windows.Forms.TextBox();
        this._btnReview = new ParkGate.WinForms.Controls.RtlButton();
        this._btnChangeStatus = new ParkGate.WinForms.Controls.RtlButton();
        this._pnlBottom = new System.Windows.Forms.TableLayoutPanel();
        this._lblSummary = new System.Windows.Forms.Label();
        this._btnClose = new ParkGate.WinForms.Controls.RtlButton();
        this._mainLayout.SuspendLayout();
        this._tabs.SuspendLayout();
        this._tabPatterns.SuspendLayout();
        this._tblPatterns.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridPatterns)).BeginInit();
        this._flowPatternButtons.SuspendLayout();
        this._tabAlerts.SuspendLayout();
        this._tblAlerts.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridAlerts)).BeginInit();
        this._tblReview.SuspendLayout();
        this._pnlBottom.SuspendLayout();
        this.SuspendLayout();
        //
        // _mainLayout
        //
        this._mainLayout.ColumnCount = 1;
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._tabs, 0, 0);
        this._mainLayout.Controls.Add(this._pnlBottom, 0, 1);
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.Location = new System.Drawing.Point(0, 0);
        this._mainLayout.Name = "_mainLayout";
        this._mainLayout.Padding = new System.Windows.Forms.Padding(10);
        this._mainLayout.RowCount = 2;
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        this._mainLayout.Size = new System.Drawing.Size(1120, 640);
        this._mainLayout.TabIndex = 0;
        //
        // _tabs
        //
        this._tabs.Controls.Add(this._tabPatterns);
        this._tabs.Controls.Add(this._tabAlerts);
        this._tabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tabs.Name = "_tabs";
        this._tabs.SelectedIndex = 0;
        this._tabs.Size = new System.Drawing.Size(1100, 568);
        this._tabs.TabIndex = 0;
        //
        // _tabPatterns
        //
        this._tabPatterns.Controls.Add(this._tblPatterns);
        this._tabPatterns.Name = "_tabPatterns";
        this._tabPatterns.Padding = new System.Windows.Forms.Padding(6);
        this._tabPatterns.Size = new System.Drawing.Size(1092, 530);
        this._tabPatterns.TabIndex = 0;
        this._tabPatterns.Text = "الأنماط";
        this._tabPatterns.UseVisualStyleBackColor = true;
        //
        // _tblPatterns
        //
        this._tblPatterns.ColumnCount = 1;
        this._tblPatterns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblPatterns.Controls.Add(this._lblPatternHint, 0, 0);
        this._tblPatterns.Controls.Add(this._gridPatterns, 0, 1);
        this._tblPatterns.Controls.Add(this._flowPatternButtons, 0, 2);
        this._tblPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblPatterns.Name = "_tblPatterns";
        this._tblPatterns.RowCount = 3;
        this._tblPatterns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblPatterns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblPatterns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        this._tblPatterns.Size = new System.Drawing.Size(1080, 518);
        this._tblPatterns.TabIndex = 0;
        //
        // _lblPatternHint
        //
        this._lblPatternHint.AutoSize = true;
        this._lblPatternHint.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblPatternHint.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
        this._lblPatternHint.Name = "_lblPatternHint";
        this._lblPatternHint.TabIndex = 0;
        this._lblPatternHint.Text = "# رقم، A حرف.";
        //
        // _gridPatterns
        //
        this._gridPatterns.AllowUserToAddRows = false;
        this._gridPatterns.AllowUserToDeleteRows = false;
        this._gridPatterns.AutoGenerateColumns = false;
        this._gridPatterns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colPatternOrg,
            this._colPatternDisplay,
            this._colPatternSource,
            this._colPatternStatus,
            this._colPatternSample,
            this._colPatternProposed,
            this._colPatternDecision});
        this._gridPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridPatterns.MultiSelect = false;
        this._gridPatterns.Name = "_gridPatterns";
        this._gridPatterns.ReadOnly = true;
        this._gridPatterns.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._gridPatterns.RowHeadersVisible = false;
        this._gridPatterns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridPatterns.Size = new System.Drawing.Size(1074, 420);
        this._gridPatterns.TabIndex = 1;
        //
        // _colPatternOrg
        //
        this._colPatternOrg.FillWeight = 22F;
        this._colPatternOrg.HeaderText = "الجهة";
        this._colPatternOrg.Name = "_colPatternOrg";
        this._colPatternOrg.ReadOnly = true;
        //
        // _colPatternDisplay
        //
        this._colPatternDisplay.FillWeight = 16F;
        this._colPatternDisplay.HeaderText = "شكل الرقم";
        this._colPatternDisplay.Name = "_colPatternDisplay";
        this._colPatternDisplay.ReadOnly = true;
        //
        // _colPatternSource
        //
        this._colPatternSource.FillWeight = 14F;
        this._colPatternSource.HeaderText = "المصدر";
        this._colPatternSource.Name = "_colPatternSource";
        this._colPatternSource.ReadOnly = true;
        //
        // _colPatternStatus
        //
        this._colPatternStatus.FillWeight = 18F;
        this._colPatternStatus.HeaderText = "الحالة";
        this._colPatternStatus.Name = "_colPatternStatus";
        this._colPatternStatus.ReadOnly = true;
        //
        // _colPatternSample
        //
        this._colPatternSample.FillWeight = 8F;
        this._colPatternSample.HeaderText = "عدد البطاقات";
        this._colPatternSample.Name = "_colPatternSample";
        this._colPatternSample.ReadOnly = true;
        //
        // _colPatternProposed
        //
        this._colPatternProposed.FillWeight = 12F;
        this._colPatternProposed.HeaderText = "اقتُرح في";
        this._colPatternProposed.Name = "_colPatternProposed";
        this._colPatternProposed.ReadOnly = true;
        //
        // _colPatternDecision
        //
        this._colPatternDecision.FillWeight = 18F;
        this._colPatternDecision.HeaderText = "القرار";
        this._colPatternDecision.Name = "_colPatternDecision";
        this._colPatternDecision.ReadOnly = true;
        //
        // _flowPatternButtons
        //
        this._flowPatternButtons.Controls.Add(this._btnApprove);
        this._flowPatternButtons.Controls.Add(this._btnReject);
        this._flowPatternButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this._flowPatternButtons.Name = "_flowPatternButtons";
        this._flowPatternButtons.Size = new System.Drawing.Size(1074, 46);
        this._flowPatternButtons.TabIndex = 2;
        //
        // _btnApprove
        //
        this._btnApprove.Enabled = false;
        this._btnApprove.Name = "_btnApprove";
        this._btnApprove.Size = new System.Drawing.Size(150, 38);
        this._btnApprove.TabIndex = 0;
        this._btnApprove.Text = "اعتماد النمط";
        //
        // _btnReject
        //
        this._btnReject.Enabled = false;
        this._btnReject.Name = "_btnReject";
        this._btnReject.Size = new System.Drawing.Size(150, 38);
        this._btnReject.TabIndex = 1;
        this._btnReject.Text = "رفض النمط";
        //
        // _tabAlerts
        //
        this._tabAlerts.Controls.Add(this._tblAlerts);
        this._tabAlerts.Name = "_tabAlerts";
        this._tabAlerts.Padding = new System.Windows.Forms.Padding(6);
        this._tabAlerts.Size = new System.Drawing.Size(1092, 530);
        this._tabAlerts.TabIndex = 1;
        this._tabAlerts.Text = "التنبيهات";
        this._tabAlerts.UseVisualStyleBackColor = true;
        //
        // _tblAlerts
        //
        this._tblAlerts.ColumnCount = 1;
        this._tblAlerts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblAlerts.Controls.Add(this._chkShowReviewed, 0, 0);
        this._tblAlerts.Controls.Add(this._gridAlerts, 0, 1);
        this._tblAlerts.Controls.Add(this._tblReview, 0, 2);
        this._tblAlerts.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblAlerts.Name = "_tblAlerts";
        this._tblAlerts.RowCount = 3;
        this._tblAlerts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblAlerts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblAlerts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        this._tblAlerts.Size = new System.Drawing.Size(1080, 518);
        this._tblAlerts.TabIndex = 0;
        //
        // _chkShowReviewed
        //
        this._chkShowReviewed.AutoSize = true;
        this._chkShowReviewed.Name = "_chkShowReviewed";
        this._chkShowReviewed.Size = new System.Drawing.Size(220, 27);
        this._chkShowReviewed.TabIndex = 0;
        this._chkShowReviewed.Text = "إظهار التنبيهات المراجَعة أيضًا";
        //
        // _gridAlerts
        //
        this._gridAlerts.AllowUserToAddRows = false;
        this._gridAlerts.AllowUserToDeleteRows = false;
        this._gridAlerts.AutoGenerateColumns = false;
        this._gridAlerts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colAlertTime,
            this._colAlertOrg,
            this._colAlertCard,
            this._colAlertHolder,
            this._colAlertShape,
            this._colAlertBy,
            this._colAlertDesk,
            this._colAlertStatus,
            this._colAlertNote});
        this._gridAlerts.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridAlerts.MultiSelect = false;
        this._gridAlerts.Name = "_gridAlerts";
        this._gridAlerts.ReadOnly = true;
        this._gridAlerts.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._gridAlerts.RowHeadersVisible = false;
        this._gridAlerts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridAlerts.Size = new System.Drawing.Size(1074, 420);
        this._gridAlerts.TabIndex = 1;
        //
        // _colAlertTime
        //
        this._colAlertTime.FillWeight = 14F;
        this._colAlertTime.HeaderText = "الوقت";
        this._colAlertTime.Name = "_colAlertTime";
        this._colAlertTime.ReadOnly = true;
        //
        // _colAlertOrg
        //
        this._colAlertOrg.FillWeight = 14F;
        this._colAlertOrg.HeaderText = "الجهة";
        this._colAlertOrg.Name = "_colAlertOrg";
        this._colAlertOrg.ReadOnly = true;
        //
        // _colAlertCard
        //
        this._colAlertCard.FillWeight = 11F;
        this._colAlertCard.HeaderText = "رقم البطاقة";
        this._colAlertCard.Name = "_colAlertCard";
        this._colAlertCard.ReadOnly = true;
        //
        // _colAlertHolder
        //
        this._colAlertHolder.FillWeight = 16F;
        this._colAlertHolder.HeaderText = "الزائر";
        this._colAlertHolder.Name = "_colAlertHolder";
        this._colAlertHolder.ReadOnly = true;
        //
        // _colAlertShape
        //
        this._colAlertShape.FillWeight = 10F;
        this._colAlertShape.HeaderText = "شكل الرقم";
        this._colAlertShape.Name = "_colAlertShape";
        this._colAlertShape.ReadOnly = true;
        //
        // _colAlertBy
        //
        this._colAlertBy.FillWeight = 11F;
        this._colAlertBy.HeaderText = "الموظف";
        this._colAlertBy.Name = "_colAlertBy";
        this._colAlertBy.ReadOnly = true;
        //
        // _colAlertDesk
        //
        this._colAlertDesk.FillWeight = 9F;
        this._colAlertDesk.HeaderText = "الشباك";
        this._colAlertDesk.Name = "_colAlertDesk";
        this._colAlertDesk.ReadOnly = true;
        //
        // _colAlertStatus
        //
        this._colAlertStatus.FillWeight = 13F;
        this._colAlertStatus.HeaderText = "الحالة";
        this._colAlertStatus.Name = "_colAlertStatus";
        this._colAlertStatus.ReadOnly = true;
        //
        // _colAlertNote
        //
        this._colAlertNote.FillWeight = 16F;
        this._colAlertNote.HeaderText = "ملاحظة المراجعة";
        this._colAlertNote.Name = "_colAlertNote";
        this._colAlertNote.ReadOnly = true;
        //
        // _tblReview
        //
        this._tblReview.ColumnCount = 4;
        this._tblReview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblReview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblReview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblReview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._tblReview.Controls.Add(this._lblReviewNote, 0, 0);
        this._tblReview.Controls.Add(this._txtReviewNote, 1, 0);
        this._tblReview.Controls.Add(this._btnReview, 2, 0);
        this._tblReview.Controls.Add(this._btnChangeStatus, 3, 0);
        this._tblReview.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblReview.Name = "_tblReview";
        this._tblReview.RowCount = 1;
        this._tblReview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblReview.Size = new System.Drawing.Size(1074, 46);
        this._tblReview.TabIndex = 2;
        //
        // _lblReviewNote
        //
        this._lblReviewNote.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblReviewNote.AutoSize = true;
        this._lblReviewNote.Name = "_lblReviewNote";
        this._lblReviewNote.TabIndex = 0;
        this._lblReviewNote.Text = "ملاحظة (اختيارية):";
        //
        // _txtReviewNote
        //
        this._txtReviewNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this._txtReviewNote.MaxLength = 250;
        this._txtReviewNote.Name = "_txtReviewNote";
        this._txtReviewNote.Size = new System.Drawing.Size(500, 27);
        this._txtReviewNote.TabIndex = 1;
        //
        // _btnReview
        //
        this._btnReview.Enabled = false;
        this._btnReview.Name = "_btnReview";
        this._btnReview.Size = new System.Drawing.Size(150, 38);
        this._btnReview.TabIndex = 2;
        this._btnReview.Text = "تمت المراجعة";
        //
        // _btnChangeStatus
        //
        this._btnChangeStatus.Enabled = false;
        this._btnChangeStatus.Name = "_btnChangeStatus";
        this._btnChangeStatus.Size = new System.Drawing.Size(190, 38);
        this._btnChangeStatus.TabIndex = 3;
        this._btnChangeStatus.Text = "إيقاف أو حظر الزائر…";
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
        this._pnlBottom.Size = new System.Drawing.Size(1100, 46);
        this._pnlBottom.TabIndex = 1;
        //
        // _lblSummary
        //
        this._lblSummary.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblSummary.AutoSize = true;
        this._lblSummary.Name = "_lblSummary";
        this._lblSummary.TabIndex = 0;
        //
        // _btnClose
        //
        this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(110, 38);
        this._btnClose.TabIndex = 1;
        this._btnClose.Text = "إغلاق";
        //
        // CardPatternsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this._btnClose;
        this.ClientSize = new System.Drawing.Size(1120, 640);
        this.Controls.Add(this._mainLayout);
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(900, 560);
        this.Name = "CardPatternsForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "أنماط أرقام البطاقات";
        this._mainLayout.ResumeLayout(false);
        this._tabs.ResumeLayout(false);
        this._tabPatterns.ResumeLayout(false);
        this._tblPatterns.ResumeLayout(false);
        this._tblPatterns.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridPatterns)).EndInit();
        this._flowPatternButtons.ResumeLayout(false);
        this._tabAlerts.ResumeLayout(false);
        this._tblAlerts.ResumeLayout(false);
        this._tblAlerts.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridAlerts)).EndInit();
        this._tblReview.ResumeLayout(false);
        this._tblReview.PerformLayout();
        this._pnlBottom.ResumeLayout(false);
        this._pnlBottom.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private ParkGate.WinForms.Controls.RtlTabControl _tabs;
    private System.Windows.Forms.TabPage _tabPatterns;
    private System.Windows.Forms.TableLayoutPanel _tblPatterns;
    private System.Windows.Forms.Label _lblPatternHint;
    private System.Windows.Forms.DataGridView _gridPatterns;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternOrg;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternDisplay;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternSource;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternSample;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternProposed;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colPatternDecision;
    private System.Windows.Forms.FlowLayoutPanel _flowPatternButtons;
    private ParkGate.WinForms.Controls.RtlButton _btnApprove;
    private ParkGate.WinForms.Controls.RtlButton _btnReject;
    private System.Windows.Forms.TabPage _tabAlerts;
    private System.Windows.Forms.TableLayoutPanel _tblAlerts;
    private ParkGate.WinForms.Controls.RtlCheckBox _chkShowReviewed;
    private System.Windows.Forms.DataGridView _gridAlerts;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertOrg;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertHolder;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertShape;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertBy;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertDesk;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colAlertNote;
    private System.Windows.Forms.TableLayoutPanel _tblReview;
    private System.Windows.Forms.Label _lblReviewNote;
    private System.Windows.Forms.TextBox _txtReviewNote;
    private ParkGate.WinForms.Controls.RtlButton _btnReview;
    private ParkGate.WinForms.Controls.RtlButton _btnChangeStatus;
    private System.Windows.Forms.TableLayoutPanel _pnlBottom;
    private System.Windows.Forms.Label _lblSummary;
    private ParkGate.WinForms.Controls.RtlButton _btnClose;
}
