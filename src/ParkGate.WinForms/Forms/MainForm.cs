using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

public partial class MainForm : RtlFormBase
{
    private static readonly Color ConnectedColor = Color.FromArgb(27, 135, 63);
    private static readonly Color DisconnectedColor = Color.FromArgb(198, 40, 40);

    private readonly ICurrentUser? _currentUser;
    private readonly ICurrentUserSession? _currentUserSession;
    private readonly ICurrentWorkstation? _currentWorkstation;
    private readonly IAuthService? _authService;
    private readonly ISettingsService? _settingsService;
    private readonly IUnitOfWorkFactory? _uowFactory;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<MainForm>? _logger;

    private TimeSpan _serverUtcOffset = TimeSpan.Zero;
    private string? _parkTimeZoneId = "Libya Standard Time";
    private bool _isLoggingOut;

    public MainForm() : this(null, null, null, null, null, null, null, null, null)
    {
    }

    public MainForm(
        ICurrentUser? currentUser,
        ICurrentUserSession? currentUserSession,
        ICurrentWorkstation? currentWorkstation,
        IAuthService? authService,
        ISettingsService? settingsService,
        IUnitOfWorkFactory? uowFactory,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<MainForm>? logger)
    {
        InitializeComponent();

        _currentUser = currentUser;
        _currentUserSession = currentUserSession;
        _currentWorkstation = currentWorkstation;
        _authService = authService;
        _settingsService = settingsService;
        _uowFactory = uowFactory;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        ApplyCurrentUser();
        ApplyPermissions();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.App_Title;

        // Menu items
        _menuFile.Text = Strings.Menu_File;
        _menuChangePassword.Text = Strings.Menu_ChangePassword;
        _menuLogout.Text = Strings.Menu_Logout;
        _menuExit.Text = Strings.Menu_Exit;

        _menuOperations.Text = Strings.Menu_Operations;
        _menuCheckIn.Text = Strings.Menu_CheckIn;

        _menuManagement.Text = Strings.Menu_Management;
        _menuBeneficiaries.Text = Strings.Menu_Beneficiaries;
        _menuOrganizations.Text = Strings.Menu_Organizations;
        _menuAgeCategories.Text = Strings.Menu_AgeCategories;
        _menuUsers.Text = Strings.Menu_Users;
        _menuWorkstations.Text = Strings.Menu_Workstations;
        _menuSettings.Text = Strings.Menu_Settings;

        _menuReports.Text = Strings.Menu_Reports;
        _menuDailyReport.Text = Strings.Menu_DailyReport;
        _menuOrgStatistics.Text = Strings.Menu_OrgStatistics;

        _menuHelp.Text = Strings.Menu_Help;
        _menuAbout.Text = Strings.Menu_About;

        // Default status labels
        _statusConnection.Text = Strings.Status_Connected;
        _statusConnection.ForeColor = ConnectedColor;
    }

    public void ApplyCurrentUser()
    {
        var fullName = _currentUser?.FullName ?? _currentUser?.UserName ?? "-";
        var roleName = GetRoleArabicName(_currentUser?.Role);
        _statusUser.Text = string.Format(Strings.Status_User, fullName, roleName);

        var wsName = _currentWorkstation?.DisplayName ?? _currentWorkstation?.MachineName ?? Environment.MachineName;
        _statusWorkstation.Text = string.Format(Strings.Status_Workstation, wsName);
    }

    public void ApplyPermissions()
    {
        if (_currentUser == null)
        {
            return;
        }

        // Operations
        _menuOperations.Visible = _currentUser.Has(Permission.CheckIn);

        // Management sub-items
        var canManageBeneficiaries = _currentUser.HasAny(Permission.EditBeneficiary, Permission.ChangeBeneficiaryStatus);
        var canManageOrganizations = _currentUser.Has(Permission.ManageOrganizations);
        var canManageAgeCategories = _currentUser.Has(Permission.ManageAgeCategories);
        var canManageUsers = _currentUser.Has(Permission.ManageUsers);
        var canManageWorkstations = _currentUser.Has(Permission.ManageWorkstations);
        var canManageSettings = _currentUser.Has(Permission.ManageSettings);

        _menuBeneficiaries.Visible = canManageBeneficiaries;
        _menuOrganizations.Visible = canManageOrganizations;
        _menuAgeCategories.Visible = canManageAgeCategories;
        _menuUsers.Visible = canManageUsers;
        _menuWorkstations.Visible = canManageWorkstations;
        _menuSettings.Visible = canManageSettings;

        _menuManagement.Visible = canManageBeneficiaries ||
                                  canManageOrganizations ||
                                  canManageAgeCategories ||
                                  canManageUsers ||
                                  canManageWorkstations ||
                                  canManageSettings;

        // Reports sub-items
        var canViewReports = _currentUser.Has(Permission.ViewReports);
        _menuDailyReport.Visible = canViewReports;
        _menuOrgStatistics.Visible = canViewReports;
        _menuReports.Visible = canViewReports;
    }

