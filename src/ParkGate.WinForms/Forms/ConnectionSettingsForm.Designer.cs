#nullable disable

namespace ParkGate.WinForms.Forms;

partial class ConnectionSettingsForm
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
    private System.Windows.Forms.Label lblServer;
    private System.Windows.Forms.TextBox txtServer;
    private System.Windows.Forms.Label lblDatabase;
    private System.Windows.Forms.TextBox txtDatabase;
    private System.Windows.Forms.Label lblAuthType;
    private System.Windows.Forms.ComboBox cmbAuthType;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.Button btnCancel;

    private void InitializeComponent()
    {
        this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblServer = new System.Windows.Forms.Label();
        this.txtServer = new System.Windows.Forms.TextBox();
        this.lblDatabase = new System.Windows.Forms.Label();
        this.txtDatabase = new System.Windows.Forms.TextBox();
        this.lblAuthType = new System.Windows.Forms.Label();
        this.cmbAuthType = new System.Windows.Forms.ComboBox();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblStatus = new System.Windows.Forms.Label();
        this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnTest = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tableLayoutPanelMain.SuspendLayout();
        this.flowLayoutPanelButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelMain
        // 
        this.tableLayoutPanelMain.ColumnCount = 2;
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Controls.Add(this.lblHeader, 0, 0);
        this.tableLayoutPanelMain.Controls.Add(this.lblServer, 0, 1);
        this.tableLayoutPanelMain.Controls.Add(this.txtServer, 1, 1);
        this.tableLayoutPanelMain.Controls.Add(this.lblDatabase, 0, 2);
        this.tableLayoutPanelMain.Controls.Add(this.txtDatabase, 1, 2);
        this.tableLayoutPanelMain.Controls.Add(this.lblAuthType, 0, 3);
        this.tableLayoutPanelMain.Controls.Add(this.cmbAuthType, 1, 3);
        this.tableLayoutPanelMain.Controls.Add(this.lblUsername, 0, 4);
        this.tableLayoutPanelMain.Controls.Add(this.txtUsername, 1, 4);
        this.tableLayoutPanelMain.Controls.Add(this.lblPassword, 0, 5);
        this.tableLayoutPanelMain.Controls.Add(this.txtPassword, 1, 5);
        this.tableLayoutPanelMain.Controls.Add(this.lblStatus, 0, 6);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelButtons, 0, 7);
        this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelMain.Location = new System.Drawing.Point(20, 20);
        this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        this.tableLayoutPanelMain.RowCount = 8;
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tableLayoutPanelMain.Size = new System.Drawing.Size(520, 345);
        this.tableLayoutPanelMain.TabIndex = 0;
        // 
        // lblHeader
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblHeader, 2);
        this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(45, 55, 72);
        this.lblHeader.Location = new System.Drawing.Point(3, 0);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(514, 50);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "إعدادات الاتصال بخادم قاعدة البيانات (SQL Server)";
        this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblServer
        // 
        this.lblServer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblServer.Location = new System.Drawing.Point(343, 50);
        this.lblServer.Name = "lblServer";
        this.lblServer.Size = new System.Drawing.Size(174, 40);
        this.lblServer.TabIndex = 1;
        this.lblServer.Text = "اسم الخادم أو المثيل:";
        this.lblServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtServer
        // 
        this.txtServer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtServer.Location = new System.Drawing.Point(3, 53);
        this.txtServer.Name = "txtServer";
        this.txtServer.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtServer.Size = new System.Drawing.Size(334, 25);
        this.txtServer.TabIndex = 2;
        // 
        // lblDatabase
        // 
        this.lblDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblDatabase.Location = new System.Drawing.Point(343, 90);
        this.lblDatabase.Name = "lblDatabase";
        this.lblDatabase.Size = new System.Drawing.Size(174, 40);
        this.lblDatabase.TabIndex = 3;
        this.lblDatabase.Text = "اسم قاعدة البيانات:";
        this.lblDatabase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtDatabase
        // 
        this.txtDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtDatabase.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtDatabase.Location = new System.Drawing.Point(3, 93);
        this.txtDatabase.Name = "txtDatabase";
        this.txtDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtDatabase.Size = new System.Drawing.Size(334, 25);
        this.txtDatabase.TabIndex = 4;
        // 
        // lblAuthType
        // 
        this.lblAuthType.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblAuthType.Location = new System.Drawing.Point(343, 130);
        this.lblAuthType.Name = "lblAuthType";
        this.lblAuthType.Size = new System.Drawing.Size(174, 40);
        this.lblAuthType.TabIndex = 5;
        this.lblAuthType.Text = "نوع المصادقة:";
        this.lblAuthType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbAuthType
        // 
        this.cmbAuthType.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cmbAuthType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbAuthType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.cmbAuthType.FormattingEnabled = true;
        this.cmbAuthType.Location = new System.Drawing.Point(3, 133);
        this.cmbAuthType.Name = "cmbAuthType";
        this.cmbAuthType.Size = new System.Drawing.Size(334, 25);
        this.cmbAuthType.TabIndex = 6;
        // 
        // lblUsername
        // 
        this.lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblUsername.Location = new System.Drawing.Point(343, 170);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(174, 40);
        this.lblUsername.TabIndex = 7;
        this.lblUsername.Text = "اسم المستخدم (User ID):";
        this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtUsername
        // 
        this.txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtUsername.Enabled = false;
        this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtUsername.Location = new System.Drawing.Point(3, 173);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtUsername.Size = new System.Drawing.Size(334, 25);
        this.txtUsername.TabIndex = 8;
        // 
        // lblPassword
        // 
        this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPassword.Location = new System.Drawing.Point(343, 210);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(174, 40);
        this.lblPassword.TabIndex = 9;
        this.lblPassword.Text = "كلمة المرور:";
        this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // txtPassword
        // 
        this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtPassword.Enabled = false;
        this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtPassword.Location = new System.Drawing.Point(3, 213);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtPassword.Size = new System.Drawing.Size(334, 25);
        this.txtPassword.TabIndex = 10;
        // 
        // lblStatus
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.lblStatus, 2);
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblStatus.Location = new System.Drawing.Point(3, 250);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(514, 35);
        this.lblStatus.TabIndex = 11;
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // flowLayoutPanelButtons
        // 
        this.tableLayoutPanelMain.SetColumnSpan(this.flowLayoutPanelButtons, 2);
        this.flowLayoutPanelButtons.Controls.Add(this.btnSave);
        this.flowLayoutPanelButtons.Controls.Add(this.btnTest);
        this.flowLayoutPanelButtons.Controls.Add(this.btnCancel);
        this.flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowLayoutPanelButtons.Location = new System.Drawing.Point(3, 288);
        this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
        this.flowLayoutPanelButtons.Size = new System.Drawing.Size(514, 54);
        this.flowLayoutPanelButtons.TabIndex = 12;
        // 
        // btnSave
        // 
        this.btnSave.BackColor = System.Drawing.Color.FromArgb(27, 135, 63);
        this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnSave.ForeColor = System.Drawing.Color.White;
        this.btnSave.Location = new System.Drawing.Point(384, 3);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(127, 36);
        this.btnSave.TabIndex = 13;
        this.btnSave.Text = "حفظ ومتابعة";
        this.btnSave.UseVisualStyleBackColor = false;
        // 
        // btnTest
        // 
        this.btnTest.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.btnTest.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnTest.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.btnTest.Location = new System.Drawing.Point(261, 3);
        this.btnTest.Name = "btnTest";
        this.btnTest.Size = new System.Drawing.Size(117, 36);
        this.btnTest.TabIndex = 14;
        this.btnTest.Text = "اختبار الاتصال";
        this.btnTest.UseVisualStyleBackColor = true;
        // 
        // btnCancel
        // 
        this.btnCancel.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.btnCancel.Location = new System.Drawing.Point(155, 3);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(100, 36);
        this.btnCancel.TabIndex = 15;
        this.btnCancel.Text = "إلغاء";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // ConnectionSettingsForm
        // 
        this.AcceptButton = this.btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(560, 385);
        this.Controls.Add(this.tableLayoutPanelMain);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ConnectionSettingsForm";
        this.Padding = new System.Windows.Forms.Padding(20);
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "إعدادات الاتصال بقاعدة البيانات";
        this.tableLayoutPanelMain.ResumeLayout(false);
        this.tableLayoutPanelMain.PerformLayout();
        this.flowLayoutPanelButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
