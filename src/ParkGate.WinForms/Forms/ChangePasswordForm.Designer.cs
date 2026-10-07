#nullable disable

namespace ParkGate.WinForms.Forms;

partial class ChangePasswordForm
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

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblCurrentPassword;
    private System.Windows.Forms.TextBox txtCurrentPassword;
    private System.Windows.Forms.Label lblNewPassword;
    private System.Windows.Forms.TextBox txtNewPassword;
    private System.Windows.Forms.Label lblConfirmNewPassword;
    private System.Windows.Forms.TextBox txtConfirmNewPassword;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
    private System.Windows.Forms.Button btnChange;
    private System.Windows.Forms.Button btnCancel;

    private void InitializeComponent()
    {
        this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblCurrentPassword = new System.Windows.Forms.Label();
        this.txtCurrentPassword = new System.Windows.Forms.TextBox();
        this.lblNewPassword = new System.Windows.Forms.Label();
        this.txtNewPassword = new System.Windows.Forms.TextBox();
        this.lblConfirmNewPassword = new System.Windows.Forms.Label();
        this.txtConfirmNewPassword = new System.Windows.Forms.TextBox();
        this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnChange = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tableLayoutPanelMain.SuspendLayout();
        this.flowLayoutPanelButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelMain
        // 
        this.tableLayoutPanelMain.ColumnCount = 2;
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Controls.Add(this.lblHeader, 0, 0);
        this.tableLayoutPanelMain.Controls.Add(this.lblCurrentPassword, 0, 1);
        this.tableLayoutPanelMain.Controls.Add(this.txtCurrentPassword, 1, 1);
        this.tableLayoutPanelMain.Controls.Add(this.lblNewPassword, 0, 2);
        this.tableLayoutPanelMain.Controls.Add(this.txtNewPassword, 1, 2);
        this.tableLayoutPanelMain.Controls.Add(this.lblConfirmNewPassword, 0, 3);
        this.tableLayoutPanelMain.Controls.Add(this.txtConfirmNewPassword, 1, 3);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelButtons, 0, 4);
        this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelMain.Location = new System.Drawing.Point(20, 20);
        this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        this.tableLayoutPanelMain.RowCount = 5;
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.Size = new System.Drawing.Size(460, 230);
        this.tableLayoutPanelMain.TabIndex = 0;
        // 
        // lblHeader
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblHeader, 2);
        this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHeader.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(45, 55, 72);
        this.lblHeader.Location = new System.Drawing.Point(3, 0);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(454, 50);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblCurrentPassword
        // 
        this.lblCurrentPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblCurrentPassword.Location = new System.Drawing.Point(293, 50);
        this.lblCurrentPassword.Name = "lblCurrentPassword";
        this.lblCurrentPassword.Size = new System.Drawing.Size(164, 40);
        this.lblCurrentPassword.TabIndex = 1;
        this.lblCurrentPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtCurrentPassword
        // 
        this.txtCurrentPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtCurrentPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtCurrentPassword.Location = new System.Drawing.Point(3, 53);
        this.txtCurrentPassword.Name = "txtCurrentPassword";
        this.txtCurrentPassword.PasswordChar = '●';
        this.txtCurrentPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtCurrentPassword.Size = new System.Drawing.Size(284, 25);
        this.txtCurrentPassword.TabIndex = 2;
        // 
        // lblNewPassword
        // 
        this.lblNewPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblNewPassword.Location = new System.Drawing.Point(293, 90);
        this.lblNewPassword.Name = "lblNewPassword";
        this.lblNewPassword.Size = new System.Drawing.Size(164, 40);
        this.lblNewPassword.TabIndex = 3;
        this.lblNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtNewPassword
        // 
        this.txtNewPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtNewPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtNewPassword.Location = new System.Drawing.Point(3, 93);
        this.txtNewPassword.Name = "txtNewPassword";
        this.txtNewPassword.PasswordChar = '●';
        this.txtNewPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtNewPassword.Size = new System.Drawing.Size(284, 25);
        this.txtNewPassword.TabIndex = 4;
        // 
        // lblConfirmNewPassword
        // 
        this.lblConfirmNewPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblConfirmNewPassword.Location = new System.Drawing.Point(293, 130);
        this.lblConfirmNewPassword.Name = "lblConfirmNewPassword";
        this.lblConfirmNewPassword.Size = new System.Drawing.Size(164, 40);
        this.lblConfirmNewPassword.TabIndex = 5;
        this.lblConfirmNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtConfirmNewPassword
        // 
        this.txtConfirmNewPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtConfirmNewPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtConfirmNewPassword.Location = new System.Drawing.Point(3, 133);
        this.txtConfirmNewPassword.Name = "txtConfirmNewPassword";
        this.txtConfirmNewPassword.PasswordChar = '●';
        this.txtConfirmNewPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtConfirmNewPassword.Size = new System.Drawing.Size(284, 25);
        this.txtConfirmNewPassword.TabIndex = 6;
        // 
        // flowLayoutPanelButtons
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.flowLayoutPanelButtons, 2);
        this.flowLayoutPanelButtons.Controls.Add(this.btnChange);
        this.flowLayoutPanelButtons.Controls.Add(this.btnCancel);
        this.flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowLayoutPanelButtons.Location = new System.Drawing.Point(3, 173);
        this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
        this.flowLayoutPanelButtons.Size = new System.Drawing.Size(454, 54);
        this.flowLayoutPanelButtons.TabIndex = 7;
        // 
        // btnChange
        // 
        this.btnChange.BackColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this.btnChange.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnChange.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnChange.ForeColor = System.Drawing.Color.White;
        this.btnChange.Location = new System.Drawing.Point(294, 3);
        this.btnChange.Name = "btnChange";
        this.btnChange.Size = new System.Drawing.Size(157, 36);
        this.btnChange.TabIndex = 8;
        this.btnChange.UseVisualStyleBackColor = false;
        // 
        // btnCancel
        // 
        this.btnCancel.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnCancel.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.btnCancel.Location = new System.Drawing.Point(188, 3);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(100, 36);
        this.btnCancel.TabIndex = 9;
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // ChangePasswordForm
        // 
        this.AcceptButton = this.btnChange;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(500, 270);
        this.Controls.Add(this.tableLayoutPanelMain);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ChangePasswordForm";
        this.Padding = new System.Windows.Forms.Padding(20);
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.tableLayoutPanelMain.ResumeLayout(false);
        this.tableLayoutPanelMain.PerformLayout();
        this.flowLayoutPanelButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
