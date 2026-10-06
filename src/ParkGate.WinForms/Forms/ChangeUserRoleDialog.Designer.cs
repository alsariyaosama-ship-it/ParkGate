#nullable disable

namespace ParkGate.WinForms.Forms;

partial class ChangeUserRoleDialog
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
        this._lblUserInfoPrompt = new System.Windows.Forms.Label();
        this._lblUserInfo = new System.Windows.Forms.Label();
        this._lblRolePrompt = new System.Windows.Forms.Label();
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
        this._pnlHeader.Size = new System.Drawing.Size(420, 50);
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
        this._lblHeader.Size = new System.Drawing.Size(420, 50);
        this._lblHeader.TabIndex = 0;
        this._lblHeader.Text = "تعديل صلاحية المستخدم";
        this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _tblLayout
        // 
        this._tblLayout.ColumnCount = 2;
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this._tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblLayout.Controls.Add(this._lblUserInfoPrompt, 0, 0);
        this._tblLayout.Controls.Add(this._lblUserInfo, 1, 0);
        this._tblLayout.Controls.Add(this._lblRolePrompt, 0, 1);
        this._tblLayout.Controls.Add(this._cmbRole, 1, 1);
        this._tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblLayout.Location = new System.Drawing.Point(0, 50);
        this._tblLayout.Name = "_tblLayout";
        this._tblLayout.Padding = new System.Windows.Forms.Padding(16);
        this._tblLayout.RowCount = 2;
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this._tblLayout.Size = new System.Drawing.Size(420, 100);
        this._tblLayout.TabIndex = 1;
        // 
        // _lblUserInfoPrompt
        // 
        this._lblUserInfoPrompt.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblUserInfoPrompt.AutoSize = true;
        this._lblUserInfoPrompt.Location = new System.Drawing.Point(313, 24);
        this._lblUserInfoPrompt.Name = "_lblUserInfoPrompt";
        this._lblUserInfoPrompt.Size = new System.Drawing.Size(71, 19);
        this._lblUserInfoPrompt.TabIndex = 0;
        this._lblUserInfoPrompt.Text = "المستخدم:";
        // 
        // _lblUserInfo
        // 
        this._lblUserInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblUserInfo.AutoSize = true;
        this._lblUserInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblUserInfo.Location = new System.Drawing.Point(220, 24);
        this._lblUserInfo.Name = "_lblUserInfo";
        this._lblUserInfo.Size = new System.Drawing.Size(51, 19);
        this._lblUserInfo.TabIndex = 1;
        this._lblUserInfo.Text = "اسم -";
        // 
        // _lblRolePrompt
        // 
        this._lblRolePrompt.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._lblRolePrompt.AutoSize = true;
        this._lblRolePrompt.Location = new System.Drawing.Point(279, 60);
        this._lblRolePrompt.Name = "_lblRolePrompt";
        this._lblRolePrompt.Size = new System.Drawing.Size(105, 19);
        this._lblRolePrompt.TabIndex = 2;
        this._lblRolePrompt.Text = "الدور الجديد:";
        // 
        // _cmbRole
        // 
        this._cmbRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this._cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbRole.FormattingEnabled = true;
        this._cmbRole.Location = new System.Drawing.Point(19, 57);
        this._cmbRole.Name = "_cmbRole";
        this._cmbRole.Size = new System.Drawing.Size(252, 25);
        this._cmbRole.TabIndex = 3;
        // 
        // _pnlButtons
        // 
        this._pnlButtons.Controls.Add(this._btnSave);
        this._pnlButtons.Controls.Add(this._btnCancel);
        this._pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._pnlButtons.Location = new System.Drawing.Point(0, 150);
        this._pnlButtons.Name = "_pnlButtons";
        this._pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
        this._pnlButtons.Size = new System.Drawing.Size(420, 50);
        this._pnlButtons.TabIndex = 2;
        // 
        // _btnSave
        // 
        this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._btnSave.ForeColor = System.Drawing.Color.White;
        this._btnSave.Location = new System.Drawing.Point(288, 11);
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
        this._btnCancel.Location = new System.Drawing.Point(182, 11);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(100, 32);
        this._btnCancel.TabIndex = 1;
        this._btnCancel.Text = "إلغاء";
        this._btnCancel.UseVisualStyleBackColor = false;
        // 
        // ChangeUserRoleDialog
        // 
        this.AcceptButton = this._btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this._btnCancel;
        this.ClientSize = new System.Drawing.Size(420, 200);
        this.Controls.Add(this._tblLayout);
        this.Controls.Add(this._pnlButtons);
        this.Controls.Add(this._pnlHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ChangeUserRoleDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "تعديل صلاحية المستخدم";
        this._pnlHeader.ResumeLayout(false);
        this._tblLayout.ResumeLayout(false);
        this._tblLayout.PerformLayout();
        this._pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblHeader;
    private System.Windows.Forms.TableLayoutPanel _tblLayout;
    private System.Windows.Forms.Label _lblUserInfoPrompt;
    private System.Windows.Forms.Label _lblUserInfo;
    private System.Windows.Forms.Label _lblRolePrompt;
    private System.Windows.Forms.ComboBox _cmbRole;
    private System.Windows.Forms.FlowLayoutPanel _pnlButtons;
    private System.Windows.Forms.Button _btnSave;
    private System.Windows.Forms.Button _btnCancel;
}
