#nullable disable

namespace ParkGate.WinForms.Forms;

partial class GiftReceiptsForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.FlowLayoutPanel _filtersPanel;
    private System.Windows.Forms.Label _lblFromDate;
    private System.Windows.Forms.DateTimePicker _dtpFromDate;
    private System.Windows.Forms.Label _lblToDate;
    private System.Windows.Forms.DateTimePicker _dtpToDate;
    private System.Windows.Forms.Label _lblOrganization;
    private System.Windows.Forms.ComboBox _cmbOrganization;
    private System.Windows.Forms.Label _lblStatus;
    private System.Windows.Forms.ComboBox _cmbStatus;
    private System.Windows.Forms.Button _btnRefresh;
    private System.Windows.Forms.Button _btnExportExcel;
    private System.Windows.Forms.Button _btnPrintPreview;
    private System.Windows.Forms.DataGridView _gridReceipts;
    private System.Windows.Forms.Panel _summaryPanel;
    private System.Windows.Forms.Label _lblSummary;

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
        _mainLayout = new System.Windows.Forms.TableLayoutPanel();
        _filtersPanel = new System.Windows.Forms.FlowLayoutPanel();
        _lblFromDate = new System.Windows.Forms.Label();
        _dtpFromDate = new System.Windows.Forms.DateTimePicker();
        _lblToDate = new System.Windows.Forms.Label();
        _dtpToDate = new System.Windows.Forms.DateTimePicker();
        _lblOrganization = new System.Windows.Forms.Label();
        _cmbOrganization = new System.Windows.Forms.ComboBox();
        _lblStatus = new System.Windows.Forms.Label();
        _cmbStatus = new System.Windows.Forms.ComboBox();
        _btnRefresh = new System.Windows.Forms.Button();
        _btnExportExcel = new System.Windows.Forms.Button();
        _btnPrintPreview = new System.Windows.Forms.Button();
        _gridReceipts = new System.Windows.Forms.DataGridView();
        _summaryPanel = new System.Windows.Forms.Panel();
        _lblSummary = new System.Windows.Forms.Label();

        _mainLayout.SuspendLayout();
        _filtersPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridReceipts).BeginInit();
        _summaryPanel.SuspendLayout();
        SuspendLayout();

        // 
        // _mainLayout
        // 
        _mainLayout.ColumnCount = 1;
        _mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        _mainLayout.Controls.Add(_filtersPanel, 0, 0);
        _mainLayout.Controls.Add(_gridReceipts, 0, 1);
        _mainLayout.Controls.Add(_summaryPanel, 0, 2);
        _mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        _mainLayout.Location = new System.Drawing.Point(0, 0);
        _mainLayout.Name = "_mainLayout";
        _mainLayout.Padding = new System.Windows.Forms.Padding(12);
        _mainLayout.RowCount = 3;
        _mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
        _mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        _mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        _mainLayout.Size = new System.Drawing.Size(1100, 680);
        _mainLayout.TabIndex = 0;

        // 
        // _filtersPanel
        // 
        _filtersPanel.Controls.Add(_lblFromDate);
        _filtersPanel.Controls.Add(_dtpFromDate);
        _filtersPanel.Controls.Add(_lblToDate);
        _filtersPanel.Controls.Add(_dtpToDate);
        _filtersPanel.Controls.Add(_lblOrganization);
        _filtersPanel.Controls.Add(_cmbOrganization);
        _filtersPanel.Controls.Add(_lblStatus);
        _filtersPanel.Controls.Add(_cmbStatus);
        _filtersPanel.Controls.Add(_btnRefresh);
        _filtersPanel.Controls.Add(_btnExportExcel);
        _filtersPanel.Controls.Add(_btnPrintPreview);
        _filtersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        _filtersPanel.Location = new System.Drawing.Point(15, 15);
        _filtersPanel.Name = "_filtersPanel";
        _filtersPanel.Size = new System.Drawing.Size(1070, 49);
        _filtersPanel.TabIndex = 0;

        // 
        // _lblFromDate
        // 
        _lblFromDate.AutoSize = true;
        _lblFromDate.Location = new System.Drawing.Point(1010, 8);
        _lblFromDate.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        _lblFromDate.Name = "_lblFromDate";
        _lblFromDate.Size = new System.Drawing.Size(57, 19);
        _lblFromDate.TabIndex = 0;
        _lblFromDate.Text = "من تاريخ:";

        // 
        // _dtpFromDate
        // 
        _dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        _dtpFromDate.Location = new System.Drawing.Point(884, 5);
        _dtpFromDate.Margin = new System.Windows.Forms.Padding(3, 5, 12, 3);
        _dtpFromDate.Name = "_dtpFromDate";
        _dtpFromDate.Size = new System.Drawing.Size(120, 27);
        _dtpFromDate.TabIndex = 1;

        // 
        // _lblToDate
        // 
        _lblToDate.AutoSize = true;
        _lblToDate.Location = new System.Drawing.Point(819, 8);
        _lblToDate.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        _lblToDate.Name = "_lblToDate";
        _lblToDate.Size = new System.Drawing.Size(59, 19);
        _lblToDate.TabIndex = 2;
        _lblToDate.Text = "إلى تاريخ:";

        // 
        // _dtpToDate
        // 
        _dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        _dtpToDate.Location = new System.Drawing.Point(693, 5);
        _dtpToDate.Margin = new System.Windows.Forms.Padding(3, 5, 12, 3);
        _dtpToDate.Name = "_dtpToDate";
        _dtpToDate.Size = new System.Drawing.Size(120, 27);
        _dtpToDate.TabIndex = 3;

        // 
        // _lblOrganization
        // 
        _lblOrganization.AutoSize = true;
        _lblOrganization.Location = new System.Drawing.Point(645, 8);
        _lblOrganization.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        _lblOrganization.Name = "_lblOrganization";
        _lblOrganization.Size = new System.Drawing.Size(42, 19);
        _lblOrganization.TabIndex = 4;
        _lblOrganization.Text = "الجهة:";

        // 
        // _cmbOrganization
        // 
        _cmbOrganization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        _cmbOrganization.Location = new System.Drawing.Point(479, 5);
        _cmbOrganization.Margin = new System.Windows.Forms.Padding(3, 5, 12, 3);
        _cmbOrganization.Name = "_cmbOrganization";
        _cmbOrganization.Size = new System.Drawing.Size(160, 27);
        _cmbOrganization.TabIndex = 5;

        // 
        // _lblStatus
        // 
        _lblStatus.AutoSize = true;
        _lblStatus.Location = new System.Drawing.Point(427, 8);
        _lblStatus.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        _lblStatus.Name = "_lblStatus";
        _lblStatus.Size = new System.Drawing.Size(46, 19);
        _lblStatus.TabIndex = 6;
        _lblStatus.Text = "الحالة:";

        // 
        // _cmbStatus
        // 
        _cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        _cmbStatus.Location = new System.Drawing.Point(291, 5);
        _cmbStatus.Margin = new System.Windows.Forms.Padding(3, 5, 12, 3);
        _cmbStatus.Name = "_cmbStatus";
        _cmbStatus.Size = new System.Drawing.Size(130, 27);
        _cmbStatus.TabIndex = 7;

        // 
        // _btnRefresh
        // 
        _btnRefresh.Location = new System.Drawing.Point(195, 4);
        _btnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 6, 3);
        _btnRefresh.Name = "_btnRefresh";
        _btnRefresh.Size = new System.Drawing.Size(90, 30);
        _btnRefresh.TabIndex = 8;
        _btnRefresh.Text = "تحديث";
        _btnRefresh.UseVisualStyleBackColor = true;

        // 
        // _btnExportExcel
        // 
        _btnExportExcel.Location = new System.Drawing.Point(89, 4);
        _btnExportExcel.Margin = new System.Windows.Forms.Padding(3, 4, 6, 3);
        _btnExportExcel.Name = "_btnExportExcel";
        _btnExportExcel.Size = new System.Drawing.Size(100, 30);
        _btnExportExcel.TabIndex = 9;
        _btnExportExcel.Text = "تصدير Excel";
        _btnExportExcel.UseVisualStyleBackColor = true;

        // 
        // _btnPrintPreview
        // 
        _btnPrintPreview.Location = new System.Drawing.Point(3, 4);
        _btnPrintPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
        _btnPrintPreview.Name = "_btnPrintPreview";
        _btnPrintPreview.Size = new System.Drawing.Size(80, 30);
        _btnPrintPreview.TabIndex = 10;
        _btnPrintPreview.Text = "طباعة";
        _btnPrintPreview.UseVisualStyleBackColor = true;

        // 
        // _gridReceipts
        // 
        _gridReceipts.AllowUserToAddRows = false;
        _gridReceipts.AllowUserToDeleteRows = false;
        _gridReceipts.AllowUserToResizeRows = false;
        _gridReceipts.BackgroundColor = System.Drawing.Color.White;
        _gridReceipts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _gridReceipts.Dock = System.Windows.Forms.DockStyle.Fill;
        _gridReceipts.Location = new System.Drawing.Point(15, 70);
        _gridReceipts.MultiSelect = false;
        _gridReceipts.Name = "_gridReceipts";
        _gridReceipts.ReadOnly = true;
        _gridReceipts.RowHeadersVisible = false;
        _gridReceipts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        _gridReceipts.Size = new System.Drawing.Size(1070, 545);
        _gridReceipts.TabIndex = 1;

        // 
        // _summaryPanel
        // 
        _summaryPanel.Controls.Add(_lblSummary);
        _summaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        _summaryPanel.Location = new System.Drawing.Point(15, 621);
        _summaryPanel.Name = "_summaryPanel";
        _summaryPanel.Size = new System.Drawing.Size(1070, 44);
        _summaryPanel.TabIndex = 2;

        // 
        // _lblSummary
        // 
        _lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
        _lblSummary.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold);
        _lblSummary.ForeColor = System.Drawing.Color.FromArgb(27, 135, 63);
        _lblSummary.Location = new System.Drawing.Point(0, 0);
        _lblSummary.Name = "_lblSummary";
        _lblSummary.Size = new System.Drawing.Size(1070, 44);
        _lblSummary.TabIndex = 0;
        _lblSummary.Text = "الإجمالي: -";
        _lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // 
        // GiftReceiptsForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 19F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1100, 680);
        Controls.Add(_mainLayout);
        Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        KeyPreview = true;
        Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        Name = "GiftReceiptsForm";
        RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "تقرير قسائم ووصولات الهدايا";
        _mainLayout.ResumeLayout(false);
        _filtersPanel.ResumeLayout(false);
        _filtersPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_gridReceipts).EndInit();
        _summaryPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
