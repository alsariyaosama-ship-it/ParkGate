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
        components = new System.ComponentModel.Container();
        _mainMenuStrip = new MenuStrip();
        _menuFile = new ToolStripMenuItem();
        _menuChangePassword = new ToolStripMenuItem();
        _menuLogout = new ToolStripMenuItem();
        _menuFileSep = new ToolStripSeparator();
        _menuExit = new ToolStripMenuItem();
        _menuOperations = new ToolStripMenuItem();
        _menuCheckIn = new ToolStripMenuItem();
        _menuManagement = new ToolStripMenuItem();
        _menuBeneficiaries = new ToolStripMenuItem();
        _menuManagementSep1 = new ToolStripSeparator();
        _menuOrganizations = new ToolStripMenuItem();
        _menuAgeCategories = new ToolStripMenuItem();
        _menuUsers = new ToolStripMenuItem();
        _menuWorkstations = new ToolStripMenuItem();
        _menuManagementSep2 = new ToolStripSeparator();
        _menuSettings = new ToolStripMenuItem();
        _menuBackupNow = new ToolStripMenuItem();
        _menuReports = new ToolStripMenuItem();
        _menuDailyReport = new ToolStripMenuItem();
        _menuOrgStatistics = new ToolStripMenuItem();
        _menuHelp = new ToolStripMenuItem();
        _menuAbout = new ToolStripMenuItem();
        _statusStrip = new StatusStrip();
        _statusUser = new ToolStripStatusLabel();
        _statusWorkstation = new ToolStripStatusLabel();
        _statusServerTime = new ToolStripStatusLabel();
        _statusConnection = new ToolStripStatusLabel();
        _mainContainer = new Panel();
        _welcomeCard = new Panel();
        _lblQuickInfo = new Label();
        _lblWelcomeSubtitle = new Label();
        _lblWelcomeParkName = new Label();
        _clockTimer = new System.Windows.Forms.Timer(components);
        _syncTimer = new System.Windows.Forms.Timer(components);
        _mainMenuStrip.SuspendLayout();
        _statusStrip.SuspendLayout();
        _mainContainer.SuspendLayout();
        _welcomeCard.SuspendLayout();
        SuspendLayout();
        // 
        // _mainMenuStrip
        // 
        _mainMenuStrip.Font = new Font("Cairo", 10F);
        _mainMenuStrip.ImageScalingSize = new Size(20, 20);
        _mainMenuStrip.Items.AddRange(new ToolStripItem[] { _menuFile, _menuOperations, _menuManagement, _menuReports, _menuHelp });
        _mainMenuStrip.Location = new Point(0, 0);
        _mainMenuStrip.Name = "_mainMenuStrip";
        _mainMenuStrip.Padding = new Padding(8, 6, 0, 6);
        _mainMenuStrip.RightToLeft = RightToLeft.Yes;
        _mainMenuStrip.Size = new Size(1350, 48);
        _mainMenuStrip.TabIndex = 0;
        _mainMenuStrip.Text = "menuStrip1";
        // 
        // _menuFile
        // 
        _menuFile.DropDownItems.AddRange(new ToolStripItem[] { _menuChangePassword, _menuLogout, _menuFileSep, _menuExit });
        _menuFile.Name = "_menuFile";
        _menuFile.Size = new Size(62, 36);
        _menuFile.Text = "ملف";
        // 
        // _menuChangePassword
        // 
        _menuChangePassword.Name = "_menuChangePassword";
        _menuChangePassword.Size = new Size(229, 36);
        _menuChangePassword.Text = "تغيير كلمة المرور...";
        // 
        // _menuLogout
        // 
        _menuLogout.Name = "_menuLogout";
        _menuLogout.Size = new Size(229, 36);
        _menuLogout.Text = "تسجيل الخروج";
        // 
        // _menuFileSep
        // 
        _menuFileSep.Name = "_menuFileSep";
        _menuFileSep.Size = new Size(226, 6);
        // 
        // _menuExit
        // 
        _menuExit.Name = "_menuExit";
        _menuExit.ShortcutKeys = Keys.Alt | Keys.F4;
        _menuExit.Size = new Size(229, 36);
        _menuExit.Text = "خروج";
        // 
        // _menuOperations
        // 
        _menuOperations.DropDownItems.AddRange(new ToolStripItem[] { _menuCheckIn });
        _menuOperations.Name = "_menuOperations";
        _menuOperations.Size = new Size(88, 36);
        _menuOperations.Text = "العمليات";
        // 
        // _menuCheckIn
        // 
        _menuCheckIn.Name = "_menuCheckIn";
        _menuCheckIn.ShortcutKeys = Keys.F1;
        _menuCheckIn.Size = new Size(225, 36);
        _menuCheckIn.Text = "شباك الدخول";
        // 
        // _menuManagement
        // 
        _menuManagement.DropDownItems.AddRange(new ToolStripItem[] { _menuBeneficiaries, _menuManagementSep1, _menuOrganizations, _menuAgeCategories, _menuUsers, _menuWorkstations, _menuManagementSep2, _menuSettings, _menuBackupNow });
        _menuManagement.Name = "_menuManagement";
        _menuManagement.Size = new Size(71, 36);
        _menuManagement.Text = "الإدارة";
        // 
        // _menuBeneficiaries
        // 
        _menuBeneficiaries.Name = "_menuBeneficiaries";
        _menuBeneficiaries.Size = new Size(312, 36);
        _menuBeneficiaries.Text = "الزبائن";
        // 
        // _menuManagementSep1
        // 
        _menuManagementSep1.Name = "_menuManagementSep1";
        _menuManagementSep1.Size = new Size(309, 6);
        // 
        // _menuOrganizations
        // 
        _menuOrganizations.Name = "_menuOrganizations";
        _menuOrganizations.Size = new Size(312, 36);
        _menuOrganizations.Text = "الجهات والمنظمات";
        // 
        // _menuAgeCategories
        // 
        _menuAgeCategories.Name = "_menuAgeCategories";
        _menuAgeCategories.Size = new Size(312, 36);
        _menuAgeCategories.Text = "الفئات العمرية";
        // 
        // _menuUsers
        // 
        _menuUsers.Name = "_menuUsers";
        _menuUsers.Size = new Size(312, 36);
        _menuUsers.Text = "المستخدمون";
        // 
        // _menuWorkstations
        // 
        _menuWorkstations.Name = "_menuWorkstations";
        _menuWorkstations.Size = new Size(312, 36);
        _menuWorkstations.Text = "محطات العمل";
        // 
        // _menuManagementSep2
        // 
        _menuManagementSep2.Name = "_menuManagementSep2";
        _menuManagementSep2.Size = new Size(309, 6);
        // 
        // _menuSettings
        // 
        _menuSettings.Name = "_menuSettings";
        _menuSettings.Size = new Size(312, 36);
        _menuSettings.Text = "إعدادات النظام";
        // 
        // _menuBackupNow
        // 
        _menuBackupNow.Name = "_menuBackupNow";
        _menuBackupNow.Size = new Size(312, 36);
        _menuBackupNow.Text = "نسخ احتياطي لقاعدة البيانات...";
        // 
        // _menuReports
        // 
        _menuReports.DropDownItems.AddRange(new ToolStripItem[] { _menuDailyReport, _menuOrgStatistics });
        _menuReports.Name = "_menuReports";
        _menuReports.Size = new Size(74, 36);
        _menuReports.Text = "التقارير";
        // 
        // _menuDailyReport
        // 
        _menuDailyReport.Name = "_menuDailyReport";
        _menuDailyReport.Size = new Size(216, 36);
        _menuDailyReport.Text = "التقرير اليومي";
        // 
        // _menuOrgStatistics
        // 
        _menuOrgStatistics.Name = "_menuOrgStatistics";
        _menuOrgStatistics.Size = new Size(216, 36);
        _menuOrgStatistics.Text = "إحصائيات الجهات";
        // 
        // _menuHelp
        // 
        _menuHelp.DropDownItems.AddRange(new ToolStripItem[] { _menuAbout });
        _menuHelp.Name = "_menuHelp";
        _menuHelp.Size = new Size(88, 36);
        _menuHelp.Text = "مساعدة";
        // 
        // _menuAbout
        // 
        _menuAbout.Name = "_menuAbout";
        _menuAbout.Size = new Size(218, 36);
        _menuAbout.Text = "حول المنظومة...";
        // 
        // _statusStrip
        // 
        _statusStrip.Font = new Font("Cairo", 9.5F);
        _statusStrip.ImageScalingSize = new Size(20, 20);
        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusUser, _statusWorkstation, _statusServerTime, _statusConnection });
        _statusStrip.Location = new Point(0, 852);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Padding = new Padding(18, 2, 1, 2);
        _statusStrip.RightToLeft = RightToLeft.Yes;
        _statusStrip.Size = new Size(1350, 48);
        _statusStrip.TabIndex = 1;
        _statusStrip.Text = "statusStrip1";
        // 
        // _statusUser
        // 
        _statusUser.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _statusUser.BorderStyle = Border3DStyle.Etched;
        _statusUser.Name = "_statusUser";
        _statusUser.Padding = new Padding(10, 2, 10, 2);
        _statusUser.Size = new Size(119, 38);
        _statusUser.Text = "المستخدم: -";
        // 
        // _statusWorkstation
        // 
        _statusWorkstation.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _statusWorkstation.BorderStyle = Border3DStyle.Etched;
        _statusWorkstation.Name = "_statusWorkstation";
        _statusWorkstation.Padding = new Padding(10, 2, 10, 2);
        _statusWorkstation.Size = new Size(102, 38);
        _statusWorkstation.Text = "المحطة: -";
        // 
        // _statusServerTime
        // 
        _statusServerTime.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _statusServerTime.BorderStyle = Border3DStyle.Etched;
        _statusServerTime.Name = "_statusServerTime";
        _statusServerTime.Padding = new Padding(10, 2, 10, 2);
        _statusServerTime.Size = new Size(129, 38);
        _statusServerTime.Text = "وقت الخادم: -";
        // 
        // _statusConnection
        // 
        _statusConnection.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _statusConnection.BorderStyle = Border3DStyle.Etched;
        _statusConnection.ForeColor = Color.FromArgb(27, 135, 63);
        _statusConnection.Name = "_statusConnection";
        _statusConnection.Padding = new Padding(10, 2, 10, 2);
        _statusConnection.Size = new Size(127, 38);
        _statusConnection.Text = "متصل بالخادم";
        // 
        // _mainContainer
        // 
        _mainContainer.BackColor = Color.FromArgb(245, 247, 250);
        _mainContainer.Controls.Add(_welcomeCard);
        _mainContainer.Dock = DockStyle.Fill;
        _mainContainer.Location = new Point(0, 48);
        _mainContainer.Margin = new Padding(4);
        _mainContainer.Name = "_mainContainer";
        _mainContainer.Padding = new Padding(12);
        _mainContainer.RightToLeft = RightToLeft.Yes;
        _mainContainer.Size = new Size(1350, 804);
        _mainContainer.TabIndex = 2;
        // 
        // _welcomeCard
        // 
        _welcomeCard.Anchor = AnchorStyles.None;
        _welcomeCard.BackColor = Color.White;
        _welcomeCard.BorderStyle = BorderStyle.FixedSingle;
        _welcomeCard.Controls.Add(_lblQuickInfo);
        _welcomeCard.Controls.Add(_lblWelcomeSubtitle);
        _welcomeCard.Controls.Add(_lblWelcomeParkName);
        _welcomeCard.Location = new Point(269, 176);
        _welcomeCard.Margin = new Padding(4);
        _welcomeCard.Name = "_welcomeCard";
        _welcomeCard.Padding = new Padding(40);
        _welcomeCard.Size = new Size(812, 450);
        _welcomeCard.TabIndex = 0;
        // 
        // _lblQuickInfo
        // 
        _lblQuickInfo.Dock = DockStyle.Fill;
        _lblQuickInfo.Font = new Font("Cairo", 10.5F);
        _lblQuickInfo.ForeColor = Color.FromArgb(69, 90, 100);
        _lblQuickInfo.Location = new Point(40, 190);
        _lblQuickInfo.Margin = new Padding(4, 0, 4, 0);
        _lblQuickInfo.Name = "_lblQuickInfo";
        _lblQuickInfo.Size = new Size(730, 218);
        _lblQuickInfo.TabIndex = 2;
        _lblQuickInfo.Text = "اضغط F1 لفتح شباك الدخول وتسجيل المستفيدين.\r\nاستخدم القوائم أعلاه لإدارة النظام أو عرض التقارير حسب الصلاحيات الممنوحة.";
        _lblQuickInfo.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _lblWelcomeSubtitle
        // 
        _lblWelcomeSubtitle.Dock = DockStyle.Top;
        _lblWelcomeSubtitle.Font = new Font("Cairo", 12F);
        _lblWelcomeSubtitle.ForeColor = Color.FromArgb(45, 55, 72);
        _lblWelcomeSubtitle.Location = new Point(40, 115);
        _lblWelcomeSubtitle.Margin = new Padding(4, 0, 4, 0);
        _lblWelcomeSubtitle.Name = "_lblWelcomeSubtitle";
        _lblWelcomeSubtitle.Size = new Size(730, 75);
        _lblWelcomeSubtitle.TabIndex = 1;
        _lblWelcomeSubtitle.Text = "منظومة التحكم في الدخول المجاني لزبائن بطاقات الدعم والرعاية";
        _lblWelcomeSubtitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _lblWelcomeParkName
        // 
        _lblWelcomeParkName.Dock = DockStyle.Top;
        _lblWelcomeParkName.Font = new Font("Cairo", 20F, FontStyle.Bold);
        _lblWelcomeParkName.ForeColor = Color.FromArgb(27, 135, 63);
        _lblWelcomeParkName.Location = new Point(40, 40);
        _lblWelcomeParkName.Margin = new Padding(4, 0, 4, 0);
        _lblWelcomeParkName.Name = "_lblWelcomeParkName";
        _lblWelcomeParkName.Size = new Size(730, 75);
        _lblWelcomeParkName.TabIndex = 0;
        _lblWelcomeParkName.Text = "منتزه المرح العائلي";
        _lblWelcomeParkName.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _clockTimer
        // 
        _clockTimer.Interval = 1000;
        // 
        // _syncTimer
        // 
        _syncTimer.Interval = 60000;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(120F, 120F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1350, 900);
        Controls.Add(_mainContainer);
        Controls.Add(_statusStrip);
        Controls.Add(_mainMenuStrip);
        MainMenuStrip = _mainMenuStrip;
        Margin = new Padding(4);
        MinimumSize = new Size(1196, 788);
        Name = "MainForm";
        Text = "ParkGate";
        _mainMenuStrip.ResumeLayout(false);
        _mainMenuStrip.PerformLayout();
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        _mainContainer.ResumeLayout(false);
        _welcomeCard.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();

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
    private System.Windows.Forms.ToolStripMenuItem _menuBackupNow;
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
