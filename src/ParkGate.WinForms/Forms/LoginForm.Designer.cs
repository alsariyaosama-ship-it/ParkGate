#nullable disable

namespace ParkGate.WinForms.Forms;

partial class LoginForm
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
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblWorkstation;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblError;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Button btnExit;

    private void InitializeComponent()
    {
        this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblWorkstation = new System.Windows.Forms.Label();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblError = new System.Windows.Forms.Label();
        this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnLogin = new System.Windows.Forms.Button();
        this.btnExit = new System.Windows.Forms.Button();
        this.tableLayoutPanelMain.SuspendLayout();
        this.flowLayoutPanelButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelMain
        // 
        this.tableLayoutPanelMain.ColumnCount = 2;
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Controls.Add(this.lblTitle, 0, 0);
        this.tableLayoutPanelMain.Controls.Add(this.lblWorkstation, 0, 1);
        this.tableLayoutPanelMain.Controls.Add(this.lblUsername, 0, 2);
        this.tableLayoutPanelMain.Controls.Add(this.txtUsername, 1, 2);
        this.tableLayoutPanelMain.Controls.Add(this.lblPassword, 0, 3);
        this.tableLayoutPanelMain.Controls.Add(this.txtPassword, 1, 3);
        this.tableLayoutPanelMain.Controls.Add(this.lblError, 0, 4);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelButtons, 0, 5);
        this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelMain.Location = new System.Drawing.Point(24, 24);
        this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        this.tableLayoutPanelMain.RowCount = 6;
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.Size = new System.Drawing.Size(432, 245);
        this.tableLayoutPanelMain.TabIndex = 0;
        // 
        // lblTitle
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblTitle, 2);
        this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this.lblTitle.Location = new System.Drawing.Point(3, 0);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(426, 45);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblWorkstation
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblWorkstation, 2);
        this.lblWorkstation.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblWorkstation.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblWorkstation.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this.lblWorkstation.Location = new System.Drawing.Point(3, 45);
        this.lblWorkstation.Name = "lblWorkstation";
        this.lblWorkstation.Size = new System.Drawing.Size(426, 35);
        this.lblWorkstation.TabIndex = 1;
        this.lblWorkstation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblUsername
        // 
        this.lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblUsername.Location = new System.Drawing.Point(305, 80);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(124, 40);
        this.lblUsername.TabIndex = 2;
        this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtUsername
        // 
        this.txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtUsername.Location = new System.Drawing.Point(3, 83);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtUsername.Size = new System.Drawing.Size(296, 25);
        this.txtUsername.TabIndex = 3;
        // 
        // lblPassword
        // 
        this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPassword.Location = new System.Drawing.Point(305, 120);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(124, 40);
        this.lblPassword.TabIndex = 4;
        this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtPassword
        // 
        this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtPassword.Location = new System.Drawing.Point(3, 123);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtPassword.Size = new System.Drawing.Size(296, 25);
        this.txtPassword.TabIndex = 5;
        // 
        // lblError
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblError, 2);
        this.lblError.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblError.ForeColor = System.Drawing.Color.FromArgb(198, 40, 40);
        this.lblError.Location = new System.Drawing.Point(3, 160);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(426, 35);
        this.lblError.TabIndex = 6;
        this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // flowLayoutPanelButtons
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.flowLayoutPanelButtons, 2);
        this.flowLayoutPanelButtons.Controls.Add(this.btnLogin);
        this.flowLayoutPanelButtons.Controls.Add(this.btnExit);
        this.flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowLayoutPanelButtons.Location = new System.Drawing.Point(3, 198);
        this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
        this.flowLayoutPanelButtons.Size = new System.Drawing.Size(426, 44);
        this.flowLayoutPanelButtons.TabIndex = 7;
        // 
        // btnLogin
        // 
        this.btnLogin.BackColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnLogin.ForeColor = System.Drawing.Color.White;
        this.btnLogin.Location = new System.Drawing.Point(283, 3);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(140, 36);
        this.btnLogin.TabIndex = 8;
        this.btnLogin.UseVisualStyleBackColor = false;
        // 
        // btnExit
        // 
        this.btnExit.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnExit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.btnExit.Location = new System.Drawing.Point(177, 3);
        this.btnExit.Name = "btnExit";
        this.btnExit.Size = new System.Drawing.Size(100, 36);
        this.btnExit.TabIndex = 9;
        this.btnExit.UseVisualStyleBackColor = true;
        // 
        // LoginForm
        // 
        this.AcceptButton = this.btnLogin;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnExit;
        this.ClientSize = new System.Drawing.Size(480, 293);
        this.Controls.Add(this.tableLayoutPanelMain);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.Padding = new System.Windows.Forms.Padding(24);
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.tableLayoutPanelMain.ResumeLayout(false);
        this.tableLayoutPanelMain.PerformLayout();
        this.flowLayoutPanelButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