    private static string GetRoleArabicName(UserRole? role) => role switch
    {
        UserRole.Admin => Strings.Role_Admin,
        UserRole.Supervisor => Strings.Role_Supervisor,
        UserRole.Operator => Strings.Role_Operator,
        _ => Strings.Role_Unknown
    };

    private void WireEvents()
    {
        Load += async (_, _) => await OnMainFormLoadedAsync();
        FormClosing += OnMainFormClosing;

        _clockTimer.Tick += (_, _) => UpdateLiveClockDisplay();
        _syncTimer.Tick += async (_, _) => await SyncServerTimeAsync();

        // Menu click handlers
        _menuChangePassword.Click += OnChangePasswordClicked;
        _menuLogout.Click += async (_, _) => await OnLogoutClickedAsync();
        _menuExit.Click += (_, _) => Close();

        _menuCheckIn.Click += (_, _) => ShowCheckInDesk();
        _menuBeneficiaries.Click += (_, _) => ShowBeneficiaries();
        _menuOrganizations.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_Organizations);
        _menuAgeCategories.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_AgeCategories);
        _menuUsers.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_Users);
        _menuWorkstations.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_Workstations);
        _menuSettings.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_Settings);
        _menuDailyReport.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_DailyReport);
        _menuOrgStatistics.Click += (_, _) => ShowNextPhaseNotice(Strings.Menu_OrgStatistics);

        _menuAbout.Click += OnAboutClicked;
    }

    private Controls.CheckInDeskControl? _checkInDeskControl;
    private Controls.BeneficiariesControl? _beneficiariesControl;

    public void ShowCheckInDesk()
    {
        if (_currentUser != null && !_currentUser.Has(Permission.CheckIn))
        {
            return;
        }

        if (_checkInDeskControl == null)
        {
            _checkInDeskControl = _formFactory?.CreateControl<Controls.CheckInDeskControl>() ??
                                  new Controls.CheckInDeskControl(
                                      null, null, null, null,
                                      _settingsService,
                                      _currentUser,
                                      _currentWorkstation,
                                      _formFactory,
                                      _notifyService,
                                      null);

            _checkInDeskControl.Dock = DockStyle.Fill;
            _mainContainer.Controls.Add(_checkInDeskControl);
        }

        _welcomeCard.Visible = false;
        if (_beneficiariesControl != null)
        {
            _beneficiariesControl.Visible = false;
        }

        _checkInDeskControl.Visible = true;
        _checkInDeskControl.BringToFront();
        _checkInDeskControl.FocusCardNumberBox();
    }

    public void ShowCheckInDeskWithCard(string cardNumber)
    {
        ShowCheckInDesk();
        if (_checkInDeskControl != null && !string.IsNullOrWhiteSpace(cardNumber))
        {
            _checkInDeskControl.SetCardNumber(cardNumber);
            _ = _checkInDeskControl.SearchCardAsync();
        }
    }

    public void ShowBeneficiaries(string? initialSearchTerm = null)
    {
        if (_beneficiariesControl == null)
        {
            _beneficiariesControl = _formFactory?.CreateControl<Controls.BeneficiariesControl>() ??
                                    new Controls.BeneficiariesControl(
                                        null, null, _settingsService,
                                        _currentUser, _currentWorkstation,
                                        _formFactory, _notifyService, null);

            _beneficiariesControl.Dock = DockStyle.Fill;
            _beneficiariesControl.RequestCheckIn += (_, card) => ShowCheckInDeskWithCard(card);
            _mainContainer.Controls.Add(_beneficiariesControl);
        }

        _welcomeCard.Visible = false;
        if (_checkInDeskControl != null)
        {
            _checkInDeskControl.Visible = false;
        }

        _beneficiariesControl.Visible = true;
        _beneficiariesControl.BringToFront();

        if (!string.IsNullOrWhiteSpace(initialSearchTerm))
        {
            _beneficiariesControl.SetSearchQuery(initialSearchTerm);
            _ = _beneficiariesControl.PerformSearchAsync();
        }
        else
        {
            _beneficiariesControl.FocusSearchBox();
        }
    }

    private async Task OnMainFormLoadedAsync()
    {
        try
        {
            if (_settingsService != null)
            {
                var settings = await _settingsService.GetSettingsAsync();
                _parkTimeZoneId = settings.TimeZoneId;
                if (!string.IsNullOrWhiteSpace(settings.ParkName))
                {
                    _lblWelcomeParkName.Text = settings.ParkName;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load application settings for park time zone.");
        }

        await SyncServerTimeAsync();
        UpdateLiveClockDisplay();

        _clockTimer.Start();
        _syncTimer.Start();

        if (_currentUser?.Has(Permission.CheckIn) ?? true)
        {
            ShowCheckInDesk();
        }
    }

    private async Task SyncServerTimeAsync()
    {
        if (_uowFactory == null)
        {
            SetConnectionState(true);
            return;
        }

        try
        {
            await using var uow = _uowFactory.Create();
            var dbUtcNow = await uow.GetDbUtcNowAsync();
            _serverUtcOffset = dbUtcNow - DateTime.UtcNow;

            SetConnectionState(true);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to synchronize server time with database.");
            SetConnectionState(false);
        }
    }

    private void SetConnectionState(bool connected)
    {
        if (connected)
        {
            _statusConnection.Text = Strings.Status_Connected;
            _statusConnection.ForeColor = ConnectedColor;
        }
        else
        {
            _statusConnection.Text = Strings.Status_Disconnected;
            _statusConnection.ForeColor = DisconnectedColor;
        }
    }

    private void UpdateLiveClockDisplay()
    {
        var serverUtc = DateTime.UtcNow + _serverUtcOffset;
        var parkLocal = DateTimeDisplayHelper.ToParkLocalTime(serverUtc, _parkTimeZoneId);
        var formatted = DateTimeDisplayHelper.FormatDateTime(parkLocal);

        _statusServerTime.Text = string.Format(Strings.Status_ServerTime, formatted);
    }

    private void OnChangePasswordClicked(object? sender, EventArgs e)
    {
        if (_currentUser?.UserId == null)
        {
            return;
        }

        var changeForm = _formFactory?.Create<ChangePasswordForm>() ?? new ChangePasswordForm(_authService, null);
        changeForm.SetContext(_currentUser.UserId.Value, isMandatoryChange: false);
        changeForm.ShowDialog(this);
    }

    private async Task OnLogoutClickedAsync()
    {
        var confirm = _notifyService?.Confirm(Strings.Msg_ConfirmLogout, Strings.Title_Logout)
            ?? (MessageBox.Show(
                Strings.Msg_ConfirmLogout,
                Strings.Title_Logout,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes);

        if (!confirm)
        {
            return;
        }

        _logger?.LogInformation("User {UserName} initiated logout.", _currentUser?.UserName);
        _authService?.Logout();
        _currentUserSession?.Clear();

        _isLoggingOut = true;
        Hide();

        var loginForm = _formFactory?.Create<LoginForm>() ?? new LoginForm(_authService, _currentWorkstation, _formFactory);
        var result = loginForm.ShowDialog();

        if (result == DialogResult.OK)
        {
            _isLoggingOut = false;
            ApplyCurrentUser();
            ApplyPermissions();
            Show();
        }
        else
        {
            Close();
        }
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        var message = $"{Strings.About_AppName}\n" +
                      $"{Strings.About_Description}\n\n" +
                      $"{Strings.About_Park}\n" +
                      $"{Strings.About_Vendor}\n" +
                      $"{Strings.About_Version}";

        if (_notifyService != null)
        {
            _notifyService.Info(message, Strings.About_Title);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.About_Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowNextPhaseNotice(string featureName)
    {
        var message = $"{featureName}:\n{Strings.Msg_FeatureInNextPhase}";
        if (_notifyService != null)
        {
            _notifyService.Info(message, featureName);
        }
        else
        {
            MessageBox.Show(
                message,
                featureName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void OnMainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isLoggingOut)
        {
            return;
        }

        if (e.CloseReason == CloseReason.UserClosing)
        {
            var confirm = _notifyService?.Confirm(Strings.Msg_ConfirmExit, Strings.Title_Exit)
                ?? (MessageBox.Show(
                    Strings.Msg_ConfirmExit,
                    Strings.Title_Exit,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes);

            if (!confirm)
            {
                e.Cancel = true;
            }
        }
    }
}
