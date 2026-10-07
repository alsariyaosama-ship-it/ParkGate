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

    private System.Windows.Forms.Panel _pnlHeader;
    private System.Windows.Forms.Label _lblAppName;
    private System.Windows.Forms.Label _lblAppSubtitle;
    private System.Windows.Forms.Panel _pnlWorkstationChip;
    private System.Windows.Forms.Label lblWorkstation;
    private System.Windows.Forms.Panel _pnlCard;
    private System.Windows.Forms.TableLayoutPanel _tblCard;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label _lblSubtitle;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblError;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Button btnExit;

    private void InitializeComponent()
    {
        this._pnlHeader = new System.Windows.Forms.Panel();
        this._lblAppName = new System.Windows.Forms.Label();
        this._lblAppSubtitle = new System.Windows.Forms.Label();
        this._pnlWorkstationChip = new System.Windows.Forms.Panel();
        this.lblWorkstation = new System.Windows.Forms.Label();
        this._pnlCard = new System.Windows.Forms.Panel();
        this._tblCard = new System.Windows.Forms.TableLayoutPanel();
        this.lblTitle = new System.Windows.Forms.Label();
        this._lblSubtitle = new System.Windows.Forms.Label();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblError = new System.Windows.Forms.Label();
        this.btnLogin = new System.Windows.Forms.Button();
        this.btnExit = new System.Windows.Forms.Button();
        this._pnlHeader.SuspendLayout();
        this._pnlWorkstationChip.SuspendLayout();
        this._pnlCard.SuspendLayout();
        this._tblCard.SuspendLayout();
        this.SuspendLayout();
        // 
        // _pnlHeader
        // 
        this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._pnlHeader.Controls.Add(this._lblAppName);
        this._pnlHeader.Controls.Add(this._lblAppSubtitle);
        this._pnlHeader.Controls.Add(this._pnlWorkstationChip);
        this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this._pnlHeader.Location = new System.Drawing.Point(0, 0);
        this._pnlHeader.Margin = new System.Windows.Forms.Padding(0);
        this._pnlHeader.Name = "_pnlHeader";
        this._pnlHeader.Size = new System.Drawing.Size(460, 130);
        this._pnlHeader.TabIndex = 0;
        // 
        // _lblAppName
        // 
        this._lblAppName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblAppName.Font = new System.Drawing.Font("Cairo", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblAppName.ForeColor = System.Drawing.Color.White;
        this._lblAppName.Location = new System.Drawing.Point(16, 12);
        this._lblAppName.Name = "_lblAppName";
        this._lblAppName.Size = new System.Drawing.Size(428, 40);
        this._lblAppName.TabIndex = 0;
        this._lblAppName.Text = "ParkGate";
        this._lblAppName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _lblAppSubtitle
        // 
        this._lblAppSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this._lblAppSubtitle.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblAppSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
        this._lblAppSubtitle.Location = new System.Drawing.Point(16, 52);
        this._lblAppSubtitle.Name = "_lblAppSubtitle";
        this._lblAppSubtitle.Size = new System.Drawing.Size(428, 26);
        this._lblAppSubtitle.TabIndex = 1;
        this._lblAppSubtitle.Text = "منظومة التحكم في بوابات الدخول";
        this._lblAppSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _pnlWorkstationChip
        // 
        this._pnlWorkstationChip.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this._pnlWorkstationChip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(105)))), ((int)(((byte)(48)))));
        this._pnlWorkstationChip.Controls.Add(this.lblWorkstation);
        this._pnlWorkstationChip.Location = new System.Drawing.Point(80, 88);
        this._pnlWorkstationChip.Name = "_pnlWorkstationChip";
        this._pnlWorkstationChip.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
        this._pnlWorkstationChip.Size = new System.Drawing.Size(300, 28);
        this._pnlWorkstationChip.TabIndex = 2;
        // 
        // lblWorkstation
        // 
        this.lblWorkstation.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblWorkstation.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblWorkstation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
        this.lblWorkstation.Location = new System.Drawing.Point(8, 2);
        this.lblWorkstation.Name = "lblWorkstation";
        this.lblWorkstation.Size = new System.Drawing.Size(284, 24);
        this.lblWorkstation.TabIndex = 0;
        this.lblWorkstation.Text = "محطة العمل";
        this.lblWorkstation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _pnlCard
        // 
        this._pnlCard.BackColor = System.Drawing.Color.White;
        this._pnlCard.Controls.Add(this._tblCard);
        this._pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
        this._pnlCard.Location = new System.Drawing.Point(0, 130);
        this._pnlCard.Name = "_pnlCard";
        this._pnlCard.Padding = new System.Windows.Forms.Padding(32, 20, 32, 24);
        this._pnlCard.Size = new System.Drawing.Size(460, 390);
        this._pnlCard.TabIndex = 1;
        // 
        // _tblCard
        // 
        this._tblCard.ColumnCount = 1;
        this._tblCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._tblCard.Controls.Add(this.lblTitle, 0, 0);
        this._tblCard.Controls.Add(this._lblSubtitle, 0, 1);
        this._tblCard.Controls.Add(this.lblUsername, 0, 3);
        this._tblCard.Controls.Add(this.txtUsername, 0, 4);
        this._tblCard.Controls.Add(this.lblPassword, 0, 6);
        this._tblCard.Controls.Add(this.txtPassword, 0, 7);
        this._tblCard.Controls.Add(this.lblError, 0, 8);
        this._tblCard.Controls.Add(this.btnLogin, 0, 9);
        this._tblCard.Controls.Add(this.btnExit, 0, 11);
        this._tblCard.Dock = System.Windows.Forms.DockStyle.Fill;
        this._tblCard.Location = new System.Drawing.Point(32, 20);
        this._tblCard.Name = "_tblCard";
        this._tblCard.RowCount = 12;
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        this._tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        this._tblCard.Size = new System.Drawing.Size(396, 346);
        this._tblCard.TabIndex = 0;
        // 
        // lblTitle
        // 
        this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblTitle.Font = new System.Drawing.Font("Cairo", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this.lblTitle.Location = new System.Drawing.Point(0, 0);
        this.lblTitle.Margin = new System.Windows.Forms.Padding(0);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(396, 32);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "تسجيل الدخول";
        this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _lblSubtitle
        // 
        this._lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblSubtitle.Font = new System.Drawing.Font("Cairo", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this._lblSubtitle.Location = new System.Drawing.Point(0, 32);
        this._lblSubtitle.Margin = new System.Windows.Forms.Padding(0);
        this._lblSubtitle.Name = "_lblSubtitle";
        this._lblSubtitle.Size = new System.Drawing.Size(396, 24);
        this._lblSubtitle.TabIndex = 1;
        this._lblSubtitle.Text = "أدخل بيانات الحساب للمتابعة";
        this._lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblUsername
        // 
        this.lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblUsername.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this.lblUsername.Location = new System.Drawing.Point(0, 66);
        this.lblUsername.Margin = new System.Windows.Forms.Padding(0);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(396, 26);
        this.lblUsername.TabIndex = 2;
        this.lblUsername.Text = "اسم المستخدم:";
        this.lblUsername.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        // 
        // txtUsername
        // 
        this.txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtUsername.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this.txtUsername.Location = new System.Drawing.Point(0, 92);
        this.txtUsername.Margin = new System.Windows.Forms.Padding(0);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.txtUsername.Size = new System.Drawing.Size(396, 37);
        this.txtUsername.TabIndex = 3;
        // 
        // lblPassword
        // 
        this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblPassword.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
        this.lblPassword.Location = new System.Drawing.Point(0, 140);
        this.lblPassword.Margin = new System.Windows.Forms.Padding(0);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(396, 26);
        this.lblPassword.TabIndex = 4;
        this.lblPassword.Text = "كلمة المرور:";
        this.lblPassword.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        // 
        // txtPassword
        // 
        this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtPassword.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
        this.txtPassword.Location = new System.Drawing.Point(0, 166);
        this.txtPassword.Margin = new System.Windows.Forms.Padding(0);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.txtPassword.Size = new System.Drawing.Size(396, 37);
        this.txtPassword.TabIndex = 5;
        // 
        // lblError
        // 
        this.lblError.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblError.Font = new System.Drawing.Font("Cairo", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
        this.lblError.Location = new System.Drawing.Point(0, 206);
        this.lblError.Margin = new System.Windows.Forms.Padding(0);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(396, 34);
        this.lblError.TabIndex = 6;
        this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // btnLogin
        // 
        this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnLogin.FlatAppearance.BorderSize = 0;
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.Font = new System.Drawing.Font("Cairo", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnLogin.ForeColor = System.Drawing.Color.White;
        this.btnLogin.Location = new System.Drawing.Point(0, 240);
        this.btnLogin.Margin = new System.Windows.Forms.Padding(0);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(396, 48);
        this.btnLogin.TabIndex = 7;
        this.btnLogin.Text = "دخول إلى النظام (Enter)";
        this.btnLogin.UseVisualStyleBackColor = false;
        // 
        // btnExit
        // 
        this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
        this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnExit.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
        this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnExit.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
        this.btnExit.Location = new System.Drawing.Point(0, 296);
        this.btnExit.Margin = new System.Windows.Forms.Padding(0);
        this.btnExit.Name = "btnExit";
        this.btnExit.Size = new System.Drawing.Size(396, 40);
        this.btnExit.TabIndex = 8;
        this.btnExit.Text = "إلغاء الأمر (Esc)";
        this.btnExit.UseVisualStyleBackColor = false;
        // 
        // LoginForm
        // 
        this.AcceptButton = this.btnLogin;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnExit;
        this.ClientSize = new System.Drawing.Size(460, 520);
        this.Controls.Add(this._pnlCard);
        this.Controls.Add(this._pnlHeader);
        this.Font = new System.Drawing.Font("Cairo", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "تسجيل الدخول - ParkGate";
        this._pnlHeader.ResumeLayout(false);
        this._pnlWorkstationChip.ResumeLayout(false);
        this._pnlCard.ResumeLayout(false);
        this._tblCard.ResumeLayout(false);
        this._tblCard.PerformLayout();
        this.ResumeLayout(false);
    }
}
