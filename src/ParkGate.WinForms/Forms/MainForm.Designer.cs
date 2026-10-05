#nullable disable

namespace ParkGate.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }

            _clockTimer?.Dispose();
            _syncTimer?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this._mainMenuStrip = new System.Windows.Forms.MenuStrip();
        this._menuFile = new System.Windows.Forms.ToolStripMenuItem();
        this._menuChangePassword = new System.Windows.Forms.ToolStripMenuItem();
        this._menuLogout = new System.Windows.Forms.ToolStripMenuItem();
        this._menuFileSep = new System.Windows.Forms.ToolStripSeparator();
        this._menuExit = new System.Windows.Forms.ToolStripMenuItem();
        this._menuOperations = new System.Windows.Forms.ToolStripMenuItem();
        this._menuCheckIn = new System.Windows.Forms.ToolStripMenuItem();
        this._menuManagement = new System.Windows.Forms.ToolStripMenuItem();
        this._menuBeneficiaries = new System.Windows.Forms.ToolStripMenuItem();
        this._menuManagementSep1 = new System.Windows.Forms.ToolStripSeparator();
        this._menuOrganizations = new System.Windows.Forms.ToolStripMenuItem();
        this._menuAgeCategories = new System.Windows.Forms.ToolStripMenuItem();
        this._menuUsers = new System.Windows.Forms.ToolStripMenuItem();
        this._menuWorkstations = new System.Windows.Forms.ToolStripMenuItem();
        this._menuManagementSep2 = new System.Windows.Forms.ToolStripSeparator();
        this._menuSettings = new System.Windows.Forms.ToolStripMenuItem();
        this._menuReports = new System.Windows.Forms.ToolStripMenuItem();
        this._menuDailyReport = new System.Windows.Forms.ToolStripMenuItem();
        this._menuOrgStatistics = new System.Windows.Forms.ToolStripMenuItem();
        this._menuHelp = new System.Windows.Forms.ToolStripMenuItem();
        this._menuAbout = new System.Windows.Forms.ToolStripMenuItem();
        this._statusStrip = new System.Windows.Forms.StatusStrip();
        this._statusUser = new System.Windows.Forms.ToolStripStatusLabel();
        this._statusWorkstation = new System.Windows.Forms.ToolStripStatusLabel();
        this._statusServerTime = new System.Windows.Forms.ToolStripStatusLabel();
        this._statusConnection = new System.Windows.Forms.ToolStripStatusLabel();
        this._mainContainer = new System.Windows.Forms.Panel();
        this._welcomeCard = new System.Windows.Forms.Panel();
        this._lblQuickInfo = new System.Windows.Forms.Label();
        this._lblWelcomeSubtitle = new System.Windows.Forms.Label();
        this._lblWelcomeParkName = new System.Windows.Forms.Label();
        this._clockTimer = new System.Windows.Forms.Timer(this.components);
        this._syncTimer = new System.Windows.Forms.Timer(this.components);
        this._mainMenuStrip.SuspendLayout();
        this._statusStrip.SuspendLayout();
        this._mainContainer.SuspendLayout();
        this._welcomeCard.SuspendLayout();
        this.SuspendLayout();
        // 
        // _mainMenuStrip
        // 
        this._mainMenuStrip.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuFile,
            this._menuOperations,
            this._menuManagement,
            this._menuReports,
            this._menuHelp});
        this._mainMenuStrip.Location = new System.Drawing.Point(0, 0);
        this._mainMenuStrip.Name = "_mainMenuStrip";
        this._mainMenuStrip.Padding = new System.Windows.Forms.Padding(6, 4, 0, 4);
        this._mainMenuStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._mainMenuStrip.Size = new System.Drawing.Size(1008, 31);
        this._mainMenuStrip.TabIndex = 0;
        this._mainMenuStrip.Text = "menuStrip1";
        // 
        // _menuFile
        // 
        this._menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuChangePassword,
            this._menuLogout,
            this._menuFileSep,
            this._menuExit});
        this._menuFile.Name = "_menuFile";
        this._menuFile.Size = new System.Drawing.Size(46, 23);
        this._menuFile.Text = "ملف";
        // 
        // _menuChangePassword
        // 
        this._menuChangePassword.Name = "_menuChangePassword";
        this._menuChangePassword.Size = new System.Drawing.Size(200, 24);
        this._menuChangePassword.Text = "تغيير كلمة المرور...";
        // 
        // _menuLogout
        // 
        this._menuLogout.Name = "_menuLogout";
        this._menuLogout.Size = new System.Drawing.Size(200, 24);
        this._menuLogout.Text = "تسجيل الخروج";
        // 
        // _menuFileSep
        // 
        this._menuFileSep.Name = "_menuFileSep";
        this._menuFileSep.Size = new System.Drawing.Size(197, 6);
        // 
        // _menuExit
        // 
        this._menuExit.Name = "_menuExit";
        this._menuExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
        this._menuExit.Size = new System.Drawing.Size(200, 24);
        this._menuExit.Text = "خروج";
        // 
        // _menuOperations
        // 
        this._menuOperations.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuCheckIn});
        this._menuOperations.Name = "_menuOperations";
        this._menuOperations.Size = new System.Drawing.Size(69, 23);
        this._menuOperations.Text = "العمليات";
        // 
        // _menuCheckIn
        // 
        this._menuCheckIn.Name = "_menuCheckIn";
        this._menuCheckIn.ShortcutKeys = System.Windows.Forms.Keys.F1;
        this._menuCheckIn.Size = new System.Drawing.Size(193, 24);
        this._menuCheckIn.Text = "شباك الدخول";
        // 
        // _menuManagement
        // 
        this._menuManagement.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuBeneficiaries,
            this._menuManagementSep1,
            this._menuOrganizations,
            this._menuAgeCategories,
            this._menuUsers,
            this._menuWorkstations,
            this._menuManagementSep2,
            this._menuSettings});
        this._menuManagement.Name = "_menuManagement";
        this._menuManagement.Size = new System.Drawing.Size(57, 23);
        this._menuManagement.Text = "الإدارة";
        // 
        // _menuBeneficiaries
        // 
        this._menuBeneficiaries.Name = "_menuBeneficiaries";
        this._menuBeneficiaries.Size = new System.Drawing.Size(188, 24);
        this._menuBeneficiaries.Text = "حاملو البطاقات";
        // 
        // _menuManagementSep1
        // 
        this._menuManagementSep1.Name = "_menuManagementSep1";
        this._menuManagementSep1.Size = new System.Drawing.Size(185, 6);
        // 
        // _menuOrganizations
        // 
        this._menuOrganizations.Name = "_menuOrganizations";
        this._menuOrganizations.Size = new System.Drawing.Size(188, 24);
        this._menuOrganizations.Text = "الجهات والمنظمات";
        // 
        // _menuAgeCategories
        // 
        this._menuAgeCategories.Name = "_menuAgeCategories";
        this._menuAgeCategories.Size = new System.Drawing.Size(188, 24);
        this._menuAgeCategories.Text = "الفئات العمرية";
        // 
        // _menuUsers
        // 
        this._menuUsers.Name = "_menuUsers";
        this._menuUsers.Size = new System.Drawing.Size(188, 24);
        this._menuUsers.Text = "المستخدمون";
        // 
        // _menuWorkstations
        // 
        this._menuWorkstations.Name = "_menuWorkstations";
        this._menuWorkstations.Size = new System.Drawing.Size(188, 24);
        this._menuWorkstations.Text = "محطات العمل";
        // 
        // _menuManagementSep2
        // 
        this._menuManagementSep2.Name = "_menuManagementSep2";
        this._menuManagementSep2.Size = new System.Drawing.Size(185, 6);
        // 
        // _menuSettings
        // 
        this._menuSettings.Name = "_menuSettings";
        this._menuSettings.Size = new System.Drawing.Size(188, 24);
        this._menuSettings.Text = "إعدادات النظام";
        // 
        // _menuReports
        // 
        this._menuReports.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuDailyReport,
            this._menuOrgStatistics});
        this._menuReports.Name = "_menuReports";
        this._menuReports.Size = new System.Drawing.Size(65, 23);
        this._menuReports.Text = "التقارير";
        // 
        // _menuDailyReport
        // 
        this._menuDailyReport.Name = "_menuDailyReport";
        this._menuDailyReport.Size = new System.Drawing.Size(171, 24);
        this._menuDailyReport.Text = "التقرير اليومي";
        // 
        // _menuOrgStatistics
        // 
        this._menuOrgStatistics.Name = "_menuOrgStatistics";
        this._menuOrgStatistics.Size = new System.Drawing.Size(171, 24);
        this._menuOrgStatistics.Text = "إحصائيات الجهات";
        // 
        // _menuHelp
        // 
        this._menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuAbout});
        this._menuHelp.Name = "_menuHelp";
        this._menuHelp.Size = new System.Drawing.Size(69, 23);
        this._menuHelp.Text = "مساعدة";
        // 
        // _menuAbout
        // 
        this._menuAbout.Name = "_menuAbout";
        this._menuAbout.Size = new System.Drawing.Size(171, 24);
        this._menuAbout.Text = "حول المنظومة...";
        // 
        // _statusStrip
        // 
        this._statusStrip.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusUser,
            this._statusWorkstation,
            this._statusServerTime,
            this._statusConnection});
        this._statusStrip.Location = new System.Drawing.Point(0, 655);
        this._statusStrip.Name = "_statusStrip";
        this._statusStrip.Padding = new System.Windows.Forms.Padding(14, 0, 1, 0);
        this._statusStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._statusStrip.Size = new System.Drawing.Size(1008, 26);
        this._statusStrip.TabIndex = 1;
        this._statusStrip.Text = "statusStrip1";
        // 
        // _statusUser
        // 
        this._statusUser.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
        this._statusUser.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
        this._statusUser.Name = "_statusUser";
        this._statusUser.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
        this._statusUser.Size = new System.Drawing.Size(95, 21);
        this._statusUser.Text = "المستخدم: -";
        // 
        // _statusWorkstation
        // 
        this._statusWorkstation.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
        this._statusWorkstation.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
        this._statusWorkstation.Name = "_statusWorkstation";
        this._statusWorkstation.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
        this._statusWorkstation.Size = new System.Drawing.Size(81, 21);
        this._statusWorkstation.Text = "المحطة: -";
        // 
        // _statusServerTime
        // 
        this._statusServerTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
        this._statusServerTime.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
        this._statusServerTime.Name = "_statusServerTime";
        this._statusServerTime.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
        this._statusServerTime.Size = new System.Drawing.Size(107, 21);
        this._statusServerTime.Text = "وقت الخادم: -";
        // 
        // _statusConnection
        // 
        this._statusConnection.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
        this._statusConnection.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
        this._statusConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._statusConnection.Name = "_statusConnection";
        this._statusConnection.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
        this._statusConnection.Size = new System.Drawing.Size(106, 21);
        this._statusConnection.Text = "متصل بالخادم";
        // 
        // _mainContainer
        // 
        this._mainContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
        this._mainContainer.Controls.Add(this._welcomeCard);
        this._mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainContainer.Location = new System.Drawing.Point(0, 31);
        this._mainContainer.Name = "_mainContainer";
        this._mainContainer.Padding = new System.Windows.Forms.Padding(20);
        this._mainContainer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this._mainContainer.Size = new System.Drawing.Size(1008, 624);
        this._mainContainer.TabIndex = 2;
        // 
        // _welcomeCard
        // 
        this._welcomeCard.Anchor = System.Windows.Forms.AnchorStyles.None;
        this._welcomeCard.BackColor = System.Drawing.Color.White;
        this._welcomeCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._welcomeCard.Controls.Add(this._lblQuickInfo);
        this._welcomeCard.Controls.Add(this._lblWelcomeSubtitle);
        this._welcomeCard.Controls.Add(this._lblWelcomeParkName);
        this._welcomeCard.Location = new System.Drawing.Point(204, 150);
        this._welcomeCard.Name = "_welcomeCard";
        this._welcomeCard.Padding = new System.Windows.Forms.Padding(30);
        this._welcomeCard.Size = new System.Drawing.Size(600, 300);
        this._welcomeCard.TabIndex = 0;
        // 
        // _lblQuickInfo
        // 
        this._lblQuickInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblQuickInfo.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblQuickInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(90)))), ((int)(((byte)(100)))));
        this._lblQuickInfo.Location = new System.Drawing.Point(30, 130);
        this._lblQuickInfo.Name = "_lblQuickInfo";
        this._lblQuickInfo.Size = new System.Drawing.Size(538, 138);
        this._lblQuickInfo.TabIndex = 2;
        this._lblQuickInfo.Text = "اضغط F1 لفتح شباك الدخول وتسجيل المستفيدين.\r\nاستخدم القوائم أعلاه لإدارة النظام أ" +
    "و عرض التقارير حسب الصلاحيات الممنوحة.";
        this._lblQuickInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _lblWelcomeSubtitle
        // 
        this._lblWelcomeSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
        this._lblWelcomeSubtitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._lblWelcomeSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
        this._lblWelcomeSubtitle.Location = new System.Drawing.Point(30, 80);
        this._lblWelcomeSubtitle.Name = "_lblWelcomeSubtitle";
        this._lblWelcomeSubtitle.Size = new System.Drawing.Size(538, 50);
        this._lblWelcomeSubtitle.TabIndex = 1;
        this._lblWelcomeSubtitle.Text = "منظومة التحكم في الدخول المجاني لحاملي بطاقات الدعم والرعاية";
        this._lblWelcomeSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _lblWelcomeParkName
        // 
        this._lblWelcomeParkName.Dock = System.Windows.Forms.DockStyle.Top;
        this._lblWelcomeParkName.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this._lblWelcomeParkName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(135)))), ((int)(((byte)(63)))));
        this._lblWelcomeParkName.Location = new System.Drawing.Point(30, 30);
        this._lblWelcomeParkName.Name = "_lblWelcomeParkName";
        this._lblWelcomeParkName.Size = new System.Drawing.Size(538, 50);
        this._lblWelcomeParkName.TabIndex = 0;
        this._lblWelcomeParkName.Text = "منتزه المرح العائلي";
        this._lblWelcomeParkName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // _clockTimer
        // 
        this._clockTimer.Interval = 1000;
        // 
        // _syncTimer
        // 
        this._syncTimer.Interval = 60000;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(1008, 681);
        this.Controls.Add(this._mainContainer);
        this.Controls.Add(this._statusStrip);
        this.Controls.Add(this._mainMenuStrip);
        this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MainMenuStrip = this._mainMenuStrip;
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "MainForm";
        this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ParkGate";
        this._mainMenuStrip.ResumeLayout(false);
        this._mainMenuStrip.PerformLayout();
        this._statusStrip.ResumeLayout(false);
        this._statusStrip.PerformLayout();
        this._mainContainer.ResumeLayout(false);
        this._welcomeCard.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip _mainMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem _menuFile;
    private System.Windows.Forms.ToolStripMenuItem _menuChangePassword;
    private System.Windows.Forms.ToolStripMenuItem _menuLogout;
    private System.Windows.Forms.ToolStripSeparator _menuFileSep;
    private System.Windows.Forms.ToolStripMenuItem _menuExit;
    private System.Windows.Forms.ToolStripMenuItem _menuOperations;
    private System.Windows.Forms.ToolStripMenuItem _menuCheckIn;
    private System.Windows.Forms.ToolStripMenuItem _menuManagement;
    private System.Windows.Forms.ToolStripMenuItem _menuBeneficiaries;
    private System.Windows.Forms.ToolStripSeparator _menuManagementSep1;
    private System.Windows.Forms.ToolStripMenuItem _menuOrganizations;
    private System.Windows.Forms.ToolStripMenuItem _menuAgeCategories;
    private System.Windows.Forms.ToolStripMenuItem _menuUsers;
    private System.Windows.Forms.ToolStripMenuItem _menuWorkstations;
    private System.Windows.Forms.ToolStripSeparator _menuManagementSep2;
    private System.Windows.Forms.ToolStripMenuItem _menuSettings;
    private System.Windows.Forms.ToolStripMenuItem _menuReports;
    private System.Windows.Forms.ToolStripMenuItem _menuDailyReport;
    private System.Windows.Forms.ToolStripMenuItem _menuOrgStatistics;
    private System.Windows.Forms.ToolStripMenuItem _menuHelp;
    private System.Windows.Forms.ToolStripMenuItem _menuAbout;
    private System.Windows.Forms.StatusStrip _statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel _statusUser;
    private System.Windows.Forms.ToolStripStatusLabel _statusWorkstation;
    private System.Windows.Forms.ToolStripStatusLabel _statusServerTime;
    private System.Windows.Forms.ToolStripStatusLabel _statusConnection;
    private System.Windows.Forms.Panel _mainContainer;
    private System.Windows.Forms.Panel _welcomeCard;
    private System.Windows.Forms.Label _lblWelcomeParkName;
    private System.Windows.Forms.Label _lblWelcomeSubtitle;
    private System.Windows.Forms.Label _lblQuickInfo;
    private System.Windows.Forms.Timer _clockTimer;
    private System.Windows.Forms.Timer _syncTimer;
}
