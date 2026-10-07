#nullable disable

namespace ParkGate.WinForms.Forms;

partial class AgeCategoriesForm
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
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
        this._lblHeaderSubtitle = new System.Windows.Forms.Label();
        this._pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
        this._btnAdd = new System.Windows.Forms.Button();
        this._btnEdit = new System.Windows.Forms.Button();
        this._btnToggleActive = new System.Windows.Forms.Button();
        this._btnRefresh = new System.Windows.Forms.Button();
        this._gridAgeCategories = new System.Windows.Forms.DataGridView();
        this._colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colSortOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._pnlFooter = new System.Windows.Forms.Panel();
        this._lblCount = new System.Windows.Forms.Label();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._pnlToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridAgeCategories)).BeginInit();
        this._pnlFooter.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblHeaderSubtitle);
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Margin = new System.Windows.Forms.Padding(0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(780, 72);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblHeader.Font = new System.Drawing.Font("Cairo", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(16, 10);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Size = new System.Drawing.Size(748, 28);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إدارة الفئات العمرية للمرافقين";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _lblHeaderSubtitle
        // 
        this._lblHeaderSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblHeaderSubtitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
        this._lblHeaderSubtitle.Location = new System.Drawing.Point(16, 40);
        this._lblHeaderSubtitle.Name = "_lblHeaderSubtitle";
        this._lblHeaderSubtitle.Size = new System.Drawing.Size(748, 22);
        this._lblHeaderSubtitle.TabIndex = 1;
        this._lblHeaderSubtitle.Text = "تحديد الفئات العمرية للمرافقين وضبط تسلسل الظهور وحالة التفعيل (FR-ADM-02)";
        this._lblHeaderSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _pnlToolbar
        // 
        this._pnlToolbar.BackColor = System.Drawing.Color.White;
        this._pnlToolbar.Controls.Add(this._btnAdd);
        this._pnlToolbar.Controls.Add(this._btnEdit);
        this._pnlToolbar.Controls.Add(this._btnToggleActive);
        this._pnlToolbar.Controls.Add(this._btnRefresh);
        this._pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlToolbar.Location = new System.Drawing.Point(0, 72);
        this._pnlToolbar.Margin = new System.Windows.Forms.Padding(0);
        this._pnlToolbar.Name = "_pnlToolbar";
        this._pnlToolbar.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
        this._pnlToolbar.Size = new System.Drawing.Size(780, 56);
        this._pnlToolbar.TabIndex = 1;
        // 
        // _btnAdd
        // 
        this._btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnAdd.FlatAppearance.BorderSize = 0;
        this._btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnAdd.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnAdd.ForeColor = System.Drawing.Color.White;
        this._btnAdd.Location = new System.Drawing.Point(608, 10);
        this._btnAdd.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        this._btnAdd.Name = "_btnAdd";
        this._btnAdd.Size = new System.Drawing.Size(140, 36);
        this._btnAdd.TabIndex = 0;
        this._btnAdd.Text = "+ إضافة جديدة";
        this._btnAdd.UseVisualStyleBackColor = false;
        // 
        // _btnEdit
        // 
        this._btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnEdit.Enabled = false;
        this._btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnEdit.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnEdit.Location = new System.Drawing.Point(498, 10);
        this._btnEdit.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new System.Drawing.Size(102, 36);
        this._btnEdit.TabIndex = 1;
        this._btnEdit.Text = "تعديل";
        this._btnEdit.UseVisualStyleBackColor = false;
        // 
        // _btnToggleActive
        // 
        this._btnToggleActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnToggleActive.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnToggleActive.Enabled = false;
        this._btnToggleActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnToggleActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnToggleActive.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnToggleActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnToggleActive.Location = new System.Drawing.Point(378, 10);
        this._btnToggleActive.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        this._btnToggleActive.Name = "_btnToggleActive";
        this._btnToggleActive.Size = new System.Drawing.Size(112, 36);
        this._btnToggleActive.TabIndex = 2;
        this._btnToggleActive.Text = "تعطيل/تفعيل";
        this._btnToggleActive.UseVisualStyleBackColor = false;
        // 
        // _btnRefresh
        // 
        this._btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnRefresh.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        this._btnRefresh.Location = new System.Drawing.Point(278, 10);
        this._btnRefresh.Margin = new System.Windows.Forms.Padding(0);
        this._btnRefresh.Name = "_btnRefresh";
        this._btnRefresh.Size = new System.Drawing.Size(92, 36);
        this._btnRefresh.TabIndex = 3;
        this._btnRefresh.Text = "تحديث";
        this._btnRefresh.UseVisualStyleBackColor = false;
        // 
        // _gridAgeCategories
        // 
        this._gridAgeCategories.AllowUserToAddRows = false;
        this._gridAgeCategories.AllowUserToDeleteRows = false;
        this._gridAgeCategories.AllowUserToResizeRows = false;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._gridAgeCategories.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
        this._gridAgeCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridAgeCategories.BackgroundColor = System.Drawing.Color.White;
        this._gridAgeCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridAgeCategories.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
        this._gridAgeCategories.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this._gridAgeCategories.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
        this._gridAgeCategories.ColumnHeadersHeight = 44;
        this._gridAgeCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this._gridAgeCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colName,
            this._colDescription,
            this._colSortOrder,
            this._colStatus});
        dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
        dataGridViewCellStyle3.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
        dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this._gridAgeCategories.DefaultCellStyle = dataGridViewCellStyle3;
        this._gridAgeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridAgeCategories.EnableHeadersVisualStyles = false;
        this._gridAgeCategories.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
        this._gridAgeCategories.Location = new System.Drawing.Point(0, 128);
        this._gridAgeCategories.MultiSelect = false;
        this._gridAgeCategories.Name = "_gridAgeCategories";
        this._gridAgeCategories.ReadOnly = true;
        this._gridAgeCategories.RowHeadersVisible = false;
        this._gridAgeCategories.RowTemplate.Height = 40;
        this._gridAgeCategories.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridAgeCategories.Size = new System.Drawing.Size(780, 344);
        this._gridAgeCategories.TabIndex = 2;
        // 
        // _colName
        // 
        this._colName.FillWeight = 30F;
        this._colName.HeaderText = "اسم الفئة";
        this._colName.MinimumWidth = 140;
        this._colName.Name = "_colName";
        this._colName.ReadOnly = true;
        // 
        // _colDescription
        // 
        this._colDescription.FillWeight = 40F;
        this._colDescription.HeaderText = "الوصف";
        this._colDescription.MinimumWidth = 180;
        this._colDescription.Name = "_colDescription";
        this._colDescription.ReadOnly = true;
        // 
        // _colSortOrder
        // 
        dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this._colSortOrder.DefaultCellStyle = dataGridViewCellStyle4;
        this._colSortOrder.FillWeight = 15F;
        this._colSortOrder.HeaderText = "الترتيب";
        this._colSortOrder.MinimumWidth = 80;
        this._colSortOrder.Name = "_colSortOrder";
        this._colSortOrder.ReadOnly = true;
        // 
        // _colStatus
        // 
        dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this._colStatus.DefaultCellStyle = dataGridViewCellStyle5;
        this._colStatus.FillWeight = 15F;
        this._colStatus.HeaderText = "الحالة";
        this._colStatus.MinimumWidth = 90;
        this._colStatus.Name = "_colStatus";
        this._colStatus.ReadOnly = true;
        // 
        // _pnlFooter
        // 
        this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlFooter.Controls.Add(this._lblCount);
        this._pnlFooter.Controls.Add(this._btnClose);
        this._pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlFooter.Location = new System.Drawing.Point(0, 472);
        this._pnlFooter.Margin = new System.Windows.Forms.Padding(0);
        this._pnlFooter.Name = "_pnlFooter";
        this._pnlFooter.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlFooter.Size = new System.Drawing.Size(780, 48);
        this._pnlFooter.TabIndex = 3;
        // 
        // _lblCount
        // 
        this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblCount.AutoSize = true;
        this._lblCount.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        this._lblCount.Location = new System.Drawing.Point(620, 14);
        this._lblCount.Name = "_lblCount";
        this._lblCount.Size = new System.Drawing.Size(144, 20);
        this._lblCount.TabIndex = 0;
        this._lblCount.Text = "إجمالي الفئات: 0";
        // 
        // _btnClose
        // 
        this._btnClose.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._btnClose.BackColor = System.Drawing.Color.White;
        this._btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClose.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this._btnClose.Location = new System.Drawing.Point(16, 8);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(96, 32);
        this._btnClose.TabIndex = 1;
        this._btnClose.Text = "إغلاق (Esc)";
        this._btnClose.UseVisualStyleBackColor = false;
        // 
        // AgeCategoriesForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this._btnClose;
        this.ClientSize = new System.Drawing.Size(780, 520);
        this.Controls.Add(this._gridAgeCategories);
        this.Controls.Add(this._pnlFooter);
        this.Controls.Add(this._pnlToolbar);
        this.Controls.Add(this._pnlHeader);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MinimumSize = new System.Drawing.Size(700, 420);
        this.Name = "AgeCategoriesForm";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إدارة الفئات العمرية للمرافقين - ParkGate";
        this._pnlHeader.ResumeLayout(false);
        this._pnlToolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridAgeCategories)).EndInit();
        this._pnlFooter.ResumeLayout(false);
        this._pnlFooter.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.Label _lblHeaderSubtitle;
    private System.Windows.Forms.FlowLayoutPanel _pnlToolbar;
    private System.Windows.Forms.Button _btnAdd;
    private System.Windows.Forms.Button _btnEdit;
    private System.Windows.Forms.Button _btnToggleActive;
    private System.Windows.Forms.Button _btnRefresh;
    private System.Windows.Forms.DataGridView _gridAgeCategories;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colDescription;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colSortOrder;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colStatus;
    private System.Windows.Forms.Panel _pnlFooter;
    private System.Windows.Forms.Label _lblCount;
    private System.Windows.Forms.Button _btnClose;
}
