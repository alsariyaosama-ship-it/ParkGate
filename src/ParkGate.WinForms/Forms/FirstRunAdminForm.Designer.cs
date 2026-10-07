#nullable disable

namespace ParkGate.WinForms.Forms;

partial class FirstRunAdminForm
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
    private System.Windows.Forms.Label lblFullName;
    private System.Windows.Forms.TextBox txtFullName;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblConfirmPassword;
    private System.Windows.Forms.TextBox txtConfirmPassword;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
    private System.Windows.Forms.Button btnCreate;
    private System.Windows.Forms.Button btnCancel;

    private void InitializeComponent()
    {
        this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblFullName = new System.Windows.Forms.Label();
        this.txtFullName = new System.Windows.Forms.TextBox();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblConfirmPassword = new System.Windows.Forms.Label();
        this.txtConfirmPassword = new System.Windows.Forms.TextBox();
        this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCreate = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tableLayoutPanelMain.SuspendLayout();
        this.flowLayoutPanelButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelMain
        // 
        this.tableLayoutPanelMain.ColumnCount = 2;
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Controls.Add(this.lblHeader, 0, 0);
        this.tableLayoutPanelMain.Controls.Add(this.lblFullName, 0, 1);
        this.tableLayoutPanelMain.Controls.Add(this.txtFullName, 1, 1);
        this.tableLayoutPanelMain.Controls.Add(this.lblUsername, 0, 2);
        this.tableLayoutPanelMain.Controls.Add(this.txtUsername, 1, 2);
        this.tableLayoutPanelMain.Controls.Add(this.lblPassword, 0, 3);
        this.tableLayoutPanelMain.Controls.Add(this.txtPassword, 1, 3);
        this.tableLayoutPanelMain.Controls.Add(this.lblConfirmPassword, 0, 4);
        this.tableLayoutPanelMain.Controls.Add(this.txtConfirmPassword, 1, 4);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelButtons, 0, 5);
        this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelMain.Location = new System.Drawing.Point(20, 20);
        this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        this.tableLayoutPanelMain.RowCount = 6;
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.Size = new System.Drawing.Size(480, 275);
        this.tableLayoutPanelMain.TabIndex = 0;
        // 
        // lblHeader
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblHeader, 2);
        this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHeader.Font = new System.Drawing.Font("Cairo", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(45, 55, 72);
        this.lblHeader.Location = new System.Drawing.Point(3, 0);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(474, 60);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblFullName
        // 
        this.lblFullName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblFullName.Location = new System.Drawing.Point(323, 60);
        this.lblFullName.Name = "lblFullName";
        this.lblFullName.Size = new System.Drawing.Size(154, 40);
        this.lblFullName.TabIndex = 1;
        this.lblFullName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtFullName
        // 
        this.txtFullName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtFullName.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtFullName.Location = new System.Drawing.Point(3, 63);
        this.txtFullName.Name = "txtFullName";
        this.txtFullName.Size = new System.Drawing.Size(314, 25);
        this.txtFullName.TabIndex = 2;
        // 
        // lblUsername
        // 
        this.lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblUsername.Location = new System.Drawing.Point(323, 100);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(154, 40);
        this.lblUsername.TabIndex = 3;
        this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtUsername
        // 
        this.txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtUsername.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtUsername.Location = new System.Drawing.Point(3, 103);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtUsername.Size = new System.Drawing.Size(314, 25);
        this.txtUsername.TabIndex = 4;
        // 
        // lblPassword
        // 
        this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPassword.Location = new System.Drawing.Point(323, 140);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(154, 40);
        this.lblPassword.TabIndex = 5;
        this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtPassword
        // 
        this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtPassword.Location = new System.Drawing.Point(3, 143);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtPassword.Size = new System.Drawing.Size(314, 25);
        this.txtPassword.TabIndex = 6;
        // 
        // lblConfirmPassword
        // 
        this.lblConfirmPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblConfirmPassword.Location = new System.Drawing.Point(323, 180);
        this.lblConfirmPassword.Name = "lblConfirmPassword";
        this.lblConfirmPassword.Size = new System.Drawing.Size(154, 40);
        this.lblConfirmPassword.TabIndex = 7;
        this.lblConfirmPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtConfirmPassword
        // 
        this.txtConfirmPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtConfirmPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtConfirmPassword.Location = new System.Drawing.Point(3, 183);
        this.txtConfirmPassword.Name = "txtConfirmPassword";
        this.txtConfirmPassword.PasswordChar = '●';
        this.txtConfirmPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtConfirmPassword.Size = new System.Drawing.Size(314, 25);
        this.txtConfirmPassword.TabIndex = 8;
        // 
        // flowLayoutPanelButtons
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.flowLayoutPanelButtons, 2);
        this.flowLayoutPanelButtons.Controls.Add(this.btnCreate);
        this.flowLayoutPanelButtons.Controls.Add(this.btnCancel);
        this.flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowLayoutPanelButtons.Location = new System.Drawing.Point(3, 223);
        this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
        this.flowLayoutPanelButtons.Size = new System.Drawing.Size(474, 49);
        this.flowLayoutPanelButtons.TabIndex = 9;
        // 
        // btnCreate
        // 
        this.btnCreate.BackColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCreate.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnCreate.ForeColor = System.Drawing.Color.White;
        this.btnCreate.Location = new System.Drawing.Point(294, 3);
        this.btnCreate.Name = "btnCreate";
        this.btnCreate.Size = new System.Drawing.Size(177, 36);
        this.btnCreate.TabIndex = 10;
        this.btnCreate.UseVisualStyleBackColor = false;
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
        this.btnCancel.TabIndex = 11;
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // FirstRunAdminForm
        // 
        this.AcceptButton = this.btnCreate;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(520, 315);
        this.Controls.Add(this.tableLayoutPanelMain);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FirstRunAdminForm";
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
