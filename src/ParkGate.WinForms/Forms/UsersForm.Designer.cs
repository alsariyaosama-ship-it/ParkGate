#nullable disable

namespace ParkGate.WinForms.Forms;

partial class UsersForm
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
        this._btnAddUser = new System.Windows.Forms.Button();
        this._btnChangeRole = new System.Windows.Forms.Button();
        this._btnResetPassword = new System.Windows.Forms.Button();
        this._btnToggleActive = new System.Windows.Forms.Button();
        this._btnRefresh = new System.Windows.Forms.Button();
        this._gridUsers = new System.Windows.Forms.DataGridView();
        this._colUsername = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colRole = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colMustChangePass = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._colLastLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._pnlFooter = new System.Windows.Forms.Panel();
        this._lblCount = new System.Windows.Forms.Label();
        this._btnClose = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._pnlToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this._gridUsers)).BeginInit();
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
        this._pnlHeader.Size = new System.Drawing.Size(850, 56);
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
        this._lblHeader.Size = new System.Drawing.Size(850, 56);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إدارة المستخدمين";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _pnlToolbar
        // 
        this._pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this._pnlToolbar.Controls.Add(this._btnAddUser);
        this._pnlToolbar.Controls.Add(this._btnChangeRole);
        this._pnlToolbar.Controls.Add(this._btnResetPassword);
        this._pnlToolbar.Controls.Add(this._btnToggleActive);
        this._pnlToolbar.Controls.Add(this._btnRefresh);
        this._pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlToolbar.Location = new System.Drawing.Point(0, 56);
        this._pnlToolbar.Name = "_pnlToolbar";
        this._pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this._pnlToolbar.Size = new System.Drawing.Size(850, 54);
        this._pnlToolbar.TabIndex = 1;
        // 
        // _btnAddUser
        // 
        this._btnAddUser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnAddUser.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnAddUser.FlatAppearance.BorderSize = 0;
        this._btnAddUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnAddUser.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnAddUser.ForeColor = System.Drawing.Color.White;
        this._btnAddUser.Location = new System.Drawing.Point(704, 8);
        this._btnAddUser.Name = "_btnAddUser";
        this._btnAddUser.Size = new System.Drawing.Size(130, 36);
        this._btnAddUser.TabIndex = 0;
        this._btnAddUser.Text = "+ إضافة مستخدم";
        this._btnAddUser.UseVisualStyleBackColor = false;
        // 
        // _btnChangeRole
        // 
        this._btnChangeRole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnChangeRole.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnChangeRole.Enabled = false;
        this._btnChangeRole.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnChangeRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnChangeRole.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnChangeRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnChangeRole.Location = new System.Drawing.Point(568, 8);
        this._btnChangeRole.Name = "_btnChangeRole";
        this._btnChangeRole.Size = new System.Drawing.Size(130, 36);
        this._btnChangeRole.TabIndex = 1;
        this._btnChangeRole.Text = "تعديل الصلاحية...";
        this._btnChangeRole.UseVisualStyleBackColor = false;
        // 
        // _btnResetPassword
        // 
        this._btnResetPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._btnResetPassword.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnResetPassword.Enabled = false;
        this._btnResetPassword.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this._btnResetPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnResetPassword.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._btnResetPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this._btnResetPassword.Location = new System.Drawing.Point(398, 8);
        this._btnResetPassword.Name = "_btnResetPassword";
        this._btnResetPassword.Size = new System.Drawing.Size(164, 36);
        this._btnResetPassword.TabIndex = 2;
        this._btnResetPassword.Text = "إعادة تعيين كلمة المرور...";
        this._btnResetPassword.UseVisualStyleBackColor = false;
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
        this._btnToggleActive.Location = new System.Drawing.Point(272, 8);
        this._btnToggleActive.Name = "_btnToggleActive";
        this._btnToggleActive.Size = new System.Drawing.Size(120, 36);
        this._btnToggleActive.TabIndex = 3;
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
        this._btnRefresh.Location = new System.Drawing.Point(176, 8);
        this._btnRefresh.Name = "_btnRefresh";
        this._btnRefresh.Size = new System.Drawing.Size(90, 36);
        this._btnRefresh.TabIndex = 4;
        this._btnRefresh.Text = "تحديث";
        this._btnRefresh.UseVisualStyleBackColor = false;
        // 
        // _gridUsers
        // 
        this._gridUsers.AllowUserToAddRows = false;
        this._gridUsers.AllowUserToDeleteRows = false;
        this._gridUsers.AllowUserToResizeRows = false;
        this._gridUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this._gridUsers.BackgroundColor = System.Drawing.Color.White;
        this._gridUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._gridUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colUsername,
            this._colFullName,
            this._colRole,
            this._colStatus,
            this._colMustChangePass,
            this._colLastLogin});
        this._gridUsers.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridUsers.Location = new System.Drawing.Point(0, 98);
        this._gridUsers.MultiSelect = false;
        this._gridUsers.Name = "_gridUsers";
        this._gridUsers.ReadOnly = true;
        this._gridUsers.RowHeadersVisible = false;
        this._gridUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridUsers.Size = new System.Drawing.Size(850, 392);
        this._gridUsers.TabIndex = 2;
        // 
        // _colUsername
        // 
        this._colUsername.FillWeight = 20F;
        this._colUsername.HeaderText = "اسم المستخدم";
        this._colUsername.Name = "_colUsername";
        this._colUsername.ReadOnly = true;
        // 
        // _colFullName
        // 
        this._colFullName.FillWeight = 25F;
        this._colFullName.HeaderText = "الاسم الكامل";
        this._colFullName.Name = "_colFullName";
        this._colFullName.ReadOnly = true;
        // 
        // _colRole
        // 
        this._colRole.FillWeight = 15F;
        this._colRole.HeaderText = "الدور / الصلاحية";
        this._colRole.Name = "_colRole";
        this._colRole.ReadOnly = true;
        // 
        // _colStatus
        // 
        this._colStatus.FillWeight = 12F;
        this._colStatus.HeaderText = "الحالة";
        this._colStatus.Name = "_colStatus";
        this._colStatus.ReadOnly = true;
        // 
        // _colMustChangePass
        // 
        this._colMustChangePass.FillWeight = 13F;
        this._colMustChangePass.HeaderText = "تغيير إلزامي";
        this._colMustChangePass.Name = "_colMustChangePass";
        this._colMustChangePass.ReadOnly = true;
        // 
        // _colLastLogin
        // 
        this._colLastLogin.FillWeight = 20F;
        this._colLastLogin.HeaderText = "آخر تسجيل دخول";
        this._colLastLogin.Name = "_colLastLogin";
        this._colLastLogin.ReadOnly = true;
        // 
        // _pnlFooter
        // 
        this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this._pnlFooter.Controls.Add(this._lblCount);
        this._pnlFooter.Controls.Add(this._btnClose);
        this._pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlFooter.Location = new System.Drawing.Point(0, 484);
        this._pnlFooter.Name = "_pnlFooter";
        this._pnlFooter.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlFooter.Size = new System.Drawing.Size(850, 48);
        this._pnlFooter.TabIndex = 3;
        // 
        // _lblCount
        // 
        this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this._lblCount.AutoSize = true;
        this._lblCount.Location = new System.Drawing.Point(718, 14);
        this._lblCount.Name = "_lblCount";
        this._lblCount.Size = new System.Drawing.Size(120, 19);
        this._lblCount.TabIndex = 0;
        this._lblCount.Text = "عدد المستخدمين: 0";
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
        // UsersForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(850, 532);
        this.Controls.Add(this._gridUsers);
        this.Controls.Add(this._pnlFooter);
        this.Controls.Add(this._pnlToolbar);
        this.Controls.Add(this._pnlHeader);
        this.MinimumSize = new System.Drawing.Size(750, 420);
        this.Name = "UsersForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إدارة المستخدمين";
        this._pnlHeader.ResumeLayout(false);
        this._pnlToolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this._gridUsers)).EndInit();
        this._pnlFooter.ResumeLayout(false);
        this._pnlFooter.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.FlowLayoutPanel _pnlToolbar;
    private System.Windows.Forms.Button _btnAddUser;
    private System.Windows.Forms.Button _btnChangeRole;
    private System.Windows.Forms.Button _btnResetPassword;
    private System.Windows.Forms.Button _btnToggleActive;
    private System.Windows.Forms.Button _btnRefresh;
    private System.Windows.Forms.DataGridView _gridUsers;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colUsername;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colFullName;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colRole;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colMustChangePass;
    private System.Windows.Forms.DataGridViewTextBoxColumn _colLastLogin;
    private System.Windows.Forms.Panel _pnlFooter;
    private System.Windows.Forms.Label _lblCount;
    private System.Windows.Forms.Button _btnClose;
}
