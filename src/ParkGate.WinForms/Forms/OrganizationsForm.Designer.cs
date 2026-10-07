#nullable disable

namespace ParkGate.WinForms.Forms;

partial class OrganizationsForm
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
        this._chkActiveOnly = new System.Windows.Forms.CheckBox();
        this._gridOrganizations = new System.Windows.Forms.DataGridView();
        this._colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colCooldown = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._pnlFooter = new System.Windows.Forms.Panel();
        this._lblCount = new System.Windows.Forms.Label();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._pnlToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridOrganizations)).BeginInit();
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
        this._pnlHeader.Size = new System.Drawing.Size(784, 50);
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
        this._lblHeader.Size = new System.Drawing.Size(784, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إدارة الجهات والمنظمات";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _pnlToolbar
        // 
        this._pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlToolbar.Controls.Add(this._btnAdd);
        this._pnlToolbar.Controls.Add(this._btnEdit);
        this._pnlToolbar.Controls.Add(this._btnToggleActive);
        this._pnlToolbar.Controls.Add(this._btnRefresh);
        this._pnlToolbar.Controls.Add(this._chkActiveOnly);
        this._pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlToolbar.Location = new System.Drawing.Point(0, 50);
        this._pnlToolbar.Name = "_pnlToolbar";
        this._pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._pnlToolbar.Size = new System.Drawing.Size(784, 48);
        this._pnlToolbar.TabIndex = 1;
        // 
        // _btnAdd
        // 
        this._btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnAdd.ForeColor = System.Drawing.Color.White;
        this._btnAdd.Location = new System.Drawing.Point(654, 11);
        this._btnAdd.Name = "_btnAdd";
        this._btnAdd.Size = new System.Drawing.Size(106, 28);
        this._btnAdd.TabIndex = 0;
        this._btnAdd.Text = "إضافة جديدة";
        this._btnAdd.UseVisualStyleBackColor = false;
        // 
        // _btnEdit
        // 
        this._btnEdit.Enabled = false;
        this._btnEdit.Location = new System.Drawing.Point(568, 11);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new System.Drawing.Size(80, 28);
        this._btnEdit.TabIndex = 1;
        this._btnEdit.Text = "تعديل";
        this._btnEdit.UseVisualStyleBackColor = true;
        // 
        // _btnToggleActive
        // 
        this._btnToggleActive.Enabled = false;
        this._btnToggleActive.Location = new System.Drawing.Point(462, 11);
        this._btnToggleActive.Name = "_btnToggleActive";
        this._btnToggleActive.Size = new System.Drawing.Size(100, 28);
        this._btnToggleActive.TabIndex = 2;
        this._btnToggleActive.Text = "تعطيل/تفعيل";
        this._btnToggleActive.UseVisualStyleBackColor = true;
        // 
        // _btnRefresh
        // 
        this._btnRefresh.Location = new System.Drawing.Point(376, 11);
        this._btnRefresh.Name = "_btnRefresh";
        this._btnRefresh.Size = new System.Drawing.Size(80, 28);
        this._btnRefresh.TabIndex = 3;
        this._btnRefresh.Text = "تحديث";
        this._btnRefresh.UseVisualStyleBackColor = true;
        // 
        // _chkActiveOnly
        // 
        this._chkActiveOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._chkActiveOnly.AutoSize = true;
        this._chkActiveOnly.Location = new System.Drawing.Point(232, 13);
        this._chkActiveOnly.Margin = new System.Windows.Forms.Padding(16, 3, 3, 3);
        this._chkActiveOnly.Name = "_chkActiveOnly";
        this._chkActiveOnly.Size = new System.Drawing.Size(125, 23);
        this._chkActiveOnly.TabIndex = 4;
        this._chkActiveOnly.Text = "عرض النشطة فقط";
        this._chkActiveOnly.UseVisualStyleBackColor = true;
        // 
        // _gridOrganizations
        // 
        this._gridOrganizations.AllowUserToAddRows = false;
        this._gridOrganizations.AllowUserToDeleteRows = false;
        this._gridOrganizations.AllowUserToResizeRows = false;
        this._gridOrganizations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridOrganizations.BackgroundColor = System.Drawing.Color.White;
        this._gridOrganizations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridOrganizations.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colCode,
            this._colName,
            this._colCooldown,
            this._colDefault,
            this._colStatus});
        this._gridOrganizations.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridOrganizations.Location = new System.Drawing.Point(0, 98);
        this._gridOrganizations.MultiSelect = false;
        this._gridOrganizations.Name = "_gridOrganizations";
        this._gridOrganizations.ReadOnly = true;
        this._gridOrganizations.RowHeadersVisible = false;
        this._gridOrganizations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridOrganizations.Size = new System.Drawing.Size(784, 372);
        this._gridOrganizations.TabIndex = 2;
        // 
        // _colCode
        // 
        this._colCode.FillWeight = 25F;
        this._colCode.HeaderText = "رمز الجهة";
        this._colCode.Name = "_colCode";
        this._colCode.ReadOnly = true;
        // 
        // _colName
        // 
        this._colName.FillWeight = 40F;
        this._colName.HeaderText = "اسم الجهة";
        this._colName.Name = "_colName";
        this._colName.ReadOnly = true;
        // 
        // _colCooldown
        // 
        this._colCooldown.FillWeight = 20F;
        this._colCooldown.HeaderText = "فترة السماح";
        this._colCooldown.Name = "_colCooldown";
        this._colCooldown.ReadOnly = true;
        // 
        // _colDefault
        // 
        this._colDefault.FillWeight = 15F;
        this._colDefault.HeaderText = "افتراضية";
        this._colDefault.Name = "_colDefault";
        this._colDefault.ReadOnly = true;
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
        this._pnlFooter.Location = new System.Drawing.Point(0, 470);
        this._pnlFooter.Name = "_pnlFooter";
        this._pnlFooter.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlFooter.Size = new System.Drawing.Size(784, 42);
        this._pnlFooter.TabIndex = 3;
        // 
        // _lblCount
        // 
        this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblCount.AutoSize = true;
        this._lblCount.Location = new System.Drawing.Point(676, 12);
        this._lblCount.Name = "_lblCount";
        this._lblCount.Size = new System.Drawing.Size(92, 19);
        this._lblCount.TabIndex = 0;
        this._lblCount.Text = "عدد الجهات: 0";
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
        // OrganizationsForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 512);
        this.Controls.Add(this._gridOrganizations);
        this.Controls.Add(this._pnlFooter);
        this.Controls.Add(this._pnlToolbar);
        this.Controls.Add(this._pnlHeader);
        this.MinimumSize = new System.Drawing.Size(700, 400);
        this.Name = "OrganizationsForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إدارة الجهات والمنظمات";
        this._pnlHeader.ResumeLayout(false);
        this._pnlToolbar.ResumeLayout(false);
        this._pnlToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridOrganizations)).EndInit();
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
    private System.Windows.Forms.CheckBox _chkActiveOnly;
    private System.Windows.Forms.DataGridView _gridOrganizations;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCode;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colCooldown;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colDefault;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colStatus;
    private System.Windows.Forms.Panel _pnlFooter;
    private System.Windows.Forms.Label _lblCount;
    private System.Windows.Forms.Button _btnClose;
}
