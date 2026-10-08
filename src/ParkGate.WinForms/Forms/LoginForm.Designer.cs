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
        _pnlHeader = new Panel();
        _lblAppName = new Label();
        _lblAppSubtitle = new Label();
        _pnlWorkstationChip = new Panel();
        lblWorkstation = new Label();
        _pnlCard = new Panel();
        _tblCard = new TableLayoutPanel();
        lblTitle = new Label();
        _lblSubtitle = new Label();
        lblUsername = new Label();
        txtUsername = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        lblError = new Label();
        btnLogin = new Button();
        btnExit = new Button();
        _pnlHeader.SuspendLayout();
        _pnlWorkstationChip.SuspendLayout();
        _pnlCard.SuspendLayout();
        _tblCard.SuspendLayout();
        SuspendLayout();
        // 
        // _pnlHeader
        // 
        _pnlHeader.BackColor = Color.FromArgb(27, 135, 63);
        _pnlHeader.Controls.Add(_lblAppName);
        _pnlHeader.Controls.Add(_lblAppSubtitle);
        _pnlHeader.Controls.Add(_pnlWorkstationChip);
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Location = new Point(0, 0);
        _pnlHeader.Margin = new Padding(0);
        _pnlHeader.Name = "_pnlHeader";
        _pnlHeader.Size = new Size(575, 162);
        _pnlHeader.TabIndex = 0;
        // 
        // _lblAppName
        // 
        _lblAppName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _lblAppName.Font = new Font("Cairo", 18F, FontStyle.Bold);
        _lblAppName.ForeColor = Color.White;
        _lblAppName.Location = new Point(20, 15);
        _lblAppName.Margin = new Padding(4, 0, 4, 0);
        _lblAppName.Name = "_lblAppName";
        _lblAppName.Size = new Size(535, 50);
        _lblAppName.TabIndex = 0;
        _lblAppName.Text = "ParkGate";
        _lblAppName.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _lblAppSubtitle
        // 
        _lblAppSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _lblAppSubtitle.Font = new Font("Cairo", 10F);
        _lblAppSubtitle.ForeColor = Color.FromArgb(220, 252, 231);
        _lblAppSubtitle.Location = new Point(20, 65);
        _lblAppSubtitle.Margin = new Padding(4, 0, 4, 0);
        _lblAppSubtitle.Name = "_lblAppSubtitle";
        _lblAppSubtitle.Size = new Size(535, 32);
        _lblAppSubtitle.TabIndex = 1;
        _lblAppSubtitle.Text = "منظومة التحكم في بوابات الدخول";
        _lblAppSubtitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _pnlWorkstationChip
        // 
        _pnlWorkstationChip.Anchor = AnchorStyles.Bottom;
        _pnlWorkstationChip.BackColor = Color.FromArgb(20, 105, 48);
        _pnlWorkstationChip.Controls.Add(lblWorkstation);
        _pnlWorkstationChip.Location = new Point(100, 110);
        _pnlWorkstationChip.Margin = new Padding(4);
        _pnlWorkstationChip.Name = "_pnlWorkstationChip";
        _pnlWorkstationChip.Padding = new Padding(10, 2, 10, 2);
        _pnlWorkstationChip.Size = new Size(375, 35);
        _pnlWorkstationChip.TabIndex = 2;
        // 
        // lblWorkstation
        // 
        lblWorkstation.Dock = DockStyle.Fill;
        lblWorkstation.Font = new Font("Cairo", 9F);
        lblWorkstation.ForeColor = Color.FromArgb(240, 253, 244);
        lblWorkstation.Location = new Point(10, 2);
        lblWorkstation.Margin = new Padding(4, 0, 4, 0);
        lblWorkstation.Name = "lblWorkstation";
        lblWorkstation.Size = new Size(355, 31);
        lblWorkstation.TabIndex = 0;
        lblWorkstation.Text = "محطة العمل";
        lblWorkstation.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _pnlCard
        // 
        _pnlCard.BackColor = Color.White;
        _pnlCard.Controls.Add(_tblCard);
        _pnlCard.Dock = DockStyle.Fill;
        _pnlCard.Location = new Point(0, 162);
        _pnlCard.Margin = new Padding(4);
        _pnlCard.Name = "_pnlCard";
        _pnlCard.Padding = new Padding(40, 25, 40, 30);
        _pnlCard.Size = new Size(575, 488);
        _pnlCard.TabIndex = 1;
        // 
        // _tblCard
        // 
        _tblCard.ColumnCount = 1;
        _tblCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tblCard.Controls.Add(lblTitle, 0, 0);
        _tblCard.Controls.Add(_lblSubtitle, 0, 1);
        _tblCard.Controls.Add(lblUsername, 0, 3);
        _tblCard.Controls.Add(txtUsername, 0, 4);
        _tblCard.Controls.Add(lblPassword, 0, 6);
        _tblCard.Controls.Add(txtPassword, 0, 7);
        _tblCard.Controls.Add(lblError, 0, 8);
        _tblCard.Controls.Add(btnLogin, 0, 9);
        _tblCard.Controls.Add(btnExit, 0, 11);
        _tblCard.Dock = DockStyle.Fill;
        _tblCard.Location = new Point(40, 25);
        _tblCard.Margin = new Padding(4);
        _tblCard.Name = "_tblCard";
        _tblCard.RowCount = 12;
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
        _tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        _tblCard.Size = new Size(495, 433);
        _tblCard.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Cairo", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
        lblTitle.Location = new Point(0, 0);
        lblTitle.Margin = new Padding(0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(495, 40);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "تسجيل الدخول";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _lblSubtitle
        // 
        _lblSubtitle.Dock = DockStyle.Fill;
        _lblSubtitle.Font = new Font("Cairo", 9F);
        _lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        _lblSubtitle.Location = new Point(0, 40);
        _lblSubtitle.Margin = new Padding(0);
        _lblSubtitle.Name = "_lblSubtitle";
        _lblSubtitle.Size = new Size(495, 30);
        _lblSubtitle.TabIndex = 1;
        _lblSubtitle.Text = "أدخل بيانات الحساب للمتابعة";
        _lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblUsername
        // 
        lblUsername.Dock = DockStyle.Fill;
        lblUsername.Font = new Font("Cairo", 10F, FontStyle.Bold);
        lblUsername.ForeColor = Color.FromArgb(51, 65, 85);
        lblUsername.Location = new Point(0, 82);
        lblUsername.Margin = new Padding(0);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(495, 32);
        lblUsername.TabIndex = 2;
        lblUsername.Text = "اسم المستخدم:";
        lblUsername.TextAlign = ContentAlignment.BottomLeft;
        // 
        // txtUsername
        // 
        txtUsername.Dock = DockStyle.Fill;
        txtUsername.Font = new Font("Cairo", 12F);
        txtUsername.ForeColor = Color.FromArgb(15, 23, 42);
        txtUsername.Location = new Point(0, 114);
        txtUsername.Margin = new Padding(0);
        txtUsername.Name = "txtUsername";
        txtUsername.RightToLeft = RightToLeft.Yes;
        txtUsername.Size = new Size(495, 45);
        txtUsername.TabIndex = 3;
        // 
        // lblPassword
        // 
        lblPassword.Dock = DockStyle.Fill;
        lblPassword.Font = new Font("Cairo", 10F, FontStyle.Bold);
        lblPassword.ForeColor = Color.FromArgb(51, 65, 85);
        lblPassword.Location = new Point(0, 174);
        lblPassword.Margin = new Padding(0);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(495, 32);
        lblPassword.TabIndex = 4;
        lblPassword.Text = "كلمة المرور:";
        lblPassword.TextAlign = ContentAlignment.BottomLeft;
        // 
        // txtPassword
        // 
        txtPassword.Dock = DockStyle.Fill;
        txtPassword.Font = new Font("Cairo", 12F);
        txtPassword.ForeColor = Color.FromArgb(15, 23, 42);
        txtPassword.Location = new Point(0, 206);
        txtPassword.Margin = new Padding(0);
        txtPassword.Name = "txtPassword";
        txtPassword.PasswordChar = '●';
        txtPassword.RightToLeft = RightToLeft.Yes;
        txtPassword.Size = new Size(495, 45);
        txtPassword.TabIndex = 5;
        // 
        // lblError
        // 
        lblError.Dock = DockStyle.Fill;
        lblError.Font = new Font("Cairo", 9.5F, FontStyle.Bold);
        lblError.ForeColor = Color.FromArgb(198, 40, 40);
        lblError.Location = new Point(0, 256);
        lblError.Margin = new Padding(0);
        lblError.Name = "lblError";
        lblError.Size = new Size(495, 42);
        lblError.TabIndex = 6;
        lblError.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnLogin
        // 
        btnLogin.BackColor = Color.FromArgb(27, 135, 63);
        btnLogin.Cursor = Cursors.Hand;
        btnLogin.Dock = DockStyle.Fill;
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Font = new Font("Cairo", 12F, FontStyle.Bold);
        btnLogin.ForeColor = Color.White;
        btnLogin.Location = new Point(0, 298);
        btnLogin.Margin = new Padding(0);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(495, 60);
        btnLogin.TabIndex = 7;
        btnLogin.Text = "دخول إلى النظام (Enter)";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // btnExit
        // 
        btnExit.BackColor = Color.FromArgb(241, 245, 249);
        btnExit.Cursor = Cursors.Hand;
        btnExit.DialogResult = DialogResult.Cancel;
        btnExit.Dock = DockStyle.Fill;
        btnExit.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.Font = new Font("Cairo", 10F, FontStyle.Bold);
        btnExit.ForeColor = Color.FromArgb(71, 85, 105);
        btnExit.Location = new Point(0, 368);
        btnExit.Margin = new Padding(0);
        btnExit.Name = "btnExit";
        btnExit.Size = new Size(495, 65);
        btnExit.TabIndex = 8;
        btnExit.Text = "إلغاء الأمر (Esc)";
        btnExit.UseVisualStyleBackColor = false;
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        AutoScaleDimensions = new SizeF(120F, 120F);
        AutoScaleMode = AutoScaleMode.Dpi;
        CancelButton = btnExit;
        ClientSize = new Size(575, 650);
        Controls.Add(_pnlCard);
        Controls.Add(_pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(4);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LoginForm";
        Text = "تسجيل الدخول - ParkGate";
        _pnlHeader.ResumeLayout(false);
        _pnlWorkstationChip.ResumeLayout(false);
        _pnlCard.ResumeLayout(false);
        _tblCard.ResumeLayout(false);
        _tblCard.PerformLayout();
        ResumeLayout(false);
    }
}
