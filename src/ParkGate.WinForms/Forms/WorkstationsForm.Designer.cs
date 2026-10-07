#nullable disable

namespace ParkGate.WinForms.Forms;

partial class WorkstationsForm
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
        this._btnEdit = new System.Windows.Forms.Button();
        this._btnToggleActive = new System.Windows.Forms.Button();
        this._btnRefresh = new System.Windows.Forms.Button();
        this._gridWorkstations = new System.Windows.Forms.DataGridView();
        this._colMachineName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colLastSeen = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._pnlFooter = new System.Windows.Forms.Panel();
        this._lblCount = new System.Windows.Forms.Label();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._pnlToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridWorkstations)).BeginInit();
        this._pnlFooter.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(784, 56);
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
        this._lblHeader.Size = new System.Drawing.Size(784, 56);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إدارة محطات العمل ونقاط الدخول";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _pnlToolbar
        // 
        this._pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlToolbar.Controls.Add(this._btnEdit);
        this._pnlToolbar.Controls.Add(this._btnToggleActive);
        this._pnlToolbar.Controls.Add(this._btnRefresh);
        this._pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlToolbar.Location = new System.Drawing.Point(0, 56);
        this._pnlToolbar.Name = "_pnlToolbar";
        this._pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._pnlToolbar.Size = new System.Drawing.Size(784, 54);
        this._pnlToolbar.TabIndex = 1;
        // 
        // _btnEdit
        // 
        this._btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnEdit.Enabled = false;
        this._btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnEdit.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnEdit.Location = new System.Drawing.Point(670, 8);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new System.Drawing.Size(90, 36);
        this._btnEdit.TabIndex = 0;
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
        this._btnToggleActive.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnToggleActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnToggleActive.Location = new System.Drawing.Point(544, 8);
        this._btnToggleActive.Name = "_btnToggleActive";
        this._btnToggleActive.Size = new System.Drawing.Size(120, 36);
        this._btnToggleActive.TabIndex = 1;
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
        this._btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnRefresh.Location = new System.Drawing.Point(448, 8);
        this._btnRefresh.Name = "_btnRefresh";
        this._btnRefresh.Size = new System.Drawing.Size(90, 36);
        this._btnRefresh.TabIndex = 2;
        this._btnRefresh.Text = "تحديث";
        this._btnRefresh.UseVisualStyleBackColor = false;
        // 
        // _gridWorkstations
        // 
        this._gridWorkstations.AllowUserToAddRows = false;
        this._gridWorkstations.AllowUserToDeleteRows = false;
        this._gridWorkstations.AllowUserToResizeRows = false;
        this._gridWorkstations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridWorkstations.BackgroundColor = System.Drawing.Color.White;
        this._gridWorkstations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridWorkstations.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colMachineName,
            this._colDisplayName,
            this._colType,
            this._colStatus,
            this._colLastSeen});
        this._gridWorkstations.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridWorkstations.Location = new System.Drawing.Point(0, 98);
        this._gridWorkstations.MultiSelect = false;
        this._gridWorkstations.Name = "_gridWorkstations";
        this._gridWorkstations.ReadOnly = true;
        this._gridWorkstations.RowHeadersVisible = false;
        this._gridWorkstations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridWorkstations.Size = new System.Drawing.Size(784, 372);
        this._gridWorkstations.TabIndex = 2;
        // 
        // _colMachineName
        // 
        this._colMachineName.FillWeight = 25F;
        this._colMachineName.HeaderText = "اسم الجهاز";
        this._colMachineName.Name = "_colMachineName";
        this._colMachineName.ReadOnly = true;
        // 
        // _colDisplayName
        // 
        this._colDisplayName.FillWeight = 30F;
        this._colDisplayName.HeaderText = "اسم المحطة";
        this._colDisplayName.Name = "_colDisplayName";
        this._colDisplayName.ReadOnly = true;
        // 
        // _colType
        // 
        this._colType.FillWeight = 20F;
        this._colType.HeaderText = "النوع";
        this._colType.Name = "_colType";
        this._colType.ReadOnly = true;
        // 
        // _colStatus
        // 
        this._colStatus.FillWeight = 15F;
        this._colStatus.HeaderText = "الحالة";
        this._colStatus.Name = "_colStatus";
        this._colStatus.ReadOnly = true;
        // 
        // _colLastSeen
        // 
        this._colLastSeen.FillWeight = 25F;
        this._colLastSeen.HeaderText = "آخر ظهور";
        this._colLastSeen.Name = "_colLastSeen";
        this._colLastSeen.ReadOnly = true;
        // 
        // _pnlFooter
        // 
        this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._pnlFooter.Controls.Add(this._lblCount);
        this._pnlFooter.Controls.Add(this._btnClose);
        this._pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlFooter.Location = new System.Drawing.Point(0, 464);
        this._pnlFooter.Name = "_pnlFooter";
        this._pnlFooter.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlFooter.Size = new System.Drawing.Size(784, 48);
        this._pnlFooter.TabIndex = 3;
        // 
        // _lblCount
        // 
        this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblCount.AutoSize = true;
        this._lblCount.Location = new System.Drawing.Point(676, 14);
        this._lblCount.Name = "_lblCount";
        this._lblCount.Size = new System.Drawing.Size(102, 19);
        this._lblCount.TabIndex = 0;
        this._lblCount.Text = "عدد المحطات: 0";
        // 
        // _btnClose
        // 
        this._btnClose.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnClose.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnClose.Location = new System.Drawing.Point(16, 6);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new System.Drawing.Size(90, 36);
        this._btnClose.TabIndex = 1;
        this._btnClose.Text = "إغلاق";
        this._btnClose.UseVisualStyleBackColor = false;
        // 
        // WorkstationsForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 512);
        this.Controls.Add(this._gridWorkstations);
        this.Controls.Add(this._pnlFooter);
        this.Controls.Add(this._pnlToolbar);
        this.Controls.Add(this._pnlHeader);
        this.MinimumSize = new System.Drawing.Size(700, 400);
        this.Name = "WorkstationsForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إدارة محطات العمل ونقاط الدخول";
        this._pnlHeader.ResumeLayout(false);
        this._pnlToolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridWorkstations)).EndInit();
        this._pnlFooter.ResumeLayout(false);
        this._pnlFooter.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.FlowLayoutPanel _pnlToolbar;
    private System.Windows.Forms.Button _btnEdit;
    private System.Windows.Forms.Button _btnToggleActive;
    private System.Windows.Forms.Button _btnRefresh;
    private System.Windows.Forms.DataGridView _gridWorkstations;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colMachineName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colDisplayName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colType;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colLastSeen;
    private System.Windows.Forms.Panel _pnlFooter;
    private System.Windows.Forms.Label _lblCount;
    private System.Windows.Forms.Button _btnClose;
}
