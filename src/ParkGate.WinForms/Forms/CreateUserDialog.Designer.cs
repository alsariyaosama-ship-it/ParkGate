#nullable disable

namespace ParkGate.WinForms.Forms;

partial class CreateUserDialog
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
        this._tblLayout = new System.Windows.Forms.TableLayoutPanel();
        this._lblUserName = new System.Windows.Forms.Label();
        this._txtUserName = new System.Windows.Forms.TextBox();
        this._lblFullName = new System.Windows.Forms.Label();
        this._txtFullName = new System.Windows.Forms.TextBox();
        this._lblPassword = new System.Windows.Forms.Label();
        this._txtPassword = new System.Windows.Forms.TextBox();
        this._lblConfirmPassword = new System.Windows.Forms.Label();
        this._txtConfirmPassword = new System.Windows.Forms.TextBox();
        this._lblRole = new System.Windows.Forms.Label();
        this._cmbRole = new System.Windows.Forms.ComboBox();
        this._pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._tblLayout.SuspendLayout();
        this._pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
        this._pnlHeader.Controls.Add(this._lblHeader);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(460, 50);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblHeader
        // 
        this._lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblHeader.ForeColor = System.Drawing.Color.White;
        this._lblHeader.Location = new System.Drawing.Point(0, 0);
        this._lblHeader.Name = "_lblHeader";
        this._lblHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
        this._lblHeader.Size = new System.Drawing.Size(460, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "إضافة مستخدم جديد";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblUserName, 0, 0);
        this._tblLayout.Controls.Add(this._txtUserName, 1, 0);
        this._tblLayout.Controls.Add(this._lblFullName, 0, 1);
        this._tblLayout.Controls.Add(this._txtFullName, 1, 1);
        this._tblLayout.Controls.Add(this._lblPassword, 0, 2);
        this._tblLayout.Controls.Add(this._txtPassword, 1, 2);
        this._tblLayout.Controls.Add(this._lblConfirmPassword, 0, 3);
        this._tblLayout.Controls.Add(this._txtConfirmPassword, 1, 3);
        this._tblLayout.Controls.Add(this._lblRole, 0, 4);
        this._tblLayout.Controls.Add(this._cmbRole, 1, 4);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16);
        this._tblLayout.RowCount = 5;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.Size = new System.Drawing.Size(460, 220);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblUserName
        // 
        this._lblUserName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblUserName.AutoSize = true;
        this._lblUserName.Location = new System.Drawing.Point(320, 24);
        this._lblUserName.Name = "_lblUserName";
        this._lblUserName.Size = new System.Drawing.Size(95, 19);
        this._lblUserName.TabIndex = 0;
        this._lblUserName.Text = "اسم المستخدم:";
        // 
        // _txtUserName
        // 
        this._txtUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtUserName.Location = new System.Drawing.Point(19, 21);
        this._txtUserName.MaxLength = 50;
        this._txtUserName.Name = "_txtUserName";
        this._txtUserName.Size = new System.Drawing.Size(262, 25);
        this._txtUserName.TabIndex = 1;
        // 
        // _lblFullName
        // 
        this._lblFullName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblFullName.AutoSize = true;
        this._lblFullName.Location = new System.Drawing.Point(320, 60);
        this._lblFullName.Name = "_lblFullName";
        this._lblFullName.Size = new System.Drawing.Size(81, 19);
        this._lblFullName.TabIndex = 2;
        this._lblFullName.Text = "الاسم الكامل:";
        // 
        // _txtFullName
        // 
        this._txtFullName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtFullName.Location = new System.Drawing.Point(19, 57);
        this._txtFullName.MaxLength = 100;
        this._txtFullName.Name = "_txtFullName";
        this._txtFullName.Size = new System.Drawing.Size(262, 25);
        this._txtFullName.TabIndex = 3;
        // 
        // _lblPassword
        // 
        this._lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblPassword.AutoSize = true;
        this._lblPassword.Location = new System.Drawing.Point(320, 96);
        this._lblPassword.Name = "_lblPassword";
        this._lblPassword.Size = new System.Drawing.Size(73, 19);
        this._lblPassword.TabIndex = 4;
        this._lblPassword.Text = "كلمة المرور:";
        // 
        // _txtPassword
        // 
        this._txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtPassword.Location = new System.Drawing.Point(19, 93);
        this._txtPassword.MaxLength = 100;
        this._txtPassword.Name = "_txtPassword";
        this._txtPassword.PasswordChar = '●';
        this._txtPassword.Size = new System.Drawing.Size(262, 25);
        this._txtPassword.TabIndex = 5;
        // 
        // _lblConfirmPassword
        // 
        this._lblConfirmPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblConfirmPassword.AutoSize = true;
        this._lblConfirmPassword.Location = new System.Drawing.Point(320, 132);
        this._lblConfirmPassword.Name = "_lblConfirmPassword";
        this._lblConfirmPassword.Size = new System.Drawing.Size(107, 19);
        this._lblConfirmPassword.TabIndex = 6;
        this._lblConfirmPassword.Text = "تأكيد كلمة المرور:";
        // 
        // _txtConfirmPassword
        // 
        this._txtConfirmPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._txtConfirmPassword.Location = new System.Drawing.Point(19, 129);
        this._txtConfirmPassword.MaxLength = 100;
        this._txtConfirmPassword.Name = "_txtConfirmPassword";
        this._txtConfirmPassword.PasswordChar = '●';
        this._txtConfirmPassword.Size = new System.Drawing.Size(262, 25);
        this._txtConfirmPassword.TabIndex = 7;
        // 
        // _lblRole
        // 
        this._lblRole.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRole.AutoSize = true;
        this._lblRole.Location = new System.Drawing.Point(320, 168);
        this._lblRole.Name = "_lblRole";
        this._lblRole.Size = new System.Drawing.Size(109, 19);
        this._lblRole.TabIndex = 8;
        this._lblRole.Text = "الدور / الصلاحية:";
        // 
        // _cmbRole
        // 
        this._cmbRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbRole.FormattingEnabled = true;
        this._cmbRole.Location = new System.Drawing.Point(19, 165);
        this._cmbRole.Name = "_cmbRole";
        this._cmbRole.Size = new System.Drawing.Size(262, 25);
        this._cmbRole.TabIndex = 9;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 270);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(460, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(328, 11);
        this._btnSave.Name = "_btnSave";
        this._btnSave.Size = new System.Drawing.Size(100, 32);
        this._btnSave.TabIndex = 0;
        this._btnSave.Text = "حفظ";
        this._btnSave.UseVisualStyleBackColor = false;
        // 
        // _btnCancel
        // 
        this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnCancel.Location = new System.Drawing.Point(222, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // CreateUserDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(460, 320);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CreateUserDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "إضافة مستخدم جديد";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblUserName;
    private System.Windows.Forms.TextBox _txtUserName;
    private System.Windows.Forms.Label _lblFullName;
    private System.Windows.Forms.TextBox _txtFullName;
    private System.Windows.Forms.Label _lblPassword;
    private System.Windows.Forms.TextBox _txtPassword;
    private System.Windows.Forms.Label _lblConfirmPassword;
    private System.Windows.Forms.TextBox _txtConfirmPassword;
    private System.Windows.Forms.Label _lblRole;
    private System.Windows.Forms.ComboBox _cmbRole;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
