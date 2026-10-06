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
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblHeader = new System.Windows.Forms.Label();
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
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(720, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
        this._lblHeader.Size = new System.Drawing.Size(720, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إدارة الفئات العمرية للمرافقين";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _pnlToolbar
        // 
        this._pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlToolbar.Controls.Add(this._btnAdd);
        this._pnlToolbar.Controls.Add(this._btnEdit);
        this._pnlToolbar.Controls.Add(this._btnToggleActive);
        this._pnlToolbar.Controls.Add(this._btnRefresh);
        this._pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlToolbar.Location = new System.Drawing.Point(0, 50);
        this._pnlToolbar.Name = "_pnlToolbar";
        this._pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._pnlToolbar.Size = new System.Drawing.Size(720, 48);
        this._pnlToolbar.TabIndex = 1;
        // 
        // _btnAdd
        // 
        this._btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnAdd.ForeColor = System.Drawing.Color.White;
        this._btnAdd.Location = new System.Drawing.Point(590, 11);
        this._btnAdd.Name = "_btnAdd";
        this._btnAdd.Size = new System.Drawing.Size(106, 28);
        this._btnAdd.TabIndex = 0;
        this._btnAdd.Text = "إضافة جديدة";
        this._btnAdd.UseVisualStyleBackColor = false;
        // 
        // _btnEdit
        // 
        this._btnEdit.Enabled = false;
        this._btnEdit.Location = new System.Drawing.Point(504, 11);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new System.Drawing.Size(80, 28);
        this._btnEdit.TabIndex = 1;
        this._btnEdit.Text = "تعديل";
        this._btnEdit.UseVisualStyleBackColor = true;
        // 
        // _btnToggleActive
        // 
        this._btnToggleActive.Enabled = false;
        this._btnToggleActive.Location = new System.Drawing.Point(398, 11);
        this._btnToggleActive.Name = "_btnToggleActive";
        this._btnToggleActive.Size = new System.Drawing.Size(100, 28);
        this._btnToggleActive.TabIndex = 2;
        this._btnToggleActive.Text = "تعطيل/تفعيل";
        this._btnToggleActive.UseVisualStyleBackColor = true;
        // 
        // _btnRefresh
        // 
        this._btnRefresh.Location = new System.Drawing.Point(312, 11);
        this._btnRefresh.Name = "_btnRefresh";
        this._btnRefresh.Size = new System.Drawing.Size(80, 28);
        this._btnRefresh.TabIndex = 3;
        this._btnRefresh.Text = "تحديث";
        this._btnRefresh.UseVisualStyleBackColor = true;
        // 
        // _gridAgeCategories
        // 
        this._gridAgeCategories.AllowUserToAddRows = false;
        this._gridAgeCategories.AllowUserToDeleteRows = false;
        this._gridAgeCategories.AllowUserToResizeRows = false;
        this._gridAgeCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridAgeCategories.BackgroundColor = System.Drawing.Color.White;
        this._gridAgeCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridAgeCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colName,
            this._colDescription,
            this._colSortOrder,
            this._colStatus});
        this._gridAgeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridAgeCategories.Location = new System.Drawing.Point(0, 98);
        this._gridAgeCategories.MultiSelect = false;
        this._gridAgeCategories.Name = "_gridAgeCategories";
        this._gridAgeCategories.ReadOnly = true;
        this._gridAgeCategories.RowHeadersVisible = false;
        this._gridAgeCategories.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridAgeCategories.Size = new System.Drawing.Size(720, 314);
        this._gridAgeCategories.TabIndex = 2;
        // 
        // _colName
        // 
        this._colName.FillWeight = 30F;
        this._colName.HeaderText = "اسم الفئة";
        this._colName.Name = "_colName";
        this._colName.ReadOnly = true;
        // 
        // _colDescription
        // 
        this._colDescription.FillWeight = 40F;
        this._colDescription.HeaderText = "الوصف";
        this._colDescription.Name = "_colDescription";
        this._colDescription.ReadOnly = true;
        // 
        // _colSortOrder
        // 
        this._colSortOrder.FillWeight = 15F;
        this._colSortOrder.HeaderText = "الترتيب";
        this._colSortOrder.Name = "_colSortOrder";
        this._colSortOrder.ReadOnly = true;
        // 
        // _colStatus
        // 
        this._colStatus.FillWeight = 15F;
        this._colStatus.HeaderText = "الحالة";
        this._colStatus.Name = "_colStatus";
        this._colStatus.ReadOnly = true;
        // 
        // _pnlFooter
        // 
        this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._pnlFooter.Controls.Add(this._lblCount);
        this._pnlFooter.Controls.Add(this._btnClose);
        this._pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlFooter.Location = new System.Drawing.Point(0, 412);
        this._pnlFooter.Name = "_pnlFooter";
        this._pnlFooter.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlFooter.Size = new System.Drawing.Size(720, 42);
        this._pnlFooter.TabIndex = 3;
        // 
        // _lblCount
        // 
        this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblCount.AutoSize = true;
        this._lblCount.Location = new System.Drawing.Point(606, 12);
        this._lblCount.Name = "_lblCount";
        this._lblCount.Size = new System.Drawing.Size(98, 19);
        this._lblCount.TabIndex = 0;
        this._lblCount.Text = "عدد الفئات: 0";
        // 
        // _btnClose
        // 
        this._btnClose.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._btnClose.Location = new System.Drawing.Point(16, 7);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(80, 28);
        this._btnClose.TabIndex = 1;
        this._btnClose.Text = "إغلاق";
        this._btnClose.UseVisualStyleBackColor = true;
        // 
        // AgeCategoriesForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(720, 454);
        this.Controls.Add(this._gridAgeCategories);
        this.Controls.Add(this._pnlFooter);
        this.Controls.Add(this._pnlToolbar);
        this.Controls.Add(this._pnlHeader);
        this.MinimumSize = new System.Drawing.Size(650, 380);
        this.Name = "AgeCategoriesForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إدارة الفئات العمرية للمرافقين";
        this._pnlHeader.ResumeLayout(false);
        this._pnlToolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridAgeCategories)).EndInit();
        this._pnlFooter.ResumeLayout(false);
        this._pnlFooter.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
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
